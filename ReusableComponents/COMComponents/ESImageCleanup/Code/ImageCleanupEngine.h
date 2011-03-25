// ImageCleanupEngine.h : Declaration of the CImageCleanupEngine

#pragma once
#include "resource.h"       // main symbols
#include "ESImageCleanup.h"

#include <MiscLeadUtils.h>

#include <string>
#include <set>
#include <vector>

using namespace std;

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

	// string to hold the path to the CleanupImage.exe
	string m_strPathToCleanupImageEXE;

	//----------------------------------------------------------------------------------------------
	// Methods
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To perform the repair operations contained in the vector of operations on the
	//			specified page of the image.
	//
	// NOTE:	The first page of the image is stored as 0
	void cleanImagePage(BITMAPHANDLE& rhBitmap, const ICiServerPtr& ipciServer,
		const ICiRepairPtr& ipciRepair,
		const vector<ESImageCleanupLib::IImageCleanupOperationPtr>& vecOperations);
	//----------------------------------------------------------------------------------------------
	void getSetOfPages(set<int>& rsetPageNumbers,
		const ESImageCleanupLib::IImageCleanupSettingsPtr& ipSettings, int nPageCount);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get a vector of all enabled image cleanup operations
	void getEnabledCleanupOperations(vector<ESImageCleanupLib::IImageCleanupOperationPtr>& rvecOps,
		const IIUnknownVectorPtr& ipOperations);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the path to CleanupImage.exe
	// PROMISE: To return the absolute path to CleanupImage.exe or throw an exception if it 
	//			cannot be found
	string getESImageCleanupEngineEXE();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To swap the palette of the bitonal bitmap from {white, black} to {black, white}
	// NOTE:	DO NOT CALL this method if the bitmaphandle is not a bitonal bitmap, it will
	//			change the bitmap to a bitonal bitmap and replace the color palette
	void swapPalette(BITMAPHANDLE& rhBitmap);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: validate the image cleanup license
	void validateLicense();
	//---------------------------------------------------------------------------------------------
};

OBJECT_ENTRY_AUTO(__uuidof(ImageCleanupEngine), CImageCleanupEngine)
