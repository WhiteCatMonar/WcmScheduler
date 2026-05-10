using System.Windows;
using MainApplication.ViewModels.SettingsModel;

namespace MainApplication.Views
{
    /// <summary>
    /// アプリケーション設定ウィンドウ。
    /// </summary>
    public partial class ApplicationSettingsWindow : Window
    {
        /// <summary>
        /// アプリケーション設定ウィンドウを初期化する。
        /// </summary>
        public ApplicationSettingsWindow()
        {
            InitializeComponent();
            Loaded += ApplicationSettingsWindow_Loaded;
        }

        /// <summary>
        /// ウィンドウ読み込み時にViewModelイベントを購読する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベント情報。</param>
        private void ApplicationSettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApplicationSettingsViewModel viewModel)
            {
                viewModel.SettingsSaved += Close;
            }
        }

        /// <summary>
        /// 閉じるボタン押下時にウィンドウを閉じる。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベント情報。</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

/* --- End of file --- */
