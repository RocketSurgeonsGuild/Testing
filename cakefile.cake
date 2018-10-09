#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.8.0";

Task("Default")
    .IsDependentOn("dotnetcore");

RunTarget(Target);
