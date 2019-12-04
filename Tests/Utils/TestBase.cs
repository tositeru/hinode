using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.TestRunner;

namespace Hinode.Tests
{
    public class TestBase
    {
        /// <summary>
        /// テスト終了時にシーンが自動的に
        /// </summary>
        [TearDown]
        public virtual void CleanUpScene()
        {
            var PlaymodeTestsControllerType = GetUnityTestRunnerController();

            var scene = SceneManager.GetActiveScene();
            foreach(var root in scene.GetRootGameObjects()
                .Where(_o => !_o.TryGetComponent(PlaymodeTestsControllerType, out var _)))
            {
                Object.DestroyImmediate(root);
            }
        }

        /// <summary>
        /// UnityEngine.TestTools.TestRunner.PlaymodeTestsControllerのvisibilityがinternalだったのでリフレクション経由でその型を取得するための関数
        /// </summary>
        /// <returns></returns>
        System.Type GetUnityTestRunnerController()
        {
            var asm = System.AppDomain.CurrentDomain.GetAssemblies().First(_asm => _asm.GetName().Name == "UnityEngine.TestRunner");
            var PlaymodeTestsControllerType = asm.GetTypes().Where(_t => _t.IsClass).First(_t => _t.Name == "PlaymodeTestsController");
            return PlaymodeTestsControllerType;
        }
    }
}
