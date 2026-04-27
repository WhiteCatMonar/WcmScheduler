using MainApplication.ViewModels.Core;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// 作業協力者の選択状態を表すViewModel。
    /// </summary>
    public class CollaboratorOptionViewModel : ViewModelBase
    {
        private readonly Action<CollaboratorOptionViewModel, bool> _selectionChanged;
        private bool _isSelected;

        /// <summary>
        /// 作業協力者の選択肢を生成する。
        /// </summary>
        /// <param name="member">対象メンバー。</param>
        /// <param name="isSelected">選択済みかどうか。</param>
        /// <param name="selectionChanged">選択状態変更時の処理。</param>
        public CollaboratorOptionViewModel(
            TeamMemberViewModel member,
            bool isSelected,
            Action<CollaboratorOptionViewModel, bool> selectionChanged
        )
        {
            MemberId = member.MemberId;
            DisplayName = member.DisplayText;
            IsActive = member.IsActive;
            _isSelected = isSelected;
            _selectionChanged = selectionChanged;
        }

        /// <summary>
        /// メンバーID。
        /// </summary>
        public Guid MemberId { get; }

        /// <summary>
        /// 表示名。
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 有効メンバーかどうか。
        /// </summary>
        public bool IsActive { get; }

        /// <summary>
        /// 選択済みかどうか。
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    _selectionChanged(this, value);
                }
            }
        }

        /// <summary>
        /// 履歴適用時などに通知なしで選択状態を更新する。
        /// </summary>
        /// <param name="value">設定する選択状態。</param>
        public void SetSelectedSilently(bool value)
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChangedA(nameof(IsSelected));
        }
    }
}

/* --- End of file --- */
