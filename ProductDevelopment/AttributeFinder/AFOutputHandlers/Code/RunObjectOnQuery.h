// RunObjectOnQuery.h : Declaration of the CRunObjectOnQuery


#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CRunObjectOnQuery
class ATL_NO_VTABLE CRunObjectOnQuery : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRunObjectOnQuery, &CLSID_RunObjectOnQuery>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IRunObjectOnQuery, &IID_IRunObjectOnQuery, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CRunObjectOnQuery>
{
public:
	CRunObjectOnQuery();
	~CRunObjectOnQuery();

DECLARE_REGISTRY_RESOURCEID(IDR_RUNOBJECTONQUERY)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRunObjectOnQuery)
	COM_INTERFACE_ENTRY(IRunObjectOnQuery)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IRunObjectOnQuery)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CRunObjectOnQuery)
	PROP_PAGE(CLSID_RunObjectOnQueryPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CRunObjectOnQuery)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRunObjectOnQuery
	// REQUIRE: nothing
	// PROMISE: to return the query being used by this object
	STDMETHOD(get_AttributeQuery)(/*[out, retval]*/ BSTR *pVal);
	// REQUIRE: newVal has a length > 0.  Should require that newVal is a valid
	//			query string but currently does not
	// PROMISE: From the time this method is called on newVal will be used to query attributes
	STDMETHOD(put_AttributeQuery)(/*[in]*/ BSTR newVal);

	// REQUIRE: nothing
	// PROMISE: This mehtod will return the object and the interface that will be run on the 
	//			queried attributes, The Object may be NULL, but if it is not it will implement the returned
	//			IID.
	STDMETHOD(GetObjectAndIID)(/*[in, out]*/ IID *pIID, /*[out, retval]*/ ICategorizedComponent **pObject);
	// REQUIRE: newIID must be one of the following:
	//			IID_IAttributeModifyingRule
	//			IID_IAttributeSplitter
	//			IID_IOutputHandler
	//			newObject must implement the interface represented by newIID
	// PROMISE: Specify the object and the interface that will be run on the queried attributes
	//			from this method call on.
	STDMETHOD(SetObjectAndIID)(/*[in]*/ IID newIID, /*[in]*/ ICategorizedComponent *newObject);


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

	// This is the component that will be run on the queried attributes
	ICategorizedComponentPtr m_ipObject;

	// This is what type of object m_ipObject is 
	// (i.e. VM, OH, Splitter...)f 
	IID m_objectIID;

	IAFUtilityPtr m_ipAFUtility;

	//////////
	// Methods
	//////////

	// Check licensing
	void validateLicense();

	// REQUIRE: m_ipObject implements IAttributeModifyingRule
	// PROMISE: the m_ipObject IAttributeModifyingRule will be run on each attribute in 
	//			ipAttributes
	void runAttributeModifier(IIUnknownVectorPtr ipAttributes, IAFDocumentPtr ipAFDoc);
	// REQUIRE: m_ipObject implements IAttributeSplitter
	// PROMISE: the m_ipObject IAttributeSplitter will be run on each attribute in 
	//			ipAttributes
	void runAttributeSplitter(IIUnknownVectorPtr ipAttributes, IAFDocumentPtr ipAFDoc);
	// REQUIRE: m_ipObject implements IOutputHandler
	// PROMISE: the m_ipObject IOutputHandler will be run on each attribute in 
	//			ipAttributesSelected, if attributes are added or removed from ipAttributesSelected
	//			by the output handler those attributes will be added or removed from the ipAttributesOut
	// ARGS:	ipAttributesOut - The vector of attributes that were passed into this CRunObjectOnQuery object
	//			ipAttributesSelected - The vector of attributes that have been selected from ipAttributesOut
	//					by the query for this CRunObjectOnQuery object
	//			ipAFDoc - The AFDoc passed into this CRunObjectOnQuery object.
	void runOutputHandler(IIUnknownVectorPtr ipAttributesOut, IIUnknownVectorPtr ipAttributesSelected,
		IAFDocumentPtr ipAFDoc);
};
