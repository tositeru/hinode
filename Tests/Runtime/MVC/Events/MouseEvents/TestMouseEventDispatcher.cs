using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.Controller.Mouse
{
    /// <summary>
	/// <seealso cref="MouseEventSenderGroup"/>
	/// </summary>
    public class TestMouseCursorEventSenderGroup
    {
        [Test]
        public void EventInfoPasses()
        {
            var senderGroup = new MouseEventDispatcher();

            var eventInfo = senderGroup.EventInfos;

            var checkList = new (MouseEventName, System.Type eventHandlerType)[]
            {
                (MouseEventName.onMouseCursorMove, typeof(IOnMouseCursorMoveEventHandler)),
                (MouseEventName.onMouseLeftButton, typeof(IOnMouseLeftButtonEventHandler)),
                (MouseEventName.onMouseRightButton, typeof(IOnMouseRightButtonEventHandler)),
                (MouseEventName.onMouseMiddleButton, typeof(IOnMouseMiddleButtonEventHandler)),
            };

            foreach (var (eventName, eventHandlerType) in checkList)
            {
                Assert.IsTrue(eventInfo.ContainKeyword(eventName), $"Invalid {eventName}...");
                Assert.AreEqual(eventHandlerType, eventInfo.GetEventHandlerType(eventName), $"Invalid {eventName}...");
                Assert.IsTrue(eventInfo.DoEnabledEvent(eventName), $"Invalid {eventName}...");
            }
        }

        class OnMovePassModel : Model
            , IOnMouseCursorMoveEventHandler
        {
            public Model SendModel { get; private set; }
            public OnMouseCursorMoveEventData EventData { get; set; }
            public int RecievedCount { get; set; }

            public void OnMouseCursorMove(Model sender, OnMouseCursorMoveEventData eventData)
            {
                RecievedCount++;
                SendModel = sender;
                EventData = eventData;
            }
        }

        [Test]
        public void OnMovePasses()
        {
            #region Initial Enviroment
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestView), new TestView.ParamBinder()));
            var allBinder = new ModelViewBinder("*", null)
                .AddControllerInfo(new ControllerInfo(
                    MouseEventName.onMouseCursorMove.ToString(),
                    new EventHandlerSelector(ModelRelationShip.Self, "", "")
                ));
            var binderMap = new ModelViewBinderMap(viewInstanceCreator,
                allBinder);
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new OnMovePassModel();
            binderInstanceMap.RootModel = model;
            #endregion

            ReplayableInput.Instance.IsReplaying = true;
            ReplayableInput.Instance.RecordedMousePresent = true;
            var senderGroup = new MouseEventDispatcher();
            Assert.IsTrue(senderGroup.DoEnabledEvent(MouseEventName.onMouseCursorMove));

            {//
                var startPos = new Vector3(111f, 222f, 0);
                var endPos = new Vector3(222f, -111f, 0);
                var loopCount = 5;
                model.RecievedCount = 0;
                for (var i = 0; i < loopCount; ++i)
                {
                    var mousePos = Vector3.Lerp(startPos, endPos, (float)i / loopCount);
                    var prevMousePos = ReplayableInput.Instance.RecordedMousePos;
                    ReplayableInput.Instance.RecordedMousePos = mousePos;
                    senderGroup.Update(binderInstanceMap);
                    senderGroup.SendTo(binderInstanceMap);

                    Assert.AreEqual(i + 1, model.RecievedCount);
                    Assert.AreSame(model, model.SendModel);
                    Assert.AreEqual(mousePos, model.EventData.CursorPosition);
                    Assert.AreEqual(prevMousePos, model.EventData.PrevCursorPosition);
                }
            }

            {//
                ReplayableInput.Instance.RecordedMousePos = Vector2.zero;
                model.RecievedCount = 0;
                for (var i = 0; i < 5; ++i)
                {
                    senderGroup.Update(binderInstanceMap);
                    senderGroup.SendTo(binderInstanceMap);
                }
                Assert.AreEqual(1, model.RecievedCount, $"OnMouseCursorMove send only when move mouse position...");
            }
        }

        class OnButtonPassModel : Model
            , IOnMouseLeftButtonEventHandler
            , IOnMouseRightButtonEventHandler
            , IOnMouseMiddleButtonEventHandler
        {
            public Model SendModel { get; set; }
            public OnMouseButtonEventData LastEventData { get; set; }
            public int RecievedCount { get; set; }

            void OnMouseButton(Model sender, OnMouseButtonEventData eventData)
            {
                RecievedCount++;
                SendModel = sender;
                LastEventData = eventData;
            }

            public void OnMouseLeftButton(Model sender, OnMouseButtonEventData eventData)
                => OnMouseButton(sender, eventData);
            public void OnMouseRightButton(Model sender, OnMouseButtonEventData eventData)
                => OnMouseButton(sender, eventData);
            public void OnMouseMiddleButton(Model sender, OnMouseButtonEventData eventData)
                => OnMouseButton(sender, eventData);
        }

        [Test]
        public void OnButtonPasses()
        {
            #region Initial Enviroment
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(TestView), new TestView.ParamBinder()));
            var allBinder = new ModelViewBinder("*", null)
                .AddControllerInfo(new ControllerInfo(
                    MouseEventName.onMouseLeftButton.ToString(),
                    new EventHandlerSelector(ModelRelationShip.Self, "", "")
                ))
                .AddControllerInfo(new ControllerInfo(
                    MouseEventName.onMouseRightButton.ToString(),
                    new EventHandlerSelector(ModelRelationShip.Self, "", "")
                ))
                .AddControllerInfo(new ControllerInfo(
                    MouseEventName.onMouseMiddleButton.ToString(),
                    new EventHandlerSelector(ModelRelationShip.Self, "", "")
                ))
                ;
            var binderMap = new ModelViewBinderMap(viewInstanceCreator,
                allBinder);
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new OnButtonPassModel();
            binderInstanceMap.RootModel = model;
            #endregion

            ReplayableInput.Instance.IsReplaying = true;
            ReplayableInput.Instance.RecordedMousePresent = true;
            var senderGroup = new MouseEventDispatcher();

            var btnEvents = new MouseEventName[]
            {
                MouseEventName.onMouseLeftButton,
                MouseEventName.onMouseRightButton,
                MouseEventName.onMouseMiddleButton,
            };
            foreach (var btn in btnEvents)
            {
                Assert.IsTrue(senderGroup.DoEnabledEvent(btn), $"Invalid Mouse Button({btn})...");

                senderGroup.SetEnabledEvent(btn, false);//for Test
            }

            foreach (var (btn, eventName) in System.Enum.GetValues(typeof(InputDefines.MouseButton)).OfType<InputDefines.MouseButton>()
                .Zip(btnEvents, (b, e) => (b, e)))
            {
                senderGroup.SetEnabledEvent(eventName, true);//for Test
                {//Down
                    model.RecievedCount = 0;
                    model.LastEventData = null;
                    model.SendModel = null;
                    var condition = InputDefines.ButtonCondition.Down;
                    ReplayableInput.Instance.SetRecordedMouseButton(btn, condition);
                    senderGroup.Update(binderInstanceMap);
                    senderGroup.SendTo(binderInstanceMap);

                    var errorMessage = $"Invalid Mouse Button{btn}...";
                    Assert.AreEqual(model, model.SendModel, errorMessage);
                    Assert.AreEqual(1, model.RecievedCount, errorMessage);
                    Assert.IsNotNull(model.LastEventData, errorMessage);
                    Assert.AreEqual(btn, model.LastEventData.TargetButton, errorMessage);
                    Assert.AreEqual(condition, model.LastEventData.Condition, errorMessage);
                    Assert.AreEqual(0, model.LastEventData.PushFrame, errorMessage);
                    Assert.AreEqual(0f, model.LastEventData.PushSeconds, errorMessage);
                    Assert.AreEqual(ReplayableInput.Instance, model.LastEventData.Input, errorMessage);
                }

                var pushFrameCount = 3;
                {
                    model.RecievedCount = 0;
                    model.LastEventData = null;
                    model.SendModel = null;
                    {
                        //状態を実際のボタンを押したときのものに合わせる
                        ReplayableInput.Instance.SetRecordedMouseButton(btn, InputDefines.ButtonCondition.Down);
                        senderGroup.Update(binderInstanceMap);
                    }

                    var condition = InputDefines.ButtonCondition.Push;
                    ReplayableInput.Instance.SetRecordedMouseButton(btn, condition);

                    int prevFrame = 0;
                    float prevSecond = 0f;
                    for (var i = 0; i < pushFrameCount; ++i)
                    {//Push
                        senderGroup.Update(binderInstanceMap);
                        senderGroup.SendTo(binderInstanceMap);

                        var errorMessage = $"Invalid Mouse Button{btn}...";
                        Assert.AreEqual(model, model.SendModel, errorMessage);
                        Assert.AreEqual(i + 1, model.RecievedCount, errorMessage);
                        Assert.IsNotNull(model.LastEventData, errorMessage);
                        Assert.AreEqual(btn, model.LastEventData.TargetButton, errorMessage);
                        Assert.AreEqual(condition, model.LastEventData.Condition, errorMessage);
                        Assert.AreEqual(prevFrame + 1, model.LastEventData.PushFrame, errorMessage);
                        Assert.IsTrue(prevSecond < model.LastEventData.PushSeconds, errorMessage);
                        Assert.AreEqual(ReplayableInput.Instance, model.LastEventData.Input, errorMessage);

                        prevFrame = model.LastEventData.PushFrame;
                        prevSecond = model.LastEventData.PushSeconds;
                    }
                }

                {//Up
                    {
                        //状態を実際のボタンを押したときのものに合わせる
                        ReplayableInput.Instance.SetRecordedMouseButton(btn, InputDefines.ButtonCondition.Down);
                        senderGroup.Update(binderInstanceMap);
                        ReplayableInput.Instance.SetRecordedMouseButton(btn, InputDefines.ButtonCondition.Push);
                        senderGroup.Update(binderInstanceMap);
                    }

                    int prevFrame = model.LastEventData.PushFrame;
                    float prevSecond = model.LastEventData.PushSeconds;

                    model.RecievedCount = 0;
                    model.LastEventData = null;
                    model.SendModel = null;

                    var condition = InputDefines.ButtonCondition.Up;
                    ReplayableInput.Instance.SetRecordedMouseButton(btn, condition);
                    senderGroup.Update(binderInstanceMap);
                    senderGroup.SendTo(binderInstanceMap);

                    var errorMessage = $"Invalid Mouse Button{btn}...";
                    Assert.AreEqual(model, model.SendModel, errorMessage);
                    Assert.AreEqual(1, model.RecievedCount, errorMessage);
                    Assert.IsNotNull(model.LastEventData, errorMessage);
                    Assert.AreEqual(btn, model.LastEventData.TargetButton, errorMessage);
                    Assert.AreEqual(condition, model.LastEventData.Condition, errorMessage);
                    Assert.AreEqual(prevFrame + 1, model.LastEventData.PushFrame, errorMessage);
                    Assert.IsTrue(prevSecond < model.LastEventData.PushSeconds, errorMessage);
                    Assert.AreEqual(ReplayableInput.Instance, model.LastEventData.Input, errorMessage);
                }

                senderGroup.SetEnabledEvent(eventName, false);//for Test
            }
        }
    }
}
