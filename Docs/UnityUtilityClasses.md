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

#### 合わせて利用すると便利なクラス

- MonoBehaviourWithSubComponents<T>
- ModelBase<T, TValueKind>
- LabelsAttribute
- BindCallbackAttribute
- GameObjectPoolBase<TRootSubComponent, TOwner, TInstance>

Editor拡張
- SubComponent Summary

##### MonoBehaviourWithSubComponents<T>

SubComponentを使用する際に必要なものを持つComponentになります。
ルートとなるMonoBehaviourは基本的にこのクラスから派生することを推奨します。

MonoBehaviourWithSubComponents<T>はISubComponent<T>インターフェイスを継承していますので、ISubComponent関連の機能を利用することができます。

##### ModelBase<T, TValueKind>

Modelを実装する時に利用できる基底クラス。

以下の機能を持ちます。

- OnChangedValueコールバックとそれに関連する関数

ModelBase<>#OnChangedValueは以下のパラメータを持ちます。
- self: 値が変更されtモデルのインスタンス。
- ValueKind: 変更された値の種類を表すEnum。
- value: 変更された値。object型。
- prevValue: 変更された値の以前の値。object型。

```csharp
[System.Serializable]
public class Model : ModelBase<Model, Model.ValueKind>
{
    public enum ValueKind
    {
        Apple,
        Orange,
        Init,
    }

    [SerializeField, ModelFieldLabel((int)ValueKind.Apple)] int _apple = 1;
    [SerializeField, ModelFieldLabel((int)ValueKind.Orange)] float _orange = 1;

    public int Apple
    {
        get => _apple;
        //Propertyが変更された時にOnChangedValueを呼び出すのが基本的な使い方です。
        set => CallOnChangedValue(ref _apple, value, ValueKind.Apple, () => "Fail in set Apple prop...");
    }
    public float Orange
    {
        get => _orange;
        set => CallOnChangedNumberValue(ref _orange, value, ValueKind.Orange, () => "Fail in set Orange prop...");
    }

    public void Init()
    {
        _apple = 0;
        _orange = 0;
        //関数が呼びされた時に合わせてOnChangedValueを呼び出すこともできます。
        CallOnChangedValueDirect(ValueKind.Init, (_apple, _orange), null, () => "Fail in Init()...");
    }
}
```

##### LabelsAttribute

`SubComponentManager`には管理するSubComponentが持つ`LabelsAttribute`が指定したメソッドを一括に呼び出すことができる関数(CallSubComponentMethods(label))を持ちます。

また、LabelsAttribute#CallMethods()を直接呼び出すこともできますので用途に合わせて使い分けてください。

以下の用途に使用すると便利です。
- Modelが変更された時にViewへその変更を知らせる(Model -> View)
- Controllerなど何らかのイベントが発生した時に、関連する関数を呼び出す。(Controller -> Model or View)

```csharp
public class Scene : MonoBehaviourWithSubComponents<Scene>
{
    const string LABEL_METHOD = "Method";
    const string LABEL_METHOD2 = "Method2";

    [SerializeField] SubComponent _sub;
    [SerializeField] SubComponent2 _sub2;

    public void OnXXX()
    {
        //Call void XXX() Method in SubComponents -> _sub.Method(), _sub2.Method()
        SubComponentManager.CallSubComponentMethods(LABEL_METHOD);

        //Call int XXX(int) Method in SubComponents -> only call _sub#MethodWithArgs(int)
        var returnType = typeof(int);
        var isStatic = false;
        var label = LABEL_METHOD;
        var methods = SubComponentManager.SubComponents.SelectMany(_s =>
            _s.GetType().GetMethods(BindingFlags.Public | bindingFlags)
                .Where(_m => {
                    var attr = _m.GetCustomAttributes<LabelsAttribute>()
                        .FirstOrDefault(_a => _a.GetType().Equals(typeof(LabelsAttribute)));
                    if (attr == null) return false;
                    // Filtering With 'label'
                    return attr.DoMatch(Labels.MatchOp.Partial, label);
                })
                // below pass to Arguments And ReturnType!
                .CallMethods(isStatic? null : _s, typeof(void), 100, 200));
        foreach(var returnValue in methods)
        {
            //recive returnValue from Methods
            Debug.Log($"returnValue => {returnValue}");
        }
    }

    class SubComponent : ISubComponent<Scene>
    {
        [Labels(LABEL_METHOD)]
        public void Method()
        {
            //...
        }

        [Labels(LABEL_METHOD2)]
        public void Method2()
        {
            //...
        }

        [Labels(LABEL_METHOD)]
        public int MethodWithArgs(int n)
        {
            //...
            return n * 2; //...
        }
    }

    class SubComponent2 : ISubComponent<Scene>
    {
        [Labels(LABEL_METHOD)]
        public void Method()
        {
            //...
        }

        [Labels(LABEL_METHOD2)]
        public void Method2()
        {
            //...
        }
    }
}
```

