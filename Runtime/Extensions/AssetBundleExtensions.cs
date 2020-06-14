using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.Extensions.TestAssetBundleExtensions"/>
    /// </summary>
    public static partial class AssetBundleExtensions
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestAssetBundleExtensions.LoadGameObjectComponentPasses()"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T LoadGameObjectComponent<T>(this AssetBundle target, string name)
            where T : Component
            => target.LoadGameObjectComponent(name, typeof(T)) as T;

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestAssetBundleExtensions.LoadGameObjectComponentPasses()"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Component LoadGameObjectComponent(this AssetBundle target, string name, System.Type type)
        {
            var obj = target.LoadAsset<GameObject>(name);
            Assert.IsNotNull(obj, $"Failed to load GameObject... name=>{name}");
            var com = obj.GetComponent(type);
            Assert.IsNotNull(com, $"Failed to GetComponent from GameObject({name})... ComponentType={type}");
            return com;
        }
    }
}