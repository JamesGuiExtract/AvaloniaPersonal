// FillStripesICO.h : Declaration of the CFillStripesICO

#pragma once
#include "resource.h"       // main symbols
#include "ICCategories.h"
#include "ESImageCleanup.h"

#include <vector>

using namespace std;

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CFillStripesICO
class ATL_NO_VTABLE CFillStripesICO :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFillStripesICO, &CLSID_FillStripesICO>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IFillStripesICO,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IImageCleanupOperation, &IID_IImageCleanupOperation, &LIBID_ESImageCleanupLib>
{
public:
	CFillStripesICO();
	~CFillStripesICO();

	DECLARE_REGISTRY_RESOURCEID(IDR_FILLSTRIPESICO)

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	BEGIN_COM_MAP(CFillStripesICO)
		COM_INTERFACE_ENTRY(IFillStripesICO)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IImageCleanupOperation)
	END_COM_MAP()

	BEGIN_CATEGORY_MAP(CFillStripesICO)
		IMPLEMENTED_CATEGORY(CATID_ICO_CLEANING_OPERATIONS)
	END_CATEGORY_MAP()

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

	// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// IImageCleanupOperation Methods
	STDMETHOD(Perform)(void* pciRepair);

private:

	// variables
	bool m_bDirty;

	// Helper functions
	//----------------------------------------------------------------------------------------------
	// PURPOSE: validate the image cleanup license
	void validateLicense();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to count the number of black pixels in each column
	void countBlackPixlesInColumns(vector<long>& rvecColumns, long lBytesPerLine, long lHeight, 
		unsigned char* pImage);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to fill in the striped columns with black pixels
	//
	// NOTE:	only put a black pixel in a certain row and column if:
	//			1. the column contains no black pixels (indication of a stripe)
	//			2. there is a black pixel to both the left and right of the current pixel
	void fillInStripedArea(vector<long>& rvecColumns, long lBytesPerLine, long lHeight,
							long lWidth, unsigned char* pImage);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to check if the pixel to the immediate left of the current pixel is black
	// REQUIRE: lColumn != 0
	bool isLeftPixelBlack(long lRow, long lColumn, long lBytesPerLine, unsigned char* pImage); 
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to check if the pixel to the immediate right of the current pixel is black
	// REQUIRE: lColumn is not the rightmost column - lColumn != (lBytesPerLine * 8) - 1
	bool isRightPixelBlack(long lRow, long lColumn, long lBytesPerLine, unsigned char* pImage); 
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to retrieve a specific byte from the image
	unsigned char getByteFromImage(long lRow, long lByteInColumn, long lBytesPerLine, 
		unsigned char* pImage);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to retrieve a pointer to a specific byte in the image
	unsigned char* getPointerToByteFromImage(long lRow, long lByteInColumn, 
		long lBytesPerLine, unsigned char* pImage);
	//----------------------------------------------------------------------------------------------
};
OBJECT_ENTRY_AUTO(__uuidof(FillStripesICO), CFillStripesICO)