using MainApplication.Infrastructure;
using MainApplication.Models.SaveData;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.SettingsModel;
using MainApplication.ViewModels.TeamModel;
using MainApplication.ViewModels.ThemeModel;
using MainApplication.Views;
using System.Collections.ObjectModel;
using System.IO;
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
        private string? _lastAutoSavedSnapshotJson;
        private bool _isDirty;

        /// <summary>ファイル読み込みコマンド</summary>
        public ICommand LoadCommand { get; }

        /// <summary>上書き保存コマンド</summary>
        public ICommand SaveCommand { get; }

        /// <summary>別名保存コマンド</summary>
        public ICommand SaveAsCommand { get; }

        private string? _currentFilePath;
        private string? _currentEditFilePath;

        /// <summary>
        /// 現在読み込んでいる保存ファイルパス。
        /// </summary>
        public string? CurrentFilePath => _currentFilePath;

        /// <summary>
        /// 現在使用している編集作業ファイルパス。
        /// </summary>
        public string? CurrentEditFilePath => _currentEditFilePath;

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

        /// <summary>アプリケーション設定編集コマンド</summary>
        public ICommand OpenApplicationSettingsCommand { get; }

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
            OpenApplicationSettingsCommand = new RelayCommand(OpenApplicationSettings);

            _savedSnapshotJson = CreateCurrentSnapshot();
            _dirtyRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _dirtyRefreshTimer.Tick += (sender, args) => RefreshDirtyStateAndAutoSave();
            _dirtyRefreshTimer.Start();
        }

        /// <summary>
        /// アプリケーション設定ウィンドウを開く。
        /// </summary>
        private void OpenApplicationSettings()
        {
            var viewModel = new ApplicationSettingsViewModel(AppSettingsManager.Current);
            var window = new ApplicationSettingsWindow
            {
                DataContext = viewModel,
                Owner = System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(window => window.IsActive)
            };

            window.ShowDialog();
        }

        /* ---------------------------------------------------------
         * 読み込み処理
         * --------------------------------------------------------- */
        
        /// <summary>
        /// 指定ファイルからデータを読み込み、ViewModelに適用する。
        /// </summary>
        /// <param name="path">読み込み対象の正式保存ファイルパス。</param>
        /// <param name="restoreEditFile">既存の編集作業ファイルを復元するかどうか。</param>
        /// <returns>読み込みに成功した場合はtrue。</returns>
        public bool LoadFromFile(string path, bool restoreEditFile = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            try
            {
                var formalPath = GetFormalFilePath(path);
                var editPath = GetEditFilePath(formalPath);
                var formalRoot = LoadRootDataModel(formalPath);
                if (formalRoot == null)
                {
                    return false;
                }

                RootSaveDataModel? root;
                if (restoreEditFile && File.Exists(editPath))
                {
                    root = LoadRootDataModel(editPath);
                    if (root == null)
                    {
                        File.Copy(formalPath, editPath, true);
                        root = formalRoot;
                    }
                }
                else
                {
                    File.Copy(formalPath, editPath, true);
                    root = formalRoot;
                }

                ApplyRootDataModel(root);

                _currentFilePath = formalPath;
                _currentEditFilePath = editPath;
                _savedSnapshotJson = _jsonSerializer.Serialize(formalRoot);
                _lastAutoSavedSnapshotJson = CreateCurrentSnapshot();
                SaveCurrentSnapshotToEditFile(_lastAutoSavedSnapshotJson);
                OnPropertyChangedA(nameof(CurrentFilePath));
                OnPropertyChangedA(nameof(CurrentEditFilePath));
                RefreshDirtyState();
                return true;
            }
            catch
            {
                return false;
            }
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

            var formalPath = GetFormalFilePath(path);
            _currentFilePath = formalPath;
            _currentEditFilePath = GetEditFilePath(formalPath);
            OnPropertyChangedA(nameof(CurrentFilePath));
            OnPropertyChangedA(nameof(CurrentEditFilePath));

            if (!SaveToFile(formalPath))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 実際の保存処理(ファイル書き込み)
        /// </summary>
        private bool SaveToFile(string path)
        {
            try
            {
                var formalPath = GetFormalFilePath(path);
                var editPath = _currentEditFilePath ?? GetEditFilePath(formalPath);
                var json = CreateCurrentSnapshot();
                SaveCurrentSnapshotToEditFile(json);
                CreateBackupFiles(formalPath);
                if (File.Exists(formalPath))
                {
                    File.Delete(formalPath);
                }

                File.Move(editPath, formalPath);
                File.Copy(formalPath, editPath, true);
                _currentFilePath = formalPath;
                _currentEditFilePath = editPath;
                _savedSnapshotJson = json;
                _lastAutoSavedSnapshotJson = json;
                OnPropertyChangedA(nameof(CurrentFilePath));
                OnPropertyChangedA(nameof(CurrentEditFilePath));
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
        /// ダーティ状態を更新し、編集作業ファイルへ自動保存する。
        /// </summary>
        public void RefreshDirtyStateAndAutoSave()
        {
            var currentSnapshot = CreateCurrentSnapshot();
            IsDirty = currentSnapshot != _savedSnapshotJson;
            if (string.IsNullOrEmpty(_currentEditFilePath))
            {
                return;
            }

            if (currentSnapshot == _lastAutoSavedSnapshotJson)
            {
                return;
            }

            SaveCurrentSnapshotToEditFile(currentSnapshot);
            _lastAutoSavedSnapshotJson = currentSnapshot;
        }

        /// <summary>
        /// 非ダーティ状態の場合に編集作業ファイルを削除する。
        /// </summary>
        public void DeleteEditFileIfClean()
        {
            RefreshDirtyState();
            if (IsDirty)
            {
                return;
            }

            DeleteEditFile();
        }

        /// <summary>
        /// 編集作業ファイルを破棄する。
        /// </summary>
        public void DiscardEditFile()
        {
            DeleteEditFile();
            _lastAutoSavedSnapshotJson = _savedSnapshotJson;
            RefreshDirtyState();
        }

        /// <summary>
        /// 指定保存ファイルに対応する編集作業ファイルが存在するかどうかを取得する。
        /// </summary>
        /// <param name="path">正式保存ファイルパス。</param>
        /// <returns>編集作業ファイルが存在する場合はtrue。</returns>
        public bool HasEditFile(string path)
        {
            return File.Exists(GetEditFilePath(GetFormalFilePath(path)));
        }

        /// <summary>
        /// 指定保存ファイルに対応する編集作業ファイルのパスを取得する。
        /// </summary>
        /// <param name="path">正式保存ファイルパス。</param>
        /// <returns>編集作業ファイルパス。</returns>
        public string GetEditFilePathFor(string path)
        {
            return GetEditFilePath(GetFormalFilePath(path));
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

        /// <summary>
        /// 指定ファイルから保存データモデルを読み込む。
        /// </summary>
        /// <param name="path">読み込み元パス。</param>
        /// <returns>保存データモデル。</returns>
        private RootSaveDataModel? LoadRootDataModel(string path)
        {
            var json = _fileService.LoadText(path);
            return _jsonSerializer.Deserialize<RootSaveDataModel>(json);
        }

        /// <summary>
        /// 現在のスナップショットを編集作業ファイルへ保存する。
        /// </summary>
        /// <param name="json">保存データJSON。</param>
        private void SaveCurrentSnapshotToEditFile(string json)
        {
            if (string.IsNullOrEmpty(_currentEditFilePath))
            {
                return;
            }

            var directory = Path.GetDirectoryName(_currentEditFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _fileService.SaveText(_currentEditFilePath, json);
        }

        /// <summary>
        /// 編集作業ファイルを削除する。
        /// </summary>
        private void DeleteEditFile()
        {
            if (string.IsNullOrEmpty(_currentEditFilePath))
            {
                return;
            }

            if (File.Exists(_currentEditFilePath))
            {
                File.Delete(_currentEditFilePath);
            }
        }

        /// <summary>
        /// 正式保存ファイルパスを取得する。
        /// </summary>
        /// <param name="path">保存ファイルパス。</param>
        /// <returns>正式保存ファイルパス。</returns>
        private static string GetFormalFilePath(string path)
        {
            var directory = Path.GetDirectoryName(path) ?? string.Empty;
            var fileName = Path.GetFileName(path);
            if (fileName.EndsWith(".edit.json", StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName[..^".edit.json".Length] + ".json";
            }

            return Path.Combine(directory, fileName);
        }

        /// <summary>
        /// 編集作業ファイルパスを取得する。
        /// </summary>
        /// <param name="formalPath">正式保存ファイルパス。</param>
        /// <returns>編集作業ファイルパス。</returns>
        private static string GetEditFilePath(string formalPath)
        {
            var directory = Path.GetDirectoryName(formalPath) ?? string.Empty;
            var fileName = Path.GetFileNameWithoutExtension(formalPath);
            return Path.Combine(directory, $"{fileName}.edit.json");
        }

        /// <summary>
        /// バックアップファイルを作成する。
        /// </summary>
        /// <param name="formalPath">正式保存ファイルパス。</param>
        private static void CreateBackupFiles(string formalPath)
        {
            if (!File.Exists(formalPath))
            {
                return;
            }

            var generationCount = Math.Max(0, AppSettingsManager.Current.AutoBackupGenerationCount);
            if (generationCount <= 0)
            {
                return;
            }

            for (var generation = generationCount - 1; generation >= 1; generation--)
            {
                var source = GetBackupFilePath(formalPath, generation - 1);
                var destination = GetBackupFilePath(formalPath, generation);
                if (!File.Exists(source))
                {
                    continue;
                }

                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }

                File.Move(source, destination);
            }

            var latestBackupPath = GetBackupFilePath(formalPath, 0);
            if (File.Exists(latestBackupPath))
            {
                File.Delete(latestBackupPath);
            }

            File.Move(formalPath, latestBackupPath);
        }

        /// <summary>
        /// バックアップファイルパスを取得する。
        /// </summary>
        /// <param name="formalPath">正式保存ファイルパス。</param>
        /// <param name="generation">世代番号。</param>
        /// <returns>バックアップファイルパス。</returns>
        private static string GetBackupFilePath(string formalPath, int generation)
        {
            var directory = Path.GetDirectoryName(formalPath) ?? string.Empty;
            var fileName = Path.GetFileNameWithoutExtension(formalPath);
            var suffix = generation == 0 ? "backup" : $"backup.{generation}";
            return Path.Combine(directory, $"{fileName}.{suffix}.json");
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
