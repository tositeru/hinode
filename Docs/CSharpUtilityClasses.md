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


