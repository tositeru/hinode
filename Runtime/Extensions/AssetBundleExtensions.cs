using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public static partial class AssetBundleExtensions
    {
        public static T LoadGameObjectComponent<T>(this AssetBundle target, string name)
            where T : Component
            => target.LoadGameObjectComponent(name, typeof(T)) as T;
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