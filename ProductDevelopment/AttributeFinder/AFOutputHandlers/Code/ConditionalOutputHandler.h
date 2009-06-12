// ConditionalOutputHandler.h : Declaration of the CConditionalOutputHandler

#ifndef __CONDITIONALOUTPUTHANDLER_H_
#define __CONDITIONALOUTPUTHANDLER_H_

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include <string>
/////////////////////////////////////////////////////////////////////////////
// CConditionalOutputHandler
class ATL_NO_VTABLE CConditionalOutputHandler : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CConditionalOutputHandler, &CLSID_ConditionalOutputHandler>,
	public IPersistStream,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<CConditionalOutputHandler>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IConditionalRule, &IID_IConditionalRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CConditionalOutputHandler();
	~CConditionalOutputHandler();

DECLARE_REGISTRY_RESOURCEID(IDR_CONDITIONALOUTPUTHANDLER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CConditionalOutputHandler)
	COM_INTERFACE_ENTRY(IConditionalRule)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IOutputHandler)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()


BEGIN_PROP_MAP(CConditionalOutputHandler)
	PROP_PAGE(CLSID_ConditionalRulePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CConditionalOutputHandler)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
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

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

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
	IOutputHandlerPtr m_ipRule;

	bool m_bInvertCondition;

	std::string m_strCategoryName;

	bool m_bDirty;

	void validateLicense();
};

#endif //__CONDITIONALOUTPUTHANDLER_H_
