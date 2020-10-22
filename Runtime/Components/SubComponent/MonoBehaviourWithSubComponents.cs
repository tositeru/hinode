using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// SubComponentManagerをラップしたMonoBehaviour
    ///
    /// 継承先のクラスが持つSubComponentの情報を閲覧することができるEditorWindowを提供してます。
    /// Hinode > Tools > SubComponent Summary からそのWindowを開けます。
    /// 
    /// <see cref="ISubComponent{T}"/>
    /// <see cref="SubComponentManager{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoBehaviourWithSubComponents<T> : MonoBehaviour
        , ISubComponent<T>
        where T : MonoBehaviour
    {
        SubComponentManager<T> _subComponents;

        protected SubComponentManager<T> SubComponents { get => _subComponents; }
        public T RootComponent { get; set; }

        ControllerLabelFilter[] _controllerLabelFilters;

        /// <summary>
        /// インスタンスが所属しているSceneまたはPrefabに存在するGameObjectにアタッチされているLabelObjectに対してのフィルター
        /// 
        /// </summary>
        public ControllerLabelFilter[] ControllerLabelFilters
        {
            get => (_controllerLabelFilters != null) ? _controllerLabelFilters : _controllerLabelFilters = CreateControllerLabelFilters();
        }

        protected virtual ControllerLabelFilter[] CreateControllerLabelFilters() { return new ControllerLabelFilter[] { }; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool CallControllerLabelFilterCallback(object obj)
        {
            var labelObj = LabelObject.GetLabelObject(obj);
            if (labelObj != null)
            {
                var (filter, com, doMatch) = ControllerLabelFilters.Select(_f => {
                    var _doMatch = _f.DoMatch(labelObj, out var _com);
                    return (filter: _f, com: _com, doMatch: _doMatch);
                })
                    .FirstOrDefault(_t => _t.doMatch);
                if (doMatch)
                {
                    filter.Callback(labelObj, com);
                    return true;
                }
            }
            return false;
        }


        protected virtual void Awake()
        {
            Assert.IsTrue(this is T, $"{this.GetType()} is not {typeof(T)}...");

            _subComponents = new SubComponentManager<T>(this as T);
            _subComponents.Init();
        }

        protected virtual void Start()
        {
            _subComponents.UpdateUI();
        }

        protected virtual void OnDestroy()
        {
            _subComponents.Destroy();
        }

        #region ISubComponent
        public virtual void Init() {}
        public virtual void Destroy() {}
        public virtual void UpdateUI() {}
        #endregion
    }

    /// <summary>
    /// MVCのContrrolerに関係するメソッドとLabelObjectを関連づけるためのクラス
    /// </summary>
    public class ControllerLabelFilter : LabelObject.LabelFilter
    {
        public HashSet<string> MethodLabels { get; set; } = new HashSet<string>();
        public System.Action<LabelObject, Component> Callback { get; set; }

        public ControllerLabelFilter(System.Type componentType, params string[] labels)
            : base(componentType, labels)
        { }

        public ControllerLabelFilter SetMethodLabels(params string[] methodLabels)
        {
            foreach (var l in methodLabels.Where(_l => !MethodLabels.Contains(_l)))
            {
                MethodLabels.Add(l);
            }
            return this;
        }

        public ControllerLabelFilter SetCallback(System.Action<LabelObject, Component> action)
        {
            Callback = action;
            return this;
        }

        public static new ControllerLabelFilter Create(System.Type comType, params string[] labels)
        {
            return new ControllerLabelFilter(comType, labels);
        }

        public static new ControllerLabelFilter Create<TCom>(params string[] labels)
            where TCom : Component
        {
            return new ControllerLabelFilter(typeof(TCom), labels);
        }
    }
}
