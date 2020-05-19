using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// グループ化できるIControllerSenderをまとめたもの
    /// </summary>
    public abstract class IEventDispatcher
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract bool DoEnabled { get; set; }

        /// <summary>
        /// 指定したmodel,viewObjectがIControllerObjectを作成できるか判定します。
        /// </summary>
        /// <param name="model"></param>
        /// <param name="viewObject"></param>
        /// <returns></returns>
        public abstract bool IsCreatableControllerObject(Model model, IViewObject viewObject);

        /// <summary>
        /// このIEventDispatcherと対応しているIControllerObjectを作成します。
        ///
        /// 作成できない場合は例外を投げます。
        /// そのため引数に渡すmodelとviewObjectがIControllerObjectを作成可能かどうかはIEventDispatcher#IsCreatableControllerObjectsで判定してください。
        /// </summary>
        /// <param name="model"></param>
        /// <param name="viewObject"></param>
        /// <returns></returns>
        public abstract IControllerObject CreateControllerObject(Model model, IViewObject viewObject);

        /// <summary>
        /// 派生クラスがサポートするEventの情報をまとめたEventInfoManagerを作成します。
        ///
        /// この関数は自動的にISenderGroup内で呼び出されます。
        /// </summary>
        /// <returns></returns>
        protected abstract EventInfoManager CreateEventInfoManager();

        /// <summary>
        /// イベント対象のControllerInfoを持つModel,IViewObjectを取得する
        /// 
        /// </summary>
        /// <param name="binderInstanceMap"></param>
        /// <returns></returns>
        protected virtual IEnumerable<(Model model, IViewObject viewObj, ControllerInfo controllerInfo)> GetSupportedControllerInfos(ModelViewBinderInstanceMap binderInstanceMap)
        {
            var dispatchStateMap = binderInstanceMap.UseEventDispatchStateMap;
            return binderInstanceMap.BindInstances.Values
                .SelectMany(_b => _b.GetControllerInfos())
                .Where(_c =>
                    EventInfos.ContainKeyword(_c.controllerInfo.Keyword)
                    && EventInfos.DoEnabledEvent(_c.controllerInfo.Keyword)
                    && (!dispatchStateMap?.DoMatch(
                            DispatchStateName.disable,
                            _c.model,
                            _c.viewObj,
                            EventInfos.GetRecieverType(_c.controllerInfo.Keyword))
                        ?? true)
                );
        }

        /// <summary>
        /// EventDataの状態を更新
        /// </summary>
        /// <param name="binderInstanceMap"></param>
        protected abstract void UpdateImpl(ModelViewBinderInstanceMap binderInstanceMap);

        /// <summary>
        /// 指定されたkeyword,model,viewObjectに対応したEventDataを返します。
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="model"></param>
        /// <param name="viewObject"></param>
        /// <param name="controllerInfo"></param>
        /// <returns></returns>
        protected abstract object GetEventData(Model model, IViewObject viewObject, ControllerInfo controllerInfo);

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

        public void Update(ModelViewBinderInstanceMap binderInstanceMap)
        {
            if (!DoEnabled) return;
            UpdateImpl(binderInstanceMap);
        }

        /// <summary>
        /// binderInstanceMapが保持しているModel,IViewObjectの内、このISenderGroupが対応しているIControllerSenderのControllerInfoを持つものを検索し、現在のEventDataを送信する関数
        /// </summary>
        /// <param name="binderInstanceMap"></param>
        public void SendTo(ModelViewBinderInstanceMap binderInstanceMap)
        {
            if (!DoEnabled) return;
            var supportedControllerInfos = GetSupportedControllerInfos(binderInstanceMap);
            foreach (var (model, viewObj, controllerInfo) in supportedControllerInfos)
            {
                var recieverType = EventInfos.GetRecieverType(controllerInfo.Keyword);
                var eventData = GetEventData(model, viewObj, controllerInfo);
                if (recieverType == null || eventData == null) continue;

                foreach (var selector in controllerInfo.RecieverSelectors)
                {
                    selector.Send(recieverType, model, eventData, binderInstanceMap);
                }
            }
        }

        /// <summary>
        /// 指定したeventが有効かどうか？
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public bool DoEnabledEvent(System.Enum keyword)
            => EventInfos[keyword].DoEnabled;
        /// <summary>
        /// 指定したeventが有効かどうか？
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public bool DoEnabledEvent(string keyword)
            => EventInfos[keyword].DoEnabled;

        /// <summary>
        /// 指定したeventの有効・無効を設定します
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="enabled"></param>
        public void SetEnabledEvent(System.Enum keyword, bool enabled)
            => EventInfos[keyword].DoEnabled = enabled;
        /// <summary>
        /// 指定したeventの有効・無効を設定します
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="enabled"></param>
        public void SetEnabledEvent(string keyword, bool enabled)
            => EventInfos[keyword].DoEnabled = enabled;

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

            public bool DoEnabled { get; set; } = true;

            public Info(System.Enum keyword, System.Type senderType, System.Type recieverType)
                : this(keyword.ToString(), senderType, recieverType)
            { }

            public Info(string keyword, System.Type senderType, System.Type recieverType)
            {
                Keyword = keyword;
                SenderType = senderType;
                RecieverType = recieverType;

                //TODO Validate Reciever, Sender and Keyword with Attribute
            }
        }

        public static Info CreateInfo<TSender, TReciever>(string keyword)
            where TSender : IControllerSender
            where TReciever : IControllerReciever
            => new Info(keyword, typeof(TSender), typeof(TReciever));
        public static Info CreateInfo<TSender, TReciever>(System.Enum keyword)
            where TSender : IControllerSender
            where TReciever : IControllerReciever
            => new Info(keyword.ToString(), typeof(TSender), typeof(TReciever));

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
}
