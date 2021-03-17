using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Hinode.Editors
{
    /// <summary>
	/// 
	/// </summary>
    public abstract class GameTheoryEditor<TGame, TNode> : ManualEditorBase
        where TGame : GameTheory
        where TNode : class, GameTheory.INode
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <param name="style"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        protected abstract bool DrawInspector(TGame target, FieldInfo fieldInfo = null);

        protected virtual bool DrawAdditiveParamters() { return false; }

        public delegate void OnTraverseSteppedDelegate(GameTheoryEditor<TGame, TNode> self, IReadOnlyList<TNode> results, int stackCount);
        public delegate void OnTraversedDelegate(GameTheoryEditor<TGame, TNode> self, IReadOnlyList<TNode> results);

        SmartDelegate<OnTraverseSteppedDelegate> _onTraverseStepped = new SmartDelegate<OnTraverseSteppedDelegate>();
        public NotInvokableDelegate<OnTraverseSteppedDelegate> OnTraverseStepped { get => _onTraverseStepped; }

        SmartDelegate<OnTraversedDelegate> _onTraversed = new SmartDelegate<OnTraversedDelegate>();
        public NotInvokableDelegate<OnTraversedDelegate> OnTraversed { get => _onTraversed; }

        public int TraversingDepth { get; set; } = 3;
        public int BufferNodeCount { get; set; } = 5;
        public Vector2Int TraversingDepthRange { get; set; } = new Vector2Int(1, 10);
        public Vector2Int BufferNodeCountRange { get; set; } = new Vector2Int(5, 100);

        CancellationTokenSource _traverseTaskCancellationTokenSrc;
        Task _traverseTask;

        public bool IsComplete { get; private set; } = true;
        public double TraversingTimer { get; private set; }
        public int TraversingStackCount { get; private set; }
        public int TraversedResultCount { get; private set; }

        public bool DoChanged { get; set; }

        public GameTheoryEditor(OwnerEditor owner)
            : base(owner, new GUIContent("GameTheory"))
        { }

        public GameTheoryEditor(OwnerEditor owner, GUIContent rootLabel)
            : base(owner, rootLabel)
        {}

        public bool Draw(TGame target, FieldInfo fieldInfo = null)
        {
            DoChanged = false;

            RootFoldout = EditorGUILayout.Foldout(RootFoldout, RootLabel);
            if (!RootFoldout) return false;

            using (var indent = new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.DoubleField("Execute Time(s)", TraversingTimer, GUILayout.ExpandWidth(false));
                EditorGUILayout.IntField("Results Count", TraversedResultCount, GUILayout.ExpandWidth(false));
                EditorGUILayout.IntField("Stack Count", TraversingStackCount, GUILayout.ExpandWidth(false));

                var newTraversingDepth = EditorGUILayout.IntSlider("TraversingDepth", TraversingDepth, TraversingDepthRange.x, TraversingDepthRange.y);
                var newBufferNodeCount = EditorGUILayout.IntSlider("Buffer Node Count", BufferNodeCount, BufferNodeCountRange.x, BufferNodeCountRange.y);
                DoChanged |= DrawAdditiveParamters();

                if (IsComplete)
                {
                    TraversingDepth = newTraversingDepth;
                    BufferNodeCount = newBufferNodeCount;

                    DoChanged |= DrawInspector(target, fieldInfo);
                    if (GUILayout.Button("Start to Traverse Game"))
                    {
                        if (IsComplete)
                        {
                            StartTraverseWithMultiThread();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Now Calculate Game...");
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Cancel Caluculation?"))
                    {
                        StopCalculate();
                    }
                }
            }

            return DoChanged;
        }

        abstract protected TGame CreateGameInstance();
        abstract protected TNode CreateNodeInstance();

        void Traverse()
        {
            if (!IsComplete) return;

            IsComplete = false;

            var checker = CreateGameInstance();

            var timer = System.DateTime.Now;

            TraversedResultCount = 0;
            TraversingStackCount = 0;

            var startNode = CreateNodeInstance();
            var results = checker.EvaluateUntilTerminal<TNode>(checker.CurrentNode as TNode, TraversingDepth, BufferNodeCount, OnStepped);

            var timer2 = System.DateTime.Now;

            var timespan = new System.TimeSpan(timer2.Ticks - timer.Ticks);
            TraversingTimer = timespan.TotalSeconds;

            _onTraversed.SafeDynamicInvoke(this, results, () => "Fail in Traverse()...");
            IsComplete = true;

            Owner.RepaintOwner();
        }

        void OnStepped(IReadOnlyList<TNode> results, int stackCount)
        {
            TraversedResultCount = results.Count;
            TraversingStackCount = stackCount;
            _onTraverseStepped.SafeDynamicInvoke(this, results, stackCount, () => "Fail in OnStepped...");
        }

        void StartTraverseWithMultiThread()
        {
            if (!IsComplete) return;

            if (_traverseTask != null)
            {
                StopCalculate();
            }

            _traverseTaskCancellationTokenSrc = new CancellationTokenSource();
            _traverseTask = Task.Run(() => {
                Traverse();

                if (_traverseTaskCancellationTokenSrc != null)
                {
                    _traverseTaskCancellationTokenSrc.Dispose();
                    _traverseTaskCancellationTokenSrc = null;
                }
                if (_traverseTask != null)
                {
                    _traverseTask = null;
                }
            }, _traverseTaskCancellationTokenSrc.Token);
        }

        public void StopCalculate()
        {
            if (_traverseTaskCancellationTokenSrc != null)
            {
                _traverseTaskCancellationTokenSrc.Cancel();
                _traverseTaskCancellationTokenSrc.Dispose();
            }

            _traverseTaskCancellationTokenSrc = null;
            _traverseTask = null;
            IsComplete = true;
        }

    }
}
