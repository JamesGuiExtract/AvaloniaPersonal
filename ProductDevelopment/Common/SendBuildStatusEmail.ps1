Param (
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BuildVersion,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $BuildStatus,
	[String][Parameter(Mandatory=$false)] $MessageBody
)

$Subject = "Build " + $BuildVersion + " has " + $BuildStatus

$emailFrom = "developers@extractsystems.com" 
$smtpserver="mail.extractsystems.com" 

# Create the mail client object
$smtp=new-object Net.Mail.SmtpClient($smtpServer) 

# enable ssl
$smtp.EnableSsl=$true

# Send the message
$smtp.Send($emailFrom, "engineering@extractsystems.com", $subject, $MessageBody) 