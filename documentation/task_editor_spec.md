# タスクエディタ仕様

このドキュメントは、WcmScheduler のタスクエディタ機能について、公開仕様として構成と依存関係を整理したものです。

## 概要

タスクエディタは、タスクをノードとして配置し、入出力ポートを接続線でつなぐことでタスク間の関係を表現する機能です。

主な責務は以下です。

- ノードの追加、削除、選択、移動
- ノード詳細情報の編集
- 作業見積時間と中断期間の編集
- タスクステータスの自動算出と表示
- 入出力ポートの表示
- 接続線の作成、削除、選択
- ズーム、パン、グリッド表示
- Undo/Redo履歴管理
- 保存データとの相互変換
- 日時編集ダイアログとの連携

## レイヤ構成

タスクエディタはWPF/MVVMを基本として、以下のレイヤで構成します。

- View: XAMLとコードビハインドで画面表示とUIイベントの受け渡しを行います。
- ViewModel: ノード、接続線、グリッド、Undo/Redoなどの状態と操作を管理します。
- Core/Service: UI非依存の補助ロジックや、ダイアログ起動サービスを管理します。
- Mapper/Model: 保存データとViewModelの相互変換を管理します。

## 主要クラス

### View

| クラス | 役割 |
| --- | --- |
| `MainWindow` | アプリケーション全体のメインウィンドウ |
| `TeamProjectsView` | チーム内プロジェクトのタブ表示 |
| `ProjectView` | 1つのプロジェクト内の機能タブ表示 |
| `NodeEditorTab` | タスク編集タブ全体 |
| `NodeEditorControl` | ノードエディタキャンバス |
| `NodeDetailControl` | 選択中ノードの詳細編集 |
| `HistoryControl` | Undo/Redo履歴表示 |
| `NodeCollectionControl` | ノード一覧表示 |
| `NodeControl` | ノード1個の表示 |
| `PortControl` | ポート表示と接続ドラッグ操作 |
| `ConnectionCollectionControl` | 接続線一覧表示 |
| `ConnectionControl` | 接続線1本の表示 |
| `DateTimeEditorWindow` | 日時編集ダイアログ |
| `ThemeSettingWindow` | テーマ設定ダイアログ |
| `ColorPickerWindow` | 汎用色編集ダイアログ |

### ViewModel

| クラス | 役割 |
| --- | --- |
| `SchedulerViewModel` | アプリケーション全体の状態管理 |
| `TeamProjectsViewModel` | チーム内プロジェクト一覧とタブ管理 |
| `ProjectViewModel` | 1つのプロジェクト全体の管理 |
| `NodeEditorViewModel` | タスクエディタ全体の状態管理 |
| `NodeCollectionViewModel` | ノード一覧、追加、削除、選択、移動 |
| `NodeViewModel` | ノード1個の状態とタスクステータス |
| `NodeDetailViewModel` | タスク名、担当者、日時、作業見積時間、中断期間、コメントなどの詳細情報 |
| `SuspensionPeriodViewModel` | タスク中断期間1件の状態と日時編集 |
| `PortViewModel` | 入出力ポートの状態 |
| `ConnectionCollectionViewModel` | 接続線一覧、作成、削除、選択 |
| `ConnectionViewModel` | 接続線1本の状態と描画ジオメトリ |
| `LineViewModel` | グリッド線の描画情報 |
| `DateTimeEditorViewModel` | 日時編集ダイアログの入力状態 |
| `ThemeMenuItemViewModel` | テーマメニューの表示状態と適用コマンド |
| `ThemeSettingViewModel` | テーマ設定ダイアログの入力状態と保存処理 |
| `ColorItemViewModel` | テーマ設定内の色1項目の表示状態 |
| `ColorPickerViewModel` | 汎用色編集ダイアログの入力状態 |

### Core / Service / Model

| クラス | 役割 |
| --- | --- |
| `GridManager` | 論理座標、ズーム、パン、グリッド線生成 |
| `UndoRedoManager` | Undo/Redoスタックと履歴管理 |
| `IEditableField` | 遅延コミット対象フィールドの共通インターフェース |
| `EditableField` | 遅延コミット対象の編集フィールド管理 |
| `TabInfo` | タブ表示用情報 |
| `IDateTimeEditorService` | 日時編集ダイアログ起動サービスのインターフェース |
| `DateTimeEditorService` | 日時編集ダイアログの起動 |
| `DateTimeEditorWindow` | 日時編集ダイアログのWindow本体 |
| `ThemeManager` | テーマファイルの読み込み、保存、適用 |
| `AppSettingsManager` | アプリケーション設定の読み込み、保存 |
| `IColorPickerService` | 汎用色編集ダイアログ起動サービスのインターフェース |
| `ColorPickerService` | 汎用色編集ダイアログの起動 |
| `ThemeSettingModel` | テーマ設定の保存モデル |
| `AppSettingsModel` | アプリケーション設定の保存モデル |

