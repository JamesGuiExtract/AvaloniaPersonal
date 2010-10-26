// MergeAttributes.h : Declaration of the CMergeAttributes

#pragma once
#include "resource.h"
#include "AFOutputHandlers.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <vector>
#include <map>
#include <set>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CMergeAttributes
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CMergeAttributes :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMergeAttributes, &CLSID_MergeAttributes>,
	public IDispatchImpl<IMergeAttributes, &IID_IMergeAttributes, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CMergeAttributes>
{
public:
	CMergeAttributes();
	~CMergeAttributes();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_MERGEATTRIBUTES)

	BEGIN_COM_MAP(CMergeAttributes)
		COM_INTERFACE_ENTRY(IMergeAttributes)
		COM_INTERFACE_ENTRY2(IDispatch, IMergeAttributes)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IOutputHandler)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CMergeAttributes)
		PROP_PAGE(CLSID_MergeAttributesPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CMergeAttributes)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
	END_CATEGORY_MAP()

// IMergeAttributes
	STDMETHOD(get_AttributeQuery)(BSTR *pVal);
	STDMETHOD(put_AttributeQuery)(BSTR newVal);
	STDMETHOD(get_OverlapPercent)(double *pVal);
	STDMETHOD(put_OverlapPercent)(double newVal);
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

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector *pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

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
	// Variables
	/////////////////

	bool m_bDirty;

	// A query to define the domain of attributes which may be merged with one another
	string m_strAttributeQuery;

	// The percentage of mutual overlap two attributes must have to qualify to be merged.
	double m_dOverlapPercent;

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

	// The text associated with the document being processed.
	ISpatialStringPtr m_ipDocText;

	// A cache of spatial page infos with skew and orientation removed so that bounding rectangles
	// of merged values (which are returned in terms of literal page coordinates and not the page
	// info) appear at the correct location.
	map<long, ISpatialPageInfoPtr> m_mapSpatialInfos;

	// An AFUtility instance to execute attribute queries.
	IAFUtilityPtr m_ipAFUtility;

	/////////////////
	// Methods
	/////////////////

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
	IAttributePtr mergeAttributes(IAttributePtr ipAttribute1, IAttributePtr ipAttribute2, long nPage,
								  IAttributePtr &ipRemovedAttribute);

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
	void removeInvalidResults(IIUnknownVectorPtr ipMergeResults);

	// Uses ipMergeResults to adjust the original list represented by ipAttributes.  This includes
	// removing the attributes that have been merged into a merge result.
	void applyResults(IIUnknownVectorPtr ipMergeResults, IIUnknownVectorPtr ipAttributes);

	// Removes the specified attribute from the specified list. If nPage is provided, 
	// the key for ipAttribute on nPage is removed from m_mapChildToParentAttributes. If
	// nPage is not provided all keys for ipAttribute are removed.  The return value
	// is the index that was removed from ipAttributeList.
	long removeAttribute(IAttributePtr ipAttribute, IIUnknownVectorPtr ipAttributeList, 
						 long nPage = -1);

	// Creates a raster zone for the specified area on the specified page.
	IRasterZonePtr createRasterZone(CRect rect, long nPage);

	// Resets the rule's settings.
	void reset();

	// Validate license.
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(MergeAttributes), CMergeAttributes)
