# Hinode Unity

このライブラリはUnity用の開発支援ライブラリとなります。

Unity 2019.2以降のバージョンに対応しています。

## ライセンス

Apache Licence Version 2.0

## インストール

UnityのPackage Managerを使用しGit URLからインストールできます。

## 機能

以下の機能を提供しています。

- [C#/Unity Componentsの拡張メソッド](./Docs/CSharpUnityExtensions.md)
- [セレクタ機能付きLogger](./Docs/Logger.md)
- [C#の便利クラス](./Docs/CSharpUtilityClasses.md)
    - [SmartDelegate](./Docs/CSharpUtilityClasses.md#SmartDelegate)
    - [ISingleton](./Docs/CSharpUtilityClasses.md#ISingleton)
    - [Type Referection Cache(RefCache)](./Docs/CSharpUtilityClasses.md#Type-Referection-CacheRefCahce)
    - [Value Update Observer](./Docs/CSharpUtilityClasses.md#Value-Update-Observer)
    - [Serializer](./Docs/CSharpUtilityClasses.md#serializer-namespace-hinodesizerialzation)
        - Json Serializer
    - [Text Resources](./Docs/CSharpUtilityClasses.md#Text-Resources)
    - [Collection Helper](./Docs/CSharpUtilityClasses.md#Collection-Helper)
    - [Math](./Docs/CSharpUtilityClasses.md#Math)
- [Unityの便利クラス](./Docs/UnityUtiliryClaesses.md)
    - [ChildObject](./Docs/UnityUtiliryClaesses.md#ChildObject)
    - [SceneObject](./Docs/UnityUtiliryClaesses.md#SceneObject)
    - [LabelObject](./Docs/UnityUtiliryClaesses.md#LabelObject)
    - [SubComponent](/Docs/UnityUtiliryClaesses.md#SubComponent)
- [Editor拡張](./Docs/Editor.md)
    - [テキストテンプレートエンジン](./Docs/Editor/TextTemplateEngine.md)
    - [Screen Shot](./Docs/Editor.md#ScreenShot)
- [仮想Input機能](./Docs/VirtualInput.md)
    - [ReplayableInput](./Docs/VirtualInput.md#ReplayableInput)
    - [InputRecorder](./Docs/VirtualInput.md#InputRecorder)
    - [InputViewer](./Docs/VirtualInput.md#InputViewer)
- [Test Tools/Library](./Docs/TestToolsAndLibrary.md)
    - [TestBase](./Docs/TestToolsAndLibrary.md#TestBase)
    - [Snapshot テスト](./Docs/TestToolsAndLibrary.md#Snapshot)
    - [UnityTest Attributeを指定されたテストのStep By Step実行機能](./Docs/TestToolsAndLibrary.md#UnityTestのStepByStep実行)
- [Layouts](./Docs/Layouts.md)
    - [設計目標](./Docs/Layouts.md#設計目標)
    - [基準領域](./Docs/Layouts.md#基準領域)
    - [ILayout](./Docs/Layouts.md#ILayout)
    - [ILayoutTarget](./Docs/Layouts.md#ILayoutTarget)
    - [LayoutInfo](./Docs/Layouts.md#LayoutInfo)
    - [LayoutManager](./Docs/Layouts.md#LayoutManager)
    - [組み込みのILayout](./Docs/Layouts.md#組み込みのILayout)
- [MVCフレームワーク](./Docs/MVCFramework.md)
    - HTMLとCSSのようなセレクタによるModel-Viewの関連付け機能
