using Monopost.BLL.Models;

namespace Monopost.DAL.Entities
{
    public class ExtractedPostModel
    {
        public DateTime DatePosted { get; set; }

        public ICollection<ExtractedPostMediaModel>? PostMedia { get; set; }
    }
}