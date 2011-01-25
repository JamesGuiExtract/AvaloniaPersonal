// OnceEveryMathCondition.h : Declaration of the COnceEveryMathCondition

#pragma once
#include "resource.h"       // main symbols

#include "..\..\Code\FPCategories.h"
#include "ESSkipConditions.h"

#include <afxmt.h>
#include <string>
#include <map>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// COnceEveryMathCondition
class ATL_NO_VTABLE COnceEveryMathCondition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COnceEveryMathCondition, &CLSID_OnceEveryMathCondition>,
	public ISupportErrorInfo,
	public IDispatchImpl<IOnceEveryMathCondition, &IID_IOnceEveryMathCondition, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IClipboardCopyable, &IID_IClipboardCopyable, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream
{
public:
	COnceEveryMathCondition();
	~COnceEveryMathCondition();


DECLARE_REGISTRY_RESOURCEID(IDR_ONCE_EVERY_MATH_CONDITION)


BEGIN_COM_MAP(COnceEveryMathCondition)
	COM_INTERFACE_ENTRY(IOnceEveryMathCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IOnceEveryMathCondition)
	COM_INTERFACE_ENTRY(IMathConditionChecker)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IClipboardCopyable)
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

// IOnceEveryMathConditionFAMCondition
	STDMETHOD(get_NumberOfTimes)(long* pnVal);
	STDMETHOD(put_NumberOfTimes)(long nVal);
	STDMETHOD(get_UsageID)(BSTR* pbstrUsageID);
	STDMETHOD(put_UsageID)(BSTR bstrUsageID);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IClipboardCopyable
	STDMETHOD(raw_NotifyCopiedFromClipboard)();

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

	// Mutex for checking and incrementing the count
	static CMutex ms_Mutex;

	// Map which holds the counts for each unique usage ID
	static map<string, long> ms_mapIDToCount;

	// The number of times to run before considering the condition met
	long m_nNumberOfTimes;

	// The unique ID for this usage
	string m_strUsageID;

	/////////////
	// Methods
	/////////////
	void validateLicense();

	bool checkCount();
};
