## C#の便利クラス

Hinodeには以下のC#の便利クラスを提供しています。

- SmartDelegate
- ISingleton
- Type Referection Cache(RefCache)
- Value Update Observer
- Serializer
    - Json Serializer
- Text Resource

### SmartDelegate

`Hinode.SmartDelegate`はSystem.Delegateをラップしたものになり、delegateの追加、削除を簡単に行えるようになっています。

`Hinode.SmartDelegate`は`Hinode.NotInvokableDelegate`から派生しており、delegateへの追加、削除のみを行いたく、実行させたくない場合はそちらを使用してください。

### ISingleton

シングルトンパターンを提供するクラスになります。

### Type Referection Cache(RefCache)

指定したTypeのReflection情報をキャッシュします。

```cshape
class TestClass
{
    int Field1;
    public string Prop1 {get; set;}

    public TestClass(int value) { Field1 = value; }

    public int Func1(int a, int b) => a + b;
}

var refCache = new RefCache(typeof(TestClass));
//Constructor
var inst = refCache.CreateInstance(321);

//Field
var field = refCache.GetField(inst, "Field1");
refCache.SetField(inst, "Field1", 432);

//Property
var prop = refCache.GetProp(inst, "Prop1");
refCache.SetProp(inst, "Prop1", "apple");

//Method
var returnValue = refCache.Invoke(inst, "Func1", 100, 200);
```

### Value Update Observer

IUpdateObserver interfaceは自身が持つ値が変更されたことを通知する機能を持つクラスになります。

Hinodeでは以下の二つの派生クラスを提供しています。

- UpdateObserver<T>
- PredicateUpdateObserver<T>

#### UpdateObserver<T>

T型の値を持つIUpdateObserverになります。

```csharp
var v = new UpdateObserver<int>();
Assert.IsFalse(v.DidUpdated);

v.Value = 100;
Assert.AreEqual(100, v.Value);
Assert.IsTrue(v.DidUpdated);

v.Value = -100;
Assert.AreEqual(-100, v.Value);
Assert.IsTrue(v.DidUpdated);

var prevValue = v.Value;
v.Reset();
Assert.AreEqual(prevValue, v.Value);
Assert.IsFalse(v.DidUpdated);

var counter = 0;
var recievedValue = 0;
v.OnChangedValue.Add((_v) => {
    counter++;
    recievedValue = _v;
});
v.Value = 2000; // <- Call OnChangedValue!

```
#### PredicateUpdateObserver<T>

コンストラクタに指定したdelegateが返す値の変更を監視するIUpdateObserverになります。

```csharp
var observer = new PredicateUpdateObserver<int>(() => {
    counter++;
    return value;
});

//値が変更された時のコールバックの登録
var counter = 0;
var recievedValue = 0;
observer.OnChangedValue.Add((_v) => {
    counter++;
    recievedValue = _v;
});

{//Predicateが返す値が変わった時のテスト
    value = 1;
    var errorMessage = "PredicateUpdateObserver#Updateが呼ばれるまでValue/RawValueは更新されないようにしてください";
    Assert.AreNotEqual(value, observer.RawValue, errorMessage);
    Assert.AreNotEqual(value, observer.Value, errorMessage);
    Assert.AreEqual(1, counter, "PredicateUpdateObserver#Updateが呼ばれるまで設定したPredicateを呼び出さないようにしてください");

    counter = 0;
    Assert.IsTrue(observer.Update());
    Assert.IsTrue(observer.DidUpdated);
    errorMessage = "PredicateUpdateObserver#Updateが呼ばれた時、値が変更されていた時はValue/RawValueも更新するようにしてください";
    Assert.AreEqual(value, observer.RawValue, errorMessage);
    Assert.AreEqual(value, observer.Value, errorMessage);
    Assert.AreEqual(1, counter, "PredicateUpdateObserver#Updateが呼び出された時に設定したPredicateを呼び出すようにしてください");

    // 一度PredicateUpdateObserver#DidUpdateddがtrueになった後の挙動テスト
    counter = 0;
    Assert.IsFalse(observer.Update());
    Assert.IsFalse(observer.DidUpdated);
    Assert.AreEqual(1, counter, "PredicateUpdateObserver#Updateが呼ばれる度に設定したPredicateを呼び出すようにしてください。");
    Assert.AreEqual(value, observer.RawValue);
    Assert.AreEqual(value, observer.Value);
}
```