### Action

| クラス | 役割 |
| --- | --- |
| `AddNodeAction` | ノード追加のUndo/Redo |
| `DeleteNodeAction` | ノード削除のUndo/Redo |
| `MoveNodeAction` | ノード移動のUndo/Redo |
| `AddConnectionAction` | 接続線追加のUndo/Redo |
| `DeleteConnectionAction` | 接続線削除のUndo/Redo |
| `EditNodeDetailPropertyAction` | ノード詳細プロパティ編集のUndo/Redo |
| `AddSuspensionPeriodAction` | 中断期間追加のUndo/Redo |
| `DeleteSuspensionPeriodAction` | 中断期間削除のUndo/Redo |
| `EditSuspensionPeriodPropertyAction` | 中断期間プロパティ編集のUndo/Redo |

### SaveData

| クラス | 役割 |
| --- | --- |
| `TaskEditorDataModel` | タスクエディタ全体の保存データ |
| `NodeDataModel` | ノード1件の保存データ |
| `NodeDetailsDataModel` | ノード詳細情報の保存データ |
| `SuspensionPeriodDataModel` | タスク中断期間の保存データ |
| `ConnectionDataModel` | 接続線の保存データ |
| `PortDataModel` | ポートの保存データ |
| `PositionDataModel` | 座標の保存データ |

## タスク詳細情報

ノード詳細では、ガントチャート表示やスケジュール算定の前提となるタスク情報を管理します。

主な管理項目は以下です。

- タスク名
- 担当者
- 開始日時
- 終了日時
- 作業見積時間
- 中断期間
- コメント

作業見積時間は分単位で保持します。
画面上では分入力に加えて、`4h30m` のような時間換算表示を行います。

中断期間は任意数を保持できます。
各中断期間は開始日時と終了日時を持ち、日時編集ダイアログを通じて編集します。
同一タスク内の中断期間同士が重複することは入力時点では許可し、ガントチャート算定前に正規化・マージする方針です。

## タスクステータス

タスクステータスは手動入力ではなく、タスク詳細情報、接続元タスク、中断期間から自動算出します。

ステータスは以下です。

- `Ready`: 着手可能状態
- `Pending`: 着手不可状態
- `InProgress`: 着手中状態
- `Done`: 完了状態

ステータスの優先順位は `Done`、`Pending`、`InProgress`、`Ready` の順です。

算出条件は以下です。

- `Done`: 終了日時が設定されており、終了日時が現在日時以前である場合。
- `Pending`: 現在日時が中断期間内である場合、またはInputに接続されている前段タスクに `Done` ではないものがある場合。
- `InProgress`: 開始日時が設定されており、開始日時が現在日時以前である場合。ただし、`Pending` 条件を満たす場合は `Pending` を優先します。
- `Ready`: 上記のいずれにも該当しない場合。

ステータスは以下のタイミングで再計算します。

- ノード一覧が変更されたとき。
- 接続線一覧が変更されたとき。
- 開始日時または終了日時が変更されたとき。
- 中断期間の追加、削除、開始日時、終了日時が変更されたとき。
- Undo/Redoまたは履歴移動によってノード、接続線、詳細情報が変化したとき。
- 一定間隔のタイマーによって現在日時に対する状態を更新するとき。

タスク詳細表示では、タスクIDとタスク名の間に現在ステータスを表示します。
ステータス表示は文字列の左にステータス色の矩形を配置し、背景色は通常のテーマ色を維持します。

ノード表示では、左端のステータスバーと枠線色で状態を表現します。
ステータス色はテーマ設定の対象です。

## ノード表示と移動制限

ノードは `NodeControl` によって表示します。
ノードの左端にはステータスバーを配置し、枠線色と合わせてタスクステータスを表現します。

ノード移動は `NodeEditorCanvas` 座標系を基準に扱います。
ドラッグ開始位置とドラッグ中の現在位置は同じ座標系で取得し、`NodeEditorViewModel` が論理座標へ変換して移動量を算出します。

