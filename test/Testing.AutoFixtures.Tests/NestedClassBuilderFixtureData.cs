using FakeItEasy;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class NestedClassBuilderFixtureData : AutoFixtureSourceData
{
    public static TheoryData<GeneratorTestContext> Data =>
    [
        DefaultBuilder()
           .AddReferences(typeof(Fake))
           .AddSources(BaseClassSource, ClassSource, AttributedFixtureSource)
           .Build(),
        DefaultBuilder()
           .AddReferences(typeof(Substitute))
           .AddSources(BaseClassSource, ClassSource, AttributedFixtureSource)
           .Build(),
//        DefaultBuilder()
//           .AddReferences(typeof(Fake))
//           .AddSources(AttributedSource)
//           .Build(),
//        DefaultBuilder()
//           .AddReferences(typeof(Substitute))
//           .AddSources(AttributedSource)
//           .Build(),
    ];

    private const string BaseClassSource = @"
public abstract class QueryHandlerBase<T, T1>
{
    protected abstract Task<Search.Result> Handle(Search.Query query, CancellationToken cancellationToken = default);
}

public interface IQuery<T> { }

public class SearchResult { }

public class SearchFilter { }";

    private const string ClassSource = @"

namespace Goony.Goo.Goo
{
    public static class Search
    {
        public record Query(Func<FilterBuilder, SearchFilter> Builder) : IQuery<Result>;

        public record Result(string OriginalQuery, IEnumerable<SearchResult> SearchResults);

        public class Handler : QueryHandlerBase<Query, Result>
        {
            /// <inheritdoc/>
            protected override Task<Result> Handle(Query query, CancellationToken cancellationToken = default)
            {
                var filter = query.Builder.Invoke(FilterBuilder.Create());
                return Task.FromResult(new Result(string.Empty, []));
            }
        }

        public class FilterBuilder
        {
            private FilterBuilder() { }

            public static implicit operator SearchFilter(FilterBuilder builder) => builder.Build();

            public static FilterBuilder Create() => new FilterBuilder();

            private SearchFilter Build()
            {
                throw new NotImplementedException();
            }
        }
    }
}
";

    private const string AttributedFixtureSource = @"using System;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Tests.Goo.Goo
{
    [AutoFixture(typeof(Search.Query))]
    internal partial class SearchQueryFixture
    {
    }
}";
}
