using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewIdentity : System.IEquatable<ViewIdentity>
    {
        public static readonly char SEPARATOR = '.';

        static readonly Regex ValidCharRegex = new Regex(@"^[\w_\?!]+$");

        public static ViewIdentity Create(params string[] terms)
            => new ViewIdentity(terms);
        public static ViewIdentity Parse(string id)
        {
            Assert.IsNotNull(id);
            return new ViewIdentity(id.Split(SEPARATOR));
        }
        public static implicit operator ViewIdentity(string parseTarget)
            => Parse(parseTarget);

        List<string> _childIdentities;

        public string MainID { get; }
        public bool HasChildIDs { get => _childIdentities?.Any() ?? false; }
        public IEnumerable<string> ChildIDs { get => _childIdentities; }
        public bool IsEmpty { get => MainID == null || MainID == ""; }
        protected ViewIdentity(params string[] terms)
        {
            if(terms.Length <= 0
                || (terms.Length == 1 && (terms[0] == null || terms[0] == "")))
            {
                MainID = null;
                _childIdentities = new List<string>();
                return;
            }

            foreach(var t in terms)
            {
                Assert.IsTrue(ValidCharRegex.IsMatch(t,0)
                    , $"IDを構成する要素は英数字,_,!,?だけにしてください... viewID term={t}");
            }

            MainID = terms[0];
            _childIdentities = terms.Skip(1).ToList();
        }

        public override string ToString()
        {
            return $"{MainID}{ChildIDs.Aggregate("", (_s, _c) => _s + "." + _c)}";
        }


        #region System.IEquatable<ViewIdentity> interface
        public bool Equals(ViewIdentity other)
            => MainID == other.MainID
            && ChildIDs.Count() == other.ChildIDs.Count()
            && ChildIDs.Zip(other.ChildIDs, (s, o) => (s, o)).All(_t => _t.s == _t.o);

        public override bool Equals(object obj)
            => obj is ViewIdentity
            ? this.Equals(obj)
            : false;

        public static bool operator ==(ViewIdentity left, ViewIdentity right)
            => left.Equals(right);
        public static bool operator !=(ViewIdentity left, ViewIdentity right)
            => !left.Equals(right);

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
