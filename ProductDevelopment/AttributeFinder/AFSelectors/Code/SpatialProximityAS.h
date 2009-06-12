// SpatialProximityAS.h : Declaration of the CSpatialProximityAS

#pragma once
#include "resource.h"       // main symbols
#include "AFSelectors.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <vector>
#include <map>
#include <utility>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CSpatialProximityAS
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CSpatialProximityAS :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpatialProximityAS, &CLSID_SpatialProximityAS>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ISpatialProximityAS, &IID_ISpatialProximityAS, &LIBID_UCLID_AFSELECTORSLib>,
	public IDispatchImpl<IAttributeSelector, &IID_ISpatialProximityAS, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CSpatialProximityAS>
{
public:
	CSpatialProximityAS();
	~CSpatialProximityAS();
	
	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALPROXIMITYAS)

	BEGIN_COM_MAP(CSpatialProximityAS)
		COM_INTERFACE_ENTRY(ISpatialProximityAS)
		COM_INTERFACE_ENTRY2(IDispatch, ISpatialProximityAS)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IAttributeSelector)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CSpatialProximityAS)
		PROP_PAGE(CLSID_SpatialProximityASPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CSpatialProximityAS)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SELECTORS)
	END_CATEGORY_MAP()

// ISpatialProximityAS
	STDMETHOD(get_TargetQuery)(BSTR* pVal);
	STDMETHOD(put_TargetQuery)(BSTR newVal);
	STDMETHOD(get_ReferenceQuery)(BSTR* pVal);
	STDMETHOD(put_ReferenceQuery)(BSTR newVal);
	STDMETHOD(get_RequireCompleteInclusion)(VARIANT_BOOL* pVal);
	STDMETHOD(put_RequireCompleteInclusion)(VARIANT_BOOL newVal);
	STDMETHOD(get_TargetsMustContainReferences)(VARIANT_BOOL* pVal);
	STDMETHOD(put_TargetsMustContainReferences)(VARIANT_BOOL newVal);
	STDMETHOD(get_CompareLinesSeparately)(VARIANT_BOOL* pVal);
	STDMETHOD(put_CompareLinesSeparately)(VARIANT_BOOL newVal);
	STDMETHOD(SetRegionBorder)(EBorder eRegionBorder, EBorderRelation eRelation, 
		EBorder eRelationBorder, EBorderExpandDirection eExpandDirection, double dExpandAmount,
		EUnits eUnits);
	STDMETHOD(GetRegionBorder)(EBorder eRegionBorder, EBorderRelation *peRelation,  
		EBorder *peRelationBorder, EBorderExpandDirection *peExpandDirection, double *pdExpandAmount, 
		EUnits *peUnits);
	STDMETHOD(get_IncludeDebugAttributes)(VARIANT_BOOL* pVal);
	STDMETHOD(put_IncludeDebugAttributes)(VARIANT_BOOL newVal);

// IAttributeSelector
	STDMETHOD(raw_SelectAttributes)(IIUnknownVector *pAttrIn, IAFDocument *pAFDoc, 
		IIUnknownVector **pAttrOut);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR *pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL *pbValue);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

