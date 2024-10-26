using Monopost.DAL.Enums;

namespace Monopost.BLL.Models
{
    public class DecodedCredential
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public required CredentialType CredentialType { get; set; }
        public required string CredentialValue { get; set; }
    }
}