using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Editors;
using System.IO;

namespace Hinode.Tests.Editors
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

            var json = JsonUtility.ToJson(templateEngine);
            File.WriteAllText("test.json", json);
        }

    }
}
