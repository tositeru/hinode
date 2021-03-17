using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// OnJoint2D()用のIBinderPredicate
    ///
    /// objに自動的にPhysic2DOnJointBreakCallbacksをアタッチします。
    /// <seealso cref="Physic2DOnJointBreakCallbacks"/>
    /// </summary>
    public class OnJointBreak2DBinder : BindCallbackAttribute.IBinderPredicate
    {
        protected string GetCallbackName() => "OnJointBreak";

        public bool EnableBind(MethodInfo methodInfo, object obj)
        {
            if (obj is GameObject)
            {
                if (null == (obj as GameObject).GetComponent<Joint2D>()) return false;
            }
            else if (!(obj is Joint2D)) return false;
            var args = methodInfo.GetParameters();
            return args.Length == 1
                && typeof(Joint2D).Equals(args[0].ParameterType);
        }

        public bool AddCallbacks(object target, MethodInfo methodInfo, object obj)
        {
            if (!EnableBind(methodInfo, obj)) return false;

            var joint2D = GameObjectExtensions.GetComponent<Joint2D>(obj);
            var onJointBreak = joint2D.gameObject.GetOrAddComponent<Physic2DOnJointBreakCallbacks>();

            return BindCallbackAttribute.BindWithTypeAndCallbackName(target, methodInfo, onJointBreak, typeof(Physic2DOnJointBreakCallbacks), GetCallbackName());
        }

        public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj)
        {
            if (!EnableBind(methodInfo, obj)) return false;

            var joint2D = GameObjectExtensions.GetComponent<Joint2D>(obj);
            var onJointBreak = joint2D.gameObject.GetOrAddComponent<Physic2DOnJointBreakCallbacks>();

            return BindCallbackAttribute.UnbindWithTypeAndCallbackName(target, methodInfo, onJointBreak, typeof(Physic2DOnJointBreakCallbacks), GetCallbackName());
        }
    }

    #region OnCollisionXXX2D
    public abstract class IOnCollision2DBinder<T> : BindCallbackAttribute.IBinderPredicate
        where T : MonoBehaviour
    {
        protected abstract string GetCallbackName();

        public bool EnableBind(MethodInfo methodInfo, object obj)
        {
            if (obj is GameObject)
            {
                if (null == (obj as GameObject).GetComponent<Collider2D>()) return false;
            }
            else if (!(obj is Collider2D)) return false;
            var args = methodInfo.GetParameters();
            return args.Length == 1
                && typeof(Collision2D).Equals(args[0].ParameterType);
        }

        public bool AddCallbacks(object target, MethodInfo methodInfo, object obj)
        {
            if (!EnableBind(methodInfo, obj)) return false;

            var collider2D = GameObjectExtensions.GetComponent<Collider2D>(obj);
            var onCollision = collider2D.gameObject.GetOrAddComponent<T>();

            return BindCallbackAttribute.BindWithTypeAndCallbackName(target, methodInfo, onCollision, typeof(T), GetCallbackName());
        }

        public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj)
        {
            if (!EnableBind(methodInfo, obj)) return false;

            var collider2D = GameObjectExtensions.GetComponent<Collider2D>(obj);
            var onCollision = collider2D.gameObject.GetOrAddComponent<T>();

            return BindCallbackAttribute.UnbindWithTypeAndCallbackName(target, methodInfo, onCollision, typeof(T), GetCallbackName());
        }
    }

    /// <summary>
    /// OnCollisionEnter2D()用のIBinderPredicate
    ///
    /// objに自動的にPhysic2DOnCollisionEnterCallbacksをアタッチします。
    /// <seealso cref="Physic2DOnCollisionEnterCallbacks"/>
    /// </summary>
    public class OnCollisionEnter2DBinder : IOnCollision2DBinder<Physic2DOnCollisionEnterCallbacks>
    {
        protected override string GetCallbackName() => "OnCollisionEnter";
    }

    /// <summary>
    /// OnCollisionStay2D()用のIBinderPredicate
    ///
    /// objに自動的にPhysic2DOnCollisionStayCallbacksをアタッチします。
    /// <seealso cref="Physic2DOnCollisionStayCallbacks"/>
    /// </summary>
    public class OnCollisionStay2DBinder : IOnCollision2DBinder<Physic2DOnCollisionStayCallbacks>
    {
        protected override string GetCallbackName() => "OnCollisionStay";
    }

    /// <summary>
    /// OnCollisionExit2D()用のIBinderPredicate
    ///
    /// objに自動的にPhysic2DOnCollisionExitCallbacksをアタッチします。
    /// <seealso cref="Physic2DOnCollisionExitCallbacks"/>
    /// </summary>
    public class OnCollisionExit2DBinder : IOnCollision2DBinder<Physic2DOnCollisionExitCallbacks>
    {
        protected override string GetCallbackName() => "OnCollisionExit";
    }

    #endregion

    #region OnTriggerXXX2D
    public abstract class IOnTrigger2DBinder<T> : BindCallbackAttribute.IBinderPredicate
        where T : MonoBehaviour
    {
        protected abstract string GetCallbackName();

        public bool EnableBind(MethodInfo methodInfo, object obj)
        {
            if (obj is GameObject)
            {
                if (null == (obj as GameObject).GetComponent<Collider2D>()) return false;
            }
            else if (!(obj is Collider2D)) return false;
            var args = methodInfo.GetParameters();
            return args.Length == 2
                && typeof(GameObject).Equals(args[0].ParameterType)
                && typeof(Collider2D).Equals(args[1].ParameterType);
        }

        public bool AddCallbacks(object target, MethodInfo methodInfo, object obj)
        {
            if (!EnableBind(methodInfo, obj)) return false;

            var collider2D = GameObjectExtensions.GetComponent<Collider2D>(obj);
            var onTrigger = collider2D.gameObject.GetOrAddComponent<T>();

            return BindCallbackAttribute.BindWithTypeAndCallbackName(target, methodInfo, onTrigger, typeof(T), GetCallbackName());
        }

        public bool RemoveCallbacks(object target, MethodInfo methodInfo, object obj)
        {
            if (!EnableBind(methodInfo, obj)) return false;

            var collider2D = GameObjectExtensions.GetComponent<Collider2D>(obj);
            var onTrigger = collider2D.gameObject.GetOrAddComponent<T>();

            return BindCallbackAttribute.UnbindWithTypeAndCallbackName(target, methodInfo, onTrigger, typeof(T), GetCallbackName());
        }
    }

    /// <summary>
    /// OnTriggerEnter2D()用のIBinderPredicate
    ///
    /// objに自動的にPhysic2DOnTriggerEnterCallbacksをアタッチします。
    /// <seealso cref="Physic2DOnTriggerEnterCallbacks"/>
    /// </summary>
    public class OnTriggerEnter2DBinder : IOnTrigger2DBinder<Physic2DOnTriggerEnterCallbacks>
    {
        protected override string GetCallbackName() => "OnTriggerEnter";
    }

    /// <summary>
    /// OnTriggerStay2D()用のIBinderPredicate
    ///
    /// objに自動的にPhysic2DOnTriggerStayCallbacksをアタッチします。
    /// <seealso cref="Physic2DOnTriggerStayCallbacks"/>
    /// </summary>
    public class OnTriggerStay2DBinder : IOnTrigger2DBinder<Physic2DOnTriggerStayCallbacks>
    {
        protected override string GetCallbackName() => "OnTriggerStay";
    }

    /// <summary>
    /// OnTriggerExit2D()用のIBinderPredicate
    ///
    /// objに自動的にPhysic2DOnTriggerExitCallbacksをアタッチします。
    /// <seealso cref="Physic2DOnTriggerExitCallbacks"/>
    /// </summary>
    public class OnTriggerExit2DBinder : IOnTrigger2DBinder<Physic2DOnTriggerExitCallbacks>
    {
        protected override string GetCallbackName() => "OnTriggerExit";
    }
    #endregion
}
