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
	STDMETHOD(get_FeedbackManager)(/*[out, retval]*/ IFeedbackMgr **pVal);
	STDMETHOD(ShowHelpAboutBox)(/*[in]*/ EHelpAboutType eType, /*[in]*/ BSTR strProductVersion);
	STDMETHOD(GetComponentDataFolder)(/*[out, retval]*/ BSTR *pstrComponentDataFolder);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);
private:
	//////////////
	// Variables
	//////////////
	IOCREnginePtr m_ipOCREngine;

	IOCRUtilsPtr m_ipOCRUtils;

	// Feedback objects
	UCLID_AFCORELib::IFeedbackMgrPtr m_ipFeedbackMgr;
	UCLID_AFCORELib::IFeedbackMgrInternalsPtr	m_ipInternals;

	// Handles registry settings
	auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;

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
	void getComponentDataFolder(string& rFolder);
	//----------------------------------------------------------------------------------------------
	UCLID_AFCORELib::IFeedbackMgrPtr getFeedbackManager();
	//----------------------------------------------------------------------------------------------
	UCLID_AFCORELib::IFeedbackMgrInternalsPtr getInternals();
	//----------------------------------------------------------------------------------------------
	void validateLicense();
};
