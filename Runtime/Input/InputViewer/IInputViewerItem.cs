using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
	/// <seealso cref="InputViewer"/>
	/// </summary>
    public abstract class IInputViewerItem : MonoBehaviour
    {
        /// <summary>
		/// 
		/// </summary>
		/// <param name="inputViewer"></param>
        public abstract void OnInitItem(InputViewer inputViewer);

        /// <summary>
		/// 
		/// </summary>
		/// <param name="inputViewer"></param>
        public abstract void OnRemoveFromViewer(InputViewer inputViewer);

        /// <summary>
		/// 
		/// </summary>
        public abstract void OnUpdateItem();

        /// <summary>
		/// 
		/// </summary>
		/// <param name="styleInfo"></param>
        public abstract void OnChangedStyle(InputViewerStyleInfo styleInfo);

        public InputViewer UseInputViewer { get; private set; }
        public ReplayableInput UseInput { get => UseInputViewer != null ? UseInputViewer.UseInput : null; }

        public void InitItem(InputViewer inputViewer)
        {
            UseInputViewer = inputViewer;
            OnInitItem(inputViewer);
        }

        public void RemoveFromViewer(InputViewer inputViewer)
        {
            OnRemoveFromViewer(inputViewer);
            UseInputViewer = null;
        }

        public void UpdateItem()
        {
            OnUpdateItem();
        }

        public void ChangedStyle(InputViewerStyleInfo styleInfo)
        {
            Assert.IsNotNull(styleInfo);
            OnChangedStyle(styleInfo);
        }

        #region Unity Callbacks
        protected virtual void Start()
        {
            if (gameObject.TryGetComponent<InputViewer>(out var viewer))
            {
                viewer.AddItem(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (gameObject.TryGetComponent<InputViewer>(out var viewer))
            {
                viewer.RemoveItem(this);
            }
        }
        #endregion

        #region Create GameObject
        public static GameObject CreateGameObject(string name)
        {
            return new GameObject(name
                , typeof(RectTransform)
                , typeof(CanvasRenderer));
        }

        public static Image CreateImage(string name)
        {
            var obj = CreateGameObject(name);
            return obj.AddComponent<Image>();
        }

        public static Text CreateText(string name)
        {
            var obj = CreateGameObject(name);
            return obj.AddComponent<Text>();
        }
        #endregion

        public static string GetButtonConditionMark(InputDefines.ButtonCondition condition)
        {
            switch(condition)
            {
                case InputDefines.ButtonCondition.Down: return "D";
                case InputDefines.ButtonCondition.Push: return "P";
                case InputDefines.ButtonCondition.Up: return "U";
                default: return "F";
            }
        }
    }
}
