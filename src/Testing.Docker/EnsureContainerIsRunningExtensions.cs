using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Rocket.Surgery.Extensions.Testing.Docker
{
    public static class EnsureContainerIsRunningExtensions
    {
        public static Task<bool> EnsureContainerIsRunning(
                DockerClient client,
                IEnsureContainerIsRunningContext context)
        {
            return EnsureContainerIsRunningInternal(client, context);
        }

        public static Task<bool> EnsureContainerIsRunning(
                DockerClient client,
                Func<IList<ContainerListResponse>, ContainerListResponse> getContainer,
                Func<CreateContainerParameters, CreateContainerParameters> createContainer,
                Func<ContainerStartParameters, ContainerStartParameters> startContainer)
        {
            return EnsureContainerIsRunningInternal(client, new EnsureContainerIsRunningContext(getContainer, createContainer, startContainer));
        }

        private static async Task<bool> EnsureContainerIsRunningInternal(
            DockerClient client,
            IEnsureContainerIsRunningContext context)
        {
            var containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters()
                {
                    All = true,
                });

            string id;
            var listContainer = context.GetContainer(containers);
            if (listContainer is null)
            {
                // docker run --name some-postgres  -e POSTGRES_PASSWORD=mysecretpassword -p 5432:5432 -d postgres
                var container =
                    await client.Containers.CreateContainerAsync(
                        context.CreateContainer(new CreateContainerParameters())
                    );
                id = container.ID;
            }
            else
            {
                id = listContainer.ID;
            }

            return await client.Containers.StartContainerAsync(id, context.StartContainer(new ContainerStartParameters()));
        }
    }
}
