using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// Sceneファイルへのパスを表す文字列に指定するAttribute
    /// Editor上でBuildingSettingsに登録されているSceneへのPopupを表示するようにUIが切り替わります。
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ScenePathAttribute : PropertyAttribute
    {
        public ScenePathAttribute()
        {
        }
    }
}
