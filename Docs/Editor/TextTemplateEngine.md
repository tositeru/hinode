## Text Template Engine

Text Template Engineはマクロのようなもので、元となるテキスト(Template)にパラメータを指定することでテキストを生成するEngineになります。

### TODO

- プロジェクト上のソースコードに埋め込まれているText Template Engineを調べ、表示するEditor拡張の作成

### 使い方

1. `Assets/Create/Hinode/Create TextTemplate`からTextTemplateEngineのScriptableObjectを作成してください。
1. Inspectorの'Template Text'にテンプレートとなるテキストを入力してください。
1. 生成したいテキストになるようにパラメータを設定してください。
1. Generateボタンを押すことで、テキストが生成されます。
1. 生成されたテキストは'Generated Text'に出力されます。
1. Copy To Clipboardボタンを押すことで'Generated Text'にあるテキストをコピーすることが可能です。
1. C#スクリプト上にUnityプロジェクトのAssets上にあるTextTemplateEngineを展開する書式を記述することで、ソースコード上にテキストを展開することも可能です。

### パラメータ

#### Template Text

元となるテキストのテンプレートになります。

「$〇〇$」と'$'で囲まれたキーワードをテキスト内に埋め込むことで、キーワードに対応したパラメータの値が展開されます。

また、他のテキストテンプレートのものを本文中に埋め込むことも可能です。
その際は「%〇〇%」と'%'で埋め込みたいテキストテンプレートのキーを囲み、'Embbed Templates'に埋め込んだキーを入力してください

```
$Key1$ is $Key2$
%Embbed%
```

#### Keywords

Template Textに埋め込まれたキーワードを設定する部分になります。

例として、以下のテンプレートの場合について見ていきます。

```
$Key1$ is $Key2$
```

このテンプレートには$Key1$と$Key2$の二つのKeywordが存在します。

その際にInspector上の'Keywords'配列の要素のKeyに'Key1'を指定し、`Values`に置き換えたいテキストを入力することでパラメータの設定が可能になります。
同じように'Key2'も設定します。

例えば、以下のように設定したとします。

Keywords
    - Element0
        - Key = Key1
        - Values
            - Tom
            - Kumi
    - Element1
        - Key = Key2
        - Values
            - Boy
            - Girl

この時Generateボタンを押すと以下のテキストが生成されます。

```
Tom is Boy
Tom is Girl
Kumi is Boy
Kumi is Girl
```

上の例からわかるようにTextTemplateEngineは設定されたKeywordsの全ての組み合わせを自動的に生成し、その全てをテンプレートに適応します。

もし、組み合わせたくないKeyの値のペアがある時はIgnore Pairsにそれらを設定してください。
また、Is Single Keyword Pair Modeを指定することで単一の組み合わせを指定することができます。

なお、組み合わせごとの末尾にはInspector上のNewlineに指定した改行文字が挿入されます。

それぞれの詳細は各項目を参照してください。

####Ignore Pairs

Inspector上のIgnore Pairsを設定することで自動的に組み合わされるKeywordsのペアから除外するものを指定することができます。

例として以下のようにTemplateとKeywordsを設定している時について見ていきます。

```
$Key1$ is $Key2$
```

Keywords
    - Element0
        - Key = Key1
        - Values
            - Tom
            - Kumi
    - Element1
        - Key = Key2
        - Values
            - Boy
            - Girl

この時にIgnore Pairsを以下のように設定します。

Ignore Pairs
    - Element0
        - Pairs
            - Tom
            - Girl
    - Element1
        - Pairs
            - Kumi
            - Boy


この状態の時にGenerateボタンを押すと以下のようなテキストが出力されます。

```
Tom is Boy
Kumi is Girl
```

また、Ignore Pairsを以下のように設定は、

Ignore Pairs
    - Element0
        - Pairs
            - Kumi

Kumiが含まれるKeywordsのペア全てが無視され以下のようなテキストが生成されます。

```
Tom is Boy
Tom is Girl
```

先のIgnore PairsのKumiをGirlに変更した時は以下のテキストが生成されます。

```
Tom is Boy
Kumi is Boy
```

このようにIgnore Pairsを利用することで生成されるテキストの内容を制御することが可能になります。

ただし、Keywordの組み合わせが膨大になり、Ignore Pairsの設定が難しくなる場合はIs Single Keyword Pair Modeを有効にすることで設定しやすくなるかもしれませんので、そちらを利用してください。

####Is Single Keyword Pair Mode

Inspector上のIs Single Keyword Pair Modeを有効にすると、Keywordの単一の組み合わせを指定することができます。

この項目を有効にするとIgnore PairsがSingle Keyword Pair Listに切り替わります。

例として以下のテンプレートを記述します。
```
$Key1$ is $Key2$
```

その時に以下のようにパラメータを設定します。

Keywords
    - Element0
        - Key = Key1
    - Element1
        - Key = Key2

