using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.Events.Pointer
{
    /// <summary>
    /// <seealso cref="PointerEventDispatcher"/>
    /// </summary>
    public class TestPointerEventDispatcher : TestBase
    {
        [SetUp]
        public void Setup()
        {
            PackagePath = PackageDefines.PACKAGE_ASSET_ROOT_PATH;
            Logger.PriorityLevel = Logger.Priority.Debug;
        }

        [Test]
        public void CheckEventInfoDefinationPasses()
        {
            var senderGroup = new PointerEventDispatcher();

            var eventInfo = senderGroup.EventInfos;

            var checkList = new (PointerEventName, System.Type eventHandlerType)[]
            {
                (PointerEventName.onPointerDown, typeof(IOnPointerDownReciever)),
                (PointerEventName.onPointerUp, typeof(IOnPointerUpReciever)),
                (PointerEventName.onPointerClick, typeof(IOnPointerClickReciever)),
                (PointerEventName.onPointerEnter, typeof(IOnPointerEnterReciever)),
                (PointerEventName.onPointerInArea, typeof(IOnPointerInAreaReciever)),
                (PointerEventName.onPointerExit, typeof(IOnPointerExitReciever)),
                (PointerEventName.onPointerBeginDrag, typeof(IOnPointerBeginDragReciever)),
                (PointerEventName.onPointerDrag, typeof(IOnPointerDragReciever)),
                (PointerEventName.onPointerEndDrag, typeof(IOnPointerEndDragReciever)),
                (PointerEventName.onPointerDrop, typeof(IOnPointerDropReciever))
            };

            foreach (var (eventName, eventHandlerType) in checkList)
            {
                Assert.IsTrue(eventInfo.ContainKeyword(eventName), $"Invalid {eventName}...");
                Assert.AreEqual(eventHandlerType, eventInfo.GetEventHandlerType(eventName), $"Invalid {eventName}...");
                Assert.IsTrue(eventInfo.DoEnabledEvent(eventName), $"Invalid {eventName}...");
            }
        }

        [Test]
        public void DoEnabledPasses()
        {
            var input = ReplayableInput.Instance;
            input.IsReplaying = true;
            var pointerEventDispatcher = new PointerEventDispatcher();

            var testData = new (bool result, bool mousePresent, bool touchSupported)[]
            {
                (true, true, true),
                (true, true, false),
                (true, false, true),
                (false, false, false)
            };
            foreach (var (result, mousePresent, touchSupported) in testData)
            {
                var errorMessage = $"Fail When (mousePresent,touchSupported)=({mousePresent}, {touchSupported})...";
                input.RecordedMousePresent = mousePresent;
                input.RecordedTouchSupported = touchSupported;

                pointerEventDispatcher.DoEnabled = true;
                if (result)
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
            , IOnPointerInAreaReciever
            , IOnPointerExitReciever
            , IOnPointerBeginDragReciever
            , IOnPointerDragReciever
            , IOnPointerEndDragReciever
            , IOnPointerDropReciever
        {
            public int RecievedEventNameValue { get; set; } = -1;
            public Model SenderModel { get; set; }
            public IOnPointerEventData EventData { get; set; }

            public Model DropSenderModel { get; set; }
            public IOnPointerEventData DropEventData { get; set; }

            public void Reset()
            {
                RecievedEventNameValue = -1;
                SenderModel = null;
                EventData = null;

                DropSenderModel = null;
                DropEventData = null;
            }

            void Set(PointerEventName recievedEventName, Model sender, IOnPointerEventData eventData)
            {
                RecievedEventNameValue = (int)recievedEventName;
                SenderModel = sender;
                EventData = eventData;
            }

            public void OnPointerBeginDrag(Model sender, IOnPointerEventData eventData) => Set(PointerEventName.onPointerBeginDrag, sender, eventData);
            public void OnPointerClick(Model sender, IOnPointerEventData eventData) => Set(PointerEventName.onPointerClick, sender, eventData);
            public void OnPointerDown(Model sender, IOnPointerEventData eventData) => Set(PointerEventName.onPointerDown, sender, eventData);
            public void OnPointerDrag(Model sender, IOnPointerEventData eventData) => Set(PointerEventName.onPointerDrag, sender, eventData);
            public void OnPointerEndDrag(Model sender, IOnPointerEventData eventData) => Set(PointerEventName.onPointerEndDrag, sender, eventData);
            public void OnPointerEnter(Model sender, IOnPointerEventData eventData) => Set(PointerEventName.onPointerEnter, sender, eventData);
            public void OnPointerInArea(Model sender, IOnPointerEventData eventData) => Set(PointerEventName.onPointerInArea, sender, eventData);
            public void OnPointerExit(Model sender, IOnPointerEventData eventData) => Set(PointerEventName.onPointerExit, sender, eventData);
            public void OnPointerUp(Model sender, IOnPointerEventData eventData) => Set(PointerEventName.onPointerUp, sender, eventData);

            public void OnPointerDrop(Model sender, IOnPointerEventData eventData)
            {
                DropSenderModel = sender;
                DropEventData = eventData;
            }
        }

        class TestOnPointerEventViewObj : MonoBehaviourViewObject
        {

        }

        [UnityTest, Description("ModelViewInstanceでIOnPointerEventEventDispathcerHelpObjectを作成しているか確認するためのテスト")]
        public IEnumerator CreatePointerEventEventDispathcerHelpObjectPasses()
        {
            yield return null;
            #region Initial Enviroment

            var viewInstanceCreator = new UnityViewInstanceCreator()
                .AddPredicate(typeof(TestOnPointerEventViewObj), () =>
                {
                    var obj = new GameObject("__onPointerEventViewObj");
                    return obj.AddComponent<TestOnPointerEventViewObj>();
                }, new EmptyModelViewParamBinder())
                .AddPredicate(typeof(EmptyViewObject), () =>
                {
                    return new EmptyViewObject();
                }, new EmptyModelViewParamBinder())
            ;

            var pointerEventModelBinder = new ModelViewBinder("*", null,
                    new ModelViewBinder.BindInfo(typeof(TestOnPointerEventViewObj))
                        .AddControllerInfo(
                            new ControllerInfo(PointerEventName.onPointerDown,
                            new EventHandlerSelector(ModelRelationShip.Self, "", "")
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
                Assert.IsTrue((bool)binderInstance.HasEventDispathcerHelpObject<IOnPointerEventHelpObject>(viewObj));
                Assert.IsTrue(viewObj.HasEventDispatcherHelpObject<IOnPointerEventHelpObject>());
                Assert.IsNotNull((object)binderInstance.GetEventDispathcerHelpObject<IOnPointerEventHelpObject>(viewObj));
                Assert.IsNotNull(viewObj.GetEventDispathcerHelpObject<IOnPointerEventHelpObject>());
                Assert.AreSame(
                    binderInstance.GetEventDispathcerHelpObject<IOnPointerEventHelpObject>(viewObj),
                    viewObj.GetEventDispathcerHelpObject<IOnPointerEventHelpObject>());
            }

            {//ない時
                var binderInstance = binderInstanceMap.BindInstances[child];
                var viewObj = binderInstance.ViewObjects.First();
                Assert.IsFalse((bool)binderInstance.HasEventDispathcerHelpObject<IOnPointerEventHelpObject>(viewObj));
                Assert.IsFalse(viewObj.HasEventDispatcherHelpObject<IOnPointerEventHelpObject>());

                Assert.IsNull((object)binderInstance.GetEventDispathcerHelpObject<IOnPointerEventHelpObject>(viewObj));
                Assert.IsNull(viewObj.GetEventDispathcerHelpObject<IOnPointerEventHelpObject>());
            }
        }

        class PointerPriorityOrderPassesModel : Model
            , IOnPointerDownReciever
        {
            public Model SenderModel { get; set; }
            public void Reset()
            {
                SenderModel = null;
            }

            public void OnPointerDown(Model sender, IOnPointerEventData eventData)
            {
                SenderModel = sender;
            }
        }

        [UnityTest, Description("オブジェクトが重なっている場合にイベントが発生される優先順位の確認")]
        public IEnumerator PointerPriorityOrderPasses()
        {
            //NOTO: たまにテストに失敗するが、原因不明 WorldCanvas周りで失敗していたので、単純にカーソルが当たり判定から外れていたからかもしれない。
            //      それとも、以前のテストでシーンに残ったオブジェクトが当たり判定の邪魔をしているかもしれない。

            //var stackFrame = new System.Diagnostics.StackFrame();
            //TakeOrValid(100, stackFrame, 0, (_, __) => true, "This Snapshot is for Check Scene.");

            string screenOverlayQuery = "ScreenOverlay";
            string nearestScreenOverlayQuery = "NearestScreenOverlay";
            string sameCanvasScreenOverlayQuery = "SameCanvasScreenOverlay";
            string screenSpaceCameraQuery = "ScreenSpaceCamera";
            string worldCanvasQuery = "WorldCanvas";
            string object3DQuery = "3DObject";
            string canvasViewID = "canvas";

            string disableEventID = "#disableEvents";
            #region Construct Enviroment
            var mainCamera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
            mainCamera.tag = "MainCamera";

            var parentSelector = new EventHandlerSelector(ModelRelationShip.Parent, "", "");

            // 画面上に以下のViewがあるようにしています。
            // - ScreenOverlayCanvas
            //   - screenOverlay
            // - ScreenSpaceCameraCanvas
            //   - screenSpaceCamera
            // - WorldCanvas
            //   - world
            // - Transform (3DObject)
            var viewCreator = new UnityViewInstanceCreator()
                .AddUnityViewObjects();

            var nearestCameraDistance = 10f;
            var middleCameraDistance = 20f;
            var farestCameraDistance = 100f;

            var nearestScreenOverlayBinder = new ModelViewBinder(nearestScreenOverlayQuery, null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay,
                        SortingOrder = 10,
                    })
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown, parentSelector))
                );

            var screenOverlayBinder = new ModelViewBinder(screenOverlayQuery, null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown, parentSelector))
                );

            var sameCanvasScreenOverlayBinder = new ModelViewBinder(sameCanvasScreenOverlayQuery, null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .AddViewLayoutValue(TransformViewLayoutName.parent, new ModelViewSelector(ModelRelationShip.Parent, "", canvasViewID))
                    .AddViewLayoutValue("anchorMin", Vector2.one * (0.5f - 0.08f))
                    .AddViewLayoutValue("anchorMax", Vector2.one * (0.5f + 0.08f))
                    .AddViewLayoutValue("offsetMin", Vector2.zero)
                    .AddViewLayoutValue("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown,
                        new EventHandlerSelector(ModelRelationShip.Parent, "Root", "")))
                );

            var screenSpaceCameraBinder = new ModelViewBinder(screenSpaceCameraQuery, null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceCamera,
                        WorldCamera = mainCamera,
                        PlaneDistance = nearestCameraDistance,
                    })
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown, parentSelector))
                );

            var worldCanvasPos = mainCamera.transform.position + mainCamera.transform.forward * middleCameraDistance;
            var worldCanvasBinder = new ModelViewBinder(worldCanvasQuery, null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.WorldSpace,
                        WorldCamera = mainCamera,
                    })
                    .AddViewLayoutValue(TransformViewLayoutName.pos, worldCanvasPos)
                    .AddViewLayoutValue(RectTransformViewLayoutName.size, new Vector2(200, 200))
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown, parentSelector))
                );

            var object3DPos = mainCamera.transform.position + mainCamera.transform.forward * farestCameraDistance;
            var obj3DBinder = new ModelViewBinder(object3DQuery, null
                , new ModelViewBinder.BindInfo(typeof(CubeViewObject))
                    .AddViewLayoutValue(TransformViewLayoutName.pos, object3DPos)
                    .AddViewLayoutValue(TransformViewLayoutName.localScale, new Vector3(40f, 40f, 1))
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown, parentSelector))
                );

            var binderMap = new ModelViewBinderMap(viewCreator
                , nearestScreenOverlayBinder
                , screenOverlayBinder
                , sameCanvasScreenOverlayBinder
                , screenSpaceCameraBinder
                , worldCanvasBinder
                , obj3DBinder
                )
            {
                UseViewLayouter = new ViewLayouter()
                    .AddBasicViewLayouter()
                    .AddTransformKeywordsAndAutoCreator()
                    .AddRectTransformKeywordsAndAutoCreator(),
                UseEventDispatcherMap = new EventDispatcherMap(
                    new PointerEventDispatcher()
                ),
                UseEventDispatchStateMap = new EventDispatchStateMap()
                    .AddState(EventDispatchStateName.disable, new EventDispatchQuery(disableEventID, ""))
            };
            var root = new PointerPriorityOrderPassesModel() { Name = "Root" };
            var nearestScreenOverlay = new Model() { Name = nearestScreenOverlayQuery };
            var screenOverlay = new Model() { Name = screenOverlayQuery };
            var sameCanvasScreenOverlay = new Model() { Name = sameCanvasScreenOverlayQuery };
            var screenSpaceCamera = new Model() { Name = screenSpaceCameraQuery };
            var worldCanvas = new Model() { Name = worldCanvasQuery };
            var object3D = new Model() { Name = object3DQuery };

            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            root.AddChildren(
                nearestScreenOverlay,
                screenOverlay,
                screenSpaceCamera,
                worldCanvas,
                object3D);
            screenOverlay.AddChildren(sameCanvasScreenOverlay);
            binderInstanceMap.RootModel = root;

            #endregion
            yield return null;
            yield return null;
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
            input.RecordedMousePresent = true;
            input.RecordedMousePos = new Vector2(Screen.width / 2f, Screen.height / 2f);

            //テスト自体は前にあるものから順にイベントを無効化していくものになります


            {// nearestScreenOverlay
                root.Reset();
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
                root.Reset();
                nearestScreenOverlay.AddLogicalID(disableEventID);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(sameCanvasScreenOverlay, root.SenderModel);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                eventDispatcherMap.Update(binderInstanceMap);
            }
            yield return null;
            {// screenOverlay
                root.Reset();

                sameCanvasScreenOverlay.AddLogicalID(disableEventID);

                nearestScreenOverlay.AddLogicalID("disableEvent");
                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(screenOverlay, root.SenderModel);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                eventDispatcherMap.Update(binderInstanceMap);

            }
            yield return null;
            {// screenSpaceCamera
                root.Reset();

                screenOverlay.AddLogicalID(disableEventID);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(screenSpaceCamera, root.SenderModel);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                eventDispatcherMap.Update(binderInstanceMap);

            }
            yield return null;
            {// worldCanvas
                root.Reset();

                screenSpaceCamera.AddLogicalID(disableEventID);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(worldCanvas, root.SenderModel);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Free);
                eventDispatcherMap.Update(binderInstanceMap);
            }
            yield return null;
            {// 3DObject
                root.Reset();
                worldCanvas.AddLogicalID(disableEventID);

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
                var eventDispatcherMap = binderInstanceMap.UseEventDispatcherMap;
                eventDispatcherMap.Update(binderInstanceMap);
                eventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(object3D, root.SenderModel);

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
            ;
            var screenOverlayBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayoutValue("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayoutValue("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayoutValue("offsetMin", Vector2.zero)
                    .AddViewLayoutValue("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDown,
                        new EventHandlerSelector(ModelRelationShip.Self, "", "")
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
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual((int)PointerEventName.onPointerDown, root.RecievedEventNameValue);
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
            Debug.Log("start Multi Touch Test");
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
            ;
            var screenOverlayBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayoutValue("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayoutValue("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayoutValue("offsetMin", Vector2.zero)
                    .AddViewLayoutValue("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerUp,
                        new EventHandlerSelector(ModelRelationShip.Self, "", "")
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
            Debug.Log("start Multi Touch Test");
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
            ;
            var screenOverlayBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayoutValue("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayoutValue("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayoutValue("offsetMin", Vector2.zero)
                    .AddViewLayoutValue("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerClick,
                        new EventHandlerSelector(ModelRelationShip.Self, "", "")
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
            Debug.Log("start Multi Touch Test");
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
            string canvasViewID = "canvas";
            #region Construct Enviroment
            var mainCamera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
            mainCamera.tag = "MainCamera";

            var viewCreator = new UnityViewInstanceCreator()
                .AddUnityViewObjects()
            ;
            var screenOverlayBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayoutValue("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayoutValue("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayoutValue("offsetMin", Vector2.zero)
                    .AddViewLayoutValue("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerEnter,
                        new EventHandlerSelector(ModelRelationShip.Self, "", "")
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
            Debug.Log("start Multi Touch Test");
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

                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                root.Reset();
            }
        }

        [UnityTest]
        public IEnumerator OnPointerStatinaryPasses()
        {
            string canvasViewID = "canvas";
            #region Construct Enviroment
            var mainCamera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
            mainCamera.tag = "MainCamera";

            var viewCreator = new UnityViewInstanceCreator()
                .AddUnityViewObjects()
            ;
            var screenOverlayBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayoutValue("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayoutValue("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayoutValue("offsetMin", Vector2.zero)
                    .AddViewLayoutValue("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerInArea,
                        new EventHandlerSelector(ModelRelationShip.Self, "", "")
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

                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Mouse, root.EventData.PointerType);
                Assert.AreEqual(-1, root.EventData.FingerID);
                Assert.AreEqual(input.MousePos, root.EventData.PointerPos);

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

                t.phase = TouchPhase.Moved;
                input.SetRecordedTouch(0, t);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);

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

                t.phase = TouchPhase.Stationary;
                input.SetRecordedTouch(1, t);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);

                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                root.Reset();
            }
            Debug.Log("start Multi Touch Test");
            {//Multiple Touch part
                input.RecordedMultiTouchEnabled = true;

                var t = new Touch()
                {
                    fingerId = 1,
                    position = screenCenterPos,
                    phase = TouchPhase.Began,
                };
                var t2 = new Touch()
                {
                    fingerId = 1,
                    position = screenCenterPos + Vector3.one * 10f,
                    phase = TouchPhase.Began,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 2;
                input.SetRecordedTouch(0, t);
                input.SetRecordedTouch(1, t2);

                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);

                t.phase = t2.phase = TouchPhase.Moved;
                input.SetRecordedTouch(0, t); // <- こちらが使用される
                input.SetRecordedTouch(1, t2);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);

                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                root.Reset();
            }
        }

        [UnityTest]
        public IEnumerator OnPointerExitPasses()
        {
            string canvasViewID = "canvas";
            #region Construct Enviroment
            var mainCamera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
            mainCamera.tag = "MainCamera";

            var viewCreator = new UnityViewInstanceCreator()
                .AddUnityViewObjects()
            ;
            var screenOverlayBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayoutValue("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayoutValue("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayoutValue("offsetMin", Vector2.zero)
                    .AddViewLayoutValue("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerExit,
                        new EventHandlerSelector(ModelRelationShip.Self, "", "")
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
                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                input.RecordedMousePos = screenCenterPos + Vector3.one * -100000f;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Mouse, root.EventData.PointerType);
                Assert.AreEqual(-1, root.EventData.FingerID);
                Assert.AreEqual(input.MousePos, root.EventData.PointerPos);

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

                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                root.Reset();
            }
            Debug.Log("start Multi Touch Test");
            {//Multiple Touch part
                input.RecordedMultiTouchEnabled = true;

                var t = new Touch()
                {
                    fingerId = 1,
                    position = screenCenterPos,
                    phase = TouchPhase.Began,
                };
                var t2 = new Touch()
                {
                    fingerId = 1,
                    position = screenCenterPos + Vector3.one * 10f,
                    phase = TouchPhase.Began,
                };
                input.RecordedTouchSupported = true;
                input.RecordedTouchCount = 2;
                input.SetRecordedTouch(0, t);
                input.SetRecordedTouch(1, t2);

                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);

                t.phase = t2.phase = TouchPhase.Ended;
                input.SetRecordedTouch(0, t); // <- こちらが使用される
                input.SetRecordedTouch(1, t2);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap);

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual(PointerType.Touch, root.EventData.PointerType);
                Assert.AreEqual(t.fingerId, root.EventData.FingerID);
                Assert.AreEqual((Vector3)t.position, root.EventData.PointerPos);

                input.RecordedTouchCount = 0;
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                root.Reset();
            }
        }

        [UnityTest]
        public IEnumerator DragPasses()
        {
            string canvasViewID = "canvas";
            #region Construct Enviroment
            var mainCamera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
            mainCamera.tag = "MainCamera";

            var viewCreator = new UnityViewInstanceCreator()
                .AddUnityViewObjects()
            ;
            var screenOverlayBinder = new ModelViewBinder("*", null
                , new ModelViewBinder.BindInfo(canvasViewID, typeof(CanvasViewObject))
                    .SetUseParamBinder(new CanvasViewObject.FixedParamBinder()
                    {
                        RenderMode = RenderMode.ScreenSpaceOverlay
                    })
                    .AddViewLayoutValue("anchorMin", Vector2.one * (0.5f - 0.1f))
                    .AddViewLayoutValue("anchorMax", Vector2.one * (0.5f + 0.1f))
                    .AddViewLayoutValue("offsetMin", Vector2.zero)
                    .AddViewLayoutValue("offsetMax", Vector2.zero)
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerBeginDrag,
                        new EventHandlerSelector(ModelRelationShip.Self, "", "")
                    ))
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDrag,
                        new EventHandlerSelector(ModelRelationShip.Self, "", "")
                    ))
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerEndDrag,
                        new EventHandlerSelector(ModelRelationShip.Self, "", "")
                    ))
                    .AddControllerInfo(new ControllerInfo(PointerEventName.onPointerDrop,
                        new EventHandlerSelector(ModelRelationShip.Self, "", "")
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

            var beginPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            var endPos = new Vector3(Screen.width, Screen.height / 2f, 0f);
            input.RecordedMousePos = beginPos;
            input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Down);
            binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);

            Debug.Log("test BeginDrag");
            {//BeginDrag

                input.RecordedMousePos = Vector3.Lerp(beginPos, endPos, 0.25f);
                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);
                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap); // occur BeginDrag

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual((int)PointerEventName.onPointerBeginDrag, root.RecievedEventNameValue);
                Assert.IsTrue(root.EventData.IsDrag);
                Assert.IsFalse(root.EventData.IsStationary);
                Assert.AreEqual(binderInstanceMap.BindInstances[root].ViewObjects.First(), root.EventData.PointerDownViewObject);
                root.Reset();
            }
            {//Drag
                for (float t = 0.5f; t < 1f; t += 0.1f)
                {
                    Debug.Log("test Drag");
                    input.RecordedMousePos = Vector3.Lerp(beginPos, endPos, t);
                    input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Push);

                    binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                    binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap); // occur Drag

                    Assert.AreSame(root, root.SenderModel);
                    Assert.AreEqual((int)PointerEventName.onPointerDrag, root.RecievedEventNameValue);
                    Assert.IsTrue(root.EventData.IsDrag);
                    Assert.IsFalse(root.EventData.IsStationary);

                    root.Reset();
                }
            }
            Debug.Log("test EndDrag and Drop");
            {//EndDrag and Drop
                input.RecordedMousePos = endPos;
                input.SetRecordedMouseButton(InputDefines.MouseButton.Left, InputDefines.ButtonCondition.Up);

                binderInstanceMap.UseEventDispatcherMap.Update(binderInstanceMap);
                binderInstanceMap.UseEventDispatcherMap.SendTo(binderInstanceMap); // occur EndDrag and Drop

                Assert.AreSame(root, root.SenderModel);
                Assert.AreEqual((int)PointerEventName.onPointerEndDrag, root.RecievedEventNameValue);
                Assert.IsTrue(root.EventData.IsDrag);

                Assert.AreSame(root, root.DropSenderModel);
                Assert.IsTrue(root.DropEventData.IsDrag);

                root.Reset();
            }
        }
    }
}
