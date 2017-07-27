# This script is intended for IIS 7.5 and above (i.e Windows 7 and Server 2008 R2), because before version 7.5, AppPool identities didn't default to "ApplicationPoolIdentity".
# This also happens to mirror the requirements for Extract Systems software in general, which uses .Net Framework 4.6, requires Windows 7 SP1.

#Assumption - before this script is run, an ES installer has set up the FAM, etc. including .Net Framework 4.6

# Add these parameters for SQL server support
#        [Parameter(Mandatory=$true)][string]$DbServerName, 
#        [Parameter(Mandatory=$true)][string]$DbTableName,
#        [Parameter(Mandatory=$true)][boolean]$LocalServer,


param
(
        [Parameter(Mandatory=$true)][string]$pathToDownloadTo, 
        [Parameter(Mandatory=$true)][string]$webApiSourcePath,
        [Parameter(Mandatory=$true)][string]$webApiTargetPath,
        [Parameter(Mandatory=$true)][boolean]$installIIS,
        [Parameter(Mandatory=$false)][string]$webSiteName,
        [Parameter(Mandatory=$true)][string]$Username,
        [Parameter(Mandatory=$true)][SecureString]$Password,
        [Parameter(Mandatory=$false)][int]$HttpPort = 80
)

class Log
{
	[String]$filename

    # the first time this CTOR is called for the specific filename, it probably won't
    # exist. Once it does exist, just keep appending output to it.
    Log([string] $fileName)
    {
        if (![System.IO.File]::Exists($fileName))
        {
            $path = [System.IO.Path]::GetDirectoryName($fileName)
            if (!(Test-Path $path))
            {
                New-Item -ItemType Directory -Force -Path $path
            }
    
            #$text = ' '
            #$text > $fileName
        }

        $this.filename = $fileName
    }
    
	WriteLine ([string]$text)
	{	
		write-host $text
		Add-Content -Path $this.filename -Value $text
	}
}

#External software that may need to be installed by this script:
# .Net framework 4.6 - for Windows versions < Windows 10
# the .Net Core Windows Server Hosting bundle 
# NOTE: DISM.exe is used to install IIS.
<#
DISM is available (pre-installed) in:
Windows 10
Windows 8.1
Windows 8
Windows Server 2016 Technical Preview
Windows Server 2012 R2
Windows Server 2012
#> 
# for other systems please install DISM before running this script - and you'll need to disable the OS version check

function CheckForAdminPrivilege
{
    If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
    {
        throw "This script requires Administrative priviledges to execute"
    }
}

function TestHostPort([string] $fqdn, [int] $port)
{
    try 
    { 
        $cn=new-object System.Net.Sockets.TcpClient
        $cn.connect($fqdn,$port)
        $cn.close()
        return $true
    } catch 
    { 
        return $false
    } 
}

function OpenSslPort
{
    netsh advfirewall firewall add rule name="Open SSL Port" dir=in action=allow protocol=TCP localport=443  
}

function Install_IIS
{
    $log.WriteLine("installing IIS...")
    dism /online /Enable-Feature /FeatureName:Web-Server /All
    dism /online /Enable-Feature /FeatureName:IIS-WebSockets /All
    dism /online /Enable-Feature /FeatureName:IIS-RequestMonitor /All
    dism /online /Enable-Feature /FeatureName:IIS-HttpTracing /All
    dism /online /Enable-Feature /FeatureName:IIS-CertProvider /All
    dism /online /Enable-Feature /FeatureName:IIS-BasicAuthentication /All
    dism /online /Enable-Feature /FeatureName:IIS-WindowsAuthentication /All
    dism /online /Enable-Feature /FeatureName:IIS-DigestAuthentication /All
    dism /online /Enable-Feature /FeatureName:IIS-ClientCertificateMappingAuthentication /All
    dism /online /Enable-Feature /FeatureName:IIS-IISCertificateMappingAuthentication /All
    dism /online /Enable-Feature /FeatureName:IIS-URLAuthorization /All
    dism /online /Enable-Feature /FeatureName:IIS-ManagementScriptingTools /All
    dism /online /Enable-Feature /FeatureName:IIS-ManagementService /All
    dism /online /Enable-Feature /FeatureName:WAS-WindowsActivationService /All
    dism /online /Enable-Feature /FeatureName:WAS-ProcessModel /All
    dism /online /Enable-Feature /FeatureName:IIS-HostableWebCore /All
    $log.WriteLine("Done installing IIS")
}

