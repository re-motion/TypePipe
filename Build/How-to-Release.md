# Install Release Process Automation Tool

`dotnet tool install Remotion.ReleaseProcessAutomation -g --add-source https://nuget.re-motion.org/nuget/re-motion-infrastructure/ --prerelease`

# Perform a Release

On the commandline, use `releasetool` to start the process.

# Reference Section

`.BuildProject`: located in the solution root. Indicates the location of the configuration file.

`Build/Customizations/releaseProcessScript.config`: The release tool configuration file.
Contains the MSBuild version and other solution specific details relevant for releasing a new version.