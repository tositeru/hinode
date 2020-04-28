using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    [CreateAssetMenu(menuName = "Hinode/MVC/Unity ViewInstanceCreator Settings")]
    public class UnityViewInstanceCreatorSettings : ScriptableObject
    {
        public enum InfoType
        {
            Asset,
            Resources,
            AssetBundle,
        }
        [SerializeField] List<InstanceInfo> _infos = new List<InstanceInfo>();

        Dictionary<string, AssetBundle> _entriedAssetBundles = new Dictionary<string, AssetBundle>();

        public IEnumerable<InstanceInfo> Infos { get => _infos; }
        public IReadOnlyDictionary<string, AssetBundle> EntriedAssetBundles { get => _entriedAssetBundles; }

        public UnityViewInstanceCreatorSettings AddInfo(InstanceInfo info)
        {
            _infos.Add(info);
            return this;
        }
        public UnityViewInstanceCreatorSettings RemoveInfo(InstanceInfo info)
        {
            _infos.Remove(info);
            return this;
        }

        public UnityViewInstanceCreator SetTo(UnityViewInstanceCreator target)
        {
            foreach(var item in Infos)
            {
                item.SetTo(this, target);
            }
            return target;
        }

        public AssetBundle GetAssetBundle(string key)
            => _entriedAssetBundles[key];

        public void EntryAssetBundle(AssetBundle assetBundle)
        {
            Assert.IsFalse(_entriedAssetBundles.ContainsKey(assetBundle.name), $"Already Entry AssetBundle({assetBundle.name})...");

            _entriedAssetBundles.Add(assetBundle.name, assetBundle);
        }

        public void RemoveAssetBundle(AssetBundle assetBundle)
        {
            Assert.IsTrue(_entriedAssetBundles.ContainsKey(assetBundle.name), $"Failed to Remove AssetBundle({assetBundle.name}) because not contain...");
            _entriedAssetBundles.Remove(assetBundle.name);
        }

        public void ClearAssetBundles()
        {
            _entriedAssetBundles.Clear();
        }

        [System.Serializable]
        public class InstanceInfo
        {
            [SerializeField] InfoType _type;
            [SerializeField] string _instanceType;
            [SerializeField] string _paramBinderType;
            [SerializeField] string _instanceKey;
            [SerializeField] string _binderKey;
            [SerializeField] string _assetPath;
            [SerializeField] string _assetBundleName;
            [SerializeField] Component _assetReference;

            public InfoType Type { get => _type; set => _type = value; }
            public string InstanceTypeFullName { get => _instanceType; set => _instanceType = value; }
            public string ParamBinderTypeFullName { get => _paramBinderType; set => _paramBinderType = value; }
            public string InstanceKey
            {
                get => (_instanceKey == null || _instanceKey == "")
                    ? InstanceTypeFullName
                    : _instanceKey;
                set => _instanceKey = value;
            }
            public string BinderKey
            {
                get => (_binderKey == null || _binderKey == "")
                    ? InstanceTypeFullName
                    : _binderKey;
                set => _binderKey = value;
            }
            public string AssetPath { get => _assetPath; set => _assetPath = value; }
            public string AssetBundleName { get => _assetBundleName; set => _assetBundleName = value; }
            public Component AssetReference { get => _assetReference; set => _assetReference = value; }

            public System.Type InstanceType
            {
                get
                {
                    var viewObjectTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(_asm => _asm.GetExportedTypes())
                        .Where(_type => _type.DoHasInterface<IViewObject>());
                    var useViewObjectType = viewObjectTypes.FirstOrDefault(_t => _t.FullName == InstanceTypeFullName);
                    Assert.IsNotNull(useViewObjectType, $"Type({useViewObjectType}) don't found...");
                    return useViewObjectType;
                }
            }

            public System.Type ParamBinderType
            {
                get
                {
                    var paramBinderTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(_asm => _asm.GetExportedTypes())
                        .Where(_type => _type.DoHasInterface<IModelViewParamBinder>());
                    var useParamBinderType = paramBinderTypes.FirstOrDefault(_t => _t.FullName == ParamBinderTypeFullName);
                    Assert.IsNotNull(useParamBinderType, $"Type({ParamBinderTypeFullName}) don't found...");
                    return useParamBinderType;
                }
            }

            public static InstanceInfo CreateAsset<TParamBinder>(IViewObject assetResource)
                where TParamBinder : IModelViewParamBinder
                => CreateAsset(assetResource, typeof(TParamBinder));

            public static InstanceInfo CreateAsset(IViewObject assetResource, System.Type paramBinderType)
            {
                Assert.IsTrue(assetResource as Component, $"{assetResource.GetType()} is not Component Type...");
                Assert.IsTrue(paramBinderType.DoHasInterface<IModelViewParamBinder>(), $"{paramBinderType.GetType()} is not IModelViewParamBinder interface...");

                return new InstanceInfo()
                {
                    Type = InfoType.Asset,
                    AssetReference = assetResource as Component,
                    InstanceTypeFullName = assetResource.GetType().FullName,
                    ParamBinderTypeFullName = paramBinderType.FullName,
                };
            }

            public static InstanceInfo CreateResource<TViewObj, TParamBinder>(string resourcePath)
                where TViewObj : Component, IViewObject
                where TParamBinder : IModelViewParamBinder
                => CreateResource(resourcePath, typeof(TViewObj), typeof(TParamBinder));

            public static InstanceInfo CreateResource(string resourcePath, System.Type viewInstanceType, System.Type paramBinderType)
            {
                Assert.IsTrue(viewInstanceType.DoHasInterface<IViewObject>()
                    && viewInstanceType.IsSubclassOf(typeof(Component)),
                    $"{viewInstanceType} is not IViewObject and Component...");
                Assert.IsTrue(paramBinderType.DoHasInterface<IModelViewParamBinder>(), $"{paramBinderType.GetType()} is not IModelViewParamBinder interface...");

                return new InstanceInfo()
                {
                    Type = InfoType.Resources,
                    AssetPath = resourcePath,
                    InstanceTypeFullName = viewInstanceType.FullName,
                    ParamBinderTypeFullName = paramBinderType.FullName,
                };
            }

            public static InstanceInfo CreateAssetBundle<TViewObj, TParamBinder>(string bundleName, string assetPath)
                where TViewObj : Component, IViewObject
                where TParamBinder : IModelViewParamBinder
                => CreateAssetBundle(bundleName, assetPath, typeof(TViewObj), typeof(TParamBinder));

            public static InstanceInfo CreateAssetBundle(string bundleName, string assetPath, System.Type viewInstanceType, System.Type paramBinderType)
            {
                Assert.IsTrue(viewInstanceType.DoHasInterface<IViewObject>()
                    && viewInstanceType.IsSubclassOf(typeof(Component)),
                    $"{viewInstanceType.FullName} is not IViewObject and Component...");
                Assert.IsTrue(paramBinderType.DoHasInterface<IModelViewParamBinder>(), $"{paramBinderType.GetType()} is not IModelViewParamBinder interface...");

                return new InstanceInfo()
                {
                    Type = InfoType.AssetBundle,
                    AssetBundleName = bundleName,
                    AssetPath = assetPath,
                    InstanceTypeFullName = viewInstanceType.FullName,
                    ParamBinderTypeFullName = paramBinderType.FullName,
                };
            }

            public InstanceInfo SetInstanceKey(string key)
            {
                InstanceKey = key;
                return this;
            }
            public InstanceInfo SetBinderKey(string key)
            {
                BinderKey = key;
                return this;
            }

            static readonly System.Type[] _emptyArgs = new System.Type[] { };
            public void SetTo(UnityViewInstanceCreatorSettings settings, UnityViewInstanceCreator target)
            {
                var paramBinderType = ParamBinderType;
                var cstor = paramBinderType.GetConstructor(_emptyArgs);
                var paramBinder = cstor.Invoke(null) as IModelViewParamBinder;

                switch (Type)
                {
                    case InfoType.Asset:

                        target.AddPrefab(InstanceKey, InstanceType, AssetReference, BinderKey, paramBinder);
                        break;
                    case InfoType.Resources:
                        {
                            var instanceType = InstanceType;
                            var prefab = Resources.Load(AssetPath, instanceType) as Component;

                            target.AddPrefab(InstanceKey, instanceType, prefab, BinderKey, paramBinder);
                            break;
                        }
                    case InfoType.AssetBundle:
                        {
                            Assert.IsTrue(settings.EntriedAssetBundles.ContainsKey(AssetBundleName));
                            var bundle = settings.EntriedAssetBundles[AssetBundleName];
                            Assert.IsNotNull(bundle);
                            var instanceType = InstanceType;
                            var prefab = bundle.LoadGameObjectComponent(AssetPath, instanceType);
                            Assert.IsNotNull(prefab, $"Failed to load Asset... assetPath={AssetPath}");

                            target.AddPrefab(InstanceKey, instanceType, prefab, BinderKey, paramBinder);
                            break;
                        }
                    default:
                        throw new System.NotImplementedException();
                }
            }
        }
    }

    public static partial class UnityViewInstanceCreatorExtension
    {
        public static UnityViewInstanceCreator AddFromSettings(this UnityViewInstanceCreator target, UnityViewInstanceCreatorSettings settings)
        {
            settings.SetTo(target);
            return target;
        }
    }
}
