using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// 汎用のModelViewBinder.IViewInstanceCreator
    ///
    /// 引数無しのコンストラクを持つIViewBinderObject型に対応しています
    /// <seealso cref="ModelViewBinder"/>
    /// <seealso cref="ModelViewBinder.IBindInfo"/>
    /// </summary>
    public class DefaultViewInstanceCreator : IViewInstanceCreator
    {
        Dictionary<string, (System.Type viewObjType, IModelViewParamBinder paramBinder)> _dict = new Dictionary<string, (System.Type viewObjType, IModelViewParamBinder paramBinder)>();

        System.Type[] _emptryArgs = new System.Type[] { };

        public DefaultViewInstanceCreator(params (System.Type viewType, IModelViewParamBinder paramBinder)[] data)
            : this(data.AsEnumerable())
        { }

        public DefaultViewInstanceCreator(IEnumerable<(System.Type viewType, IModelViewParamBinder paramBinder)> data)
        {
            foreach (var d in data)
            {
                var cstor = d.viewType.GetConstructor(_emptryArgs);
                Assert.IsNotNull(cstor, $"空引数のコンストラクターがあるクラスだけに対応しています... viewType={d.viewType.FullName}");
                Assert.IsTrue(d.viewType.DoHasInterface<IViewObject>(), $"IViewObject型を継承した型だけ対応しています... viewType={d.viewType.FullName}");
                Assert.IsNotNull(d.paramBinder, $"paramBinderは必ず設定してください...");
                Assert.IsFalse(_dict.ContainsKey(d.viewType.FullName), $"Already exist key({d.viewType.FullName})... paramBinder={d.paramBinder}");
                _dict.Add(d.viewType.FullName, (d.viewType, d.paramBinder));
            }
        }

        protected override System.Type GetViewObjTypeImpl(string instanceKey)
        {
            if (!_dict.ContainsKey(instanceKey)) return null;
            return _dict[instanceKey].viewObjType;
        }

        protected override IViewObject CreateViewObjImpl(string instanceKey)
        {
            if (!_dict.ContainsKey(instanceKey)) return null;
            var type = _dict[instanceKey].viewObjType;
            var cstor = type.GetConstructor(_emptryArgs);
            return cstor.Invoke(null) as IViewObject;
        }

        protected override IModelViewParamBinder GetParamBinderImpl(string binderKey)
        {
            if (!_dict.ContainsKey(binderKey)) return null;
            return _dict[binderKey].paramBinder;
        }
    }
}
