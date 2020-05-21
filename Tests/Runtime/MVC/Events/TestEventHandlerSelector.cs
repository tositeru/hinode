using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
    /// <seealso cref="EventHandlerSelector"/>
    /// </summary>
    public class TestRecieverSelector
    {
        class EventHandlerViewObj : EmptyViewObject, IEventHandler
        {
            #region IViewObject
            #endregion

            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        class RecieverModel : Model, IEventHandler
        {
        }

        class NoneRecieverModel : Model
        { }

        [Test]
        public void ParentSelectorPasses()
        {
            #region Consturct Enviroment
            //Model Hierarchy
            // - root: #main type=RecieverModel
            //   - reciever: #main type=RecieverModel
            //   - noneReciever: #main type=NoneRecieverModel
            //     - model: #main type=Model
            //View info
            // #main:
            //   - SenderViewObj: ID=SenderViewObj, InstanceID=SenderViewObj, BinderID=SenderViewObj
            //   - EventHandlerViewObj: ID=reciever, InstanceID=SenderViewObj, BinderID=SenderViewObj
            var root = new RecieverModel() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };
            var model1 = new Model() { Name = "model", Parent = noneRecieverModel, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(EventHandlerViewObj), new EventHandlerViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(viewReciever, typeof(EventHandlerViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//IEventHandlerを継承しているModelを取得しようとした時のテスト
                //search root model => reciever
                //selector: Parent, "", ""
                //result => root
                var selector = new EventHandlerSelector(ModelRelationShip.Parent, "", "");
                var enumerable = selector.GetEventHandlerEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "IEventHandlerを継承している時は取得できるようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(root, enumerable.First(), errorMessage);
            }

            {//ParentがnullなModelのParentを指定した時のテスト
                //search root model => root
                //selector: Parent, "", ""
                //result => (empty)
                var selector = new EventHandlerSelector(ModelRelationShip.Parent, "", "");
                var enumerable = selector.GetEventHandlerEnumerable(root, binderMapInstance);

                var errorMessage = "ParentがないmodelのParentを指定した時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//IEventHandlerを継承していないModelを取得しようとした時のテスト
                //search root model => model
                //selector: Parent, "", ""
                //result => (empty)
                var selector = new EventHandlerSelector(ModelRelationShip.Parent, "", "");
                var enumerable = selector.GetEventHandlerEnumerable(model1, binderMapInstance);
                var errorMessage = "IEventHandlerを継承していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//ParentがnullなModelのParentを指定した時のテスト2
                //search root model => model
                //selector: Parent, "root", ""
                //result => root
                var selector = new EventHandlerSelector(ModelRelationShip.Parent, "#main", "");
                var enumerable = selector.GetEventHandlerEnumerable(model1, binderMapInstance);

                var errorMessage = "親指定のrecieverSelectorの時、クエリパスを指定した時はBinderInstanceMapのRootModelをクエリルートにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(root, enumerable.First(), errorMessage);
            }

            {//IEventHandlerを継承しているViewを取得しようとした時のテスト
                //search root model => reciever
                //selector: Parent, "", "reciever"
                //result => EventHandlerViewObj in root
                var selector = new EventHandlerSelector(ModelRelationShip.Parent, "", viewReciever);
                var enumerable = selector.GetEventHandlerEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "Viewを指定した時はModelにバインドされていて、かつIEventHandlerを継承しているViewを取得できるようにしてください。";
                var viewObj = binderMapInstance[root].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(viewObj, enumerable.First(), errorMessage);
            }

            {//IEventHandlerを継承しているViewを取得しようとした時のテスト2
                //search root model => model
                //selector: Parent, "", "reciever"
                //result => EventHandlerViewObj in noneReciever
                var selector = new EventHandlerSelector(ModelRelationShip.Parent, "", viewReciever);
                var enumerable = selector.GetEventHandlerEnumerable(model1, binderMapInstance);

                var errorMessage = "バインドされているModelがIEventHandlerを継承していない場合でも取得できるようにしてください";
                var viewObj = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(viewObj, enumerable.First(), errorMessage);
            }

            {//IEventHandlerを継承しているViewを取得しようとした時のテスト3(一致しないViewID)
                //search root model => model
                //selector: Parent, "", "invalidIdentity"
                //result => (empty)
                var selector = new EventHandlerSelector(ModelRelationShip.Parent, "", "invalidIdentity");
                var enumerable = selector.GetEventHandlerEnumerable(model1, binderMapInstance);

                var errorMessage = "ViewIdentityが一致していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//QueryPathとViewIdentityを指定した時のテスト
                //search root model => model
                //selector: Parent, "root", "reciever"
                //result => root
                var selector = new EventHandlerSelector(ModelRelationShip.Parent, "#main", viewReciever);
                var enumerable = selector.GetEventHandlerEnumerable(model1, binderMapInstance);

                var errorMessage = "親指定のrecieverSelectorの時、クエリパスを指定した時はBinderInstanceMapのRootModelをクエリルートにしてください。";
                var rootViewObj = binderMapInstance[root].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                var parentViewObj = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(2, enumerable.Count(), errorMessage);
                AssertionUtils.AssertEnumerable(enumerable, new IViewObject[] { rootViewObj, parentViewObj }.OfType<IEventHandler>(), errorMessage);

            }
        }

        [Test]
        public void ChildSelectorPasses()
        {
            #region Consturct Enviroment
            //Model Hierarchy
            // - root: #main type=RecieverModel
            //   - reciever: #main type=RecieverModel
            //   - noneReciever: #main type=NoneRecieverModel
            //     - reciever2: #main type=RecieverModel
            //View info
            // #main:
            //   - SenderViewObj: ID=SenderViewObj, InstanceID=SenderViewObj, BinderID=SenderViewObj
            //   - EventHandlerViewObj: ID=reciever, InstanceID=SenderViewObj, BinderID=SenderViewObj
            var root = new RecieverModel() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };
            var recieverModel2 = new RecieverModel() { Name = "reciever2", Parent = noneRecieverModel, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(EventHandlerViewObj), new EventHandlerViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(viewReciever, typeof(EventHandlerViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//IEventHandlerを継承しているModelを取得しようとした時のテスト
                //search root model => root
                //selector: Child, "", ""
                //result => reciever
                var selector = new EventHandlerSelector(ModelRelationShip.Child, "", "");
                var enumerable = selector.GetEventHandlerEnumerable(root, binderMapInstance);

                var errorMessage = "IEventHandlerを継承している時は取得できるようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(recieverModel, enumerable.First(), errorMessage);
            }

            {//Childがない時のModelのChildを指定した時のテスト
                //search root model => reciever
                //selector: Child, "", ""
                //result => (empty)
                var selector = new EventHandlerSelector(ModelRelationShip.Child, "", "");
                var enumerable = selector.GetEventHandlerEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "ChildがないmodelのChildを指定した時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//QueryPathを指定した時のテスト
                //search root model => root
                //selector: Child, "*", ""
                //result => reciever, reciever2
                var selector = new EventHandlerSelector(ModelRelationShip.Child, "*", "");
                var enumerable = selector.GetEventHandlerEnumerable(root, binderMapInstance);
                var errorMessage = "QueryPathを指定した時はそれに一致するModel全て取得できるようにしてください";
                AssertionUtils.AssertEnumerable(enumerable, new IEventHandler[] { recieverModel, recieverModel2 }, errorMessage);
            }

            {//一致しないQueryPathを指定した時のテスト
                //search root model => root
                //selector: Child, "__invalid", ""
                //result => (empty)
                var selector = new EventHandlerSelector(ModelRelationShip.Child, "__invalid", "");
                var enumerable = selector.GetEventHandlerEnumerable(root, binderMapInstance);
                var errorMessage = "QueryPathが一致しない時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//IEventHandlerを継承しているViewを取得しようとした時のテスト
                //search root model => root
                //selector: Child, "", "reciever"
                //result => EventHandlerViewObj in reciever, EventHandlerViewObj in noneReciever
                var selector = new EventHandlerSelector(ModelRelationShip.Child, "", viewReciever);
                var enumerable = selector.GetEventHandlerEnumerable(root, binderMapInstance);

                var errorMessage = "子モデルのViewを指定した時はModelにバインドされていて、かつIEventHandlerを継承しているViewを全て取得できるようにしてください。";
                var viewObj1 = binderMapInstance[recieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                var viewObj2 = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                AssertionUtils.AssertEnumerable(enumerable, (new IViewObject[] { viewObj1, viewObj2 }).OfType<IEventHandler>(), errorMessage);
            }

            {//IEventHandlerを継承しているViewを取得しようとした時のテスト(一致しないViewID)
                //search root model => root
                //selector: Child, "", "__invalid"
                //result => (empty)
                var selector = new EventHandlerSelector(ModelRelationShip.Child, "", "__invalid");
                var enumerable = selector.GetEventHandlerEnumerable(root, binderMapInstance);

                var errorMessage = "指定したViewIdentityと一致ない時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//IEventHandlerを継承しているViewを取得しようとした時のテスト(クエリパスを指定した時)
                //search root model => root
                //selector: Child, "reciever", "reciever"
                //result => EventHandlerViewObj in reciever, 
                var selector = new EventHandlerSelector(ModelRelationShip.Child, recieverModel.Name, viewReciever);
                var enumerable = selector.GetEventHandlerEnumerable(root, binderMapInstance);

                var errorMessage = "クエリパスを伴う子モデルのViewを指定した時はクエリパスに一致しModelにバインドされていて、かつIEventHandlerを継承しているViewを全て取得できるようにしてください。";
                var viewObj1 = binderMapInstance[recieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                AssertionUtils.AssertEnumerable(enumerable, (new IViewObject[] { viewObj1 }).OfType<IEventHandler>(), errorMessage);
            }

            {//IEventHandlerを継承しているViewを取得しようとした時のテスト(クエリパスを指定し、ViewIDが一致しない時)
                //search root model => root
                //selector: Child, "reciever", "__invalid"
                //result => (empty)
                var selector = new EventHandlerSelector(ModelRelationShip.Child, recieverModel.Name, "__invalid");
                var enumerable = selector.GetEventHandlerEnumerable(root, binderMapInstance);

                var errorMessage = "指定したViewIdentityと一致ない時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }
        }

        [Test]
        public void SelfSelectorPasses()
        {
            #region Construct Enviroment 
            //Model Hierarchy
            // - root: #main type=Model
            //   - reciever: #main type=RecieverModel
            //   - noneReciever: #main type=NoneRecieverModel
            //
            //View info
            // #main:
            //   - SenderViewObj: ID=SenderViewObj, InstanceID=SenderViewObj, BinderID=SenderViewObj
            //   - EventHandlerViewObj: ID=reciever, InstanceID=SenderViewObj, BinderID=SenderViewObj
            var root = new Model() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(EventHandlerViewObj), new EventHandlerViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(viewReciever, typeof(EventHandlerViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//IEventHandlerを継承しているModelを取得しようとした時のテスト
                //search root model => reciever
                //selector: Self, "", ""
                //result => reciever
                var selector = new EventHandlerSelector(ModelRelationShip.Self, "", "");
                var enumerable = selector.GetEventHandlerEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "IEventHandlerを継承している時は取得できるようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(recieverModel, enumerable.First(), errorMessage);
            }

            {//IEventHandlerを継承していないModelを取得しようとした時のテスト
                //search root model => noneReciever
                //selector: Self, "", ""
                //result => (empty)
                var selector = new EventHandlerSelector(ModelRelationShip.Self, "", "");
                var enumerable = selector.GetEventHandlerEnumerable(noneRecieverModel, binderMapInstance);
                var errorMessage = "IEventHandlerを継承していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//IEventHandlerを継承しているViewを取得しようとした時のテスト
                //search root model => reciever
                //selector: Self, "", "reciever"
                //result => EventHandlerViewObj in reciever
                var selector = new EventHandlerSelector(ModelRelationShip.Self, "", viewReciever);
                var enumerable = selector.GetEventHandlerEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "Viewを指定した時はModelにバインドされていて、かつIEventHandlerを継承しているViewを取得できるようにしてください。";
                var viewObj = binderMapInstance[recieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(viewObj, enumerable.First(), errorMessage);
            }

            {//IEventHandlerを継承しているViewを取得しようとした時のテスト2
                //search root model => noneReciever
                //selector: Self, "", "reciever"
                //result => EventHandlerViewObj in noneReciever
                var selector = new EventHandlerSelector(ModelRelationShip.Self, "", viewReciever);
                var enumerable = selector.GetEventHandlerEnumerable(noneRecieverModel, binderMapInstance);

                var errorMessage = "バインドされているModelがIEventHandlerを継承していない場合でも取得できるようにしてください";
                var viewObj = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(viewObj, enumerable.First(), errorMessage);
            }

            {//IEventHandlerを継承しているViewを取得しようとした時のテスト3(一致しないViewID)
                //search root model => noneReciever
                //selector: Self, "", "invalidIdentity"
                //result => (empty)
                var selector = new EventHandlerSelector(ModelRelationShip.Self, "", "invalidIdentity");
                var enumerable = selector.GetEventHandlerEnumerable(noneRecieverModel, binderMapInstance);

                var errorMessage = "ViewIdentityが一致していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }
        }

        interface IFookingReciever : IEventHandler
        {
        }

        interface ITestReciever : IEventHandler
        {
        }

        class FookableModel : Model, IFookingReciever, ITestReciever
        {
        }

        [Test]
        public void QueryPasses()
        {
            #region Construct Enviroment
            //Model Hierarchy
            // - root: #main type=FookableModel
            //   - reciever: #main type=RecieverModel
            //   - noneReciever: #main type=NoneRecieverModel
            //     - model: #main type=Model
            //
            //View info
            // #main:
            //   - SenderViewObj: ID=SenderViewObj, InstanceID=SenderViewObj, BinderID=SenderViewObj
            //   - EventHandlerViewObj: ID=reciever, InstanceID=SenderViewObj, BinderID=SenderViewObj

            var root = new FookableModel() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };
            var model1 = new Model() { Name = "model", Parent = noneRecieverModel, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(EventHandlerViewObj), new EventHandlerViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(viewReciever, typeof(EventHandlerViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//None Fook Selector
                var selector = new EventHandlerSelector(ModelRelationShip.Parent, "", "");
                Assert.IsFalse(selector.IsFooking);

                float eventData = 1.234f;
                var enumerable = selector.Query(typeof(ITestReciever), recieverModel, binderMapInstance, eventData);

                var errorMessage = "RecieverSelector#Queryは指定したRecieverTypeとEventData、クエリと一致したModelを返すようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                var first = enumerable.First();
                Assert.AreEqual(typeof(ITestReciever), first.recieverType, errorMessage);
                Assert.AreSame(root, first.reciever, errorMessage);
                Assert.AreEqual(eventData, first.eventData, errorMessage);
            }

            {//Fook Selector!!
                var selector = new EventHandlerSelector(ModelRelationShip.Parent, "", "");
                Assert.IsFalse(selector.IsFooking);

                // set Fook info!!
                selector.FookingRecieverType = typeof(IFookingReciever);
                selector.FookEventData = 432;
                Assert.IsTrue(selector.IsFooking);

                int eventData = 123;
                var enumerable = selector.Query(typeof(ITestReciever), recieverModel, binderMapInstance, eventData);

                var errorMessage = "Fook情報が設定されたRecieverSelectorの場合はRecieverSelector#QueryはrecieverTypeを(RecieverSelector#FookingRecieverType)、eventDataを(RecieverSelector#FookEventData)に変換したものを返すようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                var first = enumerable.First();
                Assert.AreEqual(typeof(IFookingReciever), first.recieverType, errorMessage);
                Assert.AreSame(root, first.reciever, errorMessage);
                Assert.AreEqual(selector.FookEventData, first.eventData, errorMessage);
            }

        }

        public interface ITestSendPassReciever : IEventHandler
        {
            void Recieve(Model sender, int value);
        }
        class TestSendPassRecieverModel : Model, ITestSendPassReciever
        {
            public int RecievedValue { get; set; }
            public Model SenderModel { get; set; }

            public void Recieve(Model sender, int value)
            {
                SenderModel = sender;
                RecievedValue = value;
            }
        }

        [Test]
        public void SendPasses()
        {
            #region Construct Enviroment
            //Model Hierarchy
            // - root: #main type=FookableModel
            //   - reciever: #main type=RecieverModel
            //   - noneReciever: #main type=NoneRecieverModel
            //     - model: #main type=Model
            //
            //View info
            // #main:
            //   - SenderViewObj: ID=SenderViewObj, InstanceID=SenderViewObj, BinderID=SenderViewObj
            //   - EventHandlerViewObj: ID=reciever, InstanceID=SenderViewObj, BinderID=SenderViewObj

            var root = new FookableModel() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new TestSendPassRecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };
            var model1 = new Model() { Name = "model", Parent = noneRecieverModel, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(EventHandlerViewObj), new EventHandlerViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(viewReciever, typeof(EventHandlerViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            EventHandlerTypeManager.EntryEventHandlerExecuter<ITestSendPassReciever, int>(
                (reciever, sender, eventData) => {
                    (reciever as ITestSendPassReciever).Recieve(sender, (int)eventData);
                }
            );
            var selector = new EventHandlerSelector(ModelRelationShip.Child, "", "");

            selector.Send<ITestSendPassReciever>(root, 100, binderMapInstance);

            Assert.AreSame(root, recieverModel.SenderModel);
            Assert.AreEqual(100, recieverModel.RecievedValue);
        }
    }
}
