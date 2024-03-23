namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
/// A method that allows for emitting extra data from the generator test results into verify
/// </summary>
public delegate void GeneratorTestResultsCustomizer(GeneratorTestResults results, List<Target> targets, Dictionary<string, object> data);
