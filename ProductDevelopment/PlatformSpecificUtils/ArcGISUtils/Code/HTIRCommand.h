// HTIRCommand.h : Declaration of the CHTIRCommand

#pragma once

#include "resource.h"       // main symbols
#include "UCLIDArcMapToolbarCATID.h"

/////////////////////////////////////////////////////////////////////////////
// CHTIRCommand
class ATL_NO_VTABLE CHTIRCommand : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CHTIRCommand, &CLSID_HTIRCommand>,
	public ISupportErrorInfo,
	public IDispatchImpl<UCLID_COMLMLib::ILicensedComponent, &UCLID_COMLMLib::IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IESRCommand
{
public:
	CHTIRCommand();
	~CHTIRCommand();

DECLARE_REGISTRY_RESOURCEID(IDR_HTIRCOMMAND)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CHTIRCommand)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IESRCommand)
	COM_INTERFACE_ENTRY(UCLID_COMLMLib::ILicensedComponent)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CHTIRCommand)
	IMPLEMENTED_CATEGORY(CATID_UCLIDArcMapToolbar)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICommand
	STDMETHOD(get_Enabled)(VARIANT_BOOL *Enabled);
	STDMETHOD(get_Checked)(VARIANT_BOOL *Checked);
	STDMETHOD(get_Name)(BSTR *Name);
	STDMETHOD(get_Caption)(BSTR *Caption);
	STDMETHOD(get_Tooltip)(BSTR *Tooltip);
	STDMETHOD(get_Message)(BSTR *Message);
	STDMETHOD(get_HelpFile)(BSTR *HelpFile);
	STDMETHOD(get_HelpContextID)(long *helpID);
	STDMETHOD(get_Bitmap)(OLE_HANDLE *Bitmap);
	STDMETHOD(get_Category)(BSTR *categoryName);
	STDMETHOD(raw_OnCreate)(IDispatch* hook);
	STDMETHOD(raw_OnClick)();

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	void validateLicense();

	///////////////////
	// Member variables
	///////////////////
	HBITMAP m_bitmap;
};

