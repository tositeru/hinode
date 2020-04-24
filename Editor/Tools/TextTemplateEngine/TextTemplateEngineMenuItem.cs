using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Hinode.Editors
{
    public class TextTemplateEngineMenuItem
    {
        static readonly string EXPANDED_TEXT_TEMPLATE_KEYWORD = "////@@";
        static readonly string END_EXPANDED_TEXT_TEMPLATE_KEYWORD = "////-- Finish";

        [MenuItem("Assets/Hinode/Expand TextTemplate in Selecting Script Files", true)]
        public static bool ValidateExpandTextTemplateInScripts()
        {
            return GetSelectingScriptAssetPath().Any();
        }

        [MenuItem("Assets/Hinode/Expand TextTemplate in Selecting Script Files")]
        public static void ExpandTextTemplateInScripts()
        {
            var log = GetSelectingScriptAssetPath()
                .Aggregate("", (_s, _c) => _s + _c + ";");
            Debug.Log($"selecting scripts: {log}");

            foreach(var assetPath in GetSelectingScriptAssetPath())
            {
                try
                {
                    var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    var (text, isSuccess) = ExpandTextTemplate(textAsset.text);
                    if(isSuccess)
                    {
                        var filepath = EditorFileUtils.GetFullFilepath(assetPath);
                        File.WriteAllText(filepath, text);
                        AssetDatabase.ImportAsset(assetPath);
                    }
                }
                catch(System.Exception)
                {
                    Debug.LogWarning($"Failed to Expand TextTemplate in {assetPath}...");
                }
            }
        }

        public static (string text, bool isEdit) ExpandTextTemplate(string srcText)
        {
            bool isEdit = false;
            int pos = 0;
            var text = "";
            while (pos < srcText.Length)
            {
                var s = srcText.IndexOf(EXPANDED_TEXT_TEMPLATE_KEYWORD, pos);
                if (s == -1)
                {
                    text += srcText.Substring(pos, srcText.Length - pos);
                    break;
                }
                var useTextTemplateFilepathStart = s + EXPANDED_TEXT_TEMPLATE_KEYWORD.Length;
                var useTextTemplateFilepathEnd = srcText.IndexOf("\n", useTextTemplateFilepathStart);
                if (useTextTemplateFilepathEnd == -1)
                    useTextTemplateFilepathEnd = srcText.Length;

                var useTextTemplateFilepath = srcText.Substring(useTextTemplateFilepathStart, useTextTemplateFilepathEnd - useTextTemplateFilepathStart);

                var removeHeadSpaceRX = new Regex(@"^\s*|\s$", RegexOptions.Singleline);
                useTextTemplateFilepath = removeHeadSpaceRX.Replace(useTextTemplateFilepath, "");

                var useTextTemplate = AssetDatabase.LoadAssetAtPath<TextTemplateEngine>(useTextTemplateFilepath);
                if (useTextTemplate == null)
                {
                    Debug.LogError($"Failed to load TextTemplateEngine... assetPath='{useTextTemplateFilepath}'");
                    break;
                }

                text += srcText.Substring(pos, useTextTemplateFilepathEnd - pos) + "\n";
                text += useTextTemplate.Generate() + System.Environment.NewLine;
                text += $"{END_EXPANDED_TEXT_TEMPLATE_KEYWORD} {useTextTemplateFilepath}" + System.Environment.NewLine;

                var e = srcText.IndexOf(END_EXPANDED_TEXT_TEMPLATE_KEYWORD, s);
                if (e != -1)
                {
                    e = srcText.IndexOf("\n", e) + 1;
                }
                else
                {
                    e = useTextTemplateFilepathEnd + 1;
                }

                isEdit = true;
                pos = e;
            }
            return (text, isEdit);
        }

        static IEnumerable<string> GetSelectingScriptAssetPath()
        {
            return Selection.assetGUIDs
                .Select(_guid => AssetDatabase.GUIDToAssetPath(_guid))
                .Where(_path => Path.GetExtension(_path) == ".cs");
        }
    }
}
