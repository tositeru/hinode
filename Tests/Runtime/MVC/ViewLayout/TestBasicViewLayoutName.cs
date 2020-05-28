using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.ViewLayout
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
            var keywords = new Dictionary<string, IViewLayoutAccessor>() {
                { BasicViewLayoutName.depth.ToString(), new DepthViewLayoutAccessor() },
                { BasicViewLayoutName.siblingOrder.ToString(), new SiblingOrderViewLayoutAccessor() }
            };
            foreach (var (keyword, accessor) in keywords.Select(_t => (_t.Key, _t.Value)))
            {
                Assert.IsTrue(viewLayouter.ContainsKeyword(keyword), $"Don't exist {keyword}...");
                Assert.AreSame(accessor.GetType(), viewLayouter.Accessors[keyword].GetType(), $"cur={accessor.GetType()}, got={viewLayouter.Accessors[keyword].GetType()}");
                Assert.IsFalse(viewLayouter.ContainAutoViewObjectCreator(keyword), $"Don't exist autoLayoutCreator... keyword={keyword}");
            }
            Assert.IsFalse(viewLayouter.ContainAutoViewObjectCreator(keywords.Keys));
        }
    }
}
