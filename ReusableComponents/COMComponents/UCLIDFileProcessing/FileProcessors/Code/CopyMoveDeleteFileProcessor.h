// CopyMoveDeleteFileProcessor.h : Declaration of the CCopyMoveDeleteFileProcessor

#pragma once
#include "resource.h"       // main symbols

#include "..\..\..\UCLIDFileProcessing\Code\FPCategories.h"
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CCopyMoveDeleteFileProcessor
class ATL_NO_VTABLE CCopyMoveDeleteFileProcessor : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCopyMoveDeleteFileProcessor, &CLSID_CopyMoveDeleteFileProcessor>,
	public ISupportErrorInfo,
	public IDispatchImpl<ICopyMoveDeleteFileProcessor, &IID_ICopyMoveDeleteFileProcessor, &LIBID_UCLID_FILEPROCESSORSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CCopyMoveDeleteFileProcessor>
{
public:
	CCopyMoveDeleteFileProcessor();
	~CCopyMoveDeleteFileProcessor();

DECLARE_REGISTRY_RESOURCEID(IDR_COPYMOVEDELETEFILEPROCESSOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCopyMoveDeleteFileProcessor)
	COM_INTERFACE_ENTRY(ICopyMoveDeleteFileProcessor)
	COM_INTERFACE_ENTRY2(IDispatch, ICopyMoveDeleteFileProcessor)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CCopyMoveDeleteFileProcessor)
	PROP_PAGE(CLSID_CopyMoveDeleteFileProcessorPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CCopyMoveDeleteFileProcessor)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICopyMoveDeleteFileProcessor
	STDMETHOD(SetMoveFiles)(/*[in]*/ BSTR bstrSrcDoc, /*[in]*/ BSTR bstrDstDoc);
	STDMETHOD(SetCopyFiles)(/*[in]*/ BSTR bstrSrcDoc, /*[in]*/ BSTR bstrDstDoc);
	STDMETHOD(SetDeleteFiles)(/*[in]*/ BSTR bstrSrcDoc);
	STDMETHOD(get_Operation)(/*[out, retval]*/ ECopyMoveDeleteOperationType *pRetVal);
	STDMETHOD(get_SourceFileName)(/*[out, retval]*/ BSTR *pRetVal);
	STDMETHOD(get_DestinationFileName)(/*[out, retval]*/ BSTR *pRetVal);
	STDMETHOD(get_CreateFolder)(/*[out, retval]*/ VARIANT_BOOL *pRetVal);
	STDMETHOD(put_CreateFolder)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_SourceMissingType)(ECMDSourceMissingType *pVal);
	STDMETHOD(put_SourceMissingType)(ECMDSourceMissingType newVal);
	STDMETHOD(get_DestinationPresentType)(ECMDDestinationPresentType *pVal);
	STDMETHOD(put_DestinationPresentType)(ECMDDestinationPresentType newVal);
	STDMETHOD(get_AllowReadonly)(/*[out, retval]*/ VARIANT_BOOL *pRetVal);
	STDMETHOD(put_AllowReadonly)(/*[in]*/ VARIANT_BOOL newVal);

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

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:

	//////////////
	// Data
	//////////////
	bool	m_bDirty;

	ECopyMoveDeleteOperationType	m_eOperation;
	ECMDSourceMissingType			m_eSrcMissingType;
	ECMDDestinationPresentType		m_eDestPresentType;
	std::string m_strSrc;
	std::string m_strDst;

	bool	m_bCreateDirectory;

	// Specifies whether move/delete will work on readonly files
	bool	m_bAllowReadonly; 

	/////////////
	// Methods
	/////////////

	// Checks destination file
	// Returns true if operation (Copy or Move) should continue
	// Returns false if operation should be skipped
	// Throws exception if destination file already exists and should cause error
	bool checkDestinationFile(const std::string& strDestinationFile);

	// Checks destination folder and creates directory if needed and desired.
	// If needed and NOT desired, thorws exception
	void handleDirectory(const std::string& strDestinationFile);

	void validateLicense();
};
