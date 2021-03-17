using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public delegate void OnCollisionEnterDelegate(Collision2D collision);
    public delegate void OnCollisionStayDelegate(Collision2D collision);
    public delegate void OnCollisionExitDelegate(Collision2D collision);

    public delegate void OnTriggerEnterDelegate(GameObject self, Collider2D collision);
    public delegate void OnTriggerStayDelegate(GameObject self, Collider2D collision);
    public delegate void OnTriggerExitDelegate(GameObject self, Collider2D collision);

    public delegate void OnJointBreakDelegate(Joint2D collision);

    /// <summary>
    /// 
    /// </summary>
    public class Physic2DCallbacks : MonoBehaviour
    {
        SmartDelegate<OnCollisionEnterDelegate> _onCollisionEnter = new SmartDelegate<OnCollisionEnterDelegate>();
        SmartDelegate<OnCollisionStayDelegate> _onCollisionStay = new SmartDelegate<OnCollisionStayDelegate>();
        SmartDelegate<OnCollisionExitDelegate> _onCollisionExit = new SmartDelegate<OnCollisionExitDelegate>();

        SmartDelegate<OnTriggerEnterDelegate> _onTriggerEnter = new SmartDelegate<OnTriggerEnterDelegate>();
        SmartDelegate<OnTriggerStayDelegate> _onTriggerStay = new SmartDelegate<OnTriggerStayDelegate>();
        SmartDelegate<OnTriggerExitDelegate> _onTriggerExit = new SmartDelegate<OnTriggerExitDelegate>();

        SmartDelegate<OnJointBreakDelegate> _onJointBreak = new SmartDelegate<OnJointBreakDelegate>();

        public NotInvokableDelegate<OnCollisionEnterDelegate> OnCollisionEnter { get => _onCollisionEnter; }
        public NotInvokableDelegate<OnCollisionStayDelegate> OnCollisionStay { get => _onCollisionStay; }
        public NotInvokableDelegate<OnCollisionExitDelegate> OnCollisionExit { get => _onCollisionExit; }

        public NotInvokableDelegate<OnTriggerEnterDelegate> OnTriggerEnter { get => _onTriggerEnter; }
        public NotInvokableDelegate<OnTriggerStayDelegate> OnTriggerStay { get => _onTriggerStay; }
        public NotInvokableDelegate<OnTriggerExitDelegate> OnTriggerExit { get => _onTriggerExit; }

        public NotInvokableDelegate<OnJointBreakDelegate> OnJointBreak { get => _onJointBreak; }

        private void OnJointBreak2D(Joint2D joint)
        {
            _onJointBreak.SafeDynamicInvoke(joint, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }

        #region OnCollisionXXX2D
        private void OnCollisionEnter2D(Collision2D collision)
        {
            _onCollisionEnter.SafeDynamicInvoke(collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            _onCollisionStay.SafeDynamicInvoke(collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            _onCollisionExit.SafeDynamicInvoke(collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }
        #endregion

        #region OnTriggerXXX2D
        private void OnTriggerEnter2D(Collider2D collision)
        {
            _onTriggerEnter.SafeDynamicInvoke(gameObject, collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            _onTriggerStay.SafeDynamicInvoke(gameObject, collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            _onTriggerExit.SafeDynamicInvoke(gameObject, collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }
        #endregion
    }

    public class Physic2DOnCollisionEnterCallbacks : MonoBehaviour
    {
        SmartDelegate<OnCollisionEnterDelegate> _onEnter = new SmartDelegate<OnCollisionEnterDelegate>();
        public NotInvokableDelegate<OnCollisionEnterDelegate> OnCollisionEnter { get => _onEnter; }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            _onEnter.SafeDynamicInvoke(collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }
    }

    public class Physic2DOnCollisionStayCallbacks : MonoBehaviour
    {
        SmartDelegate<OnCollisionStayDelegate> _onStay = new SmartDelegate<OnCollisionStayDelegate>();
        public NotInvokableDelegate<OnCollisionStayDelegate> OnStay { get => _onStay; }
        private void OnCollisionStay2D(Collision2D collision)
        {
            _onStay.SafeDynamicInvoke(collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }
    }

    public class Physic2DOnCollisionExitCallbacks : MonoBehaviour
    {
        SmartDelegate<OnCollisionExitDelegate> _onExit = new SmartDelegate<OnCollisionExitDelegate>();
        public NotInvokableDelegate<OnCollisionExitDelegate> OnCollisionExit { get => _onExit; }
        private void OnCollisionExit2D(Collision2D collision)
        {
            _onExit.SafeDynamicInvoke(collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }
    }

    public class Physic2DOnTriggerEnterCallbacks : MonoBehaviour
    {
        SmartDelegate<OnTriggerEnterDelegate> _onCollisionEnter = new SmartDelegate<OnTriggerEnterDelegate>();
        public NotInvokableDelegate<OnTriggerEnterDelegate> OnTriggerEnter { get => _onCollisionEnter; }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            _onCollisionEnter.SafeDynamicInvoke(gameObject, collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }
    }

    public class Physic2DOnTriggerStayCallbacks : MonoBehaviour
    {
        SmartDelegate<OnTriggerStayDelegate> _onStay = new SmartDelegate<OnTriggerStayDelegate>();
        public NotInvokableDelegate<OnTriggerStayDelegate> OnTriggerStay { get => _onStay; }

        private void OnTriggerStay2D(Collider2D collision)
        {
            _onStay.SafeDynamicInvoke(gameObject, collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }
    }

    public class Physic2DOnTriggerExitCallbacks : MonoBehaviour
    {
        SmartDelegate<OnTriggerExitDelegate> _onExit = new SmartDelegate<OnTriggerExitDelegate>();
        public NotInvokableDelegate<OnTriggerExitDelegate> OnTriggerExit { get => _onExit; }

        private void OnTriggerExit2D(Collider2D collision)
        {
            _onExit.SafeDynamicInvoke(gameObject, collision, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }
    }

    public class Physic2DOnJointBreakCallbacks : MonoBehaviour
    {
        SmartDelegate<OnJointBreakDelegate> _onJointBreak = new SmartDelegate<OnJointBreakDelegate>();
        public NotInvokableDelegate<OnJointBreakDelegate> OnJointBreak { get => _onJointBreak; }
        private void OnJointBreak2D(Joint2D joint)
        {
            _onJointBreak.SafeDynamicInvoke(joint, () => $"Fail in Physic2DCallbacks...", PhysicCommon.LOG_SELECTOR_2D);
        }
    }
}