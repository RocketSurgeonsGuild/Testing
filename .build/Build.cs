using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Rocket.Surgery.Nuke.DotNetCore;

[PublicAPI]
[UnsetVisualStudioEnvironmentVariables]
[PackageIcon("https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png")]
//[EnsureGitHooks(GitHook.PreCommit)]
[EnsureReadmeIsUpdated("Readme.md")]
[DotNetVerbosityMapping]
[MSBuildVerbosityMapping]
[NuGetVerbosityMapping]
[ShutdownDotNetAfterServerBuild]
[LocalBuildConventions]
public partial class Pipeline : NukeBuild,
    ICanRestoreWithDotNetCore,
    ICanBuildWithDotNetCore,
    ICanTestWithDotNetCore,
    ICanPackWithDotNetCore,
    IHaveDataCollector,
    ICanClean,
    ICanLintStagedFiles,
    ICanDotNetFormat,
    ICanUpdateReadme,
    IGenerateCodeCoverageReport,
    IGenerateCodeCoverageSummary,
    IGenerateCodeCoverageBadges,
    ICanRegenerateBuildConfiguration,
    IHaveConfiguration<Configuration>

{
    /// <summary>
    ///     Support plugins are available for:
    ///     - JetBrains ReSharper        https://nuke.build/resharper
    ///     - JetBrains Rider            https://nuke.build/rider
    ///     - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///     - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>
    public static int Main()
    {
        return Execute<Pipeline>(x => x.Default);
    }

    public Target Default => _ => _
                                 .DependsOn(Restore)
                                 .DependsOn(Build)
                                 .DependsOn(Test)
                                 .DependsOn(Pack);

    [OptionalGitRepository]
    public GitRepository? GitRepository { get; }

    [Solution(GenerateProjects = true)]
    private Solution Solution { get; } = null!;

    public Target Build => _ => _.Inherit<ICanBuildWithDotNetCore>(x => x.CoreBuild);

    public Target Pack => _ => _
                              .Inherit<ICanPackWithDotNetCore>(x => x.CorePack)
                              .DependsOn(Clean)
                              .After(Test);

    public Target Clean => _ => _.Inherit<ICanClean>(x => x.Clean);
    public Target Restore => _ => _.Inherit<ICanRestoreWithDotNetCore>(x => x.CoreRestore);
    Nuke.Common.ProjectModel.Solution IHaveSolution.Solution => Solution;

    [ComputedGitVersion]
    public GitVersion GitVersion { get; } = null!;

    public Target Test => _ => _.Inherit<ICanTestWithDotNetCore>(x => x.CoreTest);

    [Parameter("Configuration to build")]
    public Configuration Configuration { get; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;
}