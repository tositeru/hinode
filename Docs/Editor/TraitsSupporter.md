## Traitsサポーター

Traitsサポーターは他の言語のTraits的な機能をC#で使用することを手助けするEditor拡張機能になります。

実装上ではテキストを展開するだけなので、クラス階層には影響を与えませんので、注意して下さい。

また、言語構文のチェック機能はありませんので、Traitsを展開した後に構文エラーがでないように定義して下さい。

### 使い方

1. スクリプトまたはテキストファイルの中にTraitsとして使用したいコードを定義
1. 上で定義したTraitsを埋め込みたい部分をスクリプト上に記述
1. Traitsを埋め込みたいスクリプトをUnity Editor上で選択し、 コンテキストメニューを開き　Hinode > 'Assets/Hinode/Expand Traits in Selecting Script Files' をクリックすることで定義したTraitsを展開することができます。

### Traitsの定義の方法

Traitsを定義する時は以下のようにして下さい。

1. Traitsを定義する部分を`//// $<traits name>`と`//// End $<traits name>`で挟みます。この時、必ず`traits name`を一致するようにして下さい。


```csharp
//// $TraitsDefine
public int TraitsProp { get; set; }
//// End $TraitsDefine
```

### Traitsを展開したい部分の指定の方法

Traitsを展開したい場合は以下のようにして下さい。

1. Traitsを展開したい部分に`////@@ traits $<traits name>`か、または`////@@ traits <filepath> $<traits name>`を記述して下さい。

`filepath`を省略した場合はTraitsを展開したいスクリプトファイルの中から`traits name`と名付けられたTraitsを探します。
`filepath`は通常のファイルパスとUnityのAssetPathの両方を指定することができます。

```csharp
class Traits {
    //// $TraitsDefine
    public int TraitsProp { get; set; }
    //// End $TraitsDefine
}

class A {
    public int Value { get; set; }
    ////@@ traits $TraitsDefine
    ////@@ traits ./TraitsDefine.cs $TraitsDefine2
}
```

展開された後のスクリプトは以下のようになります。

`////-- Finish traits $<traits> name` などの部分は処理場必要になるので、スクリプト上から削除しないよう注意して下さい。

```csharp
class A {
    public int Value { get; set; }
    ////@@ traits $TraitsDefine
    public int TraitsProp { get; set; }
    ////-- Finish traits $TraitsDefine
    ////@@ traits ./TraitsDefine.cs $TraitsDefine2
    // ... 他のファイルに定義したTraitsがここに展開される
    ////-- Finish traits ./TraitsDefine.cs $TraitsDefine2
}
```

### TraitsSupporter

`Hinode.Editors.TraitsSupporter`クラスを使用することでTraitsサポーターの機能をスクリプト上から利用することができます。

`TraitsSupporter#TraitsTemplates`に埋め込みたいTraitsを設定することができます。
`TraitsSupporter#TraitsTemplates`に設定されたTraitsは`filepath`を指定していた場合でも最優先で展開されますので注意して下さい。


```csharp
var traitsSupporter = new TraitsSupporter()
{
    TraitsTemplates = new Dictionary<string, string>() {
        { "Traits", traitsText },
    }
};

var text = @" ... text ... ";
var result = traitsSupporter.Expand(text);
var result2 = traitsSupporter.ExpandInFile("./filepath.cs");

```
