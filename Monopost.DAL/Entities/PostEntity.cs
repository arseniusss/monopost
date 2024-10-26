namespace Monopost.DAL.Entities
{
    public class Post
    {
        public int PostId { get; set; }
        public int AuthorId { get; set; }
        public DateTime DatePosted { get; set; }

        public User? Author { get; set; }
        public ICollection<PostMedia>? PostMedia { get; set; }
    }
}