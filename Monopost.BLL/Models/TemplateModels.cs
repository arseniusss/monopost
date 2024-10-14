namespace Monopost.BLL.Models
{
    public class TemplateModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Text { get; set; }
        public int AuthorId { get; set; }

        public List<TemplateFileModel>? TemplateFiles { get; set; }
    }
    public class TemplateFileModel
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public required string FileName { get; set; }
        public required byte[] FileData { get; set; }
    }
}