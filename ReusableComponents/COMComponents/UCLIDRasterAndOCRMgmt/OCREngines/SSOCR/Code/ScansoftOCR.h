
#pragma once

#include "resource.h"       // main symbols

#include <functional>
#include <string>
#include <Win32CriticalSection.h>

/////////////////////////////////////////////////////////////////////////////
// CScansoftOCR
class ATL_NO_VTABLE CScansoftOCR : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CScansoftOCR, &CLSID_ScansoftOCR>,
	public ISupportErrorInfo,
	public IDispatchImpl<IOCREngine, &IID_IOCREngine, &LIBID_UCLID_RASTERANDOCRMGMTLib>,
	public IDispatchImpl<IImageFormatConverter, &IID_IImageFormatConverter, &LIBID_UCLID_RASTERANDOCRMGMTLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IPrivateLicensedComponent, &IID_IPrivateLicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CScansoftOCR();
	~CScansoftOCR();

DECLARE_REGISTRY_RESOURCEID(IDR_SCANSOFTOCR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CScansoftOCR)
	COM_INTERFACE_ENTRY(IOCREngine)
	COM_INTERFACE_ENTRY(IImageFormatConverter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IOCREngine)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IPrivateLicensedComponent)
END_COM_MAP()

public:

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IOCREngine
	STDMETHOD(raw_RecognizeTextInImage)(BSTR strImageFileName, long lStartPage, long lEndPage, 
		EFilterCharacters eFilter, BSTR bstrCustomFilterCharacters, EOcrTradeOff eTradeOff, 
		VARIANT_BOOL bReturnSpatialInfo, IProgressStatus* pProgressStatus, 
		IOCRParameters* pOCRParameters,
		ISpatialString **pstrText);
	STDMETHOD(raw_RecognizeTextInImage2)(BSTR strImageFileName, BSTR strPageNumbers,
		VARIANT_BOOL bReturnSpatialInfo, IProgressStatus* pProgressStatus, 
		IOCRParameters* pOCRParameters,
		ISpatialString* *pstrText);
	STDMETHOD(raw_SupportsTrainingFiles)(VARIANT_BOOL *pbValue);
	STDMETHOD(raw_LoadTrainingFile)(BSTR strTrainingFileName);
	STDMETHOD(raw_RecognizeTextInImageZone)(BSTR strImageFileName, long lStartPage, long lEndPage,   
		ILongRectangle* pZone, long nRotationInDegrees, EFilterCharacters eFilter, 
		BSTR bstrCustomFilterCharacters, VARIANT_BOOL bDetectHandwriting, 
		VARIANT_BOOL bReturnUnrecognized, VARIANT_BOOL bReturnSpatialInfo, 
		IProgressStatus* pProgressStatus, IOCRParameters* pOCRParameters,
		ISpatialString* *pstrText);
	STDMETHOD(raw_WhackOCREngine)();
	STDMETHOD(raw_CreateOutputImage)(BSTR bstrImageFileName, BSTR bstrFormat, BSTR bstrOutputFileName,
		IOCRParameters* pOCRParameters);
	STDMETHOD(raw_GetPDFImage)(BSTR bstrFileName, int nPage, VARIANT* pImageData);

// IImageFormatConverter
	STDMETHOD(raw_ConvertImage)(
		BSTR inputFileName,
		BSTR outputFileName,
		ImageFormatConverterFileType outputType,
		VARIANT_BOOL preserveColor,
		BSTR pagesToRemove,
		ImageFormatConverterNuanceFormat explicitFormat,
		long compressionLevel);

	STDMETHOD(raw_ConvertImagePage)(
		BSTR inputFileName,
		BSTR outputFileName,
		ImageFormatConverterFileType outputType,
		VARIANT_BOOL preserveColor,
		long page,
		ImageFormatConverterNuanceFormat explicitFormat,
		long compressionLevel);

	STDMETHOD(raw_CreateSearchablePdf)(
		BSTR inputFileName,
		BSTR outputFileName,
		VARIANT_BOOL deleteOriginal,
		VARIANT_BOOL outputPdfA,
		BSTR userPassword,
		BSTR ownerPassword,
		VARIANT_BOOL passwordsAreEncrypted,
		long permissions);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IPrivateLicensedComponent
	STDMETHOD(raw_InitPrivateLicense)(BSTR strPrivateLicenseKey);
	STDMETHOD(raw_IsPrivateLicensed)(VARIANT_BOOL *pbIsLicensed);

