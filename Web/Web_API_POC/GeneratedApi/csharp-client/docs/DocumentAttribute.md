# IO.Swagger.Model.DocumentAttribute
## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Name** | **string** | Name of the attribute | [optional] 
**Value** | **string** | Value of the attribute | [optional] 
**Type** | **string** | The assigned type of the attribute | [optional] 
**AverageCharacterConfidence** | **int?** | The average OCR recognition confidence of each character value in the defined attribute | [optional] 
**ConfidenceLevel** | **string** | The confidence level of the redaction,  based on ConfidenceLevel enumeration, expressed as a string name. | [optional] 
**HasPositionInfo** | **bool?** | Some attributes do not have position info - in that case this will be false and the LineInfo  members will be empty. | [optional] 
**SpatialPosition** | [**Position**](Position.md) | The spatial position information of the attribute, inculding the page number, bounding rect, and zonal information (bounds plus skew) | [optional] 
**ChildAttributes** | [**List&lt;DocumentAttribute&gt;**](DocumentAttribute.md) | child attributes, 0..N | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)

