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
        public static void ConfigControllerType(EventHandlerTypeManager typeManager)
        {
            Assert.IsNotNull(typeManager);

            typeManager.EntryEventHandlerExecuter<IOnMouseCursorMoveEventHandler, OnMouseCursorMoveEventData>(
                (reciever, sender, eventData) => (reciever as IOnMouseCursorMoveEventHandler).OnMouseCursorMove(sender, eventData));

            typeManager.EntryEventHandlerExecuter<IOnMouseLeftButtonEventHandler, OnMouseButtonEventData>(
                (reciever, sender, eventData) => (reciever as IOnMouseLeftButtonEventHandler).OnMouseLeftButton(sender, eventData));

            typeManager.EntryEventHandlerExecuter<IOnMouseRightButtonEventHandler, OnMouseButtonEventData>(
                (reciever, sender, eventData) => (reciever as IOnMouseRightButtonEventHandler).OnMouseRightButton(sender, eventData));

            typeManager.EntryEventHandlerExecuter<IOnMouseMiddleButtonEventHandler, OnMouseButtonEventData>(
                (reciever, sender, eventData) => (reciever as IOnMouseMiddleButtonEventHandler).OnMouseMiddleButton(sender, eventData));
        }

        OnMouseCursorMoveEventData _onMoveEventData;
        OnMouseButtonEventData[] _onButtonEventDatas;

        public MouseEventDispatcher()
        {
            _onMoveEventData = new OnMouseCursorMoveEventData(ReplayableInput.Instance.MousePos, ReplayableInput.Instance.MousePos);
            var buttons = System.Enum.GetValues(typeof(InputDefines.MouseButton));
            _onButtonEventDatas = new OnMouseButtonEventData[buttons.Length];
            foreach (InputDefines.MouseButton btn in buttons)
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

        public override IEventDispatcherHelper CreateEventDispatcherHelpObject(Model model, IViewObject viewObject)
            => throw new System.ArgumentException($"'{this.GetType()}' don't need IEventDispatcherHelper!");

        protected override EventInfoManager CreateEventInfoManager()
            => new EventInfoManager(
                    EventInfoManager.Info.Create<IOnMouseCursorMoveEventHandler>(MouseEventName.onMouseCursorMove),
                    EventInfoManager.Info.Create<IOnMouseLeftButtonEventHandler>(MouseEventName.onMouseLeftButton),
                    EventInfoManager.Info.Create<IOnMouseRightButtonEventHandler>(MouseEventName.onMouseRightButton),
                    EventInfoManager.Info.Create<IOnMouseMiddleButtonEventHandler>(MouseEventName.onMouseMiddleButton)
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

            foreach (var btn in _onButtonEventDatas)
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