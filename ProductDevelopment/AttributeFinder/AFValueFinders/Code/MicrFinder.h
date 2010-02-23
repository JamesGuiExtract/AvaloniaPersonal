#pragma once

#include "resource.h"       // main symbols
#include "AFValueFinders.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <CachedObjectFromFile.h>
#include <RegExLoader.h>

#include <string>
#include <vector>
#include <map>
#include <afxmt.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Local structs
//-------------------------------------------------------------------------------------------------
// Defines a MICR zone
struct MicrZone
{
	RECT m_rectZone;
	string m_strMicrText;
	int m_nRotation;
};
//-------------------------------------------------------------------------------------------------
// Defines an entire MICR line along with its sub attributes
struct MicrLine
{
	MicrLine() :
		m_bHasInfo(false),
		m_bHasRouting(false),
		m_bHasAccount(false),
		m_bHasCheckNumber(false),
		m_bHasAmount(false)
		{};

	int countSubAttributes()
	{
		int count = 0;
		count += m_bHasInfo ? 1 : 0;
		count += m_bHasRouting ? 1 : 0;
		count += m_bHasAccount ? 1 : 0;
		count += m_bHasCheckNumber ? 1 : 0;
		count += m_bHasAmount ? 1 : 0;
		return count;
	};

	bool m_bHasInfo;
	MicrZone m_Info;

	bool m_bHasRouting;
	MicrZone m_Routing;

	bool m_bHasAccount;
	MicrZone m_Account;

	bool m_bHasCheckNumber;
	MicrZone m_CheckNumber;
	
	bool m_bHasAmount;
	MicrZone m_Amount;
};
//-------------------------------------------------------------------------------------------------
// Defines all the MICR objects for a particular page
struct MicrPage
{
	MicrPage(long lWidth, long lHeight) :
		m_lWidth(lWidth),
		m_lHeight(lHeight)
		{};

	long m_lWidth;
	long m_lHeight;

	vector<MicrLine> m_vecMicrLines;
};

/////////////////////////////////////////////////////////////////////////////
// CMicrFinder
class ATL_NO_VTABLE CMicrFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMicrFinder, &CLSID_MicrFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<IMicrFinder, &IID_IMicrFinder, &LIBID_UCLID_AFVALUEFINDERSLib, 1, 0>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib, 1>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib, 1>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib, 1>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib, 1>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CMicrFinder>
{
public:
	CMicrFinder();
	~CMicrFinder();

DECLARE_REGISTRY_RESOURCEID(IDR_MICRFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMicrFinder)
	COM_INTERFACE_ENTRY(IMicrFinder)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeFindingRule)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

	BEGIN_PROP_MAP(CMicrFinder)
		PROP_PAGE(CLSID_MicrFinderPP)
	END_PROP_MAP()

BEGIN_CATEGORY_MAP(CMicrFinder)
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

// IMicrFinder
	STDMETHOD(get_SplitRoutingNumber)(VARIANT_BOOL *pVal);
	STDMETHOD(put_SplitRoutingNumber)(VARIANT_BOOL newVal);
	STDMETHOD(get_SplitAccountNumber)(VARIANT_BOOL *pVal);
	STDMETHOD(put_SplitAccountNumber)(VARIANT_BOOL newVal);
	STDMETHOD(get_SplitCheckNumber)(VARIANT_BOOL *pVal);
	STDMETHOD(put_SplitCheckNumber)(VARIANT_BOOL newVal);
	STDMETHOD(get_SplitAmount)(VARIANT_BOOL *pVal);
	STDMETHOD(put_SplitAmount)(VARIANT_BOOL newVal);
	STDMETHOD(get_Rotate0)(VARIANT_BOOL *pVal);
	STDMETHOD(put_Rotate0)(VARIANT_BOOL newVal);
	STDMETHOD(get_Rotate90)(VARIANT_BOOL *pVal);
	STDMETHOD(put_Rotate90)(VARIANT_BOOL newVal);
	STDMETHOD(get_Rotate180)(VARIANT_BOOL *pVal);
	STDMETHOD(put_Rotate180)(VARIANT_BOOL newVal);
	STDMETHOD(get_Rotate270)(VARIANT_BOOL *pVal);
	STDMETHOD(put_Rotate270)(VARIANT_BOOL newVal);

private:
	bool m_bDirty;

	// Variables to determine whether to split the found MICR text into
	// specific subattributes
	bool m_bSplitRoutingNumber;
	bool m_bSplitAccountNumber;
	bool m_bSplitCheckNumber;
	bool m_bSplitAmount;

	// Map to determine whether a particular image rotation should be applied when
	// searching for MICR
	map<int, bool> m_mapRotations;

	IMiscUtilsPtr m_ipMiscUtils;

	// Use CachedObjectFromFile so that the regular expression is re-loaded from disk only when the
	// RegEx file is modified.
	CachedObjectFromFile<string, RegExLoader> m_cachedRegExLoader;

	//----------------------------------------------------------------------------------------------
	// Methods
	//----------------------------------------------------------------------------------------------
	// Searches each page in the vector of pages and attempts to find all the MICR zones
	void findMICRZones(ISpatialStringPtr ipSpatialString, const vector<long>& vecPages,
		IIUnknownVectorPtr ipAttributes);
	//----------------------------------------------------------------------------------------------
	// Builds a new attribute from a MICR line
	// ARGS:
	// zoneRect			- The bounding rectangle for the MICR zone
	// ipMap			- A spatial page info map to associate with the attribute
	// lPage			- The page number the zone should be created on
	// strMicrText		- The text of the MICR zone
	// strSourceImage	- The source document for the zone
	// strAttributeName - Optional name that will be applied to the attribute
	IAttributePtr buildAttribute(const RECT& zoneRect, ILongToObjectMapPtr ipMap,
		long lPage, string strMicrText, const string& strSourceImage,
		const string& strAttributeName = "");
	//----------------------------------------------------------------------------------------------
	// Fills the specified MicrZone from the supplied _CcMicrInfo object with the specified
	// rotation (0, 90, 180, 270). If the _CcMicrInfo objects text is empty or the
	// rectangle would be either widthless or heightless it will not fill the MicrZone and
	// will return false, otherwise returns true.
	bool buildMicrZone(_CcMicrInfoPtr ipMicrInfo, int nRotation, long lWidth, long lHeight,
		MicrZone& rMicrZone);
	//----------------------------------------------------------------------------------------------
	// Builds an IUnknownVector of attributes from the provided map of MicrPage's.
	IIUnknownVectorPtr buildAttributesFromPages(const map<long, MicrPage>& mapPages,
											const string& strImageName, ISpatialStringPtr ipSS);
	//----------------------------------------------------------------------------------------------
	// Searches the specified rectangle for the other routing number.
	// The search first tries to match the text exactly by splitting the routing number,
	// if that doesn't work it attempts a regular expression search in the specified
	// area.
	IAttributePtr findOtherRoutingNumber(ISpatialStringPtr ipSpatialString,
		const string& strRoutingNumber, const RECT& rectToSearch);
	//----------------------------------------------------------------------------------------------
	// Gets/creates a regex parser for finding the other routing number
	IRegularExprParserPtr getOtherRoutingNumberRegexParser();
	//----------------------------------------------------------------------------------------------
	// Resets the settings for the MICR finder object to defaults
	void resetSettings();
	//----------------------------------------------------------------------------------------------
	// Reads the value of the finding flags from the registry key [FlexIDSCore #3490]
	EMicrReaderFlags getMicrReaderFlags();
	//----------------------------------------------------------------------------------------------
	// Validates the license for this object
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(MicrFinder), CMicrFinder)