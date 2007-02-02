set-alias nant "C:\Program Files\NAnt\bin.net-2.0\nant.exe";

nant "-f:Commons.build" "-D:solution.global-dir=\Development\global" "-t:net-2.0" "-nologo" `
    cleantemp `
    resources doc-internal;

if ($LastExitCode -ne 0) 
{ 
  [System.Console]::ReadKey($false);
  throw "Build Commons has failed."; 
}

[System.Console]::ReadKey($false);
