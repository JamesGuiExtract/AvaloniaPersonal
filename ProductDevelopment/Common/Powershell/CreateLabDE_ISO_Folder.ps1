# Script to setup a folder with the structure of the ISO image for LabDE
# This will create symbolic links from the release folder so that can be used to create the ISO 
# NOTE: If the release folders are moved this may need to be ran with the new dest

# $BaseDestPath 'd:\Internal\ProductReleases\FlexIndex\Internal\ReleaseISO\LabDE\' 
# $SharedInstallsPath 'd:\Internal\ProductReleases\SharedInstalls\' 
# $SetupFilesPath 'd:\Internal\ProductReleases\FlexIndex\Internal\BleedingEdge\FlexIndex Ver. 10.4.0.93\LabDE\SetupFiles\'
#'d:\Internal\ProductReleases\FlexIndex\Internal\ReleaseISO\LabDE\'  'd:\Internal\ProductReleases\SharedInstalls\'  'd:\Internal\ProductReleases\FlexIndex\Internal\BleedingEdge\FlexIndex Ver. 10.4.0.93\LabDE\SetupFiles\'

Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $FlexIndexVersion
)


function MakeFileLink([String] $ServerName, [String] $LinkPath, [String] $TargetPath)
{
	# command to run on $ServerName
	$linkCmd="cmd /c mklink /H '$LinkPath' '$TargetPath'"
	$script = [scriptblock]::Create($linkCmd)
	Invoke-Command -ComputerName $ServerName -ScriptBlock $script	
}

$BaseDestRootPath = 'D:\Internal\ISOTree\' + $FlexIndexVersion + '\LabDE\'
$SharedInstallsPath = 'D:\Internal\ProductReleases\SharedInstalls\'
$SetupFilesPath = 'D:\Internal\ProductReleases\FlexIndex\Internal\BleedingEdge\' + $FlexIndexVersion + '\LabDE\SetupFiles\'

# Create the directory if it doesn't exist
New-Item -ItemType Directory -Force -Path ($BaseDestRootPath -replace 'D:', '\\Engsvr')


$BaseInvokePath = Split-Path $MyInvocation.MyCommand.Path

# LabDEExtras
$BaseDestPath =$BaseDestRootPath + 'LabDEExtras\'

New-Item -ItemType Directory -Force -Path ($BaseDestPath -replace 'D:', '\\Engsvr')

$CommonDestTarget = " -BaseDestPath '$BaseDestPath' -BaseTargetPath '$SharedInstallsPath'"
$MakeLinksToCommonArgs = "\MakeLinksToCommonInstalls.ps1 " + $CommonDestTarget
$MakeLinksToCommonPath = $BaseInvokePath + $MakeLinksToCommonArgs

Invoke-Expression $MakeLinksToCommonPath 

$MakeSymLinkCommon = $BaseInvokePath + "\MakeSymLink.ps1 -ServerName Engsvr "
$CorePointLink = $MakeSymLinkCommon + $CommonDestTarget + " -InstallFolder 'Corepoint Integration Engine'"

Invoke-Expression $CorePointLink

$CommonSetup = "$MakeSymLinkCommon -BaseDestPath '$BaseDestPath' -BaseTargetPath '$SetupFilesPath'"
#Extract Systems LM
Invoke-Expression "$CommonSetup -InstallFolder 'Extract Systems LM'"
Invoke-Expression "$CommonSetup -InstallFolder 'LabDEInstall'"

$FileDest = $BaseDestPath + 'autorun.inf'
$FileTarget = $SetupFilesPath + 'autorun.inf'

MakeFileLink 'Engsvr' $FileDest $FileTarget

$FileDest = $BaseDestPath + 'LabDEInstall.chm'
$FileTarget = $SetupFilesPath + 'LabDE\LabDEInstall.chm'

MakeFileLink 'Engsvr' $FileDest $FileTarget

$FileDest = $BaseDestPath + 'Readme.txt'
$FileTarget = $SetupFilesPath + 'Readme.txt'

MakeFileLink 'Engsvr' $FileDest $FileTarget

#LabDE

$BaseDestPath =$BaseDestRootPath + 'LabDE\'
New-Item -ItemType Directory -Path ($BaseDestPath -replace 'D:', '\\Engsvr')

$CommonDestTarget = " -BaseDestPath '$BaseDestPath' -BaseTargetPath '$SharedInstallsPath'"
$CommonSetup = "$MakeSymLinkCommon -BaseDestPath '$BaseDestPath' -BaseTargetPath '$SetupFilesPath'"

Invoke-Expression "$CommonSetup -InstallFolder 'LabDE'"
Invoke-Expression "$CommonSetup -InstallFolder 'SilentInstalls'"
Invoke-Expression "$CommonSetup -InstallFolder 'Demo_LabDE'"

$FileDest = $BaseDestPath + 'LabDE.ico'
$FileTarget = $SetupFilesPath + 'LabDEInstall\LabDE.ico'

MakeFileLink 'Engsvr' $FileDest $FileTarget

$FileDest = $BaseDestPath + 'LabDEInstall.chm'
$FileTarget = $SetupFilesPath + 'LabDE\LabDEInstall.chm'

MakeFileLink 'Engsvr' $FileDest $FileTarget

$FileDest = $BaseDestPath + 'autorun.inf'
$FileDest = $FileDest -replace 'd:', '\\Engsvr'
#Create auotrun.inf file
$text = "[autorun]`r`nopen=LabDE\setup.exe`r`nicon=LabDE.ico"
$text | Set-Content $FileDest
