using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// Buttonを表すModel
    ///
    /// ViewとしてButtonViewObjectを用意しています。
    /// <seealso cref="ButtonViewObject"/>
    /// </summary>
    public class ButtonModel : Model
    {
        public string Text { get; set; }
        public object Value { get; set; }
    }
}
