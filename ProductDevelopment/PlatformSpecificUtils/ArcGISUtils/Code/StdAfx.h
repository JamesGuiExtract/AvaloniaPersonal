#pragma once

#define STRICT
#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0400
#endif
#define _ATL_APARTMENT_THREADED
//#define EXT_CLASS _declspec(dllexport)

//#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

#include <afxcoll.h>
#include <ocidl.h>

#include <afxwin.h>
#include <afxdisp.h>
#include <atlbase.h>

//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>

//  raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriSystem.olb"                  raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE", "IStream", \
	"ISequentialStream", "_LARGE_INTEGER", "_ULARGE_INTEGER", "tagSTATSTG", "_FILETIME", "IPersist", "IPersistStream", "ISupportErrorInfo") \
	rename("GetObject", "GetESRIObject") \
	rename("min", "ESRImin") \
	rename("max", "ESRImax")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriSystemUI.olb"                raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE", "IProgressDialog") \
	rename("ICommand", "IESRCommand") 
//	rename("GetItemInfo", "raw_GetItemInfo")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeometry.olb"                raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE", "IClassFactory") \
	rename("ISegment", "IESRSegment") \
	rename("ISegmentCollection", "IESRSegmentCollection")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDisplay.olb"                 raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE", "tagRECT", \
	"tagPOINT", "IConnectionPointContainer", "IEnumConnectionPoints", "IConnectionPoint", "IEnumConnections", "tagCONNECTDATA") \
	rename("RGB", "ESRI_RGB") \
	rename("CMYK", "ESRI_CMYK") \
	rename("ResetDC", "ESRI_ResetDC") \
	rename("DrawText", "ESRI_DrawText")

//#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriServer.olb"                  raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")
#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriOutput.olb"                  raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeoDatabase.olb"             raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE", "IPersistStreamInit", "ICursor") \
	rename("IRow", "IESRROW") \
	rename("GetMessage", "ESRI_GetMessage") 

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGISClient.olb"               raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDataSourcesFile.olb"         raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDataSourcesGDB.olb"          raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDataSourcesOleDB.olb"        raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDataSourcesRaster.olb"       raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeoDatabaseDistributed.olb"  raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriCarto.olb"                   raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE", "ITableDefinition") \
	rename("UINT_PTR", "ESRI_UINT_PTR")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriFramework.olb"                   raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE", \
	"IPropertyPageSite", "tagMSG", "wireHWND", "_RemotableHandle", "__MIDL_IWinTypes_0009", "IPropertyPage", "tagPROPPAGEINFO", "tagSIZE") \
	rename("UINT_PTR", "ESRI_UINT_PTR") \

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeoDatabaseUI.olb"                   raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDisplayUI.olb"                   raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriOutputUI.olb"                   raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriCartoUI.olb"                   raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDataSourcesRasterUI.olb"                   raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriArcMapUI.olb"                   raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriEditor.olb"                   raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriLocation.olb"                raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriNetworkAnalysis.olb"         raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE") \
	rename("IStringPair", "IESRIStringPair") \
	rename("StringPair", "ESRIStringPair")


//#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriNetworkAnalyst.olb"          raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

//#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeoAnalyst.olb"              raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

//#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esri3DAnalyst.olb"               raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

//#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGlobeCore.olb"               raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

//#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriSpatialAnalyst.olb"          raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

//#import"..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeoStatisticalAnalyst.olb"   raw_native_types no_namespace named_guids exclude("OLE_COLOR", "OLE_HANDLE", "ICursorPtr", "VARTYPE")

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids 
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\InputFunnel\IFCore\Code\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\..\InputFunnel\InputReceivers\SpotRecognitionIR\Code\Core\SpotRecognitionIR.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_SPOTRECOGNITIONIRLib;

#import "..\..\..\InputFunnel\InputReceivers\HighlightedTextIR\Code\Core\HighlightedTextIR.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_HIGHLIGHTEDTEXTIRLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDDistanceConverter\Code\UCLIDDistanceConverter.tlb"
using namespace UCLID_DISTANCECONVERTERLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCurveParameter\Code\UCLIDCurveParameter.tlb" named_guids 
using namespace UCLID_CURVEPARAMETERLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids 
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDMeasurements\Code\UCLIDMeasurements.tlb" named_guids 
using namespace UCLID_MEASUREMENTSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDFeatureMgmt\Code\UCLIDFeatureMgmt.tlb"
using namespace UCLID_FEATUREMGMTLib;

#import "..\..\GISPlatInterfaces\Code\GISPlatInterfaces.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_GISPLATINTERFACESLib;


/* API REDEFINITIONS */

//#define AoInitialize   CoInitialize
//#define AoUninitialize CoUninitialize
//
//#define AoCreateObject CoCreateInstance
//
//#define AoAllocBSTR    SysAllocString
//#define AoFreeBSTR     SysFreeString
//
//#define AoExit         exit

//#endif /* __ESRI__ARCSDK_COM_SDK_WINDOWS_H__ */

