using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    public enum PointerType
    {
        Mouse,
        Touch,
    }

    public interface IOnPointerEventData : IEventData
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

    public interface IOnPointerEventHandler : IEventHandler
    {
    }

    #region OnPointerDown event
    [EnableKeywordForEventHandler("onPointerDown")]
    public interface IOnPointerDownReciever : IOnPointerEventHandler
    {
        void OnPointerDown(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerBeginDrag event
    [EnableKeywordForEventHandler("onPointerBeginDrag")]
    public interface IOnPointerBeginDragReciever : IOnPointerEventHandler
    {
        void OnPointerBeginDrag(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerDrag event
    [EnableKeywordForEventHandler("onPointerDrag")]
    public interface IOnPointerDragReciever : IOnPointerEventHandler
    {
        void OnPointerDrag(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerBeginDrag event
    [EnableKeywordForEventHandler("onPointerEndDrag")]
    public interface IOnPointerEndDragReciever : IOnPointerEventHandler
    {
        void OnPointerEndDrag(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerDrop event
    [EnableKeywordForEventHandler("onPointerDrop")]
    public interface IOnPointerDropReciever : IOnPointerEventHandler
    {
        void OnPointerDrop(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerUp event
    [EnableKeywordForEventHandler("onPointerUp")]
    public interface IOnPointerUpReciever : IOnPointerEventHandler
    {
        void OnPointerUp(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnClick event
    [EnableKeywordForEventHandler("onPointerClick")]
    public interface IOnPointerClickReciever : IOnPointerEventHandler
    {
        void OnPointerClick(Model sender, IOnPointerEventData eventData);
    }
    #endregion

    #region OnPointerEnter event
    [EnableKeywordForEventHandler("onPointerEnter")]
    public interface IOnPointerEnterReciever : IOnPointerEventHandler
    {
        void OnPointerEnter(Model sender, IOnPointerEventData eventData);
    }
    #endregion
    #region OnPointerInArea event
    [EnableKeywordForEventHandler("onPointerInArea")]
    public interface IOnPointerInAreaReciever : IOnPointerEventHandler
    {
        void OnPointerInArea(Model sender, IOnPointerEventData eventData);
    }
    #endregion
    #region OnPointerExit event
    [EnableKeywordForEventHandler("onPointerExit")]
    public interface IOnPointerExitReciever : IOnPointerEventHandler
    {
        void OnPointerExit(Model sender, IOnPointerEventData eventData);
    }
    #endregion
}
