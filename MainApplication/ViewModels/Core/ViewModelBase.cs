using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MainApplication.ViewModels.Core
{
    /// <summary>
    /// INotifyPropertyChangedを簡潔に扱うための基底クラス。
    /// SetPropertyによってプロパティ変更通知を統一する。
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /* ---------------------------------------------------------
         * イベント
         * --------------------------------------------------------- */
        /// <summary>
        /// プロパティ変更時に発火するイベント。
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /* ---------------------------------------------------------
         * 型定義
         * --------------------------------------------------------- */
        /// <summary>
        /// プロパティ変更の前後で実行される処理。
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="oldValue">変更前の値</param>
        /// <param name="newValue">変更後の値</param>
        public delegate void PropertyChangeHandler<T>(T? oldValue, T? newValue);

        /// <summary>
        /// プロパティ変更通知発行後に実行される処理
        /// </summary>
        public delegate void NotifyChainHandler();

        /* ---------------------------------------------------------
         * Hooks関連
         * --------------------------------------------------------- */

        /// <summary>
        /// SetPropertyを行う際の各タイミングで挿入するユーザー定義処理群。
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="pre">プロパティ変更直前の処理</param>
        /// <param name="post">プロパティ変更直後の処理</param>
        /// <param name="chain">プロパティ変更通知発行後の連動処理</param>
        public class SetPropertyHooks<T>(
            PropertyChangeHandler<T>? pre = null,
            PropertyChangeHandler<T>? post = null,
            NotifyChainHandler? chain = null)
        {
            public PropertyChangeHandler<T>? PreProcess { get; set; } = pre;
            public PropertyChangeHandler<T>? PostProcess { get; set; } = post;
            public NotifyChainHandler? ChainProcess { get; set; } = chain;
        }

        /// <summary>
        /// SetPropertyHooksをプロパティ型から推論して作成する。
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="_">プロパティ(型推論にのみ使用するため内部では使用しない)</param>
        /// <param name="pre">プロパティ変更直前の処理</param>
        /// <param name="post">プロパティ変更直後の処理</param>
        public static SetPropertyHooks<T> CreateHooksFromValue<T>(
                T _,
                PropertyChangeHandler<T>? pre = null,
                PropertyChangeHandler<T>? post = null,
                NotifyChainHandler? chain = null
            )
        {
            return new SetPropertyHooks<T>( pre, post, chain );
        }

        /* ---------------------------------------------------------
         * プロパティ設定
         * --------------------------------------------------------- */
        /// <summary>
        /// プロパティ値を設定し、変更があればPropertyChangedを発行する。
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">設定対象のフィールド(ref)</param>
        /// <param name="value">設定する値</param>
        /// <param name="propertyName">プロパティ名(自動設定)</param>
        /// <returns>値が変更された場合はtrue、変更がなければfalse</returns>
        protected bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            /* プロパティ変更 */
            field = value;

            /* 変更通知 */
            OnPropertyChangedA(propertyName);
            return true;
        }

        /// <summary>
        /// フィールドに値を設定し、変更があればPropertyChangedを発行する。
        /// さらに、追加で通知したいプロパティ名をまとめて通知できる。
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">設定対象のフィールド(ref)</param>
        /// <param name="value">設定する値</param>
        /// <param name="additionalProperties">
        /// 追加で PropertyChangedを通知したいプロパティ名の配列。
        /// </param>
        /// <param name="propertyName">プロパティ名(自動設定)</param>
        /// <returns>値が変更された場合はtrue、変更がなければfalse</returns>
        protected bool SetProperty<T>(
            ref T field,
            T value,
            string[] additionalProperties,
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            /* プロパティ変更 */
            field = value;

            /* 変更通知 */
            OnPropertyChangedA(propertyName);
            foreach (var p in additionalProperties)
            {
                    OnPropertyChangedA(p);
            }

            return true;
        }

        /// <summary>
        /// フィールドに値を設定し、変更があればPropertyChangedを発行する。
        /// 変更前後に任意の処理(preProcess / postProcess)を挟むことができる。
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">設定対象のフィールド(ref)</param>
        /// <param name="value">設定する値</param>
        /// <param name="hooks">
        /// SetProperty実行中の各タイミングで実行するユーザー処理。
        /// </param>
        /// <param name="propertyName">変更通知を発行するプロパティ名(自動設定)</param>
        /// <returns>値が変更された場合はtrue、変更がなければfalse。</returns>
        protected bool SetProperty<T>(
            ref T field,
            T value,
            SetPropertyHooks<T> hooks,
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            /* プロパティ変更 */
            var oldValue = field;
            hooks.PreProcess?.Invoke(oldValue, value);
            field = value;
            hooks.PostProcess?.Invoke(oldValue, value);

            /* 変更通知 */
            OnPropertyChangedA(propertyName);
            hooks.ChainProcess?.Invoke();

            return true;
        }

        /// <summary>
        /// フィールドに値を設定し、変更があればPropertyChangedを発行する。
        /// 変更前後に任意の処理(preProcess / postProcess)を挟み、
        /// さらに追加で通知したいプロパティ名をまとめて通知できる。
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">設定対象のフィールド(ref)</param>
        /// <param name="value">設定する値</param>
        /// <param name="additionalProperties">
        /// 追加で PropertyChangedを通知したいプロパティ名の配列。
        /// </param>
        /// <param name="hooks">
        /// SetProperty実行中の各タイミングで実行するユーザー処理。
        /// </param>
        /// <param name="propertyName">変更通知を発行するプロパティ名(自動設定)</param>
        /// <returns>値が変更された場合はtrue、変更がなければfalse。</returns>
        protected bool SetProperty<T>(
            ref T field,
            T value,
            string[] additionalProperties,
            SetPropertyHooks<T> hooks,
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            /* プロパティ変更 */
            var oldValue = field;
            hooks.PreProcess?.Invoke(oldValue, value);
            field = value;
            hooks.PostProcess?.Invoke(oldValue, value);

            /* 変更通知 */
            OnPropertyChangedA(propertyName);
            foreach (var p in additionalProperties)
            {
                OnPropertyChangedA(p);
            }
            hooks.ChainProcess?.Invoke();

            return true;
        }

        /* ---------------------------------------------------------
         * プロパティ変更通知
         * --------------------------------------------------------- */
        /// <summary>
        /// PropertyChangedイベントを発行する。
        /// </summary>
        /// <param name="propertyName">変更されたプロパティ名(自動設定)</param>
        private protected void OnPropertyChangedA([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

/* --- End of file --- */
