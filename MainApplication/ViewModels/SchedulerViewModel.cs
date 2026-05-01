using MainApplication.Infrastructure;
using MainApplication.Models.SaveData;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.TeamModel;
using MainApplication.ViewModels.ThemeModel;
using MainApplication.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// アプリケーション全体の状態を管理するViewModel。
    /// タブ管理、保存・読み込み、子ViewModelの生成などを担当する。
    /// </summary>
    public class SchedulerViewModel: ViewModelBase
    {
        /* ---------------------------------------------------------
         * チーム内プロジェクト管理
         * --------------------------------------------------------- */

        public TeamProjectsViewModel TeamProjects { get; }

        /// <summary>
        /// チーム設定。
        /// </summary>
        public TeamSettingsViewModel TeamSettings { get; }

        /* ---------------------------------------------------------
         * 保存・読み込み関連定義
         * --------------------------------------------------------- */

        private readonly IJsonSerializerService _jsonSerializer;
        private readonly IFileService _fileService;
        private readonly DispatcherTimer _dirtyRefreshTimer;
        private string? _savedSnapshotJson;
        private bool _isDirty;

        /// <summary>ファイル読み込みコマンド</summary>
        public ICommand LoadCommand { get; }

        /// <summary>上書き保存コマンド</summary>
        public ICommand SaveCommand { get; }

        /// <summary>別名保存コマンド</summary>
        public ICommand SaveAsCommand { get; }

        private string? _currentFilePath;

        /// <summary>
        /// 現在読み込んでいる保存ファイルパス。
        /// </summary>
        public string? CurrentFilePath => _currentFilePath;

        /// <summary>
        /// 現在の状態が保存済みスナップショットから変更されているかどうか。
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty;
            private set => SetProperty(ref _isDirty, value, [nameof(WindowTitle)]);
        }

        /// <summary>
        /// メインウィンドウタイトル。
        /// </summary>
        public string WindowTitle => IsDirty ? "WcmScheduler *" : "WcmScheduler";

        /* ---------------------------------------------------------
         * テーマ管理
         * --------------------------------------------------------- */

        /// <summary>テーマ関連メニュー一覧</summary>
        public ObservableCollection<ThemeMenuItemViewModel> ThemeMenuItems { get; set; }

        /// <summary>テーマ編集コマンド</summary>
        public ICommand OpenThemeEditorCommand { get; }

        public void RefreshThemeMenuItems()
        {
            ThemeMenuItems.Clear();

            foreach (var theme in ThemeManager.LoadedThemes)
            {
                ThemeMenuItems.Add(
                    CreateThemeMenuItem(theme)
                );
            }
        }

        /// <summary>
        /// テーマ一覧メニュー項目を生成する。
        /// </summary>
        /// <param name="theme">対象テーマ。</param>
        /// <returns>テーマメニュー項目。</returns>
        private ThemeMenuItemViewModel CreateThemeMenuItem(Models.Settings.ThemeSettingModel theme)
        {
            return new ThemeMenuItemViewModel(
                theme.Name,
                ThemeManager.CurrentTheme.Name == theme.Name,
                () =>
                {
                    ThemeManager.ApplyTheme(theme);
                    RefreshThemeMenuItems();
                }
            );
        }

        /* ---------------------------------------------------------
         * タブ管理
         * --------------------------------------------------------- */

        /// <summary>
        /// 表示中のタブ一覧
        /// </summary>
        public ObservableCollection<object> Tabs { get; }

        private object? _selectedTab;

        /// <summary>
        /// 現在選択されているタブ
        /// </summary>
        public object? SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// SchedulerViewModelを生成し、子ViewModelやサービスを初期化する。
        /// </summary>
        public SchedulerViewModel()
        {
            /* サービス */
            _jsonSerializer = new JsonSerializerService();
            _fileService = new FileService();

            /* 子となるViewModelの生成 */
            var teamMembers = new ObservableCollection<TeamMemberViewModel>();
            var specialHolidays = new ObservableCollection<DateOnly>();
            TeamProjects = new TeamProjectsViewModel(teamMembers, specialHolidays);
            TeamSettings = new TeamSettingsViewModel(TeamProjects, teamMembers, specialHolidays);
            /* TODO:タブごとの機能追加 */

            /* タブ管理 */
            var teamTab = new TabInfo("チーム内プロジェクト", TeamProjects);
            var teamSettingsTab = new TabInfo("チーム設定", TeamSettings);
            Tabs = new ObservableCollection<object>
            {
                teamTab,
                teamSettingsTab
                /* TODO:タブごとの機能追加 */
            };

            SelectedTab = teamTab;

            /* コマンド */
            LoadCommand = new RelayCommand(() => RequestLoad?.Invoke());
            SaveCommand = new RelayCommand(() => Save());
            SaveAsCommand = new RelayCommand(() => RequestSaveAs?.Invoke());
            ThemeMenuItems = new ObservableCollection<ThemeMenuItemViewModel>(
                ThemeManager.LoadedThemes.Select(CreateThemeMenuItem)
            );
            OpenThemeEditorCommand = new RelayCommand(() =>
            {
                var vm = new ThemeSettingViewModel(ThemeManager.CurrentTheme);
                vm.ThemeSaved += () =>
                {
                    RefreshThemeMenuItems();
                };
                var win = new ThemeSettingWindow { DataContext = vm };
                win.ShowDialog();
            });

            _savedSnapshotJson = CreateCurrentSnapshot();
            _dirtyRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _dirtyRefreshTimer.Tick += (sender, args) => RefreshDirtyState();
            _dirtyRefreshTimer.Start();
        }

        /* ---------------------------------------------------------
         * 読み込み処理
         * --------------------------------------------------------- */
        
        /// <summary>
        /// 指定ファイルからデータを読み込み、ViewModelに適用する。
        /// </summary>
        public void LoadFromFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            var json = _fileService.LoadText(path);
            var root = _jsonSerializer.Deserialize<RootSaveDataModel>(json);

            if (root == null)
                return;

            ApplyRootDataModel(root);

            _currentFilePath = path;
            _savedSnapshotJson = CreateCurrentSnapshot();
            RefreshDirtyState();
        }

        /// <summary>
        /// RootSaveDataModelを各ViewModelに適用する。
        /// </summary>
        private void ApplyRootDataModel(RootSaveDataModel root)
        {
            TeamProjects.LoadFromDataModels(root.Projects);
            TeamSettings.LoadFromDataModels(root.Members, root.Projects, root.SpecialHolidays);
        }


        /* ---------------------------------------------------------
         * 保存処理
         * --------------------------------------------------------- */

        /// <summary>
        /// 現在のファイルに上書き保存する。
        /// パスが未設定の場合はSaveAsを要求する。
        /// </summary>
        public bool Save()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                RequestSaveAs?.Invoke();
                return false;
            }

            return SaveToFile(_currentFilePath);
        }

        /// <summary>
        /// 指定パスに保存する。
        /// </summary>
        public bool SaveAs(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!SaveToFile(path))
            {
                return false;
            }

            _currentFilePath = path;
            OnPropertyChangedA(nameof(CurrentFilePath));
            return true;
        }

        /// <summary>
        /// 実際の保存処理(ファイル書き込み)
        /// </summary>
        private bool SaveToFile(string path)
        {
            try
            {
                var root = ToRootDataModel();
                var json = _jsonSerializer.Serialize(root);
                _fileService.SaveText(path, json);
                _savedSnapshotJson = json;
                RefreshDirtyState();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 現在の保存データと保存済みスナップショットを比較し、ダーティ状態を更新する。
        /// </summary>
        public void RefreshDirtyState()
        {
            var currentSnapshot = CreateCurrentSnapshot();
            IsDirty = currentSnapshot != _savedSnapshotJson;
        }

        /// <summary>
        /// 現在の保存データスナップショットを作成する。
        /// </summary>
        /// <returns>保存データJSON。</returns>
        private string CreateCurrentSnapshot()
        {
            var root = ToRootDataModel();
            return _jsonSerializer.Serialize(root);
        }

        /* ---------------------------------------------------------
         * RootSaveDataの構築
         * --------------------------------------------------------- */

        /// <summary>
        /// 現在の状態をRootSaveDataModelに変換する。
        /// </summary>
        private RootSaveDataModel ToRootDataModel()
        {
            RootSaveDataModel save_data = new()
            {
                Members = TeamSettings.ToMemberDataModels(),
                SpecialHolidays = TeamSettings.ToSpecialHolidayDataModels(),
                Projects =
                [
                    ..
                    TeamProjects.Projects.Select(project =>
                    {
                        project.NodeEditor.SaveToTaskEditorDataModel(out var taskEditor);
                        return new ProjectDataModel
                        {
                            ProjectId = project.ProjectId,
                            ProjectName = project.ProjectName,
                            TaskEditor = taskEditor,
                            MemberInfo = TeamSettings.ToProjectMemberInfoDataModels(project.ProjectId)
                        };
                    })
                ]
            };

            return save_data;
        }


        /* ---------------------------------------------------------
         * ViewModel → Viewへの依頼イベント
         * --------------------------------------------------------- */
        
        /// <summary>
        /// Viewに「ファイルを開くダイアログを表示してほしい」と依頼するイベント
        /// </summary>
        public event Action? RequestLoad;

        /// <summary>
        /// Viewに「名前を付けて保存ダイアログを表示してほしい」と依頼するイベント
        /// </summary>
        public event Action? RequestSaveAs;
    }
}

/* --- End of file --- */
