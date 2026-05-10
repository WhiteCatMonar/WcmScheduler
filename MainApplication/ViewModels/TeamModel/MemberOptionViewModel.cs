using MainApplication.ViewModels.Core;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// メンバー選択欄に表示する選択肢。
    /// </summary>
    public class MemberOptionViewModel : ViewModelBase
    {
        /// <summary>
        /// メンバー未選択を表す選択肢を生成する。
        /// </summary>
        public MemberOptionViewModel()
        {
            MemberId = null;
            DisplayName = "(未担当)";
            IsActive = true;
        }

        /// <summary>
        /// メンバー情報から選択肢を生成する。
        /// </summary>
        /// <param name="member">選択肢の元になるメンバー。</param>
        public MemberOptionViewModel(TeamMemberViewModel member)
        {
            MemberId = member.MemberId;
            DisplayName = member.DisplayText;
            IsActive = member.IsActive;
        }

        /// <summary>
        /// メンバーID。
        /// </summary>
        public Guid? MemberId { get; }

        /// <summary>
        /// 表示名。
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 有効メンバーかどうか。
        /// </summary>
        public bool IsActive { get; }
    }
}

/* --- End of file --- */
