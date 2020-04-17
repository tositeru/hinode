using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="IControllerSenderGroup"/>
    /// <seealso cref="IControllerSenderInstance"/>
    /// </summary>
    public class ControllerMap
    {
        HashSet<IControllerSenderGroup> _senderGroups = new HashSet<IControllerSenderGroup>();

        public IEnumerable<IControllerSenderGroup> SenderGroups { get => _senderGroups; }

        public ControllerMap(params IControllerSenderGroup[] senderGroups)
            : this(senderGroups.AsEnumerable())
        { }

        public ControllerMap(IEnumerable<IControllerSenderGroup> senderGroups)
        {
            foreach(var g in senderGroups)
            {
                _senderGroups.Add(g);
            }

            //Keywordの重複チェック
            var multipleKeywords = SenderGroups.SelectMany(_g => _g.SenderKeywords)
                .Where(_k => 1 < SenderGroups.Where(_og => _og.DoHasSenderKeyword(_k) ).Count());
            if(multipleKeywords.Any())
            {
                var keywords = multipleKeywords.Aggregate((_sum, _cur) => _sum + $",{ _cur}");
                Assert.IsTrue(false, $"SenderKeywordの中で重複したものが存在します。keywords=>{keywords}");
            }
        }

        public IControllerSenderInstance CreateController(string useEventKeyward, Model target, ModelViewBinderInstanceMap binderInstanceMap)
        {
            var group = SenderGroups.FirstOrDefault(_s => _s.DoHasSenderKeyword(useEventKeyward));
            Assert.IsNotNull(group);

            var inst = group.CreateInstance(target, binderInstanceMap);
            inst.EnableSender(group.GetSenderType(useEventKeyward));
            return inst;
        }
    }
}
