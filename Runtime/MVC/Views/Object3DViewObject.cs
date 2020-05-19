using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    #region Cube
    [AvailableModelViewParamBinder(typeof(EmptyModelViewParamBinder))]
    public class CubeViewObject : MonoBehaviourViewObject
    {
        public static IViewObject CreateInstance()
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            return obj.AddComponent<MonoBehaviourViewObject>();
        }
    }
    #endregion


}
