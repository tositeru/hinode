using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC
{
    /// <summary>
    /// <seealso cref="IController"/>
    /// </summary>
    public class TestIController
    {
        public class TestModel : Model, IOnClickReciever
        {
            public void OnClicked(IOnClickSender sender, OnClickEventData eventData)
            {

            }
        }

        public class ClickSender : IOnClickSender
        {

        }

        // A Test behaves as an ordinary method
        [Test]
        public void TestIControllerSimplePasses()
        {
            var reciever = new TestModel();
            var sender = new ClickSender();

            reciever.OnClicked(sender, new OnClickEventData());
            throw new System.NotImplementedException();
        }
    }
}
