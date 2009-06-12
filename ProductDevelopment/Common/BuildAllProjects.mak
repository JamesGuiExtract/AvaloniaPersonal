#############################################################################
# M A K E F I L E   V A R I A B L E S
#

EngineeringRootDirectory=$(LOCAL_VSS_ROOT)\Engineering
PDRootDir=$(EngineeringRootDirectory)\ProductDevelopment
RCRootDir=$(EngineeringRootDirectory)\ReusableComponents
PDCommonDir=$(PDRootDir)\Common
PDUtilsDir=$(PDRootDir)\Utils

# determine the name of the release output directory based upon the build
# configuration that is being built
!IF "$(BuildConfig)" == "Release"
BuildOutputDir=Release
!ELSEIF "$(BuildConfig)" == "Debug"
BuildOutputDir=Debug
!ELSE
!ERROR Internal error - invalid value for BuildConfig variable!
!ENDIF

# Do a clean build. If this variable is defined, before building the project, it will clean the whole project first
CleanBuild=/CLEAN

BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)

Get=ss get
# always skip any writable copies
GetOptions=-R -GWS -I-

# always force overwrite any writable copies
# GetOptions=-R -GWR -I-

# Displays a message asking the user to choose between replacing, skipping, or merging write-only files
# GetOptions=-R -GWA -I-

#############################################################################
# B U I L D    T A R G E T S
#

DisplayTimeStamp:
	@ECHO.
	@DATE /T
	@TIME /T
	@ECHO.

#############################
# Get individual project

GetRCFiles:
	@ECHO Getting ReusableComponents files....
	@IF NOT EXIST "$(RCRootDir)" @MKDIR "$(RCRootDir)"
	@CD "$(RCRootDir)"
	$(Get) $$/Engineering/ReusableComponents $(GetOptions)

GetPDUtilsFiles:
	@ECHO Getting PDUtils files...
	@IF NOT EXIST "$(PDUtilsDir)\EncryptTextFile" @MKDIR "$(PDUtilsDir)\EncryptTextFile"
	@CD "$(PDUtilsDir)\EncryptTextFile"	
	$(Get) $$/Engineering/ProductDevelopment/Utils/EncryptTextFile $(GetOptions)
	@IF NOT EXIST "$(PDUtilsDir)\SpatialStringViewer" @MKDIR "$(PDUtilsDir)\SpatialStringViewer"
	@CD "$(PDUtilsDir)\SpatialStringViewer"	
	$(Get) $$/Engineering/ProductDevelopment/Utils/SpatialStringViewer $(GetOptions)
	@IF NOT EXIST "$(PDUtilsDir)\UserLicense" @MKDIR "$(PDUtilsDir)\UserLicense"
	@CD "$(PDUtilsDir)\UserLicense"	
	$(Get) $$/Engineering/ProductDevelopment/Utils/UserLicense $(GetOptions)

GetIcoMapForArcGISFiles:
	@ECHO Getting IcoMap for ArcGIS files....
	@IF NOT EXIST "$(PDRootDir)\IcoMapESRI" @MKDIR "$(PDRootDir)\IcoMapESRI"
	@CD "$(PDRootDir)\IcoMapESRI"	
	$(Get) $$/Engineering/ProductDevelopment/IcoMapESRI $(GetOptions)
	@IF NOT EXIST "$(PDRootDir)\IcoMapCore" @MKDIR "$(PDRootDir)\IcoMapCore"
	@CD "$(PDRootDir)\IcoMapCore"	
	$(Get) $$/Engineering/ProductDevelopment/IcoMapCore $(GetOptions)
	@IF NOT EXIST "$(PDRootDir)\PlatformSpecificUtils" @MKDIR "$(PDRootDir)\PlatformSpecificUtils"
	@CD "$(PDRootDir)\PlatformSpecificUtils"	
	$(Get) $$/Engineering/ProductDevelopment/PlatformSpecificUtils $(GetOptions)

GetSwipeItForArcGISFiles:
	@ECHO Getting SwipeIt for ArcGIS files....
	@IF NOT EXIST "$(PDRootDir)\SwipeIt" @MKDIR "$(PDRootDir)\SwipeIt"
	@CD "$(PDRootDir)\SwipeIt"	
	$(Get) $$/Engineering/ProductDevelopment/SwipeIt $(GetOptions)
	@IF NOT EXIST "$(PDRootDir)\PlatformSpecificUtils" @MKDIR "$(PDRootDir)\PlatformSpecificUtils"
	@CD "$(PDRootDir)\PlatformSpecificUtils"	
	$(Get) $$/Engineering/ProductDevelopment/PlatformSpecificUtils $(GetOptions)

