$thirdPartyPackages = @( Get-Project -All | ? { $_.ProjectName } | % { Get-Package -ProjectName $_.ProjectName } ) | Where-Object { !$_.Id.StartsWith('Remotion')  } | Sort -Unique

$output = $thirdPartyPackages | % { 
"$($_.Id)
ProjectUrl: $($_.ProjectUrl)
LicenseUrl: $($_.LicenseUrl)
Description: $($_.Description)
"
}

$output +=
"RhinoMocks
ProjectUrl: http://hibernatingrhinos.com/oss/rhino-mocks
LicenseUrl: http://opensource.org/licenses/bsd-license.php
Description: Mocking Framework
"

$output +=
"SharpZipLib (addendum for license)
License: GPL, with exceptions for Non-GPL-licenses
LicenseUrl: https://github.com/re-motion/IO/tree/develop/license/SharpZipLib-0.86
"

$output +=
"MSBuild Community Tasks
ProjectUrl: https://github.com/loresoft/msbuildtasks/
LicenseUrl: The MSBuild Community Tasks project is a collection of open source tasks for MSBuild.
Description: The MSBuild Community Tasks project is a collection of open source tasks for MSBuild.
"

$output +=
"MSBuild.Extension.Pack
ProjectUrl: https://github.com/mikefourie/MSBuildExtensionPack
LicenseUrl: http://msbuildextensionpack.codeplex.com/license
Description: The MSBuild Extension Pack provides a collection of over 480 MSBuild Tasks, MSBuild Loggers and MSBuild TaskFactories.
"

$output +=
"NuGet.CommandLine
ProjectUrl: http://nuget.codeplex.com/
LicenseUrl: http://www.microsoft.com/web/webpi/eula/nuget_release_eula.htm
Description: NuGet Command Line Tool
"

$output +=
"NuGet.for.MSBuild
ProjectUrl: http://nuget4msbuild.codeplex.com/
LicenseUrl: http://nuget4msbuild.codeplex.com/license
Description: Provides MSBuild tasks around the NuGet command-line utility.
"

$output +=
"NUnit.Runners
ProjectUrl: http://nunit.org/
LicenseUrl: http://nunit.org/nuget/license.html
Description: Test runner for NUnit
"

$output +=
"SandcastleHelpFileBuilder
ProjectUrl: https://github.com/EWSoftware/SHFB 
LicenseUrl: http://shfb.codeplex.com/license
Description: This package allows you to deploy the Sandcastle Help File Builder tools inside of a project to build help files without installing the tools manually such as on a build server. Some limitations apply.
"

$output +=
"Selenium Core
ProjectUrl: http://www.seleniumhq.org/projects/ide/
LicenseUrl: https://github.com/SeleniumHQ/selenium/blob/master/LICENSE
Description: Javascript-based testrunner web clients
"

$output +=
"Subversion Client
ProjectUrl: https://subversion.apache.org/
LicenseUrl: http://www.apache.org/licenses/LICENSE-2.0
Description: Source Control Client
"

$output +=
"Used only as tool:
* MSBuild
* NuGet
* NUnit
* RhinoMocks
* SandcastleHelpFileBuilder
* Selenium Core
* Subversion Client
"

$output > 3rdParty.txt

write-host Written to 3rdParty.txt