// MoveAndModifyAttributes.h : Declaration of the CMoveAndModifyAttributes

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
using std::string;


/////////////////////////////////////////////////////////////////////////////
// CMoveAndModifyAttributes
class ATL_NO_VTABLE CMoveAndModifyAttributes : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMoveAndModifyAttributes, &CLSID_MoveAndModifyAttributes>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IMoveAndModifyAttributes, &IID_IMoveAndModifyAttributes, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CMoveAndModifyAttributes>
{
public:
	CMoveAndModifyAttributes();
	~CMoveAndModifyAttributes();


DECLARE_REGISTRY_RESOURCEID(IDR_MOVEANDMODIFYATTRIBUTES)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMoveAndModifyAttributes)
	COM_INTERFACE_ENTRY(IMoveAndModifyAttributes)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IMoveAndModifyAttributes)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CMoveAndModifyAttributes)
	PROP_PAGE(CLSID_MoveAndModifyAttributesPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CMoveAndModifyAttributes)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IMoveAndModifyAttributes
	STDMETHOD(get_AttributeQuery)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_AttributeQuery)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_OverwriteAttributeName)(/*[out, retval]*/ EOverwriteAttributeName *pVal);
	STDMETHOD(put_OverwriteAttributeName)(/*[in]*/ EOverwriteAttributeName newVal);
	STDMETHOD(get_SpecifiedAttributeName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_SpecifiedAttributeName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_RetainAttributeType)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_RetainAttributeType)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_AddRootOrParentAttributeType)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_AddRootOrParentAttributeType)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_AddSpecifiedAttributeType)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_AddSpecifiedAttributeType)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_SpecifiedAttributeType)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_SpecifiedAttributeType)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_DeleteRootOrParentIfAllChildrenMoved)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_DeleteRootOrParentIfAllChildrenMoved)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_AddAttributeNameToType)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_AddAttributeNameToType)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_MoveAttributeLevel)(/*[out, retval]*/ EMoveAttributeLevel *pVal);
	STDMETHOD(put_MoveAttributeLevel)(/*[in]*/ EMoveAttributeLevel newVal);
	

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector *pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
////////////
// Variables
////////////
	// the x-path query that will be used to find attributes
	string m_strQuery;

	// to which level should the attribute be moved
	EMoveAttributeLevel m_eMoveAttributeLevel;

	// How to specify a moved attributes new name
	EOverwriteAttributeName m_eOverwriteAttributeName;
	string m_strSpecifiedName;

	// How to specify a moved attributes new type
	bool m_bRetainCurrType;
	bool m_bAddRootOrParentType;
	bool m_bAddNameToType;
	bool m_bAddSpecified;
	string m_strSpecifiedType;

	// whether or not to delete affected root/parent attribute if childless
	bool m_bDeleteRootOrParentIfAllChildrenMoved;
	
	IAFUtilityPtr m_ipAFUtility;

	// Used by IPersistStream
	bool m_bDirty;

////////////
// Methods
////////////

	// Depends on the value of m_eMoveAttributeLevel, return the root or parent 
	// attribute of specified child attribute
	IAttributePtr getRootOrParentAttribute(IIUnknownVectorPtr ipAttributes, IAttributePtr ipChild);

	// Modify attribute name
	void modifyAttributeNames(IIUnknownVectorPtr ipTrees, IIUnknownVectorPtr ipAttributes);

	// Modify attribute type
	void modifyAttributeTypes(IIUnknownVectorPtr ipTrees, IIUnknownVectorPtr ipAttributes);

	// Move found attributes either to root level or up one level
	void moveAttributes(IIUnknownVectorPtr ipRootLevelAttributes, IIUnknownVectorPtr ipFoundAttributes);

	void validateLicense();
};

