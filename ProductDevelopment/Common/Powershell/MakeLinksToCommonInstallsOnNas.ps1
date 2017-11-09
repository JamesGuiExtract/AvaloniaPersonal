Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BaseDestPath,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BaseTargetPath
)

# Folder names of the shared installs in the baseTargetFolder
$linkPaths = 	"DotNet 3.5 Framework",
				"DotNet 4.6 Framework",
				"Powershell",
				"SQLServerExpress2014",
				"SQLServerExpress2014Mgr"

$tempScriptFile = New-TemporaryFile

#Add a Symbolic link on Engsvr for each of the shared folders
foreach ($path in $linkPaths){

	$LinkPath=($BaseDestPath + $path) -replace '\\', '/'
	$TargetPath=($BaseTargetPath + $path) -replace '\\', '/'

	# command to run on thoth
	$linkCmd="ln --symbolic ""$LinkPath"" ""$TargetPath"""
	
	$linkCmd | Out-File -Encoding ascii -Append $tempScriptFile.FullName
}
plink -load product_builder@thoth -m $tempScriptFile.FullName
Remove-Item $tempScriptFile.FullName -Force





