## Layouts

HinodeはUnityEngine.UIのような自前のLayout Systemを提供します。

`ILayout`に設定された`Hinode.Layouts.ILayoutTarget`を対象に動作するようになっています。

実装上、各`ILayout`は単一でも動作するようになっていますが、基本的に`LayoutManager`を経由してLayout計算を行うようにしてください。

`LayoutManager`は[設計目標](#設計目標)に示した項目を満たすように処理を行います。

### 設計目標

Layouts Systemの設計としては以下のものを目標に作られています。

1. 親の基準領域に収まるように変形する。(親の基準領域からはみ出さない)
1. 単一フレームでLayoutが決定する。
1. 基準領域が求まったら、親子階層の影響を受けずにLayoutを決定できるようにする。
1. 一度Layoutを計算したら、各`ILayout`の更新タイミングを満たさない限り、再度Layout計算は行わない。

もし、Layoutsに新しいものを追加・拡張したい場合は以上の項目を満たすように実装してください。

### 基準領域

Hinode.LayoutsにおいてLayoutを計算する際は各Layoutの基準領域を計算するように設計されます。

基準領域はLayout計算を行う上で、各オブジェクトの領域の目安となるものになります。

Layout計算を行う際には親の基準領域からはみ出さないように計算されるようになっています。

実際の領域ではなく基準領域を使用するようになっている理由としては、
`ILayout`によって、他のオブジェクト(子オブジェクトや親オブジェクトなど)の個数やサイズに応じて処理を行うように実装されるものがあり、
そのような`ILayout`の場合、Layout計算を行うと別の`ILayout`の領域も同時に変更する必要が生じ、それが連鎖的に他のものでも発生しLayout計算の無限ループが発生する可能性があるかです。

そのようなループを防止するために仮想的な領域(基準領域)を先に決定し、それを元に計算を行うようにしています。

もし、基準領域を決定できない`ILayout`がある場合はそれを利用する`Iayout`のLayout計算を行わないようなっています。

以上の理由から機能を拡張したい場合は必ず基準領域を定義して実装してください。(その他の原則は[設計目標](#設計目標)の方に明記しています。)

### ILayout

`ILayout`はLayoutを表すinterfaceになります。

`ILayout`は設定された`ILayoutTarget`を対象にLayout計算を行うようになっています。


### ILayoutTarget

`ILayoutTarget`は`ILayout`の処理対象となるオブジェクトを表すinterfaceになります。

基本的に`ILayout`はこのinterfaceを経由してLayout計算を行います。

以下の種類が標準に提供されています。

- LayoutTarget2D: 2Dオブジェクトを表す`ILayoutTarget`.
- LayoutTarget3D: 2Dオブジェクトを表す`ILayoutTarget`.


### LayoutManager

`LayoutManager`はシーン上に存在する`ILayout`を管理するクラスになります。

デフォルトでは`ILayout`が作成された際に`LayoutManager`が存在しない場合は自動的に`LayoutManager`を作成するようになっています。

各フレームの描画前に全`ILayout`のLayout計算を行います。

### 組み込みのILayout

Hinode.Layoutsは以下の`ILayout`を提供しています。

- AnchorBasedFillLayoutGroup
- AnchorBasedFillGridLayoutGroup
- ExpandedLayoutGroup
- AspectSizeFitter

