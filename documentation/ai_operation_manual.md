# AI向け操作マニュアル

このドキュメントは、AIツールまたは外部スクリプトが WcmScheduler のプロジェクトファイルを編集するための作業手順をまとめたものです。

AIはGUI操作ではなく、保存データJSONを読み書きすることでプロジェクトの内容を理解し、タスク追加、更新、依存関係整理を行います。

## 基本方針

WcmScheduler の保存ファイルは、人間がGUIで操作し、AIがJSONとして読み書きできる共有作業場です。

AIが編集する場合は、以下を守ります。

- 正式な保存ファイルを直接編集しない。
- 編集作業ファイルを経由する。
- 保存前にJSONとして検証する。
- `documentation/save_data.md` と `documentation/save_data.schema.json` を参照する。
- JSONキーは kebab-case を使用する。
- JSONは UTF-8、2スペースインデントで保存する。
- 日本語文字列を `\uXXXX` 形式へエスケープしない。

## 参照する公開仕様

作業前に、必要に応じて以下を参照します。

| ドキュメント | 用途 |
| --- | --- |
| [保存データ仕様](save_data.md) | JSON構造、編集作業ファイル、ダーティ判定 |
| [保存データJSON Schema](save_data.schema.json) | 保存データの機械検証 |
| [タスクエディタ仕様](task_editor_spec.md) | タスク、ノード、接続線、ステータス |
| [メンバー管理仕様](member_manager.md) | メンバー、担当者、作業協力者、作業可能時間 |
| [ガントチャート仕様](gantt_chart.md) | 予定期間算定、依存関係表示、フィルタ |
| [ステータスバー仕様](status_bar.md) | 保存状態、処理状態、進捗表示 |

## 編集対象ファイル

正式な保存ファイルが `project.json` の場合、AIは以下のファイルを使用します。

```text
project.json
project.edit.json
project.backup.json
project.backup.1.json
```

| ファイル | 役割 |
| --- | --- |
| `project.json` | 正式な保存ファイル |
| `project.edit.json` | 編集作業ファイル |
| `project.backup.json` | 直前バックアップ |
| `project.backup.N.json` | 世代バックアップ |

## 編集手順

AIが保存ファイルを編集する場合は、以下の手順を基本とします。

1. 正式な保存ファイルを読み込む。
2. 正式な保存ファイルをバックアップする。
3. 正式な保存ファイルを編集作業ファイルへコピーする。
4. 編集作業ファイルを変更する。
5. 編集作業ファイルをJSONとして検証する。
6. 必要に応じて保存データJSON Schemaで検証する。
7. 編集作業ファイルを正式な保存ファイルへ反映する。

### PowerShell例

`MainApplication/test/test_project.json` を編集する場合の例です。

```powershell
Copy-Item -LiteralPath MainApplication/test/test_project.json -Destination MainApplication/test/test_project.backup.json -Force
Copy-Item -LiteralPath MainApplication/test/test_project.json -Destination MainApplication/test/test_project.edit.json -Force
```

編集後は、JSONとして読み込めることを確認します。

```powershell
Get-Content -Raw -Encoding UTF8 MainApplication/test/test_project.edit.json | ConvertFrom-Json | Out-Null
```

問題がなければ正式ファイルへ反映します。

```powershell
Copy-Item -LiteralPath MainApplication/test/test_project.edit.json -Destination MainApplication/test/test_project.json -Force
```

## JSON編集ルール

### 文字コードと整形

- UTF-8で保存する。
- インデントは2スペースにする。
- 日本語文字列はそのまま保存する。
- 改行はリポジトリの基本方針に合わせる。

### ID

保存データ内のIDは、既存データとの重複がない値を使用します。
タスク、ポート、接続、メンバー、プロジェクトは、それぞれ一意なIDを持ちます。

新規タスクを追加する場合は、少なくとも以下のIDを用意します。

- タスクノードID
- InputポートID
- OutputポートID

接続線を追加する場合は、接続IDを用意します。

### タスクノード

タスクは `projects[].task-editor.nodes[]` に追加します。

