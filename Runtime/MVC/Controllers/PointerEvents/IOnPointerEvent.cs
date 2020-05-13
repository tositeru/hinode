using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public enum PointerType
    {
        Mouse,
        Touch,
    }
    public class OnPointerEventData
    {
        public PointerType PointerType { get; }

        public Vector3 PointerPos { get; set; }
        public InputDefines.MouseButton MouseButtonType { get; set; }
        public int FingerID { get; set; } = -1;

        public ReplayableInput Input { get => ReplayableInput.Instance; }

        public OnPointerEventData(PointerType pointerType)
        {
            PointerType = pointerType;
        }
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
        void OnPointerDown(Model sender, OnPointerEventData eventData);
    }
    #endregion

    #region OnPointerBeginDrag event
    public interface IOnPointerBeginDragSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerBeginDragReciever : IOnPointerEventReciever
    {
        void OnPointerBeginDrag(Model sender, OnPointerEventData eventData);
    }
    #endregion

    #region OnPointerDrag event
    public interface IOnPointerDragSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerDragReciever : IOnPointerEventReciever
    {
        void OnPointerDrag(Model sender, OnPointerEventData eventData);
    }
    #endregion

    #region OnPointerBeginDrag event
    public interface IOnPointerEndDragSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerEndDragReciever : IOnPointerEventReciever
    {
        void OnPointerEndDrag(Model sender, OnPointerEventData eventData);
    }
    #endregion

    #region OnPointerDrop event
    public interface IOnPointerDropSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerDropReciever : IOnPointerEventReciever
    {
        void OnPointerDrop(Model sender, OnPointerEventData eventData);
    }
    #endregion

    #region OnPointerUp event
    public interface IOnPointerUpSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerUpReciever : IOnPointerEventReciever
    {
        void OnPointerUp(Model sender, OnPointerEventData eventData);
    }
    #endregion

    #region OnClick event
    public interface IOnPointerClickSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerClickReciever : IOnPointerEventReciever
    {
        void OnPointerClick(Model sender, OnPointerEventData eventData);
    }
    #endregion

    #region OnPointerEnter event
    public interface IOnPointerEnterSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerEnterReciever : IOnPointerEventReciever
    {
        void OnPointerEnter(Model sender, OnPointerEventData eventData);
    }
    #endregion
    #region OnPointerStationary event
    public interface IOnPointerStationarySender : IOnPointerEventSender
    {
    }

    public interface IOnPointerStationaryReciever : IOnPointerEventReciever
    {
        void OnPointerStationary(Model sender, OnPointerEventData eventData);
    }
    #endregion
    #region OnPointerExit event
    public interface IOnPointerExitSender : IOnPointerEventSender
    {
    }

    public interface IOnPointerExitReciever : IOnPointerEventReciever
    {
        void OnPointerExit(Model sender, OnPointerEventData eventData);
    }
    #endregion
}
