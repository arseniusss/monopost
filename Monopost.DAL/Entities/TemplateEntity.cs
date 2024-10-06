using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopost.DAL.Entities
{
    public class Template
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public int AuthorId { get; set; }

        public User Author { get; set; }
        public ICollection<TemplateFile> TemplateFiles { get; set; }
    }
}
