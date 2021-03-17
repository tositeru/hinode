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
using System.Linq;
using System.IO;

namespace Hinode.Editors
{
    /// <summary>
    /// 
    /// </summary>
    public static class SelectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetSelectingScriptAssetPath()
        {
            return Selection.assetGUIDs
                .Select(_guid => AssetDatabase.GUIDToAssetPath(_guid))
                .Where(_path => Path.GetExtension(_path) == ".cs");
        }

    }
}
