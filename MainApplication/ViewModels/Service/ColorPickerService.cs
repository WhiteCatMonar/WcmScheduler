using MainApplication.Views;
using System;
using System.Linq;
using System.Windows;

namespace MainApplication.ViewModels.Service
{
    /// <summary>
    /// ColorPickerWindowを用いて色編集を行うサービス。
    /// </summary>
    public class ColorPickerService : IColorPickerService
    {
        /// <summary>
        /// 色編集ダイアログを開き、編集結果を返す。
        /// </summary>
        /// <param name="initial">初期色文字列。</param>
        /// <param name="validate">入力検証用デリゲート。</param>
        /// <returns>OK時は編集後の色文字列、キャンセル時は初期値。</returns>
        public string EditColor(string initial, Func<string, bool>? validate = null)
        {
            var window = new ColorPickerWindow(initial, validate)
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            var result = window.ShowDialog();

            if (result == true && window.DataContext is ColorPickerViewModel vm && vm.Result is not null)
            {
                return vm.Result;
            }

            return initial;
        }
    }
}

/* --- End of file --- */
