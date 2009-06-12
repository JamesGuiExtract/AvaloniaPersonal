// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#include <CommonToExtractProducts.h>

#define STRICT
#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0400
#endif
#define _ATL_APARTMENT_THREADED

#pragma warning(disable : 4786)
#pragma warning(disable : 4049)

#include <afxwin.h>
#include <afxdisp.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>

#pragma warning(push)
#pragma warning(disable : 4146)
#pragma warning(disable : 4192)

#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriSystem.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("GetObject", "GetESRIObject") \
	rename("min", "ESRImin") \
	rename("max", "ESRImax") \
//using namespace esriSystem;

#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriSystemUI.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("ICommand", "IESRICommand") \
	rename("IProgressDialog", "IESRIProgressDialog" )
//using namespace esriSystemUI;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeometry.olb" raw_native_types named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
using namespace esriGeometry;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDisplay.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("RGB", "ESRI_RGB") \
	rename("CMYK", "ESRI_CMYK") \
	rename("ResetDC", "ESRI_ResetDC") \
	rename("DrawText", "ESRI_DrawText" )

//using namespace esriDisplay;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriServer.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("GetObject", "GetESRIObject")

//using namespace esriServer;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriOutput.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriOutput;

#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeoDatabase.olb" raw_native_types named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("IRow", "IESRIRow") \
	rename("GetMessage", "GetESRIMessage") \
	rename("ICursor", "IESRICursor")
using namespace esriGeoDatabase;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGISClient.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriGISClient;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDataSourcesFile.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
//using namespace esriDataSourcesFile;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDataSourcesGDB.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriDataSourcesGDB;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDataSourcesOleDB.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriDataSourcesOleDB;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDataSourcesRaster.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriDataSourcesRaster;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeoDatabaseDistributed.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriGeoDatabaseDistributed;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriCarto.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("ITableDefinition", "IESRITableDefinition") \
	rename("IRow", "IESRIRow") \
	rename("ICursor", "IESRICursor") \
	rename("UINT_PTR", "ESRI_UINT_PTR")

//using namespace esriCarto;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriLocation.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriLocation;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriNetworkAnalysis.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
//using namespace esriNetworkAnalysis;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriControls.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriControls;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeoAnalyst.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriGeoAnalyst;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esri3DAnalyst.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esri3DAnalyst;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGlobeCore.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriGlobeCore;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriSpatialAnalyst.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriSpatialAnalyst;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriFramework.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("UINT_PTR", "ESRI_UINT_PTR")

//using namespace esriFramework;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriGeoDatabaseUI.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("IRow", "IESRIRow") \
	rename("ICursor", "IESRICursor")
//using namespace esriGeoDatabaseUI;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDisplayUI.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriDisplayUI;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriOutputUI.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriOutputUI;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriCatalog.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriCatalog;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriCatalogUI.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriCatalogUI;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriCartoUI.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriCartoUI;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriDataSourcesRasterUI.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriDataSourcesRasterUI;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriArcCatalogUI.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriArcCatalogUI;
//#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriArcCatalog.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriArcCatalog;
#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriArcMapUI.olb" raw_native_types no_namespace named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") 
//using namespace esriArcMapUI;

#import "..\..\..\..\ReusableComponents\APIs\ArcGIS\Bin\esriEditor.olb" raw_native_types named_guids exclude("OLE_HANDLE", "OLE_COLOR"), raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("IRow", "IESRIRow") 
using namespace esriEditor;


#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;


#import "GridGenerator.tlb"

#pragma warning(pop)

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

