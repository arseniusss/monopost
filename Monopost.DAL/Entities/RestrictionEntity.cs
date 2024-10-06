using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopost.DAL.Entities
{
    public class Restriction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RestrictionType { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateEnded { get; set; }

        public User User { get; set; }
    }
}
