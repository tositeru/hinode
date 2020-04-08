using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// ModelをGameObjectとして配置する時に使用してください
    /// </summary>
    public abstract class IModelMonoBehaivour : MonoBehaviour
    {
        public abstract Model Model { get; }
    }
}
