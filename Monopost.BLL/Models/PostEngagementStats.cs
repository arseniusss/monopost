using Monopost.DAL.Enums;

namespace Monopost.BLL.Models
{
    public class PostEngagementStats
    {
        public int Views { get; set; }
        public int Reactions { get; set; }
        public int Comments { get; set; }
        public int Forwards { get; set; }
        public int PostMediaId { get; set; }
        
        public PostEngagementStats(int id, int views, int reactions, int comments, int forwards)
        {
            Views = views;
            Reactions = reactions;
            Comments = comments;
            Forwards = forwards;
            PostMediaId = id;
        }
    }
}