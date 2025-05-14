namespace GrpcDatabaseService.Models
{
    public class User
    {
        public string NeptunCode { get; set; } // ID
        public string Name { get; set; } // Contains First, Last, Middle
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