private:
	//////////////
	// Variables
	//////////////

	IScansoftOCR2Ptr m_ipOCREngine;
	// this is the pid of the ocrEngine .exe so
	// that we may kill it if we want
	long m_pid;
	IImageUtilsPtr m_ipImageUtils;

	static std::string ms_strCharSet;
	static std::string ms_strTrainingFileName;

	bool m_bPrivateLicenseInitialized;

	Win32CriticalSection m_cs;

	Win32CriticalSection m_csKillingOCR;

	// keep track of number of images processed by the OCR engine
	// as we want each instance of the OCR engine to be limited to processing
	// a certain max number of images (to keep memory leaks, etc within control) 
	LONG m_ulNumImagesProcessed;

	// whether to look for popup error dialogs from SSOCR2
	bool m_bLookForErrorDialog;

	// handle for popup error dialog from SSOCR2
	HWND m_hwndErrorDialog;

	volatile bool m_bKilledOcrDoNotRetry;

	//////////////
	// Methods
	//////////////
	void validateLicense();

	void initOCREngineLicense(std::string strKey);
	void resetNuanceLicensing();
	IScansoftOCR2Ptr getOCREngine();
	void killOCREngine();
	void checkOCREngine();
	IImageUtilsPtr getImageUtils();
	IImageFormatConverterPtr getImageFormatConverter();

	unsigned long getMaxRecognitionsPerOCREngineInstance();

	//---------------------------------------------------------------------------------------------
	// NOTE: To recognize printed text in an image zone it is preferable to use 
	// recognizePrintedTextInImageZone(), because recognizing printed text in a zone does not 
	// require a decomposition method. If SSOCR threw an exception using this method, it would be
	// re-OCRed with a new decomposition method, even though changing the decomposition method
	// would not make a difference.
	ISpatialStringPtr recognizeText(BSTR strImageFileName, IVariantVectorPtr ipPageNumbers, 
		ILongRectangle* pZone, long nRotationInDegrees, EFilterCharacters eFilter, 
		BSTR bstrCustomFilterCharacters, EOcrTradeOff eTradeOff, VARIANT_BOOL vbDetectHandwriting, 
		VARIANT_BOOL vbReturnUnrecognized, VARIANT_BOOL bReturnSpatialInfo, 
		IProgressStatus* pProgressStatus, IOCRParameters* pOCRParameters);	
	//---------------------------------------------------------------------------------------------
	// PURPOSE: This method provides the same functionality as recognizeText except this method is
	// only to be used with printed text in an image zone. Since a decomposition method is not used
	// in this case, this method will not re-OCR an image if SSOCR throws an exception.
	ISpatialStringPtr recognizePrintedTextInImageZone(BSTR strImageFileName, long lStartPage, 
		long lEndPage, ILongRectangle* pZone, long nRotationInDegrees, EFilterCharacters eFilter, 
		BSTR bstrCustomFilterCharacters, VARIANT_BOOL bReturnUnrecognized, 
		VARIANT_BOOL bReturnSpatialInfo, IProgressStatus* pProgressStatus, IOCRParameters* pOCRParameters);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Returns an IVariantVector containing the numbers lStartPage to lEndPage inclusive.
	//          If lEndPage < 1, ends on the last page of the document.
	IVariantVectorPtr createPageNumberVector(BSTR bstrImageFileName, long lStartPage, 
		long lEndPage);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To auto-dismiss hWnd as an SSOCR2-related error dialog. Logs the occurance and sets
	//          m_bLookForErrorDialog to true.
	void dismissErrorDialog(HWND hWnd);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Auto-dismisses m_hWndErrorDialog if hWnd contains SSOCR2-related text. Returns 
	//          FALSE if hWnd contains SSOCR2-related text, returns TRUE otherwise.
	// REQUIRE: lParam is the this pointer of the calling CScansoftOCR class
	//          m_hWndErrorDialog is expected to be the parent of hWnd
	static BOOL CALLBACK enumForSSOCR2Text(HWND hWnd, LPARAM lParam);

	//---------------------------------------------------------------------------------------------
	// Retry a function if it fails because of a Nuance License Service error
	void retryOnNLSFailure(const std::string& description, std::function<void()> func,
		const std::string& eliCodeForRetry, const std::string& eliCodeForFailure);
};
//-------------------------------------------------------------------------------------------------
