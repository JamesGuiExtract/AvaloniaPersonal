$Invocation = (Get-Variable MyInvocation -Scope 0).Value
$ScriptPath = Split-Path $Invocation.MyCommand.Path

$HelperFile = Join-Path $ScriptPath "SharePointDeploymentHelper.ps1"
. $HelperFile

$ExtractSP = "Extract.SharePoint.wsp"
$IDShieldSP = "Extract.SharePoint.Redaction.wsp"

$ExtractSPfp = Join-Path $ScriptPath $ExtractSP
$IDShieldSPfp = Join-Path $ScriptPath $IDShieldSP

if (-not (Check-SolutionExists $ExtractSP))
{
	Write-Host "Adding Extract common feature to the farm..." -ForegroundColor Green
	Add-SPSolution $ExtractSPfp -ErrorAction Stop | Out-Null
}

if (-not (Check-SolutionDeployed $ExtractSP))
{
	Write-Host "Deploying Extract common feature..." -ForegroundColor Green
	Install-SPSolution $ExtractSP -GACDeployment -Confirm:$false  -ErrorAction Stop
	
	WaitForJobToFinish($ExtractSP)
}

if (-not (Check-SolutionExists $IDShieldSP))
{
	Write-Host "Adding ID Shield for SharePoint to farm..." -ForegroundColor Green
	Add-SPSolution $IDShieldSPfp  -ErrorAction Stop | Out-Null
}

if (-not (Check-SolutionDeployed $IDShieldSP))
{
	Write-Host "Deploying ID Shield for SharePoint..." -ForegroundColor Green
	Install-SPSolution $IDShieldSP -GACDeployment -Confirm:$false  -ErrorAction Stop
	
	WaitForJobToFinish($IDShieldSP)
}

Write-Host "Installing ID Shield timer jobs..." -ForegroundColor Green
Create-NewTimerJob "Extract.SharePoint.Redaction" "Extract.SharePoint.Redaction.IDShieldFolderSweeper" "ID Shield Disk To SharePoint"
Create-NewTimerJob "Extract.SharePoint.Redaction" "Extract.SharePoint.Redaction.IDShieldFileExporter" "ID Shield SharePoint To Disk"
Write-Host "ID Shield timer jobs installed..." -ForegroundColor Green


