// SelectTargetFileUI.h : Declaration of the CSelectTargetFileUI

#pragma once
#include "resource.h"       // main symbols
#include "RedactionCustomComponents.h"
#include "SelectTargetFileUIDlg.h"

// CSelectTargetFileUI

class ATL_NO_VTABLE CSelectTargetFileUI :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSelectTargetFileUI, &CLSID_SelectTargetFileUI>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISelectTargetFileUI, &IID_ISelectTargetFileUI, &LIBID_UCLID_REDACTIONCUSTOMCOMPONENTSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CSelectTargetFileUI();
	~CSelectTargetFileUI();

DECLARE_REGISTRY_RESOURCEID(IDR_SELECTTARGETFILEUI)

BEGIN_COM_MAP(CSelectTargetFileUI)
	COM_INTERFACE_ENTRY(ISelectTargetFileUI)
	COM_INTERFACE_ENTRY2(IDispatch, ISelectTargetFileUI)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

// ISelectTargetFileUI
	STDMETHOD(get_FileName)(BSTR *pVal);
	STDMETHOD(put_FileName)(BSTR newVal);
	STDMETHOD(get_FileTypes)(BSTR *pVal);
	STDMETHOD(put_FileTypes)(BSTR newVal);
	STDMETHOD(get_DefaultExtension)(BSTR *pVal);
	STDMETHOD(put_DefaultExtension)(BSTR newVal);
	STDMETHOD(get_DefaultFileName)(BSTR *pVal);
	STDMETHOD(put_DefaultFileName)(BSTR newVal);
	STDMETHOD(get_Title)(BSTR *pVal);
	STDMETHOD(put_Title)(BSTR newVal);
	STDMETHOD(get_Instructions)(BSTR *pVal);
	STDMETHOD(put_Instructions)(BSTR newVal);
	STDMETHOD(PromptForFile)(VARIANT_BOOL *pVal);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	//////////////
	// Variables
	//////////////
	CSelectTargetFileUIDlg m_dlg;

	/////////////
	// Methods
	/////////////
	void validateLicense();	
};
