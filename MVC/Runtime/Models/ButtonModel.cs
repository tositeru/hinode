using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    /// <summary>
    /// Buttonを表すModel
    ///
    /// ViewとしてButtonViewObjectを用意しています。
    /// <seealso cref="ButtonViewObject"/>
    /// </summary>
    public class ButtonModel : Model
        , ISiblingOrder
        , IHavingDialogData
    {
        public string Text { get; set; }
        public object Value { get; set; }

        #region ISiblingOrder interface
        uint _siblingOrder = ISiblingOrderConst.INVALID_ORDER;
        public uint SiblingOrder { get => _siblingOrder; set { _siblingOrder = value; DoneUpdate(); } }
        #endregion

        #region IHavingDialogData interface
        public HavingTextResourceData DialogTitleTextResource { get; set; }
        public HavingTextResourceData DialogTextTextResource { get; set; }
        #endregion
    }
}
