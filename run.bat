@echo off
echo Building and running Sentiment Analysis Data Processor...
echo.

dotnet build
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Build successful! Starting application...
echo.

dotnet run

echo.
echo Application completed.
pause
