using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public class OnPointerEventData
    {
        public Vector2 PointerPos { get; set; }
        public InputDefines.MouseButton MouseButtonType { get; set; }
        public int FingerID { get; set; }

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

    #region OnPointerBeginDrag event
    public class OnPointerBeginDragEventData : OnPointerEventData
    {
        public Vector2 PointerDownPos { get; set; }

    }
    public interface IOnPointerBeginDragSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerBeginDragReciever : IOnPointerEventReciever
    {
        void OnPointerBeginDrag(Model sender, OnPointerBeginDragEventData eventData);
    }
    #endregion

    #region OnPointerDrag event
    public class OnPointerDragEventData : OnPointerEventData
    {
        public Vector2 PointerDownPos { get; set; }

    }
    public interface IOnPointerDragSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerDragReciever : IOnPointerEventReciever
    {
        void OnPointerDrag(Model sender, OnPointerDragEventData eventData);
    }
    #endregion

    #region OnPointerBeginDrag event
    public class OnPointerEndDragEventData : OnPointerEventData
    {
        public Vector2 PointerDownPos { get; set; }

    }
    public interface IOnPointerEndDragSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerEndDragReciever : IOnPointerEventReciever
    {
        void OnPointerEndDrag(Model sender, OnPointerEndDragEventData eventData);
    }
    #endregion

    #region OnPointerDrop event
    public class OnPointerDropEventData : OnPointerEventData
    {
        public Vector2 PointerDownPos { get; set; }

    }
    public interface IOnPointerDropSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerDropReciever : IOnPointerEventReciever
    {
        void OnPointerDrop(Model sender, OnPointerDropEventData eventData);
    }
    #endregion

    #region OnPointerUp event
    public class OnPointerUpEventData : OnPointerEventData
    {
        public Vector2 PointerDownPos { get; set; }

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
        public Vector2 PointerDownPos { get; set; }
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

    #region OnPointerEnter event
    public class OnPointerEnterEventData : OnPointerEventData
    {
    }
    public interface IOnPointerEnterSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerEnterReciever : IOnPointerEventReciever
    {
        void OnPointerEnter(Model sender, OnPointerEnterEventData eventData);
    }
    #endregion

    #region OnPointerExit event
    public class OnPointerExitEventData : OnPointerEventData
    {
    }
    public interface IOnPointerExitSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerExitReciever : IOnPointerEventReciever
    {
        void OnPointerExit(Model sender, OnPointerExitEventData eventData);
    }
    #endregion

}
