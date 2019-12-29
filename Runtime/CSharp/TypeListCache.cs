using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// 現在読み込まれているAssemblyから指定したクラスとその派生クラスをリスト化するクラス
    /// </summary>
    /// <typeparam name="TBase">リスト化する型の基底クラス。</typeparam>
    public class TypeListCache<TBase>
    {
        public TypeListCache(string initialAssemblyName, string initialTypeName)
        {
            AssemblyIndex = System.Array.FindIndex(AssemblyNameList, _e => _e == initialAssemblyName);
            if (AssemblyIndex == -1) AssemblyIndex = 0;

            TypeIndex = TypeList.Zip(Enumerable.Range(0, TypeList.Count()), (_t, _i) => (type: _t, index: _i))
                .Where(_p => _p.type.FullName == initialTypeName)
                .Select(_p => _p.index)
                .FirstOrDefault();
        }

        int _assemblyIndex;
        public int AssemblyIndex
        {
            get => _assemblyIndex;
            set
            {
                Assert.IsTrue(0 <= value && value < AssemblyList.Count());
                _assemblyIndex = value;
                TypeList = GetTypeList(_assemblyIndex);
                TypeIndex = 0;
            }
        }
        int _typeIndex;
        public int TypeIndex
        {
            get => _typeIndex;
            set
            {
                _typeIndex = Mathf.Clamp(value, 0, TypeList.Count()-1);
            }
        }

        IEnumerable<System.Reflection.Assembly> _assemblyList;
        public IEnumerable<System.Reflection.Assembly> AssemblyList
        {
            get
            {
                return _assemblyList != null
                    ? _assemblyList
                    : _assemblyList = System.AppDomain.CurrentDomain.GetAssemblies()
                        .Where(_asm => _asm.GetExportedTypes().Any(_t => _t.IsSubclassOf(typeof(TBase)) || _t.Equals(typeof(TBase))));
            }
        }

        string[] _assemblyNameList;
        public string[] AssemblyNameList
        {
            get
            {
                return _assemblyNameList != null
                    ? _assemblyNameList
                    : _assemblyNameList = AssemblyList.Select(_asm => _asm.GetName().Name).ToArray();
            }
        }
        IEnumerable<System.Type> _typeList;
        public IEnumerable<System.Type> TypeList
        {
            get
            {
                if (_typeList != null) return _typeList;
                TypeList = GetTypeList(AssemblyIndex);
                return _typeList;
            }
            set
            {
                _typeList = value;
                TypeNameList = _typeList.Select(_t => _t.FullName).ToArray();
            }
        }

        IEnumerable<System.Type> GetTypeList(int assemblyIndex)
        {
            return AssemblyList.ElementAt(assemblyIndex).GetTypes()
                .Where(_t => _t.IsSubclassOf(typeof(TBase)));
        }
        public string[] TypeNameList
        {
            get; private set;
        }

        public System.Type CurrentType
        {
            get => TypeList.ElementAt(TypeIndex);
            set
            {
                var (type, index) = TypeList.Zip(Enumerable.Range(0, TypeList.Count()), (_t, _i) => (type: _t, index: _i))
                    .FirstOrDefault(_p => _p.type.Equals(value));
                if (type == null)
                {
                    var defaultType = TypeList.ElementAtOrDefault(0);
                    Debug.LogWarning($"Don't Found '{value.FullName}' Type in Type List... Use '{(defaultType != null ? defaultType.FullName : "(null)")}'(index=0)");
                    index = 0;
                }
                TypeIndex = index;
            }
        }
    }
}