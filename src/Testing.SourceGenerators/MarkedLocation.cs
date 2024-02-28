using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

public record MarkedLocation(TextSpan Location, CompletionTrigger? Trigger);