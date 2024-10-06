namespace Monopost.DAL.Entities
{
    public class TemplateFile
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }

        public Template Template { get; set; }
    }
}
