# 保存データ仕様

このドキュメントは、WcmScheduler の保存ファイルJSON構造について整理したものです。
JSONキーは kebab-case を基本とします。

JSON Schema は以下に定義します。

- [保存データJSON Schema](save_data.schema.json)

## 概要

保存ファイルは、プロジェクト一覧、チームメンバー一覧、特別休日一覧を1つのJSONとして保持します。

チームメンバーはチーム全体のマスタ情報として `members` に保存します。
タスク編集データ、メンバー別のプロジェクト参加期間、メンバー別の日付単位作業可能時間は、プロジェクト固有の情報として `projects` の各要素に保存します。

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
