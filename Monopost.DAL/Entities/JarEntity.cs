namespace Monopost.DAL.Entities
{
    public class Jar
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string? ShortJarId { get; set; }
        public string? LongJarId { get; set; }

        public User? Owner { get; set; }
    }
}
