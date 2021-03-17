## Editor拡張

Hinodeには以下のEditor拡張を提供しています。

- [テキストテンプレートエンジン](./Editor/TextTemplateEngine.md)
- [Traitsサポーター](./Docs/Editor/TextTemplateEngine.md)
- [Screen Shot](#ScreenShot)
- [Test Class Linter](#TestClassLinter)

### Screen Shot

Hinodeは現在のシーンにあるCameraを使用してScreenShotを撮る機能を提供しています。

使用する際は以下の手順を行ってください。

- メニューの Hinode > Screen Shot Window をクリック
- 任意) 使用したいカメラを Use Camera に設定。設定しない場合は画面に表示されるものを撮影します。
- 開いたWindowにある "Start ScreenShot" を押し、EditorをPlayモードにします。
- 撮影したいタイミングで "Take Screen shot"をクリックすることでScreenshotを撮ることができます。
- 撮影を終えたい時は "Finish Screenshot" をクリックまたはPlayモードを終了してください。

撮影したScreen Shotは "Asset Path" に指定したFilepathで保存されます。(ルートディレクトリは"Assets/"になります。)


設定できるパラメータは以下のもになります。

- Use Camera: 使用するシーン内にあるCamera。設定しない場合は画面を撮影します。
- Asset Path: 保存先のファイルパス

Use Cameraが指定されていない時のみのパラメータ
- Super Size: 画面のサイズを元にした倍率。

Use Cameraが指定されている時のみのパラメータ
- Image Size: Use Cameraを指定した際に自動的に設定されるRenderTextureの解像度

### Test Class Linter

#### TODO パーサー実装後に実装予定になります。

Test Class LinterはTestClassにあるテスト関数の情報をまとめて、コメントとして出力するEditor拡張になります。

Testsディレクトリ内にあるTest用のcsファイルを対象に処理を行います。

使用法は以下のものになります。

- Linterを実行したいテストファイルを選択
- Assets > Hinode > Lint Test Class in selecting files をクリック
