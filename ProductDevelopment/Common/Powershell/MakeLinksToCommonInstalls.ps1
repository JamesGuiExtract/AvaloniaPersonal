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

#Add a Symbolic link on Engsvr for each of the shared folders
foreach ($path in $linkPaths){

	$LinkPath=$BaseDestPath + $path
	$TargetPath=$BaseTargetPath + $path
	
	# command to run on Engsvr
	$linkCmd="cmd /c mklink /J ""$LinkPath"" ""$TargetPath"""
	$script = [scriptblock]::Create($linkCmd)
	Invoke-Command -ComputerName Engsvr -ScriptBlock $script
}


