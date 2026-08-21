@echo off
set "ROOT=%~dp0"
start "API - Proyecto Atlas" cmd /k "cd /d ""%ROOT%"" && set ASPNETCORE_ENVIRONMENT=Development && dotnet run --project .\src\Gimnasio.Api --urls http://localhost:5085"
start "Vue - Proyecto Atlas" cmd /k "cd /d ""%ROOT%frontend\gimnasio-web"" && npm.cmd run dev"
timeout /t 4 /nobreak >nul
start "" http://localhost:5173
