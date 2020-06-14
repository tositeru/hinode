## C#の便利クラス

Hinodeには以下のC#の便利クラスを提供しています。

- SmartDelegate
- ISingleton
- TypeListCache
- Type Referection Cache(RefCache)
- Value Update Observer
- Serializer
    - Json Serializer
- Text Resource

### SmartDelegate

`Hinode.SmartDelegate`はSystem.Delegateをラップしたものになり、delegateの追加、削除を簡単に行えるようになっています。

`Hinode.SmartDelegate`は`Hinode.NotInvokableDelegate`から派生しており、delegateへの追加、削除のみを行いたく、実行させたくない場合はそちらを使用してください。

### ISingleton

シングルトンパターンを提供するクラスになります。

### TypeListCache

現在読み込まれているAssemblyの中から
