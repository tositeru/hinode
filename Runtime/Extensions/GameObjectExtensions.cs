using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.Extensions.TestGameObjectExtensions"/>
    /// </summary>
    public static class GameObjectExtensions
    {
        public static T GetComponent<T>(object target)
            where T : Component
            => GetComponent(target, typeof(T)) as T;

        public static Component GetComponent(object target, System.Type comType)
        {
            if (target is GameObject) return (target as GameObject).GetComponent(comType);
            if (target is Transform) return (target as Transform).GetComponent(comType);
            if (target is Component) return (target as Component).GetComponent(comType);
            return null;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.GetOrAddComponentPasses()"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject target) where T : Component
        {
            if(target.TryGetComponent<T>(out var com))
            {
                return com;
            }
            else
            {
                return target.AddComponent<T>();
            }
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.GetOrAddComponentPasses()"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="componentType"></param>
        /// <returns></returns>
        public static Component GetOrAddComponent(this GameObject target, System.Type componentType)
        {
            if (target.TryGetComponent(componentType, out var com))
            {
                return com;
            }
            else
            {
                return target.AddComponent(componentType);
            }
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.CreateGameObjectPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static GameObject Create(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            return obj;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.CreateGameObjectPasses()"/>
        /// <seealso cref="Hinode.Tests.Extensions.CreateGameObjectCallbackPasses()"/>
        /// </summary>
        /// <param name="rootParam"></param>
        /// <param name="outAdditionOrderList"></param>
        /// <returns></returns>
        public static GameObject Create(CreateGameObjectParam rootParam, List<GameObject> outAdditionOrderList)
        {
            var obj = new GameObject(rootParam.name);
            obj.transform.SetParent(rootParam.parent);
            outAdditionOrderList?.Add(obj);
            rootParam.CreateChildren(obj, outAdditionOrderList);
            rootParam.onCreated?.Invoke(obj);
            return obj;
        }
    }

    /// <summary>
    /// <seealso cref="GameObjectExtensions.Create(CreateGameObjectParam, List{GameObject})"/>
    /// </summary>
    public class CreateGameObjectParam
    {
        public string name;
        public Transform parent;
        public CreateGameObjectParam[] children;
        public System.Action<GameObject> onCreated;

        public CreateGameObjectParam(string name, Transform parent, System.Action<GameObject> onCreated, params CreateGameObjectParam[] children)
        {
            this.name = name;
            this.parent = parent;
            this.onCreated = onCreated;
            this.children = children;
        }

        public void CreateChildren(GameObject instance, List<GameObject> outAdditionOrderList)
        {
            if (children == null) return;
            foreach(var childParam in children)
            {
                childParam.parent = instance.transform;
                GameObjectExtensions.Create(childParam, outAdditionOrderList);
            }
        }

        public static implicit operator CreateGameObjectParam(string name)
            => new CreateGameObjectParam(name, null, null);
        public static implicit operator CreateGameObjectParam((string name, System.Action<GameObject> onCreated) p)
            => new CreateGameObjectParam(p.name, null, p.onCreated, null);
        public static implicit operator CreateGameObjectParam((string name, CreateGameObjectParam[] children) p)
            => new CreateGameObjectParam(p.name, null, null, p.children);
        public static implicit operator CreateGameObjectParam((string name, Transform parent, CreateGameObjectParam[] children) p)
            => new CreateGameObjectParam(p.name, p.parent, null, p.children);
        public static implicit operator CreateGameObjectParam((string name, System.Action<GameObject> onCreated, CreateGameObjectParam[] children) p)
            => new CreateGameObjectParam(p.name, null, p.onCreated, p.children);
        public static implicit operator CreateGameObjectParam((string name, Transform parent, System.Action<GameObject> onCreated, CreateGameObjectParam[] children) p)
            => new CreateGameObjectParam(p.name, p.parent, p.onCreated, p.children);

    }
}
