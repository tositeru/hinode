using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    #region CursorMove Sender/Reciever
    public class OnMouseCursorMoveEventData
    {
        public Vector3 CursorPosition { get; private set; }
        public Vector3 PrevCursorPosition { get; private set; }

        public ReplayableInput Input { get; }

        public OnMouseCursorMoveEventData(Vector3 position, Vector3 prevPosition)
        {
            CursorPosition = position;
            PrevCursorPosition = prevPosition;

            Input = ReplayableInput.Instance;
        }

        public void UpdatePos(Vector3 position)
        {
            PrevCursorPosition = CursorPosition;
            CursorPosition = position;
        }
    }

    public interface IOnMouseCursorMoveSender : IControllerSender
    {

    }

    public interface IOnMouseCursorMoveReciever : IControllerReciever
    {
        void OnMouseCursorMove(Model sender, OnMouseCursorMoveEventData eventData);
    }
    #endregion
}
