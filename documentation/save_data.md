# 保存データ仕様

このドキュメントは、WcmScheduler の保存ファイルJSON構造について整理したものです。
JSONキーは kebab-case を基本とします。
JSONファイルは UTF-8 で保存します。
整形時のインデントは2スペースとし、日本語文字列は `\uXXXX` 形式へエスケープせず、そのまま保存します。

JSON Schema は以下に定義します。

- [保存データJSON Schema](save_data.schema.json)

## 概要

保存ファイルは、プロジェクト一覧、チームメンバー一覧、特別休日一覧を1つのJSONとして保持します。

チームメンバーはチーム全体のマスタ情報として `members` に保存します。
タスク編集データ、メンバー別のプロジェクト参加期間、メンバー別の日付単位作業可能時間は、プロジェクト固有の情報として `projects` の各要素に保存します。

## 編集作業ファイル

保存ファイルをGUI、AIツール、外部スクリプトから編集する場合は、正式な保存ファイルを直接編集せず、編集作業ファイルを経由します。

正式な保存ファイルが `project.json` の場合、編集作業ファイル名は `project.edit.json` とします。

編集開始時は、正式な保存ファイルを編集作業ファイルへコピーします。
編集操作中は、編集作業ファイルへ自動保存します。
保存確定時は、正式な保存ファイルをバックアップファイルへリネームしてから、編集作業ファイルを正式な保存ファイル名へリネームします。

正式な保存ファイルが `project.json` の場合、直前バックアップファイル名は `project.backup.json` とします。

```text
project.json
project.edit.json
project.backup.json
```

AIツールまたは外部スクリプトが保存データを編集する場合は、以下の手順を基本とします。

1. 正式な保存ファイルを読み込む。
2. 正式な保存ファイルを編集作業ファイルへコピーする。
3. 編集作業ファイルを変更する。
4. 編集作業ファイルをJSONとして検証する。
5. 保存確定時に正式な保存ファイルをバックアップファイルへリネームする。
6. 編集作業ファイルを正式な保存ファイル名へリネームする。

ダーティ判定は、最後に正式保存した状態から編集が発生したかどうかを表します。
編集作業ファイルへの自動保存は、クラッシュ復旧用の保存であり、正式保存とは扱いません。

### ダーティ判定

ダーティ判定の対象は、プロジェクト保存ファイルに保存される全データです。
保存データに影響しない画面表示状態、選択状態、一時的なUI状態はダーティ判定対象外です。

ダーティ判定は、正式な保存ファイルと編集作業ファイルの内容比較によって行います。
比較時は保存データモデルへ読み込んだ結果を比較します。
JSONのキー順、インデント、改行、日時表現などの正規化は行いません。

正式な保存ファイルと編集作業ファイルが同一内容の場合は非ダーティ状態です。
正式な保存ファイルと編集作業ファイルが異なる場合はダーティ状態です。

ダーティ化するタイミングは、編集履歴へ変更が登録されるタイミングとします。
編集履歴を持たないUIでは、同種の編集が履歴へ反映されるタイミングに合わせます。
ボタン操作は押下時にダーティ判定を更新します。
テキスト入力は時間判定による確定時、またはフォーカスアウト時にダーティ判定を更新します。

UndoまたはRedoによって正式な保存ファイルと編集作業ファイルが同一内容へ戻った場合は、非ダーティ状態へ戻します。
編集作業ファイルへの自動保存だけでは、ダーティ状態は解除しません。
正式保存、名前を付けて保存、Undo、Redo、正式ファイルの変更によって正式な保存ファイルと編集作業ファイルが同一内容になった場合に、非ダーティ状態となります。

### 起動時の編集作業ファイル検出

起動時または保存ファイル読み込み時に編集作業ファイルが存在する場合は、ユーザーへ復元確認ダイアログを表示します。

復元確認ダイアログでは、正式な保存ファイルと編集作業ファイルのタイムスタンプを表示します。
ユーザーは編集作業ファイルを復元するか、破棄して正式な保存ファイルから編集作業ファイルを再生成するかを選択します。

編集作業ファイルが壊れている場合は、エラーダイアログを表示します。
その後、壊れている編集作業ファイルを破棄し、正式な保存ファイルから編集作業ファイルを再生成します。

### 終了時の確認

