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

- 