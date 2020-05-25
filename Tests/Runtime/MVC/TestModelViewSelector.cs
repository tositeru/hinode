using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
    /// <seealso cref="TestModelViewSelector"/>
    /// </summary>
    public class TestModelViewSelector
    {
        class SenderViewObj : EmptyViewObject
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
            //   - RecieverViewObj: ID=reciever, InstanceID=SenderViewObj, BinderID=SenderViewObj
            var root = new RecieverModel() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };
            var model1 = new Model() { Name = "model", Parent = noneRecieverModel, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(SenderViewObj), new SenderViewObj.ParamBinder()),
                (typeof(EventHandlerViewObj), new EventHandlerViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(typeof(SenderViewObj)),
                    new ModelViewBinder.BindInfo(viewReciever, typeof(EventHandlerViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//Modelを取得しようとした時のテスト
                //search root model => reciever
                //selector: Parent, "", ""
                //result => root
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "", "");
                var enumerable = selector.GetEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "Parentを取得できるようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(root, enumerable.First(), errorMessage);
            }

            {//ParentがnullなModelのParentを指定した時のテスト
                //search root model => root
                //selector: Parent, "", ""
                //result => (empty)
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "", "");
                var enumerable = selector.GetEnumerable(root, binderMapInstance);

                var errorMessage = "ParentがないmodelのParentを指定した時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//ParentがnullなModelのParentを指定した時のテスト2
                //search root model => model
                //selector: Parent, "root", ""
                //result => root, noneReciever
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "#main", "");
                var enumerable = selector.GetEnumerable(model1, binderMapInstance);

                var errorMessage = "親指定のSelectorの時、クエリパスを指定した時はBinderInstanceMapのRootModelをクエリルートにし、検索対象のModelの親Modelのみを検索してください。";
                AssertionUtils.AssertEnumerableByUnordered(new object[] {
                    root, noneRecieverModel
                }, enumerable, errorMessage);
            }

            {//Viewを取得しようとした時のテスト
                //search root model => reciever
                //selector: Parent, "", "reciever"
                //result => RecieverViewObj in root
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "", viewReciever);
                var enumerable = selector.GetEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "Viewを指定した時はModelにバインドされているViewを取得できるようにしてください。";
                AssertionUtils.AssertEnumerableByUnordered(
                    binderMapInstance[root].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever)
                    , enumerable
                    , errorMessage);
            }

            {//Viewを取得しようとした時のテスト2
                //search root model => model
                //selector: Parent, "", "reciever"
                //result => RecieverViewObj in noneReciever
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "", viewReciever);
                var enumerable = selector.GetEnumerable(model1, binderMapInstance);

                var errorMessage = "バインドされているModelがIEventHandlerを継承していない場合でも取得できるようにしてください";
                AssertionUtils.AssertEnumerableByUnordered(
                    binderMapInstance[noneRecieverModel].ViewObjects
                        .Where(_v => _v.UseBindInfo.ID == viewReciever)
                    , enumerable
                    , errorMessage);
            }

            {//Viewを取得しようとした時のテスト3(一致しないViewID)
                //search root model => model
                //selector: Parent, "", "invalidIdentity"
                //result => (empty)
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "", "invalidIdentity");
                var enumerable = selector.GetEnumerable(model1, binderMapInstance);

                var errorMessage = "ViewIdentityが一致していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//QueryPathとViewIdentityを指定した時のテスト
                //search root model => model
                //selector: Parent, "root", "reciever"
                //result => root
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "#main", viewReciever);
                var enumerable = selector.GetEnumerable(model1, binderMapInstance);

                var errorMessage = "親指定のrecieverSelectorの時、クエリパスを指定した時はBinderInstanceMapのRootModelをクエリルートにしてください。";
                var rootViewObj = binderMapInstance[root].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                var parentViewObj = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                AssertionUtils.AssertEnumerableByUnordered(
                    binderMapInstance[noneRecieverModel].ViewObjects
                        .Concat(binderMapInstance[root].ViewObjects)
                        .Where(_v => _v.UseBindInfo.ID == viewReciever)
                    , enumerable
                    , errorMessage);
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
                (typeof(EventHandlerViewObj), new EventHandlerViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(typeof(SenderViewObj)),
                    new ModelViewBinder.BindInfo(viewReciever, typeof(EventHandlerViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//Modelを取得しようとした時のテスト
                //search root model => root
                //selector: Child, "", ""
                //result => reciever, noneReciever
                var selector = new ModelViewSelector(ModelRelationShip.Child, "", "");
                var enumerable = selector.GetEnumerable(root, binderMapInstance);

                var errorMessage = "IEventHandlerを継承している時は取得できるようにしてください。";
                AssertionUtils.AssertEnumerableByUnordered(new object[] {
                    recieverModel,
                    noneRecieverModel,
                }, enumerable, errorMessage);
            }

            {//Childがない時のModelのChildを指定した時のテスト
                //search root model => reciever
                //selector: Child, "", ""
                //result => (empty)
                var selector = new ModelViewSelector(ModelRelationShip.Child, "", "");
                var enumerable = selector.GetEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "ChildがないmodelのChildを指定した時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//QueryPathを指定した時のテスト
                //search root model => root
                //selector: Child, "*", ""
                //result => reciever, reciever2
                var selector = new ModelViewSelector(ModelRelationShip.Child, "*", "");
                var enumerable = selector.GetEnumerable(root, binderMapInstance);
                var errorMessage = "QueryPathを指定した時はそれに一致するModel全て取得できるようにしてください";
                AssertionUtils.AssertEnumerableByUnordered(new object[] {
                    recieverModel,
                    noneRecieverModel,
                    recieverModel2,
                }, enumerable, errorMessage);
            }

            {//一致しないQueryPathを指定した時のテスト
                //search root model => root
                //selector: Child, "__invalid", ""
                //result => (empty)
                var selector = new ModelViewSelector(ModelRelationShip.Child, "__invalid", "");
                var enumerable = selector.GetEnumerable(root, binderMapInstance);
                var errorMessage = "QueryPathが一致しない時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//Viewを取得しようとした時のテスト
                //search root model => root
                //selector: Child, "", "reciever"
                //result => RecieverViewObj in reciever, RecieverViewObj in noneReciever
                var selector = new ModelViewSelector(ModelRelationShip.Child, "", viewReciever);
                var enumerable = selector.GetEnumerable(root, binderMapInstance);

                var errorMessage = "子モデルのViewを指定した時はModelにバインドされていて、かつIEventHandlerを継承しているViewを全て取得できるようにしてください。";
                var viewObj1 = binderMapInstance[recieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                var viewObj2 = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                AssertionUtils.AssertEnumerableByUnordered(new object[] {
                    viewObj1,
                    viewObj2
                }, enumerable, errorMessage);
            }

            {//Viewを取得しようとした時のテスト(一致しないViewID)
                //search root model => root
                //selector: Child, "", "__invalid"
                //result => (empty)
                var selector = new ModelViewSelector(ModelRelationShip.Child, "", "__invalid");
                var enumerable = selector.GetEnumerable(root, binderMapInstance);

                var errorMessage = "指定したViewIdentityと一致ない時は何も取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//Viewを取得しようとした時のテスト(クエリパスを指定した時)
                //search root model => root
                //selector: Child, "reciever", "reciever"
                //result => RecieverViewObj in reciever, 
                var selector = new ModelViewSelector(ModelRelationShip.Child, recieverModel.Name, viewReciever);
                var enumerable = selector.GetEnumerable(root, binderMapInstance);

                var errorMessage = "クエリパスを伴う子モデルのViewを指定した時はクエリパスに一致しModelにバインドされていて、かつIEventHandlerを継承しているViewを全て取得できるようにしてください。";
                var viewObj1 = binderMapInstance[recieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                AssertionUtils.AssertEnumerable(enumerable, (new IViewObject[] { viewObj1 }).OfType<IEventHandler>(), errorMessage);
            }

            {//Viewを取得しようとした時のテスト(クエリパスを指定し、ViewIDが一致しない時)
                //search root model => root
                //selector: Child, "reciever", "__invalid"
                //result => (empty)
                var selector = new ModelViewSelector(ModelRelationShip.Child, recieverModel.Name, "__invalid");
                var enumerable = selector.GetEnumerable(root, binderMapInstance);

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
                (typeof(EventHandlerViewObj), new EventHandlerViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(typeof(SenderViewObj)),
                    new ModelViewBinder.BindInfo(viewReciever, typeof(EventHandlerViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//Modelを取得しようとした時のテスト
                //search root model => reciever
                //selector: Self, "", ""
                //result => reciever
                var selector = new ModelViewSelector(ModelRelationShip.Self, "", "");
                var enumerable = selector.GetEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "自身を取得できるようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(recieverModel, enumerable.First(), errorMessage);
            }

            {//Viewを取得しようとした時のテスト
                //search root model => reciever
                //selector: Self, "", "reciever"
                //result => RecieverViewObj in reciever
                var selector = new ModelViewSelector(ModelRelationShip.Self, "", viewReciever);
                var enumerable = selector.GetEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "Viewを指定した時はModelにバインドされているViewを取得できるようにしてください。";
                AssertionUtils.AssertEnumerableByUnordered(
                    binderMapInstance[recieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever)
                    , enumerable, errorMessage);
            }

            {//Viewを取得しようとした時のテスト3(一致しないViewID)
                //search root model => noneReciever
                //selector: Self, "", "invalidIdentity"
                //result => (empty)
                var selector = new ModelViewSelector(ModelRelationShip.Self, "", "invalidIdentity");
                var enumerable = selector.GetEnumerable(noneRecieverModel, binderMapInstance);

                var errorMessage = "ViewIdentityが一致していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }
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

            var root = new RecieverModel() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };
            var model1 = new Model() { Name = "model", Parent = noneRecieverModel, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(SenderViewObj), new SenderViewObj.ParamBinder()),
                (typeof(EventHandlerViewObj), new EventHandlerViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(typeof(SenderViewObj)),
                    new ModelViewBinder.BindInfo(viewReciever, typeof(EventHandlerViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion

            {//指定した型を持つ時(Model)
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "", "");

                var enumerable = selector.Query(typeof(RecieverModel), recieverModel, binderMapInstance);

                var errorMessage = "ModelViewSelector#Queryはクエリと一致したModelの中から指定したTypeのものを返すようにしてください。";
                AssertionUtils.AssertEnumerableByUnordered(
                    new object[] { root }
                    , enumerable, errorMessage);
            }
            {//指定した型を持つ時(View)
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "", viewReciever);

                var enumerable = selector.Query(typeof(EventHandlerViewObj), recieverModel, binderMapInstance);

                var errorMessage = "ModelViewSelector#Queryはクエリと一致したModelの中から指定したTypeのものを返すようにしてください。";
                AssertionUtils.AssertEnumerableByUnordered(
                    binderMapInstance.BindInstances[root].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever)
                    , enumerable, errorMessage);
            }
            {//指定したTypeではない時
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "", "");

                var enumerable = selector.Query(typeof(RecieverModel), model1, binderMapInstance);

                var errorMessage = "ModelViewSelector#Queryは指定したTypeではないものは返さないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }
            {//指定したTypeではない時(View)
                var selector = new ModelViewSelector(ModelRelationShip.Parent, "", viewReciever);

                var enumerable = selector.Query(typeof(SenderViewObj), model1, binderMapInstance);

                var errorMessage = "ModelViewSelector#Queryはクエリと一致したModelの中から指定したTypeのものを返すようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }
        }

        class ChildViewIDPassesViewObject : EmptyViewObject
        {
            public static string CHILD_ID = "child";
            public static string NEST_CHILD_ID = "nest";
            public EmptyViewObject Child { get; } = new EmptyViewObject();
            public NestChildView NestChild { get; } = new NestChildView();

            public override object QueryChild(string query)
            {
                if (CHILD_ID == query) return Child;
                if (NEST_CHILD_ID == query) return NestChild;
                return null;
            }

            public class NestChildView : EmptyViewObject
            {
                public static string CHILD_APPLE_ID = "apple";
                public Apple Child { get; } = new Apple();
                public override object QueryChild(string query)
                    => query == CHILD_APPLE_ID ? Child : null;

                public class Apple { }
            }
        }

        [Test]
        public void ChildViewIDPasses()
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

            var root = new Model() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new Model() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new Model() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };
            var model1 = new Model() { Name = "model", Parent = noneRecieverModel, LogicalID = new ModelIDList("main") };

            string viewID = "viewObj";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(ChildViewIDPassesViewObject), new EmptyModelViewParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(viewID, typeof(ChildViewIDPassesViewObject))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = root;
            #endregion


            {//Case Success childViewID
                var selector = new ModelViewSelector(ModelRelationShip.Self, "", $"{viewID}.{ChildViewIDPassesViewObject.CHILD_ID}");
                var enumerable = selector.Query<EmptyViewObject>(root, binderMapInstance);

                var rootViewObjs = binderMapInstance[root].ViewObjects;
                AssertionUtils.AssertEnumerableByUnordered(
                    rootViewObjs.Select(_v => _v.QueryChild<EmptyViewObject>(ChildViewIDPassesViewObject.CHILD_ID))
                    , enumerable
                    , $"");
            }

            {//Case Success nested childViewID
                var childViewID = $"{ChildViewIDPassesViewObject.NEST_CHILD_ID}.{ChildViewIDPassesViewObject.NestChildView.CHILD_APPLE_ID}";
                var queryViewID = $"{viewID}.{childViewID}";
                var selector = new ModelViewSelector(ModelRelationShip.Self, "", queryViewID);
                var enumerable = selector.Query<ChildViewIDPassesViewObject.NestChildView.Apple>(root, binderMapInstance);

                var rootViewObjs = binderMapInstance[root].ViewObjects;
                AssertionUtils.AssertEnumerableByUnordered(
                    rootViewObjs.Select(_v => _v.QueryChild<ChildViewIDPassesViewObject.NestChildView.Apple>(childViewID.Split('.')))
                    , enumerable
                    , $"Failed to query '{queryViewID}'...");
            }

            {//Case Invalid childViewID
                var selector = new ModelViewSelector(ModelRelationShip.Self, "", $"{viewID}.invalidID");
                var enumerable = selector.Query<EmptyViewObject>(root, binderMapInstance);

                Assert.IsFalse(enumerable.Any());
            }
            {//Case Invalid ViewID Selector Format
                var selector = new ModelViewSelector(ModelRelationShip.Self, "", $"{viewID}#{ChildViewIDPassesViewObject.CHILD_ID}");

                var enumerable = selector.Query<EmptyViewObject>(root, binderMapInstance);
                Assert.IsFalse(enumerable.Any());
            }
        }
    }
}
