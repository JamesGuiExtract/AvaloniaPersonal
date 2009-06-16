// AFConvertVOAToXMLTaskPP.h : Declaration of the CAFConvertVOAToXMLTaskPP

#pragma once

#include "resource.h"       // main symbols

#include <ImageButtonWithStyle.h>

EXTERN_C const CLSID CLSID_AFConvertVOAToXMLTaskPP;

/////////////////////////////////////////////////////////////////////////////
// CAFConvertVOAToXMLTaskPP
class ATL_NO_VTABLE CAFConvertVOAToXMLTaskPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAFConvertVOAToXMLTaskPP, &CLSID_AFConvertVOAToXMLTaskPP>,
	public IPropertyPageImpl<CAFConvertVOAToXMLTaskPP>,
	public CDialogImpl<CAFConvertVOAToXMLTaskPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CAFConvertVOAToXMLTaskPP();

	enum {IDD = IDD_AFCONVERTVOATOXMLTASKPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_AFCONVERTVOATOXMLTASKPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAFConvertVOAToXMLTaskPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CAFConvertVOAToXMLTaskPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CAFConvertVOAToXMLTaskPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_BROWSE_VOA_FILE, BN_CLICKED, OnClickedBtnBrowse)
	COMMAND_HANDLER(IDC_BTN_BROWSE_XML_FILE, BN_CLICKED, OnClickedBtnBrowse)
	COMMAND_HANDLER(IDC_BTN_VOA_FILE_DOC_TAGS, BN_CLICKED, OnClickedBtnDocTags)
	COMMAND_HANDLER(IDC_BTN_XML_FILE_DOC_TAGS, BN_CLICKED, OnClickedBtnDocTags)
	COMMAND_HANDLER(IDC_BTN_CONFIGURE_XML_OUTPUT, BN_CLICKED, OnClickedBtnConfigureXMLOutput)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnDocTags(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnConfigureXMLOutput(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:

	/////////////
	// Variables
	/////////////
	ATLControls::CEdit m_editVOAFileName;
	ATLControls::CButton m_btnBrowseVOAFile;
	CImageButtonWithStyle m_btnVOAFileSelectTag;

	// The output to xml handler
	IOutputToXMLPtr m_ipOutputToXML;

	///////////
	// Methods
	///////////

	// ensure that this component is licensed
	void validateLicense();
};
