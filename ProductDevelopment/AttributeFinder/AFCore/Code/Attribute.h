// Attribute.h : Declaration of the CAttribute

#pragma once

#include "resource.h"       // main symbols
#include "IdentifiableObject.h"

#include <string>
#include <COMUtilsMethods.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
namespace voa
{
	// Map (transform) a VOA (vector of IAttributePtr) to another vector
	template <typename TOutput>
	const auto map = IUnknownVectorMethods::map<UCLID_AFCORELib::IAttributePtr, TOutput>;

	// Filter a VOA (vector of IAttributePtr) to contain only items where the predicate returns true
	const auto filter = IUnknownVectorMethods::filter<UCLID_AFCORELib::IAttributePtr>;

	// Flatten a vector of vectors of x to be a vector of x
	const auto concat = IUnknownVectorMethods::concat;

	// Filter a vector so that it contains only the non-null results of the specified function (map + filter for non-null)
	template <typename TOutput>
	const auto choose = IUnknownVectorMethods::choose<UCLID_AFCORELib::IAttributePtr, TOutput>;
}

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
	public IPersistStream,
	public IDispatchImpl<IManageableMemory, &IID_IManageableMemory, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IIdentifiableObject, &IID_IIdentifiableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICloneIdentifiableObject, &IID_ICloneIdentifiableObject, &LIBID_UCLID_COMUTILSLib>,
	public CIdentifiableObject
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
	COM_INTERFACE_ENTRY(IManageableMemory)
	COM_INTERFACE_ENTRY(IIdentifiableObject)
	COM_INTERFACE_ENTRY(ICloneIdentifiableObject)
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
	STDMETHOD(SetGUID)(const GUID* pGuid);
	STDMETHOD(StowDataObject)(IMiscUtils *pMiscUtils);
	
// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown ** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

// IComparableObject
	STDMETHOD(raw_IsEqualTo)(IUnknown * pObj, VARIANT_BOOL * pbValue);

// IManageableMemory
	STDMETHOD(raw_ReportMemoryUsage)(void);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IIdentifiableObject
	STDMETHOD(get_InstanceGUID)(GUID *pVal);

// ICloneIdentifiableObject
	STDMETHOD(raw_CloneIdentifiableObject)(IUnknown ** pObject);
	STDMETHOD(raw_CopyFromIdentifiableObject)(IUnknown *pObject);

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
	// Represents the persisted stringized bytestream of an m_ipDataObject that needed to be
	// cleared for memory managment purposes.
	string m_strStowedDataObject;
	IMemoryManagerPtr m_ipMemoryManager;
	IMiscUtilsPtr m_ipMiscUtils;

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

	// Copies the properties of source to this object using the ICloneIdentifiableObject interface
	// if bWithCloneIdentifiableObject is true and exists on the properties that need to be cloned,
	// otherwise uses the ICopyableObject interface
	void copyFrom(UCLID_AFCORELib::IAttributePtr ipSource, bool bWithCloneIdentifiableObject);
	
	IMiscUtilsPtr getMiscUtils();

	void validateLicense();
};
