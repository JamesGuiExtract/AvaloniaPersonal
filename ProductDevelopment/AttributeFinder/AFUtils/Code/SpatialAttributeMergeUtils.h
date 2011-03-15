#pragma once
#include "resource.h"
#include "AFUtils.h"

#include <string>
#include <vector>
#include <map>
#include <set>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CSpatialAttributeMergeUtils
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CSpatialAttributeMergeUtils :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpatialAttributeMergeUtils, &CLSID_SpatialAttributeMergeUtils>,
	public IDispatchImpl<ISpatialAttributeMergeUtils, &IID_ISpatialAttributeMergeUtils, &LIBID_UCLID_AFUTILSLib>,
	public ISupportErrorInfo
{
	public:
	CSpatialAttributeMergeUtils();
	~CSpatialAttributeMergeUtils();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALATTRIBUTEMERGEUTILS)

	BEGIN_COM_MAP(CSpatialAttributeMergeUtils)
		COM_INTERFACE_ENTRY(ISpatialAttributeMergeUtils)
		COM_INTERFACE_ENTRY2(IDispatch, ISpatialAttributeMergeUtils)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
	END_COM_MAP()

	// ISpatialAttributeMergeUtils
	STDMETHOD(get_OverlapPercent)(double *pVal);
	STDMETHOD(put_OverlapPercent)(double newVal);
	STDMETHOD(get_UseMutualOverlap)(VARIANT_BOOL *pVal);
	STDMETHOD(put_UseMutualOverlap)(VARIANT_BOOL newVal);
	STDMETHOD(get_NameMergeMode)(EFieldMergeMode *pVal);
	STDMETHOD(put_NameMergeMode)(EFieldMergeMode newVal);
	STDMETHOD(get_TypeMergeMode)(EFieldMergeMode *pVal);
	STDMETHOD(put_TypeMergeMode)(EFieldMergeMode newVal);
	STDMETHOD(get_SpecifiedName)(BSTR *pVal);
	STDMETHOD(put_SpecifiedName)(BSTR newVal);
	STDMETHOD(get_SpecifiedType)(BSTR *pVal);
	STDMETHOD(put_SpecifiedType)(BSTR newVal);
	STDMETHOD(get_SpecifiedValue)(BSTR *pVal);
	STDMETHOD(put_SpecifiedValue)(BSTR newVal);
	STDMETHOD(get_NameMergePriority)(IVariantVector **ppVal);
	STDMETHOD(put_NameMergePriority)(IVariantVector *pNewVal);
	STDMETHOD(get_PreserveAsSubAttributes)(VARIANT_BOOL *pVal);
	STDMETHOD(put_PreserveAsSubAttributes)(VARIANT_BOOL newVal);
	STDMETHOD(get_CreateMergedRegion)(VARIANT_BOOL *pVal);
	STDMETHOD(put_CreateMergedRegion)(VARIANT_BOOL newVal);
	STDMETHOD(FindQualifiedMerges)(IIUnknownVector* pAttributes, ISpatialString *pDocText);
	STDMETHOD(CompareAttributeSets)(IIUnknownVector* pvecAttributeSet1,
		IIUnknownVector* pvecAttributeSet2, ISpatialString *pDocText,
		VARIANT_BOOL* pvbMatching);
	STDMETHOD(ApplyMerges)(IIUnknownVector* pvecAttributeSet);

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

