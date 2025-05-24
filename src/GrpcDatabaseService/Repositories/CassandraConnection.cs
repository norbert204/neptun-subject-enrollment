using Cassandra;

namespace GrpcDatabaseService.Repositories
{
    /// <summary>
    /// Manages the Cassandra database connection
    /// </summary>
    public class CassandraConnection
    {
        private readonly Cassandra.ISession _session;

        /// <summary>
        /// Initializes a new instance of the CassandraConnection class
        /// </summary>
        /// <param name="contactPoints">Comma-separated list of Cassandra contact points</param>
        /// <param name="keyspace">The keyspace to connect to</param>
        public CassandraConnection(string contactPoints, string keyspace)
        {
            try
            {
                var cluster = Cluster.Builder()
                .AddContactPoints(contactPoints.Split(','))
                .WithQueryTimeout(10000)
                .WithReconnectionPolicy(new ConstantReconnectionPolicy(1000))
                .Build();

                Console.WriteLine($"Connecting to Cassandra at {contactPoints}...");
                _session = cluster.Connect(keyspace);
                Console.WriteLine("Connected to Cassandra successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to Cassandra: {ex.Message}");
                throw;
            }
            
        }

        /// <summary>
        /// Gets the Cassandra session
        /// </summary>
        public Cassandra.ISession Session => _session;
    }
}