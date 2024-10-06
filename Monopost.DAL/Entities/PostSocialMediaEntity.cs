using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopost.DAL.Entities
{
    public class PostSocialMedia
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string SocialMediaName { get; set; }
        public string ChannelId { get; set; }
        public string MessageId { get; set; }

        public Post Post { get; set; }
    }
}
