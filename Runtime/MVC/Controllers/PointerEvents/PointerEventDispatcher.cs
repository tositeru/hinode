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
            HashSet<SupportedModelInfo> _enterAreaObjects = new HashSet<SupportedModelInfo>();
            HashSet<SupportedModelInfo> _stationaryAreaObjects = new HashSet<SupportedModelInfo>();
            HashSet<SupportedModelInfo> _exitAreaObjects = new HashSet<SupportedModelInfo>();

            public IReadOnlyCollection<SupportedModelInfo> EnterAreaObjects { get => _enterAreaObjects; }
            public IReadOnlyCollection<SupportedModelInfo> StationaryAreaObjects { get => _stationaryAreaObjects; }
            public IReadOnlyCollection<SupportedModelInfo> ExitAreaObjects { get => _exitAreaObjects; }
            public IEnumerable<SupportedModelInfo> InAreaObjects { get => _enterAreaObjects.Concat(_stationaryAreaObjects); }
            public IEnumerable<SupportedModelInfo> AllObjects { get => InAreaObjects.Concat(ExitAreaObjects); }

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

                _exitAreaObjects.Clear();
                //move enter to stationary
                foreach(var enterObj in EnterAreaObjects
                    .Where(_c => !StationaryAreaObjects.Contains(_c, SupportedModelInfoEquality.DefaultInstance)))
                {
                    _stationaryAreaObjects.Add(enterObj);
                }
                //enter new obj
                _enterAreaObjects.Clear();
                foreach (var curInAreaObj in currentInAreaObjs
                    .Where(_c => !StationaryAreaObjects.Contains(_c, SupportedModelInfoEquality.DefaultInstance)))
                {
                    _enterAreaObjects.Add(curInAreaObj);
                }
                //move stationary to exit
                foreach(var delInAreaObj in StationaryAreaObjects
                    .Where(_o => !currentInAreaObjs.Contains(_o, SupportedModelInfoEquality.DefaultInstance)))
                {
                    _exitAreaObjects.Add(delInAreaObj);
                }
                _stationaryAreaObjects.RemoveWhere(_i => !currentInAreaObjs.Contains(_i, SupportedModelInfoEquality.DefaultInstance));

                if(ButtonCondition == InputDefines.ButtonCondition.Down)
                {
                    var topModelInfo = GetTopModelInfoInArea();
                    PointerDownViewObject = topModelInfo?.ViewObj ?? null;
                }

                Logger.Log(Logger.Priority.Debug, () => $"debug -- PointerEventDispatcher -- UpdatePointer {PointerType}:{FingerID} AreaObject count(enter={EnterAreaObjects.Count}, stationary={StationaryAreaObjects.Count}, exit={_exitAreaObjects.Count})");
            }

            public bool DoMatchControllerInfo(SupportedModelInfo supportedModelInfo)
            {
                var pointerEventName = (PointerEventName)System.Enum.Parse(typeof(PointerEventName), supportedModelInfo.ControllerInfo.Keyword);
                switch (pointerEventName)
                {
                    case PointerEventName.onPointerDown:
                        return ButtonCondition == InputDefines.ButtonCondition.Down
                            && InAreaObjects.Contains(supportedModelInfo, SupportedModelInfoEquality.DefaultInstance);
                    case PointerEventName.onPointerUp:
                        return ButtonCondition == InputDefines.ButtonCondition.Up
                            && InAreaObjects.Contains(supportedModelInfo, SupportedModelInfoEquality.DefaultInstance);
                    case PointerEventName.onPointerClick:
                        return ButtonCondition == InputDefines.ButtonCondition.Up
                            && InAreaObjects.Contains(supportedModelInfo, SupportedModelInfoEquality.DefaultInstance)
                            && PointerDownViewObject == supportedModelInfo.ViewObj;
                    case PointerEventName.onPointerEnter:
                        return (ButtonCondition == InputDefines.ButtonCondition.Push
                            || ButtonCondition == InputDefines.ButtonCondition.Down)
                            && EnterAreaObjects.Contains(supportedModelInfo, SupportedModelInfoEquality.DefaultInstance);
                    case PointerEventName.onPointerExit:
                        return (ButtonCondition == InputDefines.ButtonCondition.Push
                                && ExitAreaObjects.Contains(supportedModelInfo, SupportedModelInfoEquality.DefaultInstance))
                            || (ButtonCondition == InputDefines.ButtonCondition.Up
                                && AllObjects.Contains(supportedModelInfo, SupportedModelInfoEquality.DefaultInstance));
                    case PointerEventName.onPointerStationary:
                        return ButtonCondition == InputDefines.ButtonCondition.Push
                            && StationaryAreaObjects.Contains(supportedModelInfo, SupportedModelInfoEquality.DefaultInstance);
                    default:
                        throw new System.NotImplementedException();
                }
            }

            public SupportedModelInfo GetTopModelInfoInArea()
            {
                return InAreaObjects
                    .OrderBy(_i => _i.ControllerObj, new IOnPointerEventControllerObjectComparer(Dispatcher.UseCamera)).FirstOrDefault();
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
                //タッチされなくなったものを削除する
                _touchPointerEventDatas.RemoveAll(_e => !input.GetTouches().Any(_t => _t.fingerId == _e.FingerID));
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

            var matchMouseEventData = _mousePointerEventData.AllObjects
                .Select(_obj => _mousePointerEventData)
                .Distinct()
                .OfType<OnPointerEventDataBase>();
            var matchTouchEventDatas = _touchPointerEventDatas
                .OfType<OnPointerEventDataBase>();
            var matchEventDatas = matchMouseEventData
                .Concat(matchTouchEventDatas)
                .Where(_e => _e.DoMatchControllerInfo(target));

            //Logger.Log(Logger.Priority.Debug, () =>
            //{
            //    var pointerTypes = matchEventDatas.Select(_e => $"{_e.PointerType}:{_e.FingerID}").Aggregate("", (_s, _c) => _s + _c + "; ");
            //    return $"debug -- PointerEventDispacther -- GetEventData controllerKeyword={controllerInfo.Keyword} doMatch?=>{matchEventDatas.Count()}({pointerTypes}), model={model}, view={viewObject}";
            //});
            var firstEventData = matchEventDatas.FirstOrDefault();

            switch((PointerEventName)System.Enum.Parse(typeof(PointerEventName), controllerInfo.Keyword))
            {
                case PointerEventName.onPointerDown:
                case PointerEventName.onPointerUp:
                case PointerEventName.onPointerClick:
                case PointerEventName.onPointerEnter:
                case PointerEventName.onPointerStationary:
                case PointerEventName.onPointerExit:
                    return firstEventData;
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
            ControllerTypeManager.EntryPair<IOnPointerStationarySender, IOnPointerStationaryReciever>();
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
            ControllerTypeManager.EntryRecieverExecuter<IOnPointerStationaryReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerStationary(sender, eventData);
            });
            ControllerTypeManager.EntryRecieverExecuter<IOnPointerExitReciever, OnPointerEventData>((reciever, sender, eventData) => {
                reciever.OnPointerExit(sender, eventData);
            });

        }

    }
}
