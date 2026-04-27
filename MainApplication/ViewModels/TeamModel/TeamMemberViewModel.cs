using MainApplication.Models.SaveData;
using MainApplication.ViewModels.Core;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// チームメンバーを表すViewModel
    /// </summary>
    public class TeamMemberViewModel : ViewModelBase
    {
        private string? _displayName;
        private int _sundayWorkTimeMinutes = 480;
        private int _mondayWorkTimeMinutes = 480;
        private int _tuesdayWorkTimeMinutes = 480;
        private int _wednesdayWorkTimeMinutes = 480;
        private int _thursdayWorkTimeMinutes = 480;
        private int _fridayWorkTimeMinutes = 480;
        private int _saturdayWorkTimeMinutes = 480;

        /// <summary>
        /// 新規メンバーを生成する
        /// </summary>
        public TeamMemberViewModel()
        {
            MemberId = Guid.NewGuid();
            DisplayName = "New Member";
        }

        /// <summary>
        /// 保存データからメンバーを生成する
        /// </summary>
        /// <param name="data">保存データ。</param>
        public TeamMemberViewModel(MemberDataModel data)
        {
            MemberId = data.MemberId == Guid.Empty ? Guid.NewGuid() : data.MemberId;
            DisplayName = data.DisplayName;
            SundayWorkTimeMinutes = data.SundayWorkTimeMinutes;
            MondayWorkTimeMinutes = data.MondayWorkTimeMinutes;
            TuesdayWorkTimeMinutes = data.TuesdayWorkTimeMinutes;
            WednesdayWorkTimeMinutes = data.WednesdayWorkTimeMinutes;
            ThursdayWorkTimeMinutes = data.ThursdayWorkTimeMinutes;
            FridayWorkTimeMinutes = data.FridayWorkTimeMinutes;
            SaturdayWorkTimeMinutes = data.SaturdayWorkTimeMinutes;
        }

        /// <summary>
        /// メンバーを一意に識別するID
        /// </summary>
        public Guid MemberId { get; }

        /// <summary>
        /// UIに表示するメンバー名
        /// </summary>
        public string? DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value, [nameof(DisplayText)]);
        }

        /// <summary>
        /// メンバーが有効かどうか
        /// </summary>
        public bool IsActive => true;

        /// <summary>
        /// 日曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        public int SundayWorkTimeMinutes
        {
            get => _sundayWorkTimeMinutes;
            set => SetProperty(
                ref _sundayWorkTimeMinutes,
                Math.Max(0, value),
                [nameof(SundayWorkTimeDurationText)]
            );
        }

        /// <summary>
        /// 月曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        public int MondayWorkTimeMinutes
        {
            get => _mondayWorkTimeMinutes;
            set => SetProperty(
                ref _mondayWorkTimeMinutes,
                Math.Max(0, value),
                [nameof(MondayWorkTimeDurationText)]
            );
        }

        /// <summary>
        /// 火曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        public int TuesdayWorkTimeMinutes
        {
            get => _tuesdayWorkTimeMinutes;
            set => SetProperty(
                ref _tuesdayWorkTimeMinutes,
                Math.Max(0, value),
                [nameof(TuesdayWorkTimeDurationText)]
            );
        }

        /// <summary>
        /// 水曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        public int WednesdayWorkTimeMinutes
        {
            get => _wednesdayWorkTimeMinutes;
            set => SetProperty(
                ref _wednesdayWorkTimeMinutes,
                Math.Max(0, value),
                [nameof(WednesdayWorkTimeDurationText)]
            );
        }

        /// <summary>
        /// 木曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        public int ThursdayWorkTimeMinutes
        {
            get => _thursdayWorkTimeMinutes;
            set => SetProperty(
                ref _thursdayWorkTimeMinutes,
                Math.Max(0, value),
                [nameof(ThursdayWorkTimeDurationText)]
            );
        }

        /// <summary>
        /// 金曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        public int FridayWorkTimeMinutes
        {
            get => _fridayWorkTimeMinutes;
            set => SetProperty(
                ref _fridayWorkTimeMinutes,
                Math.Max(0, value),
                [nameof(FridayWorkTimeDurationText)]
            );
        }

        /// <summary>
        /// 土曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        public int SaturdayWorkTimeMinutes
        {
            get => _saturdayWorkTimeMinutes;
            set => SetProperty(
                ref _saturdayWorkTimeMinutes,
                Math.Max(0, value),
                [nameof(SaturdayWorkTimeDurationText)]
            );
        }

        /// <summary>
        /// 日曜日のデフォルト作業可能時間の時間表記
        /// </summary>
        public string SundayWorkTimeDurationText => FormatDuration(SundayWorkTimeMinutes);

        /// <summary>
        /// 月曜日のデフォルト作業可能時間の時間表記
        /// </summary>
        public string MondayWorkTimeDurationText => FormatDuration(MondayWorkTimeMinutes);

        /// <summary>
        /// 火曜日のデフォルト作業可能時間の時間表記
        /// </summary>
        public string TuesdayWorkTimeDurationText => FormatDuration(TuesdayWorkTimeMinutes);

        /// <summary>
        /// 水曜日のデフォルト作業可能時間の時間表記
        /// </summary>
        public string WednesdayWorkTimeDurationText => FormatDuration(WednesdayWorkTimeMinutes);

        /// <summary>
        /// 木曜日のデフォルト作業可能時間の時間表記
        /// </summary>
        public string ThursdayWorkTimeDurationText => FormatDuration(ThursdayWorkTimeMinutes);

        /// <summary>
        /// 金曜日のデフォルト作業可能時間の時間表記
        /// </summary>
        public string FridayWorkTimeDurationText => FormatDuration(FridayWorkTimeMinutes);

        /// <summary>
        /// 土曜日のデフォルト作業可能時間の時間表記
        /// </summary>
        public string SaturdayWorkTimeDurationText => FormatDuration(SaturdayWorkTimeMinutes);

        /// <summary>
        /// 一覧表示用テキスト
        /// </summary>
        public string DisplayText => DisplayNameOrFallback;

        private string DisplayNameOrFallback => string.IsNullOrWhiteSpace(DisplayName) ? "(名称未設定)" : DisplayName;

        /// <summary>
        /// 指定曜日のデフォルト作業可能時間を取得する
        /// </summary>
        /// <param name="dayOfWeek">対象曜日。</param>
        /// <returns>デフォルト作業可能時間。単位は分。</returns>
        public int GetDefaultWorkTimeMinutes(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => SundayWorkTimeMinutes,
                DayOfWeek.Monday => MondayWorkTimeMinutes,
                DayOfWeek.Tuesday => TuesdayWorkTimeMinutes,
                DayOfWeek.Wednesday => WednesdayWorkTimeMinutes,
                DayOfWeek.Thursday => ThursdayWorkTimeMinutes,
                DayOfWeek.Friday => FridayWorkTimeMinutes,
                DayOfWeek.Saturday => SaturdayWorkTimeMinutes,
                _ => 0
            };
        }

        /// <summary>
        /// 分単位の期間を時間表記へ変換する
        /// </summary>
        /// <param name="minutes">分単位の期間。</param>
        /// <returns>時間表記。</returns>
        public static string FormatDuration(int minutes)
        {
            var normalized = Math.Max(0, minutes);
            var hours = normalized / 60;
            var remainingMinutes = normalized % 60;
            return $"{hours}時間{remainingMinutes}分";
        }

        /// <summary>
        /// 保存データへ変換する
        /// </summary>
        /// <returns>保存データ。</returns>
        public MemberDataModel ToDataModel()
        {
            return new MemberDataModel
            {
                MemberId = MemberId,
                DisplayName = DisplayName,
                SundayWorkTimeMinutes = SundayWorkTimeMinutes,
                MondayWorkTimeMinutes = MondayWorkTimeMinutes,
                TuesdayWorkTimeMinutes = TuesdayWorkTimeMinutes,
                WednesdayWorkTimeMinutes = WednesdayWorkTimeMinutes,
                ThursdayWorkTimeMinutes = ThursdayWorkTimeMinutes,
                FridayWorkTimeMinutes = FridayWorkTimeMinutes,
                SaturdayWorkTimeMinutes = SaturdayWorkTimeMinutes
            };
        }
    }
}

/* --- End of file --- */
