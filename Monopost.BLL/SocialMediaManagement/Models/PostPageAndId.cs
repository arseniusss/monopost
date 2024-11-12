using Monopost.DAL.Enums;

namespace Monopost.BLL.SocialMediaManagement.Models
{
    public class PostPageAndId
    {
        public string Page { get; set; }
        public string Id { get; set; }
        public SocialMediaType SocialMedia { get; set; }
        public PostPageAndId(string page, string id, SocialMediaType socialMediaType)
        {
            Page = page;
            Id = id;
            SocialMedia = socialMediaType;
        }
    }
}