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
        class SupportedModelInfo : System.IEquatable<SupportedModelInfo>
        {
            public Model Model { get; }
            public IViewObject ViewObj { get; }
            public ControllerInfo ControllerInfo { get; }
            public IOnPointerEventControllerObject ControllerObj { get; }

            public SupportedModelInfo(Model model, IViewObject viewObj, ControllerInfo controllerInfo, IOnPointerEventControllerObject controllerObj)
            {
                Model = model;
                ViewObj = viewObj;
                ControllerInfo = controllerInfo;
                ControllerObj = controllerObj;
            }

            #region IEnumerable<SupportedModelInfo> interface
            public bool Equals(SupportedModelInfo other)
            {
                return Model == other.Model
                    && ViewObj == other.ViewObj
                    && ControllerInfo == other.ControllerInfo
                    && ControllerObj == other.ControllerObj;
            }
            public override bool Equals(object obj)
            {
                if (obj is SupportedModelInfo)
                    return Equals(obj as SupportedModelInfo);
                else
                    return base.Equals(obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            #endregion
        }

        class SupportedModelInfoEquality : IEqualityComparer<SupportedModelInfo>
        {
            public static SupportedModelInfoEquality DefaultInstance { get; } = new SupportedModelInfoEquality();
            #region IEqualityComparer<SupportedModelInfo> interface
            public bool Equals(SupportedModelInfo x, SupportedModelInfo y)
                => x.Equals(y);

            public int GetHashCode(SupportedModelInfo obj)
                => obj.GetHashCode();
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        class OnPointerEventDataBase : OnPointerEventData
        {
            HashSet<SupportedModelInfo> _inAreaObjects = new HashSet<SupportedModelInfo>();
            HashSet<SupportedModelInfo> _outAreaObjects = new HashSet<SupportedModelInfo>();

            public IReadOnlyCollection<SupportedModelInfo> InAreaObjects { get => _inAreaObjects; }
            public IReadOnlyCollection<SupportedModelInfo> OutAreaObjects { get => _outAreaObjects; }

            public virtual InputDefines.ButtonCondition ButtonCondition { get; }

            protected OnPointerEventDataBase(PointerType pointerType, PointerEventDispatcher dispatcher)
                : base(pointerType, dispatcher)
            { }

            public void UpdateInAreaObjects(Camera useCamera, IEnumerable<SupportedModelInfo> infos)
            {
                var ray = useCamera.ScreenPointToRay(PointerPos);
                var raycastResults = Physics.RaycastAll(ray).Select(_r => _r.transform);
                //Raycastに当たっているものだけを抽出する
                var currentInAreaObjs = infos.Where(_i => {
                    if(_i.ControllerObj.IsScreenOverlay)
                    {
                        return _i.ControllerObj.IsOnPointer(PointerPos, useCamera);
                    }
                    else
                    {
                        return raycastResults.Contains(_i.ControllerObj.Transform);
                    }
                });

                _outAreaObjects.Clear();
                foreach (var curInAreaObj in currentInAreaObjs
                    .Where(_c => !InAreaObjects.Contains(_c, SupportedModelInfoEquality.DefaultInstance)))
                {
                    _inAreaObjects.Add(curInAreaObj);
                }
                foreach(var delInAreaObj in InAreaObjects
                    .Where(_o => !currentInAreaObjs.Contains(_o, SupportedModelInfoEquality.DefaultInstance)))
                {
                    _outAreaObjects.Add(delInAreaObj);
                }
                _inAreaObjects.RemoveWhere(_i => !currentInAreaObjs.Contains(_i, SupportedModelInfoEquality.DefaultInstance));

                if(ButtonCondition == InputDefines.ButtonCondition.Down)
                {
                    var topModelInfo = GetTopModelInfoInArea();
                    PointerDownViewObject = topModelInfo?.ViewObj ?? null;
                }

                Logger.Log(Logger.Priority.Debug, () => $"debug -- PointerEventDispatcher -- UpdatePointer {PointerType}:{FingerID} InAreaObject count={InAreaObjects.Count}, OutAreaObject count={_outAreaObjects.Count}");
            }

            public bool DoMatchControllerInfo(ControllerInfo controllerInfo)
            {
                var pointerEventName = (PointerEventName)System.Enum.Parse(typeof(PointerEventName), controllerInfo.Keyword);
                switch (pointerEventName)
                {
                    case PointerEventName.onPointerDown:
                        return ButtonCondition == InputDefines.ButtonCondition.Down;
                    case PointerEventName.onPointerUp:
                    case PointerEventName.onPointerClick:
                        return ButtonCondition == InputDefines.ButtonCondition.Up;
                    default:
                        throw new System.NotImplementedException();
                }
            }

            public SupportedModelInfo GetTopModelInfoInArea()
            {
                return _inAreaObjects.OrderBy(_i => _i.ControllerObj, new IOnPointerEventControllerObjectComparer(Dispatcher.UseCamera)).FirstOrDefault();
            }
        }

        class MousePointerEventData : OnPointerEventDataBase
        {
            public override InputDefines.ButtonCondition ButtonCondition { get => Input.GetMouseButton(InputDefines.MouseButton.Left); }

            public MousePointerEventData(PointerEventDispatcher dispatcher)
                : base(PointerType.Mouse, dispatcher)
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
            public override InputDefines.ButtonCondition ButtonCondition { get => InputDefines.ToButtonCondition(Touch); }

            public TouchPointerEventData(int index, PointerEventDispatcher dispatcher)
                : base(PointerType.Touch, dispatcher)
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
            _mousePointerEventData = new MousePointerEventData(this);
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

        protected override IEnumerable<(Model model, IViewObject viewObj, ControllerInfo controllerInfo)> GetSupportedControllerInfos(ModelViewBinderInstanceMap binderInstanceMap)
        {
            return base.GetSupportedControllerInfos(binderInstanceMap)
                .Where(_c =>
                    _c.viewObj != null
                    && _c.viewObj.HasControllerObject<IOnPointerEventControllerObject>()
                );
        }

        protected override void UpdateImpl(ModelViewBinderInstanceMap binderInstanceMap)
        {
            var supportedControllerInfos = GetSupportedControllerInfos(binderInstanceMap)
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
                        e = new TouchPointerEventData(i, this);
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

        protected override object GetEventData(Model model, IViewObject viewObject, ControllerInfo controllerInfo)
        {
            Assert.IsTrue(viewObject.HasControllerObject<IOnPointerEventControllerObject>());
            var target = new SupportedModelInfo(model, viewObject, controllerInfo, viewObject.GetControllerObject<IOnPointerEventControllerObject>());

            var matchMouseEventData = _mousePointerEventData.InAreaObjects
                .Where(_obj => _obj.Equals(target))
                .Select(_obj => _mousePointerEventData)
                .Where(_e => _e.DoMatchControllerInfo(controllerInfo))
                .OfType<OnPointerEventData>()
                ;
            var matchTouchEventDatas = _touchPointerEventDatas
                .Where(_e => _e.InAreaObjects.Contains(target, SupportedModelInfoEquality.DefaultInstance)
                    && _e.DoMatchControllerInfo(controllerInfo))
                .OfType<OnPointerEventData>()
                ;
            var matchEventDatas = matchMouseEventData
                .Concat(matchTouchEventDatas);

            //Logger.Log(Logger.Priority.Debug, () => {
            //    var pointerTypes = matchEventDatas.Select(_e => $"{_e.PointerType}:{_e.FingerID}").Aggregate("", (_s, _c) => _s + _c + "; ");
            //    return $"debug -- PointerEventDispacther -- GetEventData controllerKeyword={controllerInfo.Keyword} doMatch?=>{matchEventDatas.Count()}({pointerTypes}), model={model}, view={viewObject}";
            //});
            var firstEventData = matchEventDatas.FirstOrDefault();

            switch((PointerEventName)System.Enum.Parse(typeof(PointerEventName), controllerInfo.Keyword))
            {
                case PointerEventName.onPointerDown:
                case PointerEventName.onPointerUp:
                case PointerEventName.onPointerEnter:
                case PointerEventName.onPointerStationary:
                case PointerEventName.onPointerExit:
                    return firstEventData;
                case PointerEventName.onPointerClick:
                    return (firstEventData != null && firstEventData.PointerDownViewObject == viewObject)
                        ? firstEventData : null;
                case PointerEventName.onPointerBeginDrag:
                case PointerEventName.onPointerDrag:
                case PointerEventName.onPointerEndDrag:
                case PointerEventName.onPointerDrop:
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
