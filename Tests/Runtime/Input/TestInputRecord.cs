using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="InputRecord"/>
    /// </summary>
    public class TestInputRecord : TestBase
    {
        [System.Serializable]
        class FrameData
        {
#pragma warning disable CS0649
            public int n;
            public string s;
#pragma warning restore CS0649
        }

        // A Test behaves as an ordinary method
        [Test]
        public void BasicUsagePasses()
        {
            var screenSize = new Vector2Int(128, 128);

            var inputRecord = InputRecord.Create(screenSize);
            Assert.AreEqual(screenSize, inputRecord.ScreenSize);

            for(var i=0; i<10; ++i)
            {
			    var frame = new InputRecord.Frame((uint)i, 16f/1000f);
                frame.SetInputTextByJson(new FrameData { n = i, s = $"msg{i}" });
			    inputRecord.Push(frame);
            }
            Assert.AreEqual(10, inputRecord.FrameCount);
            Assert.IsTrue(inputRecord.Frames.Any());

            //Frameデータのクリアー処理
            inputRecord.ClearFrames();
            Assert.AreEqual(screenSize, inputRecord.ScreenSize);
            Assert.AreEqual(0, inputRecord.FrameCount);
            Assert.IsFalse(inputRecord.Frames.Any());
        }

        [Test]
        public void InputRecordFrameBasicUsagePasses()
        {
            var frame = new InputRecord.Frame(0, 16f / 1000f);
            frame.InputText = null;
            Assert.IsTrue(frame.IsEmptyInputText);
            frame.InputText = "";
            Assert.IsTrue(frame.IsEmptyInputText);
            frame.InputText = "{}";
            Assert.IsTrue(frame.IsEmptyInputText, "InputRecord#Frame#InputTextが'{}'の時は空として扱うようにしています。");

            frame.InputText = "{hogehoge}";
            Assert.IsFalse(frame.IsEmptyInputText);
        }
    }
}
