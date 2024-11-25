using Monopost.DAL.Entities;

namespace Monopost.BLL.Models
{
    public class UserData
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public List <DecodedCredential> Credentials { get; set; }
        public List<Template> templates { get; set; }
        public List<TemplateFile> TemplateFiles { get; set; }
        public List <PostMedia> PostMedia { get; set; }
    }
}