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

        class RecieverModel : Model, IControllerReceiver
        { }

        class NoneRecieverModel : Model
        { }

        [Test]
        public void ParentSelectorPasses()
        {
            throw new System.NotImplementedException("yet not implement...");
        }

        [Test]
        public void ChildSelectorPasses()
        {
            throw new System.NotImplementedException("yet not implement...");
        }

        [Test]
        public void SelfSelectorPasses()
        {
            var root = new Model() { Name = "root", LogicalID = new ModelIDList("main") };
            var recieverModel = new RecieverModel() { Name = "reciever", Parent = root, LogicalID = new ModelIDList("main") };
            var noneRecieverModel = new NoneRecieverModel() { Name = "noneReciever", Parent = root, LogicalID = new ModelIDList("main") };

            string viewReciever = "reciever";
            var viewCreator = new DefaultViewInstanceCreator(
                (typeof(SenderViewObj), new SenderViewObj.ParamBinder())
            );
            var binderMap = new ModelViewBinderMap(viewCreator,
                new ModelViewBinder("#main", null,
                    new ModelViewBinder.BindInfo(typeof(SenderViewObj)),
                    new ModelViewBinder.BindInfo(viewReciever, typeof(SenderViewObj))
                ));
            var binderMapInstance = binderMap.CreateBinderInstaceMap();
            binderMapInstance.RootModel = recieverModel;

            {//IControllerRecieverを継承しているModelを取得しようとした時のテスト
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", "");
                var enumerable = selector.GetRecieverEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "IControllerRecieverを継承している時は取得できるようにしてください。";
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(recieverModel, enumerable.First(), errorMessage);
            }

            {//IControllerRecieverを継承していないModelを取得しようとした時のテスト
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", "");
                var enumerable = selector.GetRecieverEnumerable(noneRecieverModel, binderMapInstance);
                var errorMessage = "IControllerRecieverを継承していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", viewReciever);
                var enumerable = selector.GetRecieverEnumerable(recieverModel, binderMapInstance);

                var errorMessage = "Viewを指定した時はModelにバインドされていて、かつIControllerRecieverを継承しているViewを取得できるようにしてください。";
                var viewObj = binderMapInstance[recieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(viewObj, enumerable.First(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト2
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", viewReciever);
                var enumerable = selector.GetRecieverEnumerable(noneRecieverModel, binderMapInstance);

                var errorMessage = "バインドされているModelがIControllerRecieverを継承していない場合でも取得できるようにしてください";
                var viewObj = binderMapInstance[noneRecieverModel].ViewObjects.Where(_v => _v.UseBindInfo.ID == viewReciever).First();
                Assert.AreEqual(1, enumerable.Count(), errorMessage);
                Assert.AreSame(viewObj, enumerable.First(), errorMessage);
            }

            {//IControllerRecieverを継承しているViewを取得しようとした時のテスト3(一致しないViewID)
                var selector = new RecieverSelector(RecieverSelector.ModelRelationShip.Self, "", "invalidIdentity");
                var enumerable = selector.GetRecieverEnumerable(noneRecieverModel, binderMapInstance);

                var errorMessage = "ViewIdentityが一致していない時は取得できないようにしてください。";
                Assert.IsFalse(enumerable.Any(), errorMessage);
            }
        }
    }
}
