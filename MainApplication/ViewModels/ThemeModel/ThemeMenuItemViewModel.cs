using MainApplication.ViewModels.Core;
using System.Windows.Input;

namespace MainApplication.ViewModels.ThemeModel
{
    /// <summary>
    /// テーマ一覧メニューの1項目を表すViewModel。
    /// </summary>
    public class ThemeMenuItemViewModel
    {
        /// <summary>
        /// メニューに表示するテーマ名。
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 現在適用中のテーマかどうか。
        /// </summary>
        public bool IsCurrent { get; }

        /// <summary>
        /// テーマを適用するコマンド。
        /// </summary>
        public ICommand ApplyCommand { get; }

        /// <summary>
        /// ThemeMenuItemViewModelを生成する。
        /// </summary>
        /// <param name="name">テーマ名。</param>
        /// <param name="isCurrent">現在適用中のテーマかどうか。</param>
        /// <param name="applyAction">テーマ適用処理。</param>
        public ThemeMenuItemViewModel(string name, bool isCurrent, Action applyAction)
        {
            DisplayName = name;
            IsCurrent = isCurrent;
            ApplyCommand = new RelayCommand(applyAction);
        }
    }
}

/* --- End of file --- */
