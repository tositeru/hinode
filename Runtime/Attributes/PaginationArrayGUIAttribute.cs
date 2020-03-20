using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class PaginationArrayGUIAttribute : PropertyAttribute
    {
        readonly int _maxCountPerPage;
        public int MaxCountPerPage
        {
            get
            {
                return _maxCountPerPage;
            }
        }

        public PaginationArrayGUIAttribute(int maxCountPerPage=15)
        {
            _maxCountPerPage = maxCountPerPage;
        }
    }
}
