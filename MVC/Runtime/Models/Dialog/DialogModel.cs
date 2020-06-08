using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    public class DialogModel : Model
        , IOnPointerClickReciever
    {
        public readonly static string DIALOG_BUTTON_ID = "#DialogButton";
        public readonly static string DIALOG_OK_BUTTON_ID = "#OKButton";
        public readonly static string DIALOG_YES_BUTTON_ID = "#YesButton";
        public readonly static string DIALOG_NO_BUTTON_ID = "#NoButton";
        public readonly static string DIALOG_CANCEL_BUTTON_ID = "#CancelButton";
        public readonly static string INTERRUPTABLE_DIALOG_ID = "#InterruptableDialog";
        
        List<ButtonModel> _buttons = new List<ButtonModel>();

        public string Title { get; set; }
        public string Text { get; set; }

        public IEnumerable<ButtonModel> Buttons { get; }

        public DialogModel()
        {
        }

        public DialogModel AddButton(string key, string text, ModelIDList logicalID = null, ModelIDList stylingID = null)
        {
            var btn = new ButtonModel()
            {
                Text = text,
                Value = key,
                LogicalID = logicalID,
                StylingID = stylingID,
                SiblingOrder = (uint)((100 - _buttons.Count) * 100),
            };
            btn.AddLogicalID(DIALOG_BUTTON_ID);
            _buttons.Add(btn);
            btn.Parent = this;
            return this;
        }

        #region IOnPointerClickReciever interface
        [AllowSenderModelType(typeof(ButtonModel))]
        public void OnPointerClick(Model sender, IOnPointerEventData eventData)
        {
            if (!(sender is ButtonModel)) return;
            var btn = sender as ButtonModel;
            if (!(btn.Value is string))
            {
                Logger.LogError(Logger.Priority.High, () => $"ButtonModel#Value must be string... got Type={btn.Value.GetType()}");
                return;
            }
            MarkDestroy();
        }
        #endregion

        #region EventInterrupter Utils
        public enum DialogType
        {
            OKCancel,
            YesNoCancel,
        }

        public static OnEventInterruptCallback CreateDialogOnEventInterrupted(DialogType dialogType, TextResources textResources)
        {
            return (binderInstanceMap, interruptedData) => {
                var dialogModel = new DialogModel();
                switch(dialogType)
                {
                    case DialogType.OKCancel:
                        dialogModel
                            .AddButton("OK", "OK", new ModelIDList(DialogModel.DIALOG_OK_BUTTON_ID))
                            .AddButton("Cancel", "Cancel", new ModelIDList(DialogModel.DIALOG_CANCEL_BUTTON_ID));
                        break;
                    case DialogType.YesNoCancel:
                        dialogModel
                            .AddButton("Yes", "Yes", new ModelIDList(DialogModel.DIALOG_YES_BUTTON_ID))
                            .AddButton("No", "No", new ModelIDList(DialogModel.DIALOG_NO_BUTTON_ID))
                            .AddButton("Cancel", "Cancel", new ModelIDList(DialogModel.DIALOG_CANCEL_BUTTON_ID));
                        break;
                }

                if (interruptedData.SenderModel is IHavingDialogData)
                {
                    var dialogData = interruptedData.SenderModel as IHavingDialogData;
                    dialogData.SetTexts(dialogModel, textResources);
                }
                return (dialogModel, false);
            };
        }
        #endregion
    }
}