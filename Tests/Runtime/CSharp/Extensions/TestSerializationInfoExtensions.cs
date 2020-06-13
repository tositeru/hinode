﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
    /// <seealso cref="SerializationInfoExtensions"/>
    /// </summary>
    public class TestSerializationInfoExtensions
    {
        class GetEnumerablePassesClass
        {
            public int intValue = 0;
            public float floatValue = 0f;
        }

        /// <summary>
        /// <seealso cref="SerializationInfoExtensions.GetEnumerable(SerializationInfo)"/>
        /// </summary>
        [Test]
        public void GetEnumerablePasses()
        {
            var serializationInfo = new SerializationInfo(typeof(GetEnumerablePassesClass), new FormatterConverter());

            var corrects = new (string name, int value)[]
            {
                ("test1", 111),
                ("test2", 222),
                ("test3", 333),
            };

            foreach(var c in corrects)
            {
                serializationInfo.AddValue(c.name, c.value);
            }

            AssertionUtils.AssertEnumerable<(string, int)>(
                corrects.AsEnumerable()
                , serializationInfo.GetEnumerable()
                    .Select(_e => (name: _e.Name, value: (int)_e.Value)),
                "");
        }
    }
}
