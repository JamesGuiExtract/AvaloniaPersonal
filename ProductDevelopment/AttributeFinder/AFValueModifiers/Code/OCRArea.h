// OCRArea.h : Declaration of the COCRArea

#pragma once

#include "resource.h"       // main symbols

#include <AFCategories.h>

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// COCRArea
class ATL_NO_VTABLE COCRArea : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COCRArea, &CLSID_OCRArea>,
	public IDispatchImpl<IOCRArea, &IID_IOCRArea, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<COCRArea>
{
public:
	COCRArea();
	~COCRArea();

DECLARE_REGISTRY_RESOURCEID(IDR_OCRAREA)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(COCRArea)
	COM_INTERFACE_ENTRY(IOCRArea)
	COM_INTERFACE_ENTRY2(IDispatch, IOCRArea)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(COCRArea)
	PROP_PAGE(CLSID_OCRAreaPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(COCRArea)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
END_CATEGORY_MAP()

public:
// IOCRArea
	STDMETHOD(SetOptions)(/*[in]*/ EFilterCharacters eFilter, /*[in]*/ BSTR bstrCustomFilterCharacters, 
		/*[in]*/ VARIANT_BOOL vbDetectHandwriting, /*[in]*/ VARIANT_BOOL vbReturnUnrecognized, 
		/*[in]*/ VARIANT_BOOL vbClearIfNoneFound);
	STDMETHOD(GetOptions)(/*[out]*/ EFilterCharacters* peFilter, /*[out]*/ BSTR* pbstrCustomFilterCharacters, 
		/*[out]*/ VARIANT_BOOL* pvbDetectHandwriting, /*[out]*/ VARIANT_BOOL* pvbReturnUnrecognized, 
		/*[out]*/ VARIANT_BOOL* pvbClearIfNoneFound);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(/*[in]*/ IAttribute* pAttribute, /*[in]*/ IAFDocument* pOriginInput,
		/*[in]*/ IProgressStatus* pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(/*[out, retval]*/ BSTR* pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown** pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown* pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(/*[out, retval]*/ VARIANT_BOOL* pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(/*[out]*/ VARIANT_BOOL* pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(/*[out]*/ CLSID *pClassID);
	STDMETHOD(IsDirty)();
	STDMETHOD(Load)(/*[unique][in]*/ IStream *pStm);
	STDMETHOD(Save)(/*[unique][in]*/ IStream *pStm, /*[in]*/ BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(/*[out]*/ ULARGE_INTEGER *pcbSize);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(/*[in]*/ REFIID riid);

private:

// Private variables

	// dirty flag
	bool m_bDirty;

	// filter options
	EFilterCharacters m_eFilter;

	// set of custom filter characters
	string m_strCustomFilterCharacters;

	// whether to detect handwriting (true) or printed text (false)
	bool m_bDetectHandwriting;

	// whether to return all characters (true) or only recognized characters (false)
	bool m_bReturnUnrecognized;

	// whether to clear the attribute if no text is found (true)
	// or leave the original attribute unmodified (false)
	bool m_bClearIfNoneFound;

// Private methods

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return a valid SSOCR engine.
	// PROMISE: Instantiates and licenses m_ipOCREngine if it was NULL. Returns m_ipOCREngine.
	IOCREnginePtr getOCREngine();
	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if the handwriting recognition feature is not licensed. 
	// Runs successfully otherwise.
	void validateHandwritingLicense();
	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if this component is not licensed. Runs successfully otherwise.
	void validateLicense();
};
