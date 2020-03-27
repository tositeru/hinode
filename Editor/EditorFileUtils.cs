using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    public static class EditorFileUtils
    {
        public static bool IsProjectAssetPath(string filepath)
        {
            var fullpath = Path.GetFullPath(filepath);
            return fullpath.Contains(Application.dataPath);
        }

        /// <summary>
        /// 指定したパッケージがあるかは判定していませんので注意してください
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool IsPackageAssetPath(string filepath)
        {
            return filepath.IndexOf("Packages") == 0;
        }

        /// <summary>
        /// アセットファイルが存在しているかどうか判定する関数
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool IsExistAsset(string filepath)
        {
            if (IsProjectAssetPath(filepath))
            {
                return File.Exists(filepath) || Directory.Exists(filepath);
            }
            else if (IsPackageAssetPath(filepath))
            {
                var obj = AssetDatabase.LoadAssetAtPath(filepath, typeof(Object));
                //Debug.Log($"debug -- IsExistAsset {filepath} isTrue={obj != null}");
                return obj != null;
            }
            else
            {
                return false;
            }
        }

        public static void CreateDirectory(string filepath)
        {
            var isProjectAsset = IsProjectAssetPath(filepath);
            var isPackageAsset = IsPackageAssetPath(filepath);
            Assert.IsTrue(isProjectAsset || isPackageAsset, $"プロジェクトのAssetsディレクトリまたパッケージディレクトリに所属していません。{filepath}");

            //ファイル名があるなら取り除く
            string path = filepath;
            var ext = Path.GetExtension(path);
            if ("" != ext)
            {
                path = Path.GetDirectoryName(path);
            }

            if (isProjectAsset)
            {
                var fullpath = Path.GetFullPath(path);
                path = fullpath.Remove(0, Path.GetDirectoryName(Application.dataPath).Length+1);

                var parentDir = "";
                foreach(var dirname in path.Split(Path.DirectorySeparatorChar))
                {
                    var dirpath = Path.Combine(parentDir, dirname);
                    if(!AssetDatabase.IsValidFolder(dirpath))
                    {
                        AssetDatabase.CreateFolder(parentDir, dirname);
                    }
                    parentDir = dirpath;
                }
            }
            else if(isPackageAsset)
            {
                var dirnames = path.Split(Path.DirectorySeparatorChar);
                var parentDir = Path.Combine(dirnames[0], dirnames[1]);
                for(var i=2; i<dirnames.Length; ++i)
                {
                    var dirpath = Path.Combine(parentDir, dirnames[i]);
                    if (!AssetDatabase.IsValidFolder(dirpath))
                    {
                        AssetDatabase.CreateFolder(parentDir, dirnames[i]);
                    }
                    parentDir = dirpath;
                }
            }
            else
            {
                throw new System.NotImplementedException();
            }

        }
    }
}
