using System;
using System.Collections.Generic;

namespace GrpcDatabaseService.Models
{
    public enum CourseType
    {
        Lecture,
        Lab
    }

    public class Course
    {
        public string ID { get; set; }
        public string Room { get; set; }
        public string StartTime { get; set; } 
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public List<string> EnrolledStudents { get; set; } = new List<string>();
        public CourseType CourseType { get; set; }
    }
}
