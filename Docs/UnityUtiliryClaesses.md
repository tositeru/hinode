## Unityの便利クラス

### Attributes

HinodeではEditor拡張に対応した以下のAttributeを提供しています。

- ScenePathAttribute: BuildingSettingsに登録されているSceneへのパスを表すAttribute。string型のフィールドに指定してください。
- TextureFilepathAttribute: テクスチャへのファイルパスを指定するAttribute。string型のフィールドに指定してください。
- RangeIntAttribute: int型のフィールドのEditor拡張をSliderにするAttribute。
- RangeNumberAttribute: float/double型のフィールドのEditor拡張をSliderにするAttribute。

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

### SubComponent

HinodeではMonoBehaviourのヘルパークラスとして、ISubComponentインターフェイスを提供しています。

ISubComponentはMoneBahaviourの機能を分割することを助けることを目的に作成されています。

ISubComponentを使用する時はSubComponentManagerをフィールドに持つかまたはMonoBehaviourWithSubComponentsを継承することを推奨します。

ISubComponentは以下のインターフェイスを持ちます。

- Init() : 初期化を行う関数
- UpdateUI() : 全てのUI関係の更新を行う関数
- Destroy() : 破棄処理を行う関数

`UpdateUI`につきましては全パラメータを更新することを目的としていますので、パフォーマンス上の観点から負荷が高くなるかもしれません。
そのため、個々のパラメータが更新された時に対応した関数をISubComponentを継承したクラスに別途用意することも推奨します。


```csharp
class Test : MonoBehaviour
{
    MyModel _model = new MyModel();
    public MyModel Model
    {
        get => _model;
        set
        {
            _model = value;
            _subComponents.UpdateUI();
        }
    }

    SubComponentManager<Test> _subComponents;
    [SerializedField] SubComponent1 _sub1;
    [SerializedField] SubComponent2 _sub2;

    void Awake()
    {
        _subComponents = new SubComponentManager<Test>(this);
        _subComponents.Init();
    }

    void Start()
    {
        _subComponents.UpdateUI();
    }

    void OnDestroy()
    {
        _subComponents.Destroy();
    }

    [System.Serializable]
    class SubComponent1 : ISubComponent<Test>
    {
        public Test RootComponent { get; set; }
        public void Init() => ...;
        public void Destroy() => ...;
        public void UpdateUI() => ...;
    }

    [System.Serializable]
    class SubComponent2 : ISubComponent<Test>
    {
        public Test RootComponent { get; set; }
        public void Init() => ...;
        public void Destroy() => ...;
        public void UpdateUI() => ...;
    }
}
```

#### SubComponentの設計思想

ISubComponentを使用する際は以下の設計思想に基づくことを推奨します。
基本的にMVC的な思想に基づいています。

- ルートとなるMonoBehaviourの機能を分割したものをISubComponentとして実装すること
- ルートとなるMonoBehaviourは外部に公開するインナーフェイスとSubComponentをまとめるFacadeパターンとして作成すること
- ルートとなるMonoBehaviourのパラメータの内、外部から設定可能なものを抽出したModelクラスを定義すること
- そのModelの値が更新された時、ISubComponent#UpdateUIが呼び出されること

Modelの値が変更された時に合わせてSubComponentの値も合わせて変更するように実装することを推奨します。
