// LimitAsRightPart.h : Declaration of the CLimitAsRightPart

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

/////////////////////////////////////////////////////////////////////////////
// CLimitAsRightPart
class ATL_NO_VTABLE CLimitAsRightPart : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLimitAsRightPart, &CLSID_LimitAsRightPart>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILimitAsRightPart, &IID_ILimitAsRightPart, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CLimitAsRightPart>
{
public:
	CLimitAsRightPart();
	~CLimitAsRightPart();
	
DECLARE_REGISTRY_RESOURCEID(IDR_LIMITASRIGHTPART)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLimitAsRightPart)
	COM_INTERFACE_ENTRY(ILimitAsRightPart)
	COM_INTERFACE_ENTRY2(IDispatch, ILimitAsRightPart)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CLimitAsRightPart)
	PROP_PAGE(CLSID_LimitAsRightPartPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CLimitAsRightPart)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILimitAsRightPart
	STDMETHOD(get_AcceptSmallerLength)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_AcceptSmallerLength)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_NumberOfCharacters)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_NumberOfCharacters)(/*[in]*/ long newVal);
	STDMETHOD(get_Extract)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_Extract)(/*[in]*/ VARIANT_BOOL newVal);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute* pAttribute, IAFDocument* pOriginInput,
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

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
	long	m_nNumOfChars;
	bool	m_bAcceptSmallerLength;

	// Characters are Extracted from the string instead of Removed
	bool	m_bExtract;

	bool	m_bDirty;

	//////////////
	// Methods
	//////////////
	void validateLicense();
};

