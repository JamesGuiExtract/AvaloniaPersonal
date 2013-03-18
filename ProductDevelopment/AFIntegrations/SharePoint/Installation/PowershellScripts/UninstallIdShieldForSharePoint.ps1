$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "SharePointDeploymentHelper.ps1"
. $HelperFile

$ExtractSP = "Extract.SharePoint.wsp"
$IDShieldSP = "Extract.SharePoint.Redaction.wsp"
$ExtractDCSP = "Extract.SharePoint.DataCapture.wsp"

Stop-TimerService
Write-Host "Removing ID Shield timer jobs..." -ForegroundColor Green
Delete-TimerJob "ID Shield Disk To SharePoint"
Delete-TimerJob "ID Shield SharePoint To Disk"
Write-Host "ID Shield timer jobs removed..." -ForegroundColor Green
Start-TimerService

if (Check-SolutionExists $IDShieldSP)
{
	if (Check-SolutionDeployed $IDShieldSP)
	{
		Write-Host "Undeploying ID Shield for SharePoint..." -ForegroundColor Green
		Uninstall-SPSolution $IDShieldSP -Confirm:$false -ErrorAction Stop
		iisreset
	}
	
	Write-Host "Removing ID Shield for SharePoint..." -ForegroundColor Green
	Remove-SPSolution $IDShieldSP -Confirm:$false -ErrorAction Stop
	iisreset
}

# Do not remove Extract.SharePoint unless data capture is not installed
if ((-not (Check-SolutionExists $ExtractDCSP)) `
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
