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
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Editors;
using System.Diagnostics;
using System.IO;

namespace Hinode.Tests.Editors.Tools
{
    /// <summary>
	/// <seealso cref="Hinode.Editors.TraitsSupporter"/>
	/// </summary>
    public class TestTraitsSupporter
    {
        const int ORDER_Basic = 0;
        const int ORDER_AfterBasic = ORDER_Basic + 100;
        
        /// <summary>
		/// <seealso cref="TraitsSupporter.Expand(string)"/>
		/// </summary>
        [Test, Order(ORDER_Basic)]
        public void Expand_Passes()
        {
            var sourceCode =
@"using System.IO;
public class Apple
{
    ////@@ traits $Traits
}
";
            var traitsText =
@"#region $Traits
  // this comment must keep but not keep prefix spaces.
public int TraitsValue { get; set; }

#endregion";

            var traitsSupporter = new TraitsSupporter()
            {
                TraitsTemplates = new Dictionary<string, string>() {
                    { "Traits", traitsText },
                }
            };
            var result = traitsSupporter.Expand(sourceCode);

            // Indent was included Test target!
            var correctText =
@"using System.IO;
public class Apple
{
    ////@@ traits $Traits
    #region $Traits
    // this comment must keep but not keep prefix spaces.
    public int TraitsValue { get; set; }

    #endregion
    ////-- Finish traits $Traits
}
".Replace("\r\n", "\n");

            Logger.Log(Logger.Priority.High, () => $"result -- {System.Environment.NewLine}{result}");
            Assert.AreEqual(correctText, result.Replace("\r\n", "\n"));
        }

        /// <summary>
        /// <seealso cref="TraitsSupporter.Expand(string)"/>
        /// </summary>
        [Test, Order(ORDER_AfterBasic)]
        public void Expand_AlreadyBeExpanded_Passes()
        {
            var sourceCode =
@"using System.IO;
public class Apple
{
    ////@@ traits $Traits
	text text text
    apple apple apple
    ////-- Finish traits $Traits
}
";
            var traitsText = @"public int TraitsValue { get; set; }";

            var traitsSupporter = new TraitsSupporter()
            {
                TraitsTemplates = new Dictionary<string, string>() {
                    { "Traits", traitsText },
                }
            };
            var result = traitsSupporter.Expand(sourceCode);

            // Indent was included Test target!
            var correctText =
@"using System.IO;
public class Apple
{
    ////@@ traits $Traits
    public int TraitsValue { get; set; }
    ////-- Finish traits $Traits
}
".Replace("\r\n", "\n");
            Logger.Log(Logger.Priority.High, () => $"result -- {System.Environment.NewLine}{result}");

            Assert.AreEqual(correctText, result.Replace("\r\n", "\n"));
        }

        class TraitsFormFile
        {
            //// $TraitsFromFileTest
            #region $TraitsFromFileTest
              // this comment must keep but not keep prefix spaces.
            public int TraitsFromFileValue { get; set; }

            #endregion
            //// End $TraitsFromFileTest

            //this ignore 
            //// $FromTraitsTemplatesProp
            public int FromTraitsTemplatesProp { get; set; }
            //// End $FromTraitsTemplatesProp
        }

