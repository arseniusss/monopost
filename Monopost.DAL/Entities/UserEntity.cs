namespace Monopost.DAL.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<Template> Templates { get; set; }
        public ICollection<Jar> Jars { get; set; }
        public ICollection<Credential> Credentials { get; set; }
        public ICollection<Restriction> Restrictions { get; set; }
        public ICollection<Post> Posts { get; set; }
    }
}
