# Semantic Kernel Bot in-a-box
![Banner](./readme_assets/banner.png)

このプロジェクトでは、拡張可能な Semantic Kernel ボット テンプレートを Azure にデプロイします。

## ソリューションアーキテクチャ

ソリューションのアーキテクチャを次の図に示します。

![ソリューションのアーキテクチャ](./readme_assets/architecture.png)

メッセージの流れは次のとおりです:

- エンド ユーザーは、ボットが公開されているメッセージング チャネル (Web、PowerBI ダッシュボード、Teams など) に接続します。
- メッセージは、App Services で実行されている .NET アプリケーションと通信する Azure Bot Services を介して処理されます。
- NETアプリケーションは、Semantic Kernel ステップワイズプランナーをコアで実行します。プランナーは、ユーザーの要求を処理するための一連の手順を詳しく説明し、それを実行します。
- 計画の各ステップは Azure OpenAI によって策定され、Cognitive Search (従来の RAG パターン) または Azure SQL (構造化データ RAG) に対して実行されます。
- Cognitive search にはホテルのインデックスが含まれ、Azure SQL には AdventureWorksLT サンプルの顧客データが含まれます。Azure OpenAI は、各質問のルーティング先のデータ ソースを決定する役割を担います。質問は、複数のデータソースにまたがる場合もあります。詳細については、「サンプル シナリオ」セクションを参照してください。


## 前提条件

