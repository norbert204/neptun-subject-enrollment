using System.Collections.Generic;

namespace GrpcDatabaseService.Models
{
    public class Subject
    {
        public string ID { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
        public List<string> Prerequisites { get; set; } = new List<string>();
        public List<string> Courses { get; set; } = new List<string>();
    }
}
