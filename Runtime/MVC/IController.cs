using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public interface IControllerSender
    {
        ModelViewBinderInstance ModelViewBinderInstance { get; }
        RecieverSelector Selector { get; set; }
    }

    public interface IControllerReceiver
    {
    }
}