アプリケーション終了時にダーティ状態の場合は、未保存確認ダイアログを表示します。
選択肢は、保存する、保存しない、キャンセルとします。

保存するを選択した場合は、保存確定処理を行ってから終了します。
保存しないを選択した場合は、編集作業ファイルを破棄して終了します。
キャンセルを選択した場合は、終了処理を中止します。

アプリケーション終了時に非ダーティ状態の場合は、未保存確認ダイアログを表示せず、編集作業ファイルを自動削除します。

### 保存失敗時の扱い

保存確定処理で、正式な保存ファイルのバックアップ作成後に正式ファイルへのリネームへ失敗した場合は、エラーダイアログを表示します。
自動復旧は行わず、ユーザーへバックアップファイルから復旧するよう促します。

バックアップファイルは、ユーザー設定で指定した世代数まで保持します。
バックアップ世代数はアプリ設定ファイルに保存します。

### 外部更新と競合

アプリケーション起動中に編集作業ファイルが外部更新された場合は、再読み込み確認ダイアログを表示します。
ツール側編集と外部編集が競合する場合は、ツール側編集を採用するか、外部編集を採用するかをユーザーが選択します。

### UI表示

ダーティ状態の場合は、ウィンドウタイトルまたはタブ名に `*` を表示します。
保存ボタンは、非ダーティ状態でも有効とします。
非ダーティ状態で保存した場合は、内容を変更せずに正式な保存ファイルのタイムスタンプのみを更新します。

## ルート

ルートオブジェクトは以下の項目を持ちます。

| キー | 型 | 内容 |
| --- | --- | --- |
| `projects` | array | プロジェクト一覧 |
| `members` | array | チームメンバー一覧 |
| `special-holidays` | array | 特別休日一覧 |

```json
{
  "projects": [],
  "members": [],
  "special-holidays": []
}
```

## プロジェクト

`projects` の各要素は、プロジェクト1件分のデータを表します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `project-id` | string | プロジェクトID |
| `project-name` | string/null | プロジェクト名 |
| `task-editor` | object | タスク編集データ |
| `member-info` | array | プロジェクト内メンバー情報一覧 |

`gantt-chart` はガントチャート機能実装時に、プロジェクト配下へ追加する想定です。

```json
{
  "project-id": "00000000-0000-0000-0000-000000000000",
  "project-name": "New Project",
  "task-editor": {},
  "member-info": []
}
```

## タスク編集データ

`task-editor` はタスクノードと接続線を保存します。
対象プロジェクトは親要素の `project-id` によって識別します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `nodes` | array | ノード一覧 |
| `connections` | array | ノード間接続一覧 |

```json
{
  "nodes": [],
  "connections": []
}
```

## ノード

`nodes` の各要素は、タスクノード1件を表します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `id` | string | ノードID |
| `type` | string | ノード種別。現状は `TaskNode` |
| `position` | object | ノード座標 |
| `details` | object | タスク詳細 |
| `ports` | array | 入出力ポート一覧 |

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "type": "TaskNode",
  "position": {
    "x": 0,
    "y": 0
  },
  "details": {},
  "ports": []
}
```

## 座標

`position` はノードのキャンバス上の位置を表します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `x` | number | X座標 |
| `y` | number | Y座標 |

## タスク詳細

`details` はタスクの詳細情報を保持します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `task-name` | string/null | タスク名 |
| `person` | string/null | 旧形式の担当者名。読み込み時の互換用途 |
| `assignee-member-id` | string/null | 担当者メンバーID |
| `collaborator-member-ids` | array | 作業協力者メンバーID一覧 |
| `start-date-time` | string/null | 開始日時 |
| `end-date-time` | string/null | 終了日時 |
| `work-estimate-minutes` | number/null | 作業見積時間。単位は分 |
| `suspension-periods` | array | 中断期間一覧 |
| `comment` | string/null | コメント |

日時は `System.Text.Json` の標準形式で保存します。

```json
{
  "task-name": "Task",
  "person": null,
  "assignee-member-id": "00000000-0000-0000-0000-000000000000",
  "collaborator-member-ids": [],
  "start-date-time": "2026-04-28T09:00:00",
  "end-date-time": "2026-04-28T18:00:00",
  "work-estimate-minutes": 480,
  "suspension-periods": [],
  "comment": ""
}
```

## 中断期間

`suspension-periods` の各要素は、タスクの中断期間1件を表します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `start-date-time` | string/null | 中断開始日時 |
| `end-date-time` | string/null | 中断終了日時 |

```json
{
  "start-date-time": "2026-04-28T12:00:00",
  "end-date-time": "2026-04-28T13:00:00"
}
```

## ポート

`ports` の各要素は、ノードが持つ入出力ポート1件を表します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `id` | string | ポートID |
| `name` | string | ポート名 |
| `type` | string | ポート種別 |

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "name": "Input",
  "type": "Input"
}
```

