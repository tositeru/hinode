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
        public static void CreateDirectory(string filepath)
        {
            var fullpath = Path.GetFullPath(filepath);
            Assert.IsTrue(fullpath.Contains(Application.dataPath), $"プロジェクトのAssetsディレクトリに所属していません。{filepath}");

            var path = fullpath.Remove(0, Path.GetDirectoryName(Application.dataPath).Length+1);
            var ext = Path.GetExtension(path);
            if ("" != ext)
            {
                path = Path.GetDirectoryName(path);
            }

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
    }
}
