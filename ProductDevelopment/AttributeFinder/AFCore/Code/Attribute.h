// Attribute.h : Declaration of the CAttribute

#pragma once

#include "resource.h"       // main symbols

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CAttribute
class ATL_NO_VTABLE CAttribute : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAttribute, &CLSID_Attribute>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IAttribute, &IID_IAttribute, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IComparableObject, &IID_IComparableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream
{
public:
	CAttribute();
	~CAttribute();

DECLARE_REGISTRY_RESOURCEID(IDR_ATTRIBUTE)

DECLARE_PROTECT_FINAL_CONSTRUCT()
	HRESULT FinalConstruct();
	void FinalRelease();

BEGIN_COM_MAP(CAttribute)
	COM_INTERFACE_ENTRY(IAttribute)
	COM_INTERFACE_ENTRY2(IDispatch, IAttribute)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IComparableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttribute
	STDMETHOD(get_InputValidator)(/*[out, retval]*/ IInputValidator **pVal);
	STDMETHOD(put_InputValidator)(/*[in]*/ IInputValidator* newVal);
	STDMETHOD(get_Value)(/*[out, retval]*/ ISpatialString **pVal);
	STDMETHOD(put_Value)(/*[in]*/ ISpatialString* newVal);
	STDMETHOD(get_Name)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Name)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_SubAttributes)(/*[out, retval]*/ IIUnknownVector* *pVal);
	STDMETHOD(put_SubAttributes)(/*[in]*/ IIUnknownVector *pNewVal);
	STDMETHOD(get_AttributeSplitter)(/*[out, retval]*/ IAttributeSplitter* *pVal);
	STDMETHOD(put_AttributeSplitter)(/*[in]*/ IAttributeSplitter *pNewVal);
	STDMETHOD(get_Type)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Type)(/*[in]*/ BSTR newVal);
	STDMETHOD(AddType)(/*[in]*/ BSTR newVal);
	STDMETHOD(ContainsType)(/*[in]*/ BSTR strType, /*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(IsNonSpatialMatch)(/*[in]*/ IAttribute* pTest, /*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(get_DataObject)(/*[out, retval]*/ IUnknown **pVal);
	STDMETHOD(put_DataObject)(/*[in]*/ IUnknown* newVal);
	STDMETHOD(GetAttributeSize)(long* plAttributeSize);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown ** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

// IComparableObject
	STDMETHOD(raw_IsEqualTo)(IUnknown * pObj, VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	//////////////
	// Variables
	//////////////
	UCLID_AFCORELib::IAttributeSplitterPtr m_ipAttributeSplitter;
	IInputValidatorPtr m_ipInputValidator;
	IIUnknownVectorPtr m_ipSubAttributes;
	ISpatialStringPtr m_ipAttributeValue;
	string m_strAttributeName;
	string m_strAttributeType;
	IUnknownPtr m_ipDataObject;

	// flag to keep track of whether this object has been modified
	// since the last save-to-stream operation
	bool m_bDirty;

	//////////////
	// Methods
	//////////////
	
	// true if the attribute already contains all types specified by strType.
	bool containsType(string strType);

	// Validates strName as valid Identifier.  This method is used to 
	// validate both Names and Types
	void validateIdentifier(const string& strName);

	// Gets the sub attribute collection (will create an empty one if it doesn't exist)
	IIUnknownVectorPtr getSubAttributes();

	void validateLicense();
};
