using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Hinode.Tests;

namespace Hinode.MVC.Tests.Views
{
    /// <summary>
    /// <seealso cref="ImageViewObject"/>
    /// </summary>
    public class TestImageViewObject : TestBase
    {
        [UnityTest]
        public IEnumerator FixedParamBinderUpdatePasses()
        {
            yield return null;
            var image = ImageViewObject.Create();

            ////@@ Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/ImageViewObject/TestFixedParamBinderPasses.asset
{//Color
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.Color = Color.red;
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.Color, image.Image.color);
}

{//FillAmount
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.FillAmount = 0.75f;
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.FillAmount, image.Image.fillAmount);
}

{//FillCenter
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.FillCenter = true;
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.FillCenter, image.Image.fillCenter);
}

{//FillClockwise
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.FillClockwise = true;
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.FillClockwise, image.Image.fillClockwise);
}

{//FillMethod
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.FillMethod = Image.FillMethod.Radial180;
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.FillMethod, image.Image.fillMethod);
}

{//FillOrigin
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.FillOrigin = 3;
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.FillOrigin, image.Image.fillOrigin);
}

{//PixelsPerUnitMultiplier
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.PixelsPerUnitMultiplier = 1.2f;
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.PixelsPerUnitMultiplier, image.Image.pixelsPerUnitMultiplier);
}

{//PreserveAspect
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.PreserveAspect = true;
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.PreserveAspect, image.Image.preserveAspect);
}

{//Sprite
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.Sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.zero);
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.Sprite, image.Image.sprite);
}

{//Type
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.Type = Image.Type.Sliced;
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.Type, image.Image.type);
}

{//UseSpriteMesh
    var paramBinder = new ImageViewObject.FixedParamBinder();
    paramBinder.UseSpriteMesh = true;
    paramBinder.Update(null, image);
    yield return null;
    Assert.AreEqual(paramBinder.UseSpriteMesh, image.Image.useSpriteMesh);
}

////-- Finish Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/ImageViewObject/TestFixedParamBinderPasses.asset

        }
    }
}
