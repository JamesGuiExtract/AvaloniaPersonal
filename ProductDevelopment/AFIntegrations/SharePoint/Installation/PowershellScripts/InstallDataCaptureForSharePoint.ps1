$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "SharePointDeploymentHelper.ps1"
. $HelperFile

$ExtractSP = "Extract.SharePoint.wsp"
$ExtractDCSP = "Extract.SharePoint.DataCapture.wsp"

$ExtractSPfp = Join-Path $ScriptPath $ExtractSP
$ExtractDCSPfp = Join-Path $ScriptPath $ExtractDCSP

if (-not (Check-SolutionExists $ExtractSP))
{
	Write-Host "Adding Extract common feature to the farm..." -ForegroundColor Green
	Add-SPSolution $ExtractSPfp -ErrorAction Stop | Out-Null
	iisreset
}

if (-not (Check-SolutionDeployed $ExtractSP))
{
	Write-Host "Deploying Extract common feature..." -ForegroundColor Green
	Install-SPSolution $ExtractSP -GACDeployment -Confirm:$false -ErrorAction Stop
	iisreset
}

if (-not (Check-SolutionExists $ExtractDCSP))
{
	Write-Host "Adding Data Capture for SharePoint to the farm..." -ForegroundColor Green
	Add-SPSolution $ExtractDCSPfp -ErrorAction Stop | Out-Null
	iisreset
}

if (-not (Check-SolutionDeployed $ExtractDCSP))
{
	Write-Host "Deploying Data Capture for SharePoint..." -ForegroundColor Green
	Install-SPSolution $ExtractDCSP -GACDeployment -Confirm:$false -ErrorAction Stop
	iisreset
}


iisreset

Stop-TimerService

Write-Host "Installing Data Capture timer jobs..." -ForegroundColor Green
Create-NewTimerJob "Extract.SharePoint.DataCapture" "Extract.SharePoint.DataCapture.ExtractDataCaptureDiskToSharePoint" "Data Capture Disk To SharePoint"
Create-NewTimerJob "Extract.SharePoint.DataCapture" "Extract.SharePoint.DataCapture.ExtractDataCaptureSharePointToDisk" "Data Capture SharePoint To Disk"
Write-Host "Data Capture timer jobs installed..." -ForegroundColor Green
Start-TimerService
