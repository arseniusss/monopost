namespace Monopost.DAL.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required int Age { get; set; }
        public ICollection<Template>? Templates { get; set; }
        public ICollection<Jar>? Jars { get; set; }
        public ICollection<Credential>? Credentials { get; set; }
        public ICollection<Restriction>? Restrictions { get; set; }
        public ICollection<Post>? Posts { get; set; }
    }
}