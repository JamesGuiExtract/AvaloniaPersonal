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
	STDMETHOD(get_TreatNameListAsRegex)(VARIANT_BOOL *pVal);
	STDMETHOD(put_TreatNameListAsRegex)(VARIANT_BOOL newVal);
	STDMETHOD(get_ValueMergeMode)(EFieldMergeMode *pVal);
	STDMETHOD(put_ValueMergeMode)(EFieldMergeMode newVal);
	STDMETHOD(get_ValueMergePriority)(IVariantVector **ppVal);
	STDMETHOD(put_ValueMergePriority)(IVariantVector *pNewVal);
	STDMETHOD(get_TreatValueListAsRegex)(VARIANT_BOOL *pVal);
	STDMETHOD(put_TreatValueListAsRegex)(VARIANT_BOOL newVal);
	STDMETHOD(get_TypeFromName)(VARIANT_BOOL *pVal);
	STDMETHOD(put_TypeFromName)(VARIANT_BOOL newVal);
	STDMETHOD(get_PreserveType)(VARIANT_BOOL *pVal);
	STDMETHOD(put_PreserveType)(VARIANT_BOOL newVal);
	STDMETHOD(get_TypeMergePriority)(IVariantVector **ppVal);
	STDMETHOD(put_TypeMergePriority)(IVariantVector *pNewVal);
	STDMETHOD(get_TreatTypeListAsRegex)(VARIANT_BOOL *pVal);
	STDMETHOD(put_TreatTypeListAsRegex)(VARIANT_BOOL newVal);

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

	// If true, the values in m_ipNameMergePriority will be treated as regular expressions.
	bool m_bTreatNameListAsRegex;

	// Specifies the method that should be used to determine the value text of the merged attribute.
	EFieldMergeMode m_eValueMergeMode;

	// If m_eValueMergeMode == kPreserveField, this specifies the priority in which value text should
	// be preserved.
	IVariantVectorPtr m_ipValueMergePriority;

	// If true, the values in m_ipValueMergePriority will be treated as regular expressions.
	bool m_bTreatValueListAsRegex;

	// If true, the type used will be that of the attribute whose name was chosen. If
	// m_bPreserveType is also specified, m_ipTypeMergePriority will be used as a tiebreaker.
	bool m_bTypeFromName;

	// If true, the kPreserveField merge mode will be used unless m_bTypeFromName is also specified,
	// in which case this will be used as a fallback.
	bool m_bPreserveType;

	// If m_bPreserveType is true, this specifies the priority in which types should be preserved.
	IVariantVectorPtr m_ipTypeMergePriority;

	// If true, the values in m_ipTypeMergePriority will be treated as regular expressions.
	bool m_bTreatTypeListAsRegex;

	// An AFUtility instance to execute attribute queries.
	IAFUtilityPtr m_ipAFUtility;

	// Used to perform the merging of the attributes.
	ISpatialAttributeMergeUtilsPtr m_ipAttributeMerger;

	/////////////////
	// Methods
	/////////////////

	// Resets the rule's settings.
	void reset();

	// Retrieves an ISpatialAttributeMergeUtils instance with the currently configured settings.
	ISpatialAttributeMergeUtilsPtr getAttributeMerger();

	// Validate license.
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(MergeAttributes), CMergeAttributes)