### Serializer (namespace Hinode.Sizerialzation)

Hinodeは自前のシリアライザーを提供しています。(Hinode.Sizerialzation.ISerializer interface)

System.Runtime.Serialization.ISerializableおよびUnityのJsonUtilityでシリアライズされるものをサポートしています。

シリアライズのルールに付きましては下にある「シリアライズのルール」の方を参照してください。

以下のフォーマットの入出力に対応しています。

- Json Serializer

```csharp
var src = new TestBasicClass()
{
    v1 = 1,
    sub = new TestBasicSubClass { v1 = -1, v2 = "v2 in sub" },
    sub2 = new TestBasicSubClass2 { v1 = 111, v2 = "vvvidv" },
    arr = new int[] { -1, -2, -3 },
    list = new List<string> { "App", "Ora" },
};

//Serialize to Json
var serializer = new JsonSerializer();
var stringWriter = new StringWriter();
serializer.Serialize(stringWriter, src);
var json = stringWriter.ToString();

//Deserialize From Json
var reader = new StringReader(json);
var dest = serializer.Deserialize(reader, typeof(TestBasicClass)) as TestBasicClass;

```

#### シリアライズのルール

型のシリアライズ・デシリアライズを行う時は以下の方法に従います。
複数の方法と一致している場合は上に書かれているものが優先されます。

1. ISerializerに設定されているISerializer.IInstanceCreatorが対応していた場合 -> それを使用する(シリアライズ処理の拡張)
1. System.Runtime.Serialization.ISerializableを継承してる場合 -> ISerializableを使用する
1. System.SerializableAttributeが指定されている場合 ->　publicなフィールドとUnityEngine.SerializeFieldが指定されているフィールドのみ処理対象になる(Unityのシリアライズと同じ)
1. 構造体の場合 -> System.SerializableAttributeと同じ
1. その他 -> 処理は行わない


#### ISerializer.IInstanceCreator

ISerializer.IInstanceCreatorを使用することでISerializerが対応できる型を追加できたり、シリアライズの挙動を変更するなどシリアライズ処理を拡張することができます。
(拡張する際は`DefaultInstanceCreator`から派生することを推奨します。)

拡張する際は以下の3つの関数を実装してください。

- Desrialize
- Serialize
- GetFieldKeyAndTypeDict

```csharp
public interface ISerializer.IInstanceCreator
{
    //Desrializeの時に使用されます。
    object Desrialize(System.Type type, SerializationInfo info, StreamingContext context);

    //Serializeの時に使用されます。
    bool Serialize(object target, SerializationInfo info, StreamingContext context);

    //引数に渡された型と対応するシリアライズ対象のメンバの辞書を返すようにしてください。
    //もし引数に渡した型と一致する辞書がなければ、その型がHasKeyAndTypeDictionaryGetterAttributeを指定されているか確認し、
    // 指定されている場合はそのAttributeから辞書を取得するよう試みます。
    IReadOnlyDictionary<string, System.Type> GetFieldKeyAndTypeDict(System.Type type);
}
```

##### ISerializer.IInstanceCreator.GetFieldKeyAndTypeDict(System.Type type)

ISerializer.IInstanceCreator.GetFieldKeyAndTypeDict()は渡された型のシリアライズされた時のメンバのキー名とその型のペアを表す辞書を返すこと期待されます。

この関数を使用することで、シリアライズされる際に元のクラスのフィールド名とは異なるキー名をつけることができます。

実装の際はISerializer.IInstanceCreator.Serialize()の処理内容と一致するように辞書を作成してください。
ISerializerではこの二つの実装内容が一致しているかどうかの判定は行いませんので注意してください。

#### HasKeyAndTypeDictionaryGetterAttribute

ISerializer.IInstanceCreator.GetFieldKeyAndTypeDict(System.Type type)が対応していない型の場合はISerializerはその型にHasKeyAndTypeDictionaryGetterAttributeが指定されていないか確認します。

もし指定されていた場合は、HasKeyAndTypeDictionaryGetterAttributeからシリアライズされた時のメンバのキー名とその型のペアを表す辞書を取得します。

このAttributeを指定することで指定した型のDefaultの辞書を定義することができます。

### Text Resource

