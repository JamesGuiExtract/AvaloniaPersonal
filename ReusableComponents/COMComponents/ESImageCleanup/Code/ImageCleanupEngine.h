// ImageCleanupEngine.h : Declaration of the CImageCleanupEngine

#pragma once
#include "resource.h"       // main symbols
#include "ESImageCleanup.h"

#include <MiscLeadUtils.h>
#include <PDFInputOutputMgr.h>

#include <string>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CImageCleanupEngine

class ATL_NO_VTABLE CImageCleanupEngine :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CImageCleanupEngine, &CLSID_ImageCleanupEngine>,
	public ISupportErrorInfo,
	public IImageCleanupEngine
{
public:
	CImageCleanupEngine();
	~CImageCleanupEngine();

	DECLARE_REGISTRY_RESOURCEID(IDR_IMAGECLEANUPENGINE)

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	BEGIN_COM_MAP(CImageCleanupEngine)
		COM_INTERFACE_ENTRY(IImageCleanupEngine)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
	END_COM_MAP()

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IImageCleanupEngine
	STDMETHOD(CleanupImageInternalUseOnly)(BSTR bstrInputFile, BSTR bstrOutputFile, 
		IImageCleanupSettings* pImageCleanupSettings);
	STDMETHOD(CleanupImage)(BSTR bstrInputFile, BSTR bstrOutputFile, 
		BSTR bstrImageCleanupSettingsFile);

private:
	// Variables
	// LeadTools bitmap list for the dirty file
	HBITMAPLIST m_hFileBitmaps;

	// string to hold the path to the CleanupImage.exe
	string m_strPathToCleanupImageEXE;

	// Methods
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To open the image file and load m_hFileBitmaps with the bitmap pages in the file.
	//
	// PROMISE: To return the number of pages that the image contained after opening the image
	//
	// NOTE:	rFileInfo will be reset when it enters the function and will contain the
	//			FILEINFO associated with the image that was opened
	unsigned long openDirtyImage(const string& rstrFileName, FILEINFO& rFileInfo);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To perform the repair operations contained in the vector of operations on the
	//			specified page of the image.
	//
	// NOTE:	The first page of the image is stored as 0
	void cleanImagePage(int iPageNumber, ICiServerPtr ipciServer, ICiRepairPtr ipciRepair,
		IIUnknownVectorPtr ipVectorOfCleanupOperations);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To write the cleaned up image page to the PDFInputOutputMgr output file
	void writeCleanImage(unsigned long lNumberOfPages, const FILEINFO& rFileInfo,
		PDFInputOutputMgr& rOutMgr);
	//----------------------------------------------------------------------------------------------
	void getVectorOfPages(vector<int>& rvecPageNumbers, 
		ESImageCleanupLib::IImageCleanupSettingsPtr ipSettings, long lPageCount);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the path to CleanupImage.exe
	// PROMISE: To return the absolute path to CleanupImage.exe or throw an exception if it 
	//			cannot be found
	string getESImageCleanupEngineEXE();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: validate the image cleanup license
	void validateLicense();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Frees the memory allocated for the bitmap list if necessary.
	void freeBitmapList();
};

OBJECT_ENTRY_AUTO(__uuidof(ImageCleanupEngine), CImageCleanupEngine)
