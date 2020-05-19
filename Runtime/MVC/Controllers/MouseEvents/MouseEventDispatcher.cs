using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public enum MouseEventName
    {
        onMouseLeftButton,
        onMouseRightButton,
        onMouseMiddleButton,

        onMouseCursorMove,
    }

    /// <summary>
    /// 
    /// </summary>
    public class MouseEventDispatcher : IEventDispatcher
    {
        public static void ConfigControllerType()
        {
            ControllerTypeManager.EntryPair<IOnMouseCursorMoveSender, IOnMouseCursorMoveReciever>();
            ControllerTypeManager.EntryRecieverExecuter<IOnMouseCursorMoveReciever, OnMouseCursorMoveEventData>(
                (reciever, sender, eventData) => (reciever as IOnMouseCursorMoveReciever).OnMouseCursorMove(sender, eventData));

            ControllerTypeManager.EntryPair<IOnMouseLeftButtonSender, IOnMouseLeftButtonReciever>();
            ControllerTypeManager.EntryRecieverExecuter<IOnMouseLeftButtonReciever, OnMouseButtonEventData>(
                (reciever, sender, eventData) => (reciever as IOnMouseLeftButtonReciever).OnMouseLeftButton(sender, eventData));

            ControllerTypeManager.EntryPair<IOnMouseRightButtonSender, IOnMouseRightButtonReciever>();
            ControllerTypeManager.EntryRecieverExecuter<IOnMouseRightButtonReciever, OnMouseButtonEventData>(
                (reciever, sender, eventData) => (reciever as IOnMouseRightButtonReciever).OnMouseRightButton(sender, eventData));

            ControllerTypeManager.EntryPair<IOnMouseMiddleButtonSender, IOnMouseMiddleButtonReciever>();
            ControllerTypeManager.EntryRecieverExecuter<IOnMouseMiddleButtonReciever, OnMouseButtonEventData>(
                (reciever, sender, eventData) => (reciever as IOnMouseMiddleButtonReciever).OnMouseMiddleButton(sender, eventData));

        }

        OnMouseCursorMoveEventData _onMoveEventData;
        OnMouseButtonEventData[] _onButtonEventDatas;

        public MouseEventDispatcher()
        {
            _onMoveEventData = new OnMouseCursorMoveEventData(ReplayableInput.Instance.MousePos, ReplayableInput.Instance.MousePos);
            var buttons = System.Enum.GetValues(typeof(InputDefines.MouseButton));
            _onButtonEventDatas = new OnMouseButtonEventData[buttons.Length];
            foreach(InputDefines.MouseButton btn in buttons)
            {
                _onButtonEventDatas[(int)btn] = new OnMouseButtonEventData(btn);
            }
        }

        #region ISenderGroup abstracts
        bool _doEnabled = true;
        public override bool DoEnabled
        {
            get => _doEnabled && ReplayableInput.Instance.MousePresent;
            set => _doEnabled = value;
        }

        public override bool IsCreatableControllerObject(Model model, IViewObject viewObject)
            => false;

        public override IControllerObject CreateControllerObject(Model model, IViewObject viewObject)
            => throw new System.ArgumentException($"'{this.GetType()}' don't need IControllerObject!");

        protected override EventInfoManager CreateEventInfoManager()
            => new EventInfoManager(
                    EventInfoManager.Info.Create<IOnMouseCursorMoveSender, IOnMouseCursorMoveReciever>(MouseEventName.onMouseCursorMove),
                    EventInfoManager.Info.Create<IOnMouseLeftButtonSender, IOnMouseLeftButtonReciever>(MouseEventName.onMouseLeftButton),
                    EventInfoManager.Info.Create<IOnMouseRightButtonSender, IOnMouseRightButtonReciever>(MouseEventName.onMouseRightButton),
                    EventInfoManager.Info.Create<IOnMouseMiddleButtonSender, IOnMouseMiddleButtonReciever>(MouseEventName.onMouseMiddleButton)
                    );

        protected override void UpdateImpl(ModelViewBinderInstanceMap binderInstanceMap)
        {
            //var cursorPos = ReplayableInput.MousePos;
            //var leftBtn = ReplayableInput.GetMouseButton(InputDefines.MouseButton.Left);
            //var middleBtn = ReplayableInput.GetMouseButton(InputDefines.MouseButton.Middle);
            //var rightBtn = ReplayableInput.GetMouseButton(InputDefines.MouseButton.Right);
            EventInfos.SetEnabledEvent(MouseEventName.onMouseCursorMove, _onMoveEventData.CursorPosition != ReplayableInput.Instance.MousePos);
            if (EventInfos.DoEnabledEvent(MouseEventName.onMouseCursorMove))
            {
                _onMoveEventData.UpdatePos(ReplayableInput.Instance.MousePos);
            }

            foreach(var btn in _onButtonEventDatas)
            {
                btn.Update();
            }
        }

        protected override object GetEventData(Model model, IViewObject viewObject, ControllerInfo controllerInfo)
        {
            Assert.IsTrue(EventInfos.ContainKeyword(controllerInfo.Keyword));
            switch ((MouseEventName)System.Enum.Parse(typeof(MouseEventName), controllerInfo.Keyword))
            {
                case MouseEventName.onMouseCursorMove: return _onMoveEventData;
                case MouseEventName.onMouseLeftButton: return _onButtonEventDatas[(int)InputDefines.MouseButton.Left];
                case MouseEventName.onMouseRightButton: return _onButtonEventDatas[(int)InputDefines.MouseButton.Right];
                case MouseEventName.onMouseMiddleButton: return _onButtonEventDatas[(int)InputDefines.MouseButton.Middle];
                default:
                    throw new System.NotImplementedException();
            }
        }
        #endregion
    }
}