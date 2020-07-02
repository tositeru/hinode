using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hinode
{
    /// <summary>
	/// <seealso cref="InputViewer"/>
	/// <seealso cref="IInputViewerItem"/>
	/// </summary>
    public class MouseInputViewerItem : IInputViewerItem
    {
        public bool DoEnabled { get => throw new System.NotImplementedException(); }

        public Image Cursor { get => throw new System.NotImplementedException(); }
        public Text ButtonsText { get => throw new System.NotImplementedException(); }

        #region override IInputViewerItem
        public override void InitItem(InputViewer inputViewer)
        {

        }

        public override void UpdateItem(ReplayableInput UseInput)
        {

        }
        #endregion
    }
}
