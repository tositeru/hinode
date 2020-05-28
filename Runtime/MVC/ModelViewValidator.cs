using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public static class ModelViewValidator
    {
        public static bool DoEnabled { get; set; } = false;

        const Logger.Priority LogPriority = Logger.Priority.Middle;

        /// <summary>
        /// ModelViewBinder.BindInfoの検証を行う
        ///
        /// 指定したModelとbindInfoが持つIViewObjectとIModelViewParamBinderが使用可能かどうかを検証します。
        /// </summary>
        /// <param name="model"></param>
        /// <param name="bindInfo"></param>
        /// <param name="viewInstanceCreator"></param>
        /// <seealso cref="AvailableModelAttribute"/>
        /// <seealso cref="AvailableModelViewParamBinderAttribute"/>
        public static bool ValidateBindInfo(Model model, ModelViewBinder.BindInfo bindInfo, IViewInstanceCreator viewInstanceCreator)
        {
            if (!DoEnabled) return true;
            bool isValid = true;

            System.Type viewType = null;
            try
            {
                viewType = viewInstanceCreator.GetViewObjType(bindInfo);
                if(viewType == null)
                {
                    Logger.LogWarning(LogPriority, () =>
                        $"!!Validate!! InstanceKey('{bindInfo.InstanceKey}') in BindInfo don't get IViewObject..."
                    );
                    isValid = false;
                }
                else
                {
                    var availableModelTypes = viewType.GetCustomAttributes(false)
                        .OfType<AvailableModelAttribute>()
                        .SelectMany(_a => _a.AvailableModels)
                        .Distinct();
                    if(availableModelTypes.Any())
                    {
                        if (!availableModelTypes.Any(_t => _t.Equals(model.GetType())))
                        {
                            Logger.LogWarning(LogPriority, () =>
                                $"!!Validate!! '{viewType}' is not available Model('{model}')..."
                            );
                            isValid = false;
                        }
                    }
                }
            }
            catch(System.Exception e)
            {
                Logger.LogWarning(LogPriority, () =>
                    $"!!Validate!! InstanceKey('{bindInfo.InstanceKey}') in BindInfo don't get IViewObject..." +
                    $"{System.Environment.NewLine }---{System.Environment.NewLine}{e}{System.Environment.NewLine}---"
                );
                return false;
            }


            try
            {
                var paramBinder = viewInstanceCreator.GetParamBinderType(bindInfo);
                if (paramBinder == null)
                {
                    Logger.LogWarning(LogPriority, () =>
                        $"!!Validate!! BinderKey('{bindInfo.BinderKey}') in BindInfo don't get IModelViewParamBinder..."
                    );
                    isValid = false;
                }
                else
                {
                    var availableParamBinders = viewType.GetCustomAttributes(false)
                        .OfType<AvailableModelViewParamBinderAttribute>()
                        .SelectMany(_a => _a.AvailableParamBinders)
                        .Distinct();
                    if(availableParamBinders.Any())
                    {
                        if(!availableParamBinders.Any(_t => _t.Equals(paramBinder)))
                        {
                            Logger.LogWarning(LogPriority, () =>
                                $"!!Validate!! '{viewType}' is not available ParamBinder('{paramBinder}')..."
                            );
                            isValid = false;
                        }
                    }
                }
            }
            catch(System.Exception e)
            {
                Logger.LogWarning(LogPriority, () =>
                    $"!!Validate!! BinderKey('{bindInfo.BinderKey}') in BindInfo don't get IModelViewParamBinder..." +
                    $"{System.Environment.NewLine}---{System.Environment.NewLine}{e}{System.Environment.NewLine}---"
                );
                return false;
            }
            return isValid;
        }
    }
}
