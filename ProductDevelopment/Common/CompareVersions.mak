#############################################################################
# This make file is called to stop the build process if the passed in
# in ProductVersion does not match the FlexIndexVersion defined in the 
# LatestComponentsVersions.mak file that is in the tree that has been 
# retrieved from vault.

BuildRootDirectory=$(BUILD_DRIVE)$(BUILD_DIRECTORY)\$(ProductRootDirName)
EngineeringRootDirectory=$(BuildRootDirectory)\Engineering
PDRootDir=$(EngineeringRootDirectory)\ProductDevelopment

!include $(PDRootDir)\Common\LatestComponentVersions.mak

!IF "$(ProductVersion)" != "$(FlexIndexVersion)"
!ERROR FLEX Index version being built is $(ProductVersion) which does not match LatestComponentVersions.mak file $(FlexIndexVersion).
!ENDIF

DoCheck:
