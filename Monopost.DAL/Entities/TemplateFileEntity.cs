namespace Monopost.DAL.Entities
{
    public class TemplateFile
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public required string FileName { get; set; }
        public required byte[] FileData { get; set; }

        public Template? Template { get; set; }
    }
}
