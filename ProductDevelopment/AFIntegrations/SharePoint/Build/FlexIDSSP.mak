#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

!include ComponentVersions.mak
!include ..\..\..\Common\Common.mak

#############################################################################
# E N S U R E   P R E - C O N D I T I O N S   A R E   M E T
#
# The user must tell which configuration to build (e.g. "Win32 Release")
# Otherwise, we cannot continue.
#
!IF "$(BuildConfig)" == ""
!ERROR Build variable 'BuildConfig' must be defined (e.g. "Release")
!ENDIF

#############################################################################
# M A K E F I L E   V A R I A B L E S
#
PDRootDir=$(EngineeringRootDirectory)\ProductDevelopment
SharePointRootDir=$(BuildRootDirectory)\Engineering\ProductDevelopment\AFIntegrations\SharePoint

PDCommonDir=$(PDRootDir)\Common

SPInstallationDirectory=R:\FlexIndex\Internal\BleedingEdge\$(FlexIndexVersion)\Sharepoint
InternalUseBuildFilesArchive=P:\AttributeFinder\Archive\InternalUseBuildFiles\InternalBuilds\$(FlexIndexVersion)

# determine the name of the release output directory based upon the build
# configuration that is being built
!IF "$(BuildConfig)" == "Release"
BuildOutputDir=Release
!ELSEIF "$(BuildConfig)" == "Debug"
BuildOutputDir=Debug
!ELSE
!ERROR Internal error - invalid value for BuildConfig variable!
!ENDIF

BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)

Branch=

Get=vault GETLABEL 
GetOptions=-server $(VAULT_SERVER) -repository $(VAULT_REPOSITORY) -makewritable 

#############################################################################
# B U I L D    T A R G E T S
#
LabelCommonFolder: 
    $(Label) $$/Engineering/ProductDevelopment/Common -I- -L"$(FlexIndexVersion)" -O

GetExtractSharePointFiles: 
	@ECHO Getting files for Extract.SharePoint...
	@IF NOT EXIST "$(SharePointRootDir)" MKDIR "$(SharePointRootDir)"
	$(BUILD_DRIVE) 
    @CD  "$(SharePointRootDir)"
    $(Get) $(GetOptions) -nonworkingfolder "$(SharePointRootDir)" $$$(Branch)/Engineering/ProductDevelopment/AFIntegrations/SharePoint  "$(FlexIDSSPVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"

GetRCNetAPISharePoint:
	@ECHO Getting files from RC.Net\APIs\SharePoint tree...
	@IF NOT EXIST "$(RCDotNETDir)\APIs\SharePoint" MKDIR "$(RCDotNETDir)\APIs\SharePoint"
	$(BUILD_DRIVE) 
    @CD  "$(RCDotNETDir)\APIs\SharePoint"
    $(Get) $(GetOptions) -nonworkingfolder "$(RCDotNETDir)\APIs\SharePoint" $$$(Branch)/Engineering/RC.Net/APIs/SharePoint  "$(RCDotNetVersion)" 
	
GetRCNetExtensionMethods:
	@ECHO Getting files from RC.Net\ExtensionMethods...
	@IF NOT EXIST "$(RCDotNETDir)\Core\ExtensionMethods" MKDIR "$(RCDotNETDir)\Core\ExtensionMethods"
	$(BUILD_DRIVE) 
    @CD  "$(RCDotNETDir)\Core\ExtensionMethods"
    $(Get) $(GetOptions) -nonworkingfolder "$(RCDotNETDir)\Core\ExtensionMethods" $$$(Branch)/Engineering/RC.Net/Core/ExtensionMethods  "$(RCDotNetVersion)" 

GetRCNetCore:
	@ECHO Getting files from RC.Net Core Files...
	@IF NOT EXIST "$(RCDotNETDir)\Core\Code" MKDIR "$(RCDotNETDir)\Core\Code"
	$(BUILD_DRIVE) 
    @CD  "$(RCDotNETDir)\Core\Code"
    $(Get) $(GetOptions) -nonworkingfolder "$(RCDotNETDir)\Core\Code" $$$(Branch)/Engineering/RC.Net/Core/Code  "$(RCDotNetVersion)" 

BuildExtractSharePoint:
	@ECHO Building Extract.SharePoint...
    @CD "$(SharePointRootDir)"
    @devenv Extract.SharePoint.sln /BUILD $(BuildConfig) /USEENV
	
CopyOriginalFilesBeforeObfuscating: BuildExtractSharePoint
	@ECHO Saving orignal assemblies before obfuscating...
	@IF NOT EXIST "$(BinariesFolder)\OriginalFiles" @MKDIR "$(BinariesFolder)\OriginalFiles"
	@XCOPY "$(BinariesFolder)\*.dll" "$(BinariesFolder)\OriginalFiles\"
	@XCOPY "$(BinariesFolder)\*.exe" "$(BinariesFolder)\OriginalFiles\"
	@XCOPY "$(BinariesFolder)\*.pdb" "$(BinariesFolder)\OriginalFiles\"
	
ObfuscateFiles: BuildExtractSharePoint CopyOriginalFilesBeforeObfuscating
	@ECHO Obfuscating for Extract.SharePoint...
