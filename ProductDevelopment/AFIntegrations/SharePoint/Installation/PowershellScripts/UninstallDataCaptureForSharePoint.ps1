$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "SharePointDeploymentHelper.ps1"
. $HelperFile

$ExtractSP = "Extract.SharePoint.wsp"
$IDShieldSP = "Extract.SharePoint.Redaction.wsp"
$ExtractDCSP = "Extract.SharePoint.DataCapture.wsp"

Stop-TimerService
Write-Host "Removing Data Capture timer jobs..." -ForegroundColor Green
Delete-TimerJob "Data Capture Disk To SharePoint"
Delete-TimerJob "Data Capture SharePoint To Disk"
Write-Host "Data Capture timer jobs removed..." -ForegroundColor Green
Start-TimerService


if (Check-SolutionExists $ExtractDCSP)
{
	if (Check-SolutionDeployed $ExtractDCSP)
	{
		Write-Host "Undeploying Extract Data Capture for SharePoint..." -ForegroundColor Green
		Uninstall-SPSolution $ExtractDCSP -Confirm:$false -ErrorAction Stop
		iisreset
	}
	
	Write-Host "Removing Extract Data Capture for SharePoint..." -ForegroundColor Green
	Remove-SPSolution $ExtractDCSP -Confirm:$false -ErrorAction Stop
	iisreset
}

# Do not remove Extract.SharePoint unless id shield is not installed
if ((-not (Check-SolutionExists $IDShieldSP)) `
		-and (Check-SolutionExists $ExtractSP))
{
	if (Check-SolutionDeployed $ExtractSP)
	{
		Write-Host "Undeploying Extract Systems common feature..." -ForegroundColor Green
		Uninstall-SPSolution $ExtractSP -Confirm:$false -ErrorAction Stop
		Start-Sleep 30
		iisreset
	}
	
	Write-Host "Removing Extract Systems common feature..." -ForegroundColor Green
	Remove-SPSolution $ExtractSP -Confirm:$false -ErrorAction Stop
}

iisreset
