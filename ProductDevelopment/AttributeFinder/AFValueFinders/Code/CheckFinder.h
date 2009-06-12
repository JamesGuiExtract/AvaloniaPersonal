#pragma once

#include "resource.h"       // main symbols
#include "AFValueFinders.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <vector>

//--------------------------------------------------------------------------------------------------
// structs
//--------------------------------------------------------------------------------------------------
// Contains data about the page the check was extracted from
struct CheckExtractionData
{
	long width; // Width of the image page
	long height; // Height of the image page
	double skew; // Skew of the extracted image
};

/////////////////////////////////////////////////////////////////////////////
// CCheckFinder
class ATL_NO_VTABLE CCheckFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCheckFinder, &CLSID_CheckFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<ICheckFinder, &IID_ICheckFinder, &LIBID_UCLID_AFVALUEFINDERSLib, 1, 0>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib, 1>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib, 1>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib, 1>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib, 1>,
	public IPersistStream
{
public:
	CCheckFinder();
	~CCheckFinder();

DECLARE_REGISTRY_RESOURCEID(IDR_CHECKFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCheckFinder)
	COM_INTERFACE_ENTRY(ICheckFinder)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeFindingRule)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IPersistStream)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CCheckFinder)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument * pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICheckFinder

private:
	bool m_bDirty;

	//----------------------------------------------------------------------------------------------
	// Finds checks on the specified vector of pages
	void findChecks(const std::string& strImageName, const std::vector<long>& vecPages,
		IIUnknownVectorPtr ipAttributes);
	//----------------------------------------------------------------------------------------------
	// Builds a new attribute from the specified image data and rectangular coordinates
	// ARGS:
	//		strImageName		- The name of the image the attribute applies to
	//		lPage				- The page the attribute appears on
	//		checkData			- The dimensions of the original image, along with the skew
	//							  of the extracted image
	//		rect				- The bounds of the check image
	//		strAttributeName	- The optional name to apply to the attribute
	IAttributePtr buildAttribute(const std::string& strImageName, long lPage,
		const CheckExtractionData& checkData, const RECT& rect,
		const std::string& strAttributeName = "");
	//----------------------------------------------------------------------------------------------
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(CheckFinder), CCheckFinder)