#	Copy the strong naming key to location dotfuscator xml file expects
	@IF NOT EXIST "$(StrongNameKeyDir)" @MKDIR "$(StrongNameKeyDir)"
	@COPY /V "$(RCNETDir)\Core\Code\ExtractInternalKey.snk" "$(StrongNameKeyDir)"
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_17\bin;;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages;C:\Program Files\PreEmptive Solutions\Dotfuscator Professional Edition 4.9
	dotfuscator.exe  /in:"$(BinariesFolder)\IDShieldForSPClient.exe" /mapout:"$(BinariesFolder)\Map\mapIDShieldForSPClient.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.ExtensionMethods.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.ExtensionMethods.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.SharePoint.DataCapture.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.SharePoint.DataCapture.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.SharePoint.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.SharePoint.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.SharePoint.Redaction.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.SharePoint.Redaction.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.SharePoint.Redaction.Utilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.SharePoint.Redaction.Utilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\RemoveExtractSPColumns.exe" /mapout:"$(BinariesFolder)\Map\mapRemoveExtractSPColumns.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)" $(PDCommonDir)\ObfuscateConfig.xml
	
CreateSharePointPackages: BuildExtractSharePoint CopyOriginalFilesBeforeObfuscating ObfuscateFiles
	@ECHO Creating SharePoint packages...
	msbuild /t:Package $(SharePointRootDir)\DataCapture\Core\Code\Extract.SharePoint.DataCapture.csproj /p:Configuration=$(BuildConfig)
	msbuild /t:Package $(SharePointRootDir)\Redaction\Core\Code\Extract.SharePoint.Redaction.csproj /p:Configuration=$(BuildConfig)
	msbuild /t:Package $(SharePointRootDir)\Core\Code\Extract.SharePoint.csproj /p:Configuration=$(BuildConfig)

CopyFilesForSPInstallFolder: CreateSharePointPackages
    @ECHO Copying the Sharepoint files to installation directory...
	@IF NOT EXIST "$(SPInstallationDirectory)" @MKDIR "$(SPInstallationDirectory)"
	@IF NOT EXIST "$(SPInstallationDirectory)\IDShieldForSPClient" @MKDIR "$(SPInstallationDirectory)\IDShieldForSPClient"
	@IF NOT EXIST "$(InternalUseBuildFilesArchive)" @MKDIR "$(InternalUseBuildFilesArchive)"
	@IF NOT EXIST "$(InternalUseBuildFilesArchive)\OriginalFiles" @MKDIR "$(InternalUseBuildFilesArchive)\OriginalFiles"
	
	@COPY /v "$(BinariesFolder)\Extract.SharePoint.wsp" "$(SPInstallationDirectory)"
	@COPY /v "$(BinariesFolder)\Extract.SharePoint.DataCapture.wsp" "$(SPInstallationDirectory)"
	@COPY /v "$(BinariesFolder)\Extract.SharePoint.Redaction.wsp" "$(SPInstallationDirectory)"
	@COPY /v "$(SharePointRootDir)\Installation\PowershellScripts\*.ps1" "$(SPInstallationDirectory)"
	
	@COPY /v "$(BinariesFolder)\Extract.ExtensionMethods.dll" "$(SPInstallationDirectory)\IDShieldForSPClient"
	@COPY /v "$(BinariesFolder)\Extract.SharePoint.Redaction.dll" "$(SPInstallationDirectory)\IDShieldForSPClient"
	@COPY /v "$(BinariesFolder)\IDShieldForSPClient.exe" "$(SPInstallationDirectory)\IDShieldForSPClient"
	@COPY /v "$(RCNETDir)\APIs\SharePoint\2010\bin\Microsoft.SharePoint.Client.dll" "$(SPInstallationDirectory)\IDShieldForSPClient"
	@COPY /v "$(RCNETDir)\APIs\SharePoint\2010\bin\Microsoft.SharePoint.Client.Runtime.dll" "$(SPInstallationDirectory)\IDShieldForSPClient"
	@COPY /v "$(BinariesFolder)\Extract.SharePoint.Redaction.Utilities.dll" "$(SPInstallationDirectory)\IDShieldForSPClient"
	@COPY /v "$(BinariesFolder)\RemoveExtractSPColumns.exe" "$(SPInstallationDirectory)\IDShieldForSPClient"
	@COPY /v "$(SharePointRootDir)\Installation\IDShieldForSPClient\*.bat" "$(SPInstallationDirectory)\IDShieldForSPClient"
		
# Copy pdb and map files to archive
	@COPY  "$(BinariesFolder)\*.pdb" "$(InternalUseBuildFilesArchive)" 
	@COPY  "$(BinariesFolder)\OriginalFiles\*.*" "$(InternalUseBuildFilesArchive)\OriginalFiles"
	@COPY  "$(BinariesFolder)\Map\*.xml" "$(InternalUseBuildFilesArchive)" 

GetAllFiles: GetPDCommonFiles GetExtractSharePointFiles GetRCNetAPISharePoint GetRCNetExtensionMethods GetRCNetCore

BuildAfterAF:GetExtractSharePointFiles DoEverythingNoGet

DoEverythingNoGet: SetupBuildEnv CopyFilesForSPInstallFolder 
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Flex IDShield for Sharepoint Build process completed.
    @ECHO.
  
DoEverything: SetupBuildEnv GetAllFiles DoEverythingNoGet
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Flex IDShield for Sharepoint Build process completed.
    @ECHO.
