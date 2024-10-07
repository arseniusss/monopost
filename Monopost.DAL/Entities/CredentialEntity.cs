namespace Monopost.DAL.Entities
{
    public class Credential
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public required string CredentialType { get; set; }
        public required string CredentialValue { get; set; }
        public bool StoredLocally { get; set; }
        public string? LocalPath { get; set; }

        public User? Author { get; set; }
    }
}
