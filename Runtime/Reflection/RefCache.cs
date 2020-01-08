using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public class RefCache
    {
        static readonly BindingFlags commonBindingFlags =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;

		readonly System.Type _targetType;
        public System.Type TargetType { get => _targetType; }

        Dictionary<string, FieldInfo> _fieldCaches = new Dictionary<string, FieldInfo>();
        Dictionary<string, PropertyInfo> _propCaches = new Dictionary<string, PropertyInfo>();
        Dictionary<MethodInfoKey, MethodInfo> _methodCaches = new Dictionary<MethodInfoKey, MethodInfo>();
        Dictionary<string, ConstructorInfo> _ctorCaches = new Dictionary<string, ConstructorInfo>();

        public RefCache(System.Type target)
		{
			Assert.IsNotNull(target);
			_targetType = target;
		}

        #region Field
        public object GetField(object instance, string name)
        {
            var info = FindField(name, instance != null, true);
            return info.GetValue(instance);
        }
        public void SetField(object instance, string name, object value)
        {
            var info = FindField(name, instance != null, false);
            Assert.IsFalse(info.IsInitOnly || info.IsLiteral, $"'{name}' field is not set...");
            info.SetValue(instance, value);
        }
        public bool HasField(string name)
        {
            return _fieldCaches.ContainsKey(name);
        }
        public FieldInfo FindField(string name, bool isInstance, bool isReadonly)
        {
            if(HasField(name))
            {
                return _fieldCaches[name];
            }

            var bindFlags = commonBindingFlags;
            bindFlags |= isReadonly ? BindingFlags.SetField : BindingFlags.GetField;
            bindFlags |= isInstance ? BindingFlags.Instance : BindingFlags.Static;
            var info = TargetType.GetField(name, bindFlags);
            Assert.IsNotNull(info, $"Don't exist '{name}' {(isInstance ? "Instance" : "Static")} field...");
            _fieldCaches.Add(name, info);
            return info;
        }
        #endregion
        #region Property
        public object GetProp(object instance, string name)
        {
            var info = FindProp(name, instance != null, true);
            Assert.IsTrue(info.CanRead, $"'{name}' prop is not get...");
            return info.GetValue(instance);
        }
        public void SetProp(object instance, string name, object value)
        {
            var info = FindProp(name, instance != null, false);
            Assert.IsTrue(info.CanWrite, $"'{name}' prop is not set...");
            info.SetValue(instance, value);
        }

        public bool HasProp(string name)
        {
            return _propCaches.ContainsKey(name);
        }

        public PropertyInfo FindProp(string name, bool isInstance, bool isGetter)
        {
            if (HasProp(name))
            {
                return _propCaches[name];
            }

            var bindFlags = commonBindingFlags;
            bindFlags |= isGetter ? BindingFlags.GetProperty : BindingFlags.SetProperty;
            bindFlags |= isInstance ? BindingFlags.Instance : BindingFlags.Static;
            var info = TargetType.GetProperty(name, bindFlags);
            Assert.IsNotNull(info, $"Don't exist '{name}' prop... {(isInstance ? "Instance" : "Static")}, {(isGetter ? "getter" : "setter")}");
            _propCaches.Add(name, info);
            return info;
        }
        #endregion
        #region Method
        public object Invoke(object instance, string name, params object[] args)
        {
            var info = FindAndCacheMethod(name, instance != null, args.Select(_a => _a.GetType()));
            return info.Invoke(instance, args);
        }

        public bool HasMethod(string name, params System.Type[] argumentTypes)
        {
            return GetMethodInfo(name, argumentTypes.AsEnumerable()) != null;
        }
        public MethodInfo GetMethodInfo(string name)
        {
            var key = _methodCaches.Keys.FirstOrDefault(_k => _k.Name == name);
            return key != null ? _methodCaches[key] : null;
        }
        public MethodInfo GetMethodInfo(string name, IEnumerable<System.Type> argumentTypes)
        {
            var key = _methodCaches.Keys.FirstOrDefault(_k => _k.Name == name
                && _k.ArgumentTypes.Zip(argumentTypes, (_t, _o) => (t: _t, o: _o))
                    .All(pair => pair.t == pair.o));
            return key != null ? _methodCaches[key] : null;
        }

        public MethodInfo FindAndCacheMethod(string name, bool isInstance, params System.Type[] argumentTypes)
        {
            return FindAndCacheMethod(name, isInstance, argumentTypes.AsEnumerable());
        }

        public MethodInfo FindAndCacheMethod(string name, bool isInstance, IEnumerable<System.Type> argumentTypes)
        {
            var info = GetMethodInfo(name, argumentTypes);
            if(info != null)
            {
                return info;
            }

            var bindFlags = BindingFlags.InvokeMethod | commonBindingFlags;
            bindFlags |= isInstance ? BindingFlags.Instance : BindingFlags.Static;
            info = TargetType.GetMethods(bindFlags)
                .Where(_i => _i.Name == name)
                .FirstOrDefault(_i => IsSameArgumentType(_i, argumentTypes));
            Assert.IsNotNull(info, $"Don't exist '{name}' {(isInstance ? "Instance" : "Static")} method with {ToStr(argumentTypes)}...");
            _methodCaches.Add(new MethodInfoKey(name, argumentTypes.ToArray()), info);
            return info;
        }
        #endregion
        #region Constructor
        public object CreateInstance(params object[] args)
        {
            var constructor = TargetType.GetConstructors().First(_ctor => {
                return _ctor.GetParameters()
                    .Zip(args, (_p, _a) => (param: _p, arg: _a))
                    .All(_pair => _pair.param.ParameterType == _pair.arg.GetType());
            });
            return constructor.Invoke(args);
        }

        public object CreateInstanceWithCache(string name, params object[] args)
        {
            var info = FindAndCacheConstructor(name, true, args != null ? args.Select(_a => _a.GetType()):null);
            return info.Invoke(args);
        }

        public bool HasConstructorInfo(string name)
        {
            return GetConstructorInfo(name) != null;
        }
        public ConstructorInfo GetConstructorInfo(string name)
        {
            var key = _ctorCaches.Keys.FirstOrDefault(_k => _k == name);
            return key != null ? _ctorCaches[key] : null;
        }

        public ConstructorInfo FindAndCacheConstructor(string name, bool isInstance, params System.Type[] argumentTypes)
        {
            return FindAndCacheConstructor(name, isInstance, argumentTypes?.AsEnumerable() ?? null);
        }

        public ConstructorInfo FindAndCacheConstructor(string name, bool isInstance, IEnumerable<System.Type> argumentTypes)
        {
            var info = GetConstructorInfo(name);
            if (info != null)
            {
                return info;
            }

            var bindFlags = BindingFlags.CreateInstance | commonBindingFlags;
            bindFlags |= isInstance ? BindingFlags.Instance : BindingFlags.Static;
            info = TargetType.GetConstructors(bindFlags)
                .FirstOrDefault(_i => IsSameArgumentType(_i, argumentTypes));
            Assert.IsNotNull(info, $"Don't exist constructor{ToStr(argumentTypes)}... key='{name}', {(isInstance ? "Instance" : "Static")}");
            _ctorCaches.Add(name, info);
            return info;
        }
        #endregion

        /// <summary>
        /// MethodBaseのExtensionsにしたい？
        /// </summary>
        /// <param name="info"></param>
        /// <param name="argumentTypes"></param>
        /// <returns></returns>
        bool IsSameArgumentType(MethodBase info, IEnumerable<System.Type> argumentTypes)
        {
            if (argumentTypes == null) return info.GetParameters().Count() <= 0;

            return info.GetParameters()
                    .Zip(argumentTypes, (_p, _a) => (param: _p, arg: _a))
                    .All(pair => pair.param.ParameterType == pair.arg);
        }

        string ToStr(IEnumerable<System.Type> types)
        {
            string str = "(";
            if (types != null && types.Count() > 0)
            {
                str += types.Select(_t => _t.FullName)
                    .Aggregate((_str, current) => _str + $"{(_str.Length<=0 ? "" : ", ")}{current}");
            }
            str += ")";
            return str;
        }

        /// <summary>
        /// 関数のオーバーロードに対応するために作成したクラス
        /// </summary>
        class MethodInfoKey : System.IEquatable<MethodInfoKey>
        {
            readonly string _name;
            readonly System.Type[] _argumentTypes;

            public string Name { get => _name; }
            public System.Type[] ArgumentTypes { get => _argumentTypes; }

            public MethodInfoKey(string name, System.Type[] argumentTypes)
            {
                _name = name;
                _argumentTypes = argumentTypes;
            }

            public bool Equals(MethodInfoKey other)
            {
                if (Name != other.Name) return false;
                return ArgumentTypes.Zip(other.ArgumentTypes, (_t, _o) => (t: _t, o: _o))
                    .All(pair => pair.t == pair.o);
            }

            public override bool Equals(object obj)
            {
                if (obj is MethodInfoKey)
                {
                    return Equals(obj as MethodInfoKey);
                }
                return false;
            }
            public override int GetHashCode()
            {
                return Name.GetHashCode() ^ (ArgumentTypes.GetHashCode() + 100);
            }
        }
    }
}
