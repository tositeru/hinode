using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Hinode.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// UnityEngine.EventSystemの1フレームにおける入力データを記録するためのもの
    /// 一つ前と変化がないデータはシリアライズの対象にならないようになっています。
    ///
    /// 以下のKeyCodeは他のKeyCodeと同じものと扱います。
    ///     KeyCode.LeftApple => KeyCode.LeftCommand
    ///     KeyCode.RightApple => KeyCode.RightCommand
    ///     
    /// <see cref="IFrameDataRecorder"/>
    /// <seealso cref="Hinode.Tests.Input.FrameInputDataRecorder.TestKeyboardFrameInputData"/>
    /// </summary>
    [System.Serializable, ContainsSerializationKeyTypeGetter(typeof(KeyboardFrameInputData))]
    public class KeyboardFrameInputData : IFrameDataRecorder
        , ISerializable
    {
        public static readonly string KEY_CHILD_INPUT_DATA_TYPE = "kbd";

        /// <summary>
        /// <seealso cref="FrameInputData.RegistChildFrameInputDataType(string, System.Type)"/>
        /// </summary>
        public static void RegistTypeToFrameInputData()
        {
            IFrameInputDateRecorderHelper.RegistTypeToFrameInputData(KEY_CHILD_INPUT_DATA_TYPE, typeof(KeyboardFrameInputData));
        }

        public static IEnumerable<KeyCode> AllKeyCodes { get; } = System.Enum.GetValues(typeof(KeyCode))
            .OfType<KeyCode>();

        Dictionary<KeyCode, UpdateObserver<InputDefines.ButtonCondition>> _keyButtonsDict = new Dictionary<KeyCode, UpdateObserver<InputDefines.ButtonCondition>>();

        HashSet<KeyCode> _enabledKeyCodeFilter = new HashSet<KeyCode>();

        JsonSerializer _jsonSerializer = new JsonSerializer();

        KeyCode TransformKeyCode(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.LeftApple: return KeyCode.LeftCommand;
                case KeyCode.RightApple: return KeyCode.RightApple;
                default: return keyCode;
            }
        }

        public void SetKeyButton(KeyCode keyCode, InputDefines.ButtonCondition condition)
        {
            if (!EnabledKeyCodes.Contains(keyCode)) return;

            if (!_keyButtonsDict.ContainsKey(keyCode))
            {
                // InputDefines.ButtonCondition.Free must be InputDefines.ButtonCondition.Free for IUpdateObserver.DidUpdated!
                var initialCondition = InputDefines.ButtonCondition.Free;
                _keyButtonsDict.Add(keyCode, new UpdateObserver<InputDefines.ButtonCondition>(initialCondition));
            }
            _keyButtonsDict[keyCode].Value = condition;
        }

        public InputDefines.ButtonCondition GetKeyButton(KeyCode keyCode)
        {
            if (!EnabledKeyCodes.Contains(keyCode)) return InputDefines.ButtonCondition.Free;

            keyCode = TransformKeyCode(keyCode);
            return _keyButtonsDict.ContainsKey(keyCode)
                ? _keyButtonsDict[keyCode].Value
                : InputDefines.ButtonCondition.Free;
        }

        public IEnumerable<(KeyCode keyCode, UpdateObserver<InputDefines.ButtonCondition> observer)> KeyButtons
        {
            get => _keyButtonsDict
                .Where(_t => EnabledKeyCodes.Contains(_t.Key))
                .Select(_t => (keyCode: _t.Key, observer: _t.Value));
        }

        public KeyboardFrameInputData()
        {
        }

        #region Key Filter
        public void AddEnabledKeyCode(IEnumerable<KeyCode> enableKeyCodes)
        {
            foreach (var k in enableKeyCodes
                .Where(_k => !_enabledKeyCodeFilter.Contains(_k)))
            {
                _enabledKeyCodeFilter.Add(k);
            }
        }
        public void AddEnabledKeyCode(params KeyCode[] enableKeyCodes)
            => AddEnabledKeyCode(enableKeyCodes.AsEnumerable());

        public void RemoveEnabledKeyCode(IEnumerable<KeyCode> keyCodes)
        {
            foreach (var k in keyCodes
                .Where(_k => _enabledKeyCodeFilter.Contains(_k)))
            {
                _enabledKeyCodeFilter.Remove(k);
            }
        }
        public void RemoveEnabledKeyCode(params KeyCode[] enableKeyCodes)
            => RemoveEnabledKeyCode(enableKeyCodes.AsEnumerable());

        public IEnumerable<KeyCode> EnabledKeyCodes
        {
            get => _enabledKeyCodeFilter.Any()
                ? _enabledKeyCodeFilter
                : AllKeyCodes;
        }
        #endregion

        public void CopyUpdatedDatasTo(KeyboardFrameInputData other)
        {
            foreach (var (keyCode, condition) in KeyButtons
                .Where(_t => _t.observer.DidUpdated))
            {
                other.SetKeyButton(keyCode, condition.Value);
            }
        }

        #region IFrameDataRecorder interface
        /// <summary>
        /// <see cref="IFrameDataRecorder.ResetDatas()"/>
        /// </summary>
        public void ResetDatas()
        {
            _keyButtonsDict.Clear();
        }

        public void RefleshUpdatedFlags()
        {
            foreach (var (_, observer) in KeyButtons
                .Where(_t => _t.observer.DidUpdated))
            {
                observer.Reset();
            }
        }

        /// <summary>
        /// <see cref="IFrameDataRecorder.Record(ReplayableInput)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public void Record(ReplayableInput input)
        {
            if (input.AnyKey)
            {
                foreach (var keyCode in EnabledKeyCodes)
                {
                    var condition = input.GetKeyDown(keyCode)
                        ? InputDefines.ButtonCondition.Down
                        : input.GetKeyUp(keyCode)
                            ? InputDefines.ButtonCondition.Up
                            : input.GetKey(keyCode)
                                ? InputDefines.ButtonCondition.Push
                                : InputDefines.ButtonCondition.Free;
                    SetKeyButton(keyCode, condition);
                }
            }
            else
            {
                foreach (var (keyCode, observer) in KeyButtons
                    .Where(_t => _t.observer.Value == InputDefines.ButtonCondition.Free))
                {
                    observer.Value = InputDefines.ButtonCondition.Free;
                }
            }
        }

        public void RecoverTo(ReplayableInput input)
        {
            foreach (var keyCode in AllKeyCodes)
            {
                input.SetRecordedKeyButton(keyCode, GetKeyButton(keyCode));
            }
        }

        public void CopyUpdatedDatasTo(IFrameDataRecorder other)
        {
            if (other is KeyboardFrameInputData)
            {
                CopyUpdatedDatasTo(other as KeyboardFrameInputData);
            }
            else
            {
                Assert.IsTrue(false, $"Not Support type({other.GetType()})...");
            }
        }

        public IEnumerable<FrameInputDataKeyValue> GetValuesEnumerable()
        {
            return new ValuesEnumerable(this);
        }

        class ValuesEnumerable : IEnumerable<FrameInputDataKeyValue>
            , IEnumerable
        {
            KeyboardFrameInputData _target;
            public ValuesEnumerable(KeyboardFrameInputData target)
            {
                _target = target;
            }

            public IEnumerator<FrameInputDataKeyValue> GetEnumerator()
                => _target.KeyButtons
                .Select(_t => new FrameInputDataKeyValue(((int)_t.keyCode).ToString(), _t.observer))
                .GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }
        #endregion

        #region ISerializable interface
        public KeyboardFrameInputData(SerializationInfo info, StreamingContext context)
        {
            var e = info.GetEnumerator();
            while (e.MoveNext())
            {
                if (int.TryParse(e.Name, out var keyCodeInt))
                {
                    var keyCode = (KeyCode)keyCodeInt;
                    SetKeyButton(keyCode, (InputDefines.ButtonCondition)e.Value);
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var (keyCode, observer) in KeyButtons
                .Where(_t => _t.observer.DidUpdated))
            {
                info.AddValue(((int)keyCode).ToString(), (int)observer.Value);
            }
        }
        #endregion

        [SerializationKeyTypeGetter]
        public static System.Type GetKeyType(string key)
        {
            if (int.TryParse(key, out var number))
            {
                return null != System.Enum.ToObject(typeof(KeyCode), number)
                    ? typeof(int)
                    : null;
            }
            return null;
        }
    }


    public static partial class KeyboardFrameInputDataExtensions
    {
        public static KeyboardFrameInputData AddArrowKeyCode(this KeyboardFrameInputData target)
        {
            target.AddEnabledKeyCode(
                KeyCode.UpArrow,
                KeyCode.DownArrow,
                KeyCode.RightArrow,
                KeyCode.LeftArrow
            );
            return target;
        }

        public static KeyboardFrameInputData AddAlphabetKeyCode(this KeyboardFrameInputData target)
        {
            target.AddEnabledKeyCode(
                KeyCode.A,
                KeyCode.B,
                KeyCode.C,
                KeyCode.D,
                KeyCode.E,
                KeyCode.F,
                KeyCode.G,
                KeyCode.H,
                KeyCode.I,
                KeyCode.J,
                KeyCode.K,
                KeyCode.L,
                KeyCode.M,
                KeyCode.N,
                KeyCode.O,
                KeyCode.P,
                KeyCode.Q,
                KeyCode.R,
                KeyCode.S,
                KeyCode.T,
                KeyCode.U,
                KeyCode.V,
                KeyCode.W,
                KeyCode.X,
                KeyCode.Y,
                KeyCode.Z
            );
            return target;
        }

        public static KeyboardFrameInputData AddKeypadKeyCode(this KeyboardFrameInputData target)
        {
            target.AddEnabledKeyCode(
                KeyCode.Keypad0,
                KeyCode.Keypad1,
                KeyCode.Keypad2,
                KeyCode.Keypad3,
                KeyCode.Keypad4,
                KeyCode.Keypad5,
                KeyCode.Keypad6,
                KeyCode.Keypad7,
                KeyCode.Keypad8,
                KeyCode.Keypad9,
                KeyCode.KeypadPeriod,
                KeyCode.KeypadDivide,
                KeyCode.KeypadMultiply,
                KeyCode.KeypadMinus,
                KeyCode.KeypadPlus,
                KeyCode.KeypadEnter,
                KeyCode.KeypadEquals
            );
            return target;
        }

        public static KeyboardFrameInputData AddSymbolKeyCode(this KeyboardFrameInputData target)
        {
            target.AddEnabledKeyCode(
                KeyCode.Tab,
                KeyCode.Space,
                KeyCode.Exclaim,
                KeyCode.DoubleQuote,
                KeyCode.Hash,
                KeyCode.Dollar,
                KeyCode.Percent,
                KeyCode.Ampersand,
                KeyCode.Quote,
                KeyCode.LeftParen,
                KeyCode.RightParen,
                KeyCode.Asterisk,
                KeyCode.Plus,
                KeyCode.Comma,
                KeyCode.Minus,
                KeyCode.Period,
                KeyCode.Slash,
                KeyCode.Colon,
                KeyCode.Semicolon,
                KeyCode.Less,
                KeyCode.Equals,
                KeyCode.Greater,
                KeyCode.Question,
                KeyCode.At,
                KeyCode.LeftBracket,
                KeyCode.Backslash,
                KeyCode.RightBracket,
                KeyCode.Caret,
                KeyCode.Underscore,
                KeyCode.BackQuote,
                KeyCode.LeftCurlyBracket,
                KeyCode.Pipe,
                KeyCode.RightCurlyBracket,
                KeyCode.Tilde
            );
            return target;
        }
        public static KeyboardFrameInputData AddSystemKeyCode(this KeyboardFrameInputData target)
        {
            target.AddEnabledKeyCode(
                KeyCode.Backspace,
                KeyCode.Delete,
                KeyCode.Clear,
                KeyCode.Return,
                KeyCode.Pause,
                KeyCode.Escape,
                KeyCode.Insert,
                KeyCode.Home,
                KeyCode.End,
                KeyCode.PageUp,
                KeyCode.PageDown,
                KeyCode.Numlock,
                KeyCode.CapsLock,
                KeyCode.ScrollLock,
                KeyCode.RightShift,
                KeyCode.LeftShift,
                KeyCode.RightControl,
                KeyCode.LeftControl,
                KeyCode.RightAlt,
                KeyCode.LeftAlt,
                KeyCode.LeftCommand,
                KeyCode.LeftWindows,
                KeyCode.RightCommand,
                KeyCode.RightWindows,
                KeyCode.AltGr,
                KeyCode.Help,
                KeyCode.Print,
                KeyCode.SysReq,
                KeyCode.Break,
                KeyCode.Menu
            );
            return target;
        }
        public static KeyboardFrameInputData AddFunctionKeyCode(this KeyboardFrameInputData target)
        {
            target.AddEnabledKeyCode(
                KeyCode.F1,
                KeyCode.F2,
                KeyCode.F3,
                KeyCode.F4,
                KeyCode.F5,
                KeyCode.F6,
                KeyCode.F7,
                KeyCode.F8,
                KeyCode.F9,
                KeyCode.F10,
                KeyCode.F11,
                KeyCode.F12,
                KeyCode.F13,
                KeyCode.F14,
	            KeyCode.F15
            );
            return target;
        }
        public static KeyboardFrameInputData AddJoyStickKeyCode(this KeyboardFrameInputData target)
        {
            target.AddEnabledKeyCode(
                KeyCode.JoystickButton0,
                KeyCode.JoystickButton1,
                KeyCode.JoystickButton2,
                KeyCode.JoystickButton3,
                KeyCode.JoystickButton4,
                KeyCode.JoystickButton5,
                KeyCode.JoystickButton6,
                KeyCode.JoystickButton7,
                KeyCode.JoystickButton8,
                KeyCode.JoystickButton9,
                KeyCode.JoystickButton10,
                KeyCode.JoystickButton11,
                KeyCode.JoystickButton12,
                KeyCode.JoystickButton13,
                KeyCode.JoystickButton14,
                KeyCode.JoystickButton15,
                KeyCode.JoystickButton16,
                KeyCode.JoystickButton17,
                KeyCode.JoystickButton18,
                KeyCode.JoystickButton19,
                KeyCode.Joystick1Button0,
                KeyCode.Joystick1Button1,
                KeyCode.Joystick1Button2,
                KeyCode.Joystick1Button3,
                KeyCode.Joystick1Button4,
                KeyCode.Joystick1Button5,
                KeyCode.Joystick1Button6,
                KeyCode.Joystick1Button7,
                KeyCode.Joystick1Button8,
                KeyCode.Joystick1Button9,
                KeyCode.Joystick1Button10,
                KeyCode.Joystick1Button11,
                KeyCode.Joystick1Button12,
                KeyCode.Joystick1Button13,
                KeyCode.Joystick1Button14,
                KeyCode.Joystick1Button15,
                KeyCode.Joystick1Button16,
                KeyCode.Joystick1Button17,
                KeyCode.Joystick1Button18,
                KeyCode.Joystick1Button19,
                KeyCode.Joystick2Button0,
                KeyCode.Joystick2Button1,
                KeyCode.Joystick2Button2,
                KeyCode.Joystick2Button3,
                KeyCode.Joystick2Button4,
                KeyCode.Joystick2Button5,
                KeyCode.Joystick2Button6,
                KeyCode.Joystick2Button7,
                KeyCode.Joystick2Button8,
                KeyCode.Joystick2Button9,
                KeyCode.Joystick2Button10,
                KeyCode.Joystick2Button11,
                KeyCode.Joystick2Button12,
                KeyCode.Joystick2Button13,
                KeyCode.Joystick2Button14,
                KeyCode.Joystick2Button15,
                KeyCode.Joystick2Button16,
                KeyCode.Joystick2Button17,
                KeyCode.Joystick2Button18,
                KeyCode.Joystick2Button19,
                KeyCode.Joystick3Button0,
                KeyCode.Joystick3Button1,
                KeyCode.Joystick3Button2,
                KeyCode.Joystick3Button3,
                KeyCode.Joystick3Button4,
                KeyCode.Joystick3Button5,
                KeyCode.Joystick3Button6,
                KeyCode.Joystick3Button7,
                KeyCode.Joystick3Button8,
                KeyCode.Joystick3Button9,
                KeyCode.Joystick3Button10,
                KeyCode.Joystick3Button11,
                KeyCode.Joystick3Button12,
                KeyCode.Joystick3Button13,
                KeyCode.Joystick3Button14,
                KeyCode.Joystick3Button15,
                KeyCode.Joystick3Button16,
                KeyCode.Joystick3Button17,
                KeyCode.Joystick3Button18,
                KeyCode.Joystick3Button19,
                KeyCode.Joystick4Button0,
                KeyCode.Joystick4Button1,
                KeyCode.Joystick4Button2,
                KeyCode.Joystick4Button3,
                KeyCode.Joystick4Button4,
                KeyCode.Joystick4Button5,
                KeyCode.Joystick4Button6,
                KeyCode.Joystick4Button7,
                KeyCode.Joystick4Button8,
                KeyCode.Joystick4Button9,
                KeyCode.Joystick4Button10,
                KeyCode.Joystick4Button11,
                KeyCode.Joystick4Button12,
                KeyCode.Joystick4Button13,
                KeyCode.Joystick4Button14,
                KeyCode.Joystick4Button15,
                KeyCode.Joystick4Button16,
                KeyCode.Joystick4Button17,
                KeyCode.Joystick4Button18,
                KeyCode.Joystick4Button19,
                KeyCode.Joystick5Button0,
                KeyCode.Joystick5Button1,
                KeyCode.Joystick5Button2,
                KeyCode.Joystick5Button3,
                KeyCode.Joystick5Button4,
                KeyCode.Joystick5Button5,
                KeyCode.Joystick5Button6,
                KeyCode.Joystick5Button7,
                KeyCode.Joystick5Button8,
                KeyCode.Joystick5Button9,
                KeyCode.Joystick5Button10,
                KeyCode.Joystick5Button11,
                KeyCode.Joystick5Button12,
                KeyCode.Joystick5Button13,
                KeyCode.Joystick5Button14,
                KeyCode.Joystick5Button15,
                KeyCode.Joystick5Button16,
                KeyCode.Joystick5Button17,
                KeyCode.Joystick5Button18,
                KeyCode.Joystick5Button19,
                KeyCode.Joystick6Button0,
                KeyCode.Joystick6Button1,
                KeyCode.Joystick6Button2,
                KeyCode.Joystick6Button3,
                KeyCode.Joystick6Button4,
                KeyCode.Joystick6Button5,
                KeyCode.Joystick6Button6,
                KeyCode.Joystick6Button7,
                KeyCode.Joystick6Button8,
                KeyCode.Joystick6Button9,
                KeyCode.Joystick6Button10,
                KeyCode.Joystick6Button11,
                KeyCode.Joystick6Button12,
                KeyCode.Joystick6Button13,
                KeyCode.Joystick6Button14,
                KeyCode.Joystick6Button15,
                KeyCode.Joystick6Button16,
                KeyCode.Joystick6Button17,
                KeyCode.Joystick6Button18,
                KeyCode.Joystick6Button19,
                KeyCode.Joystick7Button0,
                KeyCode.Joystick7Button1,
                KeyCode.Joystick7Button2,
                KeyCode.Joystick7Button3,
                KeyCode.Joystick7Button4,
                KeyCode.Joystick7Button5,
                KeyCode.Joystick7Button6,
                KeyCode.Joystick7Button7,
                KeyCode.Joystick7Button8,
                KeyCode.Joystick7Button9,
                KeyCode.Joystick7Button10,
                KeyCode.Joystick7Button11,
                KeyCode.Joystick7Button12,
                KeyCode.Joystick7Button13,
                KeyCode.Joystick7Button14,
                KeyCode.Joystick7Button15,
                KeyCode.Joystick7Button16,
                KeyCode.Joystick7Button17,
                KeyCode.Joystick7Button18,
                KeyCode.Joystick7Button19,
                KeyCode.Joystick8Button0,
                KeyCode.Joystick8Button1,
                KeyCode.Joystick8Button2,
                KeyCode.Joystick8Button3,
                KeyCode.Joystick8Button4,
                KeyCode.Joystick8Button5,
                KeyCode.Joystick8Button6,
                KeyCode.Joystick8Button7,
                KeyCode.Joystick8Button8,
                KeyCode.Joystick8Button9,
                KeyCode.Joystick8Button10,
                KeyCode.Joystick8Button11,
                KeyCode.Joystick8Button12,
                KeyCode.Joystick8Button13,
                KeyCode.Joystick8Button14,
                KeyCode.Joystick8Button15,
                KeyCode.Joystick8Button16,
                KeyCode.Joystick8Button17,
                KeyCode.Joystick8Button18,
                KeyCode.Joystick8Button19
            );
            return target;
        }
        public static KeyboardFrameInputData AddMouseKeyCode(this KeyboardFrameInputData target)
        {
            target.AddEnabledKeyCode(
                KeyCode.Mouse0,
                KeyCode.Mouse1,
                KeyCode.Mouse2,
                KeyCode.Mouse3,
                KeyCode.Mouse4,
                KeyCode.Mouse5,
                KeyCode.Mouse6
            );
            return target;
        }
        public static KeyboardFrameInputData AddOtherKeyCode(this KeyboardFrameInputData target)
        {
            target.AddEnabledKeyCode(
                KeyCode.None,
                KeyCode.Alpha0,
                KeyCode.Alpha1,
                KeyCode.Alpha2,
                KeyCode.Alpha3,
                KeyCode.Alpha4,
                KeyCode.Alpha5,
                KeyCode.Alpha6,
                KeyCode.Alpha7,
                KeyCode.Alpha8,
                KeyCode.Alpha9
            );
            return target;
        }
    }
}
