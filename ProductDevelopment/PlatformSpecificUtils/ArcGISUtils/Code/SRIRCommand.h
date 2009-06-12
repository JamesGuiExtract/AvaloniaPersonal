// SRIRCommand.h : Declaration of the CSRIRCommand

#pragma once
#include "resource.h"       // main symbols
#include "UCLIDArcMapToolbarCATID.h"

/////////////////////////////////////////////////////////////////////////////
// CSRIRCommand
class ATL_NO_VTABLE CSRIRCommand : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSRIRCommand, &CLSID_SRIRCommand>,
	public ISupportErrorInfo,
	public IDispatchImpl<UCLID_COMLMLib::ILicensedComponent, &UCLID_COMLMLib::IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IESRCommand
{
public:
	CSRIRCommand();
	~CSRIRCommand();

DECLARE_REGISTRY_RESOURCEID(IDR_SRIRCOMMAND)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSRIRCommand)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(UCLID_COMLMLib::ILicensedComponent)
	COM_INTERFACE_ENTRY(IESRCommand)
END_COM_MAP()

BEGIN_CATEGORY_MAP(IDR_SRIRWINDOWCOMMAND)
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

