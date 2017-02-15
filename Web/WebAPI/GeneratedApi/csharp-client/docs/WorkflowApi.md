# IO.Swagger.Api.WorkflowApi

All URIs are relative to *https://localhost/*

Method | HTTP request | Description
------------- | ------------- | -------------
[**ApiWorkflowByIdDelete**](WorkflowApi.md#apiworkflowbyiddelete) | **DELETE** /api/Workflow/{id} | Delete handler - probably not needed - unless we support management of workflows via this api
[**ApiWorkflowByIdPut**](WorkflowApi.md#apiworkflowbyidput) | **PUT** /api/Workflow/{id} | Put handler - probably not needed (update) - unless we support management of workflows via this api
[**ApiWorkflowGetDefaultWorkflowByUsernameGet**](WorkflowApi.md#apiworkflowgetdefaultworkflowbyusernameget) | **GET** /api/Workflow/GetDefaultWorkflow/{username} | get default workflow for the specified user
[**ApiWorkflowGetWorkflowStatusByWorkflowNameGet**](WorkflowApi.md#apiworkflowgetworkflowstatusbyworkflownameget) | **GET** /api/Workflow/GetWorkflowStatus/{workflowName} | get status of specified workflow
[**ApiWorkflowGetWorkflowsGet**](WorkflowApi.md#apiworkflowgetworkflowsget) | **GET** /api/Workflow/GetWorkflows | GET handler - returns a list of workflow names
[**ApiWorkflowPost**](WorkflowApi.md#apiworkflowpost) | **POST** /api/Workflow | management only - Post handler for workflow, creates a new workflow


<a name="apiworkflowbyiddelete"></a>
# **ApiWorkflowByIdDelete**
> void ApiWorkflowByIdDelete (int? id)

Delete handler - probably not needed - unless we support management of workflows via this api

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiWorkflowByIdDeleteExample
    {
        public void main()
        {
            
            var apiInstance = new WorkflowApi();
            var id = 56;  // int? | 

            try
            {
                // Delete handler - probably not needed - unless we support management of workflows via this api
                apiInstance.ApiWorkflowByIdDelete(id);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling WorkflowApi.ApiWorkflowByIdDelete: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **int?**|  | 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apiworkflowbyidput"></a>
# **ApiWorkflowByIdPut**
> void ApiWorkflowByIdPut (int? id, string value = null)

Put handler - probably not needed (update) - unless we support management of workflows via this api

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiWorkflowByIdPutExample
    {
        public void main()
        {
            
            var apiInstance = new WorkflowApi();
            var id = 56;  // int? | 
            var value = value_example;  // string |  (optional) 

            try
            {
                // Put handler - probably not needed (update) - unless we support management of workflows via this api
                apiInstance.ApiWorkflowByIdPut(id, value);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling WorkflowApi.ApiWorkflowByIdPut: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **int?**|  | 
 **value** | **string**|  | [optional] 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/json-patch+json
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apiworkflowgetdefaultworkflowbyusernameget"></a>
# **ApiWorkflowGetDefaultWorkflowByUsernameGet**
> void ApiWorkflowGetDefaultWorkflowByUsernameGet (string username)

get default workflow for the specified user

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiWorkflowGetDefaultWorkflowByUsernameGetExample
    {
        public void main()
        {
            
            var apiInstance = new WorkflowApi();
            var username = username_example;  // string | 

            try
            {
                // get default workflow for the specified user
                apiInstance.ApiWorkflowGetDefaultWorkflowByUsernameGet(username);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling WorkflowApi.ApiWorkflowGetDefaultWorkflowByUsernameGet: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **username** | **string**|  | 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apiworkflowgetworkflowstatusbyworkflownameget"></a>
# **ApiWorkflowGetWorkflowStatusByWorkflowNameGet**
> WorkflowStatus ApiWorkflowGetWorkflowStatusByWorkflowNameGet (string workflowName)

get status of specified workflow

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiWorkflowGetWorkflowStatusByWorkflowNameGetExample
    {
        public void main()
        {
            
            var apiInstance = new WorkflowApi();
            var workflowName = workflowName_example;  // string | 

            try
            {
                // get status of specified workflow
                WorkflowStatus result = apiInstance.ApiWorkflowGetWorkflowStatusByWorkflowNameGet(workflowName);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling WorkflowApi.ApiWorkflowGetWorkflowStatusByWorkflowNameGet: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **workflowName** | **string**|  | 

### Return type

[**WorkflowStatus**](WorkflowStatus.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apiworkflowgetworkflowsget"></a>
# **ApiWorkflowGetWorkflowsGet**
> List<string> ApiWorkflowGetWorkflowsGet ()

GET handler - returns a list of workflow names

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiWorkflowGetWorkflowsGetExample
    {
        public void main()
        {
            
            var apiInstance = new WorkflowApi();

            try
            {
                // GET handler - returns a list of workflow names
                List&lt;string&gt; result = apiInstance.ApiWorkflowGetWorkflowsGet();
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling WorkflowApi.ApiWorkflowGetWorkflowsGet: " + e.Message );
            }
        }
    }
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

**List<string>**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apiworkflowpost"></a>
# **ApiWorkflowPost**
> void ApiWorkflowPost (Workflow workflow = null)

management only - Post handler for workflow, creates a new workflow

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiWorkflowPostExample
    {
        public void main()
        {
            
            var apiInstance = new WorkflowApi();
            var workflow = new Workflow(); // Workflow |  (optional) 

            try
            {
                // management only - Post handler for workflow, creates a new workflow
                apiInstance.ApiWorkflowPost(workflow);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling WorkflowApi.ApiWorkflowPost: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **workflow** | [**Workflow**](Workflow.md)|  | [optional] 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/json-patch+json
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

