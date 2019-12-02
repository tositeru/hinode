using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode.MVC
{
    /// <summary>
    /// アプリ上の全てのModelのルートとなるモデル
    /// </summary>
    public class ModelRooter : SingletonMonoBehaviour<ModelRooter>
    {
        public static readonly string NAME = "__root";
        Model _model = new Model() { Name = NAME };
        public virtual Model Model { get => _model; }

        #region SingletonMonoBehaviour<> interface
        protected override string DefaultInstanceName => "__ModelRooter";

        protected override void OnAwaked()
        {
            DontDestroyOnLoad(this);
        }

        protected override void OnDestroyed(bool isInstance)
        {
        }
        #endregion
    }
}
