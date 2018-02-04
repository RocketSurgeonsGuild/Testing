using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Rocket.Surgery.Extensions.Testing.Docker
{
    public static class EnsureContainerIsRunningExtensions
    {
        public static Task<string> EnsureContainerIsRunning(
                DockerClient client,
                IEnsureContainerIsRunningContext context)
        {
            return EnsureContainerIsRunningInternal(client, context);
        }

        public static Task<string> EnsureContainerIsRunning(
                DockerClient client,
                Func<IList<ContainerListResponse>, ContainerListResponse> getContainer,
                Func<CreateContainerParameters, CreateContainerParameters> createContainer,
                Func<ContainerStartParameters, ContainerStartParameters> startContainer,
            Func<ImagesCreateParameters, ImagesCreateParameters> imageCreate)
        {
            return EnsureContainerIsRunningInternal(
                client,
                new EnsureContainerIsRunningContext(
                    getContainer,
                    createContainer,
                    startContainer,
                    imageCreate
                )
            );
        }

        private static async Task<string> EnsureContainerIsRunningInternal(
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
                var createParams = context.CreateContainer(new CreateContainerParameters());
                await client.Images.CreateImageAsync(
                    context.CreateImage(new ImagesCreateParameters()),
                    new AuthConfig(),
                    new Progress<JSONMessage>()
                );
                var container = await client.Containers.CreateContainerAsync(createParams);
                id = container.ID;
            }
            else
            {
                id = listContainer.ID;
            }

            await client.Containers.StartContainerAsync(id, context.StartContainer(new ContainerStartParameters()));
            return id;
        }
    }
}
