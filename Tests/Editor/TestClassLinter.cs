using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

namespace Hinode.Tests.Editors
{
    /// <summary>
    /// TODO C#パーサーの実装を行ってから実装する予定です。
    /// </summary>
    public class TestClassLinter
    {
        /// <summary>
        /// Testsフォルダーに存在するものスクリプトのみを処理対象にします。
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Hinode/Lint Test Class in selecting files", true)]
        static bool ValidateTestScriptLinter()
        {
            return GetSelectingScriptAssetPath()
                .Any(_path => -1 != Path.GetDirectoryName(_path).IndexOf("Tests"));
        }

        /// <summary>
        /// TestClassのドキュメントを整理します。
        /// </summary>
        [MenuItem("Assets/Hinode/Lint Test Class in selecting files")]
        public static void LintTestScript()
        {
            Logger.LogWarning(Logger.Priority.High, () => "This Function will implement after Implement C Sharp Parser.");
        }

        static IEnumerable<string> GetSelectingScriptAssetPath()
        {
            return Selection.assetGUIDs
                .Select(_guid => AssetDatabase.GUIDToAssetPath(_guid))
                .Where(_path => Path.GetExtension(_path) == ".cs");
        }
    }
}
