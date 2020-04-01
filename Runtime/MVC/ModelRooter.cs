using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// アプリ上の全てのModelのルートとなるモデル
    /// </summary>
    public class ModelRooter : IModelMonoBehaivour
    {
        IModel _model = new Model() { Name = "__root" };
        public override IModel Model { get => _model; }
    }
}
