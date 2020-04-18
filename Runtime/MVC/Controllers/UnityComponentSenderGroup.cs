using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// IControllerSenderGroupのデフォルト実装
    /// </summary>
    /// <typeparam name="InstanceType"></typeparam>
    public class UnityComponentSenderGroup<InstanceType>
        : IControllerSenderGroup
        where InstanceType : Component, IControllerSenderInstance
    {
        Dictionary<string, System.Type> _enabledSenders = new Dictionary<string, System.Type>();
        public UnityComponentSenderGroup(IReadOnlyDictionary<string, System.Type> enabledSenders)
        {
            Assert.IsTrue(enabledSenders.All(_e => _e.Value.DoHasInterface<IControllerSender>()));
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
            Assert.IsTrue(targetViewObj is Component);
            var com = targetViewObj as Component;
            var inst = com.gameObject.AddComponent<InstanceType>();
            inst.Target = target;
            inst.TargetViewObj = targetViewObj;
            inst.UseBinderInstanceMap = instanceMap;
            inst.UseSenderGroup = this;
            return inst;
        }
        #endregion
    }

}
