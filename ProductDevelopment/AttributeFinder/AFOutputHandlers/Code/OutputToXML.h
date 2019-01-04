// OutputToXML.h : Declaration of the COutputToXML

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include <IdentifiableObject.h>

#include <string>
#include <utility>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// COutputToXML
class ATL_NO_VTABLE COutputToXML : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COutputToXML, &CLSID_OutputToXML>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IOutputToXML, &IID_IOutputToXML, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IFAMAwareRuleObject, &IID_IFAMAwareRuleObject, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<COutputToXML>,
	public IDispatchImpl<IIdentifiableObject, &IID_IIdentifiableObject, &LIBID_UCLID_COMUTILSLib>,
	private CIdentifiableObject
{
public:
	COutputToXML();

DECLARE_REGISTRY_RESOURCEID(IDR_OUTPUTTOXML)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(COutputToXML)
	COM_INTERFACE_ENTRY(IOutputToXML)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IOutputToXML)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY(IFAMAwareRuleObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(IIdentifiableObject)
END_COM_MAP()

BEGIN_PROP_MAP(COutputToXML)
	PROP_PAGE(CLSID_OutputToXMLPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CSelectUsingMajority)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IOutputToXML
	STDMETHOD(get_FileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_FileName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_Format)(/*[out, retval]*/ EXMLOutputFormat *pVal);
	STDMETHOD(put_Format)(/*[in]*/ EXMLOutputFormat newVal);
	STDMETHOD(get_NamedAttributes)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_NamedAttributes)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_UseSchemaName)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_UseSchemaName)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_SchemaName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_SchemaName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_FAMTags)(VARIANT_BOOL* pVal);
	STDMETHOD(put_FAMTags)(VARIANT_BOOL newVal);
	STDMETHOD(get_RemoveSpatialInfo)(VARIANT_BOOL* pVal);
	STDMETHOD(put_RemoveSpatialInfo)(VARIANT_BOOL newVal);
	STDMETHOD(get_ValueAsFullText)(VARIANT_BOOL* pVal);
	STDMETHOD(put_ValueAsFullText)(VARIANT_BOOL newVal);
	STDMETHOD(get_RemoveEmptyNodes)(VARIANT_BOOL* pVal);
	STDMETHOD(put_RemoveEmptyNodes)(VARIANT_BOOL newVal);

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector *pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

// IFAMAwareRuleObject
	STDMETHOD(raw_ProcessAttributes)(IIUnknownVector *pAttributes, IAFDocument *pAFDoc,
		ITagUtility *pTagUtility, IProgressStatus *pProgressStatus);

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

// IIdentifiableObject
	STDMETHOD(get_InstanceGUID)(GUID *pVal);

private:
	// flag to keep track of whether object is dirty
	bool m_bDirty;
	IAFUtilityPtr m_ipAFUtils;

	// the filename string that represents where the XML data must be saved to
	string m_strFileName;

	// Format in which to write XML output
	EXMLOutputFormat	m_eOutputFormat;

	// flag for writing version 2 output using named attributes
	bool m_bUseNamedAttributes;

	// flag for including the schema name in version 2 output
	bool m_bSchemaName;

	// String that represents the XML schema name
	string m_strSchemaName;

	// Flag to indicate doc tags restriction to <SourceDocName>
	bool m_bFAMTags;

	// Flag to indicate whether spatial info should be removed from the XML output
	bool m_bRemoveSpatialInfo;

	// Flag to indicate whether the outputted XML should use a <FullText/> tag for the value
	bool m_bValueAsFullText;

	// Flag to indicate whether to remove empty nodes
	bool m_bRemoveEmptyNodes;

	void validateLicense();

	// Expands the file name based on the doc tags
	string expandFileName(IAFDocumentPtr ipDoc, ITagUtility *pTagUtility);

	// Checks if the the file name contains tags and whether those tags are valid
	// pair.first holds the contains tags value
	// pair.second holds the valid tags value
	pair<bool, bool> getContainsTagsAndTagsAreValid(const string& strFileName);

	void processAttributes(IIUnknownVector *pAttributes, IAFDocument *pAFDoc, ITagUtility *pTagUtility, IProgressStatus *pProgressStatus);
};
