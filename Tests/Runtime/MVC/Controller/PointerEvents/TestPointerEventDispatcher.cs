using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.Controller.Pointer
{
    /// <summary>
    /// <seealso cref="PointerEventDispatcher"/>
    /// </summary>
    public class TestPointerEventDispatcher : TestBase
    {
        [SetUp]
        public void Setup()
        {
            Logger.PriorityLevel = Logger.Priority.Debug;
        }

        [Test]
        public void CheckEventInfoDefinationPasses()
        {
            var senderGroup = new PointerEventDispatcher();

            var eventInfo = senderGroup.EventInfos;

            var checkList = new (PointerEventName, System.Type senderType, System.Type recieverType)[]
            {
                (PointerEventName.onPointerDown, typeof(IOnPointerDownSender), typeof(IOnPointerDropReciever)),
                (PointerEventName.onPointerUp, typeof(IOnPointerUpSender), typeof(IOnPointerUpReciever)),
                (PointerEventName.onPointerClick, typeof(IOnPointerClickSender), typeof(IOnPointerClickReciever)),
                (PointerEventName.onPointerEnter, typeof(IOnPointerEnterSender), typeof(IOnPointerEnterReciever)),
                (PointerEventName.onPointerStationary, typeof(IOnPointerStationarySender), typeof(IOnPointerStationaryReciever)),
                (PointerEventName.onPointerExit, typeof(IOnPointerExitSender), typeof(IOnPointerExitReciever)),
                (PointerEventName.onPointerBeginDrag, typeof(IOnPointerBeginDragSender), typeof(IOnPointerBeginDragReciever)),
                (PointerEventName.onPointerDrag, typeof(IOnPointerDragSender), typeof(IOnPointerDragReciever)),
                (PointerEventName.onPointerEndDrag, typeof(IOnPointerEndDragSender), typeof(IOnPointerEndDragReciever)),
                (PointerEventName.onPointerDrop, typeof(IOnPointerDropSender), typeof(IOnPointerDropReciever))
            };

            foreach (var (eventName, senderType, recieverType) in checkList)
            {
                Assert.IsTrue(eventInfo.ContainKeyword(eventName), $"Invalid {eventName}...");
                Assert.AreEqual(senderType, eventInfo.GetSenderType(eventName), $"Invalid {eventName}...");
                Assert.AreEqual(recieverType, eventInfo.GetRecieverType(eventName), $"Invalid {eventName}...");
                Assert.IsTrue(eventInfo.DoEnabledEvent(eventName), $"Invalid {eventName}...");
            }
        }

        [Test]
        public void DoEnabledPasses()
        {
            var input = ReplayableInput.Instance;
            input.IsReplaying = true;

            var pointerEventDispatcher = new PointerEventDispatcher();
            Assert.IsFalse(pointerEventDispatcher.DoEnabled);

            var testData = new (bool result, bool mousePresent, bool touchSupported)[]
            {
                (true, true, true),
                (true, true, false),
                (true, false, true),
                (false, false, false)
            };
            foreach(var (result, mousePresent, touchSupported) in testData)
            {
                var errorMessage = $"Fail When (mousePresent,touchSupported)=({mousePresent}, {touchSupported})...";
                input.RecordedMousePresent = mousePresent;
                input.RecordedTouchSupported = touchSupported;

                pointerEventDispatcher.DoEnabled = true;
                if(result)
                    Assert.IsTrue(pointerEventDispatcher.DoEnabled, errorMessage);
                else
                    Assert.IsFalse(pointerEventDispatcher.DoEnabled, errorMessage);

                pointerEventDispatcher.DoEnabled = false;
                Assert.IsFalse(pointerEventDispatcher.DoEnabled, errorMessage);
            }
        }

        class CheckOnPointerEventModel : Model
            , IOnPointerDownReciever
            , IOnPointerUpReciever
            , IOnPointerClickReciever
            , IOnPointerEnterReciever
            , IOnPointerExitReciever
            , IOnPointerBeginDragReciever
            , IOnPointerDragReciever
            , IOnPointerEndDragReciever
            , IOnPointerDropReciever
        {
            public Model SenderModel { get; set; }
            public OnPointerEventData EventData { get; set; }

            public void Reset()
            {
                SenderModel = null;
                EventData = null;
            }

            void Set(Model sender, OnPointerEventData eventData)
            {
                SenderModel = sender;
                EventData = eventData;
            }

            public void OnPointerBeginDrag(Model sender, OnPointerEventData eventData) => Set(sender, eventData);
            public void OnPointerClick(Model sender, OnPointerEventData eventData) => Set(sender, eventData);
            public void OnPointerDown(Model sender, OnPointerEventData eventData) => Set(sender, eventData);
            public void OnPointerDrag(Model sender, OnPointerEventData eventData) => Set(sender, eventData);
            public void OnPointerDrop(Model sender, OnPointerEventData eventData) => Set(sender, eventData);
            public void OnPointerEndDrag(Model sender, OnPointerEventData eventData) => Set(sender, eventData);
            public void OnPointerEnter(Model sender, OnPointerEventData eventData) => Set(sender, eventData);
            public void OnPointerExit(Model sender, OnPointerEventData eventData) => Set(sender, eventData);
            public void OnPointerUp(Model sender, OnPointerEventData eventData) => Set(sender, eventData);

        }

        class TestOnPointerEventViewObj : MonoBehaviourViewObject
        {

        }

        [UnityTest, Description("ModelViewInstanceでIOnPointerEventControllerObjectを作成しているか確認するためのテスト")]
        public IEnumerator CreatePointerEventControllerObjectPasses()
        {
            yield return null;
            #region Initial Enviroment

            var viewInstanceCreator = new UnityViewInstanceCreator()
                .AddPredicate(typeof(TestOnPointerEventViewObj), () => {
                    var obj = new GameObject("__onPointerEventViewObj");
                    return obj.AddComponent<TestOnPointerEventViewObj>();
                }, new EmptyModelViewParamBinder())
                .AddPredicate(typeof(EmptyViewObject), () => {
                    return new EmptyViewObject();
                }, new EmptyModelViewParamBinder())
            ;

            var pointerEventModelBinder = new ModelViewBinder("*", null,
                    new ModelViewBinder.BindInfo(typeof(TestOnPointerEventViewObj))
                        .AddControllerInfo(
                            new ControllerInfo(PointerEventName.onPointerDown,
                            new RecieverSelector(ModelRelationShip.Self, "", "")
                        ))
                )
                .AddEnabledModelType<CheckOnPointerEventModel>();
            var otherBinder = new ModelViewBinder("*", null,
                    new ModelViewBinder.BindInfo(typeof(EmptyViewObject))
                );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator,
                pointerEventModelBinder, otherBinder);
            binderMap.UseEventDispatcherMap = new EventDispatcherMap(
                new PointerEventDispatcher()
            );
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new CheckOnPointerEventModel();
            var child = new Model(); child.Parent = model;
            binderInstanceMap.RootModel = model;
            #endregion

            {//
                var binderInstance = binderInstanceMap.BindInstances[model];
                var viewObj = binderInstance.ViewObjects.First();
                Assert.IsTrue(binderInstance.HasControllerObject<IOnPointerEventControllerObject>(viewObj));
                Assert.IsTrue(viewObj.HasControllerObject<IOnPointerEventControllerObject>());
                Assert.IsNotNull(binderInstance.GetControllerObject<IOnPointerEventControllerObject>(viewObj));
                Assert.IsNotNull(viewObj.GetControllerObject<IOnPointerEventControllerObject>());
                Assert.AreSame(
                    binderInstance.GetControllerObject<IOnPointerEventControllerObject>(viewObj),
                    viewObj.GetControllerObject<IOnPointerEventControllerObject>());
            }

            {//ない時
                var binderInstance = binderInstanceMap.BindInstances[child];
                var viewObj = binderInstance.ViewObjects.First();
                Assert.IsFalse(binderInstance.HasControllerObject<IOnPointerEventControllerObject>(viewObj));
                Assert.IsFalse(viewObj.HasControllerObject<IOnPointerEventControllerObject>());

                Assert.IsNull(binderInstance.GetControllerObject<IOnPointerEventControllerObject>(viewObj));
                Assert.IsNull(viewObj.GetControllerObject<IOnPointerEventControllerObject>());
            }
        }

        class PointerPriorityOrderPassesModel : Model
            , IOnPointerDownReciever
        {
            public Model SenderModel { get; set; }

            public void OnPointerDown(Model sender, OnPointerEventData eventData)
            {
                SenderModel = sender;
            }
        }

        [UnityTest, Description("オブジェクトが重なっている場合にイベントが発生される優先順位の確認")]
        public IEnumerator PointerPriorityOrderPasses()
        {
            string screenOverlayQuery = "ScreenOverlay";
            string nearestScreenOverlayQuery = "NearestScreenOverlay";
            string sameCanvasScreenOverlayQuery = "SameCanvasScreenOverlay";
            string screenSpaceCameraQuery = "ScreenSpaceCamera";
            string worldCanvasQuery = "WorldCanvas";
            string object3DQuery = "3DObject";
            string canvasViewID = "canvas";

            string disableEventID = "disableEvents";
            #region Construct Enviroment
            var mainCamera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
            mainCamera.tag = "MainCamera";
            //
            // 画面上に以下のViewがあるようにしています。
            // - ScreenOverlayCanvas
            //   - screenOverlay
            // - ScreenSpaceCameraCanvas
            //   - screenSpaceCamera
            // - WorldCanvas
            //   - world
            // - Transform (3DObject)
            var viewCreator = new UnityViewInstanceCreator()
                .AddUnityViewObjects()
                .AddPredicate(typeof(CubeViewObject), () => CubeViewObject.CreateInstance(), new EmptyModelViewParamBinder())
            ;

            var nearestScreenOverlayBinder = new ModelViewBinder(nearestScreenOverlayQuery, null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay,
                        SortingOrder = 10,
                    })
                    .AddViewLayout("anchorMin", Vector2.one * (0.5f - 0.05f))
                    .AddViewLayout("anchorMax", Vector2.one * (0.5f + 0.05f))
                    .AddViewLayout("offsetMin", Vector2.zero)
                    .AddViewLayout("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown,
                        new RecieverSelector(ModelRelationShip.Parent, "root", "") 
                    ))
                );

            var screenOverlayBinder = new ModelViewBinder(screenOverlayQuery, null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder() {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayout("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayout("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayout("offsetMin", Vector2.zero)
                    .AddViewLayout("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown,
                        new RecieverSelector(ModelRelationShip.Parent, "root", "")
                    ))
                );

            var sameCanvasScreenOverlayBinder = new ModelViewBinder(sameCanvasScreenOverlayQuery, null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .AddViewLayout(TransformViewLayoutName.parent, new ModelViewSelector(ModelRelationShip.Parent, "", canvasViewID))
                    .AddViewLayout("anchorMin", Vector2.one * (0.5f - 0.08f))
                    .AddViewLayout("anchorMax", Vector2.one * (0.5f + 0.08f))
                    .AddViewLayout("offsetMin", Vector2.zero)
                    .AddViewLayout("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown,
                        new RecieverSelector(ModelRelationShip.Parent, "root", "")
                    ))
                );

            var screenSpaceCameraBinder = new ModelViewBinder(screenSpaceCameraQuery, null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceCamera,
                        WorldCamera = mainCamera,
                        PlaneDistance = 10f,
                    })
                    .AddViewLayout("anchorMin", Vector2.one * (0.5f - 0.2f))
                    .AddViewLayout("anchorMax", Vector2.one * (0.5f + 0.2f))
                    .AddViewLayout("offsetMin", Vector2.zero)
                    .AddViewLayout("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown,
                        new RecieverSelector(ModelRelationShip.Parent, "root", "")
                    ))
                );

            var worldCanvasPos = mainCamera.transform.position + mainCamera.transform.forward * 20f;
            var worldCanvasBinder = new ModelViewBinder(worldCanvasQuery, null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.WorldSpace,
                        WorldCamera = mainCamera,
                    })
                    .AddViewLayout(TransformViewLayoutName.pos, worldCanvasPos)
                    .AddViewLayout(RectTransformViewLayoutName.size, new Vector2(10, 10))
                    .AddViewLayout(RectTransformViewLayoutName.anchorMin, new Vector2(0f, 0f))
                    .AddViewLayout(RectTransformViewLayoutName.anchorMax, new Vector2(0.6f, 0.6f))
                    .AddViewLayout(RectTransformViewLayoutName.offsetMin, Vector2.zero)
                    .AddViewLayout(RectTransformViewLayoutName.offsetMax, Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown,
                        new RecieverSelector(ModelRelationShip.Parent, "root", "")
                    ))
                );

            var object3DPos = mainCamera.transform.position + mainCamera.transform.forward * 40f;
            var obj3DBinder = new ModelViewBinder(object3DQuery, null
                , new ModelViewBinder.BindInfo(typeof(CanvasViewObject))
                    .AddViewLayout(TransformViewLayoutName.pos, object3DPos)
                    .AddViewLayout(TransformViewLayoutName.localScale, new Vector3(40f, 40f, 1))
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown,
                        new RecieverSelector(ModelRelationShip.Parent, "root", "")
                    ))
                );

            var binderMap = new ModelViewBinderMap(viewCreator
                , nearestScreenOverlayBinder
                , screenOverlayBinder
                , sameCanvasScreenOverlayBinder
                , screenSpaceCameraBinder
                , worldCanvasBinder
                , obj3DBinder
                );
            binderMap.UseEventDispatchStateMap = new EventDispatchStateMap()
                .AddState(DispatchStateName.disable, new EventDispatchQuery(disableEventID, ""))
            ;
            var root = new PointerPriorityOrderPassesModel() { Name = "Root" };
            var nearestScreenOverlay = new Model() { Name = nearestScreenOverlayQuery };
            var screenOverlay = new Model() { Name = screenOverlayQuery };
            var sameCanvasScreenOverlay = new Model() { Name = sameCanvasScreenOverlayQuery };
            var screenSpaceCamera = new Model() { Name = screenSpaceCameraQuery };
            var worldCanvas = new Model() { Name = worldCanvasQuery };
            var object3D = new Model() { Name = object3DQuery };
            root.AddChildren(
                screenOverlay,
                screenSpaceCamera,
                worldCanvas,
                object3D);
            screenOverlay.AddChildren(sameCanvasScreenOverlay);

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            binderInstanceMap.RootModel = root;
            #endregion
            yield return null;

            //画面中央でPointerを押した時に送られるイベントを確認していく
            //優先順位としては
            //  Canvas(ScreenOverlayにあるもの)
            //    - SortingOrderが高いもの
            //    - 同じCanvasにある場合は、オブジェクト階層からみて末尾にあるもの
            // それ以外
            //    - Pointerの位置からのレイキャストで一番近いもの
            //
            // CanvasのScreenSpaceCameraとWorldSpaceのSortingOrderおよびSortingLayerについては無視します

            var input = ReplayableInput.Instance;
            input.IsReplaying = true;
            input.RecordedMousePos = new Vector2(Screen.width / 2f, Screen.height / 2f);

            //テスト自体は前にあるものから順にイベントを無効化していくものになります


            {// nearestScreenOverlay
                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(nearestScreenOverlay, root.SenderModel);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                eventDispatcherMap.Update(binderInstanceMap);
            }
            yield return null;

            {// sameCanvasScreenOverlay
                nearestScreenOverlay.AddLogicalID(disableEventID);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(nearestScreenOverlay, root.SenderModel);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                eventDispatcherMap.Update(binderInstanceMap);
            }
            yield return null;
            {// screenOverlay
                sameCanvasScreenOverlay.AddLogicalID(disableEventID);

                nearestScreenOverlay.AddLogicalID("disableEvent");
                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(nearestScreenOverlay, root.SenderModel);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                eventDispatcherMap.Update(binderInstanceMap);

            }
            yield return null;
            {// screenSpaceCamera
                screenOverlay.AddLogicalID(disableEventID);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(nearestScreenOverlay, root.SenderModel);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                eventDispatcherMap.Update(binderInstanceMap);

            }
            yield return null;
            {// worldCanvas
                screenSpaceCamera.AddLogicalID(disableEventID);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(nearestScreenOverlay, root.SenderModel);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                eventDispatcherMap.Update(binderInstanceMap);
            }
            yield return null;
            {// 3DObject
                worldCanvas.AddLogicalID(disableEventID);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(nearestScreenOverlay, root.SenderModel);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                eventDispatcherMap.Update(binderInstanceMap);
            }
        }

        [UnityTest]
        public IEnumerator OnPointerDownPasses()
        {
            string canvasViewID = "canvas";
            #region Construct Enviroment
            var mainCamera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
            mainCamera.tag = "MainCamera";

            var viewCreator = new UnityViewInstanceCreator()
                .AddUnityViewObjects()
                .AddPredicate(typeof(CubeViewObject), () => CubeViewObject.CreateInstance(), new EmptyModelViewParamBinder())
            ;
            var screenOverlayBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayout("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayout("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayout("offsetMin", Vector2.zero)
                    .AddViewLayout("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown,
                        new RecieverSelector(ModelRelationShip.Self, "", "")
                    ))
                );
            var binderMap = new ModelViewBinderMap(viewCreator
                , screenOverlayBinder
            ) {
                UseEventDispatcherMap = new EventDispatcherMap(
                    new PointerEventDispatcher()
                )
            };
            var root = new CheckOnPointerEventModel() { Name = "Root" };

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            binderInstanceMap.RootModel = root;
            #endregion

            yield return null;

            var input = ReplayableInput.Instance;
            input.IsReplaying = true;
            input.RecordedMousePresent = true;
            input.RecordedTouchSupported = true;
            input.RecordedMultiTouchEnabled = false;

            var screenCenterPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            input.RecordedMousePos = screenCenterPos;

            Debug.Log("start Mouse test");
            {//Mouse
                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Mouse, root.EventData.PointerType);
                Assert.AreEqual(-1, root.EventData.FingerID);
                Assert.AreEqual(input.MousePos, root.EventData.PointerPos);
                Assert.AreEqual(binderInstanceMap.BindInstances[root].ViewObjects.First(), root.EventData.PointerDownViewObject);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                root.Reset();
            }
            Debug.Log("start Touch test");
            {//Touch
                var t = new Touch()
                {
                    fingerId = 0,
                    position = screenCenterPos,
                    phase = TouchPhase.Began,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 1;
                input.SetRecordedTouch(0, t);

                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);
                Assert.AreEqual(binderInstanceMap.BindInstances[root].ViewObjects.First(), root.EventData.PointerDownViewObject);
                root.Reset();
                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
            }
            Debug.Log("start Touch Test2");
            {//Touch part2(Multiple touch)
                input.RecordedMultiTouchEnabled = true;

                var t = new Touch()
                {
                    fingerId = 1,
                    position = screenCenterPos,
                    phase = TouchPhase.Began,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 2;
                input.SetRecordedTouch(0, new Touch()
                {
                    fingerId = 0,
                    position = new Vector2(-100, -100), // <- touch out in screen
                    phase = TouchPhase.Began,
                });
                input.SetRecordedTouch(1, t);

                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);
                Assert.AreEqual(binderInstanceMap.BindInstances[root].ViewObjects.First(), root.EventData.PointerDownViewObject);
                root.Reset();
                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
            }
            {//Multiple Touch part
                input.RecordedMultiTouchEnabled = true;

                var t = new Touch()
                {
                    fingerId = 1,
                    position = screenCenterPos,
                    phase = TouchPhase.Began,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 2;
                input.SetRecordedTouch(0, t);
                input.SetRecordedTouch(1, new Touch()
                {
                    fingerId = 0,
                    position = screenCenterPos,
                    phase = TouchPhase.Began,
                });

                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);
                Assert.AreEqual(binderInstanceMap.BindInstances[root].ViewObjects.First(), root.EventData.PointerDownViewObject);
                root.Reset();
                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
            }
        }

        [UnityTest]
        public IEnumerator OnPointerUpPasses()
        {
            string canvasViewID = "canvas";
            #region Construct Enviroment
            var mainCamera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
            mainCamera.tag = "MainCamera";

            var viewCreator = new UnityViewInstanceCreator()
                .AddUnityViewObjects()
                .AddPredicate(typeof(CubeViewObject), () => CubeViewObject.CreateInstance(), new EmptyModelViewParamBinder())
            ;
            var screenOverlayBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayout("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayout("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayout("offsetMin", Vector2.zero)
                    .AddViewLayout("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerUp,
                        new RecieverSelector(ModelRelationShip.Self, "", "")
                    ))
                );
            var binderMap = new ModelViewBinderMap(viewCreator
                , screenOverlayBinder
            )
            {
                UseEventDispatcherMap = new EventDispatcherMap(
                    new PointerEventDispatcher()
                )
            };
            var root = new CheckOnPointerEventModel() { Name = "Root" };

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            binderInstanceMap.RootModel = root;
            #endregion

            yield return null;

            var input = ReplayableInput.Instance;
            input.IsReplaying = true;
            input.RecordedMousePresent = true;
            input.RecordedTouchSupported = true;
            input.RecordedMultiTouchEnabled = false;

            var screenCenterPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            input.RecordedMousePos = screenCenterPos;

            Debug.Log("start Mouse test");
            {//Mouse
                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Up);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Mouse, root.EventData.PointerType);
                Assert.AreEqual(-1, root.EventData.FingerID);
                Assert.AreEqual(input.MousePos, root.EventData.PointerPos);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                root.Reset();
            }
            Debug.Log("start Touch test");
            {//Touch
                var t = new Touch()
                {
                    fingerId = 0,
                    position = screenCenterPos,
                    phase = TouchPhase.Ended,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 1;
                input.SetRecordedTouch(0, t);

                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);
                root.Reset();
                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
            }
            Debug.Log("start Touch Test2");
            {//Touch part2(Multiple touch)
                input.RecordedMultiTouchEnabled = true;

                var t = new Touch()
                {
                    fingerId = 1,
                    position = screenCenterPos,
                    phase = TouchPhase.Ended,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 2;
                input.SetRecordedTouch(0, new Touch()
                {
                    fingerId = 0,
                    position = new Vector2(-100, -100), // <- touch out in screen
                    phase = TouchPhase.Ended,
                });
                input.SetRecordedTouch(1, t);

                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);
                root.Reset();
                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
            }
            {//Multiple Touch part
                input.RecordedMultiTouchEnabled = true;

                var t = new Touch()
                {
                    fingerId = 1,
                    position = screenCenterPos,
                    phase = TouchPhase.Ended,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 2;
                input.SetRecordedTouch(0, t);
                input.SetRecordedTouch(1, new Touch()
                {
                    fingerId = 0,
                    position = screenCenterPos,
                    phase = TouchPhase.Ended,
                });

                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);
                root.Reset();
                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
            }
        }

        [UnityTest]
        public IEnumerator OnPointerClickPasses()
        {
            string canvasViewID = "canvas";
            #region Construct Enviroment
            var mainCamera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
            mainCamera.tag = "MainCamera";

            var viewCreator = new UnityViewInstanceCreator()
                .AddUnityViewObjects()
                .AddPredicate(typeof(CubeViewObject), () => CubeViewObject.CreateInstance(), new EmptyModelViewParamBinder())
            ;
            var screenOverlayBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayout("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayout("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayout("offsetMin", Vector2.zero)
                    .AddViewLayout("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerClick,
                        new RecieverSelector(ModelRelationShip.Self, "", "")
                    ))
                );
            var binderMap = new ModelViewBinderMap(viewCreator
                , screenOverlayBinder
            )
            {
                UseEventDispatcherMap = new EventDispatcherMap(
                    new PointerEventDispatcher()
                )
            };
            var root = new CheckOnPointerEventModel() { Name = "Root" };

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();
            binderInstanceMap.RootModel = root;
            #endregion

            yield return null;

            var input = ReplayableInput.Instance;
            input.IsReplaying = true;
            input.RecordedMousePresent = true;
            input.RecordedTouchSupported = true;
            input.RecordedMultiTouchEnabled = false;

            var screenCenterPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            input.RecordedMousePos = screenCenterPos;

            Debug.Log("start Mouse test");
            {//Mouse
                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Up);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Mouse, root.EventData.PointerType);
                Assert.AreEqual(-1, root.EventData.FingerID);
                Assert.AreEqual(input.MousePos, root.EventData.PointerPos);
                Assert.AreEqual(binderInstanceMap.BindInstances[root].ViewObjects.First(), root.EventData.PointerDownViewObject);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                root.Reset();
            }
            Debug.Log("start Touch test");
            {//Touch
                var t = new Touch()
                {
                    fingerId = 0,
                    position = screenCenterPos,
                    phase = TouchPhase.Began,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 1;
                input.SetRecordedTouch(0, t);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);

                t.phase = TouchPhase.Ended;
                input.SetRecordedTouch(0, t);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);
                Assert.AreEqual(binderInstanceMap.BindInstances[root].ViewObjects.First(), root.EventData.PointerDownViewObject);

                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                root.Reset();
            }
            Debug.Log("start Touch Test2");
            {//Touch part2(Multiple touch)
                input.RecordedMultiTouchEnabled = true;

                var t = new Touch()
                {
                    fingerId = 1,
                    position = screenCenterPos,
                    phase = TouchPhase.Began,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 2;
                input.SetRecordedTouch(0, new Touch()
                {
                    fingerId = 0,
                    position = new Vector2(-100, -100), // <- touch out in screen
                    phase = TouchPhase.Began,
                });
                input.SetRecordedTouch(1, t);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);

                t.phase = TouchPhase.Ended;
                input.SetRecordedTouch(1, t);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);
                Assert.AreEqual(binderInstanceMap.BindInstances[root].ViewObjects.First(), root.EventData.PointerDownViewObject);

                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                root.Reset();
            }
            {//Multiple Touch part
                input.RecordedMultiTouchEnabled = true;

                var t = new Touch()
                {
                    fingerId = 1,
                    position = screenCenterPos,
                    phase = TouchPhase.Began,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 2;
                input.SetRecordedTouch(0, t);
                input.SetRecordedTouch(1, new Touch()
                {
                    fingerId = 0,
                    position = screenCenterPos,
                    phase = TouchPhase.Began,
                });

                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);

                t.phase = TouchPhase.Ended;
                input.SetRecordedTouch(0, t); // <- こちらが使用される
                input.SetRecordedTouch(1, t);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);
                Assert.AreEqual(binderInstanceMap.BindInstances[root].ViewObjects.First(), root.EventData.PointerDownViewObject);

                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                root.Reset();
            }
        }

        [UnityTest]
        public IEnumerator OnPointerEnterPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator OnPointerStatinaryPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator OnPointerExitPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator OnPointerBeginDragPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator OnPointerDragPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator OnPointerEndDragPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator OnPointerDropPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

    }
}
