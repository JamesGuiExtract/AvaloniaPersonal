// ConvertToPDFTask.h : Declaration of the CConvertToPDFTask

#pragma once

#include "resource.h"       // main symbols

#include <FPCategories.h>

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CConvertToPDFTask
class ATL_NO_VTABLE CConvertToPDFTask : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CConvertToPDFTask, &CLSID_ConvertToPDFTask>,
	public IDispatchImpl<IConvertToPDFTask, &IID_IConvertToPDFTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<CConvertToPDFTask>
{
public:
	CConvertToPDFTask();
	~CConvertToPDFTask();

DECLARE_REGISTRY_RESOURCEID(IDR_CONVERT_TO_PDF_TASK)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CConvertToPDFTask)
	COM_INTERFACE_ENTRY(IConvertToPDFTask)
	COM_INTERFACE_ENTRY2(IDispatch, IConvertToPDFTask)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CConvertToPDFTask)
	PROP_PAGE(CLSID_ConvertToPDFTaskPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CConvertToPDFTask)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()

public:

// IConvertToPDFTask
	STDMETHOD(SetOptions)(BSTR bstrInputFile, VARIANT_BOOL vbPDFA,
		IPdfPasswordSettings* pPdfSettings);
	STDMETHOD(GetOptions)(BSTR* pbstrInputFile, VARIANT_BOOL* pvbPDFA,
		IPdfPasswordSettings** ppPdfSettings);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR* pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown* pObject);

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
	STDMETHOD(get_DisplaysUI)(VARIANT_BOOL* pVal);

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL* pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)();
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

private:

// Private variables

	// filename of input image file
	string m_strInputImage;

	// if true then the converted PDF will be PDF/A compliant
	bool m_bPDFA;

	// dirty flag
	bool m_bDirty;

	// The pdf password settings object
	IPdfPasswordSettingsPtr m_ipPdfPassSettings;

	IImageFormatConverterPtr m_ipImageFormatConverter;

// Private methods

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To encrypt the specified string using the Pdf security values
	void encryptString(string& rstrString);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if this component is not licensed. Runs successfully otherwise.
	void validateLicense();

	IImageFormatConverterPtr getImageFormatConverter();
};

OBJECT_ENTRY_AUTO(CLSID_ConvertToPDFTask, CConvertToPDFTask)
