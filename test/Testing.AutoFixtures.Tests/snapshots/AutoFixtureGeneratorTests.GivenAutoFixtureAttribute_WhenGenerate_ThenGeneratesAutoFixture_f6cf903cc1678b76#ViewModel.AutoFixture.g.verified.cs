//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/ViewModel.AutoFixture.g.cs
using System.Collections.ObjectModel;
using Application.Features.ViewModels;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Application.Tests.Features.ViewModels
{
    internal sealed partial class ViewModelFixture : AutoFixtureBase<ViewModelFixture>
    {
        public static implicit operator ViewModel(ViewModelFixture fixture) => fixture.Build();
        public ViewModelFixture WithThing(Application.Features.ViewModels.IThing thing) => With(ref _thing, thing);
        private ViewModel Build() => new ViewModel(_thing);
        private Application.Features.ViewModels.IThing _thing = Substitute.For<Application.Features.ViewModels.IThing>();
    }
}
