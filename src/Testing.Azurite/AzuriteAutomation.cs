using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Rocket.Surgery.Extensions.Testing.Docker;

namespace Rocket.Surgery.LocalDevelopment
{
    public class AzuriteAutomation : IEnsureContainerIsRunningContext
    {
        private const int DevelopmentPort = 10000;
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly CancellationToken _token;
        private readonly int _port;
        private readonly string _containerName;
        private readonly List<string> _logs = new List<string>();

        public static AzuriteAutomation ForLocalDevelopment(TimeSpan timeout = default)
        {
            if (timeout == default) timeout = TimeSpan.FromSeconds(30);
            var containerName = $"azurite-development";

            return new AzuriteAutomation(DevelopmentPort, containerName, timeout);
        }

        public static AzuriteAutomation ForOther(string containerName, TimeSpan timeout = default)
        {
            if (timeout == default) timeout = TimeSpan.FromSeconds(30);

            return new AzuriteAutomation(PortRandomizer.GetPort(containerName, 12000, 19998), containerName, timeout);
        }

        public static AzuriteAutomation ForUnitTesting(Assembly assembly, TimeSpan timeout = default)
        {
            if (timeout == default) timeout = TimeSpan.FromMinutes(5);
            var containerName = $"azurite-tests";
            return new AzuriteAutomation(11000, containerName, timeout);
        }

        public static AzuriteAutomation ForUnitTesting(Type type, TimeSpan timeout = default)
        {
            return ForUnitTesting(type.Assembly, timeout);
        }

        private AzuriteAutomation(int port, string containerName, TimeSpan timeout)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("message", nameof(containerName));

            if (port == DevelopmentPort)
            {
                ConnectionString = "UseDevelopmentStorage=true";
            }
            else
            {
                var connectionStringBuilder = new StringBuilder();
                connectionStringBuilder.Append($"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;");
                connectionStringBuilder.Append($"AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;");
                connectionStringBuilder.Append($"BlobEndpoint=http://127.0.0.1:{port}/devstoreaccount1;");
                connectionStringBuilder.Append($"TableEndpoint=http://127.0.0.1:{port + 2}/devstoreaccount1;");
                connectionStringBuilder.Append($"QueueEndpoint=http://127.0.0.1:{port + 1}/devstoreaccount1;");

                ConnectionString = connectionStringBuilder.ToString();
            }

            if (timeout == default) timeout = TimeSpan.FromMinutes(5);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);
            _token = cts.Token;

            _port = port;
            _containerName = containerName;

            Client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"))
                .CreateClient();
        }

        public string ConnectionString { get; }
        public DockerClient Client { get; }
        public string Id { get; private set; }
        public IEnumerable<string> Logs => _logs;

        public async Task Start()
        {
            await SemaphoreSlim.WaitAsync(_token);

            _logs.Add("Creating database container (if not created)");

            Id = await EnsureContainerIsRunningExtensions.EnsureContainerIsRunning(Client, this, _token);

            _logs.Add("Waiting for the database to be available");

            SemaphoreSlim.Release();
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
                Image = "daviddriscoll/azurite:latest",
                HostConfig = new HostConfig()
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>()
                    {
                        {
                            "10000/tcp",
                            new List<PortBinding>() {new PortBinding() {HostPort = (_port + 0).ToString()}}
                        },
                        {
                            "10001/tcp",
                            new List<PortBinding>() {new PortBinding() {HostPort = (_port + 1).ToString()}}
                        },
                        {
                            "10002/tcp",
                            new List<PortBinding>() {new PortBinding() {HostPort = (_port + 2).ToString()}}
                        },
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
            imagesCreateParameters.FromImage = "daviddriscoll/azurite";
            imagesCreateParameters.Tag = "latest";
            return imagesCreateParameters;
        }
    }
}
