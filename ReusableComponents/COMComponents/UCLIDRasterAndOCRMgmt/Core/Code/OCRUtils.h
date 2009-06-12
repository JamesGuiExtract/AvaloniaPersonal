// OCRUtils.h : Declaration of the COCRUtils

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// COCRUtils
class ATL_NO_VTABLE COCRUtils : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COCRUtils, &CLSID_OCRUtils>,
	public ISupportErrorInfo,
	public IDispatchImpl<IOCRUtils, &IID_IOCRUtils, &LIBID_UCLID_RASTERANDOCRMGMTLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	COCRUtils();
	~COCRUtils();

DECLARE_REGISTRY_RESOURCEID(IDR_OCRUTILS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(COCRUtils)
	COM_INTERFACE_ENTRY(IOCRUtils)
	COM_INTERFACE_ENTRY2(IDispatch,IOCRUtils)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IOCRUtils
	STDMETHOD(BatchOCR)(/*[in]*/ BSTR strRootDirOrFile, 
		/*[in]*/ IOCREngine *pEngine, /*[in]*/ VARIANT_BOOL bRecursive, 
		/*[in]*/ long nMaxNumOfPages, /*[in]*/ VARIANT_BOOL bCreateUSSFile,
		/*[in]*/ VARIANT_BOOL bCompressUSSFile, /*[in]*/ VARIANT_BOOL bSkipCreation,
		/*[in]*/ IProgressStatus* pProgressStatus);
	STDMETHOD(RecognizeTextInImageFile)(
		/*[in]*/ BSTR strImageFileName, 
		/*[in]*/ long lNumPages, 
		/*[in]*/ IOCREngine* pOCREngine, 
		/*[in]*/ IProgressStatus* pProgressStatus,
		/*[out, retval]*/ ISpatialString* *pstrText);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(/*[out, retval]*/ VARIANT_BOOL * pbValue);

private:
	///////////
	// Methods
	///////////
	// If the file has 3-digit number (ex. d:\temp\3445654.003.tif), this
	// function will return the actual number (in the example, it shall return 3)
	// that indicates the actual page in the document (in this example, d:\temp\3445654.003.tif
	// if the third page of the document 3445654).
	// If the file doesn't have 3-digit number (ex. d:\temp\2342.tif), this
	// function will return 0
	// Require: strFullFileName must be fully qualified file name.
	int get3DigitFilePageNumber(const std::string& strFullFileName);

	// Get all image files recursively under strDirectory
	std::vector<std::string> getImageFilesInDir(const std::string& strDirectory,
		bool bRecursive);

	// process individual image file, i.e. OCR image file and store
	// the output text to a .txt file (if bCreateUSSFile == VARIANT_FALSE)
	// or to a .uss file (if bCreateUSSFile == VARIANT_TRUE).
	// If (bCreateUSSFile == VARIANT_TRUE && bCompressUSSFile == VARIANT_TRUE),
	// then the .uss file created will be compressed.
	// If bCompressUSSFile == VARIANT_TRUE and the output file already exists, 
	// the output file will not be overwritten.
	void processImageFile(const std::string& strImageFile, int nMaxNumOfPages,
		VARIANT_BOOL bCreateUSSFile, VARIANT_BOOL bCompressUSSFile, 
		VARIANT_BOOL bSkipCreation, 
		UCLID_RASTERANDOCRMGMTLib::IOCREnginePtr ipOCREngine,
		IProgressStatus* pProgressStatus);

	// recognize the image and save the output the specified file.
	// strImageFileName -- the fully qualified input image file name
	// nNumOfPagesToRecognized -- how many pages of the image to recognize. If -1, then
	//							  the entire image
	// bReturnSpatislInfo -- if true, then the returned spatial string will
	//						actually have spatial information associated with it
	//						Otherwise, the returned spatial string is just 
	//						non-spatial text.
	// return the spatial string that was recognized
	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr recognizeImage(
		const std::string& strImageFileName, int nNumOfPagesToRecognize,
		bool bReturnSpatialInfo, 
		UCLID_RASTERANDOCRMGMTLib::IOCREnginePtr ipOCREngine,
		IProgressStatus* pProgressStatus);

	void validateLicense();
};

