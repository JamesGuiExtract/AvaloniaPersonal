ECHO OFF

REM This set before the check since the "(" in the environment variable cause problems in an IF statement
REM when it they are expanded
SET PROGRAM_ROOT=%ProgramFiles(x86)%
SET CSCRIPT_PATH=%SystemRoot%\syswow64

IF NOT "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	SET PROGRAM_ROOT=%ProgramFiles%
	SET CSCRIPT_PATH=%SystemRoot%\System32
)

IF DEFINED BUILD_LOCAL (
	SET BUILD_DRIVE=%BUILD_LOCAL%
) ELSE (
	SET BUILD_DRIVE=D:
)
SET BUILD_DIRECTORY=\temp
SET PRODUCT_ROOT=Flex

SET VISUAL_STUDIO=%PROGRAM_ROOT%\Microsoft Visual Studio\2017\Enterprise
SET VB_DIR=%VISUAL_STUDIO%\VB
SET VCPP_DIR=%VISUAL_STUDIO%\VC
SET DevEnvDir=%VISUAL_STUDIO%\Common7\IDE
SET VS_COMMON=%VISUAL_STUDIO%\Common7
SET WINDOWS_SDK=C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools

SET DEV_STUDIO_DIR=%PROGRAM_ROOT%\InstallShield\2013 SP1 SAB
SET DOTFUSCATOR=%PROGRAM_ROOT%\PreEmptive Solutions\Dotfuscator Professional Edition 4.13.0
SET INSTALLSHIELD_PROJECTS_DIR=C:\InstallShield 2013 Projects
SET MERGE_MODULE_DIR=%INSTALLSHIELD_PROJECTS_DIR%\MergeModules

SET FX_COP=%PROGRAM_ROOT%\Microsoft FxCop 1.36