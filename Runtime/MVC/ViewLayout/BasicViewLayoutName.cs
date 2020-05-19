using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public enum BasicViewLayoutName
    {
        depth
    }

    public static partial class ViewLayouterExtensions
    {
        public static ViewLayouter AddBasicViewLayouter(this ViewLayouter viewLayouter)
        {
            return viewLayouter
                .AddKeywords((BasicViewLayoutName.depth.ToString(), new DepthViewLayoutAccessor()));
        }
    }
}
