using Monopost.DAL.Enums;

namespace Monopost.BLL.Models
{
    public class ExtractedPostMediaModel
    {
        public required SocialMediaType SocialMediaName { get; set; }
        public required string ChannelId { get; set; }
        public required string MessageId { get; set; }
        public required string MediaUrl { get; set; }
    }
}
