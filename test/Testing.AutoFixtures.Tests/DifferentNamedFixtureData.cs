using FakeItEasy;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class DifferentNamedFixtureData : AutoFixtureSourceData
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
    public static class Upsert
    {
        public record Command : ICommand;

        public class CommandHandler : ICommandHandler<Command>
        {
            public CommandHandler(IThing thing)
            {
                _thing = thing;
            };

            public Task Handle(Command command) => Task.CompletedTask;

            private IThing _thing;
        }
    }

    public interface IThing {}
}";

    private const string AttributedSource = @"using System;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo
{
    public static class Upsert
    {
        public record Command : ICommand;

        [AutoFixture]
        public class CommandHandler : ICommandHandler<Command>
        {
            public CommandHandler(IThing thing)
            {
                _thing = thing;
            };

            public Task Handle(Command command) => Task.CompletedTask;

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
    [AutoFixture(typeof(Upsert.CommandHandler))]
    internal partial class DifferentNamedFixture
    {
    }
}";
}
