@echo off
pushd %~dp0

set program-path=%ProgramFiles%
set program-pathX86=%ProgramFiles(x86)%
if not exist "%program-pathX86%" set program-pathX86=%program-path%
set msbuild="%program-pathX86%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
if not exist %msbuild% set msbuild="%program-path%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
if not exist %msbuild% set msbuild="%program-path%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
if not exist %msbuild% set msbuild="%program-path%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

set log-dir=build\BuildOutput\log
set nuget-bin=build\BuildOutput\temp\nuget-bin
set nuget=%nuget-bin%\nuget.exe
set nuget-download=powershell.exe -NoProfile -Command "& {(New-Object System.Net.WebClient).DownloadFile('https://dist.nuget.org/win-x86-commandline/latest/nuget.exe','%nuget%')}"
set solution=Remotion-TypePipe.sln

if not exist remotion.snk goto nosnk

if not [%1]==[] goto %1
	
echo Welcome to the re-motion build tool!
echo.
echo Using %msbuild%
echo.
echo Choose your desired build:
echo [1] ... Test build ^(x86-debug^)
echo [2] ... Full build ^(x86-debug/release, x64-debug/release, create packages^)
echo [3] ... Docs build ^(x86-debug if not present, docs^)
echo           Requires Sandcastle Help File Builder to be installed!
echo [4] ... Package ^(create NuGet packages in .\Build\BuildOutput^)
echo [5] ... Run DependDB
echo [6] ... Oops, nothing please - exit.
echo.

choice /c:123456 /n /m "Your choice: "

if %ERRORLEVEL%==1 goto run_test_build
if %ERRORLEVEL%==2 goto run_full_build
if %ERRORLEVEL%==3 goto run_docs_build
if %ERRORLEVEL%==4 goto run_pkg_build
if %ERRORLEVEL%==5 goto run_dependdb
if %ERRORLEVEL%==6 goto run_exit
goto build_succeeded

:run_test_build
mkdir %log-dir%
mkdir %nuget-bin%
%nuget-download%
%nuget% restore %solution% -NonInteractive
%msbuild% build\Remotion.Local.build /t:TestBuild /maxcpucount /verbosity:normal /flp:verbosity=normal;logfile=build\BuildOutput\log\build.log
if not %ERRORLEVEL%==0 goto build_failed
goto build_succeeded

:run_full_build
mkdir %log-dir%
mkdir %nuget-bin%
%nuget-download%
%nuget% restore %solution% -NonInteractive
%msbuild% build\Remotion.Local.build /t:FullBuildWithoutDocumentation /maxcpucount /verbosity:normal /flp:verbosity=normal;logfile=build\BuildOutput\log\build.log
if not %ERRORLEVEL%==0 goto build_failed
goto build_succeeded

:run_docs_build
mkdir %log-dir%
mkdir %nuget-bin%
%nuget-download%
%nuget% restore %solution% -NonInteractive
%msbuild% build\Remotion.Local.build /t:DocumentationBuild /maxcpucount /verbosity:minimal /flp:verbosity=normal;logfile=build\BuildOutput\log\build.log
if not %ERRORLEVEL%==0 goto build_failed
goto build_succeeded

:run_pkg_build
mkdir %log-dir%
mkdir %nuget-bin%
%nuget-download%
%nuget% restore %solution% -NonInteractive
%msbuild% build\Remotion.Local.build /t:PackageBuild /maxcpucount /verbosity:minimal /flp:verbosity=normal;logfile=build\BuildOutput\log\build.log
if not %ERRORLEVEL%==0 goto build_failed
goto build_succeeded

:run_dependdb
mkdir %log-dir%
mkdir %nuget-bin%
%nuget-download%
%nuget% restore %solution% -NonInteractive
%msbuild% build\Remotion.Local.build /t:DependDBBuild /maxcpucount /verbosity:normal /flp:verbosity=detailed;logfile=build\BuildOutput\log\build.log
if not %ERRORLEVEL%==0 goto build_failed
goto build_succeeded

:run_exit
exit /b 0


:build_failed
echo.
echo Building %solution% has failed.
start build\BuildOutput\log\build.log
pause
popd
exit /b 1

:build_succeeded
echo.
pause
popd
exit /b 0

:nosnk
echo remotion.snk does not exist. Please run Generate-Snk.cmd from a Visual Studio Command Prompt.
pause
popd
exit /b 2
