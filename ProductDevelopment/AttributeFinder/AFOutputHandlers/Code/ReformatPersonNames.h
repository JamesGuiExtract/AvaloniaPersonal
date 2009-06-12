// ReformatPersonNames.h : Declaration of the CReformatPersonNames

#pragma once
#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#pragma warning(disable:4503)
#include <string>
#include <map>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CReformatPersonNames
class ATL_NO_VTABLE CReformatPersonNames : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CReformatPersonNames, &CLSID_ReformatPersonNames>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IReformatPersonNames, &IID_IReformatPersonNames, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CReformatPersonNames>
{
public:
	CReformatPersonNames();

DECLARE_REGISTRY_RESOURCEID(IDR_REFORMATPERSONNAMES)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CReformatPersonNames)
	COM_INTERFACE_ENTRY(IReformatPersonNames)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IReformatPersonNames)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CReformatPersonNames)
	PROP_PAGE(CLSID_ReformatPersonNamesPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CReformatPersonNames)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IReformatPersonNames
	STDMETHOD(get_PersonAttributeQuery)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_PersonAttributeQuery)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_ReformatPersonSubAttributes)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ReformatPersonSubAttributes)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_FormatString)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_FormatString)(/*[in]*/ BSTR newVal);

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

public:

	std::string m_strQuery;
	bool m_bReformatPersonSubAttributes;
	std::string m_strFormat;

	bool m_bDirty;
	IAFUtilityPtr m_ipAFUtility;

	void validateLicense();
	void validateFormatString(std::string strFormat);

	typedef std::map<std::string, std::vector<ISpatialStringPtr> > VariableMap;
	void reformatAttribute(IAttributePtr ipAttribute);
	ISpatialStringPtr getReformattedName(std::string strFormat, VariableMap& mapVars);
};

