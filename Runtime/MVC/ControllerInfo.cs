using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hinode
{
    /// <summary>
    /// ModelViewBinderで使用されるControllerの情報を表すクラス
    /// <seealso cref="ModelViewBinder"/>
    /// </summary>
    public class ControllerInfo
    {
        List<RecieverSelector> _recieverInfos = new List<RecieverSelector>();
        public string Keyword { get; set; }
        public IEnumerable<RecieverSelector> RecieverSelectors { get => _recieverInfos; }

        public ControllerInfo(string keyword, params RecieverSelector[] recieverInfos)
            : this(keyword, recieverInfos.AsEnumerable())
        { }

        public ControllerInfo(string keyword, IEnumerable<RecieverSelector> recieverInfos)
        {
            Keyword = keyword;
            _recieverInfos = recieverInfos.ToList();
        }

        public ControllerInfo(System.Enum keyword, params RecieverSelector[] recieverInfos)
            : this(keyword.ToString(), recieverInfos.AsEnumerable())
        { }
        public ControllerInfo(System.Enum keyword, IEnumerable<RecieverSelector> recieverInfos)
            : this(keyword.ToString(), recieverInfos.AsEnumerable())
        { }

        public void AddRecieverInfo(RecieverSelector selector)
        {
            _recieverInfos.Add(selector);
        }
    }

}
