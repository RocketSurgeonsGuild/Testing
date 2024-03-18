using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     A marked location
/// </summary>
/// <param name="Location"></param>
/// <param name="Trigger"></param>
public record MarkedLocation(TextSpan Location, CompletionTrigger? Trigger);