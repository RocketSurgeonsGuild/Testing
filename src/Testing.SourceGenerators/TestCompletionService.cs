using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal class TestCompletionService : CompletionServiceWithProviders
{
    public TestCompletionService(Workspace workspace, string language, CompletionProvider provider)
        : base(workspace)
    {
        Language = language;
        TestProviders = new[] { provider, }.ToImmutableArray();
    }

    public ImmutableArray<CompletionProvider> TestProviders { get; }

    public override string Language { get; }

    protected override ImmutableArray<CompletionProvider> GetProviders(ImmutableHashSet<string> roles, CompletionTrigger trigger)
    {
        return TestProviders;
    }
}