function OsVersionGreaterThan([int] $minMajorVersion, [int] $minMinorVersion)
{
    $version = (Get-CimInstance Win32_OperatingSystem).version
    $parts = $version.Split('.')
    if ($parts.Count -lt 2)
    {
        throw "Could not parse returned Windows version: $version"
    }

    $majorVer = $parts.Item(0) -as [int]
    $minorVer = $parts.Item(1) -as [int]
    if ($majorVer -lt $minMajorVersion -or $minMajorVersion -eq $majorVer -and $minorVer -lt $minMinorVersion) 
    {
        return $false
    }

    return $true
}

function DownloadFile([string] $url, [string] $output)
{
    try
    {
        if ([String]::IsNullOrWhiteSpace($url))
        {
            throw "empty value for URL"   
        }

        if ([String]::IsNullOrWhiteSpace($output))
        {
            throw "empty value for output"
        }
        
        #(New-Object System.Net.WebClient).DownloadFile($url, $output)
        Import-Module BitsTransfer
        Start-BitsTransfer -Source $url -Destination $output
    }
    catch 
    {
        throw "DownloadFile error, message: $_"
    }
}

function ExeNameFromURL([string] $url)
{
    $parts = $url.Split('/')
    $lastPart = $parts.Count - 1
    [string] $name = $parts.Item($lastPart)

    if (!$name.Contains(".exe"))
    {
        throw "last portion of URL did not contain .EXE"
    }

    return $name
}

#this installs the .Net Core and the ASPNetCoreModule, used by the IIS application pool
function InstallWindowsHostingBundle([string] $path)
{
    $hostingURL = 'https://download.microsoft.com/download/C/3/2/C32D45DC-6057-4E09-8FE2-25416934BDBB/DotNetCore.1.0.5_1.1.2-WindowsHosting.exe'
    $log.WriteLine("Downloading the Windows Hosting Bundle from: $hostingURL")

    $id = ExeNameFromURL($hostingURL)
    $output = [System.IO.Path]::Combine($path, $id)
    DownloadFile $hostingURL $output

    $log.WriteLine("Download finished, invoking installer...")
    # now $output contains the downloaded installer path + filename, so invoke the command
    Start-Process $output -ArgumentList '-quiet'
    $log.WriteLine("Install of Windows Hosting Bundle is done.")
    $log.WriteLine("Restarting IIS...")
    IISReset
    Start-Sleep -s 3
    $log.WriteLine("Done")

    # TODO - verify that installing actually creates an appPool named AspNetCoreApplication!
}

function CopyDocumentAPI([string] $source, [string] $dest)
{
    $log.WriteLine("copying source from: $source, to destination: $dest")
    if (!(Test-Path -path $dest)) 
    {
        New-Item $dest -Type Directory
    }

    Copy-Item -Path $source\* -Destination $dest

    # TODO - do I need to set permissions on the folder?

    $log.WriteLine("Done")
}

# update the AspNetCore command parameters, which have text substitution tags by default,
# and now require real values
#
function UpdateWebApiWebConfig($path)
{
    $sourceLauncherPath = '%LAUNCHER_PATH%'
    $destLauncherPath = 'DocumentAPI.exe'

    $filePath = [System.IO.Path]::Combine($path, "web.config")
    $text = (Get-Content $filePath | out-string)
    $text1 = $text.Replace($sourceLauncherPath,$destLauncherPath)

    $sourceLauncherArgs = '%LAUNCHER_ARGS%'
    $destLauncherArgs = ""
    $text2 = $text1.Replace($sourceLauncherArgs,$destLauncherArgs)
    
    [System.IO.File]::WriteAllText($filePath, $text2)
}

