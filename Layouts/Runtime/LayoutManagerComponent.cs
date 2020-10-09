using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    /// <summary>
    /// <seealso cref="LayoutManager"/>
    /// </summary>
    [DisallowMultipleComponent()]
    public class LayoutManagerComponent : MonoBehaviour
    {
        public LayoutManager Manager { get; } = new LayoutManager();
    }
}
