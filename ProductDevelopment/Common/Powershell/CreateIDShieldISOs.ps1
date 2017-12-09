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
$text = "[autorun]`r`nopen=IDShield\setup.exe`r`nicon=IDShield.ico"
$text | Set-Content $FileDest

$SetupPath = Join-Path $BaseDestRootPath  'IDShield\SetupFiles'

#Build list of files and folders to include on the DVD for IDShield
$IncludedFiles = Get-ChildItem $SetupPath | Where-Object {($_.Name -match 'IDShield$') -or ($_.Name -match 'SilentInstalls')}
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\IDShield') | Where-Object {$_.Name -eq 'IDShieldInstall.chm'} 
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\IDShieldInstall') | Where-Object {$_.Name -eq 'IDShield.ico'} 
$IncludedFiles += Get-ChildItem (Join-Path $BaseDestRootPath '\Other') | Where-Object {($_.Name -match 'WebAPI$')}
$IncludedFiles += Get-ChildItem $FileDest

$ISOName = $BaseDestRootPath + '\IDShield\ISO\IDShield' + $FlexVersionString + '.iso'
$ISOTitle = 'IDShield' + $FlexVersionString
New-Item ($BaseDestRootPath + '\IDShield\ISO') -Type Directory
$IncludedFiles | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

$IncludedFiles = Get-ChildItem $SetupPath | Where-Object {($_.Name -eq 'IDShieldInstall') -or ($_.Name -eq 'Extract Systems LM')}
$IncludedFiles += Get-ChildItem $SetupPath | Where-Object {($_.Name -eq 'autorun.inf') -or ($_.Name -eq 'Readme.txt')}
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\IDShield') | Where-Object {$_.Name -eq 'IDShieldInstall.chm'} 
 
ForEach ($shared in $sharedInstallsAll)
{
	$IncludedFiles += Get-ChildItem $sharedInstallPath 	| Where-Object {$_.Name -eq $shared}  
}

$ISOName = $BaseDestRootPath + '\IDShield\ISO\IDShieldExtras' + $FlexVersionString + '.iso'
$ISOTitle = 'IDShieldExtras' + $FlexVersionString
$IncludedFiles | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

