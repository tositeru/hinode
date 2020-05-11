﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// グループ化できるIControllerSenderをまとめたもの
    /// </summary>
    public abstract class ISenderGroup
    {
        /// <summary>
        /// 派生クラスがサポートするEventの情報をまとめたEventInfoManagerを作成します。
        ///
        /// この関数は自動的にISenderGroup内で呼び出されます。
        /// </summary>
        /// <returns></returns>
        protected abstract EventInfoManager CreateEventInfoManager();

        /// <summary>
        /// EventDataの状態を更新
        /// </summary>
        /// <param name="binderInstanceMap"></param>
        public abstract void Update(ModelViewBinderInstanceMap binderInstanceMap);

        /// <summary>
        /// 指定されたkeyword,model,viewObjectに対応したEventDataを返します。
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="model"></param>
        /// <param name="viewObject"></param>
        /// <returns></returns>
        protected abstract object GetEventData(string keyword, Model model, IViewObject viewObject);

        EventInfoManager _eventInfoManager;
        /// <summary>
        /// ISenderGroupが対応しているイベントの情報をまとめたもの
        /// </summary>
        public EventInfoManager EventInfos
        {
            get => _eventInfoManager != null
                ? _eventInfoManager
                : _eventInfoManager = CreateEventInfoManager();
        }

        /// <summary>
        /// binderInstanceMapが保持しているModel,IViewObjectの内、このISenderGroupが対応しているIControllerSenderのControllerInfoを持つものを検索し、現在のEventDataを送信する関数
        /// </summary>
        /// <param name="binderInstanceMap"></param>
        public void SendTo(ModelViewBinderInstanceMap binderInstanceMap)
        {
            var supportedControllerInfos = binderInstanceMap.BindInstances.Values
                .SelectMany(_b => _b.GetControllerInfos())
                .Where(_c =>
                    EventInfos.ContainKeyword(_c.controllerInfo.Keyword)
                    && EventInfos.DoEnabledEvent(_c.controllerInfo.Keyword)
                );
            foreach (var (model, viewObj, controllerInfo) in supportedControllerInfos)
            {
                foreach (var selector in controllerInfo.RecieverSelectors)
                {
                    var recieverType = EventInfos.GetRecieverType(controllerInfo.Keyword);
                    var eventData = GetEventData(controllerInfo.Keyword, model, viewObj);

                    if (recieverType == null || eventData == null) continue;

                    selector.Send(recieverType, model, eventData, binderInstanceMap);
                }
            }
        }
    }

    /// <summary>
    /// ISenderGroupと一緒に使用されるクラス
    ///
    /// ISenderGroupが持つEventのデータを管理する。=> class EventInfoManager#Info
    /// </summary>
    public class EventInfoManager
    {
        public class Info
        {
            public static Info Create<TSender, TReciever>(System.Enum keyword)
                where TSender : IControllerSender
                where TReciever : IControllerReciever
                => Create<TSender, TReciever>(keyword.ToString());
            public static Info Create<TSender, TReciever>(string keyword)
                where TSender : IControllerSender
                where TReciever : IControllerReciever
            {
                return new Info(keyword, typeof(TSender), typeof(TReciever));
            }

            public string Keyword { get; }
            public System.Type SenderType { get; }
            public System.Type RecieverType { get; }

            public bool DoEnabled { get; set; }

            public Info(System.Enum keyword, System.Type senderType, System.Type recieverType)
                : this(keyword.ToString(), senderType, recieverType)
            { }

            public Info(string keyword, System.Type senderType, System.Type recieverType)
            {
                Keyword = keyword;
                SenderType = senderType;
                RecieverType = recieverType;
            }
        }

        Dictionary<string, Info> _infos = new Dictionary<string, Info>();
        public EventInfoManager(params Info[] infos)
            : this(infos.AsEnumerable())
        { }

        public EventInfoManager(IEnumerable<Info> infos)
        {
            foreach (var info in infos)
            {
                Assert.IsFalse(_infos.ContainsKey(info.Keyword));
                _infos.Add(info.Keyword, info);
            }
        }

        public Info this[string keyword]
        {
            get
            {
                Assert.IsTrue(_infos.ContainsKey(keyword));
                return _infos[keyword];
            }
        }

        public Info this[System.Enum keyword]
        {
            get
            {
                Assert.IsTrue(_infos.ContainsKey(keyword.ToString()));
                return _infos[keyword.ToString()];
            }
        }

        public bool ContainKeyword(System.Enum keyword)
            => _infos.ContainsKey(keyword.ToString());
        public bool ContainKeyword(string keyword)
            => _infos.ContainsKey(keyword);

        public bool DoEnabledEvent(System.Enum keyword)
            => this[keyword].DoEnabled;
        public bool DoEnabledEvent(string keyword)
            => this[keyword].DoEnabled;

        public void SetEnabledEvent(System.Enum keyword, bool enabled)
            => this[keyword].DoEnabled = enabled;
        public void SetEnabledEvent(string keyword, bool enabled)
            => this[keyword].DoEnabled = enabled;

        public System.Type GetSenderType(System.Enum keyword)
            => this[keyword].SenderType;
        public System.Type GetSenderType(string keyword)
            => this[keyword].SenderType;

        public System.Type GetRecieverType(System.Enum keyword)
            => this[keyword].RecieverType;
        public System.Type GetRecieverType(string keyword)
            => this[keyword].RecieverType;
    }

    /// <summary>
    /// 廃止予定
    /// </summary>
    public interface IControllerSenderInstance : IControllerSender
    {
        IViewObject TargetViewObj { get; set; }
        IControllerSenderGroup UseSenderGroup { get; set; }
        EnableSenderCollection EnabledSenders { get; }
        SelectorListDictionary SelectorListDict { get; }

        void Destroy();
    }

    public class EnableSenderCollection : IEnumerable<System.Type>, IEnumerable
    {
        HashSet<System.Type> _enabledSenders = new HashSet<System.Type>();

        public bool DoEnable(System.Type senderType, IControllerSenderGroup senderGroup)
        {
            Assert.IsNotNull(senderGroup);
            Assert.IsTrue(senderGroup.ContainsSender(senderType), $"Don't contains sender... senderType={senderType}");
            return _enabledSenders.Contains(senderType);
        }
        public void Enable(System.Type senderType, IControllerSenderGroup senderGroup)
        {
            Assert.IsNotNull(senderGroup);
            Assert.IsTrue(senderGroup.ContainsSender(senderType), $"Don't contains sender... senderType={senderType}");
            if (DoEnable(senderType, senderGroup)) return;
            _enabledSenders.Add(senderType);
        }
        public void Disable(System.Type senderType, IControllerSenderGroup senderGroup)
        {
            Assert.IsNotNull(senderGroup);
            Assert.IsTrue(senderGroup.ContainsSender(senderType), $"Don't contains sender... senderType={senderType}");
            if (!DoEnable(senderType, senderGroup)) return;
            _enabledSenders.Remove(senderType);
        }

        public IEnumerator<System.Type> GetEnumerator()
            => _enabledSenders.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _enabledSenders.GetEnumerator();
    }

    public class SelectorListDictionary : IEnumerable<KeyValuePair<System.Type, HashSet<RecieverSelector>>>, IEnumerable 
    {
        protected Dictionary<System.Type, HashSet<RecieverSelector>> _dict = new Dictionary<System.Type, HashSet<RecieverSelector>>();

        public bool ContainsKey(System.Type senderType, IControllerSenderGroup senderGroup)
        {
            return _dict.ContainsKey(senderType);
        }

        public IReadOnlyCollection<RecieverSelector> GetSelector(System.Type senderType, IControllerSenderGroup senderGroup)
        {
            Assert.IsTrue(senderGroup.ContainsSender(senderType), $"Don't contains sender... senderType={senderType}");
            return _dict[senderType];
        }

        public void AddSelector(System.Type senderType, RecieverSelector selector, IControllerSenderGroup senderGroup)
        {
            Assert.IsTrue(senderGroup.ContainsSender(senderType), $"Don't contains sender... senderType={senderType}");

            if (!_dict.ContainsKey(senderType))
            {
                _dict.Add(senderType, new HashSet<RecieverSelector>() { selector });
            }
            else
            {
                _dict[senderType].Add(selector);
            }
        }
        public void RemoveSelector(System.Type senderType, RecieverSelector selector, IControllerSenderGroup senderGroup)
        {
            Assert.IsTrue(senderGroup.ContainsSender(senderType), $"Don't contains sender... senderType={senderType}");

            if (_dict.ContainsKey(senderType))
            {
                _dict[senderType].Remove(selector);
            }
        }
        public void ClearSelector(System.Type senderType, IControllerSenderGroup senderGroup)
        {
            Assert.IsTrue(senderGroup.ContainsSender(senderType), $"Don't contains sender... senderType={senderType}");

            if (_dict.ContainsKey(senderType))
            {
                _dict.Remove(senderType);
            }
        }
        public void ClearAll()
        {
            _dict.Clear();
        }

        public IEnumerator<KeyValuePair<System.Type, HashSet<RecieverSelector>>> GetEnumerator()
            => _dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _dict.GetEnumerator();
    }

    public static partial class IControllerSenderInstanceExtensions
    {
        public static IReadOnlyCollection<RecieverSelector> GetSelector(this IControllerSenderInstance target, System.Type senderType)
            => target.SelectorListDict.GetSelector(senderType, target.UseSenderGroup);
        public static IReadOnlyCollection<RecieverSelector> GetSelector<TSender>(this IControllerSenderInstance target)
            where TSender : IControllerSender
            => target.SelectorListDict.GetSelector(typeof(TSender), target.UseSenderGroup);

        public static void AddSelector(this IControllerSenderInstance target, System.Type senderType, RecieverSelector selector)
            => target.SelectorListDict.AddSelector(senderType, selector, target.UseSenderGroup);
        public static void AddSelector<TSender>(this IControllerSenderInstance target, RecieverSelector selector)
            where TSender : IControllerSender
            => target.AddSelector(typeof(TSender), selector);

        public static void AddSelectors(this IControllerSenderInstance target, System.Type senderType, IEnumerable<RecieverSelector> selectors)
        {
            foreach(var s in selectors)
            {
                target.AddSelector(senderType, s);
            }
        }
        public static void AddSelectors(this IControllerSenderInstance target, System.Type senderType, params RecieverSelector[] selectors)
            => target.AddSelectors(senderType, selectors.AsEnumerable());
        public static void AddSelectors<TSender>(this IControllerSenderInstance target, IEnumerable<RecieverSelector> selectors)
            where TSender : IControllerSender
            => target.AddSelectors(typeof(TSender), selectors);
        public static void AddSelectors<TSender>(this IControllerSenderInstance target, params RecieverSelector[] selectors)
            where TSender : IControllerSender
            => target.AddSelectors(typeof(TSender), selectors.AsEnumerable());

        public static void RemoveSelector(this IControllerSenderInstance target, System.Type senderType, RecieverSelector selector)
            => target.SelectorListDict.RemoveSelector(senderType, selector, target.UseSenderGroup);
        public static void RemoveSelector<TSender>(this IControllerSenderInstance target, RecieverSelector selector)
            where TSender : IControllerSender
            => target.RemoveSelector(typeof(TSender), selector);

        public static void RemoveSelectors(this IControllerSenderInstance target, System.Type senderType, IEnumerable<RecieverSelector> selectors)
        {
            foreach (var s in selectors)
            {
                target.RemoveSelector(senderType, s);
            }
        }
        public static void RemoveSelectors(this IControllerSenderInstance target, System.Type senderType, params RecieverSelector[] selectors)
            => target.RemoveSelectors(senderType, selectors.AsEnumerable());
        public static void RemoveSelectors<TSender>(this IControllerSenderInstance target, IEnumerable<RecieverSelector> selectors)
            where TSender : IControllerSender
            => target.RemoveSelectors(typeof(TSender), selectors);
        public static void RemoveSelectors<TSender>(this IControllerSenderInstance target, params RecieverSelector[] selectors)
            where TSender : IControllerSender
            => target.RemoveSelectors(typeof(TSender), selectors.AsEnumerable());


        public static void ClearSelector(this IControllerSenderInstance target, System.Type senderType)
            => target.SelectorListDict.ClearSelector(senderType, target.UseSenderGroup);
        public static void ClearSelectorList<TSender>(this IControllerSenderInstance target)
            where TSender : IControllerSender
            => target.ClearSelector(typeof(TSender));

        public static void ClearSelectorDict(this IControllerSenderInstance target)
            => target.SelectorListDict.ClearAll();

        public static bool ContainsSelector(this IControllerSenderInstance target, System.Type senderType)
            => target.SelectorListDict.ContainsKey(senderType, target.UseSenderGroup);
        public static bool ContainsSelector<TSender>(this IControllerSenderInstance target)
            where TSender : IControllerSender
            => target.ContainsSelector(typeof(TSender));

        public static int SelectorCount(this IControllerSenderInstance target, System.Type senderType)
            => target.ContainsSelector(senderType)
                ? target.SelectorListDict.GetSelector(senderType, target.UseSenderGroup).Count
                : 0;
        public static int SelectorCount<TSender>(this IControllerSenderInstance target)
            where TSender : IControllerSender
            => target.SelectorCount(typeof(TSender));


        public static void EnableSender(this IControllerSenderInstance target, System.Type senderType)
            => target.EnabledSenders.Enable(senderType, target.UseSenderGroup);
        public static void EnableSender<TSender>(this IControllerSenderInstance target)
            where TSender : IControllerSender
            => target.EnableSender(typeof(TSender));

        public static void DisableSender(this IControllerSenderInstance target, System.Type senderType)
            => target.EnabledSenders.Disable(senderType, target.UseSenderGroup);
        public static void DisableSender<TSender>(this IControllerSenderInstance target)
            where TSender : IControllerSender
            => target.DisableSender(typeof(TSender));

        /// <summary>
        /// 指定したキーワードのイベントが有効かどうか?
        /// </summary>
        /// <param name="target"></param>
        /// <param name="keyword"></param>
        /// <param name="sendertype"></param>
        /// <returns></returns>
        public static bool DoEnableSender(this IControllerSenderInstance target, System.Type senderType)
            => target.EnabledSenders.DoEnable(senderType, target.UseSenderGroup);

        public static bool DoEnableSender<TSender>(this IControllerSenderInstance target)
            where TSender : IControllerSender
            => target.DoEnableSender(typeof(TSender));

        /// <summary>
        /// 指定したSenderに対応するRecieverにイベントを送る
        /// </summary>
        /// <typeparam name="TSenderType"></typeparam>
        /// <param name="target"></param>
        /// <param name="senderKeyword"></param>
        /// <param name="sender"></param>
        /// <param name="binderInstanceMap"></param>
        /// <param name="eventData"></param>
        public static void Send(this IControllerSenderInstance target, System.Type senderType, Model sender, ModelViewBinderInstanceMap binderInstanceMap, object eventData)
        {
            Assert.IsTrue(senderType.DoHasInterface<IControllerSender>());

            if (!target.UseSenderGroup.ContainsSender(senderType)) return;
            if (!target.DoEnableSender(senderType)) return;
            if (!target.ContainsSelector(senderType)) return;

            var pairRecieverType = ControllerTypeManager.GetRecieverType(senderType);
            foreach (var (recieverType, reciever, useEventData) in target.GetSelector(senderType)
                .SelectMany(_s => _s.Query(pairRecieverType, sender, binderInstanceMap, eventData)))
            {
                ControllerTypeManager.DoneRecieverExecuter(recieverType, reciever, sender, useEventData);
            }
        }

        /// <summary>
        /// 指定したSenderに対応するRecieverにイベントを送る
        /// </summary>
        /// <typeparam name="TSenderType"></typeparam>
        /// <param name="target"></param>
        /// <param name="senderKeyword"></param>
        /// <param name="sender"></param>
        /// <param name="binderInstanceMap"></param>
        /// <param name="eventData"></param>
        public static void Send<TSenderType>(this IControllerSenderInstance target, Model sender, ModelViewBinderInstanceMap binderInstanceMap, object eventData)
            where TSenderType : IControllerSender
            => target.Send(typeof(TSenderType), sender, binderInstanceMap, eventData);
    }
}
