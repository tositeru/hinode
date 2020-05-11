using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public class ControllerMapInstance
    {
        public ControllerMap UseControllerMap { get; }

        public ControllerMapInstance(ControllerMap useControllerMap)
        {
            UseControllerMap = useControllerMap;

        }

        public void Update(ModelViewBinderInstanceMap binderInstanceMap)
        {
            foreach (var (bindInstance, viewObj, sender) in binderInstanceMap.BindInstances.Values
                .SelectMany(_b => _b.ViewObjects
                    .Where(_v => _b.HasControllerSenders(_v))
                    .SelectMany(_v => _b.GetControllerSenders(_v).Select(_s => (viewObj: _v, sender: _s)))
                    .Select(_t => (binder: _b, view: _t.viewObj, sender: _t.sender))))
            {
                Debug.Log($"{bindInstance.Model}:{viewObj.GetType()}:{sender.GetType()}");
            }

        }
    }

    public class PointerEventSenderGroup
    {
        public enum SenderName
        {
            onPointerDown, // at down
            onPointerPush, // at pushing
            onPointerUp, // at up
            onPointerClick, // at down and up in same obj

            onPointerEnter, // pointer pos enter to obj area
            onPointerInArea, // pointer pos in obj area
            onPointerExit, // pointer pos exit to obj area

            onPointerBeginDrag, // begin drag obj
            onPointerDrag, // dragging obj
            onPointerEndDrag, // end drag obj
        }

        public void Update()
        {
        }


    }
}
