﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.MVC.Tests
{
    /// <summary>
	/// <seealso cref="ModelViewValidator"/>
	/// </summary>
    public class TestModelViewValidator
    {
        [SetUp]
        public void SetUp()
        {
            Logger.PriorityLevel = Logger.Priority.Debug;
        }

        class AppleModel : Model { }
        class OrangeModel : Model { }

        [AvailableModel(typeof(AppleModel))]
        [AvailableModelViewParamBinder(typeof(SpecifiedAttributeViewObj.ParamBinder))]
        class SpecifiedAttributeViewObj : EmptyViewObject
        {
            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj) { }
            }
        }

        class NoneAttributeViewObj : EmptyViewObject
        {
            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj) { }
            }
        }

        [AvailableModel(typeof(OrangeModel))]
        [AvailableModelViewParamBinder(typeof(SpecifiedAttributeViewObj.ParamBinder))]
        class InvalidAttributeViewObj : EmptyViewObject
        {
            public class ParamBinder : IModelViewParamBinder
            {
                public void Update(Model model, IViewObject viewObj) { }
            }
        }

        // A Test behaves as an ordinary method
        [Test]
        public void ValidateBindInfoPasses()
        {
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(SpecifiedAttributeViewObj), new SpecifiedAttributeViewObj.ParamBinder()),
                (typeof(NoneAttributeViewObj), new NoneAttributeViewObj.ParamBinder()),
                (typeof(InvalidAttributeViewObj), new InvalidAttributeViewObj.ParamBinder())
            );

            ModelViewValidator.DoEnabled = true;

            {//Check BasicUsage
                var model = new AppleModel();
                var bindInfo = new ModelViewBinder.BindInfo(typeof(SpecifiedAttributeViewObj));
                Assert.IsTrue(ModelViewValidator.ValidateBindInfo(model, bindInfo, viewInstanceCreator));
            }

            {//Check None Attributes
                var model = new AppleModel();
                var bindInfo = new ModelViewBinder.BindInfo(typeof(NoneAttributeViewObj));
                Assert.IsTrue(ModelViewValidator.ValidateBindInfo(model, bindInfo, viewInstanceCreator));
            }

            {//Check Invalid Attributes
                var bindInfo = new ModelViewBinder.BindInfo(typeof(InvalidAttributeViewObj));
                var apple = new AppleModel();
                Assert.IsFalse(ModelViewValidator.ValidateBindInfo(apple, bindInfo, viewInstanceCreator), "Invalid Model Case");

                bindInfo = new ModelViewBinder.BindInfo(
                    ModelViewBinder.BindInfo.ToID(typeof(InvalidAttributeViewObj)),
                    typeof(InvalidAttributeViewObj).FullName,
                    "invalidBinderKey");
                var orange = new OrangeModel();
                Assert.IsFalse(ModelViewValidator.ValidateBindInfo(orange, bindInfo, viewInstanceCreator), "Invalid Model Case");
            }

        }

        class AppleModelSubClass : AppleModel
        { }

        // A Test behaves as an ordinary method
        [Test, Description("Check Validate Model Subclass")]
        public void ModelSubclassValidateBindInfoPasses()
        {
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(SpecifiedAttributeViewObj), new SpecifiedAttributeViewObj.ParamBinder())
            );

            ModelViewValidator.DoEnabled = true;

            var model = new AppleModelSubClass();
            var bindInfo = new ModelViewBinder.BindInfo(typeof(SpecifiedAttributeViewObj));
            Assert.IsTrue(ModelViewValidator.ValidateBindInfo(model, bindInfo, viewInstanceCreator));
        }

        class SpecifiedAttributeViewObjParamBinderSubClass : SpecifiedAttributeViewObj.ParamBinder
        {
        }

        // A Test behaves as an ordinary method
        [Test, Description("Check Validate IModelViewParamBinder Subclass")]
        public void ParamBinderSubclassValidateBindInfoPasses()
        {
            var viewInstanceCreator = new DefaultViewInstanceCreator(
                (typeof(SpecifiedAttributeViewObj), new SpecifiedAttributeViewObjParamBinderSubClass())
            );

            ModelViewValidator.DoEnabled = true;

            var model = new AppleModel();
            var bindInfo = new ModelViewBinder.BindInfo(typeof(SpecifiedAttributeViewObj));
            Assert.IsTrue(ModelViewValidator.ValidateBindInfo(model, bindInfo, viewInstanceCreator));
        }
    }
}
