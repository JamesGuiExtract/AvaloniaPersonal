$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path
Write-Host $ScriptPath
$HelperFile = Join-Path $ScriptPath "SharePointDeploymentHelper.ps1"
. $HelperFile
Uninstall-Solutions ".\IDShieldForSharePointInstall.xml"