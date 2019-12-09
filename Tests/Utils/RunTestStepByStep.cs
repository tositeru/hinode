using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Hinode.Tests
{
    public class RunTestStepByStep : MonoBehaviour
    {
        [SerializeField] Snapshot _snapshot;
        [SerializeField] int _currentStep;

        readonly int COMPLETE_STEP = -1;
        public bool IsComplete { get => _currentStep == COMPLETE_STEP; }

        public void GoNext()
        {
            _doNext = true;
            EditorApplication.isPaused = false;
        }

        public void Reset()
        {
            if(_snapshot == null)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(RunStepByStep());
        }

        // Start is called before the first frame update
        void Start()
        {
            if(_snapshot != null)
            {
                DontDestroyOnLoad(this.gameObject);
                Reset();
            }
        }

        bool _doNext = false;
        Snapshot.TestData _testData;
        IEnumerator RunStepByStep()
        {
            _testData = _snapshot.GetTestData();

            _testData.ExecuteSetUpMethods();

            var enumerator = _testData.ExecuteMethod();
            _currentStep = 0;
            do
            {
                EditorApplication.isPaused = true;
                yield return enumerator.Current;
                yield return new WaitUntil(() => _doNext);
                _currentStep++;
                _doNext = false;
            } while (enumerator.MoveNext());

            yield return new WaitUntil(() => _doNext);
            _currentStep = COMPLETE_STEP;
            _testData.ExecuteTearDownMethods();
        }

        public static void SetupScene(Scene scene, Snapshot snapshot)
        {
            var observer = new GameObject("__RunTestStepByStep");
            var stepByStep = observer.AddComponent<RunTestStepByStep>();
            stepByStep._snapshot = snapshot.Copy();
            Selection.activeGameObject = stepByStep.gameObject;
        }
    }
}
