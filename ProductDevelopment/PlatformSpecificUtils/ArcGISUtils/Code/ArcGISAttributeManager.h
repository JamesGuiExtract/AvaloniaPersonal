// ArcGISAttributeManager.h : Declaration of the CArcGISAttributeManager

#pragma once

#include "resource.h"       // main symbols

#include <IConfigurationSettingsPersistenceMgr.h>

#include <string>
#include <memory>

/////////////////////////////////////////////////////////////////////////////
// CArcGISAttributeManager
class ATL_NO_VTABLE CArcGISAttributeManager : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CArcGISAttributeManager, &CLSID_ArcGISAttributeManager>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAttributeManager, &IID_IAttributeManager, &LIBID_UCLID_GISPLATINTERFACESLib>,
	public IDispatchImpl<UCLID_COMLMLib::ILicensedComponent, &UCLID_COMLMLib::IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IArcGISDependentComponent, &IID_IArcGISDependentComponent, &LIBID_UCLID_ARCGISUTILSLib>
{
public:
	CArcGISAttributeManager();
	~CArcGISAttributeManager();

DECLARE_REGISTRY_RESOURCEID(IDR_ARCGISATTRIBUTEMANAGER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CArcGISAttributeManager)
	COM_INTERFACE_ENTRY(IAttributeManager)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeManager)
	COM_INTERFACE_ENTRY(IArcGISDependentComponent)
	COM_INTERFACE_ENTRY(UCLID_COMLMLib::ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IArcGISDependentComponent
	STDMETHOD(SetApplicationHook)(IDispatch *pApp);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IAttributeManager
	STDMETHOD(raw_SetSketchSourceDocuments)(BSTR strFeatureID, IVariantVector *pColSrcDocStrings);
	STDMETHOD(raw_GetSketchSourceDocuments)(BSTR strFeatureID, IVariantVector **ppColSrcDocStrings);
	STDMETHOD(raw_SetSegmentSourceDocuments)(BSTR strSegmentID, IVariantVector *pColSrcDocStrings);
	STDMETHOD(raw_GetSegmentSourceDocuments)(BSTR strSegmentID, IVariantVector **ppColSrcDocStrings);
	STDMETHOD(raw_SetFeatureAttribute)(BSTR strFeatureID, BSTR strStoreFieldName, UCLID_FEATUREMGMTLib::IUCLDFeature *ipFeature);
	STDMETHOD(raw_GetFeatureAttribute)(BSTR strFeatureID, BSTR strStoreFieldName, UCLID_FEATUREMGMTLib::IUCLDFeature **ipFeature);
	STDMETHOD(raw_CanStoreAttributes)(BSTR strFeatureID, BSTR strStoreFieldName, VARIANT_BOOL *pbValue);

private:
	//////////////
	// Constants
	//////////////
	const static std::string ROOT_FOLDER;
	const static std::string GENERAL_FOLDER;
	const static std::string DATA_INTERPRETER_KEY;

	////////////
	// Variables
	////////////
	// persistent manager
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> m_apCfgMgr;

	IFeatureAttributeDataInterpreterPtr m_ipDataInterpreter;

	IVariantVectorPtr m_ipSourceDocCollection;

	//=============
	// ESRI objects
	//=============
	IApplicationPtr m_ipArcMapApp;
	/*esriEditor::*/IEditorPtr m_ipArcMapEditor;

	
	//////////
	// Methods
	//////////

	// Find the field that stores UCLID feature info and 
	// return the field index. If the field not found, -1 is returned
	long findStoreAttributeField(/*esriGeoDatabase::*/IFeatureClassPtr ipFeatureClass, BSTR strFieldName);

	IFeatureAttributeDataInterpreterPtr getAttributeDataInterpreter();

	// what's current task
	std::string getCurrentTaskName();

	// get currently selected one and only one feature from the focus map.
	// If there's no feature or more than one feature selected, return NULL
	/*esriGeoDatabase::*/IFeaturePtr getSelectedFeature();

	// what's the allowed length for the specified field
	long getStoreAttributeFieldLength(/*esriGeoDatabase::*/IFeatureClassPtr ipFeatureClass, BSTR strFieldName);

	// what's the current workspace description, such Shapefiles, Personal Geodatabase, etc.
	std::string getWorkspaceDescription(/*esriGeoDatabase::*/IWorkspacePtr ipWorkspace);

	void validateLicense();

	// validate objects
	void validateObjects();
};
