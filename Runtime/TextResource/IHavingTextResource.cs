using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public interface IHavingTextResource
    {
        string HavingTextResourceKey { get; set; }
        object[] GetTextResourceParams();
    }

    public static class IHavingTextResourceExtensions
    {
        public static bool HasTextResourceKey(this IHavingTextResource target)
            => target.HavingTextResourceKey != null && target.HavingTextResourceKey != "";
    }

    public static partial class TextResourcesExtensions
    {
        public static string Get(this TextResources resource, IHavingTextResource havingTextResource)
        {
            Assert.IsTrue(havingTextResource.HasTextResourceKey());
            return resource.Get(havingTextResource.HavingTextResourceKey, havingTextResource.GetTextResourceParams());
        }

        public static bool Contains(this TextResources resource, IHavingTextResource havingTextResource)
            => resource.Contains(havingTextResource.HavingTextResourceKey);
    }
}