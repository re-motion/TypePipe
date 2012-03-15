@echo off
pushd %~dp0
set msbuild="C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
if not exist remotion.snk goto nosnk

echo Building re-motion without docs using %msbuild%...
echo.

mkdir build\BuildOutput\log

%msbuild% build\Remotion.build /t:PackageAll /maxcpucount /verbosity:minimal /flp:verbosity=normal;logfile=build\BuildOutput\log\build.log "/p:Version=0.0.0"
    
if not %ERRORLEVEL%==0 goto build_failed

if not %ERRORLEVEL%==0 goto zip_failed

goto build_succeeded

:build_failed
echo.
echo Building re-motion has failed.
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
echo remotion.snk does not exists. Please run Generate-Snk.cmd from a Visual Studio Command Prompt.
pause
popd
exit /b 2