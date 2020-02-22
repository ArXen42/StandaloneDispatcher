using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("NuGet Api Key")] readonly string   ApiKey;
    [Solution]                   readonly Solution Solution;

    Project      LibraryProject  => Solution.GetProject("StandaloneDispatcher");
    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .After(Clean)
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target Pack => _ => _
        .DependsOn(Clean, Restore)
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(LibraryProject)
                .SetConfiguration(Configuration.Release)
                .SetOutputDirectory(OutputDirectory)
            );
        });

    Target Publish => _ => _
        .DependsOn(Pack)
        .Requires(() => ApiKey)
        .Executes(() =>
        {
            DotNetNuGetPush(_ => _
                .SetTargetPath(OutputDirectory.GlobFiles($"{LibraryProject.Name}.*.nupkg").Single())
                .SetSource("https://api.nuget.org/v3/index.json")
                .SetApiKey(ApiKey)
            );
        });
}