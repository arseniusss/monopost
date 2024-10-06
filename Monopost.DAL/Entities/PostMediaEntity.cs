namespace Monopost.DAL.Entities
{
    public class PostMedia
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string SocialMediaName { get; set; }
        public string ChannelId { get; set; }
        public string MessageId { get; set; }

        public Post Post { get; set; }
    }
}
