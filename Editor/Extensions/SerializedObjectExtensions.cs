using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

namespace Hinode.Editors
{
    public static class SerializedObjectExtensions
    {
        public static SerializedObject Copy(this SerializedObject srcSO, SerializedObject destSO)
        {
            var it = srcSO.GetIterator();
            it.Next(true);
            for (; it.Next(false);)
            {
                destSO.CopyFromSerializedProperty(it);
            }
            destSO.ApplyModifiedProperties();
            return destSO;
        }

        public static T Copy<T>(this SerializedObject srcSO, T dest)
            where T : Object
        {
            Assert.IsNotNull(srcSO.targetObject);
            Assert.AreEqual(typeof(T), srcSO.targetObject.GetType(), $"Don't be different type...");
            return srcSO.Copy(new SerializedObject(dest)).targetObject as T;
        }

        public static T Copy<T>(T src, T dest)
            where T : Object
        {
            var srcSO = new SerializedObject(src);
            return srcSO.Copy(new SerializedObject(dest)).targetObject as T;
        }

        public static void LogProperties(this SerializedObject target)
        {
            var e = target.GetIterator();
            var log = $"{target.GetType().FullName} has below props:" + System.Environment.NewLine;
            while(e.Next(true))
            {
                log += $"-- path={e.propertyPath} propType={e.propertyType}, type={e.type}" + System.Environment.NewLine;
            }
            Debug.Log(log);
        }
    }
}
