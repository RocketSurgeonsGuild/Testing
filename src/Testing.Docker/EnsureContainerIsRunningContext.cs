using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Docker.DotNet.Models;

namespace Rocket.Surgery.Extensions.Testing.Docker
{
    class EnsureContainerIsRunningContext : IEnsureContainerIsRunningContext
    {
        private readonly Func<IList<ContainerListResponse>, ContainerListResponse> _getContainer;
        private readonly Func<CreateContainerParameters, CreateContainerParameters> _createContainer;
        private readonly Func<ContainerStartParameters, ContainerStartParameters> _startContainer;

        public EnsureContainerIsRunningContext(
            Func<IList<ContainerListResponse>, ContainerListResponse> getContainer,
            Func<CreateContainerParameters, CreateContainerParameters> createContainer,
            Func<ContainerStartParameters, ContainerStartParameters> startContainer
        )
        {
            _getContainer = getContainer ?? throw new ArgumentNullException(nameof(getContainer));
            _createContainer = createContainer ?? throw new ArgumentNullException(nameof(createContainer));
            _startContainer = startContainer ?? throw new ArgumentNullException(nameof(startContainer));
        }

        public ContainerListResponse GetContainer(IList<ContainerListResponse> responses)
        {
            return _getContainer(responses);
        }

        public CreateContainerParameters CreateContainer(CreateContainerParameters createContainerParameters)
        {
            return _createContainer(createContainerParameters);
        }

        public ContainerStartParameters StartContainer(ContainerStartParameters containerStartParameters)
        {
            return _startContainer(containerStartParameters);
        }
    }
}
