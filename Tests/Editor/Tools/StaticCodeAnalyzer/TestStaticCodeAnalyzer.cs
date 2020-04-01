using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Editors.Tool.StaticCodeAnalyzer
{
    public class TestStaticCodeAnalyzer
    {
        [Ignore(""), Test, Description("ソースコードをトークンに分割するテスト")]
        public void SplitSourceToTokenPasses()
        {
            string source = @"
#define MACRO
using System;
using System.Collections;

namespace Fruits {
  public class Main {
    static public int _staticField;
    int _field;
  }
  public class Apple : {
  }
}
";
            //IEnumerable<Token> tokenSequence = TokenParser.Parse(source);

            //var validTokenSequence = new List<Token>
            //{
                
            //};
        }
    }
}
