@echo off

if [%1] equ [] (
    echo No path supplied in args
    exit /b 1
)

REM Building
dotnet publish "%0\..\..\ZombieSurvival.csproj" --output %1 -r win-x64 --self-contained
if %errorlevel% neq 0 (
	echo Build failed
	exit /b %errorlevel%
)

REM Coping folders
xcopy "%0\..\..\shaders" "%1\shaders" /E /I /Y /Q
xcopy "%0\..\..\resources" "%1\resources" /E /I /Y /Q
DEL "%1\resources\scenes\*.*" /F /Q
