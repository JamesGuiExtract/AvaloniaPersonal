# IO.Swagger.Api.TestApi

All URIs are relative to *https://localhost/*

Method | HTTP request | Description
------------- | ------------- | -------------
[**ApiTestSetAttributeSetNamePost**](TestApi.md#apitestsetattributesetnamepost) | **POST** /api/Test/SetAttributeSetName | 


<a name="apitestsetattributesetnamepost"></a>
# **ApiTestSetAttributeSetNamePost**
> void ApiTestSetAttributeSetNamePost (TestArgs arg = null)



### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiTestSetAttributeSetNamePostExample
    {
        public void main()
        {
            
            var apiInstance = new TestApi();
            var arg = new TestArgs(); // TestArgs |  (optional) 

            try
            {
                apiInstance.ApiTestSetAttributeSetNamePost(arg);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling TestApi.ApiTestSetAttributeSetNamePost: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **arg** | [**TestArgs**](TestArgs.md)|  | [optional] 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/json-patch+json
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

