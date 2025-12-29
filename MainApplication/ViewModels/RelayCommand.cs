using System;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// パラメータなしのICommand実装。
    /// MVVMにおいてボタン操作などをViewModelのメソッドに委譲するために使用する。
    /// </summary>
    public class RelayCommand : ICommand
    {
        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// RelayCommandを生成する。
        /// </summary>
        /// <param name="execute">実行時に呼び出される処理</param>
        /// <param name="canExecute">実行可能かどうかを判定する関数(省略可)</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /* ---------------------------------------------------------
         * ICommand実装
         * --------------------------------------------------------- */

        /// <summary>
        /// コマンドが実行可能かどうかを返す。
        /// </summary>
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        /// <summary>
        /// コマンドを実行する。
        /// </summary>
        public void Execute(object parameter) => _execute();

        /// <summary>
        /// 実行可否状態が変化したときに通知されるイベント。
        /// CommandManager.RequerySuggestedに委譲することで、
        /// WPFの自動再評価(フォーカス移動など)に対応する。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    /// <summary>
    /// パラメータ付きのICommand実装。
    /// ボタンやUI操作から型付きパラメータを受け取りたい場合に使用する。
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// RelayCommand&lt;T&gt;を生成する。
        /// </summary>
        /// <param name="execute">実行時に呼び出される処理</param>
        /// <param name="canExecute">実行可能かどうかを判定する関数(省略可)</param>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /* ---------------------------------------------------------
         * ICommand 実装
         * --------------------------------------------------------- */

        /// <summary>
        /// コマンドが実行可能かどうかを返す。
        /// </summary>
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
        
        /// <summary>
        /// コマンドを実行する。
        /// </summary>
        public void Execute(object parameter) => _execute((T)parameter);
        
        /// <summary>
        /// 実行可否状態が変化したときに通知されるイベント。
        /// CommandManager.RequerySuggestedに委譲する。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}

/* --- End of file --- */
