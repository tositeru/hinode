using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    public interface IHavingDialogData
    {
        HavingTextResourceData DialogTitleTextResource { get; set; }
        HavingTextResourceData DialogTextTextResource { get; set; }
    }

    public static partial class IHavingDialogDataExtensions
    {
        public static void SetTexts(this IHavingDialogData data, DialogModel target, TextResources textResources)
        {
            if (textResources.Contains(data.DialogTitleTextResource)) target.Title = textResources.Get(data.DialogTitleTextResource);
            if (textResources.Contains(data.DialogTextTextResource)) target.Text = textResources.Get(data.DialogTextTextResource);
            Debug.Log($"pass Title={target.Title} Text={target.Text}");
        }
    }
}