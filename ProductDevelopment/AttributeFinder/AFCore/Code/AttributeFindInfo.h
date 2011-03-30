
#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CAttributeFindInfo
class ATL_NO_VTABLE CAttributeFindInfo : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAttributeFindInfo, &CLSID_AttributeFindInfo>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IAttributeFindInfo, &IID_IAttributeFindInfo, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>
{
public:
	CAttributeFindInfo();
	~CAttributeFindInfo();

DECLARE_REGISTRY_RESOURCEID(IDR_ATTRIBUTEFINDINFO)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAttributeFindInfo)
	COM_INTERFACE_ENTRY(IAttributeFindInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeFindInfo)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttributeFindInfo
	STDMETHOD(get_StopSearchingWhenValueFound)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_StopSearchingWhenValueFound)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_AttributeRules)(/*[out, retval]*/ IIUnknownVector * *pVal);
	STDMETHOD(put_AttributeRules)(/*[in]*/ IIUnknownVector * newVal);
	STDMETHOD(get_InputValidator)(/*[out, retval]*/ IObjectWithDescription * *pVal);
	STDMETHOD(put_InputValidator)(/*[in]*/ IObjectWithDescription * newVal);
	STDMETHOD(ExecuteRulesOnText)(/*[in]*/ IAFDocument* pAFDoc, 
		/*[in]*/ IProgressStatus *pProgressStatus, 
		/*[out, retval]*/ IIUnknownVector **pAttributes);
	STDMETHOD(get_AttributeSplitter)(/*[out, retval]*/ IObjectWithDescription* *pVal);
	STDMETHOD(put_AttributeSplitter)(/*[in]*/ IObjectWithDescription *newVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	/////////////
	// Variables
	/////////////
	// flag to store status of whether this object has been made
	// dirty since the creation or last write to stream operation
	bool m_bDirty;

	IIUnknownVectorPtr m_ipAttributeRules;
	bool m_bStopSearchingWhenValueFound;
	IObjectWithDescriptionPtr m_ipInputValidator;
	IObjectWithDescriptionPtr m_ipAttributeSplitter;

	//////////////
	// Methods
	//////////////
	//----------------------------------------------------------------------------------------------
	//	PURPOSE: to return m_ipInputValidator
	//
	//	PROMISE: to create an instance of Input Validator and assign it to m_ipInputValidator if
	//			 m_ipInputValidator is __nullptr
	IObjectWithDescriptionPtr getValidator();
	//----------------------------------------------------------------------------------------------
	//	PURPOSE: to return m_ipAttributeSplitter
	//
	//	PROMISE: to create an instance of Attribute Splitter and assign it to m_ipAttributeSplitter 
	//			 if m_ipAttributeSplitter is __nullptr
	IObjectWithDescriptionPtr getSplitter();
	//----------------------------------------------------------------------------------------------
	void validateLicense();
	//----------------------------------------------------------------------------------------------
	// A method to determine the count of enabled attribute rules
	long getNumEnabledAttributeRules();
	//----------------------------------------------------------------------------------------------
	// A method to determine if a splitter object exists in this AttributeFindInfo and
	// if it is also enabled
	bool enabledSplitterExists();
};
