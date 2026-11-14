#:package Microsoft.VisualStudio.SolutionPersistence
#:package Sourcy.Git
#:package Sourcy.DotNet
#:package Rocket.Surgery.ModularPipelines.Extensions
#:package Joinery.Dovetail.ModularPipelines
#:package Specular
#:property JsonSerializerIsReflectionEnabledByDefault=true

using Build;
using Joinery.Dovetail;
using ModularPipelines;

// The library's joints (its modules, settings and run history) are applied by the ConfigureDovetail() that Joinery.Dovetail.ModularPipelines
// generates here; Dovetail reads build/appsettings.yaml from this file's folder.
await using var pipeline = await Pipeline.CreateBuilder(args).ConfigureDovetail();
await pipeline.RunAsync();
