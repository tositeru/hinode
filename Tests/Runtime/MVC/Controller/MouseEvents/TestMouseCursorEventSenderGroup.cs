using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.Controller.Mouse
{
    /// <summary>
	/// <seealso cref="MouseCursorEventSenderGroup"/>
	/// </summary>
    public class TestMouseCursorEventSenderGroup
    {
        class OnMovePassModel : Model
            , IOnMouseCursorMoveReciever
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
                .AddControllerInfo(new ModelViewBinder.ControllerInfo(
                    MouseCursorEventName.onCursorMove.ToString(),
                    new RecieverSelector(ModelRelationShip.Self, "", "")
                ));
            var binderMap = new ModelViewBinderMap(viewInstanceCreator,
                allBinder);
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new OnMovePassModel();
            binderInstanceMap.RootModel = model;
            #endregion

            MouseCursorEventSenderGroup.ConfigControllerType();

            ReplayableInput.Instance.IsReplaying = true;
            var senderGroup = new MouseCursorEventSenderGroup();

			{//
                var startPos = new Vector3(111f, 222f, 0);
                var endPos = new Vector3(222f, -111f, 0);
                var loopCount = 5;
                model.RecievedCount = 0;
                for (var i=0; i < loopCount; ++i)
			    {
                    var mousePos = Vector3.Lerp(startPos, endPos, (float)i/ loopCount);
                    var prevMousePos = ReplayableInput.Instance.RecordedMousePos;
                    ReplayableInput.Instance.RecordedMousePos = mousePos;
                    senderGroup.Update(binderInstanceMap);
                    senderGroup.SendTo(binderInstanceMap);

                    Assert.AreEqual(i+1, model.RecievedCount);
                    Assert.AreSame(model, model.SendModel);
                    Assert.AreEqual(mousePos, model.EventData.CursorPosition);
                    Assert.AreEqual(prevMousePos, model.EventData.PrevCursorPosition);
                }
            }

			{//
                ReplayableInput.Instance.RecordedMousePos = Vector2.zero;
                model.RecievedCount = 0;
                for (var i=0; i<5; ++i)
				{
                    senderGroup.Update(binderInstanceMap);
                    senderGroup.SendTo(binderInstanceMap);
				}
                Assert.AreEqual(1, model.RecievedCount, $"OnMouseCursorMove send only when move mouse position...");
            }
        }
    }
}
