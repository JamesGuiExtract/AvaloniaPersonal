// ImageCleanupSettings.h : Declaration of the CImageCleanupSettings

#pragma once
#include "resource.h"       // main symbols
#include "ESImageCleanup.h"

#include <string>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CImageCleanupSettings

class ATL_NO_VTABLE CImageCleanupSettings :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CImageCleanupSettings, &CLSID_ImageCleanupSettings>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IImageCleanupSettings
{
public:
	CImageCleanupSettings();
	~CImageCleanupSettings();

	DECLARE_REGISTRY_RESOURCEID(IDR_IMAGECLEANUPSETTINGS)

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	BEGIN_COM_MAP(CImageCleanupSettings)
		COM_INTERFACE_ENTRY(IImageCleanupSettings)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
	END_COM_MAP()

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// IImageCleanupSettings
	STDMETHOD(LoadFrom)(BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue);
	STDMETHOD(SaveTo)(BSTR strFullFileName, VARIANT_BOOL bClearDirtyFlag);
	STDMETHOD(get_IsEncrypted)(VARIANT_BOOL *pVal);
	STDMETHOD(get_ImageCleanupOperations)(IIUnknownVector** pVal);
	STDMETHOD(put_ImageCleanupOperations)(IIUnknownVector* pNewVal);
	STDMETHOD(Clear)();
	STDMETHOD(get_SpecifiedPages)(BSTR* pstrSpecifiedPages);
	STDMETHOD(put_SpecifiedPages)(BSTR strSpecifiedPages);
	STDMETHOD(get_ICPageRangeType)(EICPageRangeType* pVal);
	STDMETHOD(put_ICPageRangeType)(EICPageRangeType newVal);
	
private:

	// variables
	bool m_bDirty;
	bool m_bIsEncrypted;
	_bstr_t m_bstrStreamName;
	IMiscUtilsPtr m_ipMiscUtils;

	// vector of IObjectWithDescriptionPtr's 
	IIUnknownVectorPtr m_ipImageCleanupOperationsVector;

	// string to hold the specified page ranges
	std::string m_strSpecifiedPages;
	
	// type to specify the type of page range listed in the specified page range string
	ESImageCleanupLib::EICPageRangeType m_eICPageRangeType;

	// methods
	//----------------------------------------------------------------------------------------------
	ESImageCleanupLib::IImageCleanupSettingsPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: create the MiscUtils object if necessary and return it
	IMiscUtilsPtr getMiscUtils();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to clear the settings and reset to default values
	void clearSettings();
	//----------------------------------------------------------------------------------------------
};

OBJECT_ENTRY_AUTO(__uuidof(ImageCleanupSettings), CImageCleanupSettings)
