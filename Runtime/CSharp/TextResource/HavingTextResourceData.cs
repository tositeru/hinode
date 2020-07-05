using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="TextResources"/>
    /// </summary>
    public class HavingTextResourceData : IHavingTextResource
    {
        public static HavingTextResourceData Create(string key, params object[] paramList)
        {
            var d = new HavingTextResourceData();
            d.HavingTextResourceKey = key;
            d.SetParams(paramList);
            return d;
        }

        object[] _params;
        public string HavingTextResourceKey { get; set; }

        public int ParamCount { get => _params?.Length ?? 0; }
        public object[] GetTextResourceParams() => _params;

        public HavingTextResourceData ResizeParams(int length)
        {
            length = Mathf.Max(0, length);
            if (ParamCount == length) return this;

            var newParams = new object[length];
            var copyLength = System.Math.Min(length, ParamCount);
            for (var i = 0; i < copyLength; ++i)
                newParams[i] = _params[i];
            _params = newParams;
            return this;
        }

        public HavingTextResourceData SetParams(params object[] paramList)
        {
            _params = new object[paramList.Length];
            System.Array.Copy(paramList, _params, paramList.Length);
            return this;
        }

        public HavingTextResourceData SetParam(int index, object param)
        {
            Assert.IsTrue(0 <= index && index < ParamCount, $"Out of Range Index... index={index}, length={ParamCount}");
            _params[index] = param;
            return this;
        }
    }
}