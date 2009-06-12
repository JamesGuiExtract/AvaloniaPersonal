// InsertCharacters.h : Declaration of the CInsertCharacters

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

/////////////////////////////////////////////////////////////////////////////
// CInsertCharacters
class ATL_NO_VTABLE CInsertCharacters : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CInsertCharacters, &CLSID_InsertCharacters>,
	public ISupportErrorInfo,
	public IDispatchImpl<IInsertCharacters, &IID_IInsertCharacters, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CInsertCharacters>
{
public:
	CInsertCharacters();
	~CInsertCharacters();

DECLARE_REGISTRY_RESOURCEID(IDR_INSERTCHARACTERS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CInsertCharacters)
	COM_INTERFACE_ENTRY(IInsertCharacters)
	COM_INTERFACE_ENTRY2(IDispatch, IInsertCharacters)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CInsertCharacters)
	PROP_PAGE(CLSID_InsertCharactersPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CInsertCharacters)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IInsertCharacters
	STDMETHOD(get_AppendToEnd)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_AppendToEnd)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_InsertAt)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_InsertAt)(/*[in]*/ long newVal);
	STDMETHOD(get_CharsToInsert)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_CharsToInsert)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_NumOfCharsLong)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_NumOfCharsLong)(/*[in]*/ long newVal);
	STDMETHOD(get_LengthType)(/*[out, retval]*/ EInsertCharsLengthType *pVal);
	STDMETHOD(put_LengthType)(/*[in]*/ EInsertCharsLengthType newVal);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute* pAttribute, IAFDocument* pOriginInput,
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	/////////////
	// Variables
	/////////////
	EInsertCharsLengthType m_eLengthType;
	long m_nNumOfCharsLong;
	std::string m_strCharsToInsert;
	bool m_bAppendToEnd;
	long m_nPositionToInsert;

	bool m_bDirty;

	//////////////
	// Methods
	//////////////
	void validateLicense();
};
