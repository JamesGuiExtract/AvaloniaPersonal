// SelectOnlyUniqueValues.h : Declaration of the CSelectOnlyUniqueValues

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include <IdentifiableRuleObject.h>

/////////////////////////////////////////////////////////////////////////////
// CSelectOnlyUniqueValues
class ATL_NO_VTABLE CSelectOnlyUniqueValues : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSelectOnlyUniqueValues, &CLSID_SelectOnlyUniqueValues>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IIdentifiableRuleObject, &IID_IIdentifiableRuleObject, &LIBID_UCLID_AFCORELib>,
	private CIdentifiableRuleObject
{
public:
	CSelectOnlyUniqueValues();
	~CSelectOnlyUniqueValues();

DECLARE_REGISTRY_RESOURCEID(IDR_SELECTONLYUNIQUEVALUES)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSelectOnlyUniqueValues)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY2(IDispatch, IOutputHandler)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IIdentifiableRuleObject)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CSelectOnlyUniqueValues)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IIdentifiableRuleObject
	STDMETHOD(get_InstanceGUID)(GUID *pVal);

private:
	///////
	// Data
	///////

	// flag to keep track of whether object is dirty
	bool m_bDirty;

	/////////////
	// Methods
	/////////////
	void validateLicense();
};

