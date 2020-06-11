using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="changedType"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public delegate void ViewLayoutStateOnUpdatedCallback(ViewLayoutState.OnUpdatedCallbackData eventData);

    /// <summary>
    /// <seealso cref=""/>
    /// </summary>
    public class ViewLayoutState : IEnumerable<(string key, object value)>
    {
        public class OnUpdatedCallbackData
        {
            public ViewLayoutState Self { get; }
            public ViewLayoutState.OnChangedType ChangedType { get; }
            public string Key { get; }
            public object Value { get; }

            public OnUpdatedCallbackData(ViewLayoutState self, OnChangedType onChangedType, string key, object value)
            {
                Self = self;
                ChangedType = onChangedType;
                Key = key;
                Value = value;
            }
        }
        public enum OnChangedType
        {
            Set,
            Remove,
        }

        public enum ValueGroup
        {
            None,
            Raw,
            LayoutOverwriter,
            BindInfo,
        }

        HashSet<string> _keys = new HashSet<string>();
        Dictionary<string, object> _rawValues = new Dictionary<string, object>();

        SmartDelegate<ViewLayoutStateOnUpdatedCallback> _onChangedValueCallback = new SmartDelegate<ViewLayoutStateOnUpdatedCallback>();

        public IEnumerable<(string key, object value)> KeyAndValues
        {
            get => _keys.Select(_k => (key: _k, value: GetValue(_k)));
        }

        public int Count { get => _keys.Count; }

        public NotInvokableDelegate<ViewLayoutStateOnUpdatedCallback> OnChangedValue { get => _onChangedValueCallback; }

        public bool ContainsKey(System.Enum key)
            => ContainsKey(key.ToString());
        public bool ContainsKey(string key)
            => _keys.Contains(key);

        public ValueGroup GetValueGroup(string key)
        {
            if (_rawValues.ContainsKey(key))
                return ValueGroup.Raw;
            if (ContainsLayoutOverwriter)
            {
                var dict = _layoutOverwriterList.FirstOrDefault(_d => _d.ContainsKey(key));
                if (dict != null)
                {
                    return ValueGroup.LayoutOverwriter;
                }
            }
            if (ContainsBindInfo)
            {
                if (UseBindInfo.HasViewLayoutValue(key))
                    return ValueGroup.BindInfo;
            }
            return ValueGroup.None;
        }

        public object GetValue(string key)
        {
            if (_rawValues.ContainsKey(key))
                return _rawValues[key];
            if (ContainsLayoutOverwriter)
            {
                var dict = _layoutOverwriterList.FirstOrDefault(_d => _d.ContainsKey(key));
                if(dict != null)
                {
                    return dict.GetValue(key);
                }
            }
            if (ContainsBindInfo)
            {
                if (UseBindInfo.HasViewLayoutValue(key))
                    return UseBindInfo.GetViewLayoutValue(key);
            }
            Assert.IsTrue(false, $"Key({key}) is not contain...");
            return null;
        }
        public object GetValue(System.Enum key)
            => GetValue(key.ToString());

        public void Clear()
        {
            if(ContainsLayoutOverwriter)
            {
                UseModel.OnChangedModelIdentities.Remove(ModelOnChangedModelIdentities);
                UseModel.OnDestroyed.Remove(ModelOnDestroyed);
            }

            _rawValues.Clear();
            UseBindInfo = null;
            UseLayoutOverwriter = null;
            UseViewObject = null;
            _layoutOverwriterList = null;

            _keys.Clear();
        }

        void RemoveKey(string key)
        {
            if (_rawValues.ContainsKey(key))
                _rawValues.Remove(key);

            bool doRemove = true;
            if(ContainsLayoutOverwriter)
            {
                doRemove &= _layoutOverwriterList.Any(_l => _l.ContainsKey(key));
            }
            if(ContainsBindInfo)
            {
                doRemove &= UseBindInfo.HasViewLayoutValue(key);
            }

            if(doRemove)
            {
                _keys.Remove(key);
            }

            _onChangedValueCallback.Instance?.Invoke(new OnUpdatedCallbackData(this, OnChangedType.Remove, key, null));
        }

        #region Raw Value Operator
        public ViewLayoutState SetRaw(string key, object value)
        {
            if(!_rawValues.ContainsKey(key))
            {
                _rawValues.Add(key, value);
                _keys.Add(key);
            }
            else
            {
                _rawValues[key] = value;
            }
            _onChangedValueCallback.Instance?.Invoke(new OnUpdatedCallbackData(this, OnChangedType.Set, key, value));
            return this;
        }
        public ViewLayoutState SetRaw(System.Enum key, object value)
            => SetRaw(key.ToString(), value);

        public ViewLayoutState SetRaw(ViewLayoutValueDictionary dict)
        {
            foreach(var t in dict.Layouts)
            {
                SetRaw(t.Key, t.Value);
            }
            return this;
        }

        public ViewLayoutState RemoveRaw(string key)
        {
            RemoveKey(key);
            return this;
        }
        public ViewLayoutState RemoveRaw(System.Enum key)
            => RemoveRaw(key.ToString());
        public ViewLayoutState RemoveRaw(ViewLayoutValueDictionary dict)
        {
            foreach(var t in dict)
            {
                RemoveRaw(t.Key);
            }
            return this;
        }
        #endregion

        #region BindInfo operator
        public ModelViewBinder.BindInfo UseBindInfo { get; private set; }
        public bool ContainsBindInfo { get => UseBindInfo != null; }

        public ViewLayoutState SetBindInfo(ModelViewBinder.BindInfo bindInfo)
        {
            if (bindInfo == UseBindInfo) return this;

            RemoveBindInfo();

            UseBindInfo = bindInfo;
            foreach(var t in UseBindInfo.ViewLayoutValues)
            {
                if(!ContainsKey(t.Key))
                {
                    _keys.Add(t.Key);
                }

                if(ValueGroup.BindInfo == GetValueGroup(t.Key))
                {
                    _onChangedValueCallback.Instance?.Invoke(new OnUpdatedCallbackData(this, OnChangedType.Set, t.Key, t.Value));
                }
            }
            return this;
        }

        public ViewLayoutState RemoveBindInfo()
        {
            if (!ContainsBindInfo) return this;

            var bindInfo = UseBindInfo;
            UseBindInfo = null;
            foreach(var k in bindInfo.ViewLayoutValues.Select(_t => _t.Key))
            {
                RemoveKey(k);
            }
            return this;
        }
        #endregion

        #region ViewLayoutOverwriter operator
        List<IReadOnlyViewLayoutValueDictionary> _layoutOverwriterList;
        public ViewLayoutOverwriter UseLayoutOverwriter { get; private set; }
        public Model UseModel { get => UseViewObject?.UseModel ?? null; }
        public IViewObject UseViewObject { get; private set; }
        public bool ContainsLayoutOverwriter { get => UseLayoutOverwriter != null; }

        public ViewLayoutState SetLayoutOverwriter(IViewObject viewObject, ViewLayoutOverwriter viewLayoutOverwriter)
        {
            Assert.IsTrue(viewObject.DoBinding());
            Assert.IsNotNull(viewLayoutOverwriter);
            RemoveLayoutOverwriter();

            UseViewObject = viewObject;
            UseLayoutOverwriter = viewLayoutOverwriter;
            _layoutOverwriterList = viewLayoutOverwriter.MatchLayoutValueDicts(UseModel, UseViewObject).ToList();

            foreach (var t in _layoutOverwriterList
                .SelectMany(_l => _l)
                .Where(_t => !ContainsKey(_t.Key)))
            {
                _keys.Add(t.Key);
            }

            foreach(var key in _layoutOverwriterList
                .SelectMany(_l => _l.Layouts.Select(_t => _t.Key))
                .Distinct()
                .Where(_key => ValueGroup.LayoutOverwriter == GetValueGroup(_key)))
            {
                _onChangedValueCallback.Instance?.Invoke(new OnUpdatedCallbackData(this, OnChangedType.Set, key, GetValue(key)));
            }

            UseModel.OnChangedModelIdentities.Add(ModelOnChangedModelIdentities);
            UseModel.OnDestroyed.Add(ModelOnDestroyed);
            return this;
        }
        public ViewLayoutState RemoveLayoutOverwriter()
        {
            if (!ContainsLayoutOverwriter) return this;

            UseModel.OnChangedModelIdentities.Remove(ModelOnChangedModelIdentities);
            UseModel.OnDestroyed.Remove(ModelOnDestroyed);

            var overwriterList = _layoutOverwriterList;
            UseLayoutOverwriter = null;
            UseViewObject = null;
            _layoutOverwriterList = null;

            foreach (var k in overwriterList
                .SelectMany(_l => _l.Layouts.Keys)
                .Distinct())
            {
                RemoveKey(k);
            }

            return this;
        }

        void ModelOnChangedModelIdentities(Model model)
        {
            if (UseModel != model) return;

            SetLayoutOverwriter(UseViewObject, UseLayoutOverwriter);
        }

        void ModelOnDestroyed(Model model)
        {
            RemoveLayoutOverwriter();
        }
        #endregion

        #region IEnumerable<(string key, object value)> interface
        public IEnumerator<(string key, object value)> GetEnumerator()
            => KeyAndValues.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
        #endregion
    }
}
