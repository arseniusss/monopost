namespace Monopost.DAL.Entities
{
    public class Restriction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string RestrictionType { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateEnded { get; set; }

        public required User User { get; set; }
    }
}