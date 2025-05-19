namespace GrpcDatabaseService.Models
{
    /// <summary>
    /// Represents an academic subject in the system
    /// </summary>
    public class Subject
    {
        /// <summary>
        /// Unique identifier for the subject
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The owner or instructor of the subject
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// The name of the subject
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of prerequisite subject IDs required before taking this subject
        /// </summary>
        public List<string> Prerequisites { get; set; } = new List<string>();

        /// <summary>
        /// List of course IDs associated with this subject
        /// </summary>
        public List<string> Courses { get; set; } = new List<string>();
    }
}