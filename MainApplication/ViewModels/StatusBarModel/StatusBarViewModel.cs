using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace MainApplication.ViewModels.StatusBarModel
{
    /// <summary>
    /// アプリケーション共通ステータスバーの状態を管理するViewModel
    /// </summary>
    public class StatusBarViewModel : ViewModelBase
    {
        /// <summary>
        /// ステータスバーの状態種別
        /// </summary>
        public enum StatusBarState
        {
            Idle,
            Busy,
            Warning,
            Error
        }

        private readonly Stopwatch _stopwatch = new();
        private readonly DispatcherTimer _elapsedTimer;
        private string _message = "準備完了";
        private StatusBarState _state = StatusBarState.Idle;
        private bool _isBusy;
        private bool _isIndeterminate;
        private bool _hasUnsavedChanges;
        private string _currentFileName = "";
        private string _elapsedTimeText = "";
        private int _readyTaskCount;
        private int _pendingTaskCount;
        private int _inProgressTaskCount;
        private int _doneTaskCount;
        private int _totalTaskCount;
        private int _projectProgressPercent;
        private int _completedQueueCount;
        private int _totalQueueCount;

        /// <summary>
        /// StatusBarViewModelを生成する
        /// </summary>
        public StatusBarViewModel()
        {
            _elapsedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _elapsedTimer.Tick += (sender, args) => UpdateElapsedTimeText();
        }

        /// <summary>
        /// 状態メッセージ
        /// </summary>
        public string Message
        {
            get => _message;
            private set => SetProperty(ref _message, value);
        }

        /// <summary>
        /// 処理中かどうか
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// 不定進捗表示を行うかどうか
        /// </summary>
        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            private set => SetProperty(ref _isIndeterminate, value);
        }

        /// <summary>
        /// 未保存変更があるかどうか
        /// </summary>
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            private set => SetProperty(ref _hasUnsavedChanges, value);
        }

        /// <summary>
        /// 現在の保存ファイル名
        /// </summary>
        public string CurrentFileName
        {
            get => _currentFileName;
            private set => SetProperty(ref _currentFileName, value);
        }

        /// <summary>
        /// 処理時間表示
        /// </summary>
        public string ElapsedTimeText
        {
            get => _elapsedTimeText;
            private set => SetProperty(ref _elapsedTimeText, value);
        }

        /// <summary>
        /// プロジェクト進捗表示
        /// </summary>
        public int ReadyTaskCount
        {
            get => _readyTaskCount;
            private set => SetProperty(ref _readyTaskCount, value);
        }

        /// <summary>
        /// Pending迥ｶ諷九・繧ｿ繧ｹ繧ｯ謨ｰ
        /// </summary>
        public int PendingTaskCount
        {
            get => _pendingTaskCount;
            private set => SetProperty(ref _pendingTaskCount, value);
        }

        /// <summary>
        /// InProgress迥ｶ諷九・繧ｿ繧ｹ繧ｯ謨ｰ
        /// </summary>
        public int InProgressTaskCount
        {
            get => _inProgressTaskCount;
            private set => SetProperty(ref _inProgressTaskCount, value);
        }

        /// <summary>
        /// Done迥ｶ諷九・繧ｿ繧ｹ繧ｯ謨ｰ
        /// </summary>
        public int DoneTaskCount
        {
            get => _doneTaskCount;
            private set => SetProperty(ref _doneTaskCount, value);
        }

        /// <summary>
        /// 繧ｿ繧ｹ繧ｯ邱乗焚
        /// </summary>
        public int TotalTaskCount
        {
            get => _totalTaskCount;
            private set => SetProperty(ref _totalTaskCount, value);
        }

        /// <summary>
        /// 繝励Ο繧ｸ繧ｧ繧ｯ繝磯ｲ謐苓｡ｨ遉ｺ邱丞・
        /// </summary>
        public int ProjectProgressPercent
        {
            get => _projectProgressPercent;
            private set => SetProperty(ref _projectProgressPercent, value);
        }

        /// <summary>
        /// 繝励Ο繧ｸ繧ｧ繧ｯ繝磯ｲ謐苓｡ｨ遉ｺ繧定｡ｨ遉ｺ縺吶ｋ縺九←縺・°
        /// </summary>
        public bool HasProjectProgress => TotalTaskCount > 0;

        /// <summary>
        /// 繝励Ο繧ｸ繧ｧ繧ｯ繝磯ｲ謐励・陬懈焚陦ｨ遉ｺ
        /// </summary>
        public string ProjectProgressTotalText => $"/ Total {TotalTaskCount} ({ProjectProgressPercent}%)";

        /// <summary>
        /// 現在の状態
        /// </summary>
        public StatusBarState State
        {
            get => _state;
            private set => SetProperty(
                ref _state,
                value,
                [
                    nameof(IsWarning),
                    nameof(IsError)
                ]
            );
        }

        /// <summary>
        /// 警告状態かどうか
        /// </summary>
        public bool IsWarning => State == StatusBarState.Warning;

        /// <summary>
        /// エラー状態かどうか
        /// </summary>
        public bool IsError => State == StatusBarState.Error;

        /// <summary>
        /// キュー進捗表示
        /// </summary>
        public string QueueProgressText =>
            _totalQueueCount <= 0 ? "" : $"({_completedQueueCount}/{_totalQueueCount})";

        /// <summary>
        /// 通常状態へ戻す
        /// </summary>
        public void ResetToIdle()
        {
            _completedQueueCount = 0;
            _totalQueueCount = 0;
            _stopwatch.Reset();
            _elapsedTimer.Stop();
            Message = "準備完了";
            IsBusy = false;
            IsIndeterminate = false;
            State = StatusBarState.Idle;
            ElapsedTimeText = "";
            OnPropertyChangedA(nameof(QueueProgressText));
        }

        /// <summary>
        /// 処理開始を通知する
        /// </summary>
        /// <param name="message">処理中メッセージ</param>
        public void BeginOperation(string message)
        {
            if (!IsBusy && _completedQueueCount >= _totalQueueCount)
            {
                _completedQueueCount = 0;
                _totalQueueCount = 0;
            }

            _totalQueueCount++;
            Message = message;
            IsBusy = true;
            IsIndeterminate = true;
            State = StatusBarState.Busy;
            _stopwatch.Restart();
            _elapsedTimer.Start();
            UpdateElapsedTimeText();
            OnPropertyChangedA(nameof(QueueProgressText));
        }

        /// <summary>
        /// 処理完了を通知する
        /// </summary>
        /// <param name="message">完了メッセージ</param>
        public void CompleteOperation(string message)
        {
            if (_totalQueueCount > 0)
            {
                _completedQueueCount = Math.Min(_completedQueueCount + 1, _totalQueueCount);
            }

            _stopwatch.Stop();
            _elapsedTimer.Stop();
            UpdateElapsedTimeText();
            Message = message;
            IsBusy = false;
            IsIndeterminate = false;
            State = StatusBarState.Idle;
            OnPropertyChangedA(nameof(QueueProgressText));
        }

        /// <summary>
        /// 処理失敗を通知する
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        public void FailOperation(string message)
        {
            if (_totalQueueCount > 0)
            {
                _completedQueueCount = Math.Min(_completedQueueCount + 1, _totalQueueCount);
            }

            _stopwatch.Stop();
            _elapsedTimer.Stop();
            UpdateElapsedTimeText();
            Message = message;
            IsBusy = false;
            IsIndeterminate = false;
            State = StatusBarState.Error;
            OnPropertyChangedA(nameof(QueueProgressText));
        }

        /// <summary>
        /// 保存状態を反映する
        /// </summary>
        /// <param name="hasUnsavedChanges">未保存変更があるかどうか</param>
        /// <param name="currentFilePath">現在の保存ファイルパス</param>
        public void UpdateSaveState(bool hasUnsavedChanges, string? currentFilePath)
        {
            HasUnsavedChanges = hasUnsavedChanges;
            CurrentFileName = string.IsNullOrWhiteSpace(currentFilePath)
                ? "(未保存)"
                : Path.GetFileName(currentFilePath);
        }

        /// <summary>
        /// プロジェクト進捗表示を更新する
        /// </summary>
        /// <param name="project">対象プロジェクト</param>
        public void UpdateProjectProgress(ProjectViewModel? project)
        {
            if (project == null)
            {
                SetProjectProgressCounts(0, 0, 0, 0, 0, 0);
                return;
            }

            var nodes = project.NodeEditor.Nodes.Nodes;
            var total = nodes.Count;
            if (total == 0)
            {
                SetProjectProgressCounts(0, 0, 0, 0, 0, 0);
                return;
            }

            var ready = nodes.Count(node => node.Status == NodeViewModel.TaskStatus.Ready);
            var pending = nodes.Count(node => node.Status == NodeViewModel.TaskStatus.Pending);
            var inProgress = nodes.Count(node => node.Status == NodeViewModel.TaskStatus.InProgress);
            var done = nodes.Count(node => node.Status == NodeViewModel.TaskStatus.Done);
            var progress = (int)Math.Round((double)done / total * 100.0);
            SetProjectProgressCounts(ready, pending, inProgress, done, total, progress);
        }

        /// <summary>
        /// 繝励Ο繧ｸ繧ｧ繧ｯ繝磯ｲ謐励・陦ｨ遉ｺ謨ｰ繧呈峩譁ｰ縺吶ｋ
        /// </summary>
        /// <param name="ready">Ready謨ｰ</param>
        /// <param name="pending">Pending謨ｰ</param>
        /// <param name="inProgress">InProgress謨ｰ</param>
        /// <param name="done">Done謨ｰ</param>
        /// <param name="total">邱乗焚</param>
        /// <param name="progress">騾ｲ謐捺ｯ・/param>
        private void SetProjectProgressCounts(
            int ready,
            int pending,
            int inProgress,
            int done,
            int total,
            int progress
        )
        {
            ReadyTaskCount = ready;
            PendingTaskCount = pending;
            InProgressTaskCount = inProgress;
            DoneTaskCount = done;
            TotalTaskCount = total;
            ProjectProgressPercent = progress;
            OnPropertyChangedA(nameof(HasProjectProgress));
            OnPropertyChangedA(nameof(ProjectProgressTotalText));
        }

        /// <summary>
        /// 処理時間表示を更新する
        /// </summary>
        private void UpdateElapsedTimeText()
        {
            ElapsedTimeText = _stopwatch.Elapsed <= TimeSpan.Zero
                ? ""
                : $"{_stopwatch.Elapsed.TotalSeconds:0.0}s";
        }
    }
}

/* --- End of file --- */
