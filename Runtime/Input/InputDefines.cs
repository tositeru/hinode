using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hinode
{
    /// <summary>
    /// 入力系の定数や便利関数を定義するクラス
    /// </summary>
    public static class InputDefines
    {
        /// <summary>
        /// マウスのボタン
        /// </summary>
        public enum MouseButton
        {
            Left = 0,
            Right = 1,
            Middle = 2,
        }

        /// <summary>
        /// ボタンの状態
        /// </summary>
        public enum ButtonCondition
        {
            Free,
            Down,
            Push,
            Up,
        }

        /// <summary>
        /// StandaloneInputModuleから
        /// UnityEngine.EventSystemで使用されているものが以下のものだったので、それをリスト化している。
        /// </summary>
        public static readonly string[] UnityUIEventSystemButtonNames =
        {
            "Horizontal",
            "Vertical",
            "Submit",
            "Cancel",
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static MouseButton ToMouseType(PointerEventData.InputButton button)
        {
            switch (button)
            {
                case PointerEventData.InputButton.Left: return MouseButton.Left;
                case PointerEventData.InputButton.Right: return MouseButton.Right;
                case PointerEventData.InputButton.Middle: return MouseButton.Middle;
                default:
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// 指定したInputのマウスボタンの状態をButtonConditionに変換する
        /// </summary>
        /// <param name="baseInput"></param>
        /// <param name="btn"></param>
        /// <returns></returns>
        public static ButtonCondition ToButtonCondition(MouseButton btn)
        {
            var index = (int)btn;
            return Input.GetMouseButtonDown(index)
                ? InputDefines.ButtonCondition.Down
                : Input.GetMouseButtonUp(index)
                    ? InputDefines.ButtonCondition.Up
                    : Input.GetMouseButton(index)
                        ? InputDefines.ButtonCondition.Push
                        : InputDefines.ButtonCondition.Free;
        }

        /// <summary>
        /// 指定したBaseInputのマウスボタンの状態をButtonConditionに変換する
        /// </summary>
        /// <param name="baseInput"></param>
        /// <param name="btn"></param>
        /// <returns></returns>
        public static ButtonCondition ToButtonCondition(BaseInput baseInput, MouseButton btn)
        {
            var index = (int)btn;
            return baseInput.GetMouseButtonDown(index)
                ? InputDefines.ButtonCondition.Down
                : baseInput.GetMouseButtonUp(index)
                    ? InputDefines.ButtonCondition.Up
                    : baseInput.GetMouseButton(index)
                        ? InputDefines.ButtonCondition.Push
                        : InputDefines.ButtonCondition.Free;
        }

        /// <summary>
        /// 指定したTouchの状態をButtonConditionに変換する
        /// </summary>
        /// <param name="baseInput"></param>
        /// <param name="btn"></param>
        /// <returns></returns>
        public static ButtonCondition ToButtonCondition(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    return ButtonCondition.Down;
                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    return ButtonCondition.Push;
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    return ButtonCondition.Up;
                default:
                    throw new System.NotImplementedException();
            }
        }

    }
}

