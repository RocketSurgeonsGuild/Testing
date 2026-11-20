using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     A class to represent a code markup
/// </summary>
/// <param name="markup">The markup, with <c>[| |]</c> spans, <c>[|]</c> locations or <c>[* *]</c> completion triggers.</param>
public class CodeMarkup(string markup)
{
    // Point markers ([|] / [*]) are checked first: "[|]" also contains "[|" and "|]", which would otherwise parse as an empty span.
    private static TextSpan GetLocation(string markupCode) =>
        TryFindMarkedLocation(markupCode, out var ts) ? ts
        : TryFindMarkedTextSpan(markupCode, out var textSpan) ? textSpan
        : new();

    private static CompletionTrigger? GetCompletionTrigger(string markupCode) =>
        TryFindCompletion(markupCode, out var ts) ? ts
        : TryFindCompletionCharacter(markupCode, out var textSpan) ? textSpan
        : null;

    private static bool TryFindMarkedTextSpan(string markupCode, out TextSpan textSpan)
    {
        textSpan = default;
        var start = markupCode.IndexOf("[|", StringComparison.Ordinal);
        if (start == -1) start = markupCode.IndexOf("[*", StringComparison.Ordinal);
        if (start < 0) return false;

        var end = markupCode.IndexOf("|]", start, StringComparison.Ordinal);
        if (end == -1) end = markupCode.IndexOf("*]", start, StringComparison.Ordinal);
        if (end < 0) return false;

        // Bounds are in the stripped code: the opening marker (2 chars) sits before the marked text.
        textSpan = TextSpan.FromBounds(start, end - 2);
        return true;
    }

    private static bool TryFindMarkedLocation(string markupCode, out TextSpan textSpan)
    {
        textSpan = default;
        var start = markupCode.IndexOf("[|]", StringComparison.Ordinal);
        if (start == -1) start = markupCode.IndexOf("[*]", StringComparison.Ordinal);
        if (start < 0) return false;

        textSpan = new(start, 0);
        return true;
    }

    private static bool TryFindCompletionCharacter(string markupCode, out CompletionTrigger trigger)
    {
        trigger = CompletionTrigger.Invoke;
        var start = markupCode.IndexOf("[*", StringComparison.Ordinal);
        if (start < 0) return false;

        var end = markupCode.IndexOf("*]", start, StringComparison.Ordinal);
        if (end < 0) return false;

        trigger = markupCode[start + 2] == '-'
            ? CompletionTrigger.CreateDeletionTrigger(markupCode[end - 1])
            : CompletionTrigger.CreateInsertionTrigger(markupCode[end - 1]);
        return true;
    }

    private static bool TryFindCompletion(string markupCode, out CompletionTrigger trigger)
    {
        trigger = CompletionTrigger.Invoke;
        return markupCode.Contains("[*]", StringComparison.Ordinal);
    }

    /// <summary>
    ///     The location of the code
    /// </summary>
    public TextSpan Location { get; } = GetLocation(markup);

    /// <summary>
    ///     The completion trigger
    /// </summary>
    public CompletionTrigger? Trigger { get; } = GetCompletionTrigger(markup);

    /// <summary>
    ///     The source code
    /// </summary>
    public string Code { get; } = markup
                                 .Replace("[|]", "")
                                 .Replace("[*]", "")
                                 .Replace("[|", "")
                                 .Replace("|]", "")
                                 .Replace("[*", "")
                                 .Replace("*]", "");
}
