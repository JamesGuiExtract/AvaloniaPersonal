$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "SharePointDeploymentHelper.ps1"
. $HelperFile

Uninstall-Solutions ".\IDShieldForSharePointInstall.xml"