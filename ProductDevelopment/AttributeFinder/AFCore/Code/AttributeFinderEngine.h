// AttributeFinderEngine.h : Declaration of the CAttributeFinderEngine

#pragma once

#include "resource.h"       // main symbols

#include <IConfigurationSettingsPersistenceMgr.h>
#include <memory>
#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CAttributeFinderEngine
class ATL_NO_VTABLE CAttributeFinderEngine : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAttributeFinderEngine, &CLSID_AttributeFinderEngine>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAttributeFinderEngine, &IID_IAttributeFinderEngine, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CAttributeFinderEngine();
	~CAttributeFinderEngine();

DECLARE_REGISTRY_RESOURCEID(IDR_ATTRIBUTEFINDERENGINE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAttributeFinderEngine)
	COM_INTERFACE_ENTRY(IAttributeFinderEngine)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeFinderEngine)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttributeFinderEngine
	// Previous methods replaced by generalized FindAttributes() method 06/22/04 - WEL
	STDMETHOD(FindAttributes)(/*[in]*/ IAFDocument *pDoc,
							  /*[in]*/ BSTR strSrcDocFileName,
							  /*[in]*/ long nNumOfPagesToRecognize,
							  /*[in]*/ VARIANT varRuleSet,
							  /*[in]*/ IVariantVector* pvecAttributeNames,
							  /*[in]*/ VARIANT_BOOL vbUseAFDocText,
							  /*[in]*/ IProgressStatus *pProgressStatus,
							  /*[out, retval]*/ IIUnknownVector** pAttributes);
	STDMETHOD(ShowHelpAboutBox)(/*[in]*/ EHelpAboutType eType, /*[in]*/ BSTR strProductVersion);
	STDMETHOD(GetComponentDataFolder)(/*[out, retval]*/ BSTR *pstrComponentDataFolder);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// If there is a "legacy" FKB installation (installed to root of ComponentData rather than under
	// a version-specific folder), returns the version via the FKBVersion text file or empty if
	// there is no legacy FKB installation.
	static string getLegacyFKBVersion();

	// Calculates the root (not FKB version specific) component data folder. rbOverridden indicates
	// if the path has been overridden via registry key.
	static void getRootComponentDataFolder(string& rstrFolder, bool& rbOverridden);

private:
	//////////////
	// Variables
	//////////////
	IOCREnginePtr m_ipOCREngine;

	IOCRUtilsPtr m_ipOCRUtils;

	// Handles registry settings
	unique_ptr<IConfigurationSettingsPersistenceMgr> mu_pUserCfgMgr;

	// Used to get the FKBVersion for getComponentDataFolder
	UCLID_AFCORELib::IRuleExecutionEnvPtr m_ipRuleExecutionEnv;

	// Handles registry calls for the static getRootComponentDataFolder method.
	static unique_ptr<IConfigurationSettingsPersistenceMgr> mu_spUserCfgMgr;

	// Persists any value calculated by getLegacyFKBVersion()
	static string ms_strLegacyFKBVersion;

	static CMutex m_mutex;

	//////////////
	// Methods
	//////////////
	//----------------------------------------------------------------------------------------------
	IIUnknownVectorPtr findAttributesInText(const UCLID_AFCORELib::IAFDocumentPtr& ipAFDoc,
		const UCLID_AFCORELib::IRuleSetPtr& ipRuleSet, const IVariantVectorPtr& ipvecAttributeNames,
		const IProgressStatusPtr& ipProgressStatus);
	//----------------------------------------------------------------------------------------------
	IOCREnginePtr getOCREngine();
	//----------------------------------------------------------------------------------------------
	IOCRUtilsPtr getOCRUtils();
	//----------------------------------------------------------------------------------------------
	// Gets the FKB version specific (or registry overridden) component data folder
	void getComponentDataFolder(string& rFolder);
	//----------------------------------------------------------------------------------------------
	void validateLicense();
};
