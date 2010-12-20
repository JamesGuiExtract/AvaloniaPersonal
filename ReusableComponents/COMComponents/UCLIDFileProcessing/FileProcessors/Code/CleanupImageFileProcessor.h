// CleanupImageFileProcessor.h : Declaration of the CCleanupImageFileProcessor

#pragma once
#include "resource.h"       // main symbols

#include "FileProcessors.h"
#include "..\..\..\UCLIDFileProcessing\Code\FPCategories.h"

#include <string>

using namespace std;

// CCleanupImageFileProcessor

class ATL_NO_VTABLE CCleanupImageFileProcessor :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCleanupImageFileProcessor, &CLSID_CleanupImageFileProcessor>,
	public ISupportErrorInfo,
	public IDispatchImpl<ICleanupImageFileProcessor, &IID_ICleanupImageFileProcessor, 
			&LIBID_UCLID_FILEPROCESSORSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CCleanupImageFileProcessor>
{
public:
	CCleanupImageFileProcessor();
	~CCleanupImageFileProcessor();

DECLARE_REGISTRY_RESOURCEID(IDR_CLEANUPIMAGEFILEPROCESSOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCleanupImageFileProcessor)
	COM_INTERFACE_ENTRY(ICleanupImageFileProcessor)
	COM_INTERFACE_ENTRY2(IDispatch, ICleanupImageFileProcessor)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CCleanupImageFileProcessor)
	PROP_PAGE(CLSID_CleanupImageFileProcessorPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CCleanupImageFileProcessor)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// IFileProcessingTask
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB);
	STDMETHOD(raw_ProcessFile)(BSTR bstrFileFullName, long nFileID, long nActionID,
		IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);
	
// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICleanupImageFileProcessor
	STDMETHOD(get_ImageCleanupSettingsFileName)(BSTR* strFileName);
	STDMETHOD(put_ImageCleanupSettingsFileName)(BSTR strFileName);

private:
	// Variables
	bool m_bDirty;

	// image cleanup engine
	IImageCleanupEnginePtr m_ipImageCleanupEngine;

	// settings file string
	string m_strImageCleanupSettingsFileName;

	// misc utils pointer
	IMiscUtilsPtr m_ipMiscUtils;

	// Methods
	void validateLicense();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the image cleanup engine if it exists.  if it does not exist then to
	//			create it and return it
	IImageCleanupEnginePtr getImageCleanupEngine();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the MiscUtils Object if it exists, otherwise create an instance and return
	//			it
	IMiscUtilsPtr getMiscUtils();
	//----------------------------------------------------------------------------------------------
};

OBJECT_ENTRY_AUTO(__uuidof(CleanupImageFileProcessor), CCleanupImageFileProcessor)