タスクノードの基本構造は以下です。

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "type": "TaskNode",
  "position": {
    "x": 0,
    "y": 0
  },
  "details": {
    "task-name": "Task",
    "person": null,
    "assignee-member-id": null,
    "collaborator-member-ids": [],
    "start-date-time": null,
    "end-date-time": null,
    "work-estimate-minutes": 0,
    "suspension-periods": [],
    "comment": ""
  },
  "ports": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "name": "Input",
      "type": "Input"
    },
    {
      "id": "00000000-0000-0000-0000-000000000002",
      "name": "Output",
      "type": "Output"
    }
  ]
}
```

### 接続線

接続線は `projects[].task-editor.connections[]` に追加します。

`from-port-id` には前段タスクの `Output` ポートIDを指定します。
`to-port-id` には後段タスクの `Input` ポートIDを指定します。

```json
{
  "id": "00000000-0000-0000-0000-000000000003",
  "from-port-id": "00000000-0000-0000-0000-000000000002",
  "to-port-id": "00000000-0000-0000-0000-000000000004"
}
```

## タスク作成指針

AIがタスクを作成する場合は、以下を意識します。

- 1タスクは、成果物または確認可能な作業単位にする。
- タスク名は人間が読んで分かる短い名前にする。
- 詳細な意図や完了条件は `comment` に書く。
- 前後関係がある場合は接続線で表現する。
- 担当者が決まっていない場合は `assignee-member-id` を `null` にする。
- 作業見積が未定の場合は `work-estimate-minutes` を `0` または `null` 相当として扱う。

## タスクステータス

タスクステータスは保存データへ直接保存しません。
アプリケーションがタスク詳細、依存関係、中断期間、現在日時から自動算出します。

ステータスを意図的に変えたい場合は、以下の入力値を変更します。

| 目的 | 編集対象 |
| --- | --- |
| 着手済みにする | `start-date-time` を設定する |
| 完了にする | `end-date-time` を現在日時以前に設定する |
| 未着手に戻す | `start-date-time` と `end-date-time` を `null` にする |
| 作業不可期間を表す | `suspension-periods` を設定する |

## 作業開始時の更新

AIがタスクに着手する場合は、対象タスクの `start-date-time` を作業開始日時へ更新します。

例:

```json
"start-date-time": "2026-05-10T21:30:00"
```

作業が完了した場合は、`end-date-time` を設定し、`comment` に完了内容を残します。

## 担当者と作業協力者

担当者と作業協力者は、`members[].member-id` を参照します。

担当者は `details.assignee-member-id` に設定します。
作業協力者は `details.collaborator-member-ids` に配列で設定します。

担当者と作業協力者に同じメンバーを重複設定しないでください。

## 依存関係整理

依存関係を追加する場合は、以下を確認します。

1. 接続元タスクの `Output` ポートIDを確認する。
2. 接続先タスクの `Input` ポートIDを確認する。
3. `connections[]` に接続線を追加する。
4. 循環依存が発生しないことを確認する。

循環依存があると、依存順の並び替えやガントチャート算定が不自然になる可能性があります。

## 座標調整

依存関係編集上の表示位置は `position.x` と `position.y` で管理します。

AIが大量のタスクを追加する場合は、以下を目安に配置します。

- 前段タスクを左側、後段タスクを右側へ配置する。
- 同じ階層のタスクは縦方向に並べる。
- 接続線が極端に重ならないように余白を取る。
- 既存ノードの座標を不要に変更しない。

画面外の座標も保存できます。
読み込み時にノード位置は自動クリップしないため、広い配置を使って依存関係を整理できます。

## ガントチャートへの影響

ガントチャートは、以下をもとに予定期間を算定します。

- 開始日時
- 終了日時
- 作業見積時間
- 依存関係
- 担当者
- 担当者の作業可能時間
- 中断期間
- 特別休日
- プロジェクト参加期間

作業見積時間が未設定または0分の場合、タスクバーは描画されない場合があります。
ただし、タスク行は表示対象になります。

## 保存前チェック

保存ファイルへ反映する前に、以下を確認します。

- JSONとして読み込める。
- `projects`、`members`、`special-holidays` が存在する。
- 新規IDが既存IDと重複していない。
- 接続線の `from-port-id` と `to-port-id` が存在する。
- 担当者IDと作業協力者IDが `members` に存在する。
- 担当者と作業協力者が重複していない。
- 日時文字列がISO形式である。
- `work-estimate-minutes` と作業可能時間が分単位の数値である。

## 禁止事項

AI編集では以下を避けます。

- 正式な保存ファイルをバックアップなしで直接編集する。
- 既存IDを理由なく変更する。
- 既存ノード座標を一括で並べ替える。
- 人間が調整したタスク名、コメント、座標を不要に書き換える。
- JSON文字列を `\uXXXX` へ不要にエスケープする。
- 保存データ仕様にないキーを追加する。

## 作業報告

AIが保存データを編集した場合は、作業後に以下を報告します。

- 変更した保存ファイル
- 追加、更新、削除したタスク
- 変更した依存関係
- 検証結果
- 残した注意点

報告例:

```text
MainApplication/test/test_project.json を更新しました。
新規タスクを3件追加し、既存タスク2件へ依存関係を追加しました。
JSONとして読み込み可能であることを確認済みです。
```

