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
        private readonly Func<ImagesCreateParameters, ImagesCreateParameters> _imageCreate;

        public EnsureContainerIsRunningContext(
            Func<IList<ContainerListResponse>, ContainerListResponse> getContainer,
            Func<CreateContainerParameters, CreateContainerParameters> createContainer,
            Func<ContainerStartParameters, ContainerStartParameters> startContainer,
            Func<ImagesCreateParameters, ImagesCreateParameters> imageCreate
        )
        {
            _getContainer = getContainer ?? throw new ArgumentNullException(nameof(getContainer));
            _createContainer = createContainer ?? throw new ArgumentNullException(nameof(createContainer));
            _startContainer = startContainer ?? throw new ArgumentNullException(nameof(startContainer));
            _imageCreate = imageCreate ?? throw new ArgumentNullException(nameof(imageCreate));
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

        public ImagesCreateParameters CreateImage(ImagesCreateParameters imagesCreateParameters)
        {
            return _imageCreate(imagesCreateParameters);
        }
    }
}
