namespace Monopost.BLL.Models
{
    public class ExtractedTemplateModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Text { get; set; }
        public int AuthorId { get; set; }

        public List<ExtractedTemplateFileModel>? TemplateFiles { get; set; }
    }

    public class ExtractedTemplateFileModel
    {
        public required string FileName { get; set; }
        public required byte[] FileData { get; set; }
    }
}
