# WcmScheduler

## 概要

ノードベースのスケジュール管理ツールです

## MainApplicationフォルダの構造

```plaintext
📂 MainApplication
├── 📂 Helpers
│   └── 📄 VisualTreeUtils.cs                          # WPFのVisualTreeを探索・操作するユーティリティ
│
├── 📂 Infrastructure
│   ├── 📄 FileService.cs                              # ファイル読込・保存
│   ├── 📄 IFileService.cs                             # ファイル読込・保存のインターフェース
│   ├── 📄 IJsonSerializerService.cs                   # JSONシリアライザのインターフェース
│   └── 📄 JsonSerializerService.cs                    # JSONシリアライズ/デシリアライズ
│
├── 📂 Mappers
│   ├── 📄 ConnectionMapper.cs                         # 接続線情報のViewModel⇔Model相互変換
│   ├── 📄 NodeEditorMapper.cs                         # タスク編集機能(NodeEditor/TaskEditor)のViewModel⇔Model相互変換
│   └── 📄 NodeMapper.cs                               # ノード情報のViewModel⇔Model相互変換
│
├── 📂 Models
│   ├── 📂 SaveData                                    # 保存するデータのモデル
│   │   ├── 📄 ConnectionDataModel.cs                  # 接続線管理用情報
│   │   ├── 📄 MemberDataModel.cs                      # チームメンバー保存情報
│   │   ├── 📄 MemberWorkTimeDataModel.cs              # メンバーの日付別作業可能時間保存情報
│   │   ├── 📄 NodeDataModel.cs                        # ノード管理用情報
│   │   ├── 📄 NodeDetailsDataModel.cs                 # ノードごとの詳細情報
│   │   ├── 📄 PortDataModel.cs                        # ノード内ポート管理用情報
│   │   ├── 📄 PositionDataModel.cs                    # 座標管理用情報
│   │   ├── 📄 ProjectDataModel.cs                     # プロジェクト単位の保存情報
│   │   ├── 📄 ProjectMemberInfoDataModel.cs           # プロジェクト内メンバー情報
│   │   ├── 📄 RootSaveDataModel.cs                    # 保存するデータのルート情報
│   │   ├── 📄 SuspensionPeriodDataModel.cs            # タスク中断期間の保存情報
│   │   └── 📄 TaskEditorDataModel.cs                  # タスク編集機能についての情報
│   └── 📂 Settings                                    # 設定関連のモデル
│       ├── 📄 AppSettingsModel.cs                     # アプリケーション設定情報
│       └── 📄 ThemeSettingModel.cs                    # テーマ設定情報
│
├── 📂 Themes                                          # テーマ関連リソース
│   ├── 📂 schema                                      # JSONスキーマ
│   │   └── 📄 ThemeSchema.json                        # テーマ設定JSONのスキーマ
│   ├── 📄 Dark.json                                   # ダークモード向けデフォルトテーマ
│   └── 📄 Light.json                                  # ライトモード向けデフォルトテーマ
│
├── 📂 ViewModels
│   ├── 📂 Actions                                     # Undo/Redo用の操作履歴アクション
│   │   ├── 📄 AddConnectionAction.cs                  # Undo/Redoアクション：接続線追加
│   │   ├── 📄 AddNodeAction.cs                        # Undo/Redoアクション：ノード追加
│   │   ├── 📄 AddSuspensionPeriodAction.cs            # Undo/Redoアクション：中断期間追加
│   │   ├── 📄 DeleteConnectionAction.cs               # Undo/Redoアクション：接続線削除
│   │   ├── 📄 DeleteNodeAction.cs                     # Undo/Redoアクション：ノード削除
│   │   ├── 📄 DeleteSuspensionPeriodAction.cs         # Undo/Redoアクション：中断期間削除
│   │   ├── 📄 EditNodeDetailPropertyAction.cs         # Undo/Redoアクション：ノード詳細プロパティ編集
│   │   ├── 📄 EditSuspensionPeriodPropertyAction.cs   # Undo/Redoアクション：中断期間プロパティ編集
│   │   └── 📄 MoveNodeAction.cs                       # Undo/Redoアクション：ノード移動
│   │
│   ├── 📂 Converters                                  # XAMLバインディング用コンバータ
│   │   ├── 📄 BoolToVisibilityConverter.cs            # bool      → Visibility
│   │   ├── 📄 DateTimeDisplayConverter.cs             # DateTime? → 表示文字列
│   │   └── 📄 DisplayNameConverter.cs                 # DisplayName属性 → 表示名
│   │
│   ├── 📂 Core                                        # 基盤ロジック(UI非依存)
│   │   ├── 📄 AppSettingsManager.cs                   # アプリケーション設定の読込・保存
│   │   ├── 📄 EditableField.cs                        # 編集フィールドの共通ロジック(遅延コミット)
│   │   ├── 📄 GridManager.cs                          # 論理座標系・ズーム・パン管理
│   │   ├── 📄 PointEx.cs                              # 座標計算用Point型拡張
│   │   ├── 📄 RelayCommand.cs                         # ICommand実装(MVVMの基本)
│   │   ├── 📄 TabInfo.cs                              # タブ管理用情報
│   │   ├── 📄 ThemeManager.cs                         # テーマ設定管理
│   │   ├── 📄 UndoRedoManager.cs                      # Undo/Redo管理
│   │   └── 📄 ViewModelBase.cs                        # ViewModelの基底クラス
│   │
│   ├── 📂 GanttChartModel                             # ガントチャート表示用ViewModel
│   │   ├── 📄 GanttChartService.cs                    # 予定期間算定・表示用データ生成
│   │   ├── 📄 GanttChartViewModel.cs                  # ガントチャート全体の状態管理
│   │   ├── 📄 GanttDependencyLineViewModel.cs         # タスク依存関係線の描画情報
│   │   ├── 📄 GanttSuspensionItemViewModel.cs         # 中断期間表示情報
│   │   ├── 📄 GanttTaskItemViewModel.cs               # ガントチャート上のタスク1件の状態
│   │   └── 📄 GanttTimelineDayViewModel.cs            # 時間軸の日付単位表示情報
│   │
│   ├── 📂 ProjectModel
│   │   ├── 📄 ConnectionCollectionViewModel.cs        # 接続線の一覧管理
│   │   ├── 📄 ConnectionViewModel.cs                  # 接続線1本の状態
│   │   ├── 📄 LineViewModel.cs                        # 線分の描画情報(接続線の補助)
│   │   ├── 📄 NodeCollectionViewModel.cs              # ノード一覧管理(生成・削除・選択)
│   │   ├── 📄 NodeDetailViewModel.cs                  # ノードの詳細情報(編集可能プロパティ)
│   │   ├── 📄 NodeEditorViewModel.cs                  # ノードエディタ全体の状態管理(ズーム・パン・Undo/Redo)
│   │   ├── 📄 NodeViewModel.cs                        # ノード1個の状態・編集ロジック
│   │   ├── 📄 PortViewModel.cs                        # ポート(入出力端子)の状態
│   │   ├── 📄 ProjectViewModel.cs                     # 1つのプロジェクト全体の管理
│   │   ├── 📄 SuspensionPeriodRange.cs                # 中断期間の正規化済み範囲
│   │   └── 📄 SuspensionPeriodViewModel.cs            # タスク中断期間の状態・編集ロジック
│   │
│   ├── 📂 Service
│   │   ├── 📄 ColorPickerService.cs                   # 色編集ダイアログを開くサービス(UI呼び出し)
│   │   ├── 📄 DateTimeEditorService.cs                # 日時編集ダイアログを開くサービス(UI呼び出し)
│   │   ├── 📄 IColorPickerService.cs                  # 色編集サービスのインターフェース
│   │   └── 📄 IDateTimeEditorService.cs               # 日時編集サービスのインターフェース
│   │
│   ├── 📂 StatusBarModel                              # ステータスバー表示用ViewModel
│   │   └── 📄 StatusBarViewModel.cs                   # アプリケーション共通ステータスバーの状態管理
│   │
│   ├── 📂 ThemeModel
│   │   ├── 📄 ThemeMenuItemViewModel.cs               # テーマ関連メニューの1項目
│   │   └── 📄 ThemeSettingViewModel.cs                # テーマ編集画面のViewModel
│   │
│   ├── 📂 TeamModel
│   │   ├── 📄 CollaboratorSelectionViewModel.cs       # 作業協力者選択行
│   │   ├── 📄 CollaboratorOptionViewModel.cs          # 作業協力者選択肢
│   │   ├── 📄 MemberOptionViewModel.cs                # メンバー選択肢
│   │   ├── 📄 MemberWorkCalendarDayViewModel.cs       # メンバー作業可能時間カレンダーの日付単位表示
│   │   ├── 📄 ProjectMemberParticipationViewModel.cs  # プロジェクト別メンバー参加期間
│   │   ├── 📄 ProjectMemberWorkTimeViewModel.cs       # プロジェクト別日付別作業可能時間
│   │   ├── 📄 TeamMemberViewModel.cs                  # チームメンバー
│   │   └── 📄 TeamSettingsViewModel.cs                # チーム設定画面のViewModel
│   │
│   ├── 📄 ColorPickerViewModel.cs                     # 色編集ダイアログのViewModel(UI入力ロジック)
│   ├── 📄 DateTimeEditorViewModel.cs                  # 日時編集ダイアログのViewModel(UI入力ロジック)
│   ├── 📄 SchedulerViewModel.cs                       # アプリケーション全体の状態管理
│   └── 📄 TeamProjectsViewModel.cs                    # チーム内の複数のプロジェクトの管理
│
├── 📂 Views
│   ├── 📂 Behaviors                                   # XAMLの動作拡張
│   │   ├── 📄 ListBoxAutoScrollBehavior.cs            # ListBoxの自動スクロール
│   │   └── 📄 ListBoxItemDoubleClickBehavior.cs       # ダブルクリック動作
│   │
│   ├── 📂 NodeEditorTab                               # ノードエディタUI一式
│   │   ├── 📂 Controls
│   │   │   ├── 📄 ConnectionCollectionControl.xaml    # 接続線一覧管理
│   │   │   ├── 📄 ConnectionCollectionControl.xaml.cs
│   │   │   ├── 📄 ConnectionControl.xaml              # 接続線の見た目
│   │   │   ├── 📄 ConnectionControl.xaml.cs
│   │   │   ├── 📄 GridControl.xaml                    # グリッド・原点軸の見た目
│   │   │   ├── 📄 GridControl.xaml.cs
│   │   │   ├── 📄 HistoryControl.xaml                 # Undo/Redo履歴表示
│   │   │   ├── 📄 HistoryControl.xaml.cs
│   │   │   ├── 📄 NodeCollectionControl.xaml          # ノード一覧管理
│   │   │   ├── 📄 NodeCollectionControl.xaml.cs
│   │   │   ├── 📄 NodeControl.xaml                    # ノードの見た目
│   │   │   ├── 📄 NodeControl.xaml.cs
│   │   │   ├── 📄 NodeDetailControl.xaml              # ノード詳細(プロパティ編集)
│   │   │   ├── 📄 NodeDetailControl.xaml.cs
│   │   │   ├── 📄 NodeEditorControl.xaml              # エディタ全体のUI
│   │   │   ├── 📄 NodeEditorControl.xaml.cs
│   │   │   ├── 📄 PortControl.xaml                    # ポートの見た目
│   │   │   └── 📄 PortControl.xaml.cs
│   │   ├── 📄 NodeDetailTemplateSelector.cs           # ノード詳細のテンプレート切り替え
│   │   ├── 📄 NodeEditorTab.xaml                      # タブUI
│   │   └── 📄 NodeEditorTab.xaml.cs
│   │
│   ├── 📄 BindingProxy.cs                             # XAMLのバインディング補助
│   ├── 📄 ColorPickerWindow.xaml                      # 色編集ダイアログ(View)
│   ├── 📄 ColorPickerWindow.xaml.cs                   # 色編集ダイアログのコードビハインド
│   ├── 📄 DateTimeEditorWindow.xaml                   # 日時編集ダイアログ(View)
│   ├── 📄 DateTimeEditorWindow.xaml.cs                # 日時編集ダイアログのコードビハインド
│   ├── 📄 GanttChartView.xaml                         # ガントチャートView
│   ├── 📄 GanttChartView.xaml.cs                      # ガントチャートViewのコードビハインド
│   ├── 📄 ProjectView.xaml                            # プロジェクト単体情報
│   ├── 📄 ProjectView.xaml.cs
│   ├── 📄 TeamProjectsView.xaml                       # チーム内プロジェクト情報(複数のプロジェクトの管理用View)
│   ├── 📄 TeamProjectsView.xaml.cs
│   ├── 📄 TeamSettingsView.xaml                       # チーム設定View
│   ├── 📄 TeamSettingsView.xaml.cs
│   ├── 📄 ThemeSettingWindow.xaml                     # テーマ編集ウィンドウ(View)
│   └── 📄 ThemeSettingWindow.xaml.cs                  # テーマ編集ウィンドウのコードビハインド
│
├── 📄 App.xaml                                        # アプリケーション定義
├── 📄 App.xaml.cs                                     # アプリ起動ロジック
├── 📄 AssemblyInfo.cs                                 # アセンブリ情報
├── 📄 MainApplication.csproj                          # プロジェクトファイル
├── 📄 MainWindow.xaml                                 # メインウィンドウ
└── 📄 MainWindow.xaml.cs                              # メインウィンドウのコードビハインド
```

## タスクエディタ仕様

タスクエディタの構成、主要クラス、依存関係図は以下にまとめています。

- [タスクエディタ仕様](documentation/task_editor_spec.md)

## メンバー管理仕様

メンバー管理、タスク担当者、作業協力者、日次作業可能時間設定に関する仕様は以下にまとめています。

- [メンバー管理仕様](documentation/member_manager.md)

## ガントチャート仕様

プロジェクトスケジュール、チームスケジュール、予定期間算定、依存関係表示に関する仕様は以下にまとめています。

- [ガントチャート仕様](documentation/gantt_chart.md)

## ステータスバー仕様

アプリケーション全体の状態表示、処理進捗、キュー表示、プロジェクト進捗表示に関する仕様は以下にまとめています。

- [ステータスバー仕様](documentation/status_bar.md)

## 保存データ仕様

保存ファイルのJSON構造は以下にまとめています。

- [保存データ仕様](documentation/save_data.md)
