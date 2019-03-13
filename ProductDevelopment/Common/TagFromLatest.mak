!include LatestComponentVersions.mak
!include ..\..\Rules\Build_FKB\FKBVersion.mak

#Git tags converted from label
GitTagFlexIndexVersion=$(FlexIndexVersion:v.=)
GitTagFlexIndexVersion=$(GitTagFlexIndexVersion:/=)
GitTagFlexIndexVersion=$(GitTagFlexIndexVersion:Ver. =)
GitTagFlexIndexVersion=$(GitTagFlexIndexVersion: =/)
GitTagFlexIndexVersion=$(GitTagFlexIndexVersion:.=/)
GitTagFlexIndexVersion=$(GitTagFlexIndexVersion:-=/)

GitTagFKBVersion=$(FKBVersion:v.=)
GitTagFKBVersion=$(GitTagFKBVersion:/=)
GitTagFKBVersion=$(GitTagFKBVersion:Ver. =)
GitTagFKBVersion=$(GitTagFKBVersion: =/)
GitTagFKBVersion=$(GitTagFKBVersion:.=/)
GitTagFKBVersion=$(GitTagFKBVersion:-=/)

GitPath="C:\Program Files\Git\bin\git.exe"

TagRepos:
	cd %BUILD_VSS_ROOT%\Engineering
	$(GitPath) tag $(GitTagFlexIndexVersion) -m "$(FlexIndexVersion)"
	cd %BUILD_VSS_ROOT%\Engineering\RC.Net\APIs
	$(GitPath) tag $(GitTagFlexIndexVersion) -m "$(FlexIndexVersion)"
	cd %BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs
	$(GitPath) tag $(GitTagFlexIndexVersion) -m "$(FlexIndexVersion)"
	cd %BUILD_VSS_ROOT%\Engineering\Rules
	$(GitPath) tag $(GitTagFlexIndexVersion) -m "$(FlexIndexVersion)"
	IF "$(FKBBuildNeeded)"=="True" (
		$(GitPath) tag $(GitTagFKBVersion) -m "$(FKBVersion)"
	)

