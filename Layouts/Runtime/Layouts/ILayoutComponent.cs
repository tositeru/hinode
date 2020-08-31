using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    /// <summary>
    /// <seealso cref="ILayout"/>
    /// </summary>
    public interface ILayoutComponent
    {
        ILayout LayoutInstance { get; }
    }
}
