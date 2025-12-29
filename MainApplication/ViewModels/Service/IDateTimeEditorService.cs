using System;

namespace MainApplication.ViewModels.Service
{
    /// <summary>
    /// 日時編集ダイアログを提供するサービスインターフェース。
    /// モーダルウィンドウを開き、編集結果を返す。
    /// </summary>
    public interface IDateTimeEditorService
    {
        /// <summary>
        /// 日時編集ダイアログを開き、編集結果を返す。
        /// </summary>
        /// <param name="initial">初期値(null可)</param>
        /// <param name="validate">入力検証用デリゲート</param>
        /// <returns>
        /// OK → 編集後の日時(nullの可能性あり)  
        /// クリア → null  
        /// キャンセル → initialをそのまま返す
        /// </returns>
        DateTime? EditDateTime(DateTime? initial, Func<DateTime?, bool> validate);
    }
}

/* --- End of file --- */
