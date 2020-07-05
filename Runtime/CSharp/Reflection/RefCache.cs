using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// 
    /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache"/>
    /// </summary>
    public class RefCache
    {
        static readonly BindingFlags commonBindingFlags =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;

		readonly System.Type _targetType;
        public System.Type TargetType { get => _targetType; }

        Dictionary<string, FieldInfo> _fieldCaches = new Dictionary<string, FieldInfo>();
        Dictionary<string, PropertyInfo> _propCaches = new Dictionary<string, PropertyInfo>();
        Dictionary<string, HashSet<MethodInfo>> _methodCaches = new Dictionary<string, HashSet<MethodInfo>>();
        Dictionary<string, ConstructorInfo> _ctorCaches = new Dictionary<string, ConstructorInfo>();

        public RefCache(System.Type target)
		{
			Assert.IsNotNull(target);
			_targetType = target;
		}

        #region Field
        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.FieldMethodsAtInstancePassess()"/>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetField(object instance, string name)
        {
            var info = FindAndCacheField(name, instance != null);
            return info.GetValue(instance);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.FieldMethodsAtInstancePassess()"/>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetField(object instance, string name, object value)
        {
            var info = FindAndCacheField(name, instance != null);
            Assert.IsFalse(info.IsInitOnly || info.IsLiteral, $"'{name}' field is not set...");
            info.SetValue(instance, value);
        }
        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.FieldMethodsAtInstancePassess()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsCachedField(string name)
        {
            return _fieldCaches.ContainsKey(name);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.FieldMethodsAtInstancePassess()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isInstance"></param>
        /// <param name="isLiteral"></param>
        /// <returns></returns>
        public FieldInfo FindAndCacheField(string name, bool isInstance)
        {
            if(ContainsCachedField(name))
            {
                return _fieldCaches[name];
            }

            var bindFlags = commonBindingFlags;
            bindFlags |= isInstance ? BindingFlags.Instance : BindingFlags.Static;
            var info = TargetType.GetField(name, bindFlags);
            Assert.IsNotNull(info, $"Don't exist '{name}' {(isInstance ? "Instance" : "Static")} field...");
            
            _fieldCaches.Add(name, info);
            return info;
        }
        #endregion
        #region Property
        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.PropertyMethodsAtInstancePassess()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.SetterOnlyPropertyPasses()"/>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetProp(object instance, string name)
        {
            var info = FindAndCacheProp(name, instance != null);
            Assert.IsTrue(info.CanRead, $"'{name}' prop is not get...");
            return info.GetValue(instance);
        }
        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.PropertyMethodsAtInstancePassess()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.GetterOnlyPropertyPasses()"/>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetProp(object instance, string name, object value)
        {
            var info = FindAndCacheProp(name, instance != null);
            Assert.IsTrue(info.CanWrite, $"'{name}' prop is not set...");
            info.SetValue(instance, value);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.PropertyMethodsAtInstancePassess()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsCachedProp(string name)
        {
            return _propCaches.ContainsKey(name);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.PropertyMethodsAtInstancePassess()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.GetterOnlyPropertyPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isInstance"></param>
        /// <param name="isGetter"></param>
        /// <returns></returns>
        public PropertyInfo FindAndCacheProp(string name, bool isInstance)
        {
            if (ContainsCachedProp(name))
            {
                return _propCaches[name];
            }

            var bindFlags = commonBindingFlags;
            bindFlags |= isInstance ? BindingFlags.Instance : BindingFlags.Static;
            var info = TargetType.GetProperty(name, bindFlags);
            Assert.IsNotNull(info, $"Don't exist '{name}' prop... {(isInstance ? "Instance" : "Static")}");
            _propCaches.Add(name, info);
            return info;
        }
        #endregion
        #region Method
        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.MethodMethodsAtInstancePassess()"/>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object Invoke(object instance, string name, params object[] args)
        {
            var info = FindAndCacheMethod(name, instance != null, args.Select(_a => _a.GetType()));
            return info.Invoke(instance, args);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.MethodMethodsAtInstancePassess()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.OverloadMethodPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argumentTypes"></param>
        /// <returns></returns>
        public bool ContainsCachedMethod(string name, params System.Type[] argumentTypes)
        {
            return GetCachedMethodInfo(name, argumentTypes.AsEnumerable()) != null;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.MethodMethodsAtInstancePassess()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.OverloadMethodPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MethodInfo GetCachedMethodInfo(string name)
        {
            if (!_methodCaches.ContainsKey(name)) return null;
            return _methodCaches[name].FirstOrDefault();
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.MethodMethodsAtInstancePassess()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.OverloadMethodPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argumentTypes"></param>
        /// <returns></returns>
        public MethodInfo GetCachedMethodInfo(string name, params System.Type[] argumentTypes)
            => GetCachedMethodInfo(name, argumentTypes.AsEnumerable());

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.MethodMethodsAtInstancePassess()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.OverloadMethodPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argumentTypes"></param>
        /// <returns></returns>
        public MethodInfo GetCachedMethodInfo(string name, IEnumerable<System.Type> argumentTypes)
        {
            if (!_methodCaches.ContainsKey(name)) return null;

            return _methodCaches[name].FirstOrDefault(_i => IsSameArgumentType(_i, argumentTypes));
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.MethodMethodsAtInstancePassess()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.OverloadMethodPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isInstance"></param>
        /// <param name="argumentTypes"></param>
        /// <returns></returns>
        public MethodInfo FindAndCacheMethod(string name, bool isInstance, params System.Type[] argumentTypes)
        {
            return FindAndCacheMethod(name, isInstance, argumentTypes.AsEnumerable());
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.MethodMethodsAtInstancePassess()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.OverloadMethodPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isInstance"></param>
        /// <param name="argumentTypes"></param>
        /// <returns></returns>
        public MethodInfo FindAndCacheMethod(string name, bool isInstance, IEnumerable<System.Type> argumentTypes)
        {
            var info = GetCachedMethodInfo(name, argumentTypes);
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
            if(!_methodCaches.ContainsKey(name))
            {
                _methodCaches.Add(name, new HashSet<MethodInfo>());
            }
            if (!_methodCaches[name].Contains(info))
            {
                _methodCaches[name].Add(info);
            }
            return info;
        }
        #endregion
        #region Constructor
        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.CreateInstancePasses()"/>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object CreateInstance(params object[] args)
        {
            var constructor = TargetType.GetConstructors().First(_ctor => {
                return IsSameArgumentType(_ctor, args.Select(_a => _a.GetType()));
            });
            return constructor.Invoke(args);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.ConstructorMethodsPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object CreateInstanceWithCache(string name, params object[] args)
        {
            var info = FindAndCacheConstructor(name, true, args != null ? args.Select(_a => _a.GetType()):null);
            return info.Invoke(args);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.ConstructorMethodsPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsCachedConstructorInfo(string name)
        {
            return GetCachedConstructorInfo(name) != null;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.ConstructorMethodsPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ConstructorInfo GetCachedConstructorInfo(string name)
        {
            var key = _ctorCaches.Keys.FirstOrDefault(_k => _k == name);
            return key != null ? _ctorCaches[key] : null;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.ConstructorMethodsPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isInstance"></param>
        /// <param name="argumentTypes"></param>
        /// <returns></returns>
        public ConstructorInfo FindAndCacheConstructor(string name, bool isInstance, params System.Type[] argumentTypes)
        {
            return FindAndCacheConstructor(name, isInstance, argumentTypes?.AsEnumerable() ?? null);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.Reflection.TestRefCache.ConstructorMethodsPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isInstance"></param>
        /// <param name="argumentTypes"></param>
        /// <returns></returns>
        public ConstructorInfo FindAndCacheConstructor(string name, bool isInstance, IEnumerable<System.Type> argumentTypes)
        {
            var info = GetCachedConstructorInfo(name);
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
            var parameters = info.GetParameters();
            if (argumentTypes == null) return parameters.Count() <= 0;
            if (argumentTypes.Count() != parameters.Length) return false;
            return parameters
                    .Zip(argumentTypes, (_p, _a) => (param: _p, arg: _a))
                    .All(pair => pair.param.ParameterType.IsSameOrInheritedType(pair.arg));
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
    }
}
