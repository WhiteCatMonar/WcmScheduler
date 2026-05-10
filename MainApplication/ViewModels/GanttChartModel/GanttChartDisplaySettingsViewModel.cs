using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;
using MainApplication.ViewModels.TeamModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャートのフィルタと並び替え状態を管理するViewModel
    /// </summary>
    public class GanttChartDisplaySettingsViewModel : ViewModelBase
    {
        private readonly Action _settingsChanged;
        private bool _isRefreshingOptions;
        private GanttMemberFilterItemViewModel? _selectedAssigneeFilterCandidate;
        private GanttMemberFilterItemViewModel? _selectedCollaboratorFilterCandidate;
        private GanttSortKey _selectedSortKey = GanttSortKey.Dependency;

        /// <summary>
        /// ガントチャート表示設定ViewModelを生成する
        /// </summary>
        /// <param name="settingsChanged">設定変更時処理</param>
        public GanttChartDisplaySettingsViewModel(Action settingsChanged)
        {
            _settingsChanged = settingsChanged;
            SortOptions =
            [
                new(GanttSortKey.Dependency, "依存順"),
                new(GanttSortKey.TaskName, "タスク名"),
                new(GanttSortKey.StartDateTime, "開始日時"),
                new(GanttSortKey.EndDateTime, "終了日時")
            ];
            AddAssigneeFilterCommand = new RelayCommand(AddAssigneeFilter);
            AddCollaboratorFilterCommand = new RelayCommand(AddCollaboratorFilter);
            RemoveAssigneeFilterCommand = new RelayCommand<GanttMemberFilterItemViewModel>(RemoveAssigneeFilter, item => item != null);
            RemoveCollaboratorFilterCommand = new RelayCommand<GanttMemberFilterItemViewModel>(RemoveCollaboratorFilter, item => item != null);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            LoadFromAppSettings();
        }

        /// <summary>
        /// 担当者フィルタ項目
        /// </summary>
        public ObservableCollection<GanttMemberFilterItemViewModel> AssigneeFilterItems { get; } = [];

        /// <summary>
        /// 担当者フィルタ候補項目
        /// </summary>
        public ObservableCollection<GanttMemberFilterItemViewModel> AvailableAssigneeFilterItems { get; } = [];

        /// <summary>
        /// 選択済み担当者フィルタ項目
        /// </summary>
        public ObservableCollection<GanttMemberFilterItemViewModel> SelectedAssigneeFilterItems { get; } = [];

        /// <summary>
        /// 作業協力者フィルタ項目
        /// </summary>
        public ObservableCollection<GanttMemberFilterItemViewModel> CollaboratorFilterItems { get; } = [];

        /// <summary>
        /// 作業協力者フィルタ候補項目
        /// </summary>
        public ObservableCollection<GanttMemberFilterItemViewModel> AvailableCollaboratorFilterItems { get; } = [];

        /// <summary>
        /// 選択済み作業協力者フィルタ項目
        /// </summary>
        public ObservableCollection<GanttMemberFilterItemViewModel> SelectedCollaboratorFilterItems { get; } = [];

        /// <summary>
        /// ステータスフィルタ項目
        /// </summary>
        public ObservableCollection<GanttStatusFilterItemViewModel> StatusFilterItems { get; } = [];

        /// <summary>
        /// 並び替え選択肢
        /// </summary>
        public ObservableCollection<GanttSortOptionViewModel> SortOptions { get; }

        /// <summary>
        /// 担当者フィルタ追加コマンド
        /// </summary>
        public ICommand AddAssigneeFilterCommand { get; }

        /// <summary>
        /// 作業協力者フィルタ追加コマンド
        /// </summary>
        public ICommand AddCollaboratorFilterCommand { get; }

        /// <summary>
        /// 担当者フィルタ削除コマンド
        /// </summary>
        public ICommand RemoveAssigneeFilterCommand { get; }

        /// <summary>
        /// 作業協力者フィルタ削除コマンド
        /// </summary>
        public ICommand RemoveCollaboratorFilterCommand { get; }

        /// <summary>
        /// フィルタ解除コマンド
        /// </summary>
        public ICommand ClearFiltersCommand { get; }

        /// <summary>
        /// 追加対象担当者フィルタ候補
        /// </summary>
        public GanttMemberFilterItemViewModel? SelectedAssigneeFilterCandidate
        {
            get => _selectedAssigneeFilterCandidate;
            set => SetProperty(ref _selectedAssigneeFilterCandidate, value);
        }

        /// <summary>
        /// 追加対象作業協力者フィルタ候補
        /// </summary>
        public GanttMemberFilterItemViewModel? SelectedCollaboratorFilterCandidate
        {
            get => _selectedCollaboratorFilterCandidate;
            set => SetProperty(ref _selectedCollaboratorFilterCandidate, value);
        }

        /// <summary>
        /// 選択中の並び替えキー
        /// </summary>
        public GanttSortKey SelectedSortKey
        {
            get => _selectedSortKey;
            set
            {
                if (SetProperty(ref _selectedSortKey, value))
                {
                    OnSettingsChanged();
                }
            }
        }

        /// <summary>
        /// 表示メンバー一覧に応じてフィルタ項目を更新する
        /// </summary>
        /// <param name="members">チームメンバー一覧</param>
        public void RefreshMemberOptions(IEnumerable<TeamMemberViewModel>? members)
        {
            _isRefreshingOptions = true;
            try
            {
                var memberList = members?.ToList() ?? [];
                var selectedAssignees = AssigneeFilterItems
                    .Where(item => item.IsSelected)
                    .Select(item => item.MemberId)
                    .Concat(AppSettingsManager.Current.GanttChart.AssigneeMemberIds)
                    .ToHashSet();
                var selectedCollaborators = CollaboratorFilterItems
                    .Where(item => item.IsSelected)
                    .Select(item => item.MemberId)
                    .Concat(AppSettingsManager.Current.GanttChart.CollaboratorMemberIds)
                    .ToHashSet();

                AssigneeFilterItems.Clear();
                CollaboratorFilterItems.Clear();
                foreach (var member in memberList)
                {
                    AssigneeFilterItems.Add(new(member.MemberId, member.DisplayText, selectedAssignees.Contains(member.MemberId), OnMemberFilterItemSelectionChanged));
                    CollaboratorFilterItems.Add(new(member.MemberId, member.DisplayText, selectedCollaborators.Contains(member.MemberId), OnMemberFilterItemSelectionChanged));
                }

                RefreshMemberDisplayItems();
            }
            finally
            {
                _isRefreshingOptions = false;
            }
        }

        /// <summary>
        /// ステータスフィルタ項目を更新する
        /// </summary>
        public void RefreshStatusOptions()
        {
            if (StatusFilterItems.Count > 0)
            {
                return;
            }

            var selectedStatuses = AppSettingsManager.Current.GanttChart.Statuses
                .Select(ParseStatus)
                .Where(status => status != null)
                .Select(status => status!.Value)
                .ToHashSet();
            foreach (var status in Enum.GetValues<TaskNodeViewModel.TaskStatus>())
            {
                StatusFilterItems.Add(new(status, status.ToString(), selectedStatuses.Contains(status), OnSettingsChanged));
            }
        }

        /// <summary>
        /// 指定ノードがフィルタ条件を満たすかを判定する
        /// </summary>
        /// <param name="node">判定対象ノード</param>
        /// <returns>表示対象の場合はtrue</returns>
        public bool IsTaskVisible(TaskNodeViewModel node)
        {
            var selectedAssignees = AssigneeFilterItems
                .Where(item => item.IsSelected)
                .Select(item => item.MemberId)
                .ToHashSet();
            if (selectedAssignees.Count > 0 &&
                (node.Detail.AssigneeMemberId == null || !selectedAssignees.Contains(node.Detail.AssigneeMemberId.Value)))
            {
                return false;
            }

            var selectedCollaborators = CollaboratorFilterItems
                .Where(item => item.IsSelected)
                .Select(item => item.MemberId)
                .ToHashSet();
            if (selectedCollaborators.Count > 0 &&
                !node.Detail.CollaboratorMemberIds.Any(selectedCollaborators.Contains))
            {
                return false;
            }

            var selectedStatuses = StatusFilterItems
                .Where(item => item.IsSelected)
                .Select(item => item.Status)
                .ToHashSet();
            return selectedStatuses.Count == 0 || selectedStatuses.Contains(node.Status);
        }

        /// <summary>
        /// フィルタ状態を解除する
        /// </summary>
        public void ClearFilters()
        {
            _isRefreshingOptions = true;
            try
            {
                SetFilterItems(AssigneeFilterItems, false);
                SetFilterItems(CollaboratorFilterItems, false);
                SetFilterItems(StatusFilterItems, false);
                RefreshMemberDisplayItems();
            }
            finally
            {
                _isRefreshingOptions = false;
            }

            OnSettingsChanged();
        }

        /// <summary>
        /// 担当者フィルタを追加する
        /// </summary>
        private void AddAssigneeFilter()
        {
            if (SelectedAssigneeFilterCandidate == null)
            {
                return;
            }

            SelectedAssigneeFilterCandidate.IsSelected = true;
            SelectedAssigneeFilterCandidate = null;
        }

        /// <summary>
        /// 作業協力者フィルタを追加する
        /// </summary>
        private void AddCollaboratorFilter()
        {
            if (SelectedCollaboratorFilterCandidate == null)
            {
                return;
            }

            SelectedCollaboratorFilterCandidate.IsSelected = true;
            SelectedCollaboratorFilterCandidate = null;
        }

        /// <summary>
        /// 担当者フィルタを削除する
        /// </summary>
        /// <param name="item">削除対象項目</param>
        private static void RemoveAssigneeFilter(GanttMemberFilterItemViewModel? item)
        {
            if (item != null)
            {
                item.IsSelected = false;
            }
        }

        /// <summary>
        /// 作業協力者フィルタを削除する
        /// </summary>
        /// <param name="item">削除対象項目</param>
        private static void RemoveCollaboratorFilter(GanttMemberFilterItemViewModel? item)
        {
            if (item != null)
            {
                item.IsSelected = false;
            }
        }

        /// <summary>
        /// メンバーフィルタ選択変更時に表示項目と保存状態を更新する
        /// </summary>
        private void OnMemberFilterItemSelectionChanged()
        {
            if (_isRefreshingOptions)
            {
                return;
            }

            RefreshMemberDisplayItems();
            OnSettingsChanged();
        }

        /// <summary>
        /// メンバーフィルタの候補項目と選択済み項目を更新する
        /// </summary>
        private void RefreshMemberDisplayItems()
        {
            RefreshMemberDisplayItems(AssigneeFilterItems, AvailableAssigneeFilterItems, SelectedAssigneeFilterItems);
            RefreshMemberDisplayItems(CollaboratorFilterItems, AvailableCollaboratorFilterItems, SelectedCollaboratorFilterItems);

            if (SelectedAssigneeFilterCandidate?.IsSelected == true)
            {
                SelectedAssigneeFilterCandidate = null;
            }

            if (SelectedCollaboratorFilterCandidate?.IsSelected == true)
            {
                SelectedCollaboratorFilterCandidate = null;
            }
        }

        /// <summary>
        /// メンバーフィルタの表示用コレクションを更新する
        /// </summary>
        /// <param name="source">元項目一覧</param>
        /// <param name="availableItems">候補項目一覧</param>
        /// <param name="selectedItems">選択済み項目一覧</param>
        private static void RefreshMemberDisplayItems(
            IEnumerable<GanttMemberFilterItemViewModel> source,
            ObservableCollection<GanttMemberFilterItemViewModel> availableItems,
            ObservableCollection<GanttMemberFilterItemViewModel> selectedItems
        )
        {
            availableItems.Clear();
            selectedItems.Clear();
            foreach (var item in source)
            {
                if (item.IsSelected)
                {
                    selectedItems.Add(item);
                }
                else
                {
                    availableItems.Add(item);
                }
            }
        }

        /// <summary>
        /// 設定変更時に保存と表示更新を行う
        /// </summary>
        private void OnSettingsChanged()
        {
            if (_isRefreshingOptions)
            {
                return;
            }

            SaveToAppSettings();
            _settingsChanged();
        }

        /// <summary>
        /// アプリケーション設定から表示状態を読み込む
        /// </summary>
        private void LoadFromAppSettings()
        {
            if (Enum.TryParse(AppSettingsManager.Current.GanttChart.SortKey, out GanttSortKey sortKey))
            {
                _selectedSortKey = sortKey;
            }
        }

        /// <summary>
        /// アプリケーション設定へ表示状態を保存する
        /// </summary>
        private void SaveToAppSettings()
        {
            AppSettingsManager.Current.GanttChart.AssigneeMemberIds = AssigneeFilterItems
                .Where(item => item.IsSelected)
                .Select(item => item.MemberId)
                .ToList();
            AppSettingsManager.Current.GanttChart.CollaboratorMemberIds = CollaboratorFilterItems
                .Where(item => item.IsSelected)
                .Select(item => item.MemberId)
                .ToList();
            AppSettingsManager.Current.GanttChart.Statuses = StatusFilterItems
                .Where(item => item.IsSelected)
                .Select(item => item.Status.ToString())
                .ToList();
            AppSettingsManager.Current.GanttChart.SortKey = SelectedSortKey.ToString();
            AppSettingsManager.Save();
        }

        /// <summary>
        /// 保存済みステータス名をステータス値へ変換する
        /// </summary>
        /// <param name="value">ステータス名</param>
        /// <returns>ステータス値</returns>
        private static TaskNodeViewModel.TaskStatus? ParseStatus(string value)
        {
            return Enum.TryParse(value, out TaskNodeViewModel.TaskStatus status) ? status : null;
        }

        /// <summary>
        /// メンバーフィルタ項目の選択状態を一括設定する
        /// </summary>
        /// <param name="items">対象項目</param>
        /// <param name="isSelected">選択状態</param>
        private static void SetFilterItems(IEnumerable<GanttMemberFilterItemViewModel> items, bool isSelected)
        {
            foreach (var item in items)
            {
                item.IsSelected = isSelected;
            }
        }

        /// <summary>
        /// ステータスフィルタ項目の選択状態を一括設定する
        /// </summary>
        /// <param name="items">対象項目</param>
        /// <param name="isSelected">選択状態</param>
        private static void SetFilterItems(IEnumerable<GanttStatusFilterItemViewModel> items, bool isSelected)
        {
            foreach (var item in items)
            {
                item.IsSelected = isSelected;
            }
        }
    }
}

/* --- End of file --- */
