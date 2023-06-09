Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $ServerName,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BaseDestPath,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BaseTargetPath,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $InstallFolder
)

$LinkPath=$BaseDestPath + $InstallFolder
$TargetPath=$BaseTargetPath + $InstallFolder
	
# command to run on $ServerName
$linkCmd="cmd /c mklink /J ""$LinkPath"" ""$TargetPath"""
$script = [scriptblock]::Create($linkCmd)
Invoke-Command -ComputerName $ServerName -ScriptBlock $script


