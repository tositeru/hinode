using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public static class GameObjectExtensions
    {
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

        public static GameObject Create(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            return obj;
        }

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
