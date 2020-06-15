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

### Serializer

- Json Serializer

### Text Resource

