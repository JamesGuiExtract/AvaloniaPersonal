!include LatestComponentVersions.mak

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
	$(GitPath) -a $(GitTagFlexIndexVersion) -m "$(GitTagFlexIndexVersion)"
	cd %BUILD_VSS_ROOT%\Engineering\RC.Net\APIs
	$(GitPath) -a $(GitTagFlexIndexVersion) -m "$(GitTagFlexIndexVersion)"
	cd %BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs
	$(GitPath) -a $(GitTagFlexIndexVersion) -m "$(GitTagFlexIndexVersion)"
	cd %BUILD_VSS_ROOT%\Engineering\Rules
	$(GitPath) -a $(GitTagFlexIndexVersion) -m "$(GitTagFlexIndexVersion)"
	$(GitPath) -a $(GitTagFKBVersion) -m "$(GitTagFKBVersion)"
