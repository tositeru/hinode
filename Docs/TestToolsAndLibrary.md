## Test Tools/Library

Hinodeにはユニットテスト用の便利クラスやツールを提供しています。

これらの機能はUnityEditor上でのみ使用することができます。プロジェクトのビルドには含まれませんので注意してください。

- TestBase
- Snapshot
- UnityText属性を指定されたテストのStep By Step実行機能

### TestBase

TestBaseクラスはユニットテスト用のクラスのベースクラスとして使用するクラスになります。

派生クラスには以下の機能を提供します。

- Snapshot Test
- GetAssetsPathForTest(string ): テスト用のファイル・ディレクトリ用のファイルパスの取得
- ReserveDeleteAssets(paras string[]): テスト用のファイル・ディレクトリの自動削除


### Snapshot

TestBaseを継承したテストクラスはSnapshotテストを行うことができます。

Snapshotテストは[UnityTest]を指定されたユニットテストのみ実行でき、行う際はTestBase#TakeOrValid(...)またはTestBase#TakeOrValidWithCaptureScreen(...)を呼び出してください。

```csharp
[UnityTest]
public IEnumerator SnapshotPasses()
{
    var stackFrame = new StackFrame();
    string data = "test";
    int snapshotInnerNo = 0; // <- 同一テスト中に複数のSnapshotを撮る際の識別用の番号
    System.Func<string, string, bool> validateFunc = (string correct, string got) => {
        return correct == got;
    };
    var snapshot = TakeOrValid(data, stackFrame, snapshotInnerNo, validateFunc, "ErrorMssage!");
}
```

SnapshotテストのデータはプロジェクトにAssets/Snapshots/以下に自動的に生成されます。
(Packageの場合はそのPackageのルートディレクトリにSnapshotsディレクトリが生成されます。)

#### Snapshotのモード

Snapshotには以下のモードがあります。

1. 作成モード: 検証モード時に使用する正しいデータを持つSnapshotを作成するモード
1. 検証モード: 実際テストを行うモード

作成モードで正しいデータを持つSnapshotがない場合に検証モードを実行した場合エラーが発生しますので注意してください。

モードの切り替えには以下の手順を行ってください。

1. MenuのEdit > Project Settingsボタンを押し、Project Settings Windowを開きます。
1. Hinode Snapshot Testの項目を押し、
1. Do Take SnapshotのCheckBoxをクリックすることでモードを切り替えられます。

切り替えは全てのテストに影響を与えるため注意して切り替えてください。

#### Screenshot付きSnapshot

TestBase#TakeOrValidWithCaptureScreen(...)を使用することでSnapshot時の画面のスクリーンショットも撮影することができます。

撮影されたスクリーンショットはSnapshotの生成にはSnapshotと同じパスに保存されます。
Snapshotテスト時にはProjectのルートディレクトリにSnapshotScreenshotsディレクトリが作成され、その中に保存されます。

```csharp
[UnityTest]
public IEnumerator SnapshotWithScreenshotPasses()
{
    var stackFrame = new StackFrame();
    string data = "test";
    int snapshotInnerNo = 0; // <- 同一テスト中に複数のSnapshotを撮る際の識別用の番号
    System.Func<string, string, bool> validateFunc = (string correct, string got) => {
        return correct == got;
    };
    var enumerator = TakeOrValidWithCaptureScreen(data, stackFrame, snapshotInnerNo, validateFunc, "ErrorMssage!");
    while(enumerator.MoveNext()) {
        yield return enumerator.Current;
    }
    var snapshot = LastSnapshot;
}
```

### UnityTestのStepByStep実行

Hinodeでは[UnityTest]を指定されたテストをそのテストのStep By Step実行が行えます。

Step By Step実行時の中断のタイミングはその関数の内のyieldが呼び出された個所になります。

StepByStep実行を行いたい時は以下の手順を行ってください。
(Snapshotを撮っている場合はSnapshotのInspectorからでもStepByStep実行が行えます。)

1. MenuのHinode > Tools > StepByStep Test Runnerボタンを押し、Editor拡張Windowを開く
1. 実行したいテストのクラスと関数を指定する
1. Start Testボタンを押すことでテストを実行します。

実行時にはシーンにRunTestStepByStepコンポーネントがアタッチされた追加のGameObjectが生成されます。

実行中の制御はRunTestStepByStepコンポーネントのInspectorから行ってください。




