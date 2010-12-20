// AFEngineFileProcessor.h : Declaration of the CAFEngineFileProcessor

#pragma once

#include "resource.h"       // main symbols
#include <string>
#include <CachedObjectFromFile.h>

#include "..\..\..\..\ReusableComponents\COMComponents\UCLIDFileProcessing\Code\FPCategories.h"
#include "..\..\AFCore\Code\RuleSetLoader.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CAFEngineFileProcessor
class ATL_NO_VTABLE CAFEngineFileProcessor : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAFEngineFileProcessor, &CLSID_AFEngineFileProcessor>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IAFEngineFileProcessor, &IID_IAFEngineFileProcessor, &LIBID_UCLID_AFFILEPROCESSORSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CAFEngineFileProcessor>
{
public:
	CAFEngineFileProcessor();
	~CAFEngineFileProcessor();

DECLARE_REGISTRY_RESOURCEID(IDR_AFENGINEFILEPROCESSOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAFEngineFileProcessor)
	COM_INTERFACE_ENTRY(IAFEngineFileProcessor)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY2(IDispatch, IAFEngineFileProcessor)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CAFEngineFileProcessor)
	PROP_PAGE(CLSID_AFEngineFileProcessorPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CAttributeFinderEngine)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAFEngineFileProcessor
	STDMETHOD(get_ReadUSSFile)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ReadUSSFile)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_CreateUSSFile)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_CreateUSSFile)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_RuleSetFileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_RuleSetFileName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_OCRPagesType)(/*[out, retval]*/ EOCRPagesType *pVal);
	STDMETHOD(put_OCRPagesType)(/*[in]*/ EOCRPagesType newVal);
	STDMETHOD(get_OCRCertainPages)(/*[out, retval]*/ BSTR *strSpecificPages);
	STDMETHOD(put_OCRCertainPages)(/*[in]*/ BSTR strSpecificPages);
	STDMETHOD(get_UseCleanedImage)(VARIANT_BOOL* pVal);
	STDMETHOD(put_UseCleanedImage)(VARIANT_BOOL newVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IFileProcessingTask
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB);
	STDMETHOD(raw_ProcessFile)(BSTR bstrFileFullName, long nFileID, long nActionID,
		IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	/////////////
	// Variables
	/////////////
	
	// this is the rule set file name for file processing only
	string m_strRuleFileNameForFileProcessing;

	// read the uss file if exist
	bool m_bReadUSSFileIfExist;

	// create uss file if it doesn't exist
	bool m_bCreateUssFileIfNonExist;

	EOCRPagesType m_eOCRPagesType;

	string m_strSpecificPages;

	IOCRUtilsPtr m_ipOCRUtils;
	IOCREnginePtr m_ipOCREngine;
	IAttributeFinderEnginePtr m_ipAFEngine;

	CachedObjectFromFile<IRuleSetPtr, RuleSetLoader> m_ipRuleSet;

	bool m_bUseCleanedImage;

	bool m_bDirty;

	/////////////
	// Methods
	/////////////
	// reset member variables
	void clear();

	void validateLicense();

	// Returns m_ipRuleSet, after initializing it if necessary
	// The ruleset will also be loaded using the m_strRuleFileName if it hasn't been already
	IRuleSetPtr getRuleSet(const string& strRulesFile);

	IOCREnginePtr getOCREngine();
	IOCRUtilsPtr getOCRUtils();
	IAttributeFinderEnginePtr getAFEngine();
};