Single Keyword Pair List
    - Element0
        - Pair Keywords
            - Tom
            - Boy
    - Element1
        - Pair Keywords
            - Kumi
            - Girl

注意点としてはKeywordsのKeyだけを指定しValuesに値を設定しないようにしてください。(Valuesは設定しても無視されます。)
また、Single Keyword Pair ListのPair Keywordsの並び順はKeywordsのElementsの並び順と一致します。

この状態でGenerateボタンを押すと以下のテキストが生成されます。

```
Tom is Boy
Kumi is Girl
```

KeywordsのValuesの数が多くIgnore Pairsを指定するのが手間になる場合にこのModeを使用してください。

####Embbed Templates

Template上に他のTextTemplateEngineを展開したい場合はこちらのパラメータを設定してください。
Text Template Engineではこの機能を埋め込みテンプレートと呼びます。

その際、Templateの本文内に「%〇〇%」と'%'で埋め込みたいテキストテンプレートのキーを囲み、'Embbed Templates'に設定したキーを入力してください


例として、以下のテンプレートの場合について見ていきます。

```
$Key1$ is $Key2$ %Embbed1%@
```

Keywords
    - Element0
        - Key = Key1
        - Values
            - Tom
            - Kumi
    - Element1
        - Key = Key2
        - Values
            - Boy
            - Girl

この時にInspector上のEmbbed Templatesを以下のように設定します。

Embbed Templates
    - Element0
        - Key = Embbed1
        - Template = OtherTemplate

この時、OtherTemplateの内容は以下のものとします。

```
$One$$Second$
```

Keywords
    - Element0
        - Key = One
        - Values
            - !
            - ?
    - Element1
        - Key = Second
        - Values
            - !
            - ?

初めのテンプレートに戻り、Generateボタンを押すと以下のようなテキストが出力されます。

```
Tom is Boy !!
?!
!?
??@
Kumi is Boy !!
?!
!?
??@
Tom is Girl !!
?!
!?
??@
Kumi is Girl !!
?!
!?
??@
```

%Embbed1%の部分にOtherTemplateの出力結果が展開されます。

この時、OtherTemplate内のKeywordの組み合わせが全て展開され末尾の改行文字も含まれていることに注意してください。

埋め込みテンプレートは以下のパラメータでその展開の挙動を制御することができます。

- Is Only Embbed: このText Template EngineのKeywordsを展開せず、埋め込みテンプレートのみを展開するかどうか？
- Do Share Keywords: このText Template Engineのパラメータを埋め込まれたテンプレートのパラメータとして使用するかどうか?


####Is Only Embbed

このText Template EngineのKeywordsを展開せず、埋め込みテンプレートのみを展開するかどうかの設定になります。

もし、これが有効なら、このText Template Engineに設定されたKeywordは展開されずに埋め込みテンプレートのみ展開されます。

他のText Template Engineのハブとして使用したい時に利用してください。

####Do Share Keywords

このText Template Engineのパラメータを埋め込まれたテンプレートのパラメータとして使用するかどうかの設定になります。

これが有効化されている場合に埋め込みテンプレートを展開する時は埋め込みテンプレートに設定されているパラメータは使用されず、このText Template Engineのパラメータが使用されます。

その時、使用されるKeywordsは埋め込みテンプレート側に存在するKeywordsのみになりますので、埋め込みテンプレート側のKeywordsの設定も忘れずに行ってください。

####Newline

一つのKeywordsの組み合わせを適応した時に末尾に挿入される改行文字を指定することができます。

### ソースコード上へ展開する

ソースコード上に次に説明する書式を埋め込むことでText Template Engineの出力をソースコード上に展開することができるようになります。

```csharp
class Apple
{
    ////@@ Assets/TextTemplateEngine.asset
}
```

Assets/TextTemplateEngine.assetの内容は以下のものとします。

```
$Type$ $Name$;
```

Keywords
    - Element0
        - Key = Type
        - Values
            - bool
            - int
    - Element1
        - Key = Name
        - Values
            - var1
            - var2

以下の手順でText Template Engineを展開することができます。
(複数のソースコードを一括で処理することも可能です。)

1. 展開したいソースコードを選択し右クリックします。
1. 表示されたコンテキストメニューの中から、Hinode/Expand TextTemplate in Selecting Script Filesをクリックします。
1. 選択されたソースコードを探索しText Template Engineがある場合はそれを展開します。

展開された時は以下のようなソースコードになります。

```
class Apple
{
    ////@@ Assets/TextTemplateEngine.asset
bool var1;
bool var2;
int var1;
int var2;
////-- Finish Assets/TextTemplateEngine.asset
}
```

この例ではコンパイルエラーとなるソースコードが生成されていますので、実際に使用する際はエラーにならないような出力結果になるようにしてください。
なお、indentの方は考慮されませんので注意してください。

#### 書式

Text Template Engineを展開したい部分に以下のような書式を記述してください。

```
////@@ [Asset Path]
```

- [Asset Path]: 展開したいText Template EngineのAsset Path

