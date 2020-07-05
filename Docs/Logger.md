## セレクタ機能付きLogger

Hinodeには専用のLoggerを提供しています。

機能としては以下のものがあります。

- Log, Warning, Error別の出力のサポート
- Priority Levelによるログ出力の有無の制御
- キーワードによるログ出力のフィルタリング機能

### Priority Levelによるログ出力の有無の制御

`Logger.PriorityLevel`の値を変更することでログ出力を制御することができます。
`Logger.PriorityLevel`の値より大きいものは出力されないようになります。

`Logger.PriorityLevel`は以下の順に値が大きくなります。

- None
- High
- Middle
- Low
- Debug

`Logger.PriorityLevel`のDefault値は`Logger.Priority.High`になります。

```csharp
using Hinode;

Logger.PriorityLevel = Logger.Priority.Middle

Logger.Log(Logger.Priority.High, () => $"Output Log!"); // <- ログ出力
Logger.LogWarning(Logger.Priority.Middle, () => $"Output Warning!"); // <- Warning ログ出力
Logger.LogError(Logger.Priority.High, () => $"Output Error Log!"); // <- Error ログ出力

// Logger.PriorityLevel以下のものは出力されません。
Logger.Log(Logger.Priority.Lower, () => $"Output Log!"); // <- Not Output log
Logger.Log(Logger.Priority.Debug, () => $"Output Log!"); // <- Not Output log

```

### セレクタキーワードによるログ出力のフィルタリング機能

`Logger`にはセレクタによる出力制御機能もあります。

`Logger.AddSelecor(string keyword)`でセレクタとして使用するキーワードを追加し、
`Logger.RemoveSelecor(string keyword)`で設定されているキーワードを削除します。

現在のセレクタは`Logger.IsMatchSelectors(params string[] keywords)`で確認することができます。
また`Logger.Selectors`で現在の全てのセレクタを確認することができます。

```csharp
using Hinode;

Logger
    .AddSelector("Apple")
    .AddSelector("Orange");

Logger.Log(Logger.Priority.High, () => $"Output Log!", "Apple"); // <- ログ出力
Logger.LogWarning(Logger.Priority.Middle, () => $"Output Warning!", "Orange"); // <- Warning ログ出力
Logger.LogError(Logger.Priority.High, () => $"Output Error Log!", "Apple", "Orange"); // <- Error ログ出力

// Logger.Selectorsに含まれないものは出力されません。
Logger.Log(Logger.Priority.Lower, () => $"Output Log!", "Grape"); // <- Not Output log
Logger.Log(Logger.Priority.Debug, () => $"Output Log!", "Banana"); // <- Not Output log

```
