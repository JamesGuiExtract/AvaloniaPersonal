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
		# This method is supposedly depricated, but I don't know another way to load a generic
		# assembly into the app domain without having its particular version number, etc
		[Reflection.Assembly]::LoadWithPartialName($assemblyName)
	}
}


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

function Delete-TimerJob([string] $jobName)
{
	Get-SPWebApplication | ForEach-Object {
		$_.JobDefinitions | ForEach-Object {
			if ($_.Name -eq $jobName)
			{
				$_.Delete()
			}
		}
	}
}

function Create-NewTimerJob([string]$assemblyName, [string]$timerJobClass, [string]$jobName )
{
	Load-Assembly("Microsoft.SharePoint")
	Load-Assembly($assemblyName)
	
	Stop-Service "SPTimerV4"
	
	Delete-TimerJob($jobName)
	
	Get-SPWebApplication | ForEach-Object {
		$job = New-Object $timerJobClass -arg $jobName,$_
		$sched = New-Object Microsoft.SharePoint.SPMinuteSchedule
		$sched.BeginSecond = 0
		$sched.EndSecond = 59
		$sched.Interval = 1
		$job.Schedule = $sched
		$job.Update()
	}

	Start-Service "SPTimerV4"
}

function Uninstall-Solution([string]$name, [string[]]$features = @(), $timerJobs = $null)
{
	$spAdminServiceName = "SPAdminV4"
    $solution = Get-SPSolution $name -ErrorAction SilentlyContinue

	if ($solution -ne $null) {
        
		# Remove existing timer jobs
		if ($timerJobs -ne $null)
		{
			Stop-Service "SPTimerV4"
			Write-Host "Removing timer jobs..."
			$timerJobs.TimerJob | ForEach-Object {
				Delete-TimerJob $_.Name
			}
			Start-Service "SPTimerV4"
		}

		#Retract the solution
		if ($solution.Deployed) {
				
            Write-Host "Checking for active features..."
			if ($features -ne $null -and $features.Length -gt 0) {
				$features |
					ForEach-Object {
					Write-Host "Deactivating feature $_..."
					Get-SPSite | Get-SPWeb -Limit ALL |
						ForEach-Object -Begin {$featurename=$_} {
							If ((Get-SPFeature -Identity $featurename -Web $_ -ErrorAction SilentlyContinue) -ne $null) {
								Disable-SPFeature -Identity (Get-SPFeature -Identity $featurename -Web $_ -ErrorAction SilentlyContinue).ID -URL $_.URL -Confirm:$false
							}
						}
					}
				}
			}
			
			Write-Host "Retracting solution $name..."
            if ($solution.ContainsWebApplicationResource) {
                $solution | Uninstall-SPSolution -AllWebApplications -Confirm:$false
            } else {
                $solution | Uninstall-SPSolution -Confirm:$false
            }
            Stop-Service -Name $spAdminServiceName
            Start-SPAdminJob -Verbose
            Start-Service -Name $spAdminServiceName    
        
            #Block until we're sure the solution is no longer deployed.
            do { Start-Sleep 2 } while ((Get-SPSolution $name).Deployed)
        
			#Delete the solution
			Write-Host "Removing solution $name..."
			Get-SPSolution $name | Remove-SPSolution -Confirm:$false
		}
}

function Install-Solution([string]$path, [bool]$gac, [bool]$cas, [string[]]$features = @(),
	$timerJobs = $null, [string[]]$webApps = @())
{
    $spAdminServiceName = "SPAdminV4"

    [string]$name = Split-Path -Path $path -Leaf
	
	Uninstall-Solution $name $features $timerJobs
	
    #Add the solution
    Write-Host "Adding solution $name..."
    $solution = Add-SPSolution $path
    
    #Deploy the solution
    if (!$solution.ContainsWebApplicationResource) {
        Write-Host "Deploying solution $name to the Farm..."
        $solution | Install-SPSolution -GACDeployment:$gac -CASPolicies:$cas -Confirm:$false
    } else {
        if ($webApps -eq $null -or $webApps.Length -eq 0) {
            Write-Warning "The solution $name contains web application resources but no web applications were specified to deploy to."
            return
        }
        $webApps | ForEach-Object {
            Write-Host "Deploying solution $name to $_..."
            $solution | Install-SPSolution -GACDeployment:$gac -CASPolicies:$cas -WebApplication $_ -Confirm:$false
        }
    }
	

    Stop-Service -Name $spAdminServiceName
    Start-SPAdminJob -Verbose
    Start-Service -Name $spAdminServiceName    
    
    #Block until we're sure the solution is deployed.
    do { Start-Sleep 2 } while (!((Get-SPSolution $name).Deployed))
	
	# Add any timer jobs
	if ($timerJobs -ne $null)
	{
		Write-Host "Adding timer jobs..."
		$timerJobs.TimerJob | ForEach-Object {
			Create-NewTimerJob $_.Assembly $_.Class $_.Name
		}
	}
}

function Install-Solutions([string]$configFile)
{
    if ([string]::IsNullOrEmpty($configFile)) { return }

    [xml]$solutionsConfig = Get-Content $configFile
    if ($solutionsConfig -eq $null) { return }

    $solutionsConfig.Solutions.Solution | ForEach-Object {
        [string]$path = Get-AbsolutePathToFile $_.Path
		
		if (![System.IO.File]::Exists($path))
		{
			Write-Host "File not found $path..."
			return
		}
		
        [bool]$gac = [bool]::Parse($_.GACDeployment)
        [bool]$cas = [bool]::Parse($_.CASPolicies)
		$features = $_.Features.Feature
		$timerJobs = $_.TimerJobs
		$webApps = $_.WebApplications.WebApplication
        Install-Solution $path $gac $cas $features $timerJobs $webApps
    }
}

function Uninstall-Solutions([string]$configFile)
{
     if ([string]::IsNullOrEmpty($configFile)) { return }

    [xml]$solutionsConfig = Get-Content $configFile
    if ($solutionsConfig -eq $null) { return }

    $solutionsConfig.Solutions.Solution | ForEach-Object {
        [string]$path = Get-AbsolutePathToFile $_.Path
		if (![System.IO.File]::Exists($path))
		{
			Write-Host "File not found $path..."
			return
		}
		
		[string]$name = Split-Path -Path $path -Leaf
		$features = $_.Features.Feature
		$timerJobs = $_.TimerJobs
        Uninstall-Solution $name $features $timerJobs
    }
}

