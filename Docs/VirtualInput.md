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

### InputViewer

