using System.Globalization;
using System.Text;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests.Generators;

public class CodeMarkupTests
{
    [Test]
    public Task Should_Mark_Span() => Verify(Describe("class [|Foo|] { }"));

    [Test]
    public Task Should_Mark_Point_Location() => Verify(Describe("class Foo { [|] }"));

    [Test]
    public Task Should_Mark_Invoke_Completion() => Verify(Describe("Console.[*]"));

    [Test]
    public Task Should_Mark_Insertion_Completion() => Verify(Describe("Console[*.*]"));

    [Test]
    public Task Should_Have_No_Location_Without_Markers() => Verify(Describe("class Foo { }"));

    [Test]
    // A soft hyphen is ignorable under culture-aware comparison, so "[­|" would match "[|" with InvariantCulture; Ordinal must not.
    public Task Should_Match_Markers_Ordinally() => Verify(Describe("class [­|Foo|] { }"));

    private static object Describe(string markup)
    {
        var codeMarkup = new CodeMarkup(markup);
        return new
        {
            Markup = Escape(markup),
            Code = Escape(codeMarkup.Code),
            Location = codeMarkup.Location.ToString(),
            MarkedText = Escape(codeMarkup.Code.Substring(codeMarkup.Location.Start, codeMarkup.Location.Length)),
            TriggerKind = codeMarkup.Trigger?.Kind.ToString(),
            TriggerCharacter = codeMarkup.Trigger is { Character: not '\0' } trigger ? trigger.Character.ToString() : null,
        };
    }

    // Keeps invisible characters reviewable in the snapshot.
    private static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            _ = c is >= ' ' and <= '~'
                ? builder.Append(c)
                : builder.Append("\\u").Append(( (int)c ).ToString("X4", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
