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
        public void EmbbedTemplatePasses()
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
            var newline = templateEngine.NewLineStr;
            Assert.AreEqual("This Pen is Good. %Yell%" + newline
                + "This Pen is Nice. %Yell%" + newline
                + "This Grass is Nice. %Yell%", templateEngine.Generate());
        }

        [Test, Description("他のTextTemplateEngineのキーワードと無視リストを使用する時のテスト")]
        public void UseOtherTextTemplatePasses()
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

            var otherTemplateEngine = ScriptableObject.CreateInstance<TextTemplateEngine>();
            otherTemplateEngine.TemplateText = "";
            otherTemplateEngine.AddKeyword("Item", "Tom", "Kumi");
            otherTemplateEngine.AddKeyword("Condition", "Hot", "Cool");
            otherTemplateEngine.AddIgnorePair(("Item", "Tom"), ("Condition", "Hot"));

            var newline = templateEngine.NewLineStr;
            Assert.AreEqual("This Kumi is Hot. Apple! Apple! Apple!." + newline
                + "This Tom is Cool. Apple! Apple! Apple!." + newline
                + "This Kumi is Cool. Apple! Apple! Apple!.", templateEngine.Generate(otherTemplateEngine));
        }

        [Test, Description("埋め込みTextTemplateEngineを展開する時、設定されているキーワード等を共有する場合テスト")]
        public void DoShareKeywordsPasses()
        {
            var childEmbbedTemplate = ScriptableObject.CreateInstance<TextTemplateEngine>();
            childEmbbedTemplate.TemplateText = "?$Key$?";
            childEmbbedTemplate.AddKeyword("Key", "Apple");

            var embbedTemplate = ScriptableObject.CreateInstance<TextTemplateEngine>();
            embbedTemplate.TemplateText = "$Item$=>$Condition$! $NotExpanded$!. %Yell%;";
            embbedTemplate.AddKeyword("Item", "Cat");
            embbedTemplate.AddKeyword("Condition", "Sleep");
            embbedTemplate.AddKeyword("NotExpanded", "Apple"); // <- これは使用されない
            embbedTemplate.AddEmbbed("Yell", childEmbbedTemplate); // <- embbedTemplate.DoShareKaywords=falseの時はこちらは使用される。

            var templateEngine = ScriptableObject.CreateInstance<TextTemplateEngine>();
            templateEngine.TemplateText = "This $Item$ is $Condition$. %Yell%";
            templateEngine.AddKeyword("Item", "Pen", "Grass");
            templateEngine.AddKeyword("Condition", "Good", "Nice");
            templateEngine.AddIgnorePair(("Item", "Grass"), ("Condition", "Good"));
            templateEngine.AddIgnorePair(("Item", "Pen"), ("Condition", "Nice"));
            templateEngine.AddEmbbed("Yell", embbedTemplate);

            templateEngine.DoShareKaywords = true;
            var newline = templateEngine.NewLineStr;
            var expanedText = $"Pen=>Good! $NotExpanded$!. ?Apple?;{newline}Grass=>Nice! $NotExpanded$!. ?Apple?;";
            Assert.AreEqual(
                    $"This Pen is Good. {expanedText}" + newline
                + $"This Grass is Nice. {expanedText}", templateEngine.Generate());
        }

        [Test, Description("単一のキーワードのペアを指定するモードのテスト")]
        public void IsSingleKeywordPairPasses()
        {
            var templateEngine = ScriptableObject.CreateInstance<TextTemplateEngine>();
            templateEngine.TemplateText = "This $Item$ is $Condition$.";
            templateEngine.AddKeyword("Item");
            templateEngine.AddKeyword("Condition");
            templateEngine.AddSingleKeywordPair("Apple", "Fruits");
            templateEngine.AddSingleKeywordPair("Cat", "Animals");

            //これらは設定していても使用されない
            templateEngine.AddIgnorePair(("Item", "Grass"), ("Condition", "Good"));
            templateEngine.AddIgnorePair(("Item", "Pen"), ("Condition", "Nice"));
            //

            templateEngine.IsSingleKeywordPairMode = true;

            var newline = templateEngine.NewLineStr;
            Assert.AreEqual(
                    $"This Apple is Fruits." + newline
                + $"This Cat is Animals.", templateEngine.Generate());
        }

        [Test, Description("単一のキーワードのペアを指定するモードのテスト")]
        public void IsOnlyEmbbedPasses()
        {
            var embbedTemplate = ScriptableObject.CreateInstance<TextTemplateEngine>();
            embbedTemplate.TemplateText = "$Word$! $Word$!.";
            embbedTemplate.AddKeyword("Word", "Apple");

            var templateEngine = ScriptableObject.CreateInstance<TextTemplateEngine>();
            templateEngine.DoShareKaywords = true;
            templateEngine.IsSingleKeywordPairMode = true;
            templateEngine.IsOnlyEmbbed = true;

            templateEngine.TemplateText = "This $Item$ is $Condition$. %Yell%";
            templateEngine.AddKeyword("Item");
            templateEngine.AddKeyword("Condition");
            templateEngine.AddKeyword("Word");
            templateEngine.AddSingleKeywordPair("", "", "Good");
            templateEngine.AddSingleKeywordPair("", "", "Nice");
            templateEngine.AddEmbbed("Yell", embbedTemplate);
            var newline = templateEngine.NewLineStr;
            Assert.AreEqual($"This $Item$ is $Condition$. Good! Good!." + newline
                + "Nice! Nice!.", templateEngine.Generate());
        }

    }
}
