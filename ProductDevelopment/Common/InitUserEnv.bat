ECHO OFF

IF DEFINED BUILD_LOCAL (
	SET BUILD_DRIVE=%BUILD_LOCAL%
) ELSE (
	SET BUILD_DRIVE=D:
)
SET BUILD_DIRECTORY=\temp
SET PRODUCT_ROOT=Flex

SET VISUAL_STUDIO=C:\Program Files\Microsoft Visual Studio 10.0
SET VB_DIR=%VISUAL_STUDIO%\VB
SET VCPP_DIR=%VISUAL_STUDIO%\VC
SET DevEnvDir=%VISUAL_STUDIO%\Common7\IDE
SET VS_COMMON=%VISUAL_STUDIO%\Common7
SET WINDOWS_SDK=C:\C:\Program Files\Microsoft SDKs\Windows\v7.1

SET VAULT_DIR=C:\Program Files\SourceGear\Vault Client

SET INSTALL_SHIELD_DIR=C:\Program Files\InstallShield\InstallShield 5.5 Professional Edition
SET WINZIP_DIR=C:\Program Files\WinZip

SET DEV_STUDIO_DIR=C:\Program Files\InstallShield\2010