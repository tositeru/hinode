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
                KeyCodeDefines.ArrowKeyCodes
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
                KeyCodeDefines.AlphabetKeyCodes
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
                KeyCodeDefines.FunctionKeyCodes
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
                KeyCodeDefines.JoyStickKeyCodes
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
                KeyCodeDefines.KeypadKeyCodes
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
                KeyCodeDefines.MouseKeyCodes
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
                KeyCodeDefines.OtherKeyCodes
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
                KeyCodeDefines.SymbolKeyCodes
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
                KeyCodeDefines.SystemKeyCodes
                , keyboard.EnabledKeyCodes
                , ""
            );
        }
    }
}
