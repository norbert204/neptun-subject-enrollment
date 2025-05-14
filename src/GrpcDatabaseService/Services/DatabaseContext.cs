using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GrpcDatabaseService.Models;

namespace GrpcDatabaseService.Services
{
    public class DatabaseContext
    {
        private const string USERS_FILE = "users.json";
        private const string COURSES_FILE = "courses.json";
        private const string SUBJECTS_FILE = "subjects.json";
        private readonly string _dataDirectory;

        public DatabaseContext(string dataDirectory)
        {
            _dataDirectory = dataDirectory;
            EnsureDirectoryExists();
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            // Create empty JSON files if they don't exist
            EnsureFileExists(Path.Combine(_dataDirectory, USERS_FILE), new List<User>());
            EnsureFileExists(Path.Combine(_dataDirectory, COURSES_FILE), new List<Course>());
            EnsureFileExists(Path.Combine(_dataDirectory, SUBJECTS_FILE), new List<Subject>());
        }

        private void EnsureFileExists<T>(string filePath, T defaultContent)
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, JsonSerializer.Serialize(defaultContent, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
            }
        }

        #region Users

        public async Task<List<User>> GetAllUsersAsync()
        {
            string json = await File.ReadAllTextAsync(Path.Combine(_dataDirectory, USERS_FILE));
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public async Task<User> GetUserByIdAsync(string neptunCode)
        {
            var users = await GetAllUsersAsync();
            return users.Find(u => u.NeptunCode == neptunCode);
        }

        public async Task AddUserAsync(User user)
        {
            var users = await GetAllUsersAsync();

            // Check if user already exists
            if (users.Exists(u => u.NeptunCode == user.NeptunCode))
            {
                throw new Exception($"User with NEPTUN code {user.NeptunCode} already exists");
            }

            users.Add(user);
            await SaveUsersAsync(users);
        }

        public async Task UpdateUserAsync(User user)
        {
            var users = await GetAllUsersAsync();
            var index = users.FindIndex(u => u.NeptunCode == user.NeptunCode);

            if (index == -1)
            {
                throw new Exception($"User with NEPTUN code {user.NeptunCode} not found");
            }

            users[index] = user;
            await SaveUsersAsync(users);
        }

        public async Task DeleteUserAsync(string neptunCode)
        {
            var users = await GetAllUsersAsync();
            var user = users.Find(u => u.NeptunCode == neptunCode);
            //var user = await GetUserByIdAsync(neptunCode);

            if (user == null)
            {
                throw new Exception($"User with NEPTUN code {neptunCode} not found");
            }

            users.Remove(user);
            await SaveUsersAsync(users);
        }

        private async Task SaveUsersAsync(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(Path.Combine(_dataDirectory, USERS_FILE), json);
        }

        #endregion

        #region Courses

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            string json = await File.ReadAllTextAsync(Path.Combine(_dataDirectory, COURSES_FILE));
            return JsonSerializer.Deserialize<List<Course>>(json) ?? new List<Course>();
        }

        public async Task<Course> GetCourseByIdAsync(string id)
        {
            var courses = await GetAllCoursesAsync();
            return courses.Find(c => c.ID == id);
        }

        public async Task AddCourseAsync(Course course)
        {
            var courses = await GetAllCoursesAsync();

            // Check if course already exists
            if (courses.Exists(c => c.ID == course.ID))
            {
                throw new Exception($"Course with ID {course.ID} already exists");
            }

            courses.Add(course);
            await SaveCoursesAsync(courses);
        }

        public async Task UpdateCourseAsync(Course course)
        {
            var courses = await GetAllCoursesAsync();
            var index = courses.FindIndex(c => c.ID == course.ID);

            if (index == -1)
            {
                throw new Exception($"Course with ID {course.ID} not found");
            }

            courses[index] = course;
            await SaveCoursesAsync(courses);
        }

        public async Task DeleteCourseAsync(string id)
        {
            var courses = await GetAllCoursesAsync();
            var course = courses.Find(c => c.ID == id);

            if (course == null)
            {
                throw new Exception($"Course with ID {id} not found");
            }

            courses.Remove(course);
            await SaveCoursesAsync(courses);
        }

        public async Task EnrollStudentAsync(string courseId, string neptunCode)
        {
            var courses = await GetAllCoursesAsync();
            var course = courses.Find(c => c.ID == courseId);

            if (course == null)
            {
                throw new Exception($"Course with ID {courseId} not found");
            }

            if (course.EnrolledStudents.Contains(neptunCode))
            {
                throw new Exception($"Student with NEPTUN code {neptunCode} is already enrolled in this course");
            }

            if (course.EnrolledStudents.Count >= course.Capacity)
            {
                throw new Exception("Course has reached capacity");
            }

            course.EnrolledStudents.Add(neptunCode);
            await SaveCoursesAsync(courses);
        }

        public async Task RemoveStudentAsync(string courseId, string neptunCode)
        {
            var courses = await GetAllCoursesAsync();
            var course = courses.Find(c => c.ID == courseId);

            if (course == null)
            {
                throw new Exception($"Course with ID {courseId} not found");
            }

            if (!course.EnrolledStudents.Contains(neptunCode))
            {
                throw new Exception($"Student with NEPTUN code {neptunCode} is not enrolled in this course");
            }

            course.EnrolledStudents.Remove(neptunCode);
            await SaveCoursesAsync(courses);
        }

        private async Task SaveCoursesAsync(List<Course> courses)
        {
            string json = JsonSerializer.Serialize(courses, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(Path.Combine(_dataDirectory, COURSES_FILE), json);
        }

        #endregion

        #region Subjects

        public async Task<List<Subject>> GetAllSubjectsAsync()
        {
            string json = await File.ReadAllTextAsync(Path.Combine(_dataDirectory, SUBJECTS_FILE));
            return JsonSerializer.Deserialize<List<Subject>>(json) ?? new List<Subject>();
        }

        public async Task<Subject> GetSubjectByIdAsync(string id)
        {
            var subjects = await GetAllSubjectsAsync();
            return subjects.Find(s => s.ID == id);
        }

        public async Task AddSubjectAsync(Subject subject)
        {
            var subjects = await GetAllSubjectsAsync();

            // Check if subject already exists
            if (subjects.Exists(s => s.ID == subject.ID))
            {
                throw new Exception($"Subject with ID {subject.ID} already exists");
            }

            subjects.Add(subject);
            await SaveSubjectsAsync(subjects);
        }

        public async Task UpdateSubjectAsync(Subject subject)
        {
            var subjects = await GetAllSubjectsAsync();
            var index = subjects.FindIndex(s => s.ID == subject.ID);

            if (index == -1)
            {
                throw new Exception($"Subject with ID {subject.ID} not found");
            }

            subjects[index] = subject;
            await SaveSubjectsAsync(subjects);
        }

        public async Task DeleteSubjectAsync(string id)
        {
            var subjects = await GetAllSubjectsAsync();
            var subject = subjects.Find(s => s.ID == id);

            if (subject == null)
            {
                throw new Exception($"Subject with ID {id} not found");
            }

            subjects.Remove(subject);
            await SaveSubjectsAsync(subjects);
        }

        public async Task AddCourseToSubjectAsync(string subjectId, string courseId)
        {
            var subjects = await GetAllSubjectsAsync();
            var subject = subjects.Find(s => s.ID == subjectId);

            if (subject == null)
            {
                throw new Exception($"Subject with ID {subjectId} not found");
            }

            if (subject.Courses.Contains(courseId))
            {
                throw new Exception($"Course with ID {courseId} is already associated with this subject");
            }

            // Verify course exists
            var course = await GetCourseByIdAsync(courseId);
            if (course == null)
            {
                throw new Exception($"Course with ID {courseId} not found");
            }

            subject.Courses.Add(courseId);
            await SaveSubjectsAsync(subjects);
        }

        public async Task RemoveCourseFromSubjectAsync(string subjectId, string courseId)
        {
            var subjects = await GetAllSubjectsAsync();
            var subject = subjects.Find(s => s.ID == subjectId);

            if (subject == null)
            {
                throw new Exception($"Subject with ID {subjectId} not found");
            }

            if (!subject.Courses.Contains(courseId))
            {
                throw new Exception($"Course with ID {courseId} is not associated with this subject");
            }

            subject.Courses.Remove(courseId);
            await SaveSubjectsAsync(subjects);
        }

        public async Task AddPrerequisiteToSubjectAsync(string subjectId, string prerequisiteId)
        {
            var subjects = await GetAllSubjectsAsync();
            var subject = subjects.Find(s => s.ID == subjectId);

            if (subject == null)
            {
                throw new Exception($"Subject with ID {subjectId} not found");
            }

            if (subject.Prerequisites.Contains(prerequisiteId))
            {
                throw new Exception($"Subject with ID {prerequisiteId} is already a prerequisite for this subject");
            }

            // Verify prerequisite subject exists
            var prerequisite = await GetSubjectByIdAsync(prerequisiteId);
            if (prerequisite == null)
            {
                throw new Exception($"Prerequisite subject with ID {prerequisiteId} not found");
            }

            subject.Prerequisites.Add(prerequisiteId);
            await SaveSubjectsAsync(subjects);
        }

        public async Task RemovePrerequisiteFromSubjectAsync(string subjectId, string prerequisiteId)
        {
            var subjects = await GetAllSubjectsAsync();
            var subject = subjects.Find(s => s.ID == subjectId);

            if (subject == null)
            {
                throw new Exception($"Subject with ID {subjectId} not found");
            }

            if (!subject.Prerequisites.Contains(prerequisiteId))
            {
                throw new Exception($"Subject with ID {prerequisiteId} is not a prerequisite for this subject");
            }

            subject.Prerequisites.Remove(prerequisiteId);
            await SaveSubjectsAsync(subjects);
        }

        private async Task SaveSubjectsAsync(List<Subject> subjects)
        {
            string json = JsonSerializer.Serialize(subjects, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(Path.Combine(_dataDirectory, SUBJECTS_FILE), json);
        }

        #endregion
    }
}