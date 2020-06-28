﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hinode.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input.FrameInputDataRecorder
{
    /// <summary>
    /// <seealso cref="TouchFrameInputData"/>
    /// </summary>
    public class TestTouchFrameInputData
    {
        /// <summary>
        /// <seealso cref="TouchFrameInputData.TouchSupported"/>
        /// <seealso cref="TouchFrameInputData.TouchPressureSupported"/>
        /// <seealso cref="TouchFrameInputData.MultiTouchEnabled"/>
        /// <seealso cref="TouchFrameInputData.StylusTouchSupported"/>
        /// <seealso cref="TouchFrameInputData.SimulateMouseWithTouches"/>
        /// <seealso cref="TouchFrameInputData.TouchCount"/>
        /// <seealso cref="TouchFrameInputData.GetTouch(int)"/>
        /// <seealso cref="TouchFrameInputData.SetTouch(int, Touch)"/>
        /// </summary>
        [Test]
        public void PropertiesPasses()
        {
            var data = new TouchFrameInputData();

            {//TouchSupported
                data.TouchSupported = !data.TouchSupported;
                var observer = data.GetValuesEnumerable()
                    .First(_t => _t.Key == TouchFrameInputData.KeyTouchSupported)
                    .Value;
                Assert.AreEqual(data.TouchSupported, observer.RawValue);
                Assert.IsTrue(observer.DidUpdated);
            }
            Debug.Log($"Success to TouchSupported!");

            {//TouchPressureSupported
                data.TouchPressureSupported = !data.TouchPressureSupported;
                var observer = data.GetValuesEnumerable()
                    .First(_t => _t.Key == TouchFrameInputData.KeyTouchPressureSupported)
                    .Value;
                Assert.AreEqual(data.TouchPressureSupported, observer.RawValue);
                Assert.IsTrue(observer.DidUpdated);
            }
            Debug.Log($"Success to TouchPressureSupported!");

            {//StylusTouchSupported
                data.StylusTouchSupported = !data.StylusTouchSupported;
                var observer = data.GetValuesEnumerable()
                    .First(_t => _t.Key == TouchFrameInputData.KeyStylusTouchSupported)
                    .Value;
                Assert.AreEqual(data.StylusTouchSupported, observer.RawValue);
                Assert.IsTrue(observer.DidUpdated);
            }
            Debug.Log($"Success to StylusTouchSupported!");

            {//SimulateMouseWithTouches
                data.SimulateMouseWithTouches = !data.SimulateMouseWithTouches;
                var observer = data.GetValuesEnumerable()
                    .First(_t => _t.Key == TouchFrameInputData.KeySimulateMouseWithTouches)
                    .Value;
                Assert.AreEqual(data.SimulateMouseWithTouches, observer.RawValue);
                Assert.IsTrue(observer.DidUpdated);
            }
            Debug.Log($"Success to SimulateMouseWithTouches!");

            {//MultiTouchEnabled
                data.MultiTouchEnabled = !data.MultiTouchEnabled;
                var observer = data.GetValuesEnumerable()
                    .First(_t => _t.Key == TouchFrameInputData.KeyMultiTouchEnabled)
                    .Value;
                Assert.AreEqual(data.MultiTouchEnabled, observer.RawValue);
                Assert.IsTrue(observer.DidUpdated);
            }
            Debug.Log($"Success to MultiTouchEnabled!");

            for (int i = 0; i < TouchFrameInputData.LIMIT_TOUCH_COUNT; ++i)
            {
                var errorMessage = $"Failed Set/GetTouch()... index={i}";
                data.SetTouch(i, new Touch { fingerId = i + 10 });
                var observer = data.GetValuesEnumerable()
                    .First(_t => _t.Key == i.ToString())
                    .Value;
                Assert.AreEqual(data.GetTouch(i), observer.RawValue, errorMessage);
                Assert.IsTrue(observer.DidUpdated, errorMessage);
                Debug.Log($"Success to Touch({i})!");
            }
        }

        /// <summary>
        /// <seealso cref="TouchFrameInputData.GetValuesEnumerable()"/>
        /// </summary>
        [Test]
        public void GetValuesEnumerablePasses()
        {
            var data = new TouchFrameInputData();
            AssertionUtils.AssertEnumerableByUnordered(
                Enumerable.Range(0, TouchFrameInputData.LIMIT_TOUCH_COUNT)
                    .Select(_i => (_i.ToString(), data.GetTouch(_i).RawValue))
                    .Concat(
                        new (string key, object value)[]
                        {
                            (TouchFrameInputData.KeyTouchSupported, data.TouchSupported),
                            (TouchFrameInputData.KeyTouchPressureSupported, data.TouchPressureSupported),
                            (TouchFrameInputData.KeyMultiTouchEnabled, data.MultiTouchEnabled),
                            (TouchFrameInputData.KeySimulateMouseWithTouches, data.SimulateMouseWithTouches),
                            (TouchFrameInputData.KeyStylusTouchSupported, data.StylusTouchSupported),
                            (TouchFrameInputData.KeyTouchCount, data.TouchCount),
                        }),
                data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()..."
            );
        }

        /// <summary>
        /// <seealso cref="TouchFrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="TouchFrameInputData.GetKeyType(string)"/>
        /// </summary>
        [Test]
        public void GetKeyTypePasses()
        {
            var data = new TouchFrameInputData();

            var testData = data.GetValuesEnumerable()
                    .Select(_t =>
                    {
                        if (int.TryParse(_t.Key, out var _))
                        {
                            return (key: _t.Key, type: _t.Value.GetType());
                        }
                        else
                        {
                            return (key: _t.Key, type: _t.Value.RawValue.GetType());
                        }
                    });
            foreach (var d in testData)
            {
                var errorMessage = $"Don't match key and Type... key={d.key}";
                Assert.AreEqual(d.type, TouchFrameInputData.GetKeyType(d.key), errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="TouchFrameInputData.Record(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecordPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;
            //Touch
            replayInput.RecordedTouchSupported = true;
            replayInput.RecordedTouchPressureSupported = true;
            replayInput.RecordedMultiTouchEnabled = true;
            replayInput.RecordedStylusTouchSupported = true;
            replayInput.RecordedSimulateMouseWithTouches = true;
            replayInput.RecordedTouchCount = 2;
            replayInput.SetRecordedTouch(0, new Touch { fingerId = 11 });
            replayInput.SetRecordedTouch(1, new Touch { fingerId = 22 });

            //データが正しく設定されるか確認
            var data = new TouchFrameInputData();
            data.Record(replayInput);

            Assert.AreEqual(replayInput.TouchSupported, data.TouchSupported);
            Assert.AreEqual(replayInput.TouchPressureSupported, data.TouchPressureSupported);
            Assert.AreEqual(replayInput.MultiTouchEnabled, data.MultiTouchEnabled);
            Assert.AreEqual(replayInput.StylusTouchSupported, data.StylusTouchSupported);
            Assert.AreEqual(replayInput.SimulateMouseWithTouches, data.SimulateMouseWithTouches);
            Assert.AreEqual(replayInput.TouchCount, data.TouchCount);
            for (var i = 0; i < data.TouchCount; ++i)
            {
                Assert.AreEqual(replayInput.GetTouch(i), data.GetTouch(i), $"Failed Touch... index={i}");
            }
        }

        /// <summary>
        /// シリアライズも含めたデータ更新処理が想定しているように動作しているか確認するテスト
        /// <seealso cref="TouchFrameInputData.GetValuesEnumerable()"/>
        /// <seealso cref="TouchFrameInputData.TouchFrameInputData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
        /// <seealso cref="TouchFrameInputData.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)"/>
        /// </summary>
        [Test]
        public void SerializationPasses()
        {
            var data = new TouchFrameInputData();
            //Mouse
            data.TouchSupported = true;
            data.TouchPressureSupported = true;
            data.MultiTouchEnabled = true;
            data.StylusTouchSupported = true;
            data.SimulateMouseWithTouches = true;
            data.TouchCount = 2;
            data.SetTouch(0, new Touch { fingerId = 0, deltaTime = 0.5f, phase = TouchPhase.Ended });
            data.SetTouch(1, new Touch { fingerId = 1, deltaTime = 0.25f, phase = TouchPhase.Moved });

            var refCache = new RefCache(data.GetType());

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(data);
            Debug.Log($"debug -- json:{json}");
            var dest = serializer.Deserialize<TouchFrameInputData>(json);

            {
                var errorMessage = "Failed to serialize value Count...";
                Assert.AreEqual(data.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count()
                    , dest.GetValuesEnumerable().Where(_t => _t.Value.DidUpdated).Count(), errorMessage);
            }
            Debug.Log($"Success to Serialization Value Count(Only Updated)!");

            {
                Assert.AreEqual(data.TouchSupported, dest.TouchSupported);
                Assert.AreEqual(data.TouchPressureSupported, dest.TouchPressureSupported);
                Assert.AreEqual(data.MultiTouchEnabled, dest.MultiTouchEnabled);
                Assert.AreEqual(data.SimulateMouseWithTouches, dest.SimulateMouseWithTouches);
                Assert.AreEqual(data.StylusTouchSupported, dest.StylusTouchSupported);
                Assert.AreEqual(data.TouchCount, dest.TouchCount);

                for (var i = 0; i < data.TouchCount; ++i)
                {
                    Assert.AreEqual(data.GetTouch(i), dest.GetTouch(i));
                }
            }
            Debug.Log($"Success to Serialization Value(Only Updated)!");
        }

        /// <summary>
        /// <seealso cref="TouchFrameInputData.RecoverTo(ReplayableInput)"/>
        /// </summary>
        [Test]
        public void RecoverToPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;
            replayInput.IsReplaying = true;

            var data = new TouchFrameInputData();
            //Touch
            replayInput.RecordedTouchSupported = true;
            replayInput.RecordedTouchPressureSupported = true;
            replayInput.RecordedMultiTouchEnabled = true;
            replayInput.RecordedStylusTouchSupported = true;
            replayInput.RecordedSimulateMouseWithTouches = true;
            replayInput.RecordedTouchCount = 2;
            replayInput.SetRecordedTouch(0, new Touch { fingerId = 11 });
            replayInput.SetRecordedTouch(1, new Touch { fingerId = 22 });

            //データが正しく設定されるか確認
            data.RecoverTo(replayInput);

            {
                var errorMessage = "Failed to Set Input Datas to ReplayableInput...";
                Assert.AreEqual(data.TouchSupported, replayInput.TouchSupported, errorMessage);
                Assert.AreEqual(data.TouchPressureSupported, replayInput.TouchPressureSupported, errorMessage);
                Assert.AreEqual(data.MultiTouchEnabled, replayInput.MultiTouchEnabled, errorMessage);
                Assert.AreEqual(data.StylusTouchSupported, replayInput.StylusTouchSupported, errorMessage);
                Assert.AreEqual(data.SimulateMouseWithTouches, replayInput.SimulateMouseWithTouches, errorMessage);
                Assert.AreEqual(data.TouchCount, replayInput.TouchCount, errorMessage);
                for (var i = 0; i < data.TouchCount; ++i)
                {
                    Assert.AreEqual(data.GetTouch(i), replayInput.GetTouch(i), $"{errorMessage}: touchIndex={i}");
                }
            }
            Debug.Log($"Success to Set Input Datas to ReplayableInput!");
        }

        /// <summary>
        /// <see cref="TouchFrameInputData.ResetDatas()"/>
        /// </summary>
        [Test]
        public void ResetDatasPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;
            //Touch
            replayInput.RecordedTouchSupported = true;
            replayInput.RecordedTouchPressureSupported = true;
            replayInput.RecordedMultiTouchEnabled = true;
            replayInput.RecordedStylusTouchSupported = true;
            replayInput.RecordedSimulateMouseWithTouches = true;
            replayInput.RecordedTouchCount = 2;
            replayInput.SetRecordedTouch(0, new Touch { fingerId = 11 });
            replayInput.SetRecordedTouch(1, new Touch { fingerId = 22 });

            //データが正しく設定されるか確認
            var data = new TouchFrameInputData();
            data.Record(replayInput);

            data.ResetDatas(); // <- Test run here

            Assert.IsTrue(data.GetValuesEnumerable().All(_t => !_t.Value.DidUpdated), "Failed to reset DidUpdated...");
            AssertionUtils.AssertEnumerableByUnordered(
                new (string key, object value)[]
                {
                    (TouchFrameInputData.KeyTouchSupported, default(bool)),
                    (TouchFrameInputData.KeyTouchPressureSupported, default(bool)),
                    (TouchFrameInputData.KeyMultiTouchEnabled, default(bool)),
                    (TouchFrameInputData.KeyStylusTouchSupported, default(bool)),
                    (TouchFrameInputData.KeySimulateMouseWithTouches, default(bool)),
                    (TouchFrameInputData.KeyTouchCount, default(int)),
                }.Concat(Enumerable.Range(0, TouchFrameInputData.LIMIT_TOUCH_COUNT)
                    .Select(_i => (key: _i.ToString(), value: (object)new TouchUpdateObserver()))
                ),
                data.GetValuesEnumerable().Select(_t => (_t.Key, _t.Value.RawValue))
                , "Don't match GetValuesEnumerable()..."
            );
        }

        /// <summary>
        /// <see cref="TouchFrameInputData.RefleshUpdatedFlags()"/>
        /// </summary>
        [Test]
        public void RefleshUpdatedFlagsPasses()
        {
            //好きなデータを指定できるためReplayableInputを使用している
            var replayInput = ReplayableInput.Instance;

            replayInput.IsReplaying = true;
            //Touch
            replayInput.RecordedTouchSupported = true;
            replayInput.RecordedTouchPressureSupported = true;
            replayInput.RecordedMultiTouchEnabled = true;
            replayInput.RecordedStylusTouchSupported = true;
            replayInput.RecordedSimulateMouseWithTouches = true;
            replayInput.RecordedTouchCount = 2;
            replayInput.SetRecordedTouch(0, new Touch { fingerId = 11 });
            replayInput.SetRecordedTouch(1, new Touch { fingerId = 22 });

            //データが正しく設定されるか確認
            var data = new TouchFrameInputData();
            data.Record(replayInput);

            data.RefleshUpdatedFlags(); // <- Test run here

            foreach (var t in data.GetValuesEnumerable())
            {
                Assert.IsFalse(t.Value.DidUpdated, $"Key({t.Key}) don't reflesh Update Flags...");
            }
        }
    }
}
