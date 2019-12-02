using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    public enum BasicViewLayoutName
    {
        depth,
        siblingOrder,
        color,
    }

    public static partial class ViewLayouterExtensions
    {
        public static ViewLayouter AddBasicViewLayouter(this ViewLayouter viewLayouter)
        {
            viewLayouter.AddKeywords(
                (BasicViewLayoutName.depth.ToString(), new DepthViewLayoutAccessor()),
                (BasicViewLayoutName.siblingOrder.ToString(), new SiblingOrderViewLayoutAccessor()),
                (BasicViewLayoutName.color.ToString(), new ColorViewLayoutAccessor())
            );

            viewLayouter.AddAutoCreateViewObject(new SiblingOrderAutoViewLayoutObject.AutoCreator(), BasicViewLayoutName.siblingOrder.ToString());
            return viewLayouter;
        }
    }
}
