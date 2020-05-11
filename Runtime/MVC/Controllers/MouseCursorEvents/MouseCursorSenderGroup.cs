using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public enum MouseCursorEventName
    {
        onCursorDown,
        onCursorPush,
        onCursorUp,

        onCursorMove,

        onCursorEnter,
        onCursorInArea,
        onCursorExit,
    }

    /// <summary>
    /// 
    /// </summary>
    public class MouseCursorEventSenderGroup : ISenderGroup
    {
        public static void ConfigControllerType()
        {
            ControllerTypeManager.EntryPair<IOnMouseCursorMoveSender, IOnMouseCursorMoveReciever>();
            ControllerTypeManager.EntryRecieverExecuter<IOnMouseCursorMoveReciever, OnMouseCursorMoveEventData>(
                (reciever, sender, eventData) => (reciever as IOnMouseCursorMoveReciever).OnMouseCursorMove(sender, eventData));
        }

        OnMouseCursorMoveEventData _onMoveEventData;

        public MouseCursorEventSenderGroup()
        {
            _onMoveEventData = new OnMouseCursorMoveEventData(ReplayableInput.Instance.MousePos, ReplayableInput.Instance.MousePos);
        }

        #region ISenderGroup abstracts
        protected override EventInfoManager CreateEventInfoManager()
            => new EventInfoManager(
                    EventInfoManager.Info.Create<IOnMouseCursorMoveSender, IOnMouseCursorMoveReciever>(MouseCursorEventName.onCursorMove));

        public override void Update(ModelViewBinderInstanceMap binderInstanceMap)
        {
            //var cursorPos = ReplayableInput.MousePos;
            //var leftBtn = ReplayableInput.GetMouseButton(InputDefines.MouseButton.Left);
            //var middleBtn = ReplayableInput.GetMouseButton(InputDefines.MouseButton.Middle);
            //var rightBtn = ReplayableInput.GetMouseButton(InputDefines.MouseButton.Right);
            EventInfos.SetEnabledEvent(MouseCursorEventName.onCursorMove, _onMoveEventData.CursorPosition != ReplayableInput.Instance.MousePos);
            if (EventInfos.DoEnabledEvent(MouseCursorEventName.onCursorMove))
            {
                _onMoveEventData.UpdatePos(ReplayableInput.Instance.MousePos);
            }
        }

        protected override object GetEventData(string keyword, Model model, IViewObject viewObject)
        {
            Assert.IsTrue(EventInfos.ContainKeyword(keyword));
            switch ((MouseCursorEventName)System.Enum.Parse(typeof(MouseCursorEventName), keyword))
            {
                case MouseCursorEventName.onCursorMove: return _onMoveEventData;
                default:
                    throw new System.NotImplementedException();
            }
        }
        #endregion
    }
}