private:

	/////////////////
	// Structs
	/////////////////

	// Defines a region border
	struct BorderInfo
	{
		BorderInfo()
			: m_eBorder(kNoBorder),
			  m_eRelation(kReferenceAttibute),	
			  m_eExpandDirection(kNoDirection),
			  m_dExpandAmount(0),
			  m_eUnits(kInches)
		{
		}
			
		BorderInfo(EBorder eBorder, EBorderRelation eBorderRelation, 
			       EBorderExpandDirection eBorderExpandDirection, double dExpandAmount, 
				   EUnits eUnits)
			: m_eBorder(eBorder),
			  m_eRelation(eBorderRelation),	
			  m_eExpandDirection(eBorderExpandDirection),
			  m_dExpandAmount(dExpandAmount),
			  m_eUnits(eUnits)
		{
		}

		// Is it going to be the top, bottom, left or right of the reference attribute or page?
		EBorder m_eBorder;
		// Is this border to be relative to a reference attribute or the document page?
		EBorderRelation m_eRelation;
		// Which direction should the border be expanded?
		EBorderExpandDirection m_eExpandDirection;
		// How many SpatialLines|Characters|Inches|Pixels should we expand?
		double m_dExpandAmount;
		// The units used by m_dExpandAmount.
		EUnits m_eUnits;
	};
	
	/////////////////
	// Variables
	/////////////////

	bool m_bDirty;

	// The query defining the domain of attributes that may be selected.
	string m_strTargetQuery;

	// The query defining the attributes used to describe the location target attributes must
	// occupy to be selected.
	string m_strReferenceQuery;

	// If true, attributes must entirely contain or be contained in the described spatial 
	// region in order to be selected.
	bool m_bRequireCompleteInclusion;

	// If true, selected attributes will need to contain the described spatial region.  If
	// false, selected attributes will need to be contained in the described spatial region.
	bool m_bTargetsMustContainReferences;

	// If true, each line of the reference attributes will be used separately to describe
	// the location selected attributes must occupy.  If false, the unified area (except when
	// divided by a page boundary) will be used.
	bool m_bCompareLinesSeparately;

	// If true, rather than select qualifying attributes, sub-attributes will be added to the 
	// reference attributes showing the region that would be used to qualify selected attributes.
	bool m_bIncludeDebugAttributes;

	// Settings to describe the borders of the target region in relation to the reference attribute.
	map<EBorder, BorderInfo> m_mapBorderInfo;

	// The text of the document currently being processed
	ISpatialStringPtr m_ipDocText;

	// The DPI of the document currently being processed.
	int m_nXResolution;
	int m_nYResolution;

	// An AFUtility instance to execute attribute queries.
	IAFUtilityPtr m_ipAFUtility;
	

	/////////////////
	// Methods
	/////////////////

	// Retrieves a CRect describing the attribute's location and a long indicating the page
	// the rect belongs to. If bSeparateLines == true, a separate CRect pagenum pair will be
	// returned for each line of the attribute,  If bSeparateLines == false, multiple pairs
	// will be returned only if the attribute spans pages.
	vector< pair<CRect, long> > getAttributeRects(IAttributePtr ipAttribute, bool bSeparateLines);

	// The given rectangle representing a reference attribute is converted to the region to
	// search for target attributes using the rules border settings.  Returns true if successful,
	// false if the result is not a valid page area (no area or completely off-page)
	bool convertToTargetRegion(CRect &rrect, long nPage);

	// Retrives a reference to the specified border of rect.
	long &getRectBorder(CRect &rect, EBorder eBorder);

	// Given the border settings specified by borderInfo and the page being searched, determines
	// the number of pixels the border should be expanded (negative for left or up).
	long getExpansionOffset(const BorderInfo &borderInfo, long nPage);

	// Searches for all attributes in ipContainedAttributes that are completely contained
	// in an attribute from ipContainerAttributes. Each such pair that is found is added
	// as an item in the return result vector. The pairings that are made adhere to the 
	// m_bRequireCompleteInclusion and m_bCompareLinesSeparately settings.
	vector< pair<IAttributePtr, IAttributePtr> > findContainmentPairs(
		IIUnknownVectorPtr ipContainerAttributes, IIUnknownVectorPtr ipContainedAttributes);

	// Returns true if ipContainedAttribute is contained in the area described by 
	// rectContainerArea and nPage. The result is dependent upon the m_bCompareLinesSeparately
	// setting.
	bool isAttributeContainedIn(IAttributePtr ipContainedAttribute,
							    const CRect &rectContainerArea, long nPage);

	// Adds the rule's defined reference spatial regions to be tested as subattributes to the
	// attributes used to describe the regions.
	void createDebugAttributes(IIUnknownVectorPtr ipReferenceAttributes);

	// Adds the defined reference spatial regions for ipAttribute as subattributes to ipAttribute.
	void createDebugAttributes(IAttributePtr ipAttribute,  const CRect &rectReference, long nPage);
	
	// Resets the rule's settings.
	void reset();

	// Validate license.
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(SpatialProximityAS), CSpatialProximityAS)
