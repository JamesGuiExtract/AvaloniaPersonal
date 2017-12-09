Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $FlexIndexVersion
)


$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "New-ISOFile.ps1"
. $HelperFile

$sharedInstallPath = '\\extract.local\Eng\Builds\SharedInstalls\'

$sharedInstallsAll = 	"DotNet 3.5 Framework",
						"DotNet 4.6 Framework",
						"Powershell",
						"SQLServerExpress2014",
						"SQLServerExpress2014Mgr"

$FlexVersionNum = $FlexIndexVersion -replace 'FlexIndex Ver. ', '_'
$FlexVersionString = $FlexVersionNum -replace '\.', '_'

$BaseDestRootPath = Join-Path '\\extract.local\Eng\Builds\FlexIndex\Internal\BleedingEdge\' $FlexIndexVersion 

$SetupPath = Join-Path $BaseDestRootPath  'FLEXIndex\SetupFiles'

#Create autorun file
$FileDest = Join-Path $ScriptPath 'autorun.inf'
#Create auotrun.inf file
$text = "[autorun]`r`nopen=FlexIndex\setup.exe`r`nicon=FlexIndex.ico"
$text | Set-Content $FileDest
 
#Build list of files and folders to include on the DVD for FlexIndex
$IncludedFiles = Get-ChildItem $SetupPath | Where-Object {($_.Name -match 'FlexIndex$') -or ($_.Name -match 'SilentInstalls')}
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\FlexIndex') | Where-Object {$_.Name -eq 'FLEXInstall.chm'} 
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\FlexIndexInstall') | Where-Object {$_.Name -eq 'FlexIndex.ico'} 
$IncludedFiles += Get-ChildItem (Join-Path $BaseDestRootPath '\Other') | Where-Object {($_.Name -match 'WebAPI$')}
$IncludedFiles += Get-ChildItem $FileDest

$ISOName = $BaseDestRootPath + '\FlexIndex\ISO\Flex' + $FlexVersionString + '.iso'
$ISOTitle = 'Flex' + $FlexVersionString

New-Item ($BaseDestRootPath + '\FlexIndex\ISO') -Type Directory
$IncludedFiles | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

$IncludedFiles = Get-ChildItem $SetupPath | Where-Object {($_.Name -eq 'FlexIndexInstall') -or ($_.Name -eq 'Extract Systems LM')}
$IncludedFiles += Get-ChildItem $SetupPath | Where-Object {($_.Name -eq 'autorun.inf') -or ($_.Name -eq 'Readme.txt')}
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\FlexIndex') | Where-Object {$_.Name -eq 'FLEXInstall.chm'} 
 
ForEach ($shared in $sharedInstallsAll)
{
	$IncludedFiles += Get-ChildItem $sharedInstallPath 	| Where-Object {$_.Name -eq $shared}  
}

$ISOName = $BaseDestRootPath + '\FlexIndex\ISO\FlexExtras' + $FlexVersionString + '.iso'
$ISOTitle = 'FlexExtras' + $FlexVersionString
$IncludedFiles | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle
