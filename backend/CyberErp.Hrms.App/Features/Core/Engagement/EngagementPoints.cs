namespace CyberErp.Hrms.App.Features.Core.Engagement
{
    /// <summary>
    /// HC209 — gamification: points credited to the reward-points ledger for engagement actions.
    /// Anonymous submissions earn nothing (crediting would link them to a person).
    /// </summary>
    public static class EngagementPoints
    {
        public const int ForumTopic = 5;
        public const int ForumReply = 2;
        public const int SurveyResponse = 10;
        public const int Suggestion = 5;
    }
}
