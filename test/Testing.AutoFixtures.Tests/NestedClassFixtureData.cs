using FakeItEasy;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class NestedClassFixtureData : AutoFixtureSourceData
{
    public static TheoryData<GeneratorTestContext> Data =>
        new()
        {
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
        };

    private const string ClassSource = @"
namespace Goony.Goo.Goo
{
    public static class LoadThings
    {
        public record Query : IQuery<Result>;

        public record Result;

        public class QueryHandler : IQueryHandler<Query, Result>
        {
            public QueryHandler(IThing thing)
            {
                _thing = thing;
            };

            public Task<Result> Handle(Query query) => Task.FromResult(new Result());

            private IThing _thing;
        }
    }

    public interface IThing {}
}";

    private const string AttributedSource = @"using System;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo
{
    public static class LoadThings
    {
        public record Query : IQuery<Result>;

        public record Result;

        [AutoFixture]
        public class QueryHandler : IQueryHandler<Query, Result>
        {
            public QueryHandler(IThing thing)
            {
                _thing = thing;
            };

            public Task<Result> Handle(Query query) => Task.FromResult(new Result());

            private IThing _thing;
        }
    }

    public interface IThing {}
}";

    private const string AttributedFixtureSource = @"using System;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Tests.Goo.Goo
{
    [AutoFixture(typeof(LoadThings.QueryHandler))]
    internal partial class LoadThingsQueryHandlerFixture
    {
    }
}";
}
