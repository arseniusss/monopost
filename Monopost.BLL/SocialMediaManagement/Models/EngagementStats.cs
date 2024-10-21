namespace Monopost.BLL.SocialMediaManagement.Models
{
    public class EngagementStats
    {
        private int Views { get; set; }
        private int Reactions { get; set; }
        private int Comments { get; set; }
        private int Forwards { get; set; }
        public EngagementStats(int views, int reactions, int comments, int forwards)
        {
            Views = views;
            Reactions = reactions;
            Comments = comments;
            Forwards = forwards;
        }
    }
}