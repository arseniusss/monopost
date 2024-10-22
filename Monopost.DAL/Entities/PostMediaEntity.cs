﻿using Monopost.DAL.Enums;
namespace Monopost.DAL.Entities
{
    public class PostMedia
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public required SocialMediaType SocialMediaName { get; set; }
        public required string ChannelId { get; set; }
        public required string MessageId { get; set; }
    }
}
