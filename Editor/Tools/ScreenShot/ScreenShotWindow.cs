using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.EditorCoroutines.Editor;

namespace Hinode.Editors
{
    public class ScreenShotWindow : EditorWindow
    {
        [MenuItem("Hinode/Screen Shot Window")]
        public static void Open()
        {
            var window = EditorWindow.CreateWindow<ScreenShotWindow>();
            window.Show();
        }

        [SerializeField] Camera _useCamera;
        [SerializeField] Vector2Int _imageSize = new Vector2Int(1024, 1024);
        [SerializeField] int _superSize = 1;
        [SerializeField] string _assetPath = "screenshot";

        EditorCoroutine _captureScreenshotCoroutine = null;

        private void OnGUI()
        {
            _useCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Use Camera"), _useCamera, typeof(Camera), true);
            if(_useCamera != null)
            {
                _imageSize = EditorGUILayout.Vector2IntField(new GUIContent("Image Size"), _imageSize);
            }
            else
            {
                _superSize = EditorGUILayout.IntSlider("Super Size", _superSize, 1, 4);
            }

            _assetPath = EditorGUILayout.TextField("Asset Path", _assetPath);

            if(!EditorApplication.isPlaying && GUILayout.Button("Start Screenshot"))
            {
                EditorApplication.isPlaying = true;
            }

            if(EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Finish Screenshot"))
                {
                    EditorApplication.isPlaying = false;
                }

                if (_captureScreenshotCoroutine == null && GUILayout.Button("Take Screen shot"))
                {
                    _captureScreenshotCoroutine = this.StartCoroutine(CaptureScreenshot());
                }
            }
        }

        IEnumerator CaptureScreenshot()
        {
            if (!EditorApplication.isPlaying)
            {
                Logger.LogError(Logger.Priority.High, () => "Please Must Run at playmode...");
                yield break;
            }

            yield return new WaitForEndOfFrame();

            var path = Path.Combine("Assets", Path.ChangeExtension(_assetPath, ".png"));
            Texture2D screenshot = null;
            if (_useCamera == null)
            {
                ScreenCapture.CaptureScreenshot(path, _superSize);
            }
            else
            {
                var cachedActiveRT = RenderTexture.active;
                var cachedCameraRT = _useCamera.targetTexture;

                var RT = new RenderTexture(_imageSize.x, _imageSize.y, 1, RenderTextureFormat.Default);
                RenderTexture.active = RT;
                _useCamera.targetTexture = RT;
                _useCamera.Render();

                screenshot = new Texture2D(RT.width, RT.height, TextureFormat.RGBA32, 1, false);
                screenshot.ReadPixels(new Rect(0, 0, RT.width, RT.height), 0, 0);
                screenshot.Apply();

                _useCamera.targetTexture = cachedCameraRT;
                RenderTexture.active = cachedActiveRT;

                File.WriteAllBytes(path, ImageConversion.EncodeToPNG(screenshot));
            }
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
            Logger.Log(Logger.Priority.Low, () => $"Success Take screenshot! path={path}");

            _captureScreenshotCoroutine = null;
            yield break;
        }
    }
}
