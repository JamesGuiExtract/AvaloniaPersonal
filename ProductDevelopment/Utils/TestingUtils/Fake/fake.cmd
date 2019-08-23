@ECHO OFF
PUSHD %~dp0

SET TOOL_PATH=%~dp0.fake

IF NOT EXIST "%TOOL_PATH%\fake.exe" (
  dotnet tool install fake-cli --tool-path %TOOL_PATH%
)

"%TOOL_PATH%\fake.exe" %*

POPD
