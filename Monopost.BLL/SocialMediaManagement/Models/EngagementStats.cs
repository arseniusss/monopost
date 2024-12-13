﻿namespace Monopost.BLL.SocialMediaManagement.Models
{
    public class EngagementStats
    {
        public int Views { get; set; }
        public int Reactions { get; set; }
        public int Comments { get; set; }
        public int Forwards { get; set; }
        public EngagementStats(int views, int reactions, int comments, int forwards)
        {
            Views = views;
            Reactions = reactions;
            Comments = comments;
            Forwards = forwards;
        }
    }
}