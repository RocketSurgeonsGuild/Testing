; Release tracking: https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 10.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
Rsaf0001 | Support | Info | Rsaf0001: `[AutoFixture]` target has no constructor; classes without constructors are not supported
Rsaf0002 | Support | Info | Rsaf0002: a constructor uses a `params` array, which auto fixtures don't support; use `IEnumerable<T>` instead
Rsaf0003 | Support | Info | Rsaf0003: the class has multiple constructors; the generated fixture uses the one with the most parameters
