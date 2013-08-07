ECHO OFF

REM This set before the check since the "(" in the environment variable cause problems in an IF statement
REM when it they are expanded
SET PROGRAM_ROOT=%ProgramFiles(x86)%

IF NOT "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	SET PROGRAM_ROOT=%ProgramFiles%
)

IF DEFINED BUILD_LOCAL (
	SET BUILD_DRIVE=%BUILD_LOCAL%
) ELSE (
	SET BUILD_DRIVE=D:
)
SET BUILD_DIRECTORY=\temp
SET PRODUCT_ROOT=Flex

SET VISUAL_STUDIO=%PROGRAM_ROOT%\Microsoft Visual Studio 10.0
SET VB_DIR=%VISUAL_STUDIO%\VB
SET VCPP_DIR=%VISUAL_STUDIO%\VC
SET DevEnvDir=%VISUAL_STUDIO%\Common7\IDE
SET VS_COMMON=%VISUAL_STUDIO%\Common7
SET WINDOWS_SDK=C:\Program Files\Microsoft SDKs\Windows\v7.1

SET VAULT_DIR=%PROGRAM_ROOT%\SourceGear\Vault Client

SET DEV_STUDIO_DIR=%PROGRAM_ROOT%\InstallShield\2010

SET DOTFUSCATOR=%PROGRAM_ROOT%\PreEmptive Solutions\Dotfuscator Professional Edition 4.9
SET FX_COP=%PROGRAM_ROOT%\Microsoft FxCop 1.36