移動後の位置は `GridManager` によって表示領域内へ制限します。
位置制限では、ノードの実描画範囲を考慮したサイズを使用します。
実描画範囲には、枠線や負のMarginなど、レイアウトサイズだけでは表せない描画上の広がりを含めます。

ノード位置はグリッドにスナップします。
スナップによって表示領域外へ丸められる場合があるため、スナップ後に再度クランプを行います。

## 依存関係図

以下はタスクエディタ周辺の主要な依存関係です。

### レイヤ配置

タスクエディタ周辺の主要クラスを、View、ViewModel、Core/Serviceに分けて配置します。
この図は依存線よりも、クラスの所属と読み順を優先しています。

```d2
direction: down
grid-rows: 3
grid-gap: 40

views: Views {
  style.fill: "#D0E8FF"
  style.stroke: "#4A90E2"
  grid-columns: 4
  grid-gap: 16

  MainWindow
  TeamProjectsView
  ProjectView
  NodeEditorTab
  DateTimeEditorWindow
  ThemeSettingWindow
  ColorPickerWindow

  controls: "NodeEditorTab.Controls" {
    grid-columns: 3
    grid-gap: 12
    NodeEditorControl
    NodeDetailControl
    HistoryControl
    NodeCollectionControl
    NodeControl
    PortControl
    ConnectionCollectionControl
    ConnectionControl
  }
}

viewmodels: ViewModels {
  style.fill: "#DFFFE0"
  style.stroke: "#5CB85C"
  grid-columns: 4
  grid-gap: 16

  SchedulerViewModel
  TeamProjectsViewModel
  ProjectViewModel
  NodeEditorViewModel

  DateTimeEditorViewModel
  ThemeMenuItemViewModel
  ThemeSettingViewModel
  ColorItemViewModel
  ColorPickerViewModel

  node: Node {
    grid-rows: 5
    NodeCollectionViewModel
    NodeViewModel
    NodeDetailViewModel
    SuspensionPeriodViewModel
    PortViewModel
  }

  connection: Connection {
    grid-rows: 3
    ConnectionCollectionViewModel
    ConnectionViewModel
    LineViewModel
  }
}

core: CoreAndServices {
  style.fill: "#F0F0F0"
  style.stroke: "#999999"
  grid-rows: 2
  grid-gap: 16

  TabInfo
  GridManager
  UndoRedoManager
  IEditableField
  EditableField
  IDateTimeEditorService
  DateTimeEditorService
  DateTimeEditorWindowClass: "DateTimeEditorWindow"
  ThemeManager
  AppSettingsManager
  IColorPickerService
  ColorPickerService
  ThemeSettingModel
  AppSettingsModel
  SuspensionPeriodDataModel
}

views -> viewmodels: binding / commands
viewmodels -> core: use
```

### View階層

View同士の親子関係です。

```d2
direction: down

MainWindow -> TeamProjectsView
TeamProjectsView -> ProjectView
ProjectView -> NodeEditorTab

NodeEditorTab -> NodeEditorControl
NodeEditorTab -> NodeDetailControl
NodeDetailControl -> HistoryControl

NodeEditorControl -> NodeCollectionControl
NodeCollectionControl -> NodeControl
NodeControl -> PortControl

NodeEditorControl -> ConnectionCollectionControl
ConnectionCollectionControl -> ConnectionControl
```

### ViewとViewModelの対応

Viewから参照されるViewModelまたは管理クラスの対応です。

```d2
direction: right
grid-columns: 2
grid-gap: 40

views: Views {
  style.fill: "#D0E8FF"
  style.stroke: "#4A90E2"
  grid-rows: 12

  MainWindow
  TeamProjectsView
  ProjectView
  NodeEditorControl
  NodeControl
  PortControl
  ConnectionControl
  NodeDetailControl
  HistoryControl
  DateTimeEditorWindow
  ThemeSettingWindow
  ColorPickerWindow
}

targets: "ViewModels / Core" {
  style.fill: "#DFFFE0"
  style.stroke: "#5CB85C"
  grid-rows: 12

  SchedulerViewModel
  TeamProjectsViewModel
  ProjectViewModel
  Blank1:{style.opacity:0}
  Blank2:{style.opacity:0}
  NodeEditorViewModel
  Blank3:{style.opacity:0}
  NodeDetailViewModel
  UndoRedoManager
  DateTimeEditorViewModel
  ThemeSettingViewModel
  ColorPickerViewModel
}

views.MainWindow -> targets.SchedulerViewModel
views.TeamProjectsView -> targets.TeamProjectsViewModel
views.ProjectView -> targets.ProjectViewModel
views.NodeEditorControl -> targets.NodeEditorViewModel
views.NodeControl -> targets.NodeEditorViewModel
views.PortControl -> targets.NodeEditorViewModel
views.ConnectionControl -> targets.NodeEditorViewModel
views.NodeDetailControl -> targets.NodeDetailViewModel
views.HistoryControl -> targets.UndoRedoManager
views.DateTimeEditorWindow -> targets.DateTimeEditorViewModel
views.ThemeSettingWindow -> targets.ThemeSettingViewModel
views.ColorPickerWindow -> targets.ColorPickerViewModel
```

