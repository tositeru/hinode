using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public enum PointerType
    {
        Mouse,
        Touch,
    }

    public interface IOnPointerEventData
    {
        PointerType PointerType { get; }

        Vector3 PointerPos { get; }
        Vector3 PointerPrevPos { get; }
        int FingerID { get; }

        IViewObject PointerDownViewObject { get; }
        int PressFrame { get; }
        float PressSeconds { get; }

        bool IsStationary { get; }
        float StationarySeconds { get; }
        int StationaryFrame { get; }

        bool IsDrag { get; }
        int DragFrame { get; }
        float DragSeconds { get; }

        PointerEventDispatcher Dispatcher { get; }
        ReplayableInput Input { get; }
    }

    public interface IOnPointerEventSender : IControllerSender
    {
    }

    public interface IOnPointerEventReciever : IControllerReciever
    {
    }

    #region OnPointerDown event
    public interface IOnPointerDownSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerDownReciever : IOnPointerEventReciever
    {
        void OnPointerDown(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerBeginDrag event
    public interface IOnPointerBeginDragSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerBeginDragReciever : IOnPointerEventReciever
    {
        void OnPointerBeginDrag(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerDrag event
    public interface IOnPointerDragSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerDragReciever : IOnPointerEventReciever
    {
        void OnPointerDrag(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerBeginDrag event
    public interface IOnPointerEndDragSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerEndDragReciever : IOnPointerEventReciever
    {
        void OnPointerEndDrag(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerDrop event
    public interface IOnPointerDropSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerDropReciever : IOnPointerEventReciever
    {
        void OnPointerDrop(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerUp event
    public interface IOnPointerUpSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerUpReciever : IOnPointerEventReciever
    {
        void OnPointerUp(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnClick event
    public interface IOnPointerClickSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerClickReciever : IOnPointerEventReciever
    {
        void OnPointerClick(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerEnter event
    public interface IOnPointerEnterSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerEnterReciever : IOnPointerEventReciever
    {
        void OnPointerEnter(Model sender, IOnPointerEventData eventData);
    }
    #endregion
    #region OnPointerInArea event
    public interface IOnPointerInAreaSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerInAreaReciever : IOnPointerEventReciever
    {
        void OnPointerInArea(Model sender, IOnPointerEventData eventData);
    }
    #endregion
    #region OnPointerExit event
    public interface IOnPointerExitSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerExitReciever : IOnPointerEventReciever
    {
        void OnPointerExit(Model sender, IOnPointerEventData eventData);
    }
    #endregion
}
