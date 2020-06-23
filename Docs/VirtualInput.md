## 仮想Input機能

Hinodeには仮想Input機能を提供します。
この機能を利用することで以下のことができます。

- UnityEngine.Inputの入力データの代わりにスクリプト上から入力データを設定すること。
- UnityEngine.Inputの入力データの記録と再生。
- UnityEngine.Inputの入力データを画面に表示。

### ReplayableInput

`Hinode.ReplayableInput`を使用することで`UnityEngine.Input`の入力データをスクリプト上から上書きすることができます。

入力データを上書きしたい場合は`ReplayableInput.IsReplaying`をtrueに設定し、各`ReplayableInput.Recorded***`等に値を設定してください。

現状は以下のものをサポートしています。

- Mouse
- Touch
- Key
- UnityEngine.Input.Button***
- UnityEngine.Input.GetAxis (Not support GetAxisRaw().)

```csharp
var input = ReplayableInput.Instance;
input.IsReplaying = true;
input.RecordedMousePresent = true;

//MouseButtons
foreach (InputDefines.MouseButton btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)))
{
    var condition = InputDefines.ButtonCondition.Push;
    input.SetRecordedMouseButton(btn, condition);
    Assert.AreEqual(condition, input.GetRecordedMouseButton(btn));
    Assert.AreEqual(condition, input.GetMouseButton(btn));
}

{//MousePos
    input.RecordedMousePos = Vector3.one;
    Assert.AreEqual(input.RecordedMousePos, input.MousePos);
}

```

### InputRecorder

InputRecorderを使うことでReplayableInputの入力データを記録・再生を行うことができます。

以下のクラスを使用します。

- InputRecorder : ReplayableInputの記録・再生を行うためのComponent
- InputRecord : 入力データを表すScriptableObject
- InputRecorder.IFrameDataRecorder : 入力データのフレーム単位の処理を行うインターフェイス

使用するにあたっては、基本的に以下の手順を踏みます。

1. InputRecorderをシーン上に作成し、フレーム単位の処理を行うInputRecorder.IFrameDataRecorderをそれに設定します。
1. 記録対象または再生対象となるInputRecordをInputRecorderに登録
1. 記録・再生処理を行う

InputRecorder.IFrameDataRecorderのデフォルト実装として`FrameInputData`クラスを提供していますので、そちらを利用してください。
(FrameInputDataはInputRecorderに初期化に設定されています。)

```csharp
var recorderObj = new GameObject("__recorder",
    typeof(InputRecorder));
var recorder = recorderObj.GetComponent<InputRecorder>();

//入力データのインスタンス
var inputRecord = InputRecord.Create(new Vector2Int(Screen.width, Screen.height));

//入力データの記録・再生を行うクラス(InputRecorder.IFrameDataRecorder interface)
recorder.FrameDataRecorder = new DummyFrameDataRecorder();

{//Record Sample
    //Start Record
    recorder.StartRecord(inputRecord);

    //Stop Record
    recorder.StopRecord();

    //Sase Record Data To inputRecord
    recorder.SaveToTarget();
}

{//Replay Sample
    //Start Replay
    recorder.StartReplay(inputRecord);

    //Pause Replay
    recorder.PauseReplay();

    //Stop Replay
    recorder.StopReplay();
}
```

### InputViewer

