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

        public int SubComponentCount { get => _subComponents.Count; }

        public SubComponentManager(T rootComponent)
        {
            RootComponent = rootComponent;

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
    }
}
