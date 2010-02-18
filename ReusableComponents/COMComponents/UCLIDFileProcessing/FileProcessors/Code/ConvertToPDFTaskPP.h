// ConvertToPDFTaskPP.h : Declaration of the CConvertToPDFTaskPP

#pragma once

#include "resource.h"       // main symbols

#include <ImageButtonWithStyle.h>

/////////////////////////////////////////////////////////////////////////////
// CConvertToPDFTaskPP
class ATL_NO_VTABLE CConvertToPDFTaskPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CConvertToPDFTaskPP, &CLSID_ConvertToPDFTaskPP>,
	public IPropertyPageImpl<CConvertToPDFTaskPP>,
	public CDialogImpl<CConvertToPDFTaskPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CConvertToPDFTaskPP();
	~CConvertToPDFTaskPP();

	enum {IDD = IDD_CONVERTTOPDFTASKPP};

DECLARE_REGISTRY_RESOURCEID(IDR_PROPPAGE_CONVERT_TO_PDF_TASK)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CConvertToPDFTaskPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CConvertToPDFTaskPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CConvertToPDFTaskPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_CONVERT_TO_PDF_INPUT_IMAGE_DOC_TAG, BN_CLICKED, OnClickedBtnInputImageDocTag)
	COMMAND_HANDLER(IDC_BTN_CONVERT_TO_PDF_BROWSE_INPUT_IMAGE, BN_CLICKED, OnClickedBtnInputImageBrowse)
	COMMAND_HANDLER(IDC_CHECK_PDF_SECURITY, BN_CLICKED, OnClickedCheckPdfSecurity)
	COMMAND_HANDLER(IDC_BTN_PDF_SECURITY_SETTINGS, BN_CLICKED, OnClickedBtnPdfSecuritySettings)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)();

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

// Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnInputImageDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnInputImageBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckPdfSecurity(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnPdfSecuritySettings(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:

	//---------------------------------------------------------------------------------------------
	// Controls
	//---------------------------------------------------------------------------------------------

	// PDF input file controls
	ATLControls::CEdit m_editInputImage;
	CImageButtonWithStyle m_btnInputImageDocTag;
	ATLControls::CButton m_btnInputImageBrowse;
	ATLControls::CButton m_checkPDFA;
	ATLControls::CButton m_checkPDFSecurity;
	ATLControls::CButton m_btnPDFSecuritySettings;

	_PdfPasswordSettingsPtr m_ipSettings;

	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if ConvertToPDFTask is not licensed.
	void validateLicense();
};

OBJECT_ENTRY_AUTO(CLSID_ConvertToPDFTaskPP, CConvertToPDFTaskPP)