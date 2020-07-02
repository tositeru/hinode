using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public abstract void InitItem(InputViewer inputViewer);

        /// <summary>
		/// 
		/// </summary>
		/// <param name="UseInput"></param>
        public abstract void UpdateItem(ReplayableInput UseInput);
    }
}
