// RemoveSubAttributes.h : Declaration of the CRemoveSubAttributes

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
using std::string;

/////////////////////////////////////////////////////////////////////////////
// CRemoveSubAttributes
class ATL_NO_VTABLE CRemoveSubAttributes : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRemoveSubAttributes, &CLSID_RemoveSubAttributes>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IRemoveSubAttributes, &IID_IRemoveSubAttributes, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CRemoveSubAttributes>
{
public:
	CRemoveSubAttributes();

DECLARE_REGISTRY_RESOURCEID(IDR_REMOVESUBATTRIBUTES)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRemoveSubAttributes)
	COM_INTERFACE_ENTRY(IRemoveSubAttributes)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IRemoveSubAttributes)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CRemoveSubAttributes)
	PROP_PAGE(CLSID_RemoveSubAttributesPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CRemoveSubAttributes)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRemoveSubAttributes
	STDMETHOD(get_DataScorer)(/*[out, retval]*/ IObjectWithDescription** pVal);
	STDMETHOD(put_DataScorer)(/*[in]*/ IObjectWithDescription* newVal);
	STDMETHOD(get_ScoreCondition)(/*[out, retval]*/ EConditionalOp* pVal);
	STDMETHOD(put_ScoreCondition)(/*[in]*/ EConditionalOp newVal);
	STDMETHOD(get_ScoreToCompare)(/*[out, retval]*/ long* pVal);
	STDMETHOD(put_ScoreToCompare)(/*[in]*/ long newVal);
	STDMETHOD(get_ConditionalRemove)(/*[out, retval]*/ VARIANT_BOOL* pVal);
	STDMETHOD(put_ConditionalRemove)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_AttributeSelector)(IAttributeSelector ** pVal);
	STDMETHOD(put_AttributeSelector)(IAttributeSelector * newVal);
	
// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector *pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	//----------------------------------------------------------------------------------------------
	// Variables
	//----------------------------------------------------------------------------------------------
	IObjectWithDescriptionPtr m_ipDataScorer;
	EConditionalOp m_eCondition;
	long m_nScoreToCompare;

	bool m_bConditionalRemove;
	
	IAFUtilityPtr m_ipAFUtility;

	// Pointer to the attribute Selector
	IAttributeSelectorPtr m_ipAS;

	// Dirty Flag
	bool m_bDirty;

	//----------------------------------------------------------------------------------------------
	// Methods
	//----------------------------------------------------------------------------------------------
	UCLID_AFOUTPUTHANDLERSLib::IRemoveSubAttributesPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	void validateLicense();
};

