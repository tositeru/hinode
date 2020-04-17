using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public class OnPointerEventData
    {
        public Vector2 PointerPos { get; }
        public InputDefines.MouseButton MouseButtonType { get; }
        public int FingerID { get; }

        public OnPointerEventData() { }
    }

    public interface IOnPointerEventSender : IControllerSender
    {
    }

    public interface IOnPointerEventReciever : IControllerReciever
    {
    }

    #region OnPointerDown event
    public class OnPointerDownEventData : OnPointerEventData
    {

    }
    public interface IOnPointerDownSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerDownReciever : IOnPointerEventReciever
    {
        void OnPointerDown(Model sender, OnPointerDownEventData eventData);
    }
    #endregion

    #region OnPointerMove event
    public class OnPointerMoveEventData : OnPointerEventData
    {
        public Vector2 PointerDownPos { get; }

    }
    public interface IOnPointerMoveSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerMoveReciever : IOnPointerEventReciever
    {
        void OnPointerMove(Model sender, OnPointerMoveEventData eventData);
    }
    #endregion

    #region OnPointerUp event
    public class OnPointerUpEventData : OnPointerEventData
    {
        public Vector2 PointerDownPos { get; }

    }
    public interface IOnPointerUpSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerUpReciever : IOnPointerEventReciever
    {
        void OnPointerUp(Model sender, OnPointerUpEventData eventData);
    }
    #endregion

    #region OnClick event
    public class OnPointerClickEventData : OnPointerEventData
    {
        public Vector2 PointerDownPos { get; }
        public OnPointerClickEventData() { }
    }

    public interface IOnPointerClickSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerClickReciever : IOnPointerEventReciever
    {
        void OnPointerClick(Model sender, OnPointerClickEventData eventData);
    }
    #endregion
}
