using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    public interface IOnMouseEventHandler : IEventHandler { }

    #region MouseCursorMove Sender/Reciever
    public class OnMouseCursorMoveEventData : IEventData
    {
        public Vector3 CursorPosition { get; private set; }
        public Vector3 PrevCursorPosition { get; private set; }

        public ReplayableInput Input { get => ReplayableInput.Instance; }

        public OnMouseCursorMoveEventData(Vector3 position, Vector3 prevPosition)
        {
            CursorPosition = position;
            PrevCursorPosition = prevPosition;
        }

        public void UpdatePos(Vector3 position)
        {
            PrevCursorPosition = CursorPosition;
            CursorPosition = position;
        }

        #region IEventData
        public IEventData Clone()
        {
            return new OnMouseCursorMoveEventData(CursorPosition, PrevCursorPosition);
        }
        #endregion
    }

    [EnableKeywordForEventHandler("onMouseCursorMove")]
    public interface IOnMouseCursorMoveEventHandler : IOnMouseEventHandler
    {
        void OnMouseCursorMove(Model sender, OnMouseCursorMoveEventData eventData);
    }
    #endregion

    #region MouseButton Events Sender/Reciever
    public class OnMouseButtonEventData: IEventData
    {
        public InputDefines.MouseButton TargetButton { get; }
        public InputDefines.ButtonCondition Condition { get; private set; }
        public float PushSeconds { get; private set; } = 0f;
        public int PushFrame { get; private set; } = 0;

        public ReplayableInput Input { get => ReplayableInput.Instance; }

        public OnMouseButtonEventData(InputDefines.MouseButton targetButton)
        {
            TargetButton = targetButton;
        }

        public void Update()
        {
            Condition = Input.GetMouseButton(TargetButton);
            switch(Condition)
            {
                case InputDefines.ButtonCondition.Free:
                case InputDefines.ButtonCondition.Down:
                    PushSeconds = 0;
                    PushFrame = 0;
                    break;
                case InputDefines.ButtonCondition.Push:
                case InputDefines.ButtonCondition.Up:
                    PushSeconds += Time.deltaTime;
                    PushFrame ++;
                    break;
                default:
                    throw new System.NotFiniteNumberException();
            }
        }

        #region IEventData
        public IEventData Clone()
        {
            var clone = new OnMouseButtonEventData(TargetButton);
            clone.Condition = Condition;
            clone.PushFrame = PushFrame;
            clone.PushSeconds = PushSeconds;
            return clone;
        }
        #endregion
    }

    [EnableKeywordForEventHandler("onMouseLeftButton")]
    public interface IOnMouseLeftButtonEventHandler : IOnMouseEventHandler
    {
        void OnMouseLeftButton(Model sender, OnMouseButtonEventData eventData);
    }

    [EnableKeywordForEventHandler("onMouseRightButton")]
    public interface IOnMouseRightButtonEventHandler : IOnMouseEventHandler
    {
        void OnMouseRightButton(Model sender, OnMouseButtonEventData eventData);
    }

    [EnableKeywordForEventHandler("onMouseMiddleButton")]
    public interface IOnMouseMiddleButtonEventHandler : IOnMouseEventHandler
    {
        void OnMouseMiddleButton(Model sender, OnMouseButtonEventData eventData);
    }
    #endregion

}
