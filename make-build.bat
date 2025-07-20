@echo off

if [%1] equ [] (
    echo No path supplied in args
    exit /b 1
)

REM Building
dotnet build "%0\..\ZombieSurvival.csproj" --output %1
if %errorlevel% neq 0 (
	echo Build failed
	exit /b %errorlevel%
)

REM Coping folders
xcopy "%0\..\shaders" "%1\shaders\" /E /I /Y
xcopy "%0\..\resources" "%1\resources\" /E /I /Y
DEL "%1\resources\*.*" /F /Q
