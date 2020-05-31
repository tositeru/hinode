using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    public enum BasicViewLayoutName
    {
        depth,
        siblingOrder,
    }

    public static partial class ViewLayouterExtensions
    {
        public static ViewLayouter AddBasicViewLayouter(this ViewLayouter viewLayouter)
        {
            return viewLayouter
                .AddKeywords(
                    (BasicViewLayoutName.depth.ToString(), new DepthViewLayoutAccessor()),
                    (BasicViewLayoutName.siblingOrder.ToString(), new SiblingOrderViewLayoutAccessor()));
        }
    }
}
