// RedactionVerificationUI.h : Declaration of the CRedactionVerificationUI

#pragma once

#include "resource.h"       // main symbols
#include "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDFileProcessing\Code\FPCategories.h"
#include "RVUIThread.h"
#include "RedactionUISettings.h"

#include <memory>
#include <afxmt.h>

/////////////////////////////////////////////////////////////////////////////
// CRedactionVerificationUI
class ATL_NO_VTABLE CRedactionVerificationUI : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRedactionVerificationUI, &CLSID_RedactionVerificationUI>,
	public ISupportErrorInfo,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CRedactionVerificationUI>,
	public IDispatchImpl<IRedactionVerificationUI, &IID_IRedactionVerificationUI, &LIBID_UCLID_REDACTIONCUSTOMCOMPONENTSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>
{
public:
	CRedactionVerificationUI();
	~CRedactionVerificationUI();

DECLARE_REGISTRY_RESOURCEID(IDR_REDACTIONVERIFICATIONUI)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRedactionVerificationUI)
	COM_INTERFACE_ENTRY(IRedactionVerificationUI)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY2(IDispatch, IRedactionVerificationUI)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
END_COM_MAP()

BEGIN_PROP_MAP(CRedactionVerificationUI)
	PROP_PAGE(CLSID_RedactionVerificationUIPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CRedactionVerificationUI)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRedactionVerificationUI
	STDMETHOD(ShowUI)(BSTR strFileName);
	STDMETHOD(get_ReviewAllPages)(VARIANT_BOOL* pVal);
	STDMETHOD(put_ReviewAllPages)(VARIANT_BOOL newVal);
	STDMETHOD(get_AlwaysOutputImage)(VARIANT_BOOL* pVal);
	STDMETHOD(put_AlwaysOutputImage)(VARIANT_BOOL newVal);
	STDMETHOD(get_OutputImageName)(BSTR* pVal);
	STDMETHOD(put_OutputImageName)(BSTR newVal);
	STDMETHOD(get_AlwaysOutputMeta)(VARIANT_BOOL* pVal);
	STDMETHOD(put_AlwaysOutputMeta)(VARIANT_BOOL newVal);
	STDMETHOD(get_MetaOutputName)(BSTR* pVal);
	STDMETHOD(put_MetaOutputName)(BSTR newVal);
	STDMETHOD(get_CarryForwardAnnotations)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_CarryForwardAnnotations)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_ApplyRedactionsAsAnnotations)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ApplyRedactionsAsAnnotations)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_CollectFeedback)(VARIANT_BOOL* pvbCollectFeedback);
	STDMETHOD(put_CollectFeedback)(VARIANT_BOOL vbCollectFeedback);
	STDMETHOD(get_FeedbackCollectOption)(EFeedbackCollectOption *peFeedbackCollectOption);
	STDMETHOD(put_FeedbackCollectOption)(EFeedbackCollectOption eFeedbackCollectOption);
	STDMETHOD(get_FeedbackDataFolder)(BSTR *pbstrFeedbackDataFolder);
	STDMETHOD(put_FeedbackDataFolder)(BSTR bstrFeedbackDataFolder);
	STDMETHOD(get_CollectFeedbackImage)(VARIANT_BOOL *pvbCollectFeedbackImage);
	STDMETHOD(put_CollectFeedbackImage)(VARIANT_BOOL vbCollectFeedbackImage);
	STDMETHOD(get_FeedbackOriginalFilenames)(VARIANT_BOOL *pvbFeedbackOriginalFilenames);
	STDMETHOD(put_FeedbackOriginalFilenames)(VARIANT_BOOL vbFeedbackOriginalFilenames);
	STDMETHOD(get_RequireRedactionTypes)(VARIANT_BOOL *pvbRequireRedactionTypes);
	STDMETHOD(put_RequireRedactionTypes)(VARIANT_BOOL vbRequireRedactionTypes);
	STDMETHOD(get_InputDataFile)(BSTR *pbstrInputDataFile);
	STDMETHOD(put_InputDataFile)(BSTR bstrInputDataFile);
	STDMETHOD(get_InputRedactedImage)(VARIANT_BOOL *pvbInputRedactedImage);
	STDMETHOD(put_InputRedactedImage)(VARIANT_BOOL vbInputRedactedImage);
	STDMETHOD(get_RequireExemptionCodes)(VARIANT_BOOL *pvbRequireExemptionCodes);
	STDMETHOD(put_RequireExemptionCodes)(VARIANT_BOOL vbRequireExemptionCodes);
	STDMETHOD(get_RedactionText)(BSTR *pbstrRedactionText);
	STDMETHOD(put_RedactionText)(BSTR bstrRedactionText);
	STDMETHOD(get_BorderColor)(long *plBorderColor);
	STDMETHOD(put_BorderColor)(long lBorderColor);
	STDMETHOD(get_FillColor)(long *plFillColor);
	STDMETHOD(put_FillColor)(long lFillColor);
	STDMETHOD(get_FontName)(BSTR *pbstrFontName);
	STDMETHOD(put_FontName)(BSTR bstrFontName);
	STDMETHOD(get_IsBold)(VARIANT_BOOL *pvbBold);
	STDMETHOD(put_IsBold)(VARIANT_BOOL vbBold);
	STDMETHOD(get_IsItalic)(VARIANT_BOOL *pvbItalic);
	STDMETHOD(put_IsItalic)(VARIANT_BOOL vbItalic);
	STDMETHOD(get_FontSize)(long *plFontSize);
	STDMETHOD(put_FontSize)(long lFontSize);
	
// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IFileProcessingTask
	STDMETHOD(raw_Init)();
	STDMETHOD(raw_ProcessFile)(BSTR bstrFileFullName, IFAMTagManager *pTagManager, 
		IFileProcessingDB *pDB, IProgressStatus *pProgressStatus, VARIANT_BOOL bCancelRequested,
		VARIANT_BOOL *pbSuccessfulCompletion);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	//////////////
	// Data
	//////////////
	bool			m_bDirty;

	RedactionUISettings m_UISettings;

	// Default wait time from registry for receiving a new file
	unsigned long	m_ulDefaultWaitTimeMilliseconds;

	// There should only one UI thread that is shared among instances.  Make
	// the thread and the varibles used to control it static
	static CMutex ms_mutex;
	static std::auto_ptr<RVUIThread>	ms_apThread;
	static LONG ms_nInitializationCount;

	//////////////
	// Methods
	//////////////

	// Notify thread that file is ready for processing
	// with provision for single retry
	void processTheFile(string strFile, bool bIsRetry, IFAMTagManagerPtr ipFAMTagManager, 
		IFileProcessingDBPtr ipFAMDB);

	// Computes wait time in milliseconds for specified file.  Wait time is one millisecond per byte 
	// for the associated VOA file.  If the VOA file cannot be found, the returned time will be the 
	// default value from the registry.
	unsigned long	getWaitTime(string strFile);

	void			validateLicense();
};
