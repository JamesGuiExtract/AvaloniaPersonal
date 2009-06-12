// ModifyAttributeValueOH.h : Declaration of the CModifyAttributeValueOH

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CModifyAttributeValueOH
class ATL_NO_VTABLE CModifyAttributeValueOH : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CModifyAttributeValueOH, &CLSID_ModifyAttributeValueOH>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IModifyAttributeValueOH, &IID_IModifyAttributeValueOH, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CModifyAttributeValueOH>
{
public:
	CModifyAttributeValueOH();

DECLARE_REGISTRY_RESOURCEID(IDR_MODIFYATTRIBUTEVALUEOH)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CModifyAttributeValueOH)
	COM_INTERFACE_ENTRY(IModifyAttributeValueOH)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IModifyAttributeValueOH)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CModifyAttributeValueOH)
	PROP_PAGE(CLSID_ModifyAttributeValuePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CModifyAttributeValueOH)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IModifyAttributeValueOH
	STDMETHOD(get_AttributeName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_AttributeName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_AttributeValue)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_AttributeValue)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_AttributeType)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_AttributeType)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_SetAttributeName)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_SetAttributeName)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_SetAttributeValue)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_SetAttributeValue)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_SetAttributeType)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_SetAttributeType)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_AttributeQuery)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_AttributeQuery)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_CreateSubAttribute)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_CreateSubAttribute)(/*[in]*/ VARIANT_BOOL newVal);
	

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector *pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	///////
	// Data
	///////

	// flag to keep track of whether object is dirty
	bool		m_bDirty;

	// Name of Attribute(s) to be affected
	// May also be a query to find the affected Attribute(s)
	std::string	m_strAttributeQuery;

	// New type for affected Attribute(s)
	std::string m_strAttributeName;
	bool		m_bSetAttributeName;

	// New value for affected Attribute(s)
	std::string m_strAttributeValue;
	bool		m_bSetAttributeValue;

	// New type for affected Attribute(s)
	std::string m_strAttributeType;
	bool		m_bSetAttributeType;

	bool m_bCreateSubAttribute;

	IAFUtilityPtr m_ipAFUtil;

	//////////
	// Methods
	//////////

	// Check licensing
	void		validateLicense();
};
