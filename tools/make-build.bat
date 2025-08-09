@echo off
setlocal

set csproj="%0\..\..\ZombieSurvival.csproj"
set build_folder=%1
set root_path=%0
mkdir %build_folder%

if [%1] equ [] (
    echo No path supplied in args
    exit /b 1
)

CALL :Build win-x64
if %errorlevel% neq 0 (
    echo Build failed
    exit /b %errorlevel%
)
CALL :Build win-x86
if %errorlevel% neq 0 (
    echo Build failed
    exit /b %errorlevel%
)
CALL :Build linux-x64
if %errorlevel% neq 0 (
    echo Build failed
    exit /b %errorlevel%
)
CALL :Build osx-x64
if %errorlevel% neq 0 (
    echo Build failed
    exit /b %errorlevel%
)

endlocal
EXIT /B %ERRORLEVEL%

:Build
    REM Building
    set os_build_path=%build_folder%\%~1

    echo Building %~1
    dotnet publish %csproj% --output %os_build_path% -r %~1 --self-contained
    if %errorlevel% neq 0 (
        echo Build failed
        exit /b %errorlevel%
    )

    REM Coping folders
    xcopy "%root_path%\..\..\shaders" "%os_build_path%\shaders" /E /I /Y /Q
    xcopy "%root_path%\..\..\resources" "%os_build_path%\resources" /E /I /Y /Q
    DEL "%os_build_path%\resources\scenes\*.*" /F /Q
EXIT /B 0