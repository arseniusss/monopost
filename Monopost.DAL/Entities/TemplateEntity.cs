namespace Monopost.DAL.Entities
{
    public class Template
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Text { get; set; }
        public int AuthorId { get; set; }

        public ICollection<TemplateFile>? TemplateFiles { get; set; }
    }
}