using Monopost.DAL.Enums;
namespace Monopost.BLL.Models
{
    public class CredentialModel
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public required CredentialType CredentialType { get; set; }
        public string? CredentialValue { get; set; }
        public bool StoredLocally { get; set; }
        public string? LocalPath { get; set; }
    }
}