##### 専用/関連するAttribute

- NotNullAttribute: 指定したFieldがNullの時に警告ログを出力するAttribute。SerializeされるFieldに指定してください。
- BindCallbackAttribute: ControllerのコールバックとModelのメソッドを関連付けたり、Viewの関数を一括に呼び出す時に利用しています。

#### GameObjectPoolBase<TRootSubComponent, TOwner, TInstance>

`MonoBehaviourWithSubComponents<TRootSubComponent>`に対応した`ObjectPool`実装の基底クラスになります。
`UnityComponentPool<TInstance>`を継承しています。

`ObjectPool`関連の実装が定義されているため、それらを使用したい場合はこちらのクラスを使用することで簡単に対応できます。


UnityComponentPool<TInstance>から追加で以下の関数を追加で定義しております。

- DestroyNotInstanceTypeInRoot() : Rootとなっている親GameObjectの子からTemplateとして指定されていないGameObjectを全て削除します。
- PushAllIcon() : Rootとなっている親GameObjectにあるTInstance型がアタッチされているGameObject全てをPoolにプッシュします。
- GetInstance() : TInstanceを生成またはPoolからPopし、GameObjectをActive、およびRootの子へ登録します。


#### SubComponent Summary

専用のEditor拡張(EditorWindow)になります。

Hinodeではシーン上にあるMonoBehaviourWithSubComponents<>を継承するComponentを持つGameObjectの情報を閲覧することができるEditorWindowも合わせて提供します。

このEditorWindowを開くには以下の手順を行ってください。
- Hinode > Tools > SubComponent Summary ボタンを押す

このEditorWindowでは以下の機能を持ちます。
- 選択したMonoBehaviourWithSubComponents<>が持つSubComponentの一覧
- SubComponentのメンバメソッドに指定されているLabelsAttributeの表示

### AnimationHub

Hinodeでは`UnityEngine.Animator`とその中のAnimationを管理するための`Hinode.AnimationHub`を提供しています。

`Hinode.AnimationHub`を利用することで、キー名を指定することで複数の`UnityEngine.Animator`のアニメーションを再生したり、Triggerを引くことが可能になります。

`Hinode.AnimationHub`にはキー名と以下の項目のリストを設定することができます。

- 指定したAnimatorのパラメータ(Int, Float, Bool, Trigger)
- 指定したAnimatorのGameObjectのActive
- 任意のUnityEvent(Animatorは指定する必要はありません。)


以下に、スクリプト上からの`AnimationHub`に設定したデータを実行する方法を示します。


```csharp
var hub = gameObject.GetComponent<AnimationHub>();

var key = "<key name>";
hub.FirePack(key); // <- fire animations
```

#### AnimationSequence

`Hinode.AnimationSequence`は`Hinode.AnimationHub`と一緒に使用されることを想定されたComponentです。

このComponentを利用することで`Hinode.AnimationHub`に設定されたAnimation情報を順番に実行することが可能です。

```csharp
var hub = gameObject.GetComponent<AnimationHub>();
var animationSequence = hub.Sequence;

animationSequence.NextStepAnimation(); // <- run next animations
```

また、`Hinode.AnimationSequence`は現在再生中のアニメーションの再生が終わるまで、次のアニメーションに移行させない機能(Lock機能)も提供しています。

Lock機能を使用する際は、再生が完了するのを監視したい`UnityEngine.Animator`のStateに`Hinode.AnimationHubBehaviour`も必ずアタッチして下さい。

### SceneManagerModel

`Hinode.SceneUtils.SceneManagerModel`を利用することで`UnityEngine.Scene`の管理をすることができます。

`Hinode.SceneUtils.SceneManagerModel`は、あるシーンをルートシーンとして配置して、`LoadSceneMode.Additive`としてシーンを追加する形を想定しています。
ルートシーンとして配置したシーンは`Hinode.SceneUtils.SceneManagerModel`では管理しないようにして下さい。

