using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// 親の中での子供の並び順を持つことを表すinterface
    /// </summary>
    public interface ISiblingOrder
    {
        uint SiblingOrder { get; set; }
    }

    public static class ISiblingOrderConst
    {
        public const uint INVALID_ORDER = uint.MaxValue;
    }
}
