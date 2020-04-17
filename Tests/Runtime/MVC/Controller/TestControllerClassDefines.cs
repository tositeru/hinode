using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Hinode.Tests.MVC.Controller
{
    interface ITestSender : IControllerSender
    { }

    interface ITestReciever : IControllerReciever
    {
        void Test(Model sender, int value);
    }


    interface ITest2Sender : IControllerSender
    { }

    interface ITest2Reciever : IControllerReciever
    {
        void Test2(Model sender, int value);
    }

    interface IEmptySender : IControllerSender
    { }

    interface IEmptyReciever : IControllerReciever
    {
    }

    public class TestSenderInstance
        : IControllerSenderInstance
        , ITestSender
        , ITest2Sender
    {
        public static readonly string KEYWORD_ON_TEST = "onTest";
        public static readonly string KEYWORD_ON_TEST2 = "onTest2";

        #region IControllerSenderInstance
        EnableSenderCollection _enabledSenders = new EnableSenderCollection();
        SelectorListDictionary _selectorListDict = new SelectorListDictionary();
        public IControllerSenderGroup UseSenderGroup { get; set; }
        public EnableSenderCollection EnabledSenders { get => _enabledSenders; }
        public SelectorListDictionary SelectorListDict { get => _selectorListDict; }

        #endregion

        #region IControllerSender
        public Model Target { get; set; }
        public ModelViewBinderInstanceMap UseBinderInstanceMap { get; set; }
        #endregion

        #region ITestSender
        public void Send(int value)
        {
            this.Send<ITestSender>(Target, UseBinderInstanceMap, value);
        }
        #endregion

        #region ITest2Sender
        public void Send2(int value)
        {
            this.Send<ITest2Sender>(Target, UseBinderInstanceMap, value);
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

    class TestView : IViewObject
    {
        public Model UseModel { get; set; }
        public ModelViewBinder.BindInfo UseBindInfo { get; set; }

        public void Bind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
        {
            UseModel = targetModel;
            UseBindInfo = bindInfo;
        }

        public void Unbind()
        {
        }

        public class ParamBinder : IModelViewParamBinder
        {
            public void Update(Model model, IViewObject viewObj)
            {
            }
        }
    }
}
