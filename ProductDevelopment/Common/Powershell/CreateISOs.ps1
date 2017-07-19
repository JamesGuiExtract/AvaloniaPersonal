Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $FlexIndexVersion
)


$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "New-ISOFile.ps1"
. $HelperFile

$CreateFlexIndex = Join-Path $ScriptPath "CreateFlexINDEX_ISO_Folder.ps1"
$CreateIDShield = Join-Path $ScriptPath "CreateIDShield_ISO_Folder.ps1"
$CreateLabDE = Join-Path $ScriptPath "CreateLabDE_ISO_Folder.ps1"

$FlexVersionNum = $FlexIndexVersion -replace 'FlexIndex Ver. ', '_'
$FlexVersionString = $FlexVersionNum -replace '\.', '_'

Invoke-Expression "& `"$CreateFlexIndex`" `"$FlexIndexVersion`""
Invoke-Expression "& `"$CreateIDShield`" `"$FlexIndexVersion`""
Invoke-Expression "& `"$CreateLabDE`" `"$FlexIndexVersion`""

$BaseDestRootPath = 'M:\ISOTree\' + $FlexIndexVersion 

$ISOName = $BaseDestRootPath + '\FlexIndex\ISO\Flex' + $FlexVersionString + '.iso'
$ISOTitle = 'Flex' + $FlexVersionString
New-Item ($BaseDestRootPath + '\FlexIndex\ISO') -Type Directory
Get-ChildItem ($BaseDestRootPath + '\FLEXIndex\FlexIndex') | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

$ISOName = $BaseDestRootPath + '\FlexIndex\ISO\FlexExtras' + $FlexVersionString + '.iso'
$ISOTitle = 'FlexExtras' + $FlexVersionString
Get-ChildItem ($BaseDestRootPath + '\FLEXIndex\FlexIndexExtras') | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

$ISOName = $BaseDestRootPath + '\IDShield\ISO\IDShield' + $FlexVersionString + '.iso'
$ISOTitle = 'IDShield' + $FlexVersionString
New-Item ($BaseDestRootPath + '\IDShield\ISO') -Type Directory
Get-ChildItem ($BaseDestRootPath + '\IDShield\IDShield') | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

$ISOName = $BaseDestRootPath + '\IDShield\ISO\IDShieldExtras' + $FlexVersionString + '.iso'
$ISOTitle = 'IDShieldExtras' + $FlexVersionString
Get-ChildItem ($BaseDestRootPath + '\IDShield\IDShieldExtras') | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

$ISOName = $BaseDestRootPath + '\LabDE\ISO\LabDE' + $FlexVersionString + '.iso'
$ISOTitle = 'IDShield' + $FlexVersionString
New-Item ($BaseDestRootPath + '\LabDE\ISO') -Type Directory
Get-ChildItem ($BaseDestRootPath + '\LabDE\LabDE') | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

$ISOName = $BaseDestRootPath + '\LabDE\ISO\LabDEExtras' + $FlexVersionString + '.iso'
$ISOTitle = 'LabDEExtras' + $FlexVersionString
Get-ChildItem ($BaseDestRootPath + '\LabDE\LabDEExtras') | New-ISOFile -Path $ISOName -Media DVDPLUSR -Title $ISOTitle

$Makelinks = Join-Path $ScriptPath "MakeISOLinks.ps1"
Invoke-Expression "& `"$Makelinks`" `"$FlexIndexVersion`""