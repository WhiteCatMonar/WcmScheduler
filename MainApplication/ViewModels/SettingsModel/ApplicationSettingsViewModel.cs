using MainApplication.Models.Settings;
using MainApplication.ViewModels.Core;
using System.Windows.Input;

namespace MainApplication.ViewModels.SettingsModel
{
    /// <summary>
    /// アプリケーション設定ウィンドウの状態を管理するViewModel。
    /// </summary>
    public class ApplicationSettingsViewModel : ViewModelBase
    {
        private int _autoBackupGenerationCount;

        /// <summary>
        /// 自動バックアップ保持世代数。
        /// </summary>
        public int AutoBackupGenerationCount
        {
            get => _autoBackupGenerationCount;
            set => SetProperty(ref _autoBackupGenerationCount, Math.Max(0, value));
        }

        /// <summary>
        /// 設定保存コマンド。
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// アプリケーション設定ViewModelを生成する。
        /// </summary>
        /// <param name="source">編集元アプリケーション設定。</param>
        public ApplicationSettingsViewModel(AppSettingsModel source)
        {
            AutoBackupGenerationCount = Math.Max(0, source.AutoBackupGenerationCount);
            SaveCommand = new RelayCommand(Save);
        }

        /// <summary>
        /// 設定保存時に通知するイベント。
        /// </summary>
        public event Action? SettingsSaved;

        /// <summary>
        /// 現在の編集内容をアプリケーション設定として保存する。
        /// </summary>
        private void Save()
        {
            AppSettingsManager.Current.AutoBackupGenerationCount = AutoBackupGenerationCount;
            AppSettingsManager.Save();
            SettingsSaved?.Invoke();
        }
    }
}

/* --- End of file --- */
