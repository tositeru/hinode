﻿## C#の便利クラス

Hinodeには以下のC#の便利クラスを提供しています。

- SmartDelegate
- ISingleton
- Type Referection Cache(RefCache)
- Value Update Observer
- Serializer
    - Json Serializer
- Text Resources

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
- GetKeyTypeGetter

```csharp
public interface ISerializer.IInstanceCreator
{
    //Desrializeの時に使用されます。
    object Desrialize(System.Type type, SerializationInfo info, StreamingContext context);

    //Serializeの時に使用されます。
    bool Serialize(object target, SerializationInfo info, StreamingContext context);

    //引数に渡された型と対応するISerializationKeyTypeGetterを返すようにしてください。
    //もし引数に渡した型と一致する辞書がなければ、その型がContainsSerializationKeyTypeGetterAttributeを指定されているか確認し、
    // 指定されている場合はそのAttributeからISerializationKeyTypeGetterを取得するよう試みます。
    ISerializationKeyTypeGetter GetKeyTypeGetter(System.Type type)
}
```

##### ISerializer.IInstanceCreator.GetKeyTypeGetter(System.Type type)

ISerializer.IInstanceCreator.GetKeyTypeGetter()は渡された型のシリアライズされた時のメンバのキー名とその型のペアを表す辞書を返すこと期待されます。

この関数を使用することで、シリアライズされる際に元のクラスのフィールド名とは異なるキー名をつけることができます。

実装の際はISerializer.IInstanceCreator.Serialize()の処理内容と一致するように辞書を作成してください。
ISerializerではこの二つの実装内容が一致しているかどうかの判定は行いませんので注意してください。

#### ContainsSerializationKeyTypeGetterAttribute

ISerializer.IInstanceCreator.GetKeyTypeGetter(System.Type type)が対応していない型の場合はISerializerはその型にContainsSerializationKeyTypeGetterAttributeが指定されていないか確認します。

もし指定されていた場合は、ContainsSerializationKeyTypeGetterAttributeからISerializationKeyTypeGetterを取得します。

このAttributeを指定することで指定した型のDefaultのISerializationKeyTypeGetterを定義することができます。

指定した場合はキーとそれに対応する型を返す関数にSerializationKeyTypeGetterAttributeを指定してください。

SerializationKeyTypeGetterAttributeが指定された関数は戻り値がSystem.Typeで引数にstringを受け取るようにしてください。

```csharp
[ContainsSerializationKeyTypeGetter(typeof(TestClass))]
class TestClass
{
    [SerializationKeyTypeGetter]
    static System.Type GetKeyType(string key)
    {
        //...
    }
}
```

もし、ContainsSerializationKeyTypeGetterAttributeが指定されたクラスの親クラスにもContainsSerializationKeyTypeGetterAttributeが指定されていた場合、キーが一致していない場合は親クラスのものも探すようになっています。

優先順位としては以下のようになります。

1. Attributeが指定されたクラスのSerializationKeyTypeGetterAttributeが指定されたメソッド
1. クラス階層が近いものが優先として、親クラスの内、ContainsSerializationKeyTypeGetterAttributeが指定されているクラスのSerializationKeyTypeGetterAttributeが指定されたメソッド
1. どれも一致しない場合はnullを返します。

### Text Resources

TextResourcesはキーとテキストのペアの辞書を持つクラスになります。

埋め込みテキストの管理や多言語対応などに利用できます。

`TextResources.Get()`用のデータクラスとして`IHavingTextResource interface`と`HavingTextResourceData`を用意しています。

```csharp
var resource = new TextResources();
var formattedKey1 = "formattedKey";
var normalKey = "normalKey";
resource
    .Add(formattedKey1, "Apple is {0}.")
    .Add(normalKey, "Orange is furits.");

Assert.AreEqual(2, resource.Count);
Assert.AreEqual("Apple is 100.", resource.Get(formattedKey1, 100));
Assert.AreEqual("Orange is furits.", resource.Get(normalKey));
Assert.IsTrue(resource.Contains(normalKey));
Assert.IsTrue(resource.Contains(formattedKey1));

{
    resource.Dispose();
    Assert.AreEqual(0, resource.Count);
    Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
        Assert.AreEqual("Apple is 100.", resource.Get(formattedKey1, 100));
    });
    Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
        Assert.AreEqual("Orange is furits.", resource.Get(normalKey));
    });
    Assert.IsFalse(resource.Contains(normalKey));
    Assert.IsFalse(resource.Contains(formattedKey1));
}
```

