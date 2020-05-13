using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public enum PointerEventName
    {
        onPointerDown,
        onPointerEnter,
        onPointerStationary,
        onPointerExit,
        onPointerUp,
        onPointerClick,
        onPointerBeginDrag,
        onPointerDrag,
        onPointerEndDrag,
        onPointerDrop,
    }

    /// <summary>
    /// イベントの発行が可能な対象はITouchableViewObjectを継承したIViewObjectのみです。
    /// <seealso cref="ITouchableViewObject"/>
    /// </summary>
    public class PointerEventDispatcher : IEventDispatcher
    {
        class SupportedModelInfo
        {
            public Model Model { get; }
            public IViewObject ViewObj { get; }
            public ControllerInfo ControllerInfo { get; }
            public IOnPointerEventControllerObject ControllerObj { get; }

            public SupportedModelInfo(Model model, IViewObject viewObj, ControllerInfo controllerInfo, IOnPointerEventControllerObject controllerObj)
            {
                Model = Model;
                ViewObj = viewObj;
                ControllerInfo = controllerInfo;
                ControllerObj = controllerObj;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        class OnPointerEventDataBase : OnPointerEventData
        {
            HashSet<(Model, IViewObject)> _inAreaObjects = new HashSet<(Model, IViewObject)>();

            protected OnPointerEventDataBase(PointerType pointerType)
                : base(pointerType)
            { }

            public void UpdateInAreaObjects(Camera useCamera, IEnumerable<SupportedModelInfo> infos)
            {
                var ray = useCamera.ScreenPointToRay(PointerPos);
                var results = Physics.RaycastAll(ray);
                //Raycastに当たっているものだけを抽出する
            }

            public void AddInAreaObject((Model, IViewObject) objPair)
            {
                if (_inAreaObjects.Contains(objPair)) return;
                _inAreaObjects.Add(objPair);
            }

            public void RemoveInAreaObject((Model, IViewObject) objPair)
            {
                if (!_inAreaObjects.Contains(objPair)) return;
                _inAreaObjects.Remove(objPair);
            }
        }

        class MousePointerEventData : OnPointerEventDataBase
        {
            public InputDefines.ButtonCondition ButtonCondition { get => Input.GetMouseButton(InputDefines.MouseButton.Left); }

            public MousePointerEventData()
                : base(PointerType.Mouse)
            { }

            public void Update()
            {
                PointerPos = Input.MousePos;
            }
        }

        class TouchPointerEventData : OnPointerEventDataBase
        {
            public bool IsAlive { get => TouchIndex < Input.TouchCount && FingerID == Touch.fingerId; }
            public int TouchIndex { get; }
            public Touch Touch { get => Input.GetTouch(TouchIndex); }
            public InputDefines.ButtonCondition ButtonCondition { get => InputDefines.ToButtonCondition(Touch); }

            public TouchPointerEventData(int index)
                : base(PointerType.Touch)
            {
                TouchIndex = index;
                FingerID = Touch.fingerId;
            }

            public bool Update()
            {
                if (!IsAlive) return false;

                PointerPos = Touch.position;
                return true;
            }
        }

        class ModelPointerInfo
        {
            public OnPointerEventData OnPointerDown { get; set; }
            public OnPointerEventData OnPointerUp { get; set; }
            public OnPointerEventData OnPointerClick { get; set; }
            public OnPointerEventData OnPointerEnter { get; set; }
            public OnPointerEventData OnPointerExit { get; set; }
            public OnPointerEventData OnPointerBeginDrag { get; set; }
            public OnPointerEventData OnPointerDrag { get; set; }
            public OnPointerEventData OnPointerEndDrag { get; set; }
            public OnPointerEventData OnPointerDrop { get; set; }
        }

        MousePointerEventData _mousePointerEventData;
        List<TouchPointerEventData> _touchPointerEventDatas = new List<TouchPointerEventData>();

        Dictionary<(Model, IViewObject), ModelPointerInfo> _eventDatas = new Dictionary<(Model, IViewObject), ModelPointerInfo>();

        Camera _useCamera;
        public Camera UseCamera
        {
            get => _useCamera;
            set
            {
                _useCamera = value;
                _doEnabled = (_useCamera != null);
            }
        }

        public PointerEventDispatcher()
        {
            _useCamera = Camera.main;
            _mousePointerEventData = new MousePointerEventData();
        }

        #region ISenderGroup interface
        bool _doEnabled = true;
        public override bool DoEnabled
        {
            get => _doEnabled && (ReplayableInput.Instance.MousePresent || ReplayableInput.Instance.TouchSupported);
            set => _doEnabled = value;
        }

        public override bool IsCreatableControllerObject(Model model, IViewObject viewObject)
        {
            if (viewObject == null) return false;
            return viewObject is MonoBehaviour;
        }

        public override IControllerObject CreateControllerObject(Model model, IViewObject viewObject)
        {
            Assert.IsTrue(IsCreatableControllerObject(model, viewObject));

            var behaviour = viewObject as MonoBehaviour;
            return behaviour.gameObject.GetOrAddComponent<OnPointerEventControllerMonoBehaivour>();
        }

        protected override void UpdateImpl(ModelViewBinderInstanceMap binderInstanceMap)
        {
            var supportedControllerInfos = binderInstanceMap.BindInstances.Values
                .SelectMany(_b => _b.GetControllerInfos())
                .Where(_c =>
                    EventInfos.ContainKeyword(_c.controllerInfo.Keyword)
                    && EventInfos.DoEnabledEvent(_c.controllerInfo.Keyword)
                    && _c.viewObj != null
                    && _c.viewObj.HasControllerObject<IOnPointerEventControllerObject>() 
                )
                .Select(_t => new SupportedModelInfo(
                    _t.model,
                    _t.viewObj,
                    _t.controllerInfo,
                    _t.viewObj.GetControllerObject<IOnPointerEventControllerObject>()))
                .OrderBy(_t => _t.ControllerObj, new IOnPointerEventControllerObjectComparer(UseCamera))
                .ToList();

            _mousePointerEventData.Update();
            _mousePointerEventData.UpdateInAreaObjects(UseCamera, supportedControllerInfos);
            {//Touch
                var input = ReplayableInput.Instance;
                for(var i=0; i<input.TouchCount; ++i)
                {
                    var t = input.GetTouch(i);
                    var e = _touchPointerEventDatas.FirstOrDefault(_e => _e.FingerID == t.fingerId);
                    if(e == null)
                    {
                        e = new TouchPointerEventData(i);
                        _touchPointerEventDatas.Add(e);
                    }

                    e.Update();
                    e.UpdateInAreaObjects(UseCamera, supportedControllerInfos);
                }
                //タッチされなくなったものを削除する
                _touchPointerEventDatas.RemoveAll(_e => !input.GetTouches().Any(_t => _t.fingerId == _e.FingerID));
            }
        }
        
        protected override EventInfoManager CreateEventInfoManager()
            => new EventInfoManager(
                    EventInfoManager.Info.Create<IOnPointerDownSender, IOnPointerDropReciever>(PointerEventName.onPointerDown),
                    EventInfoManager.Info.Create<IOnPointerUpSender, IOnPointerUpReciever>(PointerEventName.onPointerUp),
                    EventInfoManager.Info.Create<IOnPointerClickSender, IOnPointerClickReciever>(PointerEventName.onPointerClick),
                    EventInfoManager.Info.Create<IOnPointerEnterSender, IOnPointerEnterReciever>(PointerEventName.onPointerEnter),
                    EventInfoManager.Info.Create<IOnPointerStationarySender, IOnPointerStationaryReciever>(PointerEventName.onPointerStationary),
                    EventInfoManager.Info.Create<IOnPointerExitSender, IOnPointerExitReciever>(PointerEventName.onPointerExit),
                    EventInfoManager.Info.Create<IOnPointerBeginDragSender, IOnPointerBeginDragReciever>(PointerEventName.onPointerBeginDrag),
                    EventInfoManager.Info.Create<IOnPointerDragSender, IOnPointerDragReciever>(PointerEventName.onPointerDrag),
                    EventInfoManager.Info.Create<IOnPointerEndDragSender, IOnPointerEndDragReciever>(PointerEventName.onPointerEndDrag),
                    EventInfoManager.Info.Create<IOnPointerDropSender, IOnPointerDropReciever>(PointerEventName.onPointerDrop)
                    );

        protected override object GetEventData(string keyword, Model model, IViewObject viewObject)
        {
            if (_eventDatas.ContainsKey((model, viewObject))) return null;

            var eventData = _eventDatas[(model, viewObject)];
            var eventName = (PointerEventName)System.Enum.Parse(typeof(PointerEventName), keyword);
            switch (eventName)
            {
                case PointerEventName.onPointerDown: return eventData.OnPointerDown;
                case PointerEventName.onPointerUp: return eventData.OnPointerUp;
                case PointerEventName.onPointerClick: return eventData.OnPointerClick;
                case PointerEventName.onPointerEnter: return eventData.OnPointerEnter;
                case PointerEventName.onPointerExit: return eventData.OnPointerExit;
                case PointerEventName.onPointerBeginDrag: return eventData.OnPointerBeginDrag;
                case PointerEventName.onPointerDrag: return eventData.OnPointerDrag;
                case PointerEventName.onPointerEndDrag: return eventData.OnPointerEndDrag;
                case PointerEventName.onPointerDrop: return eventData.OnPointerDrop;
                default:
                    throw new System.NotImplementedException();
            }
        }
        #endregion

        public static void ConfigControllerType()
        {
            ControllerTypeManager.EntryPair<IOnPointerDownSender, IOnPointerDownReciever>();
            ControllerTypeManager.EntryPair<IOnPointerUpSender, IOnPointerUpReciever>();
            ControllerTypeManager.EntryPair<IOnPointerClickSender, IOnPointerClickReciever>();
            ControllerTypeManager.EntryPair<IOnPointerBeginDragSender, IOnPointerBeginDragReciever>();
            ControllerTypeManager.EntryPair<IOnPointerDragSender, IOnPointerDragReciever>();
            ControllerTypeManager.EntryPair<IOnPointerEndDragSender, IOnPointerEndDragReciever>();
            ControllerTypeManager.EntryPair<IOnPointerDropSender, IOnPointerDropReciever>();
            ControllerTypeManager.EntryPair<IOnPointerEnterSender, IOnPointerEnterReciever>();
            ControllerTypeManager.EntryPair<IOnPointerExitSender, IOnPointerExitReciever>();

            ControllerTypeManager.EntryRecieverExecuter<IOnPointerDownReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerDown(sender, eventData);
            });

            ControllerTypeManager.EntryRecieverExecuter<IOnPointerUpReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerUp(sender, eventData);
            });

            ControllerTypeManager.EntryRecieverExecuter<IOnPointerClickReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerClick(sender, eventData);
            });

            ControllerTypeManager.EntryRecieverExecuter<IOnPointerBeginDragReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerBeginDrag(sender, eventData);
            });
            ControllerTypeManager.EntryRecieverExecuter<IOnPointerDragReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerDrag(sender, eventData);
            });
            ControllerTypeManager.EntryRecieverExecuter<IOnPointerEndDragReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerEndDrag(sender, eventData);
            });
            ControllerTypeManager.EntryRecieverExecuter<IOnPointerDropReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerDrop(sender, eventData);
            });

            ControllerTypeManager.EntryRecieverExecuter<IOnPointerEnterReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerEnter(sender, eventData);
            });
            ControllerTypeManager.EntryRecieverExecuter<IOnPointerExitReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerExit(sender, eventData);
            });

        }

    }
}
