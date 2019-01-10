Param(
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $ServiceName,
	[Int] $NumberOfIterations = 30
)

$Service = Get-Service $ServiceName

if ($Service.Status -eq 'Stopped') {
    Start-Service $Service
}

$iterationCount = 0
while ($iterationCount -lt $NumberOfIterations )
{
    Start-Sleep -Seconds (Get-Random -Minimum 25 -Maximum 35)
    Stop-Service $Service
    Start-Service $Service
    $iterationCount = $iterationCount + 1
}
	



