// RedactionTaskPP.h : Declaration of the CRedactionTaskPP

#pragma once

#include "resource.h"       // main symbols
#include "RedactionAppearanceDlg.h"

#include <ImageButtonWithStyle.h>

/////////////////////////////////////////////////////////////////////////////
// CRedactionTaskPP
class ATL_NO_VTABLE CRedactionTaskPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRedactionTaskPP, &CLSID_RedactionTaskPP>,
	public IPropertyPageImpl<CRedactionTaskPP>,
	public CDialogImpl<CRedactionTaskPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRedactionTaskPP();
	~CRedactionTaskPP();
	
	enum {IDD = IDD_REDACTIONTASKPP};

DECLARE_REGISTRY_RESOURCEID(IDR_REDACTIONTASKPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRedactionTaskPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CRedactionTaskPP)
	COMMAND_HANDLER(IDC_BUTTON_REDACT_APPEARANCE, BN_CLICKED, OnBnClickedButtonRedactAppearance)
	COMMAND_HANDLER(IDC_BUTTON_DATA_FILE, BN_CLICKED, OnClickedButtonDataFile)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRedactionTaskPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_OUTPUT_BROWSE_FILE, BN_CLICKED, OnClickedBtnBrowseOutput)
	COMMAND_HANDLER(IDC_ALL_ATTRIBUTES, BN_CLICKED, OnClickedBtnAllAttributes)
	COMMAND_HANDLER(IDC_SELECT_ATTRIBUTES, BN_CLICKED, OnClickedBtnSelectAttributes)
	COMMAND_HANDLER(IDC_CHK_OCDATA, BN_CLICKED, OnClickedBtnOCData)
	COMMAND_HANDLER(IDC_BTN_SELECT_IMAGE_FILE_TAG, BN_CLICKED, OnClickedSelectImageFileTag)
	COMMAND_HANDLER(IDC_CHECK_PDF_SECURITY, BN_CLICKED, OnClickedCheckPdfSecurity)
	COMMAND_HANDLER(IDC_BTN_PDF_SECURITY_SETTINGS, BN_CLICKED, OnClickedBtnPdfSettings)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnBrowseOutput(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnAllAttributes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnSelectAttributes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnOCData(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectImageFileTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonRedactAppearance(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonDataFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckPdfSecurity(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnPdfSettings(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	/////////////
	// Variables
	/////////////

	// Data categories to redact
	ATLControls::CButton m_radioAllAttributes;
	ATLControls::CButton m_radioSelectAttributes;
	ATLControls::CButton m_chkHCData;
	ATLControls::CButton m_chkMCData;
	ATLControls::CButton m_chkLCData;
	ATLControls::CButton m_chkManual;
	ATLControls::CButton m_chkOCData;
	ATLControls::CEdit m_editAttributes;

	// Output file
	ATLControls::CEdit m_editOutputFileName;
	CImageButtonWithStyle m_btnSelectImageFileTag;
	ATLControls::CButton m_radioUseRedactedImage;
	ATLControls::CButton m_radioUseOriginalImage;
	ATLControls::CButton m_chkCarryAnnotation;
	ATLControls::CButton m_chkRedactionAsAnnotation;
	ATLControls::CButton m_chkPdfSecurity;
	ATLControls::CButton m_btnPdfSettings;

	// Data file
	ATLControls::CStatic m_stcDataFile;

	// Redaction text and color settings
	ATLControls::CButton m_btnRedactionAppearance;

	// Variables
	string m_strVoaFileName;
	RedactionAppearanceOptions m_redactionAppearance;

	// Pdf password security settings object
	IPdfPasswordSettingsPtr m_ipPdfSettings;

	/////////////
	// Methods
	/////////////

	// Makes sure the correct buttons are checked and edit box is active or inactive
	void updateAttributeGroup();

	// Sets the text of the data file label based on m_strVoaFileName
	void updateDataFileDescription();

	// converts the contents of the attribute names check boxes and edit control to vector
	IVariantVectorPtr getAttributeNames();

	// converts the AttributeNames vector into a string and sets the check boxes
	void putAttributeNames(IVariantVectorPtr ipAttributeNames);

	// ensure that this component is licensed
	void validateLicense();
};