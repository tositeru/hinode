## C#/Unity Componentsの拡張メソッド

以下の拡張メソッドを提供しています。

### C#

- System.Array
    - GetEnumerable()
    - GetEnumerable<T>()
- System.Delegate
    - ClearInvocations()
    - ClearInvocations<T>() where T : System.Delegate
- System.Collections.Generic.Dictionary
    - Merge<TKey, TValue>(bool isOverwrite, params IEnumerable<KeyValuePair<TKey, TValue>>[] srcDicts)
    - Merge<TKey, TValue>(bool isOverwrite, IEnumerable<IEnumerable<KeyValuePair<TKey, TValue>>> srcDicts)
- System.Runtime.Serialization.SerializationInfo
    - GetEnumerable()
- System.IO.TextReader
    - MoveTo(char)
    - MoveTo(char[])
    - MoveTo(Regex)
    - SkipTo(char)
    - SkipTo(char[])
    - SkipTo(Regex)
    - IsMatchPeek(char)
    - IsMatchPeek(char[])
    - IsMatchPeek(Regex)
- System.Type
    - IsSameOrInheritedType(System.Type type)
    - IsSameOrInheritedType<T>()
    - ContainsInterface(System.Type interfaceType)
    - ContainsInterface<T>()
    - GetFieldInHierarchy(string name, BindingFlags flags)
    - IsArrayOrList()
    - GetArrayElementType()
    - EqualGenericTypeDefinition()
    - GetClassHierarchyEnumerable()
    - IsInteger()
    - IsFloat()
    - IsNumeric()
    - IsStruct()
    - ParseToNumber(string str)
    - TryParseToNumber(string str, out object outNum)

### Unity

- AssetBundle
    - LoadGameObjectComponent<T>(string name) where T : Component
    - LoadGameObjectComponent(string name, System.Type type)
- Color
    - HSVToRGBA(float H, float S, float V, float a, bool hdr=false) <= not Extension Methods
- GameObject
    - GetOrAddComponent<T>() where T : Component
    - GetOrAddComponent(System.Type componentType)
    - Create(string name, Transform parent) <= not Extension Methods
- MonoBehaviour
    - AssertObjectReference(HashSet<object> objHash = null)
    - SafeStartCoroutine(ref Coroutine coroutine, IEnumerator routine)
- RangeAttribute
    - IsInRange(float value)
    - IsInRange(double value)
    - Clamp(float value)
    - Clamp(double value)
- Rect
    - Overlaps(Vector2 point)
    - Overlaps(Rect other)
- Scene
    - GetGameObjectEnumerable()
- Transform
    - GetChildEnumerable()
    - GetHierarchyEnumerable()
    - GetParentEnumerable()
- Vector2
    - TryParse(string text, out Vector2 result) <= not Extension Methods