# assumes cwd is %windir%\system32\inetsrv, where appcmd resides
function AddSite([string] $siteName, [string] $path)
{
    if ($siteName -eq "Default Web Site")
    {
        $log.WriteLine("Site is Default Web Site, so no action add site performed")
        # TODO - in this case, how to ensure that binding is correct?
        return
    }

    # Note: /id:intValue has been omitted here so that IIS will assign the next ID value automatically
    # Note: this creates site: $siteName, app: $siteName/, and vdir: $siteName/, and associates the site with the default app pool
    $bindings = 'http/*:' + $HttpPort + ':,https/*:443:'
    $ret = .\appcmd add site /name:$siteName /physicalPath:$path /bindings:$bindings
    $log.WriteLine("added Site: $siteName, appCmd returned: $ret")
}

# assumes cwd is %windir%\system32\inetsrv, where appcmd resides
function SetApplication([string] $siteName, [string] $path)
{
    # the app has already been created, just need to associate the app with the correct appPool
    $ret = ./appcmd set app "$siteName/" /applicationPool:AspNetCoreApplication
    $log.WriteLine("Modified Application, appCmd returned: $ret")
}

function AddAppPool
{
    # a special app pool is needed just to set the .Net CLR Version to "No Managed Runtime"
    $ret = .\appcmd add apppool /name:AspNetCoreApplication /managedRuntimeVersion:
    $log.WriteLine("added AppPool, appCmd returned: $ret")

    # Now set the identity - username and password of an administrative account
    Import-Module WebAdministration

    $pool = Get-Item "IIS:\AppPools\AspNetCoreApplication"
    $pool.processModel.userName = $Username
    $pool.processModel.password = $Password
    #identityType.SpecificUser = 3
    $pool.processModel.identityType = 3
    $pool | Set-Item
    $pool.Stop()
    $pool.Start()

    $log.WriteLine("Set appPool identity to User: $Username")
}

# assumes cwd is %windir%\system32\inetsrv, where appcmd resides
function AddVirtualDirectory([string] $siteName, [string] $path)
{
    $appName = "$siteName/DocumentAPI"
#    $ret = .\appcmd add vdir /app.name:$appName /path:/ /physicalPath:$path
    $ret = .\appcmd add vdir /app.name:$appName /path:/
    $log.WriteLine("added virtual Directory, appCmd returned: $ret")
}

function SetAccessOnFolderAndContents([string] $targetPath)
{
    $log.WriteLine("Setting access permissions on folder: $targetPath")
    $acl = Get-Acl $targetPath
    
    #This access rule sets the permissions on all of the folder's child objects.
    $ar_contents = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\AspNetCoreApplication", "ReadAndExecute", "ContainerInherit, ObjectInherit", "InheritOnly", "Allow")
    $acl.SetAccessRule($ar_contents)
    
    #This access rule sets the access on the top-level object (folder) - and yes, I tried the bitwise combination of the PropagationFlags enumeration to no avail, only works when I use two separate access rules.
    $ar_folder = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\AspNetCoreApplication", "ReadAndExecute", "ContainerInherit, ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($ar_folder)
    Set-Acl $targetPath $acl
    $log.WriteLine("Done")
}

#this function gets the SQL version that corresponds to the instance name, or if the
#instance name is empty, it returns the latest version of SQL that is installed
function GetSqlVersion([string] $instanceName)
{
    $inst = (get-itemproperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server').InstalledInstances
    $maxVersion = ""
    $maxEdition = ""
    foreach ($i in $inst)
    {
        $p = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL').$i
        $edition = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$p\Setup").Edition
        $version = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$p\Setup").Version

        if (![String]::IsNullOrWhiteSpace($i) -and 0 -eq $instanceName.CompareTo($i))
        {
            return $version
        }
        elseif ($version -gt $maxVersion)
        {
            $maxVersion = $version
            $maxEdition = $edition
        }
    }

    return $maxVersion
}

function ConvertSqlVersionToPathVersion([string] $sqlVersion)
{
    $parts = $sqlVersion.Split('.')
    $majorVersion = $parts[0]
    if ([String]::IsNullOrWhiteSpace($majorVersion))
    {
        return "Unknown_Version";
    }

    return $majorVersion + "0"
}

#C:\Program Files (x86)\Microsoft SQL Server\110\Tools\PowerShell\Modules\SQLPS
#C:\Program Files (x86)\Microsoft SQL Server\SQL_Version\Tools\PowerShell\Modules\SQLPS
function PathToSQLPS($pathVersion)
{
    $path = "C:\Program Files (x86)\Microsoft SQL Server\$pathVersion\Tools\PowerShell\Modules"
    return $path;
}


#When IIS and SQL Server are both on the same system, the ApplicationPoolIdentity is:
#IIS AppPool\appPoolName
#When SQL Server is remote, then the login usede for the application pool identity is:
#<domain>\<computerName>$
#The virtual account (used in the first case) only exists on the local machine.
#Thus the login is transformed into an access from the domain\computer$ - so SQL Server will need a login for that
#
function MakeRemoteApplicationPoolIdentity
{
    $name = (Get-WmiObject Win32_ComputerSystem).Name
    $domain = (Get-WmiObject Win32_ComputerSystem).Domain
    return "$domain\$name$"
}

function ImportSqlPowerShellModule
{
    try
    {
        Import-Module SQLPS -DisableNameChecking
    }
    catch
    {
        $log.WriteLine("Warning - initial import of SQLPS module failed - trying again...")
    
        try
        {
            $sqlVer = GetSqlVersion "MSSQLSERVER"
            $pathVer = ConvertSqlVersionToPathVersion($sqlVer)
            $pathToSQLPowerShell = PathToSqlPS($pathVer)
            
            $log.WriteLine("Path to SQL PowerShell import module: $pathToSQLPowerShell")
            
            #OK, now that we have the path to the SQL PS module, add it to the environment
            $env:PSModulePath = $env:PSModulePath + ";$pathToSqlPowerShell"
            Import-Module SQLPS -DisableNameChecking
    
            #get-module -ListAvailable -Name SqlPs
        }
        catch
        {
            $msg = "Error: Import of SQLPS module failed: $_"
            $log.WriteLine($msg)
            throw $msg
        }
    }
}

function ConfigureSQL
{
}

# Make a server-level login account for the IIS appPool, and an associated DB User account,
# and map the minimal roles required by the FAM to access the DB.
function MakeSqlLoginForAppPool
{
    try
    {
        ImportSqlPowerShellModule
    
        [string] $loginName
        if ($LocalServer -eq $false)
        {
            $name = (Get-WmiObject Win32_ComputerSystem).Name
            $domain = (Get-WmiObject Win32_ComputerSystem).Domain
            $loginName = MakeRemoteApplicationPoolIdentity
        }
        else
        {
            $loginName = 'IIS APPPOOL\AspNetCoreApplication'
        }
        $log.WriteLine("Creating login account named: $loginName")
   
        $server = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Server -ArgumentList $DbServerName
        if ($server.Logins.Contains("AspNetCoreApplication"))
        {
            $log.WriteLine("AspNetCoreApplication login account is already set")
            return
        }
    
        #create the server login
        $login = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Login -ArgumentList $server, $loginName
        $login.LoginType = [Microsoft.SqlServer.Management.Smo.LoginType]::WindowsUser 
        $login.Create()
        $log.WriteLine("Created login: $loginName, on server: $DbServerName")
    
        #map the server login to a DB
        $databaseToMap = $DbTableName
        $database = $server.Databases[$databaseToMap]
    
        #Map the login account to the minimum number of roles needed. Note that this automatically create the database user account 
        #as well as the server login account.
        #Note: userName and loginName are the same, so I just use loginName here
        $dbUser = New-Object -TypeName Microsoft.SqlServer.Management.Smo.User -ArgumentList $database, $loginName
        $dbUser.Login = $loginName
        $dbUser.Create()
        Write-Host "user $dbUser created"
    
        $dbRoleReader = $database.Roles["db_datareader"]
        $dbRoleReader.AddMember($loginName)
        $dbRoleReader.Alter()
        Write-Host "user $dbUser added to db_datareader role"
    
        $dbRoleWriter = $database.Roles["db_datawriter"]
        $dbRoleWriter.AddMember($loginName)
        $dbRoleWriter.Alter()
        Write-Host "user $dbUser added to db_datawriter role"
    
        $dbRoleExec = $database.Roles["db_procexecutor"]
        $dbRoleExec.AddMember($loginName)
        $dbRoleExec.Alter()
        Write-Host "user $dbUser added to db_dataexecutor role"
    }
    catch
    {
        Write-Host "Failed: $_"
        Write-Host "Best attempt to auto-configure AspNetCoreApplication login account has failed, please manually configure the SQL Login account for the IIS AppPool: $loginName"
    }
}

<#
        [Parameter(Mandatory=$true)][string]$pathToDownloadTo, 
        [Parameter(Mandatory=$true)][string]$webApiSourcePath,
        [Parameter(Mandatory=$true)][string]$webApiTargetPath,
        [Parameter(Mandatory=$true)][boolean]$installIIS,
        [Parameter(Mandatory=$true)][string]$DbServerName, 
        [Parameter(Mandatory=$true)][string]$DbTableName,
        [Parameter(Mandatory=$true)][boolean]$LocalServer,
        [Parameter(Mandatory=$false)][string]$webSiteName

#>

function LogAllInputParameters
{
    $log.WriteLine("path to log and download to: $pathToDownloadTo")
    $log.WriteLine("path for Web API source (copy from): $webApiSourcePath")
    $log.WriteLine("path to copy Web API to: $webApiTargetPath")
    $log.WriteLine("value of installIIS flag: $installIIS")
    #$log.WriteLine("DbServerName: $DbServerName")
    #$log.WriteLine("DbTableName: $DbTableName")
    #$log.WriteLine("value of LocalServer flag: $LocalServer")
    $log.WriteLine("webSiteName: $webSiteName")
    $log.WriteLine("Username: $Username")
    $log.WriteLine("HttpPort: $HttpPort")
}

#___________________________________________________________________________________________
#____________________ Main code starts here ________________________________________________
#___________________________________________________________________________________________

$ErrorActionPreference = "Stop"

try
{

    $AppCmdFolder = 'c:\windows\system32\inetsrv';

    if ([String]::IsNullOrWhiteSpace($pathToDownloadTo) -or 
        [String]::IsNullOrWhiteSpace($webApiSourcePath) -or
        [String]::IsNullOrWhiteSpace($webApiTargetPath))
    {
        throw "required command argument is empty"
    }

    if ([String]::IsNullOrWhiteSpace($webSiteName))
    {
        $webSiteName = 'ExtractWebAPI'
    }

    $LogFile = [System.IO.Path]::Combine($pathToDownloadTo, "InstallAPI_Log.txt")
    $log = [Log]::new($LogFile)
    Write-Host "Log will be written to: $LogFile"

    LogAllInputParameters
    
    $result = OsVersionGreaterThan 8 0
    if ($result -eq $false)
    {
        throw "Requires Window 8.0 or above - due to use of DISM"
    }

    CheckForAdminPrivilege
    
    $result = TestHostPort "localhost" 443
    if ($result -eq $false)
    {
        $log.WriteLine("Opening port 443 in the Windows Firewall...")
        OpenSslPort
        $log.WriteLine("Port 443 opened")
    }

    if ($installIIS -eq $true)
    {
        Install_IIS
    }
    else
    {
        $log.WriteLine("Skipped installation of IIS")
    }

    InstallWindowsHostingBundle($pathToDownloadTo)

    CopyDocumentAPI $webApiSourcePath $webApiTargetPath
    
    UpdateWebApiWebConfig($webApiTargetPath)

    $cwd = $PSScriptRoot
    Set-Location -Path $AppCmdFolder

        AddAppPool
        AddSite $webSiteName $webApiTargetPath
    
        SetApplication $webSiteName $webApiTargetPath

        #only needed if the AppPool identity is ApplicationPoolIdentity
        #SetAccessOnFolderAndContents($webApiTargetPath)

        # only needed if it is desired to set a VDIR other than default '/' - but in that case swagger.json path will need to be manually fixed.
        # AddVirtualDirectory $webSiteName $webApiTargetPath

    Set-Location -Path $cwd

    #Now add the application pool Id to the specified SQL server - only needed if the AppPool Identity is ApplicationPoolIdentity
    # MakeSqlLoginForAppPool

    $log.WriteLine("Done. Be sure to update the DocumentAPI appsettings.json file, and install an SSL certificate for the site.")
}
catch
{
    Write-Host "Error: $_"
}

