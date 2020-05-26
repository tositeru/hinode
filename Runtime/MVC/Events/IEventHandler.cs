using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public interface IEventHandler
    {
    }

    public interface IEventData
    {
        IEventData Clone();
    }
}
