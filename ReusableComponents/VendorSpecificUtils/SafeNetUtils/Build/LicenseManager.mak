#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

!include ..\..\..\..\ProductDevelopment\Common\Common.mak

#############################################################################
# E N S U R E   P R E - C O N D I T I O N S   A R E   M E T
#
# The user must tell which configuration to build (e.g. "Win32 Release")
# Otherwise, we cannot continue.
#
!IF "$(BuildConfig)" == ""
!ERROR Build variable 'BuildConfig' must be defined (e.g. "Win32 Release")
!ENDIF

#############################################################################
# M A K E F I L E   V A R I A B L E S
#
PDRootDir=$(EngineeringRootDirectory)\ProductDevelopment

LMInstallFilesRootDir=P:\LicenseManager\Files

ExtractLMInstallRootDir=$(PDRootDir)\Installation\Extract Systems LM
ExtractLMInstallMediaDir=$(ExtractLMInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

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

#############################################################################
# B U I L D    T A R G E T S
#
# This file requires the appropriate project has been built before calling
#
CopyFilesToExtractLMInstall:
	@Echo Copying Extract LM files to Installation directory...
	@IF NOT EXIST "$(LMInstallFilesRootDir)\FieldActivation" MKDIR "$(LMInstallFilesRootDir)\FieldActivation"
	@IF NOT EXIST "$(LMInstallFilesRootDir)\NonSelfRegCC" MKDIR "$(LMInstallFilesRootDir)\NonSelfRegCC"
	@DeleteFiles "$(LMInstallFilesRootDir)\FieldActivation\*.*"
    @DeleteFiles "$(LMInstallFilesRootDir)\NonSelfRegCC\*.*"
    @XCOPY "$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\FieldActivationUtility\*.*" "$(LMInstallFilesRootDir)\FieldActivation" /v /y
    $(VerifyDir) "$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\FieldActivationUtility" "$(LMInstallFilesRootDir)\FieldActivation"
    @COPY /v  "$(BinariesFolder)\USBLicenseKeyManager.exe" "$(LMInstallFilesRootDir)\NonSelfRegCC"
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\MergeModules\*.*" "$(MERGE_MODULE_DIR)" /v /y /s

BuildExtractLMInstall:CopyFilesToExtractLMInstall
    @ECHO Building Extract Systems LM installation...
	$(SetProductVerScript) "$(ExtractLMInstallRootDir)\Extract Systems LM.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(ExtractLMInstallRootDir)\Extract Systems LM.ism"
	
CreateFlexLMInstall: BuildExtractLMInstall
	@ECHO Copying LM Install to Flex Index Release Directory...
    @IF NOT EXIST "$(FLEXIndexExtractLMDir)" MKDIR "$(FLEXIndexExtractLMDir)"
    @XCOPY "$(ExtractLMInstallMediaDir)\*.*" "$(FLEXIndexExtractLMDir)" /v /s /e /y
    $(VerifyDir) "$(ExtractLMInstallMediaDir)" "$(FLEXIndexExtractLMDir)"
	@DeleteFiles "$(FLEXIndexExtractLMDir)\vssver.scc"
    @IF NOT EXIST "$(IDShieldExtractLMDir)" MKDIR "$(IDShieldExtractLMDir)"
    @XCOPY "$(ExtractLMInstallMediaDir)\*.*" "$(IDShieldExtractLMDir)" /v /s /e /y
    $(VerifyDir) "$(ExtractLMInstallMediaDir)" "$(IDShieldExtractLMDir)"
	@DeleteFiles "$(IDShieldExtractLMDir)\vssver.scc"
    @IF NOT EXIST "$(LabDEExtractLMDir)" MKDIR "$(LabDEExtractLMDir)"
    @XCOPY "$(ExtractLMInstallMediaDir)\*.*" "$(LabDEExtractLMDir)" /v /s /e /y
    $(VerifyDir) "$(ExtractLMInstallMediaDir)" "$(LabDEExtractLMDir)"
	@DeleteFiles "$(LabDEExtractLMDir)\vssver.scc"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Flex Index License Manager Build process completed.
    @ECHO.


