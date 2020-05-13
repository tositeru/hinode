using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="IControllerSenderInstance"/>
    /// <seealso cref="IOnPointerDownSender"/>
    /// <seealso cref="IOnPointerUpSender"/>
    /// <seealso cref="IOnPointerClickSender"/>
    /// <seealso cref="IOnPointerBeginDragSender"/>
    /// <seealso cref="IOnPointerDragSender"/>
    /// <seealso cref="IOnPointerEndDragSender"/>
    /// <seealso cref="IOnPointerEnterSender"/>
    /// <seealso cref="IOnPointerExitSender"/>
    /// </summary>
    public class UnityPointerControllerSenderInstance : MonoBehaviour
        , IControllerSenderInstance
        , __legacy.IOnPointerDownSender
        , __legacy.IOnPointerUpSender
        , __legacy.IOnPointerClickSender
        , __legacy.IOnPointerBeginDragSender
        , __legacy.IOnPointerDragSender
        , __legacy.IOnPointerEndDragSender
        , __legacy.IOnPointerDropSender
        , __legacy.IOnPointerEnterSender
        , __legacy.IOnPointerExitSender
        //UnityEngine.EventSystems
        , IPointerDownHandler
        , IPointerUpHandler
        , IPointerClickHandler
        , IPointerEnterHandler
        , IPointerExitHandler
        , IBeginDragHandler
        , IDragHandler
        , IEndDragHandler
        , IDropHandler
    {
        public static IControllerSenderGroup CreateSenderGroup()
        {
            return new UnityComponentSenderGroup<UnityPointerControllerSenderInstance>(
                new Dictionary<string, System.Type>()
                {
                    {"onPointerDown", typeof(IOnPointerDownSender) },
                    {"onPointerUp", typeof(IOnPointerUpSender) },
                    {"onClick", typeof(IOnPointerClickSender) },
                    {"onBeginDrag", typeof(IOnPointerBeginDragSender) },
                    {"onDrag", typeof(IOnPointerDragSender) },
                    {"onEndDrag", typeof(IOnPointerEndDragSender) },
                    {"onDrop", typeof(IOnPointerDropSender) },
                    {"onPointerEnter", typeof(IOnPointerEnterSender) },
                    {"onPointerExit", typeof(IOnPointerExitSender) },
                });
        }

        #region IControllerSenderInstance
        public IControllerSenderGroup UseSenderGroup { get; set; }

        public EnableSenderCollection EnabledSenders { get; } = new EnableSenderCollection();

        public SelectorListDictionary SelectorListDict { get; } = new SelectorListDictionary();

        public Model Target { get; set; }
        public IViewObject TargetViewObj { get; set; }
        public ModelViewBinderInstanceMap UseBinderInstanceMap { get; set; }

        public void Destroy()
        {
            Destroy(this);
        }
        #endregion

        #region UnityEngine.EventSystems
        public void OnPointerClick(PointerEventData eventData)
        {
            var sendEventData = new __legacy.OnPointerClickEventData
            {
                FingerID = eventData.pointerId,
                PointerPos = eventData.position,
                PointerDownPos = eventData.pressPosition,
                MouseButtonType = InputDefines.ToMouseType(eventData.button)
            };
            this.Send<IOnPointerClickSender>(Target, UseBinderInstanceMap, sendEventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var sendEventData = new __legacy.OnPointerDownEventData
            {
                FingerID = eventData.pointerId,
                PointerPos = eventData.position,
                MouseButtonType = InputDefines.ToMouseType(eventData.button)
            };
            this.Send<IOnPointerDownSender>(Target, UseBinderInstanceMap, sendEventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var sendEventData = new __legacy.OnPointerUpEventData
            {
                FingerID = eventData.pointerId,
                PointerPos = eventData.position,
                PointerDownPos = eventData.pressPosition,
                MouseButtonType = InputDefines.ToMouseType(eventData.button)
            };
            this.Send<IOnPointerUpSender>(Target, UseBinderInstanceMap, sendEventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var sendEventData = new __legacy.OnPointerEnterEventData
            {
                FingerID = eventData.pointerId,
                PointerPos = eventData.position,
                MouseButtonType = InputDefines.ToMouseType(eventData.button)
            };
            this.Send<IOnPointerEnterSender>(Target, UseBinderInstanceMap, sendEventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var sendEventData = new __legacy.OnPointerExitEventData
            {
                FingerID = eventData.pointerId,
                PointerPos = eventData.position,
                MouseButtonType = InputDefines.ToMouseType(eventData.button)
            };
            this.Send<IOnPointerExitSender>(Target, UseBinderInstanceMap, sendEventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var sendEventData = new __legacy.OnPointerBeginDragEventData
            {
                FingerID = eventData.pointerId,
                PointerPos = eventData.position,
                MouseButtonType = InputDefines.ToMouseType(eventData.button)
            };
            this.Send<IOnPointerBeginDragSender>(Target, UseBinderInstanceMap, sendEventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var sendEventData = new __legacy.OnPointerDragEventData
            {
                FingerID = eventData.pointerId,
                PointerPos = eventData.position,
                MouseButtonType = InputDefines.ToMouseType(eventData.button)
            };
            this.Send<IOnPointerDragSender>(Target, UseBinderInstanceMap, sendEventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var sendEventData = new __legacy.OnPointerEndDragEventData
            {
                FingerID = eventData.pointerId,
                PointerPos = eventData.position,
                MouseButtonType = InputDefines.ToMouseType(eventData.button)
            };
            this.Send<IOnPointerEndDragSender>(Target, UseBinderInstanceMap, sendEventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var sendEventData = new __legacy.OnPointerDropEventData
            {
                FingerID = eventData.pointerId,
                PointerPos = eventData.position,
                MouseButtonType = InputDefines.ToMouseType(eventData.button)
            };
            this.Send<IOnPointerDropSender>(Target, UseBinderInstanceMap, sendEventData);
        }
        #endregion


    }
}
