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

        class OnPointerEventModel : Model
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
            public void OnPointerBeginDrag(Model sender, OnPointerEventData eventData)
            {
            }

            public void OnPointerClick(Model sender, OnPointerEventData eventData)
            {
            }

            public void OnPointerDown(Model sender, OnPointerEventData eventData)
            {
            }

            public void OnPointerDrag(Model sender, OnPointerEventData eventData)
            {
            }

            public void OnPointerDrop(Model sender, OnPointerEventData eventData)
            {
            }

            public void OnPointerEndDrag(Model sender, OnPointerEventData eventData)
            {
            }

            public void OnPointerEnter(Model sender, OnPointerEventData eventData)
            {
            }

            public void OnPointerExit(Model sender, OnPointerEventData eventData)
            {
            }

            public void OnPointerUp(Model sender, OnPointerEventData eventData)
            {
            }
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
                .AddEnabledModelType<OnPointerEventModel>();
            var otherBinder = new ModelViewBinder("*", null,
                    new ModelViewBinder.BindInfo(typeof(EmptyViewObject))
                );
            var binderMap = new ModelViewBinderMap(viewInstanceCreator,
                pointerEventModelBinder, otherBinder);
            binderMap.UseEventDispatcherMap = new EventDispatcherMap(
                new PointerEventDispatcher()
            );
            var binderInstanceMap = binderMap.CreateBinderInstaceMap();

            var model = new OnPointerEventModel();
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

        [UnityTest]
        public IEnumerator OnPointerDownPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator OnPointerUpPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
        }

        [UnityTest]
        public IEnumerator OnPointerClickPasses()
        {
            yield return null;
            throw new System.NotImplementedException();
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
