@echo off

set cwd=%CD%

cd %WINDIR%\Microsoft.NET\Framework\v4*
cd %WINDIR%\Microsoft.NET\Framework64\v4*

echo Compiling the project...
MSBuild.exe "%cwd%\..\Cygnus.sln" /t:Build /p:Configuration=Release /p:Platform="Any CPU"

cd %cwd%\..\Cygnus\bin\Release
echo Merging Dlls...
..\..\..\build\ILMerge.exe /wildcards /targetplatform:v4 /out:..\..\..\build\Cygnus.exe Cygnus.exe *.dll

cd %cwd%

echo Done!

pause