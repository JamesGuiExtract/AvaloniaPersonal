// RedactionVerificationUIPP.h : Declaration of the CRedactionVerificationUIPP

#pragma once

#include "resource.h"       // main symbols
#include "MetaDataDlg.h"
#include "ImageOutputDlg.h"
#include "FeedbackDlg.h"
#include "RedactionAppearanceDlg.h"

#include <string>

using namespace std;

EXTERN_C const CLSID CLSID_RedactionVerificationUIPP;

/////////////////////////////////////////////////////////////////////////////
// CRedactionVerificationUIPP
class ATL_NO_VTABLE CRedactionVerificationUIPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRedactionVerificationUIPP, &CLSID_RedactionVerificationUIPP>,
	public IPropertyPageImpl<CRedactionVerificationUIPP>,
	public CDialogImpl<CRedactionVerificationUIPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRedactionVerificationUIPP();
	~CRedactionVerificationUIPP();

	enum {IDD = IDD_REDACTIONVERIFICATIONUIPP};

DECLARE_REGISTRY_RESOURCEID(IDR_REDACTIONVERIFICATIONUIPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRedactionVerificationUIPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CRedactionVerificationUIPP)
	COMMAND_HANDLER(IDC_CHECK_COLLECT_FEEDBACK, BN_CLICKED, OnBnClickedCheckCollectFeedback)
	COMMAND_HANDLER(IDC_BUTTON_FEEDBACK, BN_CLICKED, OnBnClickedButtonFeedback)
	COMMAND_HANDLER(IDC_BUTTON_DATA_FILE, BN_CLICKED, OnBnClickedButtonDataFile)
	COMMAND_HANDLER(IDC_BUTTON_IMAGE_OUTPUT, BN_CLICKED, OnBnClickedButtonImageOutput)
	COMMAND_HANDLER(IDC_BUTTON_METADATA, BN_CLICKED, OnBnClickedButtonMetadata)
	COMMAND_HANDLER(IDC_BUTTON_REDACTION_APPEARANCE, BN_CLICKED, OnBnClickedButtonRedactionAppearance)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRedactionVerificationUIPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
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
	LRESULT OnBnClickedCheckCollectFeedback(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonFeedback(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonDataFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonImageOutput(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonMetadata(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonRedactionAppearance(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	
private:
	
	//---------------------------------------------------------------------------------------------
	// Controls
	//---------------------------------------------------------------------------------------------

	// General settings
	ATLControls::CButton m_checkReviewAllPages;
	ATLControls::CButton m_chkRequireRedactionType;
	ATLControls::CButton m_chkRequireExemptionCodes;

	// Data file
	ATLControls::CStatic m_stcDataFile;

	// Image output
	ATLControls::CButton m_optionAlwaysOutputImage;
	ATLControls::CButton m_optionOnlyRedactedImage;

	// Metadata output
	ATLControls::CButton m_optionAlwaysOutputMeta;
	ATLControls::CButton m_optionOnlyRedactedMeta;
	
	// Feedback collection
	ATLControls::CButton m_chkCollectFeedback;
	ATLControls::CButton m_btnFeedbackOptions;
	
	//---------------------------------------------------------------------------------------------
	// Options
	//---------------------------------------------------------------------------------------------

	MetadataOptions m_metadata;
	ImageOutputOptions m_imageOutput;
	FeedbackOptions m_feedback;
	RedactionAppearanceOptions m_redactionAppearance;

	// The path to the input ID Shield data file. May include tags.
	string m_strInputFile;

	//---------------------------------------------------------------------------------------------
	// Methods
	//---------------------------------------------------------------------------------------------

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the label for the input ID Shield data file based on m_strInputFile.
	void updateDataFileDescription();

	// ensure that this component is licensed
	void validateLicense();
};
