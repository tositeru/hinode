using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Hinode.Tests.MVC.Views
{
    /// <summary>
    /// <seealso cref="TextViewObject"/>
    /// </summary>
    public class TestTextViewObject : TestBase
    {
        [UnityTest]
        public IEnumerator FixedParamBinderUpdatePasses()
        {
            yield return null;
            var text = TextViewObject.Create();

            ////@@ Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/TextViewObject/TestFixedParamBinderPasses.asset
{//AlignByGeometry
    var paramBinder = new TextViewObject.FixedParamBinder();
    paramBinder.AlignByGeometry = true;
    paramBinder.Update(null, text);
    yield return null;
    Assert.AreEqual(paramBinder.AlignByGeometry, text.Text.alignByGeometry);
}

{//Alignment
    var paramBinder = new TextViewObject.FixedParamBinder();
    paramBinder.Alignment = TextAnchor.MiddleCenter;
    paramBinder.Update(null, text);
    yield return null;
    Assert.AreEqual(paramBinder.Alignment, text.Text.alignment);
}

{//Color
    var paramBinder = new TextViewObject.FixedParamBinder();
    paramBinder.Color = Color.blue;
    paramBinder.Update(null, text);
    yield return null;
    Assert.AreEqual(paramBinder.Color, text.Text.color);
}

{//FontSize
    var paramBinder = new TextViewObject.FixedParamBinder();
    paramBinder.FontSize = 22;
    paramBinder.Update(null, text);
    yield return null;
    Assert.AreEqual(paramBinder.FontSize, text.Text.fontSize);
}

{//FontStyle
    var paramBinder = new TextViewObject.FixedParamBinder();
    paramBinder.FontStyle = FontStyle.Italic;
    paramBinder.Update(null, text);
    yield return null;
    Assert.AreEqual(paramBinder.FontStyle, text.Text.fontStyle);
}

{//HorizontalOverflow
    var paramBinder = new TextViewObject.FixedParamBinder();
    paramBinder.HorizontalOverflow = HorizontalWrapMode.Overflow;
    paramBinder.Update(null, text);
    yield return null;
    Assert.AreEqual(paramBinder.HorizontalOverflow, text.Text.horizontalOverflow);
}

{//VerticalOverflow
    var paramBinder = new TextViewObject.FixedParamBinder();
    paramBinder.VerticalOverflow = VerticalWrapMode.Overflow;
    paramBinder.Update(null, text);
    yield return null;
    Assert.AreEqual(paramBinder.VerticalOverflow, text.Text.verticalOverflow);
}

{//LineSpacing
    var paramBinder = new TextViewObject.FixedParamBinder();
    paramBinder.LineSpacing = 33;
    paramBinder.Update(null, text);
    yield return null;
    Assert.AreEqual(paramBinder.LineSpacing, text.Text.lineSpacing);
}

{//Text
    var paramBinder = new TextViewObject.FixedParamBinder();
    paramBinder.Text = "Apple";
    paramBinder.Update(null, text);
    yield return null;
    Assert.AreEqual(paramBinder.Text, text.Text.text);
}

{//SupportRichText
    var paramBinder = new TextViewObject.FixedParamBinder();
    paramBinder.SupportRichText = true;
    paramBinder.Update(null, text);
    yield return null;
    Assert.AreEqual(paramBinder.SupportRichText, text.Text.supportRichText);
}

////-- Finish Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/TextViewObject/TestFixedParamBinderPasses.asset

        }
    }
}