        /// <summary>
		/// <seealso cref="TraitsSupporter.Expand(string)"/>
        /// </summary>
        [Test, Order(ORDER_AfterBasic)]
        public void Expand_TemplateInFile_Passes()
        {
            var sourceCode =
@"using System.IO;
public class Apple
{
    ////@@ traits Packages/com.tositeru.hinode/Tests/Editor/Tools/TraitsSupporter/TestTraitsSupporter.cs $TraitsFromFileTest

	////@@ traits $FromTraitsTemplatesProp
}
";

            var traitsSupporter = new TraitsSupporter()
            {
                TraitsTemplates = new Dictionary<string, string>()
                {
                    { "FromTraitsTemplatesProp", "public int FromProps { get; set; }" },
                },
            };
            var result = traitsSupporter.Expand(sourceCode);

            // Indent was included Test target!
            var correctText =
@"using System.IO;
public class Apple
{
    ////@@ traits Packages/com.tositeru.hinode/Tests/Editor/Tools/TraitsSupporter/TestTraitsSupporter.cs $TraitsFromFileTest
    #region $TraitsFromFileTest
    // this comment must keep but not keep prefix spaces.
    public int TraitsFromFileValue { get; set; }

    #endregion
    ////-- Finish traits Packages/com.tositeru.hinode/Tests/Editor/Tools/TraitsSupporter/TestTraitsSupporter.cs $TraitsFromFileTest

	////@@ traits $FromTraitsTemplatesProp
	public int FromProps { get; set; }
	////-- Finish traits $FromTraitsTemplatesProp
}
".Replace("\r\n", "\n");
            Logger.Log(Logger.Priority.High, () => $"result -- {System.Environment.NewLine}{result}");
            Assert.AreEqual(correctText, result.Replace("\r\n", "\n"));

        }

//        /// <summary>
//		/// <seealso cref="TraitsSupporter.Expand(string)"/>
//        /// </summary>
//        [Test, Order(ORDER_AfterBasic)]
//        public void Expand_TemplateInFile_InComment_Passes()
//        {
//            var sourceCode =
//@"using System.IO;
//public class Apple
//{
//    ////@@ traits Packages/com.tositeru.hinode/Tests/Editor/Tools/TraitsSupporter/TestTraitsSupporter.cs $TraitsFromFileInCommentTest
//}
//";

//            // traits text
//            //// $TraitsFromFileInCommentTest
//            //#region $TraitsFromFileInCommentTest
//            //  // this comment must keep but not keep prefix spaces.

//            //public int TraitsFromFileInCommentValue { get; set; }
//            //#endregion
//            //// End $TraitsFromFileInCommentTest

//            var traitsSupporter = new TraitsSupporter();
//            var result = traitsSupporter.Expand(sourceCode);

//            // Indent was included Test target!
//            var correctText =
//@"using System.IO;
//public class Apple
//{
//    ////@@ traits Packages/com.tositeru.hinode/Tests/Editor/Tools/TraitsSupporter/TestTraitsSupporter.cs $TraitsFromFileInCommentTest
//    #region $TraitsFromFileInCommentTest
//    // this comment must keep but not keep prefix spaces.

//    public int TraitsFromFileInCommentValue { get; set; }
//    #endregion
//    ////-- Finish traits Packages/com.tositeru.hinode/Tests/Editor/Tools/TraitsSupporter/TestTraitsSupporter.cs $TraitsFromFileInCommentTest
//}
//".Replace("\r\n", "\n");
//            Assert.AreEqual(correctText, result.Replace("\r\n", "\n"));
//        }

        class ExpandInFile_PassesClass
        {
            // traits text
            //// $ExpandInFileTest
            public int ExpandInFileValue { get; set; }
            //// End $ExpandInFileTest
        }

        /// <summary>
		/// <seealso cref="TraitsSupporter.RootDir"/>
		/// <seealso cref="TraitsSupporter.ExpandInFile(string)"/>
        /// </summary>
        [Test, Order(ORDER_AfterBasic)]
        public void ExpandInFile_Passes()
        {
            var stackFrame = new StackFrame(true);
            UnityEngine.Debug.Log($"test -- {stackFrame.GetFileName()}");
            var traitsSupporter = new TraitsSupporter()
            {
                RootDir = Path.GetDirectoryName(stackFrame.GetFileName()),
            };
            var result = traitsSupporter.ExpandInFile("Traits_ExpandInFile.cs.txt");

            // Indent was included Test target!
            var correctText =
@"using System.IO;
public class Apple
{
    ////@@ traits ./TestTraitsSupporter.cs $ExpandInFileTest
    public int ExpandInFileValue { get; set; }
    ////-- Finish traits ./TestTraitsSupporter.cs $ExpandInFileTest
}
".Replace("\r\n", "\n");
            Assert.AreEqual(correctText, result.Replace("\r\n", "\n"));

        }

        /// <summary>
        /// <seealso cref="TraitsSupporter.RootDir"/>
        /// <seealso cref="TraitsSupporter.ExpandInFile(string)"/>
        /// </summary>
        [Test, Order(ORDER_AfterBasic)]
        public void ExpandInFile_FromSelfFile_Passes()
        {
            var stackFrame = new StackFrame(true);
            var traitsSupporter = new TraitsSupporter()
            {
                RootDir = Path.GetDirectoryName(stackFrame.GetFileName()),
            };
            var result = traitsSupporter.ExpandInFile("Traits_ExpandInFile_FromSelfFile.cs.txt");

            // Indent was included Test target!
            var correctText =
@"//// $SearchFromSelfFile
public int ExpandInFileValue { get; set; }
//// End $SearchFromSelfFile

using System.IO;
public class Apple
{
    ////@@ traits $SearchFromSelfFile
    public int ExpandInFileValue { get; set; }
    ////-- Finish traits $SearchFromSelfFile
}
".Replace("\r\n", "\n");
            Assert.AreEqual(correctText, result.Replace("\r\n", "\n"));

        }

    }
}
