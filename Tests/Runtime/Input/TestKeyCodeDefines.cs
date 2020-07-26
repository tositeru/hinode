using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Input
{
    /// <summary>
	/// <seealso cref="KeyCodeDefines"/>
	/// </summary>
    public class TestKeyCodesDefines
    {
        /// <summary>
        /// <seealso cref="KeyCodeDefines.IsSupportedKeyCodes(KeyCode keyCode)"/>
        /// </summary>
        [Test]
        public void IsSupportedKeyCodesPasses()
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                var errorMessage = $"Fail test... keyCode={keyCode}";
                Assert.IsTrue(
                    KeyCodeDefines.IsSupportedKeyCodes(keyCode)
                    , errorMessage);
            }
        }

        /// <summary>
		/// <seealso cref="KeyCodeDefines.AllSupportedKeyCodes"/>
		/// </summary>
        [Test]
        public void AllSupportedKeyCodesPasses()
        {
            var ignoreKeyCodes = new HashSet<KeyCode>()
            {
                KeyCode.RightApple,
                KeyCode.LeftApple,
            };

            // 同値のEnumに対応するために使用している
            var ignoreCounter = new Dictionary<KeyCode, int>();

            AssertionUtils.AssertEnumerableByUnordered(
                System.Enum.GetValues(typeof(KeyCode))
                    .GetEnumerable<KeyCode>()
                    .Where(_k => {
                        if (!ignoreKeyCodes.Contains(_k)) return true;
                        if (ignoreCounter.ContainsKey(_k)) return false;
                        ignoreCounter.Add(_k, 1);
                        return true;
                    }).ToArray(),
                KeyCodeDefines.AllSupportedKeyCodes,
                ""
            );
        }

        /// <summary>
		/// <seealso cref="KeyCodeDefines.ArrowKeyCodes"/>
        /// </summary>
        [Test]
        public void ArrowKeyCodesPasses()
        {
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] {
                    KeyCode.UpArrow,
                    KeyCode.DownArrow,
                    KeyCode.RightArrow,
                    KeyCode.LeftArrow
                }
                , KeyCodeDefines.ArrowKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyCodeDefines.AlphabetKeyCodes"/>
        /// </summary>
        [Test]
        public void AlphabetKeyCodesPasses()
        {
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] {
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
                }
                , KeyCodeDefines.AlphabetKeyCodes
                , ""
            );
        }

        /// <summary>
		/// <seealso cref="KeyCodeDefines.FunctionKeyCodes"/>
        /// </summary>
        [Test]
        public void FunctionKeyCodesPasses()
        {
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] {
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
                }
                , KeyCodeDefines.FunctionKeyCodes
                , ""
            );
        }

        /// <summary>
		/// <seealso cref="KeyCodeDefines.JoyStickKeyCodes"/>
        /// </summary>
        [Test]
        public void JoyStickKeyCodesPasses()
        {
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] {
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
                }
                , KeyCodeDefines.JoyStickKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyCodeDefines.KeypadKeyCodes"/>
        /// </summary>
        [Test]
        public void KeypadKeyCodesPasses()
        {
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] {
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
                }
                , KeyCodeDefines.KeypadKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyCodeDefines.MouseKeyCodes"/>
        /// </summary>
        [Test]
        public void MouseKeyCodesPasses()
        {
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] {
                    KeyCode.Mouse0,
                    KeyCode.Mouse1,
                    KeyCode.Mouse2,
                    KeyCode.Mouse3,
                    KeyCode.Mouse4,
                    KeyCode.Mouse5,
                    KeyCode.Mouse6
                }
                , KeyCodeDefines.MouseKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyCodeDefines.OtherKeyCodes"/>
        /// </summary>
        [Test]
        public void OtherKeyCodesPasses()
        {
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] {
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
                }
                , KeyCodeDefines.OtherKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyCodeDefines.SymbolKeyCodes"/>
        /// </summary>
        [Test]
        public void SymbolKeyCodesPasses()
        {
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] {
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
                }
                , KeyCodeDefines.SymbolKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyCodeDefines.SystemKeyCodes"/>
        /// </summary>
        [Test]
        public void SystemKeyCodesPasses()
        {
            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] {
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
                }
                , KeyCodeDefines.SystemKeyCodes
                , ""
            );
        }
    }
}
