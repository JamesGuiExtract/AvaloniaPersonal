// ShortInputValidator.h : Declaration of the CShortInputValidator

#pragma once

#include "resource.h"       // main symbols

#import "GeneralIV.tlb"		// for access to IntegerInputValidator

/////////////////////////////////////////////////////////////////////////////
// CShortInputValidator
class ATL_NO_VTABLE CShortInputValidator : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CShortInputValidator, &CLSID_ShortInputValidator>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IShortInputValidator, &IID_IShortInputValidator, &LIBID_UCLID_GENERALIVLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IInputValidator, &IID_IInputValidator, &LIBID_UCLID_INPUTFUNNELLib>
{
public:
	CShortInputValidator();
	~CShortInputValidator();

DECLARE_REGISTRY_RESOURCEID(IDR_SHORTINPUTVALIDATOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CShortInputValidator)
	COM_INTERFACE_ENTRY(IShortInputValidator)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IShortInputValidator)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IInputValidator)
END_COM_MAP()

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// IInputValidator
	STDMETHOD(raw_ValidateInput)(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful);
	STDMETHOD(raw_GetInputType)(BSTR * pstrInputType);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// IShortInputValidator

private:

	// IntegerIV data member that handles the load
	UCLID_GENERALIVLib::IIntegerInputValidatorPtr	m_ipIIV;

	// Set member variable defaults
	void	setDefaults();

	// Validate license, throw exception if the component is not licensed
	void	validateLicense();

	// flag to keep track of whether this object has changed
	// since the last save-to-stream operation
	bool m_bDirty;
};
