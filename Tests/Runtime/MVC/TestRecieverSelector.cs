using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
    /// <seealso cref="RecieverSelector"/>
    /// </summary>
    public class TestRecieverSelector
    {
        class SenderViewObj : IViewObject, IControllerSender
        {
            #region IControllerSender
            public Model Target { get; set; }
            public ModelViewBinderInstanceMap UseBinderInstanceMap { get; set; }
            public RecieverSelector Selector { get; set; }

            #endregion

            #region IViewObject
            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            {
            }

            public void Unbind()
            {
            }
            #endregion

            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        class RecieverViewObj : IViewObject, IControllerReciever
        {
            #region IControllerSender
            public ModelViewBinderInstance ModelViewBinderInstance { get; set; }
            public RecieverSelector Selector { get; set; }
            #endregion

            #region IViewObject
            public Model UseModel { get; set; }
            public ModelViewBinder.BindInfo UseBindInfo { get; set; }

            public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            {
            }

            public void Unbind()
            {
            }
            #endregion

            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj)
                {
                }
            }
        }

        class RecieverModel : Model, IControllerReciever
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
            //   - RecieverViewObj: ID=reciever, InstanceID=SenderViewObj, BinderID=SenderViewObj
            var root = new RecieverModel() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };
            var model1 = new Model() { Name = "model", Parent = noneRecieverModel, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(SenderViewObj), new SenderViewObj.ParamBinder()),
                (typeof(RecieverViewObj), new RecieverViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(typeof(SenderViewObj)),
                    new ModelViewBinder.BindInfo(viewReciever, typeof(RecieverViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//IControllerRecieverを継承しているModelを取得しようとした時のテスト
                //search root model => reciever
                //selector: Parent, "", ""
                //result => root
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Parent, "", "");
                var enumerable = selector.GetRecieverEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "IControllerRecieverを継承している時は取得できるようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(root, enumerable.First(), errorMessage);
            }

            {//ParentがnullなModelのParentを指定した時のテスト
                //search root model => root
                //selector: Parent, "", ""
                //result => (empty)
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Parent, "", "");
                var enumerable = selector.GetRecieverEnumerable(root, binderMapInstance);

                var errorMessage = "ParentがないmodelのParentを指定した時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//IControllerRecieverを継承していないModelを取得しようとした時のテスト
                //search root model => model
                //selector: Parent, "", ""
                //result => (empty)
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Parent, "", "");
                var enumerable = selector.GetRecieverEnumerable(model1, binderMapInstance);
                var errorMessage = "IControllerRecieverを継承していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//ParentがnullなModelのParentを指定した時のテスト2
                //search root model => model
                //selector: Parent, "root", ""
                //result => root
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Parent, "#main", "");
                var enumerable = selector.GetRecieverEnumerable(model1, binderMapInstance);

                var errorMessage = "親指定のrecieverSelectorの時、クエリパスを指定した時はBinderInstanceMapのRootModelをクエリルートにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(root, enumerable.First(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト
                //search root model => reciever
                //selector: Parent, "", "reciever"
                //result => RecieverViewObj in root
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Parent, "", viewReciever);
                var enumerable = selector.GetRecieverEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "Viewを指定した時はModelにバインドされていて、かつIControllerRecieverを継承しているViewを取得できるようにしてください。";
                var viewObj = binderMapInstance[root].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(viewObj, enumerable.First(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト2
                //search root model => model
                //selector: Parent, "", "reciever"
                //result => RecieverViewObj in noneReciever
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Parent, "", viewReciever);
                var enumerable = selector.GetRecieverEnumerable(model1, binderMapInstance);

                var errorMessage = "バインドされているModelがIControllerRecieverを継承していない場合でも取得できるようにしてください";
                var viewObj = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(viewObj, enumerable.First(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト3(一致しないViewID)
                //search root model => model
                //selector: Parent, "", "invalidIdentity"
                //result => (empty)
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Parent, "", "invalidIdentity");
                var enumerable = selector.GetRecieverEnumerable(model1, binderMapInstance);

                var errorMessage = "ViewIdentityが一致していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//QueryPathとViewIdentityを指定した時のテスト
                //search root model => model
                //selector: Parent, "root", "reciever"
                //result => root
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Parent, "#main", viewReciever);
                var enumerable = selector.GetRecieverEnumerable(model1, binderMapInstance);

                var errorMessage = "親指定のrecieverSelectorの時、クエリパスを指定した時はBinderInstanceMapのRootModelをクエリルートにしてください。";
                var rootViewObj = binderMapInstance[root].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                var parentViewObj = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(2, enumerable.Count(), errorMessage);
                AssertionUtils.AssertEnumerable(enumerable, new IViewObject[] { rootViewObj, parentViewObj }.OfType<IControllerReciever>(), errorMessage);

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
            //   - RecieverViewObj: ID=reciever, InstanceID=SenderViewObj, BinderID=SenderViewObj
            var root = new RecieverModel() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };
            var recieverModel2 = new RecieverModel() { Name = "reciever2", Parent = noneRecieverModel, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(SenderViewObj), new SenderViewObj.ParamBinder()),
                (typeof(RecieverViewObj), new RecieverViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(typeof(SenderViewObj)),
                    new ModelViewBinder.BindInfo(viewReciever, typeof(RecieverViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//IControllerRecieverを継承しているModelを取得しようとした時のテスト
                //search root model => root
                //selector: Child, "", ""
                //result => reciever
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Child, "", "");
                var enumerable = selector.GetRecieverEnumerable(root, binderMapInstance);

                var errorMessage = "IControllerRecieverを継承している時は取得できるようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(recieverModel, enumerable.First(), errorMessage);
            }

            {//Childがない時のModelのChildを指定した時のテスト
                //search root model => reciever
                //selector: Child, "", ""
                //result => (empty)
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Child, "", "");
                var enumerable = selector.GetRecieverEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "ChildがないmodelのChildを指定した時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//QueryPathを指定した時のテスト
                //search root model => root
                //selector: Child, "*", ""
                //result => reciever, reciever2
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Child, "*", "");
                var enumerable = selector.GetRecieverEnumerable(root, binderMapInstance);
                var errorMessage = "QueryPathを指定した時はそれに一致するModel全て取得できるようにしてください";
                AssertionUtils.AssertEnumerable(enumerable, new IControllerReciever[] { recieverModel, recieverModel2 }, errorMessage);
            }

            {//一致しないQueryPathを指定した時のテスト
                //search root model => root
                //selector: Child, "__invalid", ""
                //result => (empty)
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Child, "__invalid", "");
                var enumerable = selector.GetRecieverEnumerable(root, binderMapInstance);
                var errorMessage = "QueryPathが一致しない時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト
                //search root model => root
                //selector: Child, "", "reciever"
                //result => RecieverViewObj in reciever, RecieverViewObj in noneReciever
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Child, "", viewReciever);
                var enumerable = selector.GetRecieverEnumerable(root, binderMapInstance);

                var errorMessage = "子モデルのViewを指定した時はModelにバインドされていて、かつIControllerRecieverを継承しているViewを全て取得できるようにしてください。";
                var viewObj1 = binderMapInstance[recieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                var viewObj2 = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                AssertionUtils.AssertEnumerable(enumerable, (new IViewObject[] { viewObj1, viewObj2 }).OfType<IControllerReciever>(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト(一致しないViewID)
                //search root model => root
                //selector: Child, "", "__invalid"
                //result => (empty)
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Child, "", "__invalid");
                var enumerable = selector.GetRecieverEnumerable(root, binderMapInstance);

                var errorMessage = "指定したViewIdentityと一致ない時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト(クエリパスを指定した時)
                //search root model => root
                //selector: Child, "reciever", "reciever"
                //result => RecieverViewObj in reciever, 
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Child, recieverModel.Name, viewReciever);
                var enumerable = selector.GetRecieverEnumerable(root, binderMapInstance);

                var errorMessage = "クエリパスを伴う子モデルのViewを指定した時はクエリパスに一致しModelにバインドされていて、かつIControllerRecieverを継承しているViewを全て取得できるようにしてください。";
                var viewObj1 = binderMapInstance[recieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                AssertionUtils.AssertEnumerable(enumerable, (new IViewObject[] { viewObj1 }).OfType<IControllerReciever>(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト(クエリパスを指定し、ViewIDが一致しない時)
                //search root model => root
                //selector: Child, "reciever", "__invalid"
                //result => (empty)
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Child, recieverModel.Name, "__invalid");
                var enumerable = selector.GetRecieverEnumerable(root, binderMapInstance);

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
            //   - RecieverViewObj: ID=reciever, InstanceID=SenderViewObj, BinderID=SenderViewObj
            var root = new Model() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(SenderViewObj), new SenderViewObj.ParamBinder()),
                (typeof(RecieverViewObj), new RecieverViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(typeof(SenderViewObj)),
                    new ModelViewBinder.BindInfo(viewReciever, typeof(RecieverViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//IControllerRecieverを継承しているModelを取得しようとした時のテスト
                //search root model => reciever
                //selector: Self, "", ""
                //result => reciever
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", "");
                var enumerable = selector.GetRecieverEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "IControllerRecieverを継承している時は取得できるようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(recieverModel, enumerable.First(), errorMessage);
            }

            {//IControllerRecieverを継承していないModelを取得しようとした時のテスト
                //search root model => noneReciever
                //selector: Self, "", ""
                //result => (empty)
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", "");
                var enumerable = selector.GetRecieverEnumerable(noneRecieverModel, binderMapInstance);
                var errorMessage = "IControllerRecieverを継承していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト
                //search root model => reciever
                //selector: Self, "", "reciever"
                //result => RecieverViewObj in reciever
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", viewReciever);
                var enumerable = selector.GetRecieverEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "Viewを指定した時はModelにバインドされていて、かつIControllerRecieverを継承しているViewを取得できるようにしてください。";
                var viewObj = binderMapInstance[recieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(viewObj, enumerable.First(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト2
                //search root model => noneReciever
                //selector: Self, "", "reciever"
                //result => RecieverViewObj in noneReciever
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", viewReciever);
                var enumerable = selector.GetRecieverEnumerable(noneRecieverModel, binderMapInstance);

                var errorMessage = "バインドされているModelがIControllerRecieverを継承していない場合でも取得できるようにしてください";
                var viewObj = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(viewObj, enumerable.First(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト3(一致しないViewID)
                //search root model => noneReciever
                //selector: Self, "", "invalidIdentity"
                //result => (empty)
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", "invalidIdentity");
                var enumerable = selector.GetRecieverEnumerable(noneRecieverModel, binderMapInstance);

                var errorMessage = "ViewIdentityが一致していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }
        }

        interface IFookingReciever : IControllerReciever
        {
        }

        interface ITestReciever : IControllerReciever
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
            //   - RecieverViewObj: ID=reciever, InstanceID=SenderViewObj, BinderID=SenderViewObj

            var root = new FookableModel() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };
            var model1 = new Model() { Name = "model", Parent = noneRecieverModel, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(SenderViewObj), new SenderViewObj.ParamBinder()),
                (typeof(RecieverViewObj), new RecieverViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(typeof(SenderViewObj)),
                    new ModelViewBinder.BindInfo(viewReciever, typeof(RecieverViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//None Fook Selector
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Parent, "", "");
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
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Parent, "", "");
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

    }
}
