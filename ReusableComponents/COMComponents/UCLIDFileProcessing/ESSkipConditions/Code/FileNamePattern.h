// FileNamePattern.h : Declaration of the CFileNamePattern

#pragma once
#include "resource.h"       // main symbols

#include "..\..\Code\FPCategories.h"
#include "ESSkipConditions.h"

#include <CachedObjectFromFile.h>
#include <RegExLoader.h>

#include <string>
#include <map>


/////////////////////////////////////////////////////////////////////////////
// CFileNamePattern
class ATL_NO_VTABLE CFileNamePattern :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileNamePattern, &CLSID_FileNamePattern>,
	public ISupportErrorInfo,
	public IDispatchImpl<IFileNamePatternFAMCondition, &IID_IFileNamePatternFAMCondition, &LIBID_EXTRACT_FAMCONDITIONSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IFAMCondition, &IID_IFAMCondition, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CFileNamePattern>
{
public:
	CFileNamePattern();
	~CFileNamePattern();


DECLARE_REGISTRY_RESOURCEID(IDR_FILENAMEPATTERN)


BEGIN_COM_MAP(CFileNamePattern)
	COM_INTERFACE_ENTRY(IFileNamePatternFAMCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IFileNamePatternFAMCondition)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IFAMCondition)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CFileNamePattern)
	PROP_PAGE(CLSID_FileNamePatternPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CFileNamePattern)
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

// IFileNamePatternFAMCondition
	STDMETHOD(get_DoesContainOrMatch)(VARIANT_BOOL* pRetVal);
	STDMETHOD(put_DoesContainOrMatch)(VARIANT_BOOL newVal);
	STDMETHOD(get_ContainMatch)(VARIANT_BOOL* pRetVal);
	STDMETHOD(put_ContainMatch)(VARIANT_BOOL newVal);
	STDMETHOD(get_IsCaseSensitive)(VARIANT_BOOL* pRetVal);
	STDMETHOD(put_IsCaseSensitive)(VARIANT_BOOL newVal);
	STDMETHOD(get_IsRegFromFile)(VARIANT_BOOL* pRetVal);
	STDMETHOD(put_IsRegFromFile)(VARIANT_BOOL newVal);

	STDMETHOD(get_FileString)(BSTR *strFileString);
	STDMETHOD(put_FileString)(BSTR strFileString);
	STDMETHOD(get_RegExpFileName)(BSTR *strFileString);
	STDMETHOD(put_RegExpFileName)(BSTR strFileString);
	STDMETHOD(get_RegPattern)(BSTR *strFileString);
	STDMETHOD(put_RegPattern)(BSTR strFileString);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IFAMCondition
	STDMETHOD(raw_FileMatchesFAMCondition)(BSTR bstrFile, IFileProcessingDB* pFPDB, long lFileID, 
		long lActionID, IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal);

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

	bool m_bDoesDoesNot;
	bool m_bContainMatch;
	bool m_bCaseSensitive;
	bool m_bIsRegExpFromFile;

	std::string m_strFileName;
	std::string m_strRegFileName;
	std::string m_strRegPattern;

	IMiscUtilsPtr	m_ipMiscUtils;

	// Use CachedObjectFromFile so that the regular expression is re-loaded from disk only when the
	// RegEx file is modified.
	CachedObjectFromFile<string, RegExLoader> m_cachedRegExLoader;

	/////////////
	// Methods
	/////////////
	void validateLicense();

	// Returns m_ipMiscUtils, after initializing it if necessary
	IMiscUtilsPtr getMiscUtils();
};
