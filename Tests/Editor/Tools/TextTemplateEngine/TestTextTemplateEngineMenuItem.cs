using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Editors;

namespace Hinode.Tests.Editors.Tools
{
    /// <summary>
    /// <seealso cref="TextTemplateEngineMenuItem"/>
    /// </summary>
    public class TestTextTemplateEngineMenuItem
    {
        /// <summary>
        /// <seealso cref="TextTemplateEngineMenuItem.ExpandTextTemplate(string)"/>
        /// </summary>
        [Test]
        public void BasicCasePasses()
        {
            var srcText = @"
////@@ Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset

Hogehoge
";
            var destText = @"
////@@ Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset
Apple is Fruits
Cat is Animal
////-- Finish Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset

Hogehoge
".Replace("\r\n", "\n");
            Assert.AreEqual(destText,TextTemplateEngineMenuItem.ExpandTextTemplate(srcText).text.Replace("\r\n", "\n"));
        }

        /// <summary>
        /// <seealso cref="TextTemplateEngineMenuItem.ExpandTextTemplate(string)"/>
        /// </summary>
        [Test]
        public void AlreadyExpanededPasses()
        {
            var srcText = @"
////@@ Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset
Apple is Fruits
Cat is Animal
////-- Finish Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset

Hogehoge
";
            var destText = @"
////@@ Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset
Apple is Fruits
Cat is Animal
////-- Finish Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset

Hogehoge
".Replace("\r\n", "\n");
            Assert.AreEqual(destText, TextTemplateEngineMenuItem.ExpandTextTemplate(srcText).text.Replace("\r\n", "\n"));
        }

        /// <summary>
        /// <seealso cref="TextTemplateEngineMenuItem.ExpandTextTemplate(string)"/>
        /// </summary>
        [Test]
        public void ExpanedMultiplePasses()
        {
            var srcText = @"
////@@ Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset

Hogehoge
////@@ Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset
";
            var destText = @"
////@@ Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset
Apple is Fruits
Cat is Animal
////-- Finish Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset

Hogehoge
////@@ Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset
Apple is Fruits
Cat is Animal
////-- Finish Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset
".Replace("\r\n", "\n");
            Assert.AreEqual(destText, TextTemplateEngineMenuItem.ExpandTextTemplate(srcText).text.Replace("\r\n", "\n"));
        }

        /// <summary>
        /// <seealso cref="TextTemplateEngineMenuItem.ExpandTextTemplate(string)"/>
        /// </summary>
        [Test]
        public void EdgeCasePasses()
        {
            var srcText = @"
////@@ Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset";
            var destText = @"
////@@ Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset
Apple is Fruits
Cat is Animal
////-- Finish Packages/com.tositeru.hinode/Tests/Editor/Tools/TextTemplateEngine/ForTextTemplateEngineMenuItemTest.asset
".Replace("\r\n", "\n");
            Assert.AreEqual(destText, TextTemplateEngineMenuItem.ExpandTextTemplate(srcText).text.Replace("\r\n", "\n"));
        }

    }
}
