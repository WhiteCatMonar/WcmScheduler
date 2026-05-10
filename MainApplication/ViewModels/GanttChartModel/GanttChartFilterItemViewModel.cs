using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.DependencyEditorModel;

namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャートのメンバーフィルタ項目を表すViewModel
    /// </summary>
    public class GanttMemberFilterItemViewModel : ViewModelBase
    {
        private readonly Action _selectionChanged;
        private bool _isSelected;

        /// <summary>
        /// メンバーフィルタ項目を生成する
        /// </summary>
        /// <param name="memberId">メンバーID</param>
        /// <param name="displayText">表示文字列</param>
        /// <param name="isSelected">選択状態</param>
        /// <param name="selectionChanged">選択変更時処理</param>
        public GanttMemberFilterItemViewModel(Guid memberId, string displayText, bool isSelected, Action selectionChanged)
        {
            MemberId = memberId;
            DisplayText = displayText;
            _isSelected = isSelected;
            _selectionChanged = selectionChanged;
        }

        /// <summary>
        /// メンバーID
        /// </summary>
        public Guid MemberId { get; }

        /// <summary>
        /// 表示文字列
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// フィルタ項目が選択されているか
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    _selectionChanged();
                }
            }
        }
    }

    /// <summary>
    /// ガントチャートのステータスフィルタ項目を表すViewModel
    /// </summary>
    public class GanttStatusFilterItemViewModel : ViewModelBase
    {
        private readonly Action _selectionChanged;
        private bool _isSelected;

        /// <summary>
        /// ステータスフィルタ項目を生成する
        /// </summary>
        /// <param name="status">対象ステータス</param>
        /// <param name="displayText">表示文字列</param>
        /// <param name="isSelected">選択状態</param>
        /// <param name="selectionChanged">選択変更時処理</param>
        public GanttStatusFilterItemViewModel(
            TaskNodeViewModel.TaskStatus status,
            string displayText,
            bool isSelected,
            Action selectionChanged
        )
        {
            Status = status;
            DisplayText = displayText;
            _isSelected = isSelected;
            _selectionChanged = selectionChanged;
        }

        /// <summary>
        /// 対象ステータス
        /// </summary>
        public TaskNodeViewModel.TaskStatus Status { get; }

        /// <summary>
        /// 表示文字列
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// フィルタ項目が選択されているか
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    _selectionChanged();
                }
            }
        }
    }
}

/* --- End of file --- */
