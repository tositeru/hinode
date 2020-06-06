using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    /// <summary>
    /// <seealso cref="ViewIdentity"/>
    /// </summary>
    public class OnlyMainIDViewIdentity : ViewIdentity
    {
        public static OnlyMainIDViewIdentity Create(string id)
            => new OnlyMainIDViewIdentity(id);
        public static implicit operator OnlyMainIDViewIdentity(string target)
            => Create(target);

        OnlyMainIDViewIdentity(string id)
            : base(id)
        { }
    }

}
