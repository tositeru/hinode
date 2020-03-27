using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Hinode.Editors
{
    /// <summary>
    /// <seealso cref="TextureFilepathAttribute"/>
    /// </summary>
	[CustomPropertyDrawer(typeof(TextureFilepathAttribute))]
    public class TextureFilepathAttributeDrawProperty : PropertyDrawer
	{
        string _usedFilepath;
        Texture2D _tex;
        Texture2D GetTexture(string filepath)
        {
            if (_usedFilepath == filepath) return _tex;

            if(EditorFileUtils.IsExistAsset(filepath))
            {//Assetの時
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(filepath);
                if (tex == null) return null;

                _tex = tex;
                _usedFilepath = filepath;
                return _tex;
            }
            else
            {//Assetではない場合
                if (!File.Exists(filepath))
                {
                    _tex = null;
                    return null;
                }

                if (_tex != null) return _tex;

                var tex = new Texture2D(4, 4);
                var bytes = File.ReadAllBytes(filepath);
                if (tex.LoadImage(bytes))
                {
                    _tex = tex;
                    _usedFilepath = filepath;
                }
                return _tex;
            }
        }

		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// First get the attribute since it contains the range for the slider
			TextureFilepathAttribute textureFilepath = attribute as TextureFilepathAttribute;

            if(property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.LabelField(position, label.text, "Use TextureFilepath with string.");
				return;
			}

            EditorGUI.TextField(position, property.displayName, property.stringValue);

            //GUILayout.Space(128 + position.height);

            var offset = 5;
            position = GUILayoutUtility.GetLastRect();
            position.y += offset + position.height;
            var areaSize = position.size + new Vector2(0, 128);
            position = new Rect(position.position, areaSize);
            //他のGUILayoutのために表示領域を確保している
            GUILayout.Space(position.height + offset + EditorGUI.GetPropertyHeight(property, label));
            var tex = GetTexture(property.stringValue);
            if (tex != null)
            {
                EditorGUI.DrawTextureTransparent(position, tex, ScaleMode.ScaleToFit);
            }
            else
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                };
                EditorGUI.LabelField(position, "Don't Exist Texture.", style);
            }
        }
    }
}
