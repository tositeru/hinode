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

    public interface IOnPointerEventSender
    {
    }

    public interface IOnPointerEventReciever
    {
    }

    #region OnClick event
    public class OnClickEventData : OnPointerEventData
    {
        public Vector2 ClickPos { get => base.PointerPos; }
        public OnClickEventData() { }
    }

    public interface IOnClickSender : IOnPointerEventSender
    {
    }

    public interface IOnClickReciever : IOnPointerEventReciever
    {
        void OnClicked(IOnClickSender sender, OnClickEventData eventData);
    }
    #endregion
}
