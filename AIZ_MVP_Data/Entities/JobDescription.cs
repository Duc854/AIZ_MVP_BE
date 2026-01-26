using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Entities
{
    public class JobDescription
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string RoleId { get; set; } = null!; 
        public string Sector { get; set; } = null!;
        public string Level { get; set; } = null!;
        public string? Description { get; set; }
    }
}
