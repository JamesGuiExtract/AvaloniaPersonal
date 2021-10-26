# Rquires
#   Azure CLI - It can be downloaded https://aka.ms/installazurecliwindows
#   User that is running script will need Cloud application administrator role


param (
    [String][Parameter(HelpMessage='Azure AD application name to create.', Mandatory=$true)][ValidateNotNull()] $ApplicationName
)


$subscriptions =  az login | ConvertFrom-Json

Write-Host "Azure Tenant: " $subscriptions[0].tenantId

$json  = az ad app create --display-name $ApplicationName --available-to-other-tenants false --native-app true --reply-urls "https://login.microsoftonline.com/common/oauth2/nativeclient"  | ConvertFrom-Json

Write-Host "Azure Client ID: " $json.appId