- ローカルで実行する場合:
    - [.NET のインストール](https://dotnet.microsoft.com/en-us/download);
    - [Bot Framework Emulator のインストール](https://github.com/Microsoft/BotFramework-Emulator);

- Azure にデプロイする場合:
    - Azure CLI をインストール
    - Azure サブスクリプションにログイン

    ```
    az login
    ```

## Azure へのデプロイ

1. このリポジトリをローカルにクローンします。

```
git clone https://github.com/Azure/semantic-kernel-bot-in-a-box
```

2. 新しいリソース グループを作成する
3. 新しいマルチテナント アプリケーション登録を作成し、クライアント シークレットを追加する
4. `infra` ディレクトリで、`main.example.bicepparam` ファイルを探します。名前を `main.bicepparam` に変更し、生成したアプリ情報を下部に入力します。この手順では、Document Intelligence、Cognitive Search、Azure SQL の作成を無効にすることもできます。これらのリソースを無効にすると、アプリケーション上のそれぞれのプラグインも無効になることに注意してください。

5. リソースをデプロイします。
```
cd infra
az deployment group create --resource-group=YOUR_RG_NAME -f main.bicep --parameters main.bicepparam
```
この手順でエラーが発生した場合は、Azure CLI を更新してみてください。

6. Azure Cognitive Services インスタンスの Connect Hotels サンプル インデックスの展開。Cognitive Skillの追加などは行わず、デフォルト設定で完了させます。
![Cognitive Search ホーム](./readme_assets/cognitive-search-home.png)
![サンプルから Cognitive Search インデックスを作成する](./readme_assets/cognitive-search-index-sample.png)
7. ボット アプリケーションを App Services にデプロイします:
```
cd src
rm -r bin obj Archive.zip
zip -r Archive.zip ./* .deployment
az webapp deployment source config-zip --resource-group "YOUR_RG_NAME" --name "YOUR_APPSERVICES_NAME" --src "Archive.zip"
```

8. Web チャットでテストする - Azure portal で Azure Bot リソースに移動し、左側のメニューで Web チャット機能を探します。

![Webチャットのテスト](./readme_assets/webchat-test.ja.png)


## ローカルで実行する (最初にリソースを Azure にデプロイする必要があります)

デプロイ テンプレートを実行した後、開発とデバッグのためにアプリケーションをローカルで実行することもできます。

- `src` ディレクトリに移動し、`appsettings.example.json` ファイルを探します。名前を `appsettings.example.json` に変更し、必要なサービスの資格情報と URL を入力します
- プロジェクトを実行します。
```
    dotnet run
```
-  (オプション)Visual Studio Code でデバッグを行う場合には、SKBot.csを選択して `c#: SKBot` でデバッグを実行します。
- Bot Framework Emulator を開き、http://localhost:3987/api/messages に接続します
- ファイアウォールが制限されている可能性のあるサービスへのアクセスを有効にすることを忘れないでください。既定では、SQL Server はパブリック接続が無効になっています。

## サンプルシナリオ

このアプリケーションには、GPT-4自体、Cognitive Search、SQL、およびエンドユーザーがアップロードしたドキュメントからの情報を直接使用する機能があります。これらの各データ ソースには、いくつかのサンプル データがプリロードされていますが、接続をテンプレートとして使用して、独自のデータ ソースを接続できます。

各機能をテストするために、次のトピックについて質問できます

1. 利用可能な関数を確認する
    - 実装されている関数について尋ねる。
![利用可能な関数](./readme_assets/webchat-functions.ja.png)

2. 一般的な知識の質問
    - 公開されている知識について尋ねる。
![一般的な質問のシナリオ](./readme_assets/webchat-general.ja.png)

1. 検索拡張生成(SearchPlugin)
    - 説明に一致するホテルを探すように依頼します。
![検索拡張シナリオ](./readme_assets/webchat-search.ja.png)

1. 構造化データ取得 (SQLPlugin)
    - 顧客と売上について尋ねる。
![SQL接続シナリオ](./readme_assets/webchat-sql.ja.png)

1. ドキュメントをコンテキストとしてアップロードする (UploadPlugin)
    - ファイルをアップロードし、それについて質問します。
![アップロードシナリオ](./readme_assets/webchat-upload.ja.png)

1. Bing 検索を使用してインターネット上の情報を検索する (BingSearchPlugin)
    - インターネットで検索したい内容について質問します。
![Bing検索シナリオ](./readme_assets/webchat-bing.ja.png)

1. Semantic Kernel の Core Plugin ([TimePlugin](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/Plugins/Plugins.Core/TimePlugin.cs), [HttpPlugin](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/Plugins/Plugins.Core/HttpPlugin.cs))
    - 時間に関するタスク、HTTPのGETやPOSTなどに関するタスクの実行

**Note:** HTTPPlugin でWebサイトの情報取得すると大量のデータダウンロードされる恐れがあるので、対象サイトについては十分に注意してご利用ください

## 中間思考のデバッグ

このプロジェクトには、Semantic Kernel プランナーの中間ステップ(思考、アクション、観察など)を出力できるデバッグツールが組み込まれています。この機能は、DEBUG 環境変数を "true" に切り替えることで有効にできます。

![中間思考のデバッグ](./readme_assets/webchat-debug.ja.png)

## 独自のプラグインを開発する

このプロジェクトには 2 つのプラグインが付属しており、Plugins/ ディレクトリにあります。これらのプラグインは、独自のプラグインを開発する際の例として使用できます。

カスタムプラグインを作成するには:

- Pluginsディレクトリに新しいファイルを追加します。例の 1 つをテンプレートとして使用します。
- プラグインにコードを追加します。各セマンティック関数には、最上位の説明と各引数の説明が含まれており、Semantic Kernel がその機能を活用する方法を理解できるようにする必要があります。
- Bots/SKBot.cs ファイルにプラグインをロードします

これで完了です。アプリを再デプロイすると、Semantic Kernel は、ユーザーの質問でプラグインが必要になるたびにプラグインを使用するようになります。

## 貢献

このプロジェクトは、貢献と提案を歓迎します。 ほとんどのコントリビューションでは、コントリビューターライセンス契約(CLA)に同意して、コントリビューションを使用する権利があり、実際に付与することを宣言する必要があります。詳しくは https://cla.opensource.microsoft.com をご覧ください。

プルリクエストを送信すると、CLAボットはCLAを提供し、PRを適切に装飾する必要があるかどうかを自動的に判断します(ステータスチェック、コメントなど)。ボットの指示に従うだけです。これは、CLA を使用するすべてのリポジトリで 1 回だけ行う必要があります。

このプロジェクトでは、[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/)を採用しています。
詳細については、[行動規範に関するFAQ](https://opensource.microsoft.com/codeofconduct/faq/)または
その他の質問やコメントについては、[opencode@microsoft.com](mailto:opencode@microsoft.com)にお問い合わせください。

## 商標について

このプロジェクトには、プロジェクト、製品、またはサービスの商標またはロゴが含まれている場合があります。Microsoft の商標またはロゴの許可された使用は、[Microsoft の商標およびブランド ガイドライン](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general) に従う必要があります。
このプロジェクトの修正版での Microsoft の商標またはロゴの使用は、混乱を招いたり、Microsoft のスポンサーシップを暗示したりしてはなりません。
第三者の商標またはロゴの使用は、それらの第三者のポリシーの対象となります。