### ViewModelとCore/Serviceの依存

ViewModel間、およびCore/Serviceへの主要な依存です。

```d2
direction: right
grid-columns: 2
grid-gap: 40

viewmodels: ViewModels {
  style.fill: "#DFFFE0"
  style.stroke: "#5CB85C"
  grid-columns: 3

  Blank1_1:{style.opacity:0}
  Blank1_2:{style.opacity:0}
  Blank1_3:{style.opacity:0}
  Blank1_4:{style.opacity:0}
  ConnectionCollectionViewModel
  ConnectionViewModel
  LineViewModel
  PortViewModel
  Blank1_5:{style.opacity:0}
  Blank1_6:{style.opacity:0}

  SchedulerViewModel
  TeamProjectsViewModel
  ProjectViewModel
  NodeEditorViewModel
  Blank2_1:{style.opacity:0}
  NodeCollectionViewModel
  NodeViewModel
  NodeDetailViewModel
  Blank2_2:{style.opacity:0}
  Blank2_3:{style.opacity:0}

  Blank3_1:{style.opacity:0}
  Blank3_2:{style.opacity:0}
  Blank3_3:{style.opacity:0}
  Blank3_4:{style.opacity:0}
  Blank3_5:{style.opacity:0}
  Blank3_6:{style.opacity:0}
  Blank3_7:{style.opacity:0}
  Blank3_8:{style.opacity:0}
  SuspensionPeriodViewModel
  DateTimeEditorViewModel
}

core: CoreAndServices {
  style.fill: "#F0F0F0"
  style.stroke: "#999999"
  grid-columns: 2

  Blank1_1:{style.opacity:0}
  TabInfo
  GridManager
  Blank1_2:{style.opacity:0}
  UndoRedoManager
  Blank1_3:{style.opacity:0}
  IDateTimeEditorService
  EditableField
  IEditableField
  Blank1_4:{style.opacity:0}

  Blank2_1:{style.opacity:0}
  Blank2_2:{style.opacity:0}
  Blank2_3:{style.opacity:0}
  DateTimeEditorService
  Blank2_4:{style.opacity:0}
  Blank2_5:{style.opacity:0}
  Blank2_6:{style.opacity:0}
  Blank2_7:{style.opacity:0}
  DateTimeEditorWindowClass: "DateTimeEditorWindow"
}

viewmodels.SchedulerViewModel -> viewmodels.TeamProjectsViewModel
viewmodels.SchedulerViewModel -> core.TabInfo
viewmodels.TeamProjectsViewModel -> viewmodels.ProjectViewModel
viewmodels.TeamProjectsViewModel -> core.TabInfo
viewmodels.ProjectViewModel -> viewmodels.NodeEditorViewModel
viewmodels.ProjectViewModel -> core.TabInfo

viewmodels.NodeEditorViewModel -> viewmodels.NodeCollectionViewModel
viewmodels.NodeEditorViewModel -> viewmodels.ConnectionCollectionViewModel
viewmodels.NodeEditorViewModel -> core.GridManager
viewmodels.NodeEditorViewModel -> core.UndoRedoManager
viewmodels.NodeEditorViewModel -> core.IDateTimeEditorService
viewmodels.NodeEditorViewModel -> core.DateTimeEditorService

viewmodels.NodeCollectionViewModel -> viewmodels.NodeViewModel
viewmodels.NodeCollectionViewModel -> core.UndoRedoManager
viewmodels.NodeCollectionViewModel -> core.IDateTimeEditorService
viewmodels.NodeViewModel -> viewmodels.NodeDetailViewModel
viewmodels.NodeViewModel -> viewmodels.PortViewModel
viewmodels.NodeViewModel -> core.IDateTimeEditorService
viewmodels.NodeDetailViewModel -> viewmodels.SuspensionPeriodViewModel
viewmodels.NodeDetailViewModel -> core.EditableField
viewmodels.NodeDetailViewModel -> core.IEditableField
viewmodels.NodeDetailViewModel -> core.UndoRedoManager
viewmodels.NodeDetailViewModel -> core.IDateTimeEditorService
viewmodels.SuspensionPeriodViewModel -> core.UndoRedoManager
viewmodels.SuspensionPeriodViewModel -> core.IDateTimeEditorService

viewmodels.ConnectionCollectionViewModel -> viewmodels.ConnectionViewModel
viewmodels.ConnectionCollectionViewModel -> core.UndoRedoManager
viewmodels.ConnectionViewModel -> viewmodels.LineViewModel

core.DateTimeEditorService -> core.DateTimeEditorWindowClass
core.DateTimeEditorService -> core.IDateTimeEditorService: implements
core.DateTimeEditorWindowClass -> viewmodels.DateTimeEditorViewModel
```

### テーマ設定とColorPickerの依存

テーマ設定と汎用ColorPickerはタスクエディタ本体から独立した支援機能です。
テーマメニューからテーマ適用またはテーマ設定を開き、テーマ設定内の色編集でColorPickerを利用します。

```d2
direction: right
grid-columns: 3
grid-gap: 40

views: Views {
  style.fill: "#D0E8FF"
  style.stroke: "#4A90E2"
  grid-columns: 1

  MainWindow
  Blank1_1:{style.opacity:0}
  ThemeSettingWindow
  Blank1_2:{style.opacity:0}
  ColorPickerWindow
}

viewmodels: ViewModels {
  style.fill: "#DFFFE0"
  style.stroke: "#5CB85C"
  grid-columns: 3

  SchedulerViewModel
  ThemeMenuItemViewModel
  Blank1_1:{style.opacity:0}
  ColorPickerViewModel
  Blank1_2:{style.opacity:0}

  Blank2_1:{style.opacity:0}
  Blank2_2:{style.opacity:0}
  Blank2_3:{style.opacity:0}
  Blank2_4:{style.opacity:0}
  Blank2_5:{style.opacity:0}
  
  Blank3_1:{style.opacity:0}
  Blank3_2:{style.opacity:0}
  ThemeSettingViewModel
  ColorItemViewModel
  Blank3_3:{style.opacity:0}
}

core: "Core / Service / Model" {
  style.fill: "#F0F0F0"
  style.stroke: "#999999"
  grid-columns: 2

  ThemeManager
  Blank1_1:{style.opacity:0}
  ThemeSettingModel
  IColorPickerService
  ColorPickerService

  AppSettingsManager
  AppSettingsModel
  Blank2_1:{style.opacity:0}
  Blank2_2:{style.opacity:0}
  Blank2_3:{style.opacity:0}
}

views.MainWindow -> viewmodels.SchedulerViewModel
views.ThemeSettingWindow -> viewmodels.ThemeSettingViewModel
views.ColorPickerWindow -> viewmodels.ColorPickerViewModel

viewmodels.SchedulerViewModel -> viewmodels.ThemeMenuItemViewModel
viewmodels.SchedulerViewModel -> viewmodels.ThemeSettingViewModel
viewmodels.SchedulerViewModel -> core.ThemeManager
viewmodels.SchedulerViewModel -> views.ThemeSettingWindow

viewmodels.ThemeSettingViewModel -> viewmodels.ColorItemViewModel
viewmodels.ThemeSettingViewModel -> viewmodels.ColorPickerViewModel: color validation
viewmodels.ThemeSettingViewModel -> core.IColorPickerService
viewmodels.ThemeSettingViewModel -> core.ThemeManager
viewmodels.ThemeSettingViewModel -> core.ThemeSettingModel
viewmodels.ColorItemViewModel -> viewmodels.ColorPickerViewModel: color validation

core.ColorPickerService -> core.IColorPickerService: implements
core.ColorPickerService -> views.ColorPickerWindow
views.ColorPickerWindow -> viewmodels.ColorPickerViewModel

core.ThemeManager -> core.ThemeSettingModel
core.ThemeManager -> core.AppSettingsManager
core.AppSettingsManager -> core.AppSettingsModel
```

## 補足

ViewはUIイベントを受け取り、できるだけ `NodeEditorViewModel` へ処理を委譲します。
ノードや接続線の座標は論理座標で管理し、表示時にズームとパンを反映します。

Undo/Redo対象の操作は `IUndoableAction` として表現します。
ノード追加、削除、移動、接続線追加、削除、ノード詳細編集、中断期間追加、削除、中断期間編集が主な履歴対象です。
