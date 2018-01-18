using System.Collections.Generic;
using Docker.DotNet.Models;

namespace Rocket.Surgery.Extensions.Testing.Docker
{
    public interface IEnsureContainerIsRunningContext
    {
        ContainerListResponse GetContainer(IList<ContainerListResponse> responses);
        CreateContainerParameters CreateContainer(CreateContainerParameters createContainerParameters);
        ContainerStartParameters StartContainer(ContainerStartParameters containerStartParameters);

    }
}