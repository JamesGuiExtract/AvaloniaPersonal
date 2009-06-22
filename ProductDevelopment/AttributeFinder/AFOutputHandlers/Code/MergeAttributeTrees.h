// MergeAttributeTrees.h : Declaration of the CMergeAttributeTrees

#pragma once
#include "resource.h"
#include "AFOutputHandlers.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <vector>

using namespace std;

//--------------------------------------------------------------------------------------------------
// CMergeAttributeTrees
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CMergeAttributeTrees :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMergeAttributeTrees, &CLSID_MergeAttributeTrees>,
	public IDispatchImpl<IMergeAttributeTrees, &IID_IMergeAttributeTrees, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CMergeAttributeTrees>
{
public:
	CMergeAttributeTrees();
	~CMergeAttributeTrees();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_MERGEATTRIBUTETREES)

	BEGIN_COM_MAP(CMergeAttributeTrees)
		COM_INTERFACE_ENTRY(IMergeAttributeTrees)
		COM_INTERFACE_ENTRY2(IDispatch, IMergeAttributeTrees)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IOutputHandler)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CMergeAttributeTrees)
		PROP_PAGE(CLSID_MergeAttributeTreesPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CMergeAttributeTrees)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
	END_CATEGORY_MAP()

// IMergeAttributeTrees
	STDMETHOD(get_AttributesToBeMerged)(BSTR* pbstrAttributesToBeMerged);
	STDMETHOD(put_AttributesToBeMerged)(BSTR bstrAttributesToBeMerged);
	STDMETHOD(get_MergeAttributeTreesInto)(EMergeAttributeTreesInto* pMergeInto);
	STDMETHOD(put_MergeAttributeTreesInto)(EMergeAttributeTreesInto mergeInto);
	STDMETHOD(get_SubAttributesToCompare)(BSTR* pbstrSubAttributesToCompare);
	STDMETHOD(put_SubAttributesToCompare)(BSTR bstrSubAttributesToCompare);
	STDMETHOD(get_DiscardNonMatchingComparisons)(VARIANT_BOOL* pvbDiscard);
	STDMETHOD(put_DiscardNonMatchingComparisons)(VARIANT_BOOL vbDiscard);
	STDMETHOD(get_CaseSensitive)(VARIANT_BOOL* pvbCaseSensitive);
	STDMETHOD(put_CaseSensitive)(VARIANT_BOOL vbCaseSensitive);
	STDMETHOD(get_CompareTypeInformation)(VARIANT_BOOL* pvbCompareTypeInformation);
	STDMETHOD(get_CompareSubAttributes)(VARIANT_BOOL* pvbCompareSubAttributes);
	// TODO: FUTURE - Allow these settings to be used
	//STDMETHOD(put_CompareTypeInformation)(VARIANT_BOOL vbCompareTypeInformation);
	//STDMETHOD(put_CompareSubAttributes)(VARIANT_BOOL vbCompareSubAttributes);

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

	// A query to define the attributes which will be compared and possibly merged
	string m_strAttributesToBeMerged;

	// The enum indicating which attribute the trees will be merged into
	EMergeAttributeTreesInto m_eMergeInto;

	// The list of sub attributes to compare when computing a match
	vector<string> m_vecSubattributesToCompare;

	// Whether to discard non-matching comparison attributes or preserve them at the end
	// of the merged list
	bool m_bDiscardNonMatch;

	// Whether the comparison should be case sensitive
	bool m_bCaseSensitive;

	// Whether the type information should be used in the compare
	bool m_bCompareTypeInfo;

	// Whether to compare sub attributes
	bool m_bCompareSubAttributes;

	// The AFUtility object to be used in this object
	IAFUtilityPtr m_ipAFUtils;

	/////////////////
	// Methods
	/////////////////

	// Validate license.
	void validateLicense();

	// Sets the sub attribute compare vector from the \r\n tokenized string
	void setSubAttributeComparesFromString(const string& strSubAttributes);

	// Gets the sub attribute compare vector as a \r\n tokenized string
	string getSubAttributeComparesAsString();

	// Compares the sub attribute vectors based on the sub attribute compare strings
	bool compareSubAttributes(IIUnknownVectorPtr ipSubAttributes1,
		IIUnknownVectorPtr ipSubAttributes2);

	// Gets the collection of sub attributes that will either be moved (place at the end of
	// the attribute collection) or removed
	void getAttributesToMoveOrRemove(IIUnknownVectorPtr ipKeep, IIUnknownVectorPtr ipToCheck,
		vector<IAttributePtr>& rvecAttributesToModify);

	// Validates the string (string cannot contain | characters)
	static void validateAttributeString(const string& strSubAttributes);

	// Removes the attributes that have already been merged from the attribute collection
	void removeMergedAttributes(IIUnknownVectorPtr ipAttributes,
		const vector<IAttributePtr>& vecMergedAttributes);

	// Gets the AFUtility pointer for this object
	IAFUtilityPtr getAFUtility();
};

OBJECT_ENTRY_AUTO(__uuidof(MergeAttributeTrees), CMergeAttributeTrees)
