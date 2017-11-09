Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $ServerName,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BaseDestPath,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BaseTargetPath,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $InstallFolder
)

$tempScriptFile = New-TemporaryFile

$LinkPath=($BaseDestPath + $InstallFolder) -replace '\\', '/'
$TargetPath=($BaseTargetPath + $InstallFolder) -replace '\\', '/'

$linkCmd="ln --symbolic ""$LinkPath"" ""$TargetPath"""

$linkCmd | Out-File -Encoding ascii -Append $tempScriptFile.FullName

plink -load product_builder@thoth -m $tempScriptFile.FullName
Remove-Item $tempScriptFile.FullName -Force

