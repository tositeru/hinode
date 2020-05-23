using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hinode
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    [AvailableModelViewParamBinder(typeof(ImageViewObject.FixedParamBinder))]
    [DisallowMultipleComponent()]
    public class ImageViewObject : RectTransformViewObject
        , RectTransformViewObject.IOptionalViewObject
    {
        public static new ImageViewObject Create(string name = "UIImage")
        {
            var obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            return obj.AddComponent<ImageViewObject>();
        }

        public Image Image { get => gameObject.GetOrAddComponent<Image>(); }

        public new class FixedParamBinder : RectTransformViewObject.FixedParamBinder
            , RectTransformViewObject.IOptionalViewObjectParamBinder
        {
            public bool Contains(Params paramType)
                => Contains(paramType.ToString());

            public FixedParamBinder Set(Params param, object value)
                => Set(param.ToString(), value) as FixedParamBinder;
            public FixedParamBinder Set<T>(Params param, T value)
                => Set(param.ToString(), value) as FixedParamBinder;

            public T Get<T>(Params param)
                => (T)Get(param.ToString());

            public FixedParamBinder Delete(Params param)
                => Delete(param.ToString()) as FixedParamBinder;

            protected override void UpdateImpl(Model model, IViewObject viewObj)
            {
                var text = viewObj as ImageViewObject;
                UpdateParams(text);
            }

            #region RectTransform.IOptionalViewObjectParamBinder
            public RectTransformViewObject.IOptionalViewObject AppendTo(GameObject target)
            {
                return target.AddComponent<ImageViewObject>();
            }
            #endregion

            ////@@ Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/ImageViewObject/DefineParamsTemplate.asset
public enum Params
{
Color,
FillAmount,
FillCenter,
FillClockwise,
FillMethod,
FillOrigin,
PixelsPerUnitMultiplier,
PreserveAspect,
Sprite,
Type,
UseSpriteMesh,
}

public Color Color { get => Get<Color>(Params.Color); set => Set(Params.Color, value); }
public float FillAmount { get => Get<float>(Params.FillAmount); set => Set(Params.FillAmount, value); }
public bool FillCenter { get => Get<bool>(Params.FillCenter); set => Set(Params.FillCenter, value); }
public bool FillClockwise { get => Get<bool>(Params.FillClockwise); set => Set(Params.FillClockwise, value); }
public Image.FillMethod FillMethod { get => Get<Image.FillMethod>(Params.FillMethod); set => Set(Params.FillMethod, value); }
public int FillOrigin { get => Get<int>(Params.FillOrigin); set => Set(Params.FillOrigin, value); }
public float PixelsPerUnitMultiplier { get => Get<float>(Params.PixelsPerUnitMultiplier); set => Set(Params.PixelsPerUnitMultiplier, value); }
public bool PreserveAspect { get => Get<bool>(Params.PreserveAspect); set => Set(Params.PreserveAspect, value); }
public Sprite Sprite { get => Get<Sprite>(Params.Sprite); set => Set(Params.Sprite, value); }
public Image.Type Type { get => Get<Image.Type>(Params.Type); set => Set(Params.Type, value); }
public bool UseSpriteMesh { get => Get<bool>(Params.UseSpriteMesh); set => Set(Params.UseSpriteMesh, value); }

void UpdateParams(ImageViewObject image)
{
if (Contains(Params.Color)) image.Image.color = Color;
if (Contains(Params.FillAmount)) image.Image.fillAmount = FillAmount;
if (Contains(Params.FillCenter)) image.Image.fillCenter = FillCenter;
if (Contains(Params.FillClockwise)) image.Image.fillClockwise = FillClockwise;
if (Contains(Params.FillMethod)) image.Image.fillMethod = FillMethod;
if (Contains(Params.FillOrigin)) image.Image.fillOrigin = FillOrigin;
if (Contains(Params.PixelsPerUnitMultiplier)) image.Image.pixelsPerUnitMultiplier = PixelsPerUnitMultiplier;
if (Contains(Params.PreserveAspect)) image.Image.preserveAspect = PreserveAspect;
if (Contains(Params.Sprite)) image.Image.sprite = Sprite;
if (Contains(Params.Type)) image.Image.type = Type;
if (Contains(Params.UseSpriteMesh)) image.Image.useSpriteMesh = UseSpriteMesh;
}

////-- Finish Packages/com.tositeru.hinode/Editor/Assets/MVC/Views/ImageViewObject/DefineParamsTemplate.asset
        }
    }
}
