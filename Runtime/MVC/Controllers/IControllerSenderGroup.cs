using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public interface IControllerSenderGroup
    {
        IEnumerable<string> SenderKeywords { get; }
        bool ContainsSenderKeyword(string keyword);
        System.Type GetSenderType(string keyword);
        bool ContainsSender(System.Type senderType);
        IControllerSenderInstance CreateInstance(IViewObject targetViewObj, Model target, ModelViewBinderInstanceMap instanceMap);
    }

    public static partial class IControllerSenderGroupExtensions
    {
        public static bool ContainsSender<TSender>(this IControllerSenderGroup target)
            where TSender : IControllerSender
            => target.ContainsSender(typeof(TSender));
    }

    /// <summary>
    /// IControllerSenderGroupのデフォルト実装
    /// </summary>
    /// <typeparam name="InstanceType"></typeparam>
    public class DefaultControllerSenderGroup<InstanceType>
        : IControllerSenderGroup
        where InstanceType : IControllerSenderInstance, new()
    {
        static readonly System.Type[] _emptryArgs = new System.Type[] { };

        Dictionary<string, System.Type> _enabledSenders = new Dictionary<string, System.Type>();
        public DefaultControllerSenderGroup(IReadOnlyDictionary<string, System.Type> enabledSenders)
        {
            Assert.IsTrue(enabledSenders.All(_e => _e.Value.HasInterface<IControllerSender>()));
            _enabledSenders.Merge(true, enabledSenders);
        }

        #region IControllerSenderGroup
        public IEnumerable<string> SenderKeywords { get => _enabledSenders.Keys; }

        public bool ContainsSenderKeyword(string keyword)
            => _enabledSenders.ContainsKey(keyword);

        public System.Type GetSenderType(string keyword)
            => _enabledSenders[keyword];
        public bool ContainsSender(System.Type senderType)
        {
            return _enabledSenders.Any(_t => _t.Value == senderType);
        }
        public IControllerSenderInstance CreateInstance(IViewObject targetViewObj, Model target, ModelViewBinderInstanceMap instanceMap)
        {
            var inst = new InstanceType
            {
                Target = target,
                TargetViewObj = targetViewObj,
                UseBinderInstanceMap = instanceMap,
                UseSenderGroup = this
            };
            return inst;
        }
        #endregion
    }
}
