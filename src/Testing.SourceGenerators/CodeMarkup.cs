﻿using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     A class to represent a code markup
/// </summary>
public class CodeMarkup
{
    private static TextSpan GetLocation(string markupCode)
    {
        if (TryFindMarkedTextSpan(markupCode, out var textSpan)) return textSpan;

        if (TryFindMarkedLocation(markupCode, out var ts)) return ts;

        return new();
    }

    private static CompletionTrigger? GetCompletionTrigger(string markupCode)
    {
        if (TryFindCompletion(markupCode, out var ts)) return ts;

        if (TryFindCompletionCharacter(markupCode, out var textSpan)) return textSpan;

        return null;
    }

    private static bool TryFindMarkedTextSpan(string markupCode, out TextSpan textSpan)
    {
        textSpan = default;
        var start = markupCode.IndexOf("[|", StringComparison.InvariantCulture);
        if (start == -1) start = markupCode.IndexOf("[*", StringComparison.InvariantCulture);
        if (start < 0) return false;

        var end = markupCode.IndexOf("|]", start, StringComparison.InvariantCulture);
        if (end == -1) end = markupCode.IndexOf("*]", StringComparison.InvariantCulture);
        if (end < 0) return false;

        textSpan = TextSpan.FromBounds(start, ( end - 2 ) + 1);
        return true;
    }

    private static bool TryFindMarkedLocation(string markupCode, out TextSpan textSpan)
    {
        textSpan = default;
        var start = markupCode.IndexOf("[|]", StringComparison.InvariantCulture);
        if (start == -1) start = markupCode.IndexOf("[*]", StringComparison.InvariantCulture);
        if (start < 0) return false;

        textSpan = TextSpan.FromBounds(start, start + 1);
        return true;
    }

    private static bool TryFindCompletionCharacter(string markupCode, out CompletionTrigger trigger)
    {
        trigger = CompletionTrigger.Invoke;
        var start = markupCode.IndexOf("[*", StringComparison.InvariantCulture);
        if (start < 0) return false;

        var end = markupCode.IndexOf("*]", start, StringComparison.InvariantCulture);
        if (end < 0) return false;

        trigger = markupCode[start + 2] == '-'
            ? CompletionTrigger.CreateDeletionTrigger(markupCode[end - 1])
            : CompletionTrigger.CreateInsertionTrigger(markupCode[end - 1]);
        return true;
    }

    private static bool TryFindCompletion(string markupCode, out CompletionTrigger trigger)
    {
        trigger = CompletionTrigger.Invoke;
        var start = markupCode.IndexOf("[*]", StringComparison.InvariantCulture);
        return start >= 0;
    }

    /// <summary>
    ///     Create a new instance of the <see cref="CodeMarkup" /> class
    /// </summary>
    /// <param name="markup"></param>
    public CodeMarkup(string markup)
    {
        Code = markup
              .Replace("[|", "")
              .Replace("|]", "")
              .Replace("[|]", "")
              .Replace("[*]", "")
              .Replace("[*", "")
              .Replace("*]", "")
            ;
        Location = GetLocation(markup);
        Trigger = GetCompletionTrigger(markup);
    }

    /// <summary>
    ///     The location of the code
    /// </summary>
    public TextSpan Location { get; }

    /// <summary>
    ///     The completion trigger
    /// </summary>
    public CompletionTrigger? Trigger { get; }

    /// <summary>
    ///     The source code
    /// </summary>
    public string Code { get; }
}