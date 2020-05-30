using System.Collections;
using System.Collections.Generic;
using Hinode;
using NUnit.Framework;
using UnityEngine;

namespace Hinode.Tests.MVC.Events
{
    interface ITestReciever : IEventHandler
    {
        void Test(Model sender, int value);
    }

    interface ITest2Reciever : IEventHandler
    {
        void Test2(Model sender, int value);
    }

    interface IEmptyReciever : IEventHandler
    {
    }

    public class TestEventDispatcher : IEventDispatcher
    {
        public static readonly string KEYWORD_ON_TEST = "onTest";
        public static readonly string KEYWORD_ON_TEST2 = "onTest2";

        #region IEventDispatcher interface
        public override bool DoEnabled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override IEventDispatcherHelper CreateEventDispatcherHelpObject(Model model, IViewObject viewObject)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsCreatableControllerObject(Model model, IViewObject viewObject)
        {
            throw new System.NotImplementedException();
        }

        protected override EventInfoManager CreateEventInfoManager()
            => new EventInfoManager(
                EventInfoManager.CreateInfo<ITestReciever>("onTest"),
                EventInfoManager.CreateInfo<ITest2Reciever>("onTest2")
            );

        protected override object GetEventData(Model model, IViewObject viewObject, ControllerInfo controllerInfo)
        {
            throw new System.NotImplementedException();
        }

        protected override void UpdateImpl(ModelViewBinderInstanceMap binderInstanceMap)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }

    class TestModel : Model
        , ITestReciever
        , ITest2Reciever
    {
        public Model TestSender { get; set; }
        public Model Test2Sender { get; set; }
        public int Value { get; set; }
        public int Value2 { get; set; }

        public void Test(Model sender, int value)
        {
            TestSender = sender;
            Value = value;
        }
        public void Test2(Model sender, int value)
        {
            Test2Sender = sender;
            Value2 = value;
        }
    }

    class TestView : EmptyViewObject
    {
        protected override void OnBind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
        {
            UseModel = targetModel;
            UseBindInfo = bindInfo;
        }

        public class ParamBinder : IModelViewParamBinder
        {
            public void Update(Model model, IViewObject viewObj)
            {
            }
        }
    }
}
