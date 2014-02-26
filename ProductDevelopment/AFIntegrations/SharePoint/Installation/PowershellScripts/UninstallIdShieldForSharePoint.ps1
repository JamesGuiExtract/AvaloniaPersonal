$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "SharePointDeploymentHelper.ps1"
. $HelperFile

$ExtractSP = "Extract.SharePoint.wsp"
$IDShieldSP = "Extract.SharePoint.Redaction.wsp"
$ExtractDCSP = "Extract.SharePoint.DataCapture.wsp"

Write-Host "Removing ID Shield timer jobs..." -ForegroundColor Green
Delete-TimerJob "ID Shield Disk To SharePoint"
Delete-TimerJob "ID Shield SharePoint To Disk"
Write-Host "ID Shield timer jobs removed..." -ForegroundColor Green

#Restart the timer service - this is to clear the old timer jobs from the cache
Stop-TimerService
Start-TimerService

if (Check-SolutionExists $IDShieldSP)
{
	if (Check-SolutionDeployed $IDShieldSP)
	{
		Write-Host "Undeploying ID Shield for SharePoint..." -ForegroundColor Green
		Uninstall-SPSolution $IDShieldSP -Confirm:$false -ErrorAction Stop
		
		WaitForJobToFinish($IDShieldSP)
	}
	
	Write-Host "Removing ID Shield for SharePoint..." -ForegroundColor Green
	Remove-SPSolution $IDShieldSP -Confirm:$false -ErrorAction Stop
	
	while (Check-SolutionExists $IDShieldSP)
	{
		Start-Sleep 30
		Write-Host "--> Checking if ID Shield for SharePoint has been removed..." -ForegroundColor Yellow
	}
}

# Do not remove Extract.SharePoint unless data capture is not installed
if ((-not (Check-SolutionExists $ExtractDCSP)) `
		-and (Check-SolutionExists $ExtractSP))
{
	if (Check-SolutionDeployed $ExtractSP)
	{
		Write-Host "Undeploying Extract Systems common feature..." -ForegroundColor Green
		Uninstall-SPSolution $ExtractSP -Confirm:$false -ErrorAction Stop

		WaitForJobToFinish($ExtractSP)
	}
	
	Write-Host "Removing Extract Systems common feature..." -ForegroundColor Green
	Remove-SPSolution $ExtractSP -Confirm:$false -ErrorAction Stop
	
	while (Check-SolutionExists $ExtractSP)
	{
		Start-Sleep 30
		Write-Host "--> Checking if Extract Systems common feature has been removed..." -ForegroundColor Yellow
	}
}

