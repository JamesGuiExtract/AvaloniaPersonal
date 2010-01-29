// PersonNameSplitter.h : Declaration of the CPersonNameSplitter

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

/////////////////////////////////////////////////////////////////////////////
// CPersonNameSplitter
// Splits a human name into Title, First, Middle, Last, and Suffix
class ATL_NO_VTABLE CPersonNameSplitter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CPersonNameSplitter, &CLSID_EntityNameSplitter>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IAttributeSplitter, &IID_IAttributeSplitter, &LIBID_UCLID_AFSPLITTERSLib>,
	public IDispatchImpl<IPersonNameSplitter, &IID_IPersonNameSplitter, &LIBID_UCLID_AFSPLITTERSLib>
{
public:
	CPersonNameSplitter();
	~CPersonNameSplitter();

DECLARE_REGISTRY_RESOURCEID(IDR_PERSONNAMESPLITTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CPersonNameSplitter)
	COM_INTERFACE_ENTRY(IAttributeSplitter)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeSplitter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IPersonNameSplitter)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CPersonNameSplitter)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SPLITTERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttributeSplitter
	STDMETHOD(raw_SplitAttribute)(IAttribute *pAttribute, IAFDocument *pAFDoc, 
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IPersonNameSplitter
	STDMETHOD(BuildAttribute)(BSTR strParentName, BSTR strTitle, BSTR strFirst, BSTR strMiddle, 
		BSTR strLast, BSTR strSuffix, VARIANT_BOOL bAutoBuildParent, IAttribute* *pVal);

private:
	// flag to keep track of whether object is dirty
	bool m_bDirty;

	// Provides collections of person and company keywords
	IEntityKeywordsPtr	m_ipKeys;

	IMiscUtilsPtr m_ipMisc;

	// Gets a new instance of the regular expression parser
	IRegularExprParserPtr getParser();

	// ensure that this component is licensed
	void validateLicense();
};
