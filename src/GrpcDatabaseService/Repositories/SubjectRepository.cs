using Cassandra;
using GrpcDatabaseService.Models;
using GrpcDatabaseService.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using ISession = Cassandra.ISession;

namespace GrpcDatabaseService.Repositories
{
    /// <summary>
    /// Cassandra implementation of the Subject repository
    /// </summary>
    public class SubjectRepository : ISubjectRepository
    {
        private readonly ISession _session;
        private readonly ILogger<SubjectRepository> _logger;
        private readonly PreparedStatement _createStatement;
        private readonly PreparedStatement _getStatement;
        private readonly PreparedStatement _updateStatement;
        private readonly PreparedStatement _deleteStatement;
        private readonly PreparedStatement _listStatement;

        /// <summary>
        /// Initializes a new instance of the SubjectRepository class
        /// </summary>
        public SubjectRepository(CassandraConnection connection, ILogger<SubjectRepository> logger)
        {
            _session = connection.Session;
            _logger = logger;

            // Prepare statements for better performance
            _createStatement = _session.Prepare(
                "INSERT INTO subjects (id, owner, name, prerequisites, courses) VALUES (?, ?, ?, ?, ?)");
            _getStatement = _session.Prepare(
                "SELECT * FROM subjects WHERE id = ?");
            _updateStatement = _session.Prepare(
                "UPDATE subjects SET owner = ?, name = ?, prerequisites = ?, courses = ? WHERE id = ?");
            _deleteStatement = _session.Prepare(
                "DELETE FROM subjects WHERE id = ?");
            _listStatement = _session.Prepare(
                "SELECT * FROM subjects");
        }

        /// <inheritdoc/>
        public async Task<Subject> CreateSubjectAsync(Subject subject)
        {
            try
            {
                var boundStatement = _createStatement.Bind(
                    subject.Id,
                    subject.Owner,
                    subject.Name,
                    subject.Prerequisites,
                    subject.Courses);

                await _session.ExecuteAsync(boundStatement);
                return subject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject with ID: {SubjectId}", subject.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Subject?> GetSubjectAsync(string id)
        {
            try
            {
                var boundStatement = _getStatement.Bind(id);
                var rows = await _session.ExecuteAsync(boundStatement);
                var row = rows.FirstOrDefault();

                if (row == null)
                    return null;

                return new Subject
                {
                    Id = row.GetValue<string>("id"),
                    Owner = row.GetValue<string>("owner"),
                    Name = row.GetValue<string>("name"),
                    Prerequisites = row.GetValue<List<string>>("prerequisites"),
                    Courses = row.GetValue<List<string>>("courses"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subject with ID: {SubjectId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Subject> UpdateSubjectAsync(Subject subject)
        {
            try
            {
                var boundStatement = _updateStatement.Bind(
                    subject.Owner,
                    subject.Name,
                    subject.Prerequisites,
                    subject.Courses,
                    subject.Id);

                await _session.ExecuteAsync(boundStatement);
                return subject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject with ID: {SubjectId}", subject.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteSubjectAsync(string id)
        {
            try
            {
                var boundStatement = _deleteStatement.Bind(id);
                var result = await _session.ExecuteAsync(boundStatement);
                return result.IsFullyFetched;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject with ID: {SubjectId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Subject>> ListSubjectsAsync()
        {
            try
            {
                var boundStatement = _listStatement.Bind();
                var rows = await _session.ExecuteAsync(boundStatement);

                var subjects = new List<Subject>();
                foreach (var row in rows)
                {
                    subjects.Add(new Subject
                    {
                        Id = row.GetValue<string>("id"),
                        Owner = row.GetValue<string>("owner"),
                        Name = row.GetValue<string>("name"),
                        Prerequisites = row.GetValue<List<string>>("prerequisites"),
                        Courses = row.GetValue<List<string>>("courses"),
                    });
                }

                return subjects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing subjects");
                throw;
            }
        }
    }
}