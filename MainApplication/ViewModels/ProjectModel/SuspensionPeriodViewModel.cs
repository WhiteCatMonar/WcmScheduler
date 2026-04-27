using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using System.ComponentModel;
using System.Windows.Input;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// タスクの中断期間を管理するViewModel。
    /// </summary>
    public class SuspensionPeriodViewModel : ViewModelBase
    {
        private readonly NodeDetailViewModel _owner;
        private readonly UndoRedoManager _undoRedo;
        private readonly IDateTimeEditorService _dateTimeEditor;
        private DateTime? _startDateTime;
        private DateTime? _endDateTime;

        /// <summary>
        /// 中断期間ViewModelを生成する。
        /// </summary>
        /// <param name="owner">所有するノード詳細ViewModel。</param>
        /// <param name="undoRedo">Undo/Redo管理。</param>
        /// <param name="dateTimeEditor">日時編集サービス。</param>
        public SuspensionPeriodViewModel(
            NodeDetailViewModel owner,
            UndoRedoManager undoRedo,
            IDateTimeEditorService dateTimeEditor,
            DateTime? startDateTime = null,
            DateTime? endDateTime = null
        )
        {
            _owner = owner;
            _undoRedo = undoRedo;
            _dateTimeEditor = dateTimeEditor;
            _startDateTime = startDateTime;
            _endDateTime = endDateTime;

            EditStartDateTimeCommand = new RelayCommand(() => EditDateTime(true));
            EditEndDateTimeCommand = new RelayCommand(() => EditDateTime(false));
            DeleteCommand = new RelayCommand(Delete);
        }

        /// <summary>
        /// 中断開始日時。
        /// </summary>
        [DisplayName("中断開始日時")]
        public DateTime? StartDateTime
        {
            get => _startDateTime;
            set => SetProperty(
                ref _startDateTime,
                value,
                [nameof(IsIgnoredForNormalization)],
                CreateHooksFromValue(
                    value,
                    pre: (oldValue, newValue) =>
                    {
                        if (!_undoRedo.IsApplyingHistory)
                        {
                            _undoRedo.Execute(
                                new EditSuspensionPeriodPropertyAction(
                                    this,
                                    nameof(StartDateTime),
                                    _startDateTime,
                                    newValue
                                )
                            );
                        }
                    }
                )
            );
        }

        /// <summary>
        /// 中断終了日時。
        /// </summary>
        [DisplayName("中断終了日時")]
        public DateTime? EndDateTime
        {
            get => _endDateTime;
            set => SetProperty(
                ref _endDateTime,
                value,
                [nameof(IsIgnoredForNormalization)],
                CreateHooksFromValue(
                    value,
                    pre: (oldValue, newValue) =>
                    {
                        if (!_undoRedo.IsApplyingHistory)
                        {
                            _undoRedo.Execute(
                                new EditSuspensionPeriodPropertyAction(
                                    this,
                                    nameof(EndDateTime),
                                    _endDateTime,
                                    newValue
                                )
                            );
                        }
                    }
                )
            );
        }

        /// <summary>
        /// 中断開始日時編集コマンド。
        /// </summary>
        public ICommand EditStartDateTimeCommand { get; }

        /// <summary>
        /// 中断終了日時編集コマンド。
        /// </summary>
        public ICommand EditEndDateTimeCommand { get; }

        /// <summary>
        /// 中断期間削除コマンド。
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// 正規化済み中断期間の算定対象から除外されるかどうか。
        /// </summary>
        public bool IsIgnoredForNormalization => StartDateTime == null || EndDateTime == null;

        /// <summary>
        /// 中断開始日時が他の中断期間と重複しているかどうか。
        /// </summary>
        public bool IsStartDateTimeOverlapping => _owner.IsSuspensionPeriodStartOverlapping(this);

        /// <summary>
        /// 中断終了日時が他の中断期間と重複しているかどうか。
        /// </summary>
        public bool IsEndDateTimeOverlapping => _owner.IsSuspensionPeriodEndOverlapping(this);

        /// <summary>
        /// 正規化または重複状態に関するプロパティ変更通知を行う。
        /// </summary>
        public void NotifyNormalizationStateChanged()
        {
            OnPropertyChangedA(nameof(IsIgnoredForNormalization));
            OnPropertyChangedA(nameof(IsStartDateTimeOverlapping));
            OnPropertyChangedA(nameof(IsEndDateTimeOverlapping));
        }

        /// <summary>
        /// 中断開始日時を設定できるかどうかを判定する。
        /// </summary>
        /// <param name="newStart">設定候補の中断開始日時。</param>
        /// <returns>設定可能ならtrue。</returns>
        public bool CanSetStartDateTime(DateTime? newStart) =>
            (newStart == null) || (EndDateTime == null) || (newStart <= EndDateTime);

        /// <summary>
        /// 中断終了日時を設定できるかどうかを判定する。
        /// </summary>
        /// <param name="newEnd">設定候補の中断終了日時。</param>
        /// <returns>設定可能ならtrue。</returns>
        public bool CanSetEndDateTime(DateTime? newEnd) =>
            (newEnd == null) || (StartDateTime == null) || (StartDateTime <= newEnd);

        /// <summary>
        /// 中断日時を編集する。
        /// </summary>
        /// <param name="isStart">開始日時を編集する場合はtrue。</param>
        private void EditDateTime(bool isStart)
        {
            var initial = isStart ? StartDateTime : EndDateTime;
            var picked = _dateTimeEditor.EditDateTime(
                initial,
                isStart ? CanSetStartDateTime : CanSetEndDateTime
            );

            if (picked == initial)
            {
                return;
            }

            if (isStart)
            {
                if (CanSetStartDateTime(picked))
                {
                    StartDateTime = picked;
                }
            }
            else
            {
                if (CanSetEndDateTime(picked))
                {
                    EndDateTime = picked;
                }
            }
        }

        /// <summary>
        /// 中断期間を削除する。
        /// </summary>
        private void Delete()
        {
            _owner.DeleteSuspensionPeriod(this);
        }
    }
}

/* --- End of file --- */
