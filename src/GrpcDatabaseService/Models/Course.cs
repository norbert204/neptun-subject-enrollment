namespace GrpcDatabaseService.Models
{
    /// <summary>
    /// Represents a course offering for a subject
    /// </summary>
    public class Course
    {
        /// <summary>
        /// Unique identifier for the course (also serves as owner reference)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The room where the course is held
        /// </summary>
        public string Room { get; set; }

        /// <summary>
        /// Weekly start time of the course
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// Weekly end time of the course
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// Maximum number of students that can enroll
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// List of student IDs who are enrolled in the course
        /// </summary>
        public List<string> EnrolledStudents { get; set; } = new List<string>();

        /// <summary>
        /// Type of course (lecture, lab, etc.)
        /// </summary>
        public string CourseType { get; set; }
    }
}