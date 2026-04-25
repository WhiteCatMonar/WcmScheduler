using System;

namespace MainApplication.ViewModels.Service
{
    /// <summary>
    /// 色編集ダイアログを提供するサービスインターフェース。
    /// </summary>
    public interface IColorPickerService
    {
        /// <summary>
        /// 色編集ダイアログを開き、編集結果を返す。
        /// </summary>
        /// <param name="initial">初期色文字列。</param>
        /// <param name="validate">入力検証用デリゲート。</param>
        /// <returns>OK時は編集後の色文字列、キャンセル時は初期値。</returns>
        string EditColor(string initial, Func<string, bool>? validate = null);
    }
}

/* --- End of file --- */
