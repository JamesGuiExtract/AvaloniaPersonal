// RandomMathCondition.h : Declaration of the CRandomMathCondition

#pragma once
#include "resource.h"       // main symbols

#include "..\..\Code\FPCategories.h"
#include "ESSkipConditions.h"

/////////////////////////////////////////////////////////////////////////////
// CRandomMathCondition
class ATL_NO_VTABLE CRandomMathCondition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRandomMathCondition, &CLSID_RandomMathCondition>,
	public ISupportErrorInfo,
	public IDispatchImpl<IRandomMathCondition, &IID_IRandomMathCondition, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream
{
public:
	CRandomMathCondition();
	~CRandomMathCondition();


DECLARE_REGISTRY_RESOURCEID(IDR_RANDOM_MATH_CONDITION)


BEGIN_COM_MAP(CRandomMathCondition)
	COM_INTERFACE_ENTRY(IRandomMathCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IRandomMathCondition)
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

// IRandomMathConditionFAMCondition
	STDMETHOD(get_Percent)(long* pnVal);
	STDMETHOD(put_Percent)(long nVal);

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

	// Indicates whether the randomizer has been seeded yet or not
	bool m_bSeeded;

	// The modulus value to use
	long m_nPercent;

	/////////////
	// Methods
	/////////////
	void validateLicense();
};
