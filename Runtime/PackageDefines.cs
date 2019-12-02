using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Hinode
{
    /// <summary>
    /// Hinodeパッケージに関連する定数など定義する
    /// </summary>
    public static class PackageDefines
    {
        public static readonly string PACKAGE_ASSET_ROOT_PATH = "Packages/com.tositeru.hinode";

        /// <summary>
        /// このパッケージのアセットへのパスを取得する
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string GetHinodeAssetPath(params string[] paths)
        {
            return Path.Combine(PACKAGE_ASSET_ROOT_PATH, Path.Combine(paths));
        }
    }
}
