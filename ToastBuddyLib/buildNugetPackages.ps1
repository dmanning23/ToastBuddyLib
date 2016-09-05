rm *.nupkg
nuget pack .\ToastBuddyLib.nuspec -IncludeReferencedProjects -Prop Configuration=Release
nuget push *.nupkg