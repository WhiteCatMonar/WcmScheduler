using MainApplication.Views;
using System;
using System.Linq;
using System.Windows;

namespace MainApplication.ViewModels.Service
{
    /// <summary>
    /// DateTimeEditorWindowを用いて日時編集を行うサービス。
    /// モーダルダイアログを開き、編集結果を返す。
    /// </summary>
    public class DateTimeEditorService : IDateTimeEditorService
    {
        /* ---------------------------------------------------------
         * 日時編集ダイアログの起動
         * --------------------------------------------------------- */

        /// <summary>
        /// 日時編集ダイアログを開き、編集結果を返す。
        /// </summary>
        /// <param name="initial">初期値(null可)</param>
        /// <param name="validate">入力検証用デリゲート</param>
        /// <returns>
        /// OK → 編集後の日時(null可)  
        /// クリア → null  
        /// キャンセル → initialをそのまま返す
        /// </returns>
        public DateTime? EditDateTime(DateTime? initial, Func<DateTime?, bool> validate)
        {
            var window = new DateTimeEditorWindow(initial, validate)
            {
                /* 現在アクティブなウィンドウをOwnerに設定 */
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            var result = window.ShowDialog();

            if (result == true && window.DataContext is DateTimeEditorViewModel vm)
            {
                /* OK → vm.Result(null の可能性あり) */
                return vm.Result;
            }

            /* キャンセル → 初期値を返す */
            return initial;
        }
    }
}

/* --- End of file --- */
