using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.AI
{
    /// <summary>
	/// test case
	/// ## INode#ParentEnumerable
	/// ## INode AddParent
	/// <seealso cref="GameTheory"/>
	/// </summary>
    public class TestGameTheory
    {
        const int Order_INode_ParentEnumerable = 0;

        #region INode#ParentEnumerable
        class ParentEnumerableNode : GameTheoryNodeBase<ParentEnumerableNode>
        {
            public int Value { get; set; }

            public override bool Equals(ParentEnumerableNode other)
            {
                return Value == other.Value;
            }

            public override float Evaluate()
            {
                return Value;
            }

            public override IEnumerable<GameTheory.INode> GetChildNodesEnumerable()
            {
                for(var i=0; i<3; ++i)
                {
                    yield return new ParentEnumerableNode() { Value = Value * (i + 1) + 1 };
                }
            }

            public override bool IsTerminal()
                => Value > 100;

            public override string ToString()
            {
                return $"{Value}";
            }
        }

        // A Test behaves as an ordinary method
        [Test, Order(Order_INode_ParentEnumerable), Description("")]
        public void INode_ParentEnumerable_Passes()
        {
            var nodes = Enumerable.Range(0, 11)
                .Select(_i => new ParentEnumerableNode() { Value = _i })
                .ToList();

            //Node Hierachy
            // 0 -> 1 -> 3 -> 4 -> 5
            //             ------>
            //   -> 2 -> 6 -> 2 -> 6...
            //       <---|
            //   -> 8 -> 9 -> 8...
            //        -> 10
            nodes[1].AddParent(nodes[0]);
            nodes[3].AddParent(nodes[1]);
            nodes[4].AddParent(nodes[3]);
            nodes[5].AddParent(nodes[4]);
            nodes[5].AddParent(nodes[3]);

            // Loop nodes
            nodes[2].AddParent(nodes[0]);
            nodes[6].AddParent(nodes[2]);
            nodes[2].AddParent(nodes[6]);

            // Loop nodes2
            nodes[8].AddParent(nodes[0]);
            nodes[9].AddParent(nodes[8]);
            nodes[10].AddParent(nodes[8]);
            nodes[8].AddParent(nodes[9]);

            AssertionUtils.AssertEnumerableByUnordered(
                new GameTheory.ParentEnumerableNode[]
                {
                    // 0 -> 1 -> 3 -> 4 -> 5
                    new GameTheory.ParentEnumerableNode(nodes[5], null),
                    new GameTheory.ParentEnumerableNode(nodes[4], new ParentEnumerableNode[]{ nodes[5] }),
                    new GameTheory.ParentEnumerableNode(nodes[3], new ParentEnumerableNode[]{ nodes[4], nodes[5] }),
                    new GameTheory.ParentEnumerableNode(nodes[1], new ParentEnumerableNode[]{ nodes[3], nodes[4], nodes[5] }),
                    new GameTheory.ParentEnumerableNode(nodes[0], new ParentEnumerableNode[]{ nodes[1], nodes[3], nodes[4], nodes[5] }),
                    // 0 -> 1 -> 3 -> 5
                    new GameTheory.ParentEnumerableNode(nodes[3], new ParentEnumerableNode[]{ nodes[5] }),
                    new GameTheory.ParentEnumerableNode(nodes[1], new ParentEnumerableNode[]{ nodes[3], nodes[5] }),
                    new GameTheory.ParentEnumerableNode(nodes[0], new ParentEnumerableNode[]{ nodes[1], nodes[3], nodes[5] }),
                }
                , nodes[5].GetParentEnumerable()
                , "Fail Normal Node Hierachy..."
            );

            AssertionUtils.AssertEnumerableByUnordered(
                new GameTheory.ParentEnumerableNode[]
                {
                    // 0 -> 2 -> 6
                    new GameTheory.ParentEnumerableNode(nodes[6], null),
                    new GameTheory.ParentEnumerableNode(nodes[2], new ParentEnumerableNode[]{ nodes[6] }),
                    new GameTheory.ParentEnumerableNode(nodes[0], new ParentEnumerableNode[]{ nodes[2], nodes[6] }),
                    // ...2 -> 6 -> 2 -> 6
                    new GameTheory.ParentEnumerableNode(nodes[6], new ParentEnumerableNode[]{ nodes[2], nodes[6] }),
                }
                , nodes[6].GetParentEnumerable()
                , "Fail Loop Node Hierachy..."
            );

            AssertionUtils.AssertEnumerableByUnordered(
                new GameTheory.ParentEnumerableNode[]
                {
                    // 0 -> 8 -> 10
                    new GameTheory.ParentEnumerableNode(nodes[10], null),
                    new GameTheory.ParentEnumerableNode(nodes[8], new ParentEnumerableNode[]{ nodes[10] }),
                    new GameTheory.ParentEnumerableNode(nodes[0], new ParentEnumerableNode[]{ nodes[8], nodes[10] }),
                    // ...8 -> 9 -> 8 -> 10
                    new GameTheory.ParentEnumerableNode(nodes[9], new ParentEnumerableNode[]{ nodes[8], nodes[10] }),
                    new GameTheory.ParentEnumerableNode(nodes[8], new ParentEnumerableNode[]{ nodes[9], nodes[8], nodes[10] }),
                }
                , nodes[10].GetParentEnumerable()
                , "Fail Loop Node Hierachy2..."
            );

        }
        #endregion
    }


}
