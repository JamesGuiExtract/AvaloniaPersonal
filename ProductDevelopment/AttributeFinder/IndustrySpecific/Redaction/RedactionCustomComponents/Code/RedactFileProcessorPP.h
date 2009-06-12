// RedactFileProcessorPP.h : Declaration of the CRedactFileProcessorPP

#pragma once

#include "resource.h"       // main symbols
#include "RedactionAppearanceDlg.h"

#include <ImageButtonWithStyle.h>

EXTERN_C const CLSID CLSID_RedactFileProcessorPP;

/////////////////////////////////////////////////////////////////////////////
// CRedactFileProcessorPP
class ATL_NO_VTABLE CRedactFileProcessorPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRedactFileProcessorPP, &CLSID_RedactFileProcessorPP>,
	public IPropertyPageImpl<CRedactFileProcessorPP>,
	public CDialogImpl<CRedactFileProcessorPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRedactFileProcessorPP();
	
	enum {IDD = IDD_REDACTFILEPROCESSORPP};

DECLARE_REGISTRY_RESOURCEID(IDR_REDACTFILEPROCESSORPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRedactFileProcessorPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CRedactFileProcessorPP)
	COMMAND_HANDLER(IDC_BUTTON_REDACT_APPEARANCE, BN_CLICKED, OnBnClickedButtonRedactAppearance)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRedactFileProcessorPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_RULES_BROWSE_FILE, BN_CLICKED, OnClickedBtnBrowseRules)
	COMMAND_HANDLER(IDC_BTN_OUTPUT_BROWSE_FILE, BN_CLICKED, OnClickedBtnBrowseOutput)
	COMMAND_HANDLER(IDC_ALL_ATTRIBUTES, BN_CLICKED, OnClickedBtnAllAttributes)
	COMMAND_HANDLER(IDC_SELECT_ATTRIBUTES, BN_CLICKED, OnClickedBtnSelectAttributes)
	COMMAND_HANDLER(IDC_CHK_OCDATA, BN_CLICKED, OnClickedBtnOCData)
	COMMAND_HANDLER(IDC_BTN_SELECT_RULES_FILE_TAG, BN_CLICKED, OnClickedSelectRulesFileTag)
	COMMAND_HANDLER(IDC_BTN_SELECT_IMAGE_FILE_TAG, BN_CLICKED, OnClickedSelectImageFileTag)
	COMMAND_HANDLER(IDC_BTN_SELECT_VOA_FILE_TAG, BN_CLICKED, OnClickedSelectVOAFileTag)
	COMMAND_HANDLER(IDC_BTN_USEVOA, BN_CLICKED, OnClickedUseVOAFile)
	COMMAND_HANDLER(IDC_BTN_VOA_BROWSE_FILE, BN_CLICKED, OnClickedBtnBrowseVOAFile)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnBrowseRules(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnBrowseOutput(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnAllAttributes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnSelectAttributes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnOCData(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectRulesFileTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectImageFileTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectVOAFileTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedUseVOAFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnBrowseVOAFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonRedactAppearance(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	/////////////
	// Variables
	/////////////
	ATLControls::CEdit m_editRuleFileName;
	ATLControls::CEdit m_editOutputFileName;
	ATLControls::CButton m_radioUseRedactedImage;
	ATLControls::CButton m_radioUseOriginalImage;
	ATLControls::CButton m_chkReadUSS;
	ATLControls::CButton m_radioAllAttributes;
	ATLControls::CButton m_radioSelectAttributes;
	ATLControls::CEdit m_editAttributes;
	ATLControls::CButton m_radioOutputAlways;
	ATLControls::CButton m_radioOutputOnly;
	CImageButtonWithStyle m_btnSelectRulesFileTag;
	CImageButtonWithStyle m_btnSelectImageFileTag;
	CImageButtonWithStyle m_btnSelectVOAFileTag;
	ATLControls::CEdit m_editVOAFileName;
	ATLControls::CButton m_chkUseVOA;
	ATLControls::CButton m_btnBrowseVOA;
	ATLControls::CButton m_chkCarryAnnotation;
	ATLControls::CButton m_chkRedactionAsAnnotation;
	ATLControls::CButton m_chkHCData;
	ATLControls::CButton m_chkMCData;
	ATLControls::CButton m_chkLCData;
	ATLControls::CButton m_chkOCData;
	ATLControls::CButton m_btnRedactionAppearance;

	// Redaction text and color settings
	RedactionAppearanceOptions m_redactionAppearance;

	/////////////
	// Methods
	/////////////

	// Makes sure the correct buttons are checked and edit box is active or inactive
	void updateAttributeGroup();

	// Make sure the voa file selection controls are enabled and disabled depending on the value of the check
	void updateVOAGroup();

	// converts the contents of the attribute names check boxes and edit control to vector
	IVariantVectorPtr getAttributeNames();
	// converts the AttributeNames vector into a string and sets the check boxes
	void putAttributeNames( IVariantVectorPtr ipAttributeNames );

	// ensure that this component is licensed
	void validateLicense();
};
