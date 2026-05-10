namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// プロジェクト単位のメンバー稼働条件を提供するインターフェイス
    /// </summary>
    public interface IProjectMemberAvailabilityProvider
    {
        /// <summary>
        /// 指定メンバーが対象日にプロジェクトへ参加しているかどうかを取得する
        /// </summary>
        /// <param name="projectId">対象プロジェクトID</param>
        /// <param name="memberId">対象メンバーID</param>
        /// <param name="date">対象日</param>
        /// <returns>参加期間内の場合はtrue</returns>
        bool IsParticipating(Guid projectId, Guid memberId, DateOnly date);

        /// <summary>
        /// 指定メンバーの対象日におけるプロジェクト作業可能時間を取得する
        /// </summary>
        /// <param name="projectId">対象プロジェクトID</param>
        /// <param name="member">対象メンバー</param>
        /// <param name="date">対象日</param>
        /// <returns>作業可能時間。単位は分</returns>
        int GetEffectiveWorkTimeMinutes(Guid projectId, TeamMemberViewModel member, DateOnly date);
    }
}

/* --- End of file --- */
