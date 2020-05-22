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
        onPointerInArea,
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
            public IOnPointerEventHelpObject ControllerObj { get; }

            public SupportedModelInfo(Model model, IViewObject viewObj, ControllerInfo controllerInfo, IOnPointerEventHelpObject controllerObj)
            {
                Model = model;
                ViewObj = viewObj;
                ControllerInfo = controllerInfo;
                ControllerObj = controllerObj;
            }

            public bool HasSameModelView(SupportedModelInfo other)
                => Model == other.Model
                && ViewObj == other.ViewObj;

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
        abstract class OnPointerEventDataBase : IOnPointerEventData
        {
            public virtual InputDefines.ButtonCondition ButtonCondition { get; }
            protected abstract void UpdatePointerPos();

            #region IOnPointerEventData interface
            public abstract PointerType PointerType { get; }
            public abstract Vector3 PointerPos { get; }
            public abstract int FingerID { get; }
            #endregion

            HashSet<SupportedModelInfo> _enterAreaObjects = new HashSet<SupportedModelInfo>();
            HashSet<SupportedModelInfo> _stationaryAreaObjects = new HashSet<SupportedModelInfo>();
            HashSet<SupportedModelInfo> _exitAreaObjects = new HashSet<SupportedModelInfo>();
            SupportedModelInfo _topInAreaObject;

            public IReadOnlyCollection<SupportedModelInfo> EnterAreaObjects { get => _enterAreaObjects; }
            public IReadOnlyCollection<SupportedModelInfo> StationaryAreaObjects { get => _stationaryAreaObjects; }
            public IReadOnlyCollection<SupportedModelInfo> ExitAreaObjects { get => _exitAreaObjects; }
            public IEnumerable<SupportedModelInfo> InAreaObjects { get => _enterAreaObjects.Concat(_stationaryAreaObjects); }
            public IEnumerable<SupportedModelInfo> AllObjects { get => InAreaObjects.Concat(ExitAreaObjects); }

            #region IOnPointerEventData interface
            public Vector3 PointerPrevPos { get; private set; }

            public IViewObject PointerDownViewObject { get; private set; }
            public int PressFrame { get; private set; }
            public float PressSeconds { get; private set; }

            public bool IsStationary { get; private set; }
            public float StationarySeconds { get; private set; }
            public int StationaryFrame { get; private set; }

            public bool IsDrag { get; protected set; }
            public float DragSeconds { get; private set; }
            public int DragFrame { get; private set; }

            public PointerEventDispatcher Dispatcher { get; }
            public ReplayableInput Input { get => ReplayableInput.Instance; }
            #endregion

            protected OnPointerEventDataBase(PointerEventDispatcher dispatcher)
            {
                Dispatcher = dispatcher;

                UpdatePointerPos();
                PointerPrevPos = PointerPos;
            }

            public void Update()
            {
                PointerPrevPos = PointerPos;
                UpdatePointerPos();

                UpdatePressTime();
                UpdateStationary();
                UpdateDrag();
            }


            void UpdatePressTime()
            {
                switch (ButtonCondition)
                {
                    case InputDefines.ButtonCondition.Down:
                    case InputDefines.ButtonCondition.Free:
                        PressSeconds = 0f;
                        PressFrame = 0;
                        break;
                    case InputDefines.ButtonCondition.Up:
                    case InputDefines.ButtonCondition.Push:
                        PressSeconds += Time.deltaTime;
                        PressFrame++;
                        break;
                }
            }

            void UpdateStationary()
            {
                if (IsStationary)
                {
                    StationaryFrame++;
                    StationarySeconds = Time.deltaTime;
                }
                else
                {
                    StationaryFrame = 0;
                    StationarySeconds = 0f;
                }
                IsStationary = (PointerPos == PointerPrevPos);
            }

            void UpdateDrag()
            {
                if (PointerDownViewObject == null)
                {
                    IsDrag = false;
                    DragFrame = 0;
                    DragSeconds = 0;
                    return;
                }

                if (IsDrag)
                {
                    DragFrame++;
                    DragSeconds += Time.deltaTime;
                }
                else if (PointerDownViewObject.UseBindInfo != null)
                {
                    var bindInfo = PointerDownViewObject.UseBindInfo;
                    var isDraggable = bindInfo.Controllers.ContainsKey(PointerEventName.onPointerBeginDrag.ToString())
                        || bindInfo.Controllers.ContainsKey(PointerEventName.onPointerDrag.ToString())
                        || bindInfo.Controllers.ContainsKey(PointerEventName.onPointerEndDrag.ToString());
                    IsDrag = !IsStationary && isDraggable;
                }
            }

            public void UpdateInAreaObjects(Camera useCamera, IEnumerable<SupportedModelInfo> infos)
            {
                var ray = useCamera.ScreenPointToRay(PointerPos);
                var raycastResults = Physics.RaycastAll(ray).Select(_r => _r.transform);
                //Raycastに当たっているものだけを抽出する
                var currentInAreaObjs = infos.Where(_i =>
                {
                    if (_i.ControllerObj.IsScreenOverlay)
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
                foreach (var enterObj in EnterAreaObjects
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
                foreach (var delInAreaObj in StationaryAreaObjects
                    .Where(_o => !currentInAreaObjs.Contains(_o, SupportedModelInfoEquality.DefaultInstance)))
                {
                    _exitAreaObjects.Add(delInAreaObj);
                }
                _stationaryAreaObjects.RemoveWhere(_i => !currentInAreaObjs.Contains(_i, SupportedModelInfoEquality.DefaultInstance));

                _topInAreaObject = GetTopModelInfoInArea();
                switch (ButtonCondition)
                {
                    case InputDefines.ButtonCondition.Down:
                        PointerDownViewObject = _topInAreaObject?.ViewObj ?? null;
                        break;
                    case InputDefines.ButtonCondition.Free:
                        PointerDownViewObject = null;
                        break;
                }

                Logger.Log(Logger.Priority.Debug, () => $"debug -- PointerEventDispatcher -- UpdatePointer {PointerType}:{FingerID}:{ButtonCondition} AreaObject count(enter={EnterAreaObjects.Count}, stationary={StationaryAreaObjects.Count}, exit={_exitAreaObjects.Count})");
            }

            public bool DoMatchControllerInfo(SupportedModelInfo supportedModelInfo)
            {
                var pointerEventName = (PointerEventName)System.Enum.Parse(typeof(PointerEventName), supportedModelInfo.ControllerInfo.Keyword);
                switch (pointerEventName)
                {
                    case PointerEventName.onPointerDown:
                        return ButtonCondition == InputDefines.ButtonCondition.Down
                            && (_topInAreaObject?.HasSameModelView(supportedModelInfo) ?? false);
                    case PointerEventName.onPointerUp:
                        return ButtonCondition == InputDefines.ButtonCondition.Up
                            && (_topInAreaObject?.HasSameModelView(supportedModelInfo) ?? false);
                    case PointerEventName.onPointerClick:
                        return ButtonCondition == InputDefines.ButtonCondition.Up
                            && (_topInAreaObject?.HasSameModelView(supportedModelInfo) ?? false)
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
                    case PointerEventName.onPointerInArea:
                        return ButtonCondition == InputDefines.ButtonCondition.Push
                            && StationaryAreaObjects.Contains(supportedModelInfo, SupportedModelInfoEquality.DefaultInstance);
                    case PointerEventName.onPointerBeginDrag:
                        return IsDrag
                            && ButtonCondition == InputDefines.ButtonCondition.Push
                            && DragFrame == 0
                            && PointerDownViewObject == supportedModelInfo.ViewObj;
                    case PointerEventName.onPointerDrag:
                        return IsDrag
                            && ButtonCondition == InputDefines.ButtonCondition.Push
                            && DragFrame > 0
                            && !IsStationary
                            && PointerDownViewObject == supportedModelInfo.ViewObj;
                    case PointerEventName.onPointerEndDrag:
                        return IsDrag
                            && ButtonCondition == InputDefines.ButtonCondition.Up
                            && PointerDownViewObject == supportedModelInfo.ViewObj;
                    case PointerEventName.onPointerDrop:
                        {
                            return IsDrag
                                && ButtonCondition == InputDefines.ButtonCondition.Up
                                && (_topInAreaObject?.HasSameModelView(supportedModelInfo) ?? false);
                        }
                    default:
                        throw new System.NotImplementedException();
                }
            }

            public SupportedModelInfo GetTopModelInfoInArea()
            {
                return InAreaObjects
                    .OrderBy(_i => _i.ControllerObj, new IOnPointerEventControllerObjectComparer(Dispatcher.UseCamera))
                    .FirstOrDefault();
            }
        }

        class MousePointerEventData : OnPointerEventDataBase
        {
            Vector3 _pointerPos;
            #region override OnPointerEventDataBase
            public override PointerType PointerType { get => PointerType.Mouse; }
            public override Vector3 PointerPos { get => _pointerPos; }
            public override int FingerID { get => -1; }
            public override InputDefines.ButtonCondition ButtonCondition { get => Input.GetMouseButton(InputDefines.MouseButton.Left); }

            protected override void UpdatePointerPos()
            {
                _pointerPos = Input.MousePos;
            }
            #endregion

            public MousePointerEventData(PointerEventDispatcher dispatcher)
                : base(dispatcher)
            { }
        }

        class TouchPointerEventData : OnPointerEventDataBase
        {
            Vector3 _pointerPos;

            public bool IsAlive { get => TouchIndex < Input.TouchCount && FingerID == Touch.fingerId; }
            public int TouchIndex { get; }
            public Touch Touch { get => Input.GetTouch(TouchIndex); }

            #region override OnPointerEventDataBase
            public override PointerType PointerType { get => PointerType.Touch; }
            public override Vector3 PointerPos { get => _pointerPos; }
            public override int FingerID { get; }
            public override InputDefines.ButtonCondition ButtonCondition { get => InputDefines.ToButtonCondition(Touch); }

            protected override void UpdatePointerPos()
            {
                _pointerPos = Touch.position;
            }
            #endregion

            public TouchPointerEventData(int index, PointerEventDispatcher dispatcher)
                : base(dispatcher)
            {
                TouchIndex = index;
                FingerID = Touch.fingerId;
            }
        }

        MousePointerEventData _mousePointerEventData;
        List<TouchPointerEventData> _touchPointerEventDatas = new List<TouchPointerEventData>();
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

        public override IEventDispatcherHelper CreateEventDispatcherHelpObject(Model model, IViewObject viewObject)
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
                    && _c.viewObj.HasEventDispatcherHelpObject<IOnPointerEventHelpObject>()
                );
        }

        protected override void UpdateImpl(ModelViewBinderInstanceMap binderInstanceMap)
        {
            var supportedControllerInfos = GetSupportedControllerInfos(binderInstanceMap)
                .Select(_t => new SupportedModelInfo(
                    _t.model,
                    _t.viewObj,
                    _t.controllerInfo,
                    _t.viewObj.GetEventDispathcerHelpObject<IOnPointerEventHelpObject>()))
                .OrderBy(_t => _t.ControllerObj, new IOnPointerEventControllerObjectComparer(UseCamera))
                .ToList();
            var checkOrder = supportedControllerInfos
                .Select(_s => _s.Model.Name)
                .Aggregate("", (_s, _c) => _s + _c + " : ");

            _mousePointerEventData.Update();
            _mousePointerEventData.UpdateInAreaObjects(UseCamera, supportedControllerInfos);
            {//Touch
                var input = ReplayableInput.Instance;
                //タッチされなくなったものを削除する
                _touchPointerEventDatas.RemoveAll(_e => !input.GetTouches().Any(_t => _t.fingerId == _e.FingerID));
                for (var i = 0; i < input.TouchCount; ++i)
                {
                    var t = input.GetTouch(i);
                    var e = _touchPointerEventDatas.FirstOrDefault(_e => _e.FingerID == t.fingerId);
                    if (e == null)
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
                    EventInfoManager.Info.Create<IOnPointerDownReciever>(PointerEventName.onPointerDown),
                    EventInfoManager.Info.Create<IOnPointerUpReciever>(PointerEventName.onPointerUp),
                    EventInfoManager.Info.Create<IOnPointerClickReciever>(PointerEventName.onPointerClick),
                    EventInfoManager.Info.Create<IOnPointerEnterReciever>(PointerEventName.onPointerEnter),
                    EventInfoManager.Info.Create<IOnPointerInAreaReciever>(PointerEventName.onPointerInArea),
                    EventInfoManager.Info.Create<IOnPointerExitReciever>(PointerEventName.onPointerExit),
                    EventInfoManager.Info.Create<IOnPointerBeginDragReciever>(PointerEventName.onPointerBeginDrag),
                    EventInfoManager.Info.Create<IOnPointerDragReciever>(PointerEventName.onPointerDrag),
                    EventInfoManager.Info.Create<IOnPointerEndDragReciever>(PointerEventName.onPointerEndDrag),
                    EventInfoManager.Info.Create<IOnPointerDropReciever>(PointerEventName.onPointerDrop)
                    );

        protected override object GetEventData(Model model, IViewObject viewObject, ControllerInfo controllerInfo)
        {
            Assert.IsTrue(viewObject.HasEventDispatcherHelpObject<IOnPointerEventHelpObject>());
            var target = new SupportedModelInfo(model, viewObject, controllerInfo, viewObject.GetEventDispathcerHelpObject<IOnPointerEventHelpObject>());

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
            return matchEventDatas.FirstOrDefault();

            //switch((PointerEventName)System.Enum.Parse(typeof(PointerEventName), controllerInfo.Keyword))
            //{
            //    case PointerEventName.onPointerDown:
            //    case PointerEventName.onPointerUp:
            //    case PointerEventName.onPointerClick:
            //    case PointerEventName.onPointerEnter:
            //    case PointerEventName.onPointerInArea:
            //    case PointerEventName.onPointerExit:
            //    case PointerEventName.onPointerBeginDrag:
            //    case PointerEventName.onPointerDrag:
            //    case PointerEventName.onPointerEndDrag:
            //    case PointerEventName.onPointerDrop:
            //        return firstEventData;
            //    default:
            //        throw new System.NotImplementedException();
            //}
        }
        #endregion

        public static void ConfigControllerType(EventHandlerTypeManager typeManager)
        {
            typeManager.EntryEventHandlerExecuter<IOnPointerDownReciever, IOnPointerEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerDown(sender, eventData);
            });

            typeManager.EntryEventHandlerExecuter<IOnPointerUpReciever, IOnPointerEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerUp(sender, eventData);
            });

            typeManager.EntryEventHandlerExecuter<IOnPointerClickReciever, IOnPointerEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerClick(sender, eventData);
            });

            typeManager.EntryEventHandlerExecuter<IOnPointerBeginDragReciever, IOnPointerEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerBeginDrag(sender, eventData);
            });
            typeManager.EntryEventHandlerExecuter<IOnPointerDragReciever, IOnPointerEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerDrag(sender, eventData);
            });
            typeManager.EntryEventHandlerExecuter<IOnPointerEndDragReciever, IOnPointerEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerEndDrag(sender, eventData);
            });
            typeManager.EntryEventHandlerExecuter<IOnPointerDropReciever, IOnPointerEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerDrop(sender, eventData);
            });

            typeManager.EntryEventHandlerExecuter<IOnPointerEnterReciever, IOnPointerEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerEnter(sender, eventData);
            });
            typeManager.EntryEventHandlerExecuter<IOnPointerInAreaReciever, IOnPointerEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerInArea(sender, eventData);
            });
            typeManager.EntryEventHandlerExecuter<IOnPointerExitReciever, IOnPointerEventData>((reciever, sender, eventData) =>
            {
                reciever.OnPointerExit(sender, eventData);
            });

        }

    }
}
