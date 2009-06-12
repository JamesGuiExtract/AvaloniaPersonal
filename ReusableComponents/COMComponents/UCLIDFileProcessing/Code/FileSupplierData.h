// FileSupplierData.h : Declaration of the CFileSupplierData

#pragma once
#include "resource.h"       // main symbols

#include "UCLIDFileProcessing.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CFileSupplierData

class ATL_NO_VTABLE CFileSupplierData :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileSupplierData, &CLSID_FileSupplierData>,
	public ISupportErrorInfo,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IPersistStream,
	public IDispatchImpl<IFileSupplierData, &IID_IFileSupplierData, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CFileSupplierData();
	~CFileSupplierData();

DECLARE_REGISTRY_RESOURCEID(IDR_FILESUPPLIERDATA)

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}


BEGIN_COM_MAP(CFileSupplierData)
	COM_INTERFACE_ENTRY(IFileSupplierData)
	COM_INTERFACE_ENTRY2(IDispatch, IFileSupplierData)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IPersistStream)
END_COM_MAP()

public:

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);
	
// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IFileSupplierData
	STDMETHOD(get_FileSupplier)(/*[out, retval]*/ IObjectWithDescription **pVal);
	STDMETHOD(put_FileSupplier)(/*[in]*/ IObjectWithDescription *newVal);
	STDMETHOD(get_ForceProcessing)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ForceProcessing)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_FileSupplierStatus)(/*[out, retval]*/ EFileSupplierStatus *pVal);
	STDMETHOD(put_FileSupplierStatus)(/*[in]*/ EFileSupplierStatus newVal);

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

	// File Supplier object + Description + Enabled flag
	IObjectWithDescriptionPtr m_ipFileSupplier;

	// Whether or not supplied files will be processed 
	// regardless of the database state
	bool m_bForceProcessing;

	// Status of this File Supplier object
	// - Defaults to: UCLID_FILEPROCESSINGLib::kInactiveStatus
	// - Not persistent, loading FPS file assumes all File Suppliers are Inactive
	EFileSupplierStatus m_eSupplierStatus;

	// True if this object has been modified
	bool m_bDirty;

	/////////////
	// Methods
	/////////////
	void validateLicense();
};

//OBJECT_ENTRY_AUTO(__uuidof(FileSupplierData), CFileSupplierData)
