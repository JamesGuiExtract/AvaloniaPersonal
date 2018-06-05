// AFDocument.h : Declaration of the CAFDocument

#pragma once

#include "resource.h"       // main symbols
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CAFDocument
class ATL_NO_VTABLE CAFDocument : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAFDocument, &CLSID_AFDocument>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAFDocument, &IID_IAFDocument, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public IDispatchImpl<IHasOCRParameters, &IID_IHasOCRParameters, &LIBID_UCLID_RASTERANDOCRMGMTLib>
{
public:
	CAFDocument();
	~CAFDocument();

DECLARE_REGISTRY_RESOURCEID(IDR_AFDOCUMENT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

BEGIN_COM_MAP(CAFDocument)
	COM_INTERFACE_ENTRY(IAFDocument)
	COM_INTERFACE_ENTRY2(IDispatch,IAFDocument)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IHasOCRParameters)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAFDocument
	STDMETHOD(get_ObjectTags)(/*[out, retval]*/ IStrToObjectMap* *pVal);
	STDMETHOD(put_ObjectTags)(/*[in]*/ IStrToObjectMap* newVal);
	STDMETHOD(get_StringTags)(/*[out, retval]*/ IStrToStrMap* *pVal);
	STDMETHOD(put_StringTags)(/*[in]*/ IStrToStrMap* newVal);
	STDMETHOD(get_Text)(/*[out, retval]*/ ISpatialString* *pVal);
	STDMETHOD(put_Text)(/*[in]*/ ISpatialString* newVal);
	STDMETHOD(get_Attribute)(/*[out, retval]*/ IAttribute* *pVal);
	STDMETHOD(put_Attribute)(/*[in]*/ IAttribute* newVal);
	STDMETHOD(PartialClone)(VARIANT_BOOL vbCloneAttributes, VARIANT_BOOL vbCloneText,
		IAFDocument **pAFDoc);
	STDMETHOD(PushRSDFileName)(/*[in]*/ BSTR strFileName, 
		/*[out, retval]*/ long *pnStackSize);
	STDMETHOD(PopRSDFileName)(/*[out, retval]*/ long *pnStackSize);
	STDMETHOD(get_RSDFileStack)(IVariantVector* *pVal);
	STDMETHOD(put_RSDFileStack)(IVariantVector *newVal);
	STDMETHOD(get_FKBVersion)(BSTR *pVal);
	STDMETHOD(put_FKBVersion)(BSTR newVal);
	STDMETHOD(get_AlternateComponentDataDir)(BSTR *pVal);
	STDMETHOD(put_AlternateComponentDataDir)(BSTR newVal);
	STDMETHOD(IsRSDFileExecuting)(BSTR bstrFileName, VARIANT_BOOL *pbValue);
	STDMETHOD(GetCurrentRSDFileDir)(/*[out, retval]*/ BSTR *pstrRSDFileDir);
	STDMETHOD(get_ParallelRunMode)(EParallelRunMode *pVal);
	STDMETHOD(put_ParallelRunMode)(EParallelRunMode newVal);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown ** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IHasOCRParameters
	STDMETHOD(get_OCRParameters)(IOCRParameters** ppOCRParameters);
	STDMETHOD(put_OCRParameters)(IOCRParameters* pOCRParameters);

private:
	//////////
	// Variables
	//////////
	UCLID_AFCORELib::IAttributePtr m_ipAttribute;
	long m_nVersionNumber;
	IStrToStrMapPtr m_ipStringTags;
	IStrToObjectMapPtr m_ipObjectTags;
	IVariantVectorPtr m_ipRSDFileStack;
	std::string m_strFKBVersion;
	std::string m_strAlternateComponentDataDir;
	EParallelRunMode m_eParallelRunMode;
	IOCRParametersPtr m_ipOCRParameters;

	//////////
	// Methods
	//////////
	UCLID_AFCORELib::IAttributePtr getAttribute();
	void validateLicense();
	IOCRParametersPtr getOCRParameters();
};
