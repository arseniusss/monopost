using Monopost.DAL.Entities;

namespace Monopost.BLL.Models
{
    public class ExtractedUserData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public List<DecodedCredential> Credentials { get; set; }
        public List<ExtractedTemplateModel> Templates { get; set; }
        public List<ExtractedPostModel> Posts { get; set; }
    }
}