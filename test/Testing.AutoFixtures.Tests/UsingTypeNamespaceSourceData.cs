using FakeItEasy;

using NSubstitute;

using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class UsingTypeNamespaceSourceData : AutoFixtureSourceData
{
    public static TheoryData<GeneratorTestContext> Data =>
        [
            DefaultBuilder()
               .AddReferences(typeof(Fake))
               .AddSources(ClassSource, AttributedFixtureSource)
               .Build(),
            DefaultBuilder()
               .AddReferences(typeof(Substitute))
               .AddSources(ClassSource, AttributedFixtureSource)
               .Build(),
            DefaultBuilder()
               .AddReferences(typeof(Fake))
               .AddSources(AttributedSource)
               .Build(),
            DefaultBuilder()
               .AddReferences(typeof(Substitute))
               .AddSources(AttributedSource)
               .Build(),
        ];

    private const string ClassSource = @"
namespace Application.Features.ViewModels
{
    public class ViewModel
    {
        public ViewModel(IThing thing)
        {
        }
    }

    public interface IThing
    {
    }
}";

    private const string AttributedSource = @"using System;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Application.Features.ViewModels
{
    [AutoFixture]
    public class ViewModel
    {
        public ViewModel(IThing thing)
        {
        }
    }

    public interface IThing
    {
    }
}";

    private const string AttributedFixtureSource = @"using System;
using Application.Features.ViewModels;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Application.Tests.Features.ViewModels
{
    [AutoFixture(typeof(ViewModel))]
    internal partial class ViewModelFixture
    {
    }
}";
}
