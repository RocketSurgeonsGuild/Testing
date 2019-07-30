using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Npgsql;
using Rocket.Surgery.LocalDevelopment;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.Testing.Docker;

namespace Rocket.Surgery.LocalDevelopment
{
    public static class PortRandomizer
    {
        public static int GetPort(string assemblyName, int startingPortRange, int endingPortRange)
        {
            var code = Math.Abs(BitConverter.ToInt32(MD5.Create().ComputeHash(Encoding.Default.GetBytes($"{GitRemoteAutomation.RemoteName}@{assemblyName}")), 0));
            var max = ((double)code) / int.MaxValue;
            var range = endingPortRange - startingPortRange;
            return (int)(range * max + startingPortRange);
        }
    }

    public class PostgresAutomation : IEnsureContainerIsRunningContext
    {
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly string _containerName;
        private readonly string _databaseName;
        private CancellationToken _token;
        private readonly List<string> _logs = new List<string>();
        private readonly string _originalDatabaseName;

        public static PostgresAutomation ForLocalDevelopment(TimeSpan timeout = default)
        {
            if (timeout == default) timeout = TimeSpan.FromSeconds(30);
            var containerName = $"pgsql-development";

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = "localhost",
                Port = 5432,
                Username = "myuser",
                Password = "mypassword",
                Database = "development",
            };
            return new PostgresAutomation(connectionStringBuilder.ToString(), containerName, GitRemoteAutomation.RemoteName, timeout);
        }

        public static PostgresAutomation ForOther(string containerName, TimeSpan timeout = default)
        {
            if (timeout == default) timeout = TimeSpan.FromSeconds(30);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = "localhost",
                Port = PortRandomizer.GetPort(containerName, 40000, 49999),
                Username = "myuser",
                Password = "mypassword",
                Database = "development",
            };
            return new PostgresAutomation(connectionStringBuilder.ToString(), containerName, GitRemoteAutomation.RemoteName, timeout);
        }

        public static PostgresAutomation ForUnitTesting(Assembly assembly, TimeSpan timeout = default)
        {
            if (timeout == default) timeout = TimeSpan.FromMinutes(5);

            var databaseName = "pgsql" + Guid.NewGuid().ToString("N");
            var containerName = $"pgsql-tests";

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = "localhost",
                Port = 65432,
                Username = "myuser",
                Password = "mypassword",
                Database = "unittesting_db",
            };

            return new PostgresAutomation(connectionStringBuilder.ToString(), containerName, databaseName, timeout);
        }

        public static PostgresAutomation ForUnitTesting(Type type, TimeSpan timeout = default)
        {
            return ForUnitTesting(type.Assembly, timeout);
        }

        private PostgresAutomation(string connectionString, string containerName, string finalDatabaseName, TimeSpan timeout)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("message", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("message", nameof(containerName));

            if (timeout == default) timeout = TimeSpan.FromMinutes(5);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);
            _token = cts.Token;

            ConnectionString = new NpgsqlConnectionStringBuilder(connectionString);
            _containerName = containerName;
            _databaseName = finalDatabaseName ?? ConnectionString.Database;
            _originalDatabaseName = ConnectionString.Database;

            Client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"))
                .CreateClient();
        }

        public NpgsqlConnectionStringBuilder ConnectionString { get; }
        public DockerClient Client { get; }
        public string Id { get; private set; }
        public IEnumerable<string> Logs => _logs;

        public async Task Start()
        {
            await SemaphoreSlim.WaitAsync(_token);

            _logs.Add("Creating database container (if not created)");

            Id = await EnsureContainerIsRunningExtensions.EnsureContainerIsRunning(Client, this, _token);

            _logs.Add("Waiting for the database to be available");

            await WaitForDatabaseToBeAvailable();

            SemaphoreSlim.Release();

            await CreateDatabase();

            ConnectionString.Database = _databaseName;
        }

        ContainerListResponse IEnsureContainerIsRunningContext.GetContainer(IList<ContainerListResponse> responses)
        {
            return responses.SingleOrDefault(x => x.Names.Any(name => name == $"/{_containerName}"));
        }

        CreateContainerParameters IEnsureContainerIsRunningContext.CreateContainer(CreateContainerParameters createContainerParameters)
        {
            return new CreateContainerParameters()
            {
                Name = _containerName,
                Image = "postgres:9.6",
                Env = new List<string>()
                {
                    $"POSTGRES_USER={ConnectionString.Username}",
                    $"POSTGRES_PASSWORD={ConnectionString.Password}",
                    $"POSTGRES_DB={ConnectionString.Database}",
                },
                HostConfig = new HostConfig()
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>()
                    {
                        {
                            "5432/tcp",
                            new List<PortBinding>() {new PortBinding() {HostPort = ConnectionString.Port.ToString()}}
                        }
                    }
                },
            };
        }

        ContainerStartParameters IEnsureContainerIsRunningContext.StartContainer(ContainerStartParameters containerStartParameters)
        {
            return containerStartParameters;
        }

        ImagesCreateParameters IEnsureContainerIsRunningContext.CreateImage(ImagesCreateParameters imagesCreateParameters)
        {
            imagesCreateParameters.FromImage = "postgres";
            imagesCreateParameters.Tag = "9.6";
            return imagesCreateParameters;
        }

        async Task WaitForDatabaseToBeAvailable()
        {
            while (!_token.IsCancellationRequested)
                try
                {
                    using (var c = new NpgsqlConnection(ConnectionString.ToString()))
                    {
                        c.Open();
                        var command = c.CreateCommand();
                        command.CommandText = "SELECT * from pg_catalog.pg_tables;";
                        command.ExecuteNonQuery();
                    }
                    return;
                }
                catch
                {
                    await Task.Delay(50, _token);
                }
        }

        async Task CreateDatabase()
        {
            using (var c = new NpgsqlConnection(ConnectionString.ToString()))
            {
                c.Open();
                var cc = c.CreateCommand();
                cc.CommandText = $"SELECT EXISTS (SELECT 1 FROM pg_database WHERE datname = '{_databaseName}')";
                var hasDatabase = (bool)await cc.ExecuteScalarAsync(_token);
                if (!hasDatabase)
                {
                    var command = c.CreateCommand();
                    command.CommandText = $"CREATE DATABASE {_databaseName};";
                    await command.ExecuteNonQueryAsync(_token);
                }
            }
        }

        public async Task DropDatabase()
        {
            if (_originalDatabaseName == _databaseName) return;

            var connectionString = new NpgsqlConnectionStringBuilder(ConnectionString.ToString());
            connectionString.Database = _originalDatabaseName;
            using (var c = new NpgsqlConnection(connectionString.ToString()))
            {
                c.Open();
                var command = c.CreateCommand();
                command.CommandText = $@"
UPDATE pg_database SET datallowconn = 'false' WHERE datname = '{_databaseName}';
ALTER DATABASE {_databaseName} CONNECTION LIMIT 1;

SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{_databaseName}';

DROP DATABASE {_databaseName};";
                var result = await command.ExecuteNonQueryAsync(_token);
            }
        }
    }
}
