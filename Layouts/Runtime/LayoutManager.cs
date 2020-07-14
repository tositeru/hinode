using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hinode.Layouts
{
    /// <summary>
	/// <seealso cref="ILayout"/>
	/// <seealso cref="ILayoutTarget"/>
	/// </summary>
    public class LayoutManager
    {
        public HashSetHelper<ILayout> Layouts { get; } = new HashSetHelper<ILayout>();

        public LayoutManager()
        {
            Layouts.OnAdded.Add((item) => {
                item.OnDisposed.Add(ILayoutOnDisposed);
            });
            Layouts.OnRemoved.Add((item) => {
                item.OnDisposed.Remove(ILayoutOnDisposed);
            });
        }

        void ILayoutOnDisposed(ILayout layout)
        {
            Layouts.Remove(layout);
        }

        public void CaluculateLayouts()
        {
            foreach(var l in Layouts
                .Where(_l => _l.ContainsTarget() && _l.DoChanged))
            {
                l.UpdateUnitSize();
            }

            foreach (var l in Layouts
                .Where(_l => _l.ContainsTarget()))
            {
                l.UpdateLayout();
            }
        }
    }
}