GetAttributeFinderFiles:
	@ECHO Getting Attribute Finder files....
	@IF NOT EXIST "$(PDRootDir)\AttributeFinder" @MKDIR "$(PDRootDir)\AttributeFinder"
	@CD "$(PDRootDir)\AttributeFinder"	
	$(Get) $$/Engineering/ProductDevelopment/AttributeFinder $(GetOptions)


#############################
# Build individual project

BuildInputFunnel: DisplayTimeStamp GetRCFiles
	@ECHO Building InputFunnel...
	@CD "$(RCRootDir)\InputFunnel\Packages\LandRecords\Code"
!IFDEF CleanBuild
	@devenv LandRecords.sln /REBUILD $(BuildConfig) /USEENV
!ENDIF
	@devenv LandRecords.sln /BUILD $(BuildConfig) /USEENV
	@ECHO.
	@DATE /T
	@TIME /T
	@ECHO.

BuildIcoMapForArcGIS: DisplayTimeStamp GetRCFiles GetInputFunnelFiles GetIcoMapForArcGISFiles
	@ECHO Building IcoMap for ArcGIS...
	@CD "$(PDRootDir)\IcoMapESRI\ArcGISIcoMap\Code"
!IFDEF CleanBuild
	@devenv ArcGISIcoMap.sln /REBUILD $(BuildConfig) /USEENV
!ENDIF
	@devenv ArcGISIcoMap.sln /BUILD $(BuildConfig) /USEENV
	@ECHO.
	@DATE /T
	@TIME /T
	@ECHO.

BuildSwipeItForArcGIS: DisplayTimeStamp GetRCFiles GetSwipeItForArcGISFiles
	@ECHO Building SwipeIt for ArcGIS...
	@CD "$(PDRootDir)\SwipeIt\ArcGIS\Code"
!IFDEF CleanBuild
	@devenv SwipeItForArcGIS.sln /REBUILD $(BuildConfig) /USEENV
!ENDIF
	@devenv SwipeItForArcGIS.sln /BUILD $(BuildConfig) /USEENV
	@ECHO.
	@DATE /T
	@TIME /T
	@ECHO.

BuildAttributeFinder: DisplayTimeStamp GetRCFiles GetPDUtilsFiles GetAttributeFinderFiles
	@ECHO Building AttributeFinder...
	@CD "$(PDRootDir)\AttributeFinder\AFCore\AFCoreTest\Code"
!IFDEF CleanBuild
	@devenv AFCoreTest.sln /REBUILD $(BuildConfig) /USEENV
!ENDIF
	@devenv AFCoreTest.sln /BUILD $(BuildConfig) /USEENV
	@ECHO.
	@DATE /T
	@TIME /T
	@ECHO.


#############################
# Build all projects

BuildAllProjects: DisplayTimeStamp GetRCFiles GetPDUtilsFiles GetIcoMapForArcGISFiles GetSwipeItForArcGISFiles GetAttributeFinderFiles
!IFDEF CleanBuild
	@ECHO Cleaning all projects...
	@CD "$(RCRootDir)\InputFunnel\Packages\LandRecords\Code"
	@devenv LandRecords.sln /CLEAN $(BuildConfig) /USEENV
	@CD "$(PDRootDir)\IcoMapESRI\ArcGISIcoMap\Code"
	@devenv ArcGISIcoMap.sln /CLEAN $(BuildConfig) /USEENV
	@CD "$(PDRootDir)\SwipeIt\ArcGIS\Code"
	@devenv SwipeItForArcGIS.sln /CLEAN  $(BuildConfig) /USEENV
	@CD "$(PDRootDir)\AttributeFinder\AFCore\AFCoreTest\Code"
	@devenv AFCoreTest.sln /CLEAN  $(BuildConfig) /USEENV
!ENDIF
	@ECHO Building InputFunnel...
	@CD "$(RCRootDir)\InputFunnel\Packages\LandRecords\Code"
	@devenv LandRecords.sln /BUILD $(BuildConfig) /USEENV

	@ECHO Building IcoMap for ArcGIS...
	@CD "$(PDRootDir)\IcoMapESRI\ArcGISIcoMap\Code"
	@devenv ArcGISIcoMap.sln /BUILD $(BuildConfig) /USEENV

	@ECHO Building SwipeIt for ArcGIS...
	@CD "$(PDRootDir)\SwipeIt\ArcGIS\Code"
	@devenv SwipeItForArcGIS.sln /BUILD $(BuildConfig) /USEENV

	@ECHO Building AttributeFinder...
	@CD "$(PDRootDir)\AttributeFinder\AFCore\AFCoreTest\Code"
	@devenv AFCoreTest.sln /BUILD $(BuildConfig) /USEENV

	@ECHO.
	@DATE /T
	@TIME /T
	@ECHO.