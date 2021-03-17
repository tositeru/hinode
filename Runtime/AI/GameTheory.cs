// Copyright 2019 ~ tositeru
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hinode
{
    public abstract class GameTheory
    {
        public interface INode
        {
            IReadOnlyCollection<INode> Parents { get; }
            void AddParent(INode parent);
            void RemoveParent(INode parent);

            float Evaluate();
            bool IsTerminal();
            //float CalNearlyValue(INode node);

            IEnumerable<INode> GetChildNodesEnumerable();
            ParentEnumerable GetParentEnumerable();
        }

        protected abstract INode EvaluateImpl(int depth);
        protected abstract INode SelectRemoveNodeFromRecordedResults();

        List<INode> _recordedResults = new List<INode>();

        public INode CurrentNode { get; set; }
        public int RecordResultCount { get; private set; } = 5;
        public IReadOnlyList<INode> RecordedResults { get => _recordedResults; }

        public IReadOnlyList<INode> Evaluate(int depth, int recordResultCount = 5)
        {
            RecordResultCount = recordResultCount;
            _recordedResults.Clear();

            depth = System.Math.Max(0, depth);

            EvaluateImpl(depth);
            return _recordedResults;
        }

        public IEnumerable<T> Evaluate<T>(int depth, int recordResultCount = 5)
            where T : INode
            => Evaluate(depth, recordResultCount).OfType<T>();

        protected INode Alpha(INode node, int depth, INode alpha)
        {
            if (node.IsTerminal() || depth == 0)
            {
                AddToRecordedResults(node);
                return node;
            }

            foreach (var child in node.GetChildNodesEnumerable())
            {
                var childNode = Alpha(child, depth - 1, alpha);
                if (childNode.Evaluate() >= alpha.Evaluate())
                {
                    alpha = childNode;
                }
            }
            return alpha;
        }

        void AddToRecordedResults(INode node)
        {
            var sameIndex = _recordedResults.FindIndex(_n => _n.Equals(node));
            if (-1 != sameIndex)
            {//同一局面があったら、既にあるものにnode#Parentsを追加する
                var sameNode = _recordedResults[sameIndex];
                foreach (var p in node.Parents
                    .Where(_p => !sameNode.Parents.Contains(_p)))
                {
                    sameNode.AddParent(p);
                }
                return;
            }

            var nodeEvalValue = node.Evaluate();
            var index = _recordedResults.FindIndex(_n => nodeEvalValue >= _n.Evaluate());
            if (index == -1)
            {
                if (_recordedResults.Count <= 0)
                    _recordedResults.Add(node);
                return;
            }

            _recordedResults.Insert(index, node);
            if (_recordedResults.Count > RecordResultCount)
            {
                var removeNode = SelectRemoveNodeFromRecordedResults();
                _recordedResults.Remove(removeNode);
            }
        }

        public IReadOnlyList<TNode> EvaluateUntilTerminal<TNode>(TNode startNode, int depth, int recordNodeCount, System.Action<IReadOnlyList<TNode>, int> onStepped = null)
            where TNode : INode
        {
            var i = 0;
            List<TNode> results = new List<TNode>();
            if (startNode.IsTerminal()) return results;

            Stack<TNode> _stack = new Stack<TNode>();
            _stack.Push(startNode);
            do
            {
                CurrentNode = _stack.Pop();
                var tmpResults = Evaluate<TNode>(depth, recordNodeCount);

                results.AddRange(tmpResults
                    .Where(_r => _r.IsTerminal()));

                foreach (var r in tmpResults
                    .Where(_r => !_r.IsTerminal() && !_stack.Any(_n => _n.Equals(_r))))
                {
                    _stack.Push(r);
                }
                i++;

                onStepped?.Invoke(results, _stack.Count);
            } while (_stack.Count > 0);
            return results;
        }

        public class ParentEnumerableNode : System.IEquatable<ParentEnumerableNode>
        {
            public INode Node { get; }
            public bool IsLoop { get; }
            public bool DoHasParents { get => Node?.Parents.Count > 0; }
            public IEnumerable<INode> TraversedNodes { get; }

            public ParentEnumerableNode(INode node, IEnumerable<INode> traversedNodes)
            {
                Node = node;
                TraversedNodes = traversedNodes;

                IsLoop = traversedNodes?.Contains(node) ?? false;
            }

            #region IEqualable interface
            public bool Equals(ParentEnumerableNode other)
            {
                if (!Node.Equals(other.Node)
                    || IsLoop != other.IsLoop
                    || DoHasParents != other.DoHasParents)
                    return false;

                if(TraversedNodes == null)
                {
                    return (!other.TraversedNodes?.Any()) ?? true;
                }
                else if (other.TraversedNodes == null)
                {
                    return (!TraversedNodes?.Any()) ?? true;
                }
                else
                {
                    var counter = 0;
                    foreach(var (t, o) in TraversedNodes.Zip(other.TraversedNodes, (_t, _o) => (_t, _o)))
                    {
                        if (!t.Equals(o)) return false;
                        counter++;
                    }
                    return counter == other.TraversedNodes.Count();
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is ParentEnumerableNode) return Equals(obj as ParentEnumerableNode);
                return base.Equals(obj);
            }

            public override int GetHashCode()
                => base.GetHashCode();
            #endregion

            public override string ToString()
            {
                string s = Node.ToString() + TraversedNodes?.Aggregate(",", (_s, _c) => _s + _c + ",") ?? "(empty)";
                s += " IsLoop=" + IsLoop.ToString();
                s += " HasParent=" + DoHasParents.ToString();
                return s;
            }
        }

        public class ParentEnumerable : IEnumerable<ParentEnumerableNode>, IEnumerable
        {
            INode _target;

            Stack<INode> _traversedNodes = new Stack<INode>();
            public IEnumerable<INode> TraversedNodes { get => _traversedNodes; }

            public ParentEnumerable(INode target)
            {
                _target = target;
            }

            public IEnumerator<ParentEnumerableNode> GetEnumerator()
            {
                Stack<INode> stack = new Stack<INode>();
                stack.Push(_target);
                while (stack.Count > 0)
                {
                    var node = stack.Pop();
                    var parentEnumerable = new ParentEnumerableNode(node, _traversedNodes);
                    yield return parentEnumerable;

                    //if (parentEnumerable.IsLoop) continue;

                    if(parentEnumerable.DoHasParents && !parentEnumerable.IsLoop)
                    {
                        while (_traversedNodes.Count > 0)
                        {
                            var child = _traversedNodes.Peek();
                            if (child.Parents.Contains(node))
                                break;

                            _traversedNodes.Pop();
                        }

                        _traversedNodes.Push(node);
                        foreach (var p in node.Parents
                            .Where(_p => !stack.Contains(_p)))
                        {
                            stack.Push(p);
                        }
                    }
                    else if(stack.Count > 0)
                    {
                        var next = stack.Peek();
                        INode rollbackNode = null;
                        foreach(var t in _traversedNodes)
                        {
                            if (t == next) break;
                            if (t.Parents.Contains(next))
                                rollbackNode = t;
                        }
                        while (rollbackNode != _traversedNodes.Peek())
                        {
                            _traversedNodes.Pop();
                        }
                    }

                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }
    }

    public abstract class GameTheoryNodeBase<TNode>
        : GameTheory.INode
        , System.IEquatable<TNode>
        where TNode : class, GameTheory.INode
    {
        #region GameTheory.INode interface
        protected HashSet<GameTheory.INode> _parents = new HashSet<GameTheory.INode>();
        public IReadOnlyCollection<GameTheory.INode> Parents { get => _parents; }

        public abstract float Evaluate();
        public abstract bool IsTerminal();
        public abstract IEnumerable<GameTheory.INode> GetChildNodesEnumerable();


        public void AddParent(GameTheory.INode parent)
        {
            if (parent is TNode) AddParent(parent as TNode);
        }
        public void AddParent(TNode parent)
        {
            if (parent != null) _parents.Add(parent);
        }

        public GameTheory.ParentEnumerable GetParentEnumerable()
        {
            return new GameTheory.ParentEnumerable(this);
        }

        public void RemoveParent(GameTheory.INode parent)
        {
            if (parent is TNode) RemoveParent(parent as TNode);
        }
        public void RemoveParent(TNode node)
            => _parents.Remove(node);
        #endregion

        #region System.IEquatable<TNode> interface 
        public abstract bool Equals(TNode other);

        public override bool Equals(object obj)
        {
            if (obj is TNode) return Equals(obj as TNode);
            return base.Equals(obj);
        }
        public override int GetHashCode()
            => base.GetHashCode();
        #endregion
    }
}
