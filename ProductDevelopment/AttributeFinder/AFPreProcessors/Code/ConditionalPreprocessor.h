// ConditionalPreprocessor.h : Declaration of the CConditionalPreprocessor
// ConditionalPreprocessor.h : Declaration of the CConditionalPreprocessor

#ifndef __CONDITIONALPREPROCESSOR_H_
#define __CONDITIONALPREPROCESSOR_H_

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include <string>
/////////////////////////////////////////////////////////////////////////////
// CConditionalPreprocessor
class ATL_NO_VTABLE CConditionalPreprocessor : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CConditionalPreprocessor, &CLSID_ConditionalPreprocessor>,
	public IPersistStream,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<CConditionalPreprocessor>,
	public IDispatchImpl<IDocumentPreprocessor, &IID_IDocumentPreprocessor, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IConditionalRule, &IID_IConditionalRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CConditionalPreprocessor();
	~CConditionalPreprocessor();

DECLARE_REGISTRY_RESOURCEID(IDR_CONDITIONALPREPROCESSOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CConditionalPreprocessor)
	COM_INTERFACE_ENTRY(IConditionalRule)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IDocumentPreprocessor)
	COM_INTERFACE_ENTRY(IDocumentPreprocessor)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()


BEGIN_PROP_MAP(CConditionalPreprocessor)
	PROP_PAGE(CLSID_ConditionalRulePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CConditionalPreprocessor)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_DOCUMENT_PREPROCESSORS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IConditionalRule
	STDMETHOD(raw_GetRuleIID)(IID* pIID);
	STDMETHOD(raw_GetCondition)(IAFCondition** ppCondition);
	STDMETHOD(raw_SetCondition)(IAFCondition* pCondition);
	STDMETHOD(raw_GetRule)(IUnknown** ppRule);
	STDMETHOD(raw_SetRule)(IUnknown* pRule);
	STDMETHOD(get_InvertCondition)(VARIANT_BOOL * pbRetVal);
	STDMETHOD(put_InvertCondition)(VARIANT_BOOL bNewVal);
	STDMETHOD(raw_GetCategoryName)(BSTR *pstrCategoryName);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IDocumentPreprocessor
	STDMETHOD(raw_Process)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);
public:

	/////////////////
	// Variables
	/////////////////
	// the condition that will be checked
	IAFConditionPtr m_ipCondition;

	// The rule that will execute if the condition is met
	IDocumentPreprocessorPtr m_ipRule;

	bool m_bInvertCondition;

	std::string m_strCategoryName;

	bool m_bDirty;

	void validateLicense();
};

#endif //__CONDITIONALPREPROCESSOR_H_