using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.MVC.Tests.ViewLayout
{
    /// <summary>
    /// <seealso cref="BasicViewLayoutName"/>
    /// </summary>
    public class TestBasicViewLayoutName
    {
        [Test]
        public void CheckAddBasicViewLayouterPasses()
        {
            var viewLayouter = new ViewLayouter()
                .AddBasicViewLayouter();
            var keywords = new Dictionary<string, (IViewLayoutAccessor accessor, bool containAutoViewObj)>() {
                { BasicViewLayoutName.depth.ToString(), (new DepthViewLayoutAccessor(), false) },
                { BasicViewLayoutName.siblingOrder.ToString(), (new SiblingOrderViewLayoutAccessor(), true) }
            };
            foreach (var (keyword, accessor, containAutoViewObj) in keywords.Select(_t => (_t.Key, _t.Value.accessor, _t.Value.containAutoViewObj)))
            {
                Assert.IsTrue(viewLayouter.ContainsKeyword(keyword), $"Don't exist {keyword}...");
                Assert.AreSame(accessor.GetType(), viewLayouter.Accessors[keyword].GetType(), $"cur={accessor.GetType()}, got={viewLayouter.Accessors[keyword].GetType()}");
                Assert.AreEqual(containAutoViewObj, viewLayouter.ContainAutoViewObjectCreator(keyword), $"Don't equal to Contain autoLayoutCreator... keyword={keyword}");
            }
        }
    }
}