## 接続

`connections` の各要素は、ポート間接続1件を表します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `id` | string | 接続ID |
| `from-port-id` | string | 接続元ポートID |
| `to-port-id` | string | 接続先ポートID |

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "from-port-id": "00000000-0000-0000-0000-000000000001",
  "to-port-id": "00000000-0000-0000-0000-000000000002"
}
```

## メンバー

`members` の各要素は、チームメンバー1件を表します。
配列順序はUI上の表示順として保存します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `member-id` | string | メンバーID |
| `display-name` | string/null | 表示名 |
| `sunday-work-time-minutes` | number | 日曜日のデフォルト作業可能時間 |
| `monday-work-time-minutes` | number | 月曜日のデフォルト作業可能時間 |
| `tuesday-work-time-minutes` | number | 火曜日のデフォルト作業可能時間 |
| `wednesday-work-time-minutes` | number | 水曜日のデフォルト作業可能時間 |
| `thursday-work-time-minutes` | number | 木曜日のデフォルト作業可能時間 |
| `friday-work-time-minutes` | number | 金曜日のデフォルト作業可能時間 |
| `saturday-work-time-minutes` | number | 土曜日のデフォルト作業可能時間 |
| `special-holiday-work-time-minutes` | number | 特別休日のデフォルト作業可能時間 |

作業可能時間の単位は分です。

```json
{
  "member-id": "00000000-0000-0000-0000-000000000000",
  "display-name": "Alex",
  "sunday-work-time-minutes": 0,
  "monday-work-time-minutes": 480,
  "tuesday-work-time-minutes": 480,
  "wednesday-work-time-minutes": 480,
  "thursday-work-time-minutes": 480,
  "friday-work-time-minutes": 480,
  "saturday-work-time-minutes": 0,
  "special-holiday-work-time-minutes": 0
}
```

## 特別休日

`special-holidays` は、チーム全体で共有する特別休日の日付一覧を表します。
祝日など、土曜日または日曜日以外に休日扱いしたい日付を保存します。

日付は `DateOnly` の標準形式で保存します。

```json
[
  "2026-05-04",
  "2026-05-05"
]
```

## プロジェクト内メンバー情報

`member-info` の各要素は、プロジェクト内におけるメンバー1件分の設定を表します。
対象プロジェクトは親要素の `project-id` によって識別します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `member-id` | string | 対象メンバーID |
| `participation-start-date` | string/null | 参加開始日 |
| `participation-end-date` | string/null | 参加終了日 |
| `work-times` | array | 日付別の作業可能時間上書き値 |

日付は `DateOnly` の標準形式で保存します。

```json
{
  "member-id": "00000000-0000-0000-0000-000000000001",
  "participation-start-date": "2026-04-01",
  "participation-end-date": null,
  "work-times": []
}
```

## 作業可能時間上書き

`work-times` の各要素は、プロジェクト内メンバーの日付単位作業可能時間上書き値を表します。
対象メンバーは親要素の `member-id` によって識別します。
未入力の日付は、メンバーの曜日別デフォルト作業可能時間を使用します。
対象日が特別休日の場合は、メンバーの特別休日デフォルト作業可能時間を使用します。

| キー | 型 | 内容 |
| --- | --- | --- |
| `work-date` | string | 対象日 |
| `work-time-minutes` | number | 作業可能時間。単位は分 |

```json
{
  "work-date": "2026-04-28",
  "work-time-minutes": 420
}
```

## JSON全体例

以下は、保存ファイル構造を網羅した例です。
`projects` の各要素が、タスク編集データとプロジェクト内メンバー情報を内包します。

```json
{
  "projects": [
    {
      "project-id": "11111111-1111-1111-1111-111111111111",
      "project-name": "New Project",
      "task-editor": {
        "nodes": [
          {
            "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            "type": "TaskNode",
            "position": {
              "x": 120,
              "y": 80
            },
            "details": {
              "task-name": "Requirement Review",
              "person": null,
              "assignee-member-id": "22222222-2222-2222-2222-222222222222",
              "collaborator-member-ids": [
                "33333333-3333-3333-3333-333333333333"
              ],
              "start-date-time": "2026-04-28T09:00:00",
              "end-date-time": "2026-04-28T18:00:00",
              "work-estimate-minutes": 480,
              "suspension-periods": [
                {
                  "start-date-time": "2026-04-28T12:00:00",
                  "end-date-time": "2026-04-28T13:00:00"
                }
              ],
              "comment": "Initial review task"
            },
            "ports": [
              {
                "id": "aaaaaaaa-0001-0000-0000-000000000000",
                "name": "Input",
                "type": "Input"
              },
              {
                "id": "aaaaaaaa-0002-0000-0000-000000000000",
                "name": "Output",
                "type": "Output"
              }
            ]
          },
          {
            "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
            "type": "TaskNode",
            "position": {
              "x": 420,
              "y": 220
            },
            "details": {
              "task-name": "Implementation",
              "person": null,
              "assignee-member-id": "33333333-3333-3333-3333-333333333333",
              "collaborator-member-ids": [],
              "start-date-time": null,
              "end-date-time": null,
              "work-estimate-minutes": 900,
              "suspension-periods": [],
              "comment": ""
            },
            "ports": [
              {
                "id": "bbbbbbbb-0001-0000-0000-000000000000",
                "name": "Input",
                "type": "Input"
              },
              {
                "id": "bbbbbbbb-0002-0000-0000-000000000000",
                "name": "Output",
                "type": "Output"
              }
            ]
          }
        ],
        "connections": [
          {
            "id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
            "from-port-id": "aaaaaaaa-0002-0000-0000-000000000000",
            "to-port-id": "bbbbbbbb-0001-0000-0000-000000000000"
          }
        ]
      },
      "member-info": [
        {
          "member-id": "22222222-2222-2222-2222-222222222222",
          "participation-start-date": "2026-04-01",
          "participation-end-date": null,
          "work-times": [
            {
              "work-date": "2026-04-29",
              "work-time-minutes": 420
            }
          ]
        },
        {
          "member-id": "33333333-3333-3333-3333-333333333333",
          "participation-start-date": "2026-04-15",
          "participation-end-date": "2026-05-31",
          "work-times": []
        }
      ]
    },
    {
      "project-id": "44444444-4444-4444-4444-444444444444",
      "project-name": "Another Project",
      "task-editor": {
        "nodes": [],
        "connections": []
      },
      "member-info": []
    }
  ],
  "members": [
    {
      "member-id": "22222222-2222-2222-2222-222222222222",
      "display-name": "Alex",
      "sunday-work-time-minutes": 0,
      "monday-work-time-minutes": 480,
      "tuesday-work-time-minutes": 480,
      "wednesday-work-time-minutes": 480,
      "thursday-work-time-minutes": 480,
      "friday-work-time-minutes": 480,
      "saturday-work-time-minutes": 0,
      "special-holiday-work-time-minutes": 0
    },
    {
      "member-id": "33333333-3333-3333-3333-333333333333",
      "display-name": "Bob",
      "sunday-work-time-minutes": 0,
      "monday-work-time-minutes": 360,
      "tuesday-work-time-minutes": 360,
      "wednesday-work-time-minutes": 360,
      "thursday-work-time-minutes": 360,
      "friday-work-time-minutes": 360,
      "saturday-work-time-minutes": 0,
      "special-holiday-work-time-minutes": 0
    }
  ],
  "special-holidays": [
    "2026-05-04",
    "2026-05-05"
  ]
}
```

## 補足

- `members` はチーム全体のメンバーマスタです。
- `member-info` は、親プロジェクトに属するメンバー設定として扱います。
- `work-times` は、親の `member-id` に属する日付別上書き値として扱います。
- `task-editor` は親プロジェクトに属するため、内部には `project-id` を持ちません。
- ガントチャート機能の保存データは、実装時にプロジェクト配下へ追加します。
