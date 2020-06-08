using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using System.Linq;
using Hinode.Tests;

namespace Hinode.MVC.Tests.Views
{
    /// <summary>
    /// <seealso cref="RectTransformViewObject"/>
    /// </summary>
    public class TestRectTransformViewObject : TestBase
    {
        [UnityTest]
        public IEnumerator FixedParamBinderUpdatePasses()
        {
            var canvas = RectTransformViewObject.Create();
            var cameraObj = new GameObject();
            var camera = cameraObj.AddComponent<Camera>();
            yield return null;
        }

        class AppendViewObj : MonoBehaviourViewObject
            , RectTransformViewObject.IOptionalViewObject
        {
            public int Value { get; set; }

            #region RectTransformViewObject.IOptionalViewObject interface
            public void DettachFromMainViewObject()
            {
                Destroy(this);
            }
            #endregion

            public class ParamBinder : RectTransformViewObject.IOptionalViewObjectParamBinder
            {
                public int Value { get; set; }
                public RectTransformViewObject.IOptionalViewObject AppendTo(GameObject target)
                {
                    return target.AddComponent<AppendViewObj>();
                }

                public void Update(Model model, IViewObject viewObj)
                {
                    (viewObj as AppendViewObj).Value = Value;
                }
            }
        }

        [UnityTest]
        public IEnumerator OptionalViewObjectPasses()
        {
            var canvas = RectTransformViewObject.Create();
            yield return null;
            Assert.IsNull(canvas.GetComponent<AppendViewObj>());

            var paramBinder = new RectTransformViewObject.FixedParamBinder();
            paramBinder.AddOptionalViewObjectParamBinder(new AppendViewObj.ParamBinder() { Value = 432 });

            paramBinder.Update(null, canvas);

            Assert.IsNotNull(canvas.GetComponent<AppendViewObj>());
            var appendParamBinder = paramBinder.OptionalViewObjectParamBinders.OfType<AppendViewObj.ParamBinder>().First();
            Assert.AreEqual(appendParamBinder.Value, canvas.GetComponent<AppendViewObj>().Value);
        }

    }
}