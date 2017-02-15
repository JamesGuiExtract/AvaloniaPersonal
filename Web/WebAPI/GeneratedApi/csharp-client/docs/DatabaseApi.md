# IO.Swagger.Api.DatabaseApi

All URIs are relative to *https://localhost/*

Method | HTTP request | Description
------------- | ------------- | -------------
[**ApiDatabaseSetDatabaseNameByIdPost**](DatabaseApi.md#apidatabasesetdatabasenamebyidpost) | **POST** /api/Database/SetDatabaseName/{id} | Set the database name, supported for testing only
[**ApiDatabaseSetDatabaseServerByIdPost**](DatabaseApi.md#apidatabasesetdatabaseserverbyidpost) | **POST** /api/Database/SetDatabaseServer/{id} | Set the database server, supported for testing only


<a name="apidatabasesetdatabasenamebyidpost"></a>
# **ApiDatabaseSetDatabaseNameByIdPost**
> void ApiDatabaseSetDatabaseNameByIdPost (string id)

Set the database name, supported for testing only

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiDatabaseSetDatabaseNameByIdPostExample
    {
        public void main()
        {
            
            var apiInstance = new DatabaseApi();
            var id = id_example;  // string | 

            try
            {
                // Set the database name, supported for testing only
                apiInstance.ApiDatabaseSetDatabaseNameByIdPost(id);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DatabaseApi.ApiDatabaseSetDatabaseNameByIdPost: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **string**|  | 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apidatabasesetdatabaseserverbyidpost"></a>
# **ApiDatabaseSetDatabaseServerByIdPost**
> void ApiDatabaseSetDatabaseServerByIdPost (string id)

Set the database server, supported for testing only

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiDatabaseSetDatabaseServerByIdPostExample
    {
        public void main()
        {
            
            var apiInstance = new DatabaseApi();
            var id = id_example;  // string | 

            try
            {
                // Set the database server, supported for testing only
                apiInstance.ApiDatabaseSetDatabaseServerByIdPost(id);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DatabaseApi.ApiDatabaseSetDatabaseServerByIdPost: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **string**|  | 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

