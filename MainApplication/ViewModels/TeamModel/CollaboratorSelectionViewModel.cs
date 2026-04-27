using MainApplication.ViewModels.Core;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// 作業協力者1行分の選択状態を表すViewModel。
    /// </summary>
    public class CollaboratorSelectionViewModel : ViewModelBase
    {
        private readonly Action<CollaboratorSelectionViewModel> _remove;
        private readonly Action _selectionChanged;
        private Guid? _selectedMemberId;

        /// <summary>
        /// 作業協力者選択行を生成する。
        /// </summary>
        /// <param name="options">選択肢一覧。</param>
        /// <param name="selectedMemberId">選択中メンバーID。</param>
        /// <param name="selectionChanged">選択変更時の処理。</param>
        /// <param name="remove">削除時の処理。</param>
        public CollaboratorSelectionViewModel(
            ObservableCollection<MemberOptionViewModel> options,
            Guid? selectedMemberId,
            Action selectionChanged,
            Action<CollaboratorSelectionViewModel> remove
        )
        {
            Options = options;
            _selectedMemberId = selectedMemberId;
            _selectionChanged = selectionChanged;
            _remove = remove;
            RemoveCommand = new RelayCommand(() => _remove(this));
        }

        /// <summary>
        /// 選択肢一覧。
        /// </summary>
        public ObservableCollection<MemberOptionViewModel> Options { get; }

        /// <summary>
        /// 選択中メンバーID。
        /// </summary>
        public Guid? SelectedMemberId
        {
            get => _selectedMemberId;
            set
            {
                if (SetProperty(ref _selectedMemberId, value))
                {
                    _selectionChanged();
                }
            }
        }

        /// <summary>
        /// 行削除コマンド。
        /// </summary>
        public ICommand RemoveCommand { get; }
    }
}

/* --- End of file --- */
