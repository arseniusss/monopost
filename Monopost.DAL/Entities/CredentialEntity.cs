namespace Monopost.DAL.Entities
{
    public class Credential
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string CredentialType { get; set; }
        public string CredentialValue { get; set; }
        public bool StoredLocally { get; set; }
        public string LocalPath { get; set; }

        public User Author { get; set; }
    }
}
