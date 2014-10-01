// OCRFileProcessor.h : Declaration of the COCRFileProcessor

#pragma once

#include "resource.h"       // main symbols
#include "IdentifiableObject.h"

#include "..\..\..\UCLIDFileProcessing\Code\FPCategories.h"
#include <Win32Semaphore.h>
#include <Win32Event.h>

#include <string>


/////////////////////////////////////////////////////////////////////////////
// COCRFileProcessor
class ATL_NO_VTABLE COCRFileProcessor : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COCRFileProcessor, &CLSID_OCRFileProcessor>,
	public ISupportErrorInfo,
	public IDispatchImpl<IOCRFileProcessor, &IID_IOCRFileProcessor, &LIBID_UCLID_FILEPROCESSORSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<COCRFileProcessor>,
	public IDispatchImpl<IParallelizableTask, &__uuidof(IParallelizableTask), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1>,
	public IDispatchImpl<IIdentifiableObject, &IID_IIdentifiableObject, &LIBID_UCLID_COMUTILSLib>,
	public CIdentifiableObject
{
public:
	COCRFileProcessor();
	~COCRFileProcessor();

	DECLARE_REGISTRY_RESOURCEID(IDR_OCRFILEPROCESSOR)

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	BEGIN_COM_MAP(COCRFileProcessor)
		COM_INTERFACE_ENTRY(IOCRFileProcessor)
		COM_INTERFACE_ENTRY2(IDispatch, IOCRFileProcessor)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IFileProcessingTask)
		COM_INTERFACE_ENTRY(IAccessRequired)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
		COM_INTERFACE_ENTRY(IParallelizableTask)
		COM_INTERFACE_ENTRY(IIdentifiableObject)
	END_COM_MAP()

	BEGIN_PROP_MAP(COCRFileProcessor)
		PROP_PAGE(CLSID_OCRFileProcessorPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(COCRFileProcessor)
		IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
	END_CATEGORY_MAP()

public:
	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IOCRFileProcessor
	STDMETHOD(get_OCRPageRangeType)(EOCRFPPageRangeType *pVal);
	STDMETHOD(put_OCRPageRangeType)(EOCRFPPageRangeType newVal);
	STDMETHOD(get_SpecificPages)(BSTR *strSpecificPages);
	STDMETHOD(put_SpecificPages)(BSTR strSpecificPages);
	STDMETHOD(get_UseCleanedImage)(VARIANT_BOOL* pbUseCleaned);
	STDMETHOD(put_UseCleanedImage)(VARIANT_BOOL bUseCleaned);

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
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB,
		IFileRequestHandler* pFileRequestHandler);
	STDMETHOD(raw_ProcessFile)(IFileRecord* pFileRecord, long nActionID,
		IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();
	STDMETHOD(raw_Standby)(VARIANT_BOOL* pVal);
	STDMETHOD(get_MinStackSize)(unsigned long *pnMinStackSize);

	// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

	// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// IParallelizableTask Methods
	STDMETHOD(raw_ProcessWorkItem)(IWorkItemRecord *pWorkItem, long nActionID,
		IFAMTagManager* pFAMTM,  IFileProcessingDB* pDB, IProgressStatus *pProgressStatus);
	STDMETHOD(get_Parallelize)(VARIANT_BOOL *pVal);
	STDMETHOD(put_Parallelize)(VARIANT_BOOL newVal);
	
	// IIdentifiableObject
	STDMETHOD(get_InstanceGUID)(GUID *pVal);
private:

	//////////////
	// Variables
	//////////////
	bool m_bDirty;
	UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType m_eOCRPageRangeType;
	std::string m_strSpecificPages;

	// flag to OCR a cleaned image if it is available instead of the original image
	bool m_bUseCleanedImageIfAvailable;

	IOCREnginePtr m_ipOCREngine;

	// Event used to signal cancelation, this will only be signaled if m_bAllowCancelBeforeComplete
	// is true.
	Win32Event m_CancelEvent;

	// Flag that idicates if work items should be created for Parallelization
	bool m_bParallelize;

	// Flag that indicates that processing for the task can be canceled before it is completed
	// if true cancel will exit leaving the existing work items and when a process starts again
	// if all the settings are the same processing will begin where it left off
	bool m_bAllowCancelBeforeComplete;

	IMiscUtilsPtr m_ipMiscUtils;

	/////////////
	// Methods
	/////////////
	void validateLicense();
	//----------------------------------------------------------------------------------------------
	IOCREnginePtr getOCREngine();

	// Creates the work items for OCR in the database
	long createWorkItems(long nActionID, IFileRecordPtr ipFileRecord, IFileProcessingDBPtr ipDB);

	// Creates the work item for the filename and page number
	IWorkItemRecordPtr createWorkItem(string strFileName, int iPageNumber);

	// Stitches the results for individual pages together
	ISpatialStringPtr stitchWorkItems(const string &strInputFile, long nWorkItemGroupID, IFileProcessingDBPtr ipDB);

	// Gets the MiscUtils object used for saving objects to stringized byte stream
	IMiscUtilsPtr getMiscUtils();

	// Method that waits for work to complete
	EFileProcessingResult waitForWorkToComplete(IProgressStatusPtr ipProgressStatus,
		IFileProcessingDBPtr ipDB, long nWorkGroup, const string &strInputFileName);
};
