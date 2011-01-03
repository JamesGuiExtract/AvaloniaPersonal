// FileExistence.h : Declaration of the CFileExistence

#pragma once
#include "resource.h"       // main symbols

#include "..\..\Code\FPCategories.h"
#include "ESSkipConditions.h"
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CFileExistence
class ATL_NO_VTABLE CFileExistence :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileExistence, &CLSID_FileExistence>,
	public ISupportErrorInfo,
	public IDispatchImpl<IFileExistenceFAMCondition, &IID_IFileExistenceFAMCondition, &LIBID_EXTRACT_FAMCONDITIONSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IFAMCondition, &IID_IFAMCondition, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CFileExistence>
{
public:
	CFileExistence();
	~CFileExistence();


DECLARE_REGISTRY_RESOURCEID(IDR_FILEEXISTENCE)


BEGIN_COM_MAP(CFileExistence)
	COM_INTERFACE_ENTRY(IFileExistenceFAMCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IFileExistenceFAMCondition)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IFAMCondition)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CFileExistence)
	PROP_PAGE(CLSID_FileExistencePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CFileExistence)
	IMPLEMENTED_CATEGORY(CATID_FP_FAM_CONDITIONS)
END_CATEGORY_MAP()

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IFileExistenceFAMCondition
	STDMETHOD(get_FileExists)(VARIANT_BOOL* pRetVal);
	STDMETHOD(put_FileExists)(VARIANT_BOOL newVal);
	STDMETHOD(get_FileString)(BSTR* strFileString);
	STDMETHOD(put_FileString)(BSTR strFileString);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR* pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown* pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL* pbValue);

// IFAMCondition
	STDMETHOD(raw_FileMatchesFAMCondition)(BSTR bstrFile, IFileProcessingDB* pFPDB, long lFileID, 
		long lActionID, IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal);

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID* pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream* pStm);
	STDMETHOD(Save)(IStream* pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER* pcbSize);

private:
	//////////////
	// Variables
	//////////////
	bool m_bDirty;

	bool m_bFileDoesExist;
	std::string m_strFileName;

	/////////////
	// Methods
	/////////////
	void validateLicense();
};