private:

	/////////////////
	// Variables
	/////////////////

	// The percentage of mutual overlap two attributes must have to qualify to be merged.
	double m_dOverlapPercent;

	// If true, OverlapPercent refers to the minimum percentage overlap when compared both ways;
	// If false, OverlapPercent refers to the maximum percentage overlap.
	bool m_bUseMutualOverlap;

	// Specifies the method that should be used to determine the name of the merged attribute.
	EFieldMergeMode m_eNameMergeMode;

	// Specifies the method that should be used to determine the type of the merged attribute.
	EFieldMergeMode m_eTypeMergeMode;

	// If m_eNameMergeMode == kSpecifyField, merged attributes will be assigned this value.
	string m_strSpecifiedName;

	// If m_eTypeMergeMode == kSpecifyField, merged attributes will be assigned this value.
	string m_strSpecifiedType;

	// The text of merged attributes will be assigned this value.
	string m_strSpecifiedValue;

	// If NameMergeMode == kPreserveField, this specifies the priority in which names should be preserved.
	IVariantVectorPtr m_ipNameMergePriority;

	// If true, the original attributes be added as as sub-attributes to the merged value.
	bool m_bPreserveAsSubAttributes;

	// If true, the resulting attributes will be a unification of the overall region the attributes
	// occupy, otherwise the attribute raster zones will be merged on an individual basis.
	bool m_bCreateMergedRegion;

	// Define type to represent the portion of an attribute that is on a given page and 
	typedef pair<IAttribute*, long> AttributePage;
	// Define a map to Keep track of the merged attribute each AttributePage belongs to.
	typedef map<AttributePage, IAttributePtr> AttributeMap;
	AttributeMap m_mapChildToParentAttributes;

	// A structure to cache spatial information for an attribute
	struct AttributeInfo
	{
		set<long> setPages;
		map<long, vector<CRect> > mapRasterZones;
	};

	// A cache of spatial information for all eligible attributes.
	map<IAttribute*, AttributeInfo> m_mapAttributeInfo;

	// The set of qualified merges that have been found via FindQualifiedMerges.
	IIUnknownVectorPtr m_ipQualifiedMerges;

	// The text associated with the document being processed.
	ISpatialStringPtr m_ipDocText;

	// A cache of spatial page infos with skew and orientation removed so that bounding rectangles
	// of merged values (which are returned in terms of literal page coordinates and not the page
	// info) appear at the correct location.
	map<long, ISpatialPageInfoPtr> m_mapSpatialInfos;

	/////////////////
	// Methods
	/////////////////

	// Resets all existing comparison results in preperation for a new comparison.
	void initialize(IIUnknownVectorPtr ipAttributeSet1,
		IIUnknownVectorPtr ipAttributeSet2 = __nullptr);

	// Finds all attributes qualified to be merged from the two sets based on the current settings.
	// The sets may be the same vector to find all qualified merges within a single set.
	// NOTE: The input vectors will not be modified by this call. applyResults must be called to
	// use the qualified merges that were found.
	void findQualifiedMerges(IIUnknownVectorPtr ipAttributeSet1, IIUnknownVectorPtr ipAttributeSet2);

	// Returns a set of pages representing all pages in which ipAttribute1 and ipAttribute2 overlap
	// at least the amount specified by m_dOverlapPercent.
	set<long> getPagesWithOverlap(IAttributePtr ipAttribute1, IAttributePtr ipAttribute2);

	// Return a set of pages common to both attributes. (regardless of whether they overlap)
	set<long> getAttributePages(IAttributePtr ipAttribute1, IAttributePtr ipAttribute2);

	// Calculates that percentage of mutual overlap between ipAttribute1 and ipAttribute2 on the
	// specified page.
	double calculateOverlap(IAttributePtr ipAttribute1, IAttributePtr ipAttribute2, long nPage);

	// Load page & spatial information from this ipAttribute into the m_mapAttributeInfo info cache.
	void loadAttributeInfo(IAttributePtr ipAttribute);

	// Combines ipAttribute1 and ipAttribute2 on nPage and returns the merged result. If
	// both attributes were part of separate existing merged results, ipRemovedAttribute will
	// identify the merged result that should no longer be used.
	IAttributePtr mergeAttributes(IAttributePtr ipAttribute1, IAttributePtr ipAttribute2,
		long nPage, IAttributePtr &ipRemovedAttribute);

	// Attributes ripAttribute1 and ripAttribute2 are changed to be any existing merged result 
	// that either already belongs to so that any additional merges are made with the exising merge
	// result. The return value is the attribute which the attributes should be merged
	// into. This will be an existing merged result if either of these attributes were already
	// merged, or a new attribute if this is the first merge for either attribute. NULL
	// with be returned if both attributes have already been merged into the same result.
	IAttributePtr getMergeTarget(IAttributePtr &ripAttribute1, IAttributePtr &ripAttribute2, 
								 long nPage);

	// Merges ipAttribute1 and ipAttribute2 into ipMergedAttribute on the specified page.
	void mergeAttributePair(IAttributePtr ipMergedAttribute, IAttributePtr ipAttribute1, 
							IAttributePtr ipAttribute2, long nPage);

	// Creates and returns the spatial string value to be assigned to the merged result of 
	// ipAttribute1 and ipAttribute2.
	ISpatialStringPtr createMergedValue(IAttributePtr ipAttribute1, IAttributePtr ipAttribute2, 
										long nPage);

	// Given bstrValueA and bstrValueB, rbstrResult is set to the value that should be
	// preserved given the values in ipValuePriorityList. false if returned if neither
	// value can be found in ipValuePriorityList. 
	bool getValueToPreserve(_bstr_t bstrValueA, _bstr_t bstrValueB, 
							IVariantVectorPtr ipValuePriorityList, _bstr_t &rbstrResult);

	// If m_bPreserveAsSubAttributes is true, all sub-attributes of ipAttribute are added as
	// sub-attributes of ipMergeResult. In either case, if ipAttribute is a result of a previous
	// merge, all links to it from its original attributes in m_mapChildToParentAttributes will
	// be updated to associate them with ipMergeResult instead. true will be returned if 
	// ipAttribute is no longer needed, otherwise false will be returned.
	bool associateAttributeWithResult(IAttributePtr ipAttribute, 
									  IAttributePtr ipMergeResult,
									  long nPage);

	// Removes invalid merge results and logs appropriate exceptions.
	void removeInvalidResults();

	// Uses ipMergeResults to adjust the original list represented by ipAttributes.  This includes
	// removing the attributes that have been merged into a merge result.
	void applyResults(IIUnknownVectorPtr ipAttributes);

	// Removes the specified attribute from the specified list. If nPage is provided, 
	// the key for ipAttribute on nPage is removed from m_mapChildToParentAttributes. If
	// nPage is not provided all keys for ipAttribute are removed.  The return value
	// is the index that was removed from ipAttributeList.
	long removeAttribute(IAttributePtr ipAttribute, IIUnknownVectorPtr ipAttributeList, 
						 long nPage = -1);

	// Creates a raster zone for the specified area on the specified page.
	IRasterZonePtr createRasterZone(CRect rect, long nPage);

	// Checks to see if all spatial attributes in the provided vector have been merged into the
	// current set of qualified merges.
	bool areAllAttributesMerged(IIUnknownVectorPtr ipAttributes);

	// Validate license.
	void validateLicense();	
};

OBJECT_ENTRY_AUTO(__uuidof(SpatialAttributeMergeUtils), CSpatialAttributeMergeUtils)