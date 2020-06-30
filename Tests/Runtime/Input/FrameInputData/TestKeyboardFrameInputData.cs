using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hinode.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input.FrameInputDataRecorder
{
    /// <summary>
    /// <seealso cref="KeyboardFrameInputData"/>
    /// </summary>
    public class TestKeyboardFrameInputData
    {
        [SetUp]
        public void SetUp()
        {
            FrameInputData.ClearChildFrameInputDataType();
        }

        /// <summary>
        /// <see cref="KeyboardFrameInputData.GetKeyButton(KeyCode)"/>
        /// <see cref="KeyboardFrameInputData.SetKeyButton(KeyCode, InputDefines.ButtonCondition)"/>
        /// <see cref="KeyboardFrameInputData.KeyButtons"/>
        /// </summary>
        [Test]
        public void KeyButtonsPasses()
        {
            var data = new KeyboardFrameInputData();

            foreach (var keyCode in System.Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>())
            {
                var errorMessage = $"Failed KeyButton... KeyCode={keyCode}";
                Assert.AreEqual(InputDefines.ButtonCondition.Free, data.GetKeyButton(keyCode), errorMessage);

                //Debug.Log($"Success to not Set yet to Button({keyCode})!");

                data.SetKeyButton(keyCode, InputDefines.ButtonCondition.Push);
                Assert.AreEqual(InputDefines.ButtonCondition.Push, data.GetKeyButton(keyCode), errorMessage);
                Assert.IsTrue(data.KeyButtons.Any(_t => _t.keyCode == keyCode), errorMessage);

                //Debug.Log($"Success to Set to Button({keyCode})!");

                data.SetKeyButton(keyCode, InputDefines.ButtonCondition.Free);
                Assert.IsTrue(data.KeyButtons.Any(_t => _t.keyCode == keyCode), errorMessage);

                //Debug.Log($"Success to Set ButtonCondition.Free to Button({keyCode})!");
            }
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputData.SetKeyButton(KeyCode, InputDefines.ButtonCondition)"/>
        /// <seealso cref="KeyboardFrameInputData.GetKeyButton(KeyCode)"/>
        /// </summary>
        [Test]
        public void TransformedKeyCodePasses()
        {
            var data = new KeyboardFrameInputData();

            var testData = new (KeyCode use, KeyCode transform)[] {
                (KeyCode.LeftApple, KeyCode.LeftCommand),
                (KeyCode.RightApple, KeyCode.RightCommand),
            };

            foreach (var d in testData)
            {
                var errorMessage = $"keyCode={d.use}, transformed={d.transform}";
                data.SetKeyButton(d.use, InputDefines.ButtonCondition.Push);
                Assert.AreEqual(data.GetKeyButton(d.transform), data.GetKeyButton(d.use), errorMessage);

                data.SetKeyButton(d.use, InputDefines.ButtonCondition.Down);
                Assert.AreEqual(data.GetKeyButton(d.transform), data.GetKeyButton(d.use), errorMessage);

                data.SetKeyButton(d.use, InputDefines.ButtonCondition.Free);
                Assert.AreEqual(data.GetKeyButton(d.transform), data.GetKeyButton(d.use), errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputData.AddEnabledKeyCode(IEnumerable{KeyCode})"/>
        /// <seealso cref="KeyboardFrameInputData.EnabledKeyCodes"/>
        /// </summary>
        [Test]
        public void AddKeyFilterPasses()
        {
            var data = new KeyboardFrameInputData();
            AssertionUtils.AssertEnumerableByUnordered(KeyboardFrameInputData.AllKeyCodes, data.EnabledKeyCodes, "");
            Debug.Log($"Success Initial EnabledKeyCodes!");

            var enableKeyCodes = new KeyCode[]
            {
                KeyCode.A,
                KeyCode.LeftCommand,
                KeyCode.At,
                KeyCode.F2,
                KeyCode.Keypad2,
            };
            data.AddEnabledKeyCode(enableKeyCodes);
            AssertionUtils.AssertEnumerableByUnordered(enableKeyCodes, data.EnabledKeyCodes, "");
            Debug.Log($"Success to SetEnabledKeyCode!");

            var allKeyCodes = KeyboardFrameInputData.AllKeyCodes;
            foreach (var keyCode in allKeyCodes)
            {
                var condition = InputDefines.ButtonCondition.Push;
                data.SetKeyButton(keyCode, condition);

                if (enableKeyCodes.Contains(keyCode))
                {
                    Assert.AreEqual(condition, data.GetKeyButton(keyCode), $"Fail Filtering KeyCode({keyCode})...");
                }
                else
                {
                    Assert.AreNotEqual(condition, data.GetKeyButton(keyCode), $"Fail None Filtering KeyCode({keyCode})...");
                }
            }
            Debug.Log($"Success to Filter EnabledKeyCode!");

            AssertionUtils.AssertEnumerableByUnordered(data.EnabledKeyCodes, data.KeyButtons.Select(_t => _t.keyCode), "");
            Debug.Log($"Success to Filtering KeyButtons!");
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputData.RemoveEnabledKeyCode(IEnumerable{KeyCode})"/>
        /// <seealso cref="KeyboardFrameInputData.EnabledKeyCodes"/>
        /// </summary>
        [Test]
        public void RemoveKeyFilterPasses()
        {
            var data = new KeyboardFrameInputData();

            var enableKeyCodes = new KeyCode[]
            {
                KeyCode.A,
                KeyCode.LeftCommand,
                KeyCode.At,
                KeyCode.F2,
                KeyCode.Keypad2,
            };
            data.AddEnabledKeyCode(enableKeyCodes);

            var removeKeyCodes = new KeyCode[]
            {
                KeyCode.A, KeyCode.B, KeyCode.At
            };
            data.RemoveEnabledKeyCode(removeKeyCodes);

            var allKeyCodes = System.Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>();

            foreach (var keyCode in allKeyCodes)
            {
                var condition = InputDefines.ButtonCondition.Push;
                data.SetKeyButton(keyCode, condition);

                if (enableKeyCodes.Contains(keyCode) && !removeKeyCodes.Contains(keyCode))
                {
                    Assert.AreEqual(condition, data.GetKeyButton(keyCode), $"Fail Filtering KeyCode({keyCode})...");
                }
                else
                {
                    Assert.AreNotEqual(condition, data.GetKeyButton(keyCode), $"Fail None Filtering KeyCode({keyCode})...");
                }
            }
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputData.GetValuesEnumerable()"/>
        /// </summary>
        [Test, Description("KeyButton only Get Not ButtonCondition.Free!")]
        public void GetValuesEnumerablePasses()
        {
            var data = new KeyboardFrameInputData();
            var testData = new (KeyCode keyCode, InputDefines.ButtonCondition condition)[]
            {
                (KeyCode.A, InputDefines.ButtonCondition.Push),
                (KeyCode.Keypad3, InputDefines.ButtonCondition.Down),
                (KeyCode.JoystickButton8, InputDefines.ButtonCondition.Up),
                (KeyCode.F1, InputDefines.ButtonCondition.Down),
                (KeyCode.LeftShift, InputDefines.ButtonCondition.Push),
            };

            foreach (var (keyCode, condition) in testData)
            {
                data.SetKeyButton(keyCode, condition);
            }

            AssertionUtils.AssertEnumerableByUnordered(
                testData.Select(_t => (((int)_t.keyCode).ToString(), (object)_t.condition))
                , data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()... only Not ButtonCondition.Free KeyCode..."
            );
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="KeyboardFrameInputData.GetKeyType(string)"/>
        /// </summary>
        [Test]
        public void GetKeyTypePasses()
        {
            var data = new KeyboardFrameInputData();

            var allKeyCodes = System.Enum.GetValues(typeof(KeyCode))
                .OfType<KeyCode>();

            var testData = allKeyCodes.Select(_k => (key: ((int)_k).ToString(), type: typeof(int)))
                    .Distinct();
            foreach (var d in testData)
            {
                var errorMessage = $"Don't match key and Type... key={d.key}";
                Assert.AreEqual(d.type, KeyboardFrameInputData.GetKeyType(d.key), errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputData.Record(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecordPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;
            replayInput.IsReplaying = true;

            var allKeyCodes = System.Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>();
            foreach (var keyCode in allKeyCodes)
            {
                var n = (int)keyCode % 5;
                if (n < 4)
                {
                    var condition = (InputDefines.ButtonCondition)(n);
                    replayInput.SetRecordedKeyButton(keyCode, condition);
                }
            }

            //データが正しく設定されるか確認
            var data = new KeyboardFrameInputData();
            data.Record(replayInput);

            foreach (var keyCode in allKeyCodes)
            {
                Assert.AreEqual(replayInput.GetRecordedKeyButton(keyCode), data.GetKeyButton(keyCode), $"Fail KeyCode({keyCode})");
            }
        }

        /// <summary>
        /// シリアライズも含めたデータ更新処理が想定しているように動作しているか確認するテスト
        /// <seealso cref="KeyboardFrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="KeyboardFrameInputData.KeyboardFrameInputData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
        /// <seealso cref="KeyboardFrameInputData.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
        /// </summary>
        [Test]
        public void SerializationPasses()
        {
            var data = new KeyboardFrameInputData();

            var allKeyCodes = System.Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>();
            foreach (var keyCode in allKeyCodes)
            {
                var n = (int)keyCode % 5;
                if (n < 4)
                {
                    var condition = (InputDefines.ButtonCondition)(n);
                    data.SetKeyButton(keyCode, condition);
                }
            }

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(data);
            Debug.Log($"debug -- json:{json}");
            var dest = serializer.Deserialize<KeyboardFrameInputData>(json);

            {
                var errorMessage = "Failed to serialize value Count...";
                Assert.AreEqual(data.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count()
                    , dest.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count(), errorMessage);
            }
            Debug.Log($"Success to Serialization Value Count(Only Updated)!");

            {
                foreach (var keyCode in allKeyCodes)
                {
                    Assert.AreEqual(data.GetKeyButton(keyCode), dest.GetKeyButton(keyCode), $"Failed to serialize Key Button({keyCode})...");
                }
            }
            Debug.Log($"Success to Serialization Value(Only Updated)!");
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputData.RecoverTo(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecoverToPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            var data = new KeyboardFrameInputData();

            var allKeyCodes = System.Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>();
            foreach (var keyCode in allKeyCodes)
            {
                var n = (int)keyCode % 5;
                if (n < 4)
                {
                    var condition = (InputDefines.ButtonCondition)(n);
                    data.SetKeyButton(keyCode, condition);
                }
            }

            //データが正しく設定されるか確認
            data.RecoverTo(replayInput);

            {
                var errorMessage = "Failed to Set Input Datas to ReplayableInput...";
                foreach (var keyCode in allKeyCodes)
                {
                    Assert.AreEqual(data.GetKeyButton(keyCode), replayInput.GetRecordedKeyButton(keyCode), $"{errorMessage}... keyCode={keyCode}");
                }
            }
            Debug.Log($"Success to Set Input Datas to ReplayableInput!");
        }

        /// <summary>
        /// <see cref="KeyboardFrameInputData.ResetDatas()"/>
        /// </summary>
        [Test]
        public void ResetDatasPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;

            var allKeyCodes = System.Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>();
            foreach (var keyCode in allKeyCodes)
            {
                var n = (int)keyCode % 5;
                if (n < 4)
                {
                    var condition = (InputDefines.ButtonCondition)(n);
                    replayInput.SetRecordedKeyButton(keyCode, condition);
                }
            }

            //データが正しく設定されるか確認
            var data = new KeyboardFrameInputData();
            data.Record(replayInput);

            data.ResetDatas(); // <- Test run here

            Assert.IsTrue(data.GetValuesEnumerable().All(_t => !_t.Value.DidUpdated), "Failed to reset DidUpdated...");
            AssertionUtils.AssertEnumerableByUnordered(
                new (string key, object value)[] { },
                data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()..."
            );
        }

        /// <summary>
        /// <see cref="KeyboardFrameInputData.RefleshUpdatedFlags()"/>
        /// </summary>
        [Test]
        public void RefleshUpdatedFlagsPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;

            var allKeyCodes = System.Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>();
            foreach (var keyCode in allKeyCodes)
            {
                var n = (int)keyCode % 5;
                if (n < 4)
                {
                    var condition = (InputDefines.ButtonCondition)(n);
                    replayInput.SetRecordedKeyButton(keyCode, condition);
                }
            }

            //データが正しく設定されるか確認
            var data = new KeyboardFrameInputData();
            data.Record(replayInput);

            data.RefleshUpdatedFlags(); // <- Test run here

            foreach (var t in data.GetValuesEnumerable())
            {
                Assert.IsFalse(t.Value.DidUpdated, $"Key({t.Key}) don't reflesh Update Flags...");
            }
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputData.RegistTypeToFrameInputData()"/>
        /// </summary>
        [Test]
        public void RegistTypeToFrameInputDataPasses()
        {
            KeyboardFrameInputData.RegistTypeToFrameInputData();

            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType(KeyboardFrameInputData.KEY_CHILD_INPUT_DATA_TYPE));
            Assert.IsTrue(FrameInputData.ContainsChildFrameInputDataType<KeyboardFrameInputData>());
            Assert.AreEqual(typeof(KeyboardFrameInputData), FrameInputData.GetChildFrameInputDataType(KeyboardFrameInputData.KEY_CHILD_INPUT_DATA_TYPE));
            Assert.AreEqual(KeyboardFrameInputData.KEY_CHILD_INPUT_DATA_TYPE, FrameInputData.GetChildFrameInputDataKey<KeyboardFrameInputData>());

            Assert.DoesNotThrow(() => {
                KeyboardFrameInputData.RegistTypeToFrameInputData();
            });
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputDataExtensions.AddArrowKeyCode(KeyboardFrameInputData)"/>
        /// </summary>
        [Test]
        public void AddArrowKeyCodePasses()
        {
            var keyboard = new KeyboardFrameInputData();
            keyboard.AddArrowKeyCode();

            AssertionUtils.AssertEnumerableByUnordered(
                new KeyCode[] {
                    KeyCode.UpArrow,
                    KeyCode.DownArrow,
                    KeyCode.RightArrow,
                    KeyCode.LeftArrow
                }
                , keyboard.EnabledKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputDataExtensions.AddAlphabetKeyCode(KeyboardFrameInputData)"/>
        /// </summary>
        [Test]
        public void AddAlphabetKeyCodePasses()
        {
            var keyboard = new KeyboardFrameInputData();
            keyboard.AddAlphabetKeyCode();

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
                , keyboard.EnabledKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputDataExtensions.AddFunctionKeyCode(KeyboardFrameInputData)"/>
        /// </summary>
        [Test]
        public void AddFunctionKeyCodePasses()
        {
            var keyboard = new KeyboardFrameInputData();
            keyboard.AddFunctionKeyCode();

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
                , keyboard.EnabledKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputDataExtensions.AddJoyStickKeyCode(KeyboardFrameInputData)"/>
        /// </summary>
        [Test]
        public void AddJoyStickKeyCodePasses()
        {
            var keyboard = new KeyboardFrameInputData();
            keyboard.AddJoyStickKeyCode();

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
                , keyboard.EnabledKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputDataExtensions.AddKeypadKeyCode(KeyboardFrameInputData)"/>
        /// </summary>
        [Test]
        public void AddKeypadKeyCodePasses()
        {
            var keyboard = new KeyboardFrameInputData();
            keyboard.AddKeypadKeyCode();

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
                , keyboard.EnabledKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputDataExtensions.AddMouseKeyCode(KeyboardFrameInputData)"/>
        /// </summary>
        [Test]
        public void AddMouseKeyCodePasses()
        {
            var keyboard = new KeyboardFrameInputData();
            keyboard.AddMouseKeyCode();

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
                , keyboard.EnabledKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputDataExtensions.AddOtherKeyCode(KeyboardFrameInputData)"/>
        /// </summary>
        [Test]
        public void AddOtherKeyCodePasses()
        {
            var keyboard = new KeyboardFrameInputData();
            keyboard.AddOtherKeyCode();

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
                , keyboard.EnabledKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputDataExtensions.AddSymbolKeyCode(KeyboardFrameInputData)"/>
        /// </summary>
        [Test]
        public void AddSymbolKeyCodePasses()
        {
            var keyboard = new KeyboardFrameInputData();
            keyboard.AddSymbolKeyCode();

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
                , keyboard.EnabledKeyCodes
                , ""
            );
        }

        /// <summary>
        /// <seealso cref="KeyboardFrameInputDataExtensions.AddSystemKeyCode(KeyboardFrameInputData)"/>
        /// </summary>
        [Test]
        public void AddSystemKeyCodePasses()
        {
            var keyboard = new KeyboardFrameInputData();
            keyboard.AddSystemKeyCode();

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
                , keyboard.EnabledKeyCodes
                , ""
            );
        }


    }
}
