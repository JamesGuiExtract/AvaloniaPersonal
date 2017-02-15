# IO.Swagger.Api.DocumentApi

All URIs are relative to *https://localhost/*

Method | HTTP request | Description
------------- | ------------- | -------------
[**ApiDocumentGetDocumentTypeGet**](DocumentApi.md#apidocumentgetdocumenttypeget) | **GET** /api/Document/GetDocumentType | Gets the type of the submitted document (document classification)
[**ApiDocumentGetFileResultExGet**](DocumentApi.md#apidocumentgetfileresultexget) | **GET** /api/Document/GetFileResultEx | Gets a result file - experimental!
[**ApiDocumentGetFileResultGet**](DocumentApi.md#apidocumentgetfileresultget) | **GET** /api/Document/GetFileResult | Gets a result file for the specified input document
[**ApiDocumentGetResultSetByIdGet**](DocumentApi.md#apidocumentgetresultsetbyidget) | **GET** /api/Document/GetResultSet/{id} | Gets result set for a submitted file that has finished processing
[**ApiDocumentGetStatusGet**](DocumentApi.md#apidocumentgetstatusget) | **GET** /api/Document/GetStatus | get a list of 1..N processing status instances that corespond to the stringId of the submitted document
[**ApiDocumentGetTextResultGet**](DocumentApi.md#apidocumentgettextresultget) | **GET** /api/Document/GetTextResult | Gets a text result for a specified input document
[**ApiDocumentSubmitFilePost**](DocumentApi.md#apidocumentsubmitfilepost) | **POST** /api/Document/SubmitFile | Upload 1 to N files for document processing
[**ApiDocumentSubmitTextPost**](DocumentApi.md#apidocumentsubmittextpost) | **POST** /api/Document/SubmitText | submit text for processing


<a name="apidocumentgetdocumenttypeget"></a>
# **ApiDocumentGetDocumentTypeGet**
> void ApiDocumentGetDocumentTypeGet (string documentId = null)

Gets the type of the submitted document (document classification)

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiDocumentGetDocumentTypeGetExample
    {
        public void main()
        {
            
            var apiInstance = new DocumentApi();
            var documentId = documentId_example;  // string |  (optional) 

            try
            {
                // Gets the type of the submitted document (document classification)
                apiInstance.ApiDocumentGetDocumentTypeGet(documentId);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DocumentApi.ApiDocumentGetDocumentTypeGet: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **documentId** | **string**|  | [optional] 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apidocumentgetfileresultexget"></a>
# **ApiDocumentGetFileResultExGet**
> void ApiDocumentGetFileResultExGet ()

Gets a result file - experimental!

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiDocumentGetFileResultExGetExample
    {
        public void main()
        {
            
            var apiInstance = new DocumentApi();

            try
            {
                // Gets a result file - experimental!
                apiInstance.ApiDocumentGetFileResultExGet();
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DocumentApi.ApiDocumentGetFileResultExGet: " + e.Message );
            }
        }
    }
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apidocumentgetfileresultget"></a>
# **ApiDocumentGetFileResultGet**
> byte[] ApiDocumentGetFileResultGet (string fileId = null)

Gets a result file for the specified input document

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiDocumentGetFileResultGetExample
    {
        public void main()
        {
            
            var apiInstance = new DocumentApi();
            var fileId = fileId_example;  // string |  (optional) 

            try
            {
                // Gets a result file for the specified input document
                byte[] result = apiInstance.ApiDocumentGetFileResultGet(fileId);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DocumentApi.ApiDocumentGetFileResultGet: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **fileId** | **string**|  | [optional] 

### Return type

**byte[]**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apidocumentgetresultsetbyidget"></a>
# **ApiDocumentGetResultSetByIdGet**
> DocumentAttributeSet ApiDocumentGetResultSetByIdGet (string id)

Gets result set for a submitted file that has finished processing

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiDocumentGetResultSetByIdGetExample
    {
        public void main()
        {
            
            var apiInstance = new DocumentApi();
            var id = id_example;  // string | 

            try
            {
                // Gets result set for a submitted file that has finished processing
                DocumentAttributeSet result = apiInstance.ApiDocumentGetResultSetByIdGet(id);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DocumentApi.ApiDocumentGetResultSetByIdGet: " + e.Message );
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

[**DocumentAttributeSet**](DocumentAttributeSet.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apidocumentgetstatusget"></a>
# **ApiDocumentGetStatusGet**
> List<ProcessingStatus> ApiDocumentGetStatusGet (string stringId = null)

get a list of 1..N processing status instances that corespond to the stringId of the submitted document

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiDocumentGetStatusGetExample
    {
        public void main()
        {
            
            var apiInstance = new DocumentApi();
            var stringId = stringId_example;  // string |  (optional) 

            try
            {
                // get a list of 1..N processing status instances that corespond to the stringId of the submitted document
                List&lt;ProcessingStatus&gt; result = apiInstance.ApiDocumentGetStatusGet(stringId);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DocumentApi.ApiDocumentGetStatusGet: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **stringId** | **string**|  | [optional] 

### Return type

[**List<ProcessingStatus>**](ProcessingStatus.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apidocumentgettextresultget"></a>
# **ApiDocumentGetTextResultGet**
> byte[] ApiDocumentGetTextResultGet (string textId = null)

Gets a text result for a specified input document

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiDocumentGetTextResultGetExample
    {
        public void main()
        {
            
            var apiInstance = new DocumentApi();
            var textId = textId_example;  // string |  (optional) 

            try
            {
                // Gets a text result for a specified input document
                byte[] result = apiInstance.ApiDocumentGetTextResultGet(textId);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DocumentApi.ApiDocumentGetTextResultGet: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **textId** | **string**|  | [optional] 

### Return type

**byte[]**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apidocumentsubmitfilepost"></a>
# **ApiDocumentSubmitFilePost**
> void ApiDocumentSubmitFilePost ()

Upload 1 to N files for document processing

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiDocumentSubmitFilePostExample
    {
        public void main()
        {
            
            var apiInstance = new DocumentApi();

            try
            {
                // Upload 1 to N files for document processing
                apiInstance.ApiDocumentSubmitFilePost();
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DocumentApi.ApiDocumentSubmitFilePost: " + e.Message );
            }
        }
    }
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a name="apidocumentsubmittextpost"></a>
# **ApiDocumentSubmitTextPost**
> void ApiDocumentSubmitTextPost (SubmitTextArgs args = null)

submit text for processing

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ApiDocumentSubmitTextPostExample
    {
        public void main()
        {
            
            var apiInstance = new DocumentApi();
            var args = new SubmitTextArgs(); // SubmitTextArgs |  (optional) 

            try
            {
                // submit text for processing
                apiInstance.ApiDocumentSubmitTextPost(args);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling DocumentApi.ApiDocumentSubmitTextPost: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **args** | [**SubmitTextArgs**](SubmitTextArgs.md)|  | [optional] 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/json-patch+json
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

