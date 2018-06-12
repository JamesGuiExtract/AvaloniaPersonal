Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $FlexIndexVersion
)


$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "New-ISOFile.ps1"
. $HelperFile

$FlexVersion = $FlexIndexVersion -replace 'FlexIndex Ver. ', ''
$FlexVersionNum = $FlexIndexVersion -replace 'FlexIndex Ver. ', '_'
$FlexVersionString = $FlexVersionNum -replace '\.', '_'

$BaseDestRootPath = Join-Path '\\extract.local\Eng\Builds\FlexIndex\Internal\BleedingEdge\' $FlexIndexVersion 
$isoPath = Join-Path '\\extract.local\Eng\Builds\FlexIndex\External\Current' $FlexVersion 

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

$ISOName = $isoPath + '\FlexIndex\Flex' + $FlexVersionString + '.iso'
$ISOTitle = 'Flex' + $FlexVersionString

New-Item ($isoPath + '\FlexIndex') -Type Directory
$IncludedFiles | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

#Create autorun file
$FileDest = Join-Path $ScriptPath 'autorun.inf'
#Create auotrun.inf file
$text = "[autorun]`r`nopen=IDShield\setup.exe`r`nicon=IDShield.ico"
$text | Set-Content $FileDest

$SetupPath = Join-Path $BaseDestRootPath  'IDShield\SetupFiles'

#Build list of files and folders to include on the DVD for FlexIndex
$IncludedFiles = Get-ChildItem $SetupPath | Where-Object {($_.Name -match 'IDShield$') -or ($_.Name -match 'SilentInstalls')}
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\IDShield') | Where-Object {$_.Name -eq 'IDShieldInstall.chm'} 
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\IDShieldInstall') | Where-Object {$_.Name -eq 'IDShield.ico'} 
$IncludedFiles += Get-ChildItem (Join-Path $BaseDestRootPath '\Other') | Where-Object {($_.Name -match 'WebAPI$')}
$IncludedFiles += Get-ChildItem $FileDest

$ISOName = $isoPath + '\IDShield\IDShield' + $FlexVersionString + '.iso'
$ISOTitle = 'IDShield' + $FlexVersionString
New-Item ($isoPath + '\IDShield') -Type Directory
$IncludedFiles | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

#Create autorun file
$FileDest = Join-Path $ScriptPath 'autorun.inf'
#Create auotrun.inf file
$text = "[autorun]`r`nopen=LabDE\setup.exe`r`nicon=LabDE.ico"
$text | Set-Content $FileDest

$SetupPath = Join-Path $BaseDestRootPath  'LabDE\SetupFiles'

#Build list of files and folders to include on the DVD for FlexIndex
$IncludedFiles = Get-ChildItem $SetupPath | Where-Object {($_.Name -match 'LabDE$') -or ($_.Name -match 'SilentInstalls')}
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\LabDE') | Where-Object {$_.Name -eq 'LabDEInstall.chm'} 
$IncludedFiles += Get-ChildItem (Join-Path $SetupPath '\LabDEInstall') | Where-Object {$_.Name -eq 'LabDE.ico'} 
$IncludedFiles += Get-ChildItem (Join-Path $BaseDestRootPath '\Other') | Where-Object {($_.Name -match 'WebAPI$')}
$IncludedFiles += Get-ChildItem $FileDest

$ISOName = $isoPath + '\LabDE\LabDE' + $FlexVersionString + '.iso'
$ISOTitle = 'LabDE' + $FlexVersionString
New-Item ($isoPath + '\LabDE') -Type Directory
$IncludedFiles| New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle
