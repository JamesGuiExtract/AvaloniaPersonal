# Script to setup a folder with the structure of the ISO image for LabDE
# This will create symbolic links from the release folder so that can be used to create the ISO 
# NOTE: If the release folders are moved this may need to be ran with the new dest

# $BaseDestPath 'd:\Internal\ProductReleases\FlexIndex\Internal\ReleaseISO\LabDE\' 
# $SharedInstallsPath 'd:\Internal\ProductReleases\SharedInstalls\' 
# $SetupFilesPath 'd:\Internal\ProductReleases\FlexIndex\Internal\BleedingEdge\FlexIndex Ver. 10.4.0.93\LabDE\SetupFiles\'
# 'd:\Internal\ProductReleases\FlexIndex\Internal\ReleaseISO\IDShield\'  'd:\Internal\ProductReleases\SharedInstalls\'  'd:\Internal\ProductReleases\FlexIndex\Internal\BleedingEdge\FlexIndex Ver. 10.4.0.93\IDShield\SetupFiles\'
Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BaseDestRootPath,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $SharedInstallsPath,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $SetupFilesPath
)


function MakeFileLink([String] $ServerName, [String] $LinkPath, [String] $TargetPath)
{
	# command to run on $ServerName
	$linkCmd="cmd /c mklink /H '$LinkPath' '$TargetPath'"
	$script = [scriptblock]::Create($linkCmd)
	Invoke-Command -ComputerName $ServerName -ScriptBlock $script	
}


$BaseInvokePath = Split-Path $MyInvocation.MyCommand.Path

# FlexIndexExtras
$BaseDestPath =$BaseDestRootPath + 'IDShieldExtras\'

$CommonDestTarget = " -BaseDestPath $BaseDestPath -BaseTargetPath $SharedInstallsPath"
$MakeLinksToCommonArgs = "\MakeLinksToCommonInstalls.ps1 " + $CommonDestTarget
$MakeLinksToCommonPath = $BaseInvokePath + $MakeLinksToCommonArgs
Invoke-Expression $MakeLinksToCommonPath 

$MakeSymLinkCommon = $BaseInvokePath + "\MakeSymLink.ps1 -ServerName Engsvr "

$CommonSetup = "$MakeSymLinkCommon -BaseDestPath $BaseDestPath -BaseTargetPath '$SetupFilesPath'"
#Extract Systems LM
Invoke-Expression "$CommonSetup -InstallFolder 'Extract Systems LM'"
Invoke-Expression "$CommonSetup -InstallFolder 'IDShieldInstall'"

$FileDest = $BaseDestPath + 'autorun.inf'
$FileTarget = $SetupFilesPath + 'autorun.inf'

MakeFileLink 'Engsvr' $FileDest $FileTarget

$FileDest = $BaseDestPath + 'IDShieldInstall.chm'
$FileTarget = $SetupFilesPath + 'IDShield\IDShieldInstall.chm'

MakeFileLink 'Engsvr' $FileDest $FileTarget

$FileDest = $BaseDestPath + 'Readme.txt'
$FileTarget = $SetupFilesPath + 'Readme.txt'

MakeFileLink 'Engsvr' $FileDest $FileTarget

#FlexIndex

$BaseDestPath =$BaseDestRootPath + 'IDShield\'
$CommonDestTarget = " -BaseDestPath $BaseDestPath -BaseTargetPath $SharedInstallsPath"
$CommonSetup = "$MakeSymLinkCommon -BaseDestPath $BaseDestPath -BaseTargetPath '$SetupFilesPath'"

Invoke-Expression "$CommonSetup -InstallFolder 'IDShield'"
Invoke-Expression "$CommonSetup -InstallFolder 'SilentInstalls'"
Invoke-Expression "$CommonSetup -InstallFolder 'Demo_IDShield'"

$FileDest = $BaseDestPath + 'IDShield.ico'
$FileTarget = $SetupFilesPath + 'IDShieldInstall\IDShield.ico'

MakeFileLink 'Engsvr' $FileDest $FileTarget

$FileDest = $BaseDestPath + 'IDShieldInstall.chm'
$FileTarget = $SetupFilesPath + 'IDShield\IDShieldInstall.chm'

MakeFileLink 'Engsvr' $FileDest $FileTarget
$FileDest = $BaseDestPath + 'autorun.inf'
$FileDest = $FileDest -replace 'd:', '\\Engsvr'
#Create auotrun.inf file
$text = "[autorun]`r`nopen=IDShield\setup.exe`r`nicon=IDShield.ico"
$text | Set-Content $FileDest
