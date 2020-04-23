using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Hinode
{
    public class ButtonViewObject : MonoBehaviourViewObject
    {
        string _textPath;
        ChildObject<Text> _text;

        Text Text { get => ChildObject<Text>.GetOrCreate(ref _text, transform, _textPath); }

        public string TextPath
        {
            get => _textPath;
            set
            {
                _textPath = value;
                _text = new ChildObject<Text>(transform, _textPath);
            }
        }

        public class ParamBinder : IModelViewParamBinder
        {
            public string TextPath { get; set; } = "Text";

            public void Update(Model model, IViewObject viewObj)
            {
                Assert.IsTrue(model is ButtonModel);
                Assert.IsTrue(viewObj is ButtonViewObject);
                var btn = model as ButtonModel;
                var view = viewObj as ButtonViewObject;

                if(TextPath != view.TextPath)
                {
                    view.TextPath = TextPath;
                }

                view.Text.text = btn.Text;
            }
        }
    }
}
