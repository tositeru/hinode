using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Editors;
using System.IO;

namespace Hinode.Tests.Editors.Tools
{
    public class TestTextTemplateEngine : TestBase
    {
        // A Test behaves as an ordinary method
        [Test]
        public void SimpleUsagePasses()
        {
            var templateEngine = ScriptableObject.CreateInstance<TextTemplateEngine>();

            templateEngine.TemplateText = @"This $Item$ is $Condition$.";

            Assert.AreEqual("This $Item$ is $Condition$.", templateEngine.Generate(), "To not matched keyword do nothing.");

            templateEngine.AddKeyword("Item", "Pen");
            templateEngine.AddKeyword("Condition", "Good");

            Assert.AreEqual("This Pen is Good.", templateEngine.Generate(), "Matched keywords replace it's value.");
        }

        [Test]
        public void KeywordWithMultipleValueUsagePasses()
        {
            var templateEngine = ScriptableObject.CreateInstance<TextTemplateEngine>();

            templateEngine.TemplateText = @"This $Item$ is $Condition$.";
            templateEngine.AddKeyword("Item", "Pen", "Grass");
            templateEngine.AddKeyword("Condition", "Good", "Nice");

            var generatedText = templateEngine.Generate();
            var newline = templateEngine.NewLineStr;
            Assert.AreEqual("This Pen is Good." + newline
                + "This Grass is Good." + newline
                + "This Pen is Nice." + newline
                + "This Grass is Nice.", generatedText);
        }

        [Test]
        public void IgnorePairPasses()
        {
            var templateEngine = ScriptableObject.CreateInstance<TextTemplateEngine>();

            templateEngine.TemplateText = @"This $Item$ is $Condition$.";
            templateEngine.AddKeyword("Item", "Pen", "Grass");
            templateEngine.AddKeyword("Condition", "Good", "Nice");

            templateEngine.AddIgnorePair(("Item", "Grass"), ("Condition", "Good"));

            var generatedText = templateEngine.Generate();
            var newline = templateEngine.NewLineStr;
            Assert.AreEqual("This Pen is Good." + newline
                + "This Pen is Nice." + newline
                + "This Grass is Nice.", generatedText);
        }

        [Test, Description("値が設定されていない無視リストがある時のテスト")]
        public void EmptyIgnorePairPasses()
        {
            var templateEngine = ScriptableObject.CreateInstance<TextTemplateEngine>();

            templateEngine.TemplateText = @"This $Item$ is $Condition$.";
            templateEngine.AddKeyword("Item", "Pen", "Grass");
            templateEngine.AddKeyword("Condition", "Good", "Nice");

            templateEngine.AddIgnorePair(("Item", "Grass"), ("Condition", "Good"));

            templateEngine.AddIgnorePair(("", "Grass"), ("Condition", ""));

            var generatedText = templateEngine.Generate();
            var newline = templateEngine.NewLineStr;
            Assert.AreEqual("This $Item$ is $Condition$.", generatedText);
        }

        [Test]
        public void EmbbedOtherTemplatePasses()
        {
            var embbedTemplate = ScriptableObject.CreateInstance<TextTemplateEngine>();
            embbedTemplate.TemplateText = "$Word$! $Word$! $Word$!.";
            embbedTemplate.AddKeyword("Word", "Apple");

            var templateEngine = ScriptableObject.CreateInstance<TextTemplateEngine>();

            templateEngine.TemplateText = "This $Item$ is $Condition$. %Yell%";
            templateEngine.AddKeyword("Item", "Pen", "Grass");
            templateEngine.AddKeyword("Condition", "Good", "Nice");
            templateEngine.AddIgnorePair(("Item", "Grass"), ("Condition", "Good"));

            templateEngine.AddEmbbed("Yell", embbedTemplate);

            var newline = templateEngine.NewLineStr;
            Assert.AreEqual("This Pen is Good. Apple! Apple! Apple!." + newline
                + "This Pen is Nice. Apple! Apple! Apple!." + newline
                + "This Grass is Nice. Apple! Apple! Apple!.", templateEngine.Generate());
        }

        [Test, Description("キー名が空または値が設定されていない埋め込みテキストがあったときのテスト")]
        public void EmptyEmbbedTemplatePasses()
        {
            var embbedTemplate = ScriptableObject.CreateInstance<TextTemplateEngine>();
            embbedTemplate.TemplateText = "$Word$! $Word$! $Word$!.";
            embbedTemplate.AddKeyword("Word", "Apple");

            var templateEngine = ScriptableObject.CreateInstance<TextTemplateEngine>();

            templateEngine.TemplateText = "This $Item$ is $Condition$. %Yell%";
            templateEngine.AddKeyword("Item", "Pen", "Grass");
            templateEngine.AddKeyword("Condition", "Good", "Nice");
            templateEngine.AddIgnorePair(("Item", "Grass"), ("Condition", "Good"));

            //埋め込みテンプレートのキー名が空の時はそれを無視する
            templateEngine.AddEmbbed("", embbedTemplate);
            //埋め込みテンプレートの値が空の時もそれを無視する
            templateEngine.AddEmbbed("Yell", null);
            Assert.DoesNotThrow(() => {
                var newline = templateEngine.NewLineStr;
                Assert.AreEqual("This Pen is Good. %Yell%" + newline
                    + "This Pen is Nice. %Yell%" + newline
                    + "This Grass is Nice. %Yell%", templateEngine.Generate());
            });
        }
    }
}
