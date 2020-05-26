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
        List<EventHandlerSelector> _recieverInfos = new List<EventHandlerSelector>();
        public bool IsInterruptMode { get; private set; } = false;
        public string Keyword { get; set; }
        public IEnumerable<EventHandlerSelector> RecieverSelectors { get => _recieverInfos; }

        public ControllerInfo(string keyword, params EventHandlerSelector[] recieverInfos)
            : this(keyword, recieverInfos.AsEnumerable())
        { }

        public ControllerInfo(string keyword, IEnumerable<EventHandlerSelector> recieverInfos)
        {
            Keyword = keyword;
            _recieverInfos = recieverInfos.ToList();
        }

        public ControllerInfo(System.Enum keyword, params EventHandlerSelector[] recieverInfos)
            : this(keyword.ToString(), recieverInfos.AsEnumerable())
        { }
        public ControllerInfo(System.Enum keyword, IEnumerable<EventHandlerSelector> recieverInfos)
            : this(keyword.ToString(), recieverInfos.AsEnumerable())
        { }

        public ControllerInfo AddRecieverInfo(EventHandlerSelector selector)
        {
            _recieverInfos.Add(selector);
            return this;
        }

        public ControllerInfo SetInterrupt(bool enable)
        {
            IsInterruptMode = enable;
            return this;
        }
    }

}
