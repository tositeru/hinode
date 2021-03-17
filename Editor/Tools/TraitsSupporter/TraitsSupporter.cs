// Copyright 2019 ~ tositeru
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace Hinode.Editors
{
    /// <summary>
    /// 
    /// </summary>
    public class TraitsSupporter
    {
        static public readonly string LOG_SELECTOR = "TraitsSupporter";
        static public readonly string LOG_PREFIX = LOG_SELECTOR + ": ";

        public Dictionary<string, string> TraitsTemplates { get; set; } = new Dictionary<string, string>();
        public string RootDir { get; set; } = "";
        string Filepath { get; set; }

        static readonly Regex REGEX_SearchLine = new Regex(@"^(\s*)////(.+?)\s+(.+?)$", RegexOptions.Multiline);

        const string TRAITS_EXPAND_PREFIX = "@@";
        const string TRAITS_EXPAND_END_PREFIX = "--";

        const string TRAITS_POINT_KEYWORD = "traits";
        const string TRAITS_ENDPOINT_KEYWORD = "Finish traits";
        static readonly Regex REGEX_SearchExpandPoints = new Regex(@"^(\s*?)////@@\s+?traits\s+?(.+?)$", RegexOptions.Multiline);
        static readonly Regex REGEX_PickUpElement = new Regex(@"\$?([\w_/\¥\.]+)");
        static readonly Regex REGEX_SearchExpandEndPoints = new Regex(@"^(\s*?)////-- Finish traits\s+\$?([\w_/¥¥\.]+)\s*\$([\w_]+)?$", RegexOptions.Multiline);
        static readonly Regex REGEX_SearchDefineTraits = new Regex(@"^(\s*?)////\s+?\$([\w_]+)", RegexOptions.Multiline);
        static readonly Regex REGEX_SearchDefineTraitsEnd = new Regex(@"^(\s*?)////\s+?End\s+?\$([\w_]+)", RegexOptions.Multiline);

        #region Custom UnityEditor
        [MenuItem("Assets/Hinode/Expand Traits in Selecting Script Files", true)]
        public static bool ValidateExpandTraitsInScripts()
        {
            return SelectionExtensions.GetSelectingScriptAssetPath().Any();
        }

        [MenuItem("Assets/Hinode/Expand Traits in Selecting Script Files")]
        public static void ExpandTraitsInScripts()
        {
            var log = SelectionExtensions.GetSelectingScriptAssetPath()
                .Aggregate("", (_s, _c) => _s + _c + ";");
            Logger.Log(Logger.Priority.High, () => $"{LOG_PREFIX}selecting scripts: {log}", LOG_SELECTOR);

            foreach (var assetPath in SelectionExtensions.GetSelectingScriptAssetPath())
            {
                try
                {
                    var traitsSupporter = new TraitsSupporter();
                    var result = traitsSupporter.ExpandInFile(assetPath);

                    File.WriteAllText(traitsSupporter.Filepath, result);
                    AssetDatabase.ImportAsset(assetPath);
                }
                catch (System.Exception e)
                {
                    Logger.LogError(Logger.Priority.High
                        , () => $"Failed to Expand TextTemplate in {assetPath}..." + System.Environment.NewLine
                            + "-----"
                            + e
                            + "-----"
                        , LOG_SELECTOR);
                    Debug.LogWarning($"Failed to Expand TextTemplate in {assetPath}...");
                }
            }
        }

        #endregion
        public string Expand(string source)
        {
            Filepath = "";

            return ExpandImpl(source);
        }

        public string ExpandInFile(string filepath)
        {
            Filepath = Path.Combine(RootDir, filepath);
            if (!File.Exists(Filepath))
            {
                throw new System.ArgumentException($"{LOG_PREFIX}Not exist filepaht('{Filepath}')... Please see also other logs!");
            }
            var source = File.ReadAllText(Filepath);
            return ExpandImpl(source);
        }

        string ExpandImpl(string source)
        {
            var result = "";

            int pos = 0;
            while(pos < source.Length)
            {
                var match = REGEX_SearchExpandPoints.Match(source, pos);
                if (!match.Success)
                {
                    break;
                }

                // append skip text
                int endPos = source.GoNext('\n', match.Index + match.Length, 1);
                result += source.Substring(pos, endPos - pos);

                var newlineStr = result.GetNewlineStr(result.Length - 1);

                var appendIndent = match.Groups[1].ToString().TrimStart('\n', '\r');
                var elementsStr = match.Groups[2].ToString();

                var (keyword, filepath) = PickUpFromElements(elementsStr);
                if(string.IsNullOrWhiteSpace(keyword))
                {
                    throw new System.ArgumentException($"{LOG_PREFIX}Don't exist Traits'keywords... Please see alse other logs! key='{keyword}' filepath='{filepath}'");
                }

                if(!SearchExpandText(keyword, filepath, out var expandText))
                {
                    throw new System.ArgumentException($"{LOG_PREFIX}Fail to get Traits('{keyword}')... Please see alse other logs! line({source.GetLineNo(match.Index)})");
                }
                // append indent to lines
                {
                    foreach(var lineParam in expandText.GetLineEnumerable())
                    {
                        if (lineParam.IsEmptyOrWhiteSpaceLine)
                        {
                            //not insert indents when only space char line
                            result += lineParam.Newline;
                        }
                        else
                        {
                            result += appendIndent + lineParam.GetLine() + lineParam.Newline;
                        }
                    }
                    if (result[result.Length - 1] != '\n')
                        result += newlineStr;
                    result += appendIndent + $"////-- Finish traits {match.Groups[2]}" + newlineStr;
                }

                //move to expand point end
                // skip other traits if the terminal line of the traits exist
                pos = MoveToEndPoint(source, endPos, (keyword, filepath));
            }

            // append reminded text in source
            if (pos < source.Length)
            {
                result += source.Substring(pos);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="filepath"></param>
        /// <param name="expandText"></param>
        /// <returns></returns>
        bool SearchExpandText(string keyword, string filepath, out string expandText)
        {
            expandText = null;
            if (TraitsTemplates.ContainsKey(keyword))
            {//case: use buildin template
                var template = TraitsTemplates[keyword];
                expandText = RemoveIndents(template, 0, template.Length);
                return true;
            }

            var fullFilepath = EditorFileUtils.GetFullFilepath(filepath);
            if(!File.Exists(fullFilepath))
            {
                Logger.LogWarning(Logger.Priority.High, () => $"{LOG_PREFIX}Not exist filepath('{fullFilepath}')...", LOG_SELECTOR);
                return false;
            }
            var text = File.ReadAllText(fullFilepath);

            // find traits define to match 'keyword'
            var defineTraitsEnumerable = REGEX_SearchDefineTraits.GetMatchEnumerable(text)
                .Where(_m => {
                    return _m.Groups[2].ToString() == keyword;
                });
            var match = defineTraitsEnumerable.FirstOrDefault();
            if(match == null)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"{LOG_PREFIX}Not define Traits({keyword}) in filepath({filepath})...", LOG_SELECTOR);
                expandText = null;
                return false;
            }

            //check to exist traits define endpoint
            var endMatch = REGEX_SearchDefineTraitsEnd.GetMatchEnumerable(text, match.Index + match.Length)
                .Where(_m => _m.Groups[2].ToString() == keyword)
                .FirstOrDefault();
            if (endMatch == null)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"{LOG_PREFIX}Not exist Traits({keyword}) terminal in filepath({filepath})...", LOG_SELECTOR);
                expandText = null;
                return false;
            }

            // check to duplicate define
            if (defineTraitsEnumerable.Count() >= 2)
            {
                Logger.LogWarning(Logger.Priority.High, () => {
                    return $"{LOG_PREFIX}Traits({keyword}) define Duplicate...";
                }, LOG_SELECTOR);
                expandText = null;
                return false;
            }

            //remove indents in traits's define
            expandText = RemoveIndents(text, text.GoNext('\n', match.Index + match.Length, 1), endMatch.Index);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineElements"></param>
        /// <returns></returns>
        (string keyword, string filepath) PickUpFromElements(string lineElements)
        {
            string keyword = null;
            string filepath = "";
            var elementsMatches = REGEX_PickUpElement.Matches(lineElements);
            switch (elementsMatches.Count)
            {
                case 1:// indent and keyword
                    keyword = elementsMatches[0].Groups[1].ToString();
                    if(!string.IsNullOrWhiteSpace(Filepath))
                    {// pick up a traits from the parsing file
                        filepath = Filepath;
                    }
                    break;
                case 2:// indent, filepath and keyword
                    keyword = elementsMatches[1].Groups[1].ToString();
                    filepath = elementsMatches[0].Groups[1].ToString();
                    filepath = Path.Combine(RootDir, filepath);
                    break;
            }
            return (keyword, filepath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        string RemoveIndents(string text, int startPos, int endPos)
        {
            var newText = "";
            foreach(var lineParam in text.GetLineEnumerable(startPos, endPos))
            {
                newText += lineParam.GetLine().Trim() + lineParam.Newline;
            }
            return newText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="start"></param>
        /// <param name="traitsParam"></param>
        /// <returns></returns>
        int MoveToEndPoint(string source, int start, (string keyword, string filepath) traitsParam)
        {
            int pos = start;
            int nestCount = 0;
            foreach (var lineMatch in REGEX_SearchLine.GetMatchEnumerable(source, pos))
            {
                var elementsStr = lineMatch.Groups[3].ToString();
                var prefix = lineMatch.Groups[2].ToString();

                {// remove prefix from 'elementsStr'
                    var traitsPrefix = "";
                    if (prefix == TRAITS_EXPAND_END_PREFIX)
                    {
                        traitsPrefix = TRAITS_ENDPOINT_KEYWORD;
                    }
                    else if(prefix == TRAITS_EXPAND_PREFIX)
                    {
                        traitsPrefix = TRAITS_POINT_KEYWORD;
                    }
                    else
                    {
                        continue;
                    }

                    var index = elementsStr.IndexOf(traitsPrefix);
                    if (index != -1)
                    {
                        elementsStr = elementsStr.Substring(index + traitsPrefix.Length);
                    }
                }

                var otherElements = PickUpFromElements(elementsStr);
                if (string.IsNullOrWhiteSpace(otherElements.keyword))
                    continue;
                if (otherElements.keyword != traitsParam.keyword
                    || otherElements.filepath != traitsParam.filepath)
                    continue;

                if (TRAITS_EXPAND_PREFIX == prefix)
                {
                    nestCount++;
                }
                else if (TRAITS_EXPAND_END_PREFIX == prefix)
                {
                    nestCount--;
                    if (nestCount < 0)
                    {
                        pos = source.GoNext('\n', lineMatch.Index, 1);
                        break;
                    }
                }
            }

            if (nestCount > 0)
            {
                throw new System.ArgumentException($"{LOG_PREFIX}Syntax error... Please see alse other logs! line({source.GetLineNo(pos)})");
            }
            return pos;
        }
    }
}
