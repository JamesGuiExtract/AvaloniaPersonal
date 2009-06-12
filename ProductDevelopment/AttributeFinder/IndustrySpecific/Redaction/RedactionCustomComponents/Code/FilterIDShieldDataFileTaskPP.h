// FilterIDShieldDataFileTaskPP.h : Declaration of the CFilterIDShieldDataFileTaskPP

#pragma once

#include "resource.h"       // main symbols
#include "RedactionAppearanceDlg.h"

#include <ImageButtonWithStyle.h>

EXTERN_C const CLSID CLSID_FilterIDShieldDataFileTaskPP;

/////////////////////////////////////////////////////////////////////////////
// CFilterIDShieldDataFileTaskPP
class ATL_NO_VTABLE CFilterIDShieldDataFileTaskPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFilterIDShieldDataFileTaskPP, &CLSID_FilterIDShieldDataFileTaskPP>,
	public IPropertyPageImpl<CFilterIDShieldDataFileTaskPP>,
	public CDialogImpl<CFilterIDShieldDataFileTaskPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CFilterIDShieldDataFileTaskPP();
	
	enum {IDD = IDD_FILTERIDSHIELDPP};

DECLARE_REGISTRY_RESOURCEID(IDR_FILTERIDSHIELDPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CFilterIDShieldDataFileTaskPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
BEGIN_MSG_MAP(CFilterIDShieldDataFileTaskPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CFilterIDShieldDataFileTaskPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BUTTON_FILTER_INPUT_TAGS, BN_CLICKED, OnClickedButtonInputTags)
	COMMAND_HANDLER(IDC_BUTTON_FILTER_INPUT_BROWSE, BN_CLICKED, OnClickedButtonInputBrowse)
	COMMAND_HANDLER(IDC_BUTTON_FILTER_OUTPUT_TAGS, BN_CLICKED, OnClickedButtonOutputTags)
	COMMAND_HANDLER(IDC_BUTTON_FILTER_OUTPUT_BROWSE, BN_CLICKED, OnClickedButtonOutputBrowse)
	COMMAND_HANDLER(IDC_CHECK_FILTER_OTHER, BN_CLICKED, OnClickedCheckOther)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedButtonInputTags(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonInputBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonOutputTags(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonOutputBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckOther(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	/////////////
	// Variables
	/////////////
	ATLControls::CEdit m_editVOAFileToRead;
	CImageButtonWithStyle m_btnInputTags;
	ATLControls::CButton m_btnInputBrowse;
	ATLControls::CButton m_checkSSN;
	ATLControls::CButton m_checkTaxID;
	ATLControls::CButton m_checkCreditDebit;
	ATLControls::CButton m_checkDriversLicense;
	ATLControls::CButton m_checkBankAccountNumbers;
	ATLControls::CButton m_checkOtherAccount;
	ATLControls::CButton m_checkDOB;
	ATLControls::CButton m_checkOther;
	ATLControls::CEdit m_editOtherData;
	CImageButtonWithStyle m_btnOutputTags;
	ATLControls::CButton m_btnOutputBrowse;
	ATLControls::CEdit m_editVOAFileToWrite;

	/////////////
	// Methods
	/////////////

	// ensure that this component is licensed
	void validateLicense();

	// Check that the input and output VOA files are not the same
	bool doInputOutputFilesMatch(const string& strInput, const string& strOutput);

	// Sets the check boxes based on the variant vector's contents
	void setDataTypeCheckBoxes(IVariantVectorPtr ipVarDataTypes);

	// Builds a variant vector containing the appropriate data type filter list
	// based on the current state of the check boxes
	IVariantVectorPtr getDataTypeCheckBoxes();
};
