if ((Get-PsSnapin -name Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -eq $null)
{
	Add-PsSnapin Microsoft.SharePoint.PowerShell
}

# Checks if a particular assembly has already been loaded into the app domain
function Check-AssemblyLoaded([string]$assemblyName)
{
	if (([AppDomain]::CurrentDomain.GetAssemblies() | ? {$_.GetName().Name -eq $assemblyName}) -ne $null)
	{
		$true
	}
	else
	{
		$false
	}
}

# Loads the specified assembly into the app domain (if it has not already been loaded)
function Load-Assembly([string]$assemblyName)
{
	if (!(Check-AssemblyLoaded $assemblyName))
	{
		# This method is supposedly deprecated, but I don't know another way to load a generic
		# assembly into the app domain without having its particular version number, etc
		[void][Reflection.Assembly]::LoadWithPartialName($assemblyName)
	}
}

# Builds the absolute path to the specified file name
function Get-AbsolutePathToFile([string]$fileName)
{
		if (![System.IO.Path]::IsPathRooted($fileName))
		{
			$localPath = Split-Path $MyInvocation.ScriptName -Parent
			$fileName = Join-Path $localPath $fileName
			$fileName = [System.IO.Path]::GetFullPath($fileName)
		}
		
		$fileName
}

# Checks if the specified solution is installed
function Check-SolutionExists([string]$solutionFile)
{
	(Get-SPSolution $solutionFile -ErrorAction SilentlyContinue) -ne $null
}

# Checks if the specified solution is deployed
function Check-SolutionDeployed([string]$solutionFile)
{
	$solution = Get-SPSolution $solutionFile -ErrorAction SilentlyContinue
	if ($solution -eq $null)
	{
		$false
	}
	else
	{
		$solution.Deployed
	}
}

# Stops the SPTimerV4 service
function Stop-TimerService()
{
	Write-Host "Stopping timer service"
	Stop-Service "SPTimerV4"
	while ((Get-Service -Name "SPTimerV4").Status -ne "Stopped")
	{
		Write-Host "Waiting for timer service to stop..." -ForegroundColor Green
		Start-Sleep 2
	}
}

# Starts the SPTimerV4 service
function Start-TimerService()
{
	Write-Host "Starting timer service"
	Start-Service "SPTimerV4"
	while((Get-Service -Name "SPTimerV4").Status -ne "Running")
	{
		Write-Host "Waiting for timer service to start..." -ForegroundColor Green
		Start-Sleep 2
	}
}

# Attempts to delete the specified timer job from all webapplications
# in the SharePoint farm
function Delete-TimerJob([string] $jobname)
{
	Get-SPWebApplication | ForEach-Object {
		$_.JobDefinitions | ForEach-Object {
			if ($_.Name -eq $jobname)
			{
				$_.Delete()
			}
		}
	}
}

# Attempts to add the specified timer job to all webapplications in the
# SharePoint farm.
# Note: This will create the job with a schedule that runs every minute
# this could be extended in the future to take schedule information
# so that the schedule can be customized for each job being created
# ARGS:
# assemblyName - The simple name for the assembly that contains the timer job
#	This is vital since we must create an instance of the job to add it to the
#	SP content database
# timerJobClass - The full name for the class (including namespace) so that the
#	proper timer class is created
# jobname - The name to assign to the timer job
function Create-NewTimerJob([string]$assemblyName, [string]$timerJobClass, [string]$jobname )
{
	Load-Assembly("Microsoft.SharePoint")
	Load-Assembly($assemblyName)
	
	Delete-TimerJob $jobname
	
	Get-SPWebApplication | ForEach-Object {
		$job = New-Object $timerJobClass -arg $jobname,$_
		$sched = New-Object Microsoft.SharePoint.SPMinuteSchedule
		$sched.BeginSecond = 0
		$sched.EndSecond = 59
		$sched.Interval = 1
		$job.Schedule = $sched
		$job.Update()
	}
}