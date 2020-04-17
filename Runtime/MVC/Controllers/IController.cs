using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public interface IControllerSender
    {
        Model Target { get; set; }
        ModelViewBinderInstanceMap UseBinderInstanceMap { get; set; }
    }

    public interface IControllerReciever
    {
    }

    public interface IControllerPairTag { }
}
