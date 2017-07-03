
Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $FlexIndexVersion
)
function MakeDirLink([String] $ServerName, [String] $LinkPath, [String] $TargetPath)
{
	# command to run on $ServerName
	$linkCmd="cmd /c mklink /D '$LinkPath' '$TargetPath'"
	Write-Output $linkCmd
	$script = [scriptblock]::Create($linkCmd)
	Invoke-Command -ComputerName $ServerName -ScriptBlock $script	
}


$FlexIndexBaseDestRootPath = '\\Engsvr\Internal\ISOTree\' + $FlexIndexVersion + '\FLEXIndex\ISO'
$IDShieldBaseDestRootPath = '\\Engsvr\Internal\ISOTree\' + $FlexIndexVersion + '\IDShield\ISO'
$LabDEBaseDestRootPath = '\\Engsvr\Internal\ISOTree\' + $FlexIndexVersion + '\LabDE\ISO'
$FlexIndexISOSetupFilesPath = 'D:\Internal\ProductReleases\FlexIndex\Internal\BleedingEdge\' + $FlexIndexVersion + '\FLEXIndex\ISO'
$IDShieldISOSetupFilesPath = 'D:\Internal\ProductReleases\FlexIndex\Internal\BleedingEdge\' + $FlexIndexVersion + '\IDSHield\ISO'
$LabDEISOSetupFilesPath = 'D:\Internal\ProductReleases\FlexIndex\Internal\BleedingEdge\' + $FlexIndexVersion + '\LabDE\ISO'


MakeDirLink 'EngSvr' $FlexIndexISOSetupFilesPath $FlexIndexBaseDestRootPath 
MakeDirLink 'EngSvr' $IDShieldISOSetupFilesPath $IDShieldBaseDestRootPath 
MakeDirLink 'EngSvr' $LabDEISOSetupFilesPath $LabDEBaseDestRootPath 
