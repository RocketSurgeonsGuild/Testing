#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.5.1";

Task("Default")
    .IsDependentOn("dotnet");

RunTarget(Target);
