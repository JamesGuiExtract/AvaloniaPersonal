Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $FlexIndexVersion,
    [String][Parameter(Mandatory=$true, HelpMessage="Iso location (Internal, External")]
    [ValidateSet("Internal", "External")] $ISOLocation
)


$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "New-ISOFile.ps1"
. $HelperFile

$BuildFileBase = '\\extract.local\Builds'

$FlexVersion = $FlexIndexVersion -replace 'FlexIndex Ver. ', ''
$FlexVersionNum = $FlexIndexVersion -replace 'FlexIndex Ver. ', '_'
$FlexVersionString = $FlexVersionNum -replace '\.', '_'

$VersionFolder= $FlexVersion
if ($ISOLocation -eq "Internal")
{
    $VersionFolder = $FlexIndexVersion
}

$BaseSourceDestRootPath = Join-Path $BuildFileBase "Internal\$FlexIndexVersion"
$isoPath = Join-Path $BuildFileBase "$ISOLocation\$VersionFolder"

$SetupPath = $BaseSourceDestRootPath 

#Create autorun file
$FileDest = Join-Path $ScriptPath 'autorun.inf'
#Create auotrun.inf file
$text = "[autorun]`r`nopen=Install\setup.exe`r`nicon=ExtractInstall.ico"
$text | Set-Content $FileDest
 
#Build list of files and folders to include on the DVD for FlexIndex
$IncludedFiles = Get-ChildItem "$SetupPath" -Exclude "Extract$FlexVersionString.iso"
#| Where-Object {($_.Name -match 'FlexIndex$') -or ($_.Name -match 'SilentInstalls')}
#$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\FlexIndex') | Where-Object {$_.Name -eq 'FLEXInstall.chm'} 
#$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\FlexIndexInstall') | Where-Object {$_.Name -eq 'FlexIndex.ico'} 
#$IncludedFiles += Get-ChildItem (Join-Path $BaseDestRootPath '\Other') | Where-Object {($_.Name -match 'WebAPI$')}
$IncludedFiles += Get-ChildItem $FileDest

$ISOName = $isoPath + '\Extract' + $FlexVersionString + '.iso'
$ISOTitle = 'Extract' + $FlexVersionString

#New-Item ($isoPath + '\FlexIndex') -Type Directory
$IncludedFiles | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle -Force
