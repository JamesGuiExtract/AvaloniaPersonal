Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $FlexIndexVersion
)


$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "New-ISOFile.ps1"
. $HelperFile

$sharedInstallPath = "\\extract.local\Eng\Builds\SharedInstalls\"

$sharedInstallsAll = 	"DotNet 3.5 Framework",
						"DotNet 4.6 Framework",
						"Powershell",
						"SQLServerExpress2014",
						"SQLServerExpress2014Mgr"

$FlexVersion = $FlexIndexVersion -replace 'FlexIndex Ver. ', ''
$FlexVersionNum = $FlexIndexVersion -replace 'FlexIndex Ver. ', '_'
$FlexVersionString = $FlexVersionNum -replace '\.', '_'

$BaseDestRootPath = Join-Path '\\extract.local\Eng\Builds\FlexIndex\Internal\BleedingEdge\' $FlexIndexVersion 
$isoPath = Join-Path '\\extract.local\Eng\Builds\FlexIndex\External\Current' $FlexVersion 

$SetupPath = Join-Path $BaseDestRootPath  'FLEXIndex\SetupFiles'

$IncludedFiles = Get-ChildItem $SetupPath | Where-Object {($_.Name -eq 'FlexIndexInstall') -or ($_.Name -eq 'Extract Systems LM')}
$IncludedFiles += Get-ChildItem $SetupPath | Where-Object {($_.Name -eq 'autorun.inf') -or ($_.Name -eq 'Readme.txt')}
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\FlexIndex') | Where-Object {$_.Name -eq 'FLEXInstall.chm'} 
 
ForEach ($shared in $sharedInstallsAll)
{
	$IncludedFiles += Get-ChildItem $sharedInstallPath 	| Where-Object {$_.Name -eq $shared}  
}

$ISOName = $isoPath + '\FlexIndex\FlexExtras' + $FlexVersionString + '.iso'
$ISOTitle = 'FlexExtras' + $FlexVersionString
$IncludedFiles | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle -Force  


$SetupPath = Join-Path $BaseDestRootPath  'IDShield\SetupFiles'

$IncludedFiles = Get-ChildItem $SetupPath | Where-Object {($_.Name -eq 'IDShieldInstall') -or ($_.Name -eq 'Extract Systems LM')}
$IncludedFiles += Get-ChildItem $SetupPath | Where-Object {($_.Name -eq 'autorun.inf') -or ($_.Name -eq 'Readme.txt')}
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\IDShield') | Where-Object {$_.Name -eq 'IDShieldInstall.chm'} 
 
ForEach ($shared in $sharedInstallsAll)
{
	$IncludedFiles += Get-ChildItem $sharedInstallPath 	| Where-Object {$_.Name -eq $shared}  
}

$ISOName = $isoPath + '\IDShield\IDShieldExtras' + $FlexVersionString + '.iso'
$ISOTitle = 'IDShieldExtras' + $FlexVersionString
$IncludedFiles | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle -Force  

$SetupPath = Join-Path $BaseDestRootPath  'LabDE\SetupFiles'


$IncludedFiles = Get-ChildItem $SetupPath | Where-Object {($_.Name -eq 'LabDEInstall') -or ($_.Name -eq 'Extract Systems LM')}
$IncludedFiles += Get-ChildItem $SetupPath | Where-Object {($_.Name -eq 'autorun.inf') -or ($_.Name -eq 'Readme.txt')}
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\LabDE') | Where-Object {$_.Name -eq 'LabDEInstall.chm'} 
 
ForEach ($shared in $sharedInstallsAll)
{
	$IncludedFiles += Get-ChildItem $sharedInstallPath 	| Where-Object {$_.Name -eq $shared} 
}
 $IncludedFiles += Get-ChildItem $sharedInstallPath 	| Where-Object {$_.Name -eq 'Corepoint Integration Engine'}  


$ISOName = $isoPath + '\LabDE\LabDEExtras' + $FlexVersionString + '.iso'
$ISOTitle = 'LabDEExtras' + $FlexVersionString
$IncludedFiles
$IncludedFiles | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle -Force 

