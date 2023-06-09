Param (
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BuildVersion,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BuildStatus,
	[String][Parameter(Mandatory=$false)] $MessageBody
)
$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

$Subject = "Internal Build " + $BuildVersion + " has " + $BuildStatus

$emailFrom = "Build@extractsystems.com" 
$smtpserver="extractsystems-com.mail.protection.outlook.com" 

# Create the mail client object
$smtp=new-object Net.Mail.SmtpClient($smtpServer, 25) 

$username = "devices@extractsystems.com"
$password = cat ($scriptDir + "\devicespassword.txt") | ConvertTo-SecureString

$smtp.Credentials =  New-Object -TypeName System.Management.Automation.PSCredential($username, $password)
# enable ssl
$smtp.EnableSsl=$true
# Send the message
$smtp.Send($emailFrom, "builds@extractsystems.com", $subject, $MessageBody) 