### Collection Helper

Hinodeでは以下のCollectionのヘルパークラスを提供しています。

- HashSetHelper
- ListHelper
- DictionaryHelper
- TreeNodeHelper

これらのHelperクラスはクラスを実装するに当たってCollection操作関係の処理を肩代わりすることを目的に作成されています。

これらのクラスを使用することで重複するメソッドの共通化やテストコードの削減を図ることができます。

各Helperクラスには追加・削除などのタイミングでコールバックを設定することができるように設計されています。

もしCollection操作の時に追加の処理を行いたい時はコールバックを登録することで対応することが可能になっています。

また、コールバックも含めたCollection操作内で例外が発生した時は内部でキャッチするようになっているため、操作に失敗した場合でも処理を続行できるように設計されています。
(例外が発生した時はログに出力します。)

```csharp
var helper = new HashSetHelper();
helper.OnAdded.Add((addedValue) => ...);
helper.OnRemoved.Add((removedValue) => ...);
helper.OnCleared.Add(() => ...);
```

#### TreeNodeHelper

TreeNodeHelperは他のCollection Helperとは異なり、IReadOnlyTreeNodeから派生していません。

その代わりにこのクラスを継承してTreeデータ構造を利用することができます。
継承する際は以下の点に注意してください。

- TreeNodeHelper<T>#Valueに自身の参照を設定すること。

コード例は以下のようになります。

```
class Test : TreeNodeHelper<Test>
{
    public Test()
    {
        base.Value = this; // <- Self reference must set to TreeNodeHelper<T>#Value!!!
    }
    // ....
}
```

### Math

Hinodeでは以下の数学ライブラリを用意しています。

- フーリエ変換
- IndexCombinationEnumerable: 指定した長さの配列に対する添字の全ての組み合わせを列挙するEnumerable

### Array2D

Hinodeでは多次元配列の便利クラスを提供しています。

- Array2D<T>


UnityのSerializeにはUnityの仕様上、Templateを含む型はシリアライズの対象にならないため対応していませんが、
拡張Editorに対応した`Hinode.Editors.Array2DEditor`を提供おり、以下の機能を提供しています。

こちらのクラスを使用することでInspector上などにArray2Dを表示することができますので利用してください。

- 配列内の部分的な表示 (CountPerPage/PageOffset)
    利便性または処理負荷などから画面に表示する際、表示対象のデータの範囲を指定することが可能です。
- 2段回に分けたサイズ変更 (EditingSize)
- 配列のサイズ変更時に元々あったデータをシフトさせる機能 (EditingShiftOffset/PrevShiftOffset)
    配列サイズを変更する際は変更するサイズを指定したからボタンを押す必要があります。
    その際に元のデータを指定したオフセット分ズラすことも可能です。

```csharp
using Hinode.Editors;
using UnityEditor;

class Hoge : Editor
{
    Array2D<int> _array2D = new Array2D<int>();
    Array2DEditor<int> Array2DEditor;

    void OnEnable()
    {
        Array2DEditor = new Array2DEditor<int>(new GUIContent("Title"));
        Array2DEditor.OnChanged.Add(OnChangedArray2D);
    }

    void OnChangedArray2D(Array2D<int> inst, Array2DEditor<int>.ValueKind kinds)
    {
        //Resize Board and Update Data Page UI
        if (0 != (kinds & Array2DEditor<int>.ValueKind.Size))
        {
            //Changed Array2D<int>#Size.
        }

        //Changed Draw Page(for GUI)
        if (0 != (kinds & Array2DEditor<int>.ValueKind.PageParams))
        {
            //Changed Array2D<int> Data
        }
    }

    void OnInspectorGUI()
    {
        Array2DEditor.Draw(_array2D);
    }
}
```

### Labels

Hinodeではラベルを表すクラスを提供しています。

このクラスと関連したクラス・AttributeをHinodeでは提供していますのでそちらと合わせて使用してください。

- LabelsAttribute
- BindCallbackAttribute
- LabelsObject(Unity)

#### Labels#DoMatch

Labelsの一致判定用の関数として`Labels#DoMatch(MatchOp op, string[] labels)`を提供しています。

一致判定の際は以下のOptionを指定してください。
- MatchOp#Complete : Labelsが持つlabelと完全に一致しているかどうか?
- MatchOp#Included : Labelsが持つlabelが全て含まれているかどうか?
- MatchOp#Included : Labelsが持つlabelの内一つでも一致しているかどうか？
