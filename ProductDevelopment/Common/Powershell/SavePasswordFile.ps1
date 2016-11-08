#Change saved password
Param (
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $User,
	[String][Parameter(Mandatory=$true)][ValidateNotNull()] $PasswordFile
)

$cred = (Get-Credential -Credential $User)


($cred).Password | ConvertFrom-SecureString | Out-File $PasswordFile

