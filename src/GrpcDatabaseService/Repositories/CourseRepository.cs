using Cassandra;
using GrpcDatabaseService.Models;
using GrpcDatabaseService.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using ISession = Cassandra.ISession;

namespace GrpcDatabaseService.Repositories
{
    /// <summary>
    /// Cassandra implementation of the Course repository
    /// </summary>
    public class CourseRepository : ICourseRepository
    {
        private readonly ISession _session;
        private readonly ILogger<CourseRepository> _logger;
        private readonly PreparedStatement _createStatement;
        private readonly PreparedStatement _getStatement;
        private readonly PreparedStatement _updateStatement;
        private readonly PreparedStatement _deleteStatement;
        private readonly PreparedStatement _listStatement;

        /// <summary>
        /// Initializes a new instance of the CourseRepository class
        /// </summary>
        public CourseRepository(CassandraConnection connection, ILogger<CourseRepository> logger)
        {
            _session = connection.Session;
            _logger = logger;

            // Prepare statements for better performance
            _createStatement = _session.Prepare(
                "INSERT INTO courses (id, room, start_time, end_time, capacity, enrolled_students, course_type) VALUES (?, ?, ?, ?, ?, ?, ?)");
            _getStatement = _session.Prepare(
                "SELECT * FROM courses WHERE id = ?");
            _updateStatement = _session.Prepare(
                "UPDATE courses SET room = ?, start_time = ?, end_time = ?, capacity = ?, enrolled_students = ?, course_type = ? WHERE id = ?");
            _deleteStatement = _session.Prepare(
                "DELETE FROM courses WHERE id = ?");
            _listStatement = _session.Prepare(
                "SELECT * FROM courses");
        }

        /// <inheritdoc/>
        public async Task<Course> CreateCourseAsync(Course course)
        {
            try
            {
                var boundStatement = _createStatement.Bind(
                    course.Id,
                    course.Room,
                    course.StartTime,
                    course.EndTime,
                    course.Capacity,
                    course.EnrolledStudents,
                    course.CourseType);

                await _session.ExecuteAsync(boundStatement);
                return course;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course with ID: {CourseId}", course.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Course?> GetCourseAsync(string id)
        {
            try
            {
                var boundStatement = _getStatement.Bind(id);
                var rows = await _session.ExecuteAsync(boundStatement);
                var row = rows.FirstOrDefault();

                if (row == null)
                    return null;

                return new Course
                {
                    Id = row.GetValue<string>("id"),
                    Room = row.GetValue<string>("room"),
                    StartTime = row.GetValue<string>("start_time"),
                    EndTime = row.GetValue<string>("end_time"),
                    Capacity = row.GetValue<int>("capacity"),
                    EnrolledStudents = row.GetValue<List<string>>("enrolled_students"),
                    CourseType = row.GetValue<string>("course_type")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course with ID: {CourseId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Course> UpdateCourseAsync(Course course)
        {
            try
            {
                var boundStatement = _updateStatement.Bind(
                    course.Room,
                    course.StartTime,
                    course.EndTime,
                    course.Capacity,
                    course.EnrolledStudents,
                    course.CourseType,
                    course.Id);

                await _session.ExecuteAsync(boundStatement);
                return course;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course with ID: {CourseId}", course.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCourseAsync(string id)
        {
            try
            {
                var boundStatement = _deleteStatement.Bind(id);
                var result = await _session.ExecuteAsync(boundStatement);
                return result.IsFullyFetched;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course with ID: {CourseId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Course>> ListCoursesAsync()
        {
            try
            {
                var boundStatement = _listStatement.Bind();
                var rows = await _session.ExecuteAsync(boundStatement);

                var courses = new List<Course>();
                foreach (var row in rows)
                {
                    courses.Add(new Course
                    {
                        Id = row.GetValue<string>("id"),
                        Room = row.GetValue<string>("room"),
                        StartTime = row.GetValue<string>("start_time"),
                        EndTime = row.GetValue<string>("end_time"),
                        Capacity = row.GetValue<int>("capacity"),
                        EnrolledStudents = row.GetValue<List<string>>("enrolled_students"),
                        CourseType = row.GetValue<string>("course_type")
                    });
                }

                return courses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing courses");
                throw;
            }
        }
    }
}