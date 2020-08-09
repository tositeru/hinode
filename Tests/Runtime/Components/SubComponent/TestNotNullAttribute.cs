using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Components.SubComponent
{
    /// <summary>
    /// <seealso cref="NotNullAttribute"/>
    /// </summary>
    public class TestNotNullAttribute
    {
        class ValidClass
        {
            [NotNull("ErrorMessage")]
            public System.Action Action = null;
        }

        #region Valid
        /// <summary>
        /// <seealso cref="NotNullAttribute.Valid(object)"/>
        /// </summary>
        /// <returns></returns>
        [Test]
        public void ValidPass()
        {
            var actionInfo = typeof(ValidClass).GetField("Action");
            var notNullAttr = actionInfo.GetCustomAttribute<NotNullAttribute>();
            Assert.IsNotNull(notNullAttr);

            var inst = new ValidClass();
            inst.Action = () => { };
            Assert.IsTrue(notNullAttr.Valid(actionInfo, inst)); // test point
        }

        /// <summary>
        /// <seealso cref="NotNullAttribute.Valid(object)"/>
        /// </summary>
        /// <returns></returns>
        [Test]
        public void ValidPassWhenNullField()
        {
            var actionInfo = typeof(ValidClass).GetField("Action");
            var notNullAttr = actionInfo.GetCustomAttribute<NotNullAttribute>();
            Assert.IsNotNull(notNullAttr);

            var inst = new ValidClass();
            inst.Action = null;
            Assert.IsFalse(notNullAttr.Valid(actionInfo, inst)); // test point
        }

        /// <summary>
        /// <seealso cref="NotNullAttribute.Valid(object)"/>
        /// </summary>
        /// <returns></returns>
        [Test]
        public void ValidPassWhenNullInstance()
        {
            var actionInfo = typeof(ValidClass).GetField("Action");
            var notNullAttr = actionInfo.GetCustomAttribute<NotNullAttribute>();
            Assert.IsNotNull(notNullAttr);

            var inst = new ValidClass();
            inst.Action = null;
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                notNullAttr.Valid(actionInfo, null) // test point
            );
        }

        #endregion

        #region ValidInstanceFields
        /// <summary>
        /// <seealso cref="NotNullAttribute.ValidInstanceFields(object)"/>
        /// </summary>
        /// <returns></returns>
        [Test]
        public void ValidInstanceFieldsPasses()
        {
            var inst = new ValidClass();
            inst.Action = () => { };
            Assert.DoesNotThrow(() =>
                NotNullAttribute.ValidInstanceFields(inst)//test point
            );
        }

        /// <summary>
        /// <seealso cref="NotNullAttribute.ValidInstanceFields(object)"/>
        /// </summary>
        [Test]
        public void ValidInstanceFieldsFails()
        {
            LogAssert.ignoreFailingMessages = true;

            var inst = new ValidClass();
            inst.Action = null;
            Assert.DoesNotThrow(() => 
                NotNullAttribute.ValidInstanceFields(inst) //test point
            );
        }

        /// <summary>
        /// <seealso cref="NotNullAttribute.ValidInstanceFields(object)"/>
        /// </summary>
        [Test]
        public void RegistToSubComponentAttributeManagerPasses()
        {
            Assert.IsTrue(SubComponentAttributeManager.ContainsInitMethod<NotNullAttribute>(), $"アセンブリ読み込み時にSubComponentAttributeManagerに追加されるようにしてください。");
        }
        #endregion
    }
}
