// ModulusEqualsMathCondition.h : Declaration of the CModulusEqualsMathCondition

#pragma once
#include "resource.h"       // main symbols

#include "..\..\Code\FPCategories.h"
#include "ESSkipConditions.h"

/////////////////////////////////////////////////////////////////////////////
// CModulusEqualsMathCondition
class ATL_NO_VTABLE CModulusEqualsMathCondition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CModulusEqualsMathCondition, &CLSID_ModulusEqualsMathCondition>,
	public ISupportErrorInfo,
	public IDispatchImpl<IModulusEqualsMathCondition, &IID_IModulusEqualsMathCondition, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream
{
public:
	CModulusEqualsMathCondition();
	~CModulusEqualsMathCondition();


DECLARE_REGISTRY_RESOURCEID(IDR_MODULUS_EQUALS_MATH_CONDITION)


BEGIN_COM_MAP(CModulusEqualsMathCondition)
	COM_INTERFACE_ENTRY(IModulusEqualsMathCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IModulusEqualsMathCondition)
	COM_INTERFACE_ENTRY(IMathConditionChecker)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
END_COM_MAP()

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease() {};

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IMathConditionChecker
	STDMETHOD(raw_CheckCondition)(IFileRecord* pFileRecord, long lActionID, 
		VARIANT_BOOL* pbResult);

// IModulusEqualsMathConditionFAMCondition
	STDMETHOD(get_Modulus)(long* pnVal);
	STDMETHOD(put_Modulus)(long nVal);
	STDMETHOD(get_ModEquals)(long* pnVal);
	STDMETHOD(put_ModEquals)(long nVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	//////////////
	// Variables
	//////////////
	bool m_bDirty;

	// The modulus value to use
	long m_nModulus;

	// The value that the modulus operation must equal for the condition to be satisfied
	long m_nModEquals;

	/////////////
	// Methods
	/////////////
	void validateLicense();
};
