## Unityの便利クラス

### ChildObject

子GameObjectのComponentにアクセスするための便利クラスになります。

```csharp
class Behaviour : MonoBehaviour
{
    ChildObject<BoxCollider> _childObj;

    public BoxCollider ChildObj
    {
        get => ChildObject<BoxCollider>.GetOrCreate(ref _childObj, transform, "ChildObj");
    }
}
```

### SceneObject

現在アクティブになっているSceneにあるGameObjectにアクセスする便利クラスになります。

```csharp
class Behaviour : MonoBehaviour
{
    SceneObject<BoxCollider> _childObj;

    public BoxCollider ChildObj
    {
        get => ChildObject<BoxCollider>.GetOrCreate(ref _childObj, transform, "RootObj/ChildObj");
    }
}
```

### LabelObject

GameObjectに追加のLabelを追加できるようにするComponentになります。

有効なラベル文字列は以下の文字のみを含むものになります。

- アルファベット
- 数字
- _

```csharp
var gameObject = new GameObject("obj");
var label = gameObject.AddComponent<LabelObject>();

var word = "word";
label.Add(word);
Assert.AreEqual(1, label.Count);

Assert.IsTrue(label.Contains(word));

label.Remove(word);
Assert.IsFalse(label.Contains(word));
Assert.AreEqual(0, label.Count);
```
