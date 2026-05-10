using MainApplication.Models.SaveData;
using MainApplication.ViewModels.Core;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// チームメンバーを表すViewModel
    /// </summary>
    public class TeamMemberViewModel : ViewModelBase
    {
        private string? _primaryName;
        private string? _secondaryName;
        private int _sundayWorkTimeMinutes = 480;
        private int _mondayWorkTimeMinutes = 480;
        private int _tuesdayWorkTimeMinutes = 480;
        private int _wednesdayWorkTimeMinutes = 480;
        private int _thursdayWorkTimeMinutes = 480;
        private int _fridayWorkTimeMinutes = 480;
        private int _saturdayWorkTimeMinutes = 480;
        private int _specialHolidayWorkTimeMinutes;

        /// <summary>
        /// 新規メンバーを生成する
        /// </summary>
        public TeamMemberViewModel()
        {
            MemberId = Guid.NewGuid();
            PrimaryName = "New";
            SecondaryName = "Member";
        }

        /// <summary>
        /// 保存データからメンバーを生成する
        /// </summary>
        /// <param name="data">保存データ。</param>
        public TeamMemberViewModel(MemberDataModel data)
        {
            MemberId = data.MemberId == Guid.Empty ? Guid.NewGuid() : data.MemberId;
            PrimaryName = data.PrimaryName;
            SecondaryName = data.SecondaryName;
            SundayWorkTimeMinutes = data.SundayWorkTimeMinutes;
            MondayWorkTimeMinutes = data.MondayWorkTimeMinutes;
            TuesdayWorkTimeMinutes = data.TuesdayWorkTimeMinutes;
            WednesdayWorkTimeMinutes = data.WednesdayWorkTimeMinutes;
            ThursdayWorkTimeMinutes = data.ThursdayWorkTimeMinutes;
            FridayWorkTimeMinutes = data.FridayWorkTimeMinutes;
            SaturdayWorkTimeMinutes = data.SaturdayWorkTimeMinutes;
            SpecialHolidayWorkTimeMinutes = data.SpecialHolidayWorkTimeMinutes;
        }

        /// <summary>
        /// メンバーを一意に識別するID
        /// </summary>
        public Guid MemberId { get; }

        /// <summary>
        /// メンバー名の主名
        /// </summary>
        public string? PrimaryName
        {
            get => _primaryName;
            set => SetProperty(ref _primaryName, value, [nameof(DisplayText), nameof(Initials)]);
        }

        /// <summary>
        /// メンバー名の副名
        /// </summary>
        public string? SecondaryName
        {
            get => _secondaryName;
            set => SetProperty(ref _secondaryName, value, [nameof(DisplayText), nameof(Initials)]);
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
        /// 特別休日のデフォルト作業可能時間。単位は分
        /// </summary>
        public int SpecialHolidayWorkTimeMinutes
        {
            get => _specialHolidayWorkTimeMinutes;
            set => SetProperty(
                ref _specialHolidayWorkTimeMinutes,
                Math.Max(0, value),
                [nameof(SpecialHolidayWorkTimeDurationText)]
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
        /// 特別休日のデフォルト作業可能時間の時間表記
        /// </summary>
        public string SpecialHolidayWorkTimeDurationText => FormatDuration(SpecialHolidayWorkTimeMinutes);

        /// <summary>
        /// 一覧表示用テキスト
        /// </summary>
        public string DisplayText => DisplayNameOrFallback;

        /// <summary>
        /// バッジ表示用イニシャル
        /// </summary>
        public string Initials
        {
            get
            {
                var primaryInitial = GetInitial(PrimaryName);
                var secondaryInitial = GetInitial(SecondaryName);
                var initials = primaryInitial + secondaryInitial;
                return string.IsNullOrWhiteSpace(initials) ? "?" : initials;
            }
        }

        private string DisplayNameOrFallback
        {
            get
            {
                var names = new[] { PrimaryName, SecondaryName }
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name!.Trim());
                var displayName = string.Join(" ", names);
                return string.IsNullOrWhiteSpace(displayName) ? "(名称未設定)" : displayName;
            }
        }

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
        /// 指定日のデフォルト作業可能時間を取得する
        /// </summary>
        /// <param name="date">対象日。</param>
        /// <param name="specialHolidays">特別休日一覧。</param>
        /// <returns>デフォルト作業可能時間。単位は分。</returns>
        public int GetDefaultWorkTimeMinutes(DateOnly date, IEnumerable<DateOnly> specialHolidays)
        {
            return specialHolidays.Contains(date)
                ? SpecialHolidayWorkTimeMinutes
                : GetDefaultWorkTimeMinutes(date.DayOfWeek);
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
                PrimaryName = PrimaryName,
                SecondaryName = SecondaryName,
                SundayWorkTimeMinutes = SundayWorkTimeMinutes,
                MondayWorkTimeMinutes = MondayWorkTimeMinutes,
                TuesdayWorkTimeMinutes = TuesdayWorkTimeMinutes,
                WednesdayWorkTimeMinutes = WednesdayWorkTimeMinutes,
                ThursdayWorkTimeMinutes = ThursdayWorkTimeMinutes,
                FridayWorkTimeMinutes = FridayWorkTimeMinutes,
                SaturdayWorkTimeMinutes = SaturdayWorkTimeMinutes,
                SpecialHolidayWorkTimeMinutes = SpecialHolidayWorkTimeMinutes
            };
        }

        /// <summary>
        /// 指定文字列の先頭1文字を取得する
        /// </summary>
        /// <param name="value">対象文字列。</param>
        /// <returns>先頭1文字。空欄の場合は空文字。</returns>
        private static string GetInitial(string? value)
        {
            var text = value?.Trim();
            return string.IsNullOrEmpty(text) ? string.Empty : text[..1];
        }
    }
}

/* --- End of file --- */
