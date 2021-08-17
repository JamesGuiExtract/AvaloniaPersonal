param (
    [Parameter(HelpMessage="Path for ConsoleRunner exe")]
        [string]$ConsoleRunner="$PSScriptRoot\..\..\..\..\Binaries\Debug\nunit3-console.exe",
    [Parameter(, HelpMessage="Directory containing the test.dlls")]
        [string]$DllDir="$PSScriptRoot\..\..\..\..\Binaries\Debug",
    [Parameter(HelpMessage="Name of output file without extension 2 files will be created .htm and .xml")]
        [string]$OutputFile="$PSScriptRoot\TestResults-NonInteractive",
    [Parameter(HelpMessage="Name of xslt transform file")]
        [string]$Transform="$PSScriptRoot\html-report.xslt",
    [Parameter(HelpMessage="Options for nunit3")]
        [string[]]$Options=("--where:`"cat!=Interactive and cat!=Broken and cat!=Automated_ADMIN`"", "--dispose-runners", "--result=`"$OutputFile.xml`"", "--result=`"$OutputFile.htm;transform=$Transform`""),
    [Parameter(HelpMessage="Options for nunit3")]
        [string[]]$AdditionalOptions,
    [Parameter(HelpMessage="List of files to run with nunint")]
        [string[]]$FileList
)


if (!$FileList){
    $FileList = @(Get-ChildItem "$dllDir\*.Test.dll" | ForEach-Object{ '"' + $_.FullName + '"' })
} 

foreach($FullFileName in $FileList){
	$FileName = Split-Path -Path $FullFileName -Leaf
	$FileName = $FileName -Replace ".dll" -Replace "\." 
	$Options = ("--where:`"cat!=Interactive and cat!=Broken and cat!=Automated_ADMIN`"", "--dispose-runners", "--result=`"$OutputFile.$FileName.xml`"", "--result=`"$OutputFile.$FileName.htm;transform=$Transform`"")
	
	$params = @($FullFileName) + $Options + $AdditionalOptions

	$ConsoleRunner
	$params
	& $ConsoleRunner $params
}
