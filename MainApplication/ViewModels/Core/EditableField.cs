using System;

namespace MainApplication.ViewModels.Core
{
    /// <summary>
    /// 編集可能なフィールドを表すクラス。
    /// Getter/Setterを通じて値を取得・設定し、
    /// 値が変更された場合は履歴記録処理を呼び出す。
    /// </summary>
    public class EditableField<T>(string name, Func<T?> getter, Action<T?> setter)
    {
        /* ---------------------------------------------------------
         * プロパティ
         * --------------------------------------------------------- */

        /// <summary>
        /// フィールド名(履歴記録時の識別用)
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// 現在の値を取得するためのデリゲート
        /// </summary>
        public Func<T?> Getter { get; } = getter;

        /// <summary>
        /// 新しい値を設定するためのデリゲート
        /// </summary>
        public Action<T?> Setter { get; } = setter;

        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private T? _oldValue = getter();

        /* ---------------------------------------------------------
         * 変更確定処理
         * --------------------------------------------------------- */

        /// <summary>
        /// 現在値と前回値を比較し、変更があれば履歴記録処理を実行する。
        /// </summary>
        /// <param name="commitHistory">
        /// 履歴記録処理(フィールド名・旧値・新値を受け取る)
        /// </param>
        /// <returns>
        /// 値が変更されて履歴が記録された場合はtrue、変更がなければfalse。
        /// </returns>
        public bool TryCommit(Action<string, T?, T?> commitHistory)
        {
            var newValue = Getter();
            if (!Equals(_oldValue, newValue))
            {
                commitHistory(Name, _oldValue, newValue);
                _oldValue = newValue;
                return true;
            }
            return false;
        }

    }
}

/* --- End of file --- */
