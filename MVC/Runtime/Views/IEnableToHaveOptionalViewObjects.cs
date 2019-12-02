using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    /// <summary>
    /// オプション的なIViewObjectを持つことが可能なIViewObjectを表します。
    ///
    /// UnityのGameObject#AddComponent的な意味合いを持ちます。
    /// <seealso cref="IAppendableViewObjectParamBinder"/>
    /// </summary>
    /// <typeparam name="TViewObject"></typeparam>
    /// <typeparam name="TParamBinder"></typeparam>
    public interface IEnableToHaveOptionalViewObjects<TOptionViewObject, TOptionParamBinder>
        where TOptionViewObject : class, IViewObject
        where TOptionParamBinder : class, IModelViewParamBinder
    {
        IReadOnlyDictionary<TOptionParamBinder, TOptionViewObject> OptionalViewObjectDict { get; }

        IEnableToHaveOptionalViewObjects<TOptionViewObject, TOptionParamBinder> AddOptionalViewObject(TOptionParamBinder paramBinder, TOptionViewObject applendableViewObject);
    }

    /// <summary>
    /// IEnableToHaveOptionViewObjects用のIModelViewParamBinder
    /// <seealso cref="IEnableToHaveOptionalViewObjects{TViewObject, TParamBinder}"/>
    /// </summary>
    public interface IEnableToHaveOptionalViewObjectParamBinder<TOptionalParamBinder> : IModelViewParamBinder
        where TOptionalParamBinder : IModelViewParamBinder
    {
        IReadOnlyList<TOptionalParamBinder> OptionalViewObjectParamBinders { get; }

        IEnableToHaveOptionalViewObjectParamBinder<TOptionalParamBinder> AddOptionalViewObjectParamBinder(TOptionalParamBinder optionalParamBinder);
    }
}
