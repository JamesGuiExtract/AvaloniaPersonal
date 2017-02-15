# IO.Swagger.Api.UsersApi

All URIs are relative to *https://localhost/*

Method | HTTP request | Description
------------- | ------------- | -------------
[**ApiUsersLoginPost**](UsersApi.md#apiusersloginpost) | **POST** /api/Users/Login | login
[**ApiUsersLogoutDelete**](UsersApi.md#apiuserslogoutdelete) | **DELETE** /api/Users/Logout | logout
[**GetUserClaimsGet**](UsersApi.md#getuserclaimsget) | **GET** /GetUserClaims | Get user claims


<a name="apiusersloginpost"></a>
# **ApiUsersLoginPost**
> void ApiUsersLoginPost (User user = null)

login

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiUsersLoginPostExample
    {
        public void main()
        {
            
            var apiInstance = new UsersApi();
            var user = new User(); // User | A User object (name, password, optional claim) (optional) 

            try
            {
                // login
                apiInstance.ApiUsersLoginPost(user);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling UsersApi.ApiUsersLoginPost: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **user** | [**User**](User.md)| A User object (name, password, optional claim) | [optional] 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/json-patch+json
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apiuserslogoutdelete"></a>
# **ApiUsersLogoutDelete**
> void ApiUsersLogoutDelete (User user = null)

logout

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiUsersLogoutDeleteExample
    {
        public void main()
        {
            
            var apiInstance = new UsersApi();
            var user = new User(); // User |  (optional) 

            try
            {
                // logout
                apiInstance.ApiUsersLogoutDelete(user);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling UsersApi.ApiUsersLogoutDelete: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **user** | [**User**](User.md)|  | [optional] 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/json-patch+json
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="getuserclaimsget"></a>
# **GetUserClaimsGet**
> List<Claim> GetUserClaimsGet (string username = null)

Get user claims

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetUserClaimsGetExample
    {
        public void main()
        {
            
            var apiInstance = new UsersApi();
            var username = username_example;  // string |  (optional) 

            try
            {
                // Get user claims
                List&lt;Claim&gt; result = apiInstance.GetUserClaimsGet(username);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling UsersApi.GetUserClaimsGet: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **username** | **string**|  | [optional] 

### Return type

[**List<Claim>**](Claim.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

