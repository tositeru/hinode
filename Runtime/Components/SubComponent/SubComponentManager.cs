using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// ISubComponentを管理するクラス
    ///
    /// <seealso cref="ISubComponent{T}"/>
    /// </summary>
    public class SubComponentManager<T>
        where T : MonoBehaviour
    {
        public T RootComponent { get; }

        HashSet<ISubComponent<T>> _subComponents = new HashSet<ISubComponent<T>>();

        public IReadOnlyCollection<ISubComponent<T>> SubComponents { get => _subComponents; }
        public int SubComponentCount { get => _subComponents.Count; }

        public SubComponentManager(T rootComponent)
        {
            RootComponent = rootComponent;

            if(RootComponent is ISubComponent<T>)
            {
                var rootCom = RootComponent as ISubComponent<T>;
                rootCom.RootComponent = null;
                _subComponents.Add(rootCom);
            }

            var rootType = typeof(T);
            var fields = rootType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(_f => _f.FieldType.ContainsInterface<ISubComponent<T>>());
            foreach(var subComponentInfo in fields)
            {
                var inst = subComponentInfo.GetValue(rootComponent) as ISubComponent<T>;
                if (inst == null) continue;

                inst.RootComponent = rootComponent;
                _subComponents.Add(inst);
            }
        }

        public void Init()
        {
            foreach (var com in _subComponents)
            {
                SubComponentAttributeManager.RunInitMethods(com);
                com.Init();
            }
        }

        public void Destroy()
        {
            foreach(var com in _subComponents)
            {
                SubComponentAttributeManager.RunDestroyMethods(com);
                com.Destroy();
                com.RootComponent = null;
            }
        }

        public void UpdateUI()
        {
            foreach (var com in _subComponents)
            {
                SubComponentAttributeManager.RunUpdateUIMethods(com);
                com.UpdateUI();
            }
        }

        /// <summary>
        /// Hinode.MethodLabelAttributeが指定されたISubComponentのメソッドを呼び出す関数
        ///
        /// void XXX()なメソッドのみ対応しています。
        ///
        /// 任意の戻り値や引数を持つメソッドを呼び出したい時は以下の例を参考にしてください。
        /// <code>
        /// //call -> int XXX(int, int);
        /// var manager = new SubComponentManager<T>();
        /// var returnType = typeof(int);
        /// var isStatic = false;
        /// var label = "Label";
        /// var methods = manager.SubComponents.SelectMany(_s =>
        ///     _s.GetType().GetMethods(BindingFlags.Public | bindingFlags)
        ///         .Where(_m => {
        ///             var attr = _m.GetCustomAttributes<LabelsAttribute>()
        ///                 .FirstOrDefault(_a => _a.GetType().Equals(typeof(LabelsAttribute)));
        ///             if (attr == null) return false;
        ///             // Filtering With 'label'
        ///             return attr.DoMatch(Labels.MatchOp.Partial, label);
        ///         })
        ///         // below pass to Arguments And ReturnType!
        ///         .CallMethods(isStatic? null : _s, typeof(void), 100, 200));
        /// foreach(var returnValue in methods)
        /// {
        ///     //recive returnValue from Methods
        ///     Debug.Log($"returnValue => {returnValue}");
        /// }
        /// </code>
        /// <seealso cref="Hinode.MethodLabelAttribute"/>
        /// </summary>
        /// <param name="methodLabel"></param>
        public void CallSubComponentMethods(string methodLabel, bool isStatic = false)
        {
            var bindingFlags = isStatic ? BindingFlags.Static : BindingFlags.Instance;
            var callMethods = _subComponents.SelectMany(_s =>
                    _s.GetType().GetMethods(BindingFlags.Public | bindingFlags)
                        .Where(_m => {
                            var attr = _m.GetCustomAttributes<LabelsAttribute>().FirstOrDefault(_a => _a.GetType().Equals(typeof(LabelsAttribute)));
                            if (attr == null) return false;
                            return attr.DoMatch(Labels.MatchOp.Partial, methodLabel);
                        })
                        .CallMethods(isStatic ? null : _s, typeof(void))); 

            foreach(var returnValue in callMethods) { }
        }
    }
}
