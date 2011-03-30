// RedactionVerificationUIPP.cpp : Implementation of CRedactionVerificationUIPP
#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "RedactionVerificationUIPP.h"
#include "RedactionCCConstants.h"
#include "RedactionUISettings.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <string>

//-------------------------------------------------------------------------------------------------
// CRedactionVerificationUIPP
//-------------------------------------------------------------------------------------------------
CRedactionVerificationUIPP::CRedactionVerificationUIPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLERedactionVerificationUIPP;
		m_dwHelpFileID = IDS_HELPFILERedactionVerificationUIPP;
		m_dwDocStringID = IDS_DOCSTRINGRedactionVerificationUIPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI11512")
}
//-------------------------------------------------------------------------------------------------
CRedactionVerificationUIPP::~CRedactionVerificationUIPP() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16483");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUIPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI18556", pbValue != __nullptr);

		try
		{
			// Check license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18555");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUIPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	int nResult = S_OK;

	try
	{
		ATLTRACE(_T("CRedactionVerificationUIPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_REDACTIONCUSTOMCOMPONENTSLib::IRedactionVerificationUIPtr ipUI = m_ppUnk[i];
			if (ipUI)
			{
				ipUI->ReviewAllPages = 
					asVariantBool(m_checkReviewAllPages.GetCheck() == BST_CHECKED);
				ipUI->AlwaysOutputImage = 
					asVariantBool(m_optionAlwaysOutputImage.GetCheck() == BST_CHECKED);

				// Retrieve and store Annotation settings
				ipUI->CarryForwardAnnotations = asVariantBool(m_imageOutput.bRetainAnnotations);
				ipUI->ApplyRedactionsAsAnnotations = asVariantBool(m_imageOutput.bApplyAsAnnotations);
				
				// Set the output file name
				ipUI->OutputImageName = m_imageOutput.strOutputFile.c_str();

				// Store the metadata settings
				ipUI->AlwaysOutputMeta = 
					asVariantBool(m_optionAlwaysOutputMeta.GetCheck() == BST_CHECKED);

				// Set the metadata output file
				ipUI->MetaOutputName = m_metadata.strOutputFile.c_str();

				// get the setting of the feedback collection checkbox
				bool bCollectFeedback = m_chkCollectFeedback.GetCheck() == BST_CHECKED;
				ipUI->CollectFeedback = asVariantBool(bCollectFeedback);

				// set the feedback folder
				ipUI->FeedbackDataFolder = m_feedback.strDataFolder.c_str();

				// set whether to include the original document in feedback data
				ipUI->CollectFeedbackImage = asVariantBool(m_feedback.bCollectImage);

				// set whether to use the original filenames or to generate unique filenames
				ipUI->FeedbackOriginalFilenames = asVariantBool(m_feedback.bOriginalFilenames);

				// set the feedback collection object
				ipUI->FeedbackCollectOption = m_feedback.eCollectionOptions;						

				// set the require redaction types setting [p16 #2833]
				ipUI->RequireRedactionTypes = asVariantBool(
					m_chkRequireRedactionType.GetCheck() == BST_CHECKED);

				// Set the require exemption codes setting
				ipUI->RequireExemptionCodes = asVariantBool(
					m_chkRequireExemptionCodes.GetCheck() == BST_CHECKED);

				// Set the input ID Shield data file
				ipUI->InputDataFile = m_strInputFile.c_str();

				// Set whether to retain redactions from the previous image
				ipUI->InputRedactedImage = asVariantBool(m_imageOutput.bRetainRedactions);

				// Set the redaction text
				ipUI->RedactionText = m_redactionAppearance.m_strText.c_str();

				// Set the redaction colors
				ipUI->BorderColor = m_redactionAppearance.m_crBorderColor;
				ipUI->FillColor = m_redactionAppearance.m_crFillColor;

				// Set the font
				ipUI->FontName = m_redactionAppearance.m_lgFont.lfFaceName;
				ipUI->IsBold = asVariantBool(m_redactionAppearance.m_lgFont.lfWeight >= FW_BOLD);
				ipUI->IsItalic = asVariantBool(m_redactionAppearance.m_lgFont.lfItalic == gucIS_ITALIC);
				ipUI->FontSize = m_redactionAppearance.m_iPointSize;
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11516");

	return nResult;
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionVerificationUIPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
												 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// General settings
		m_checkReviewAllPages = GetDlgItem(IDC_CHECK_INCLUDE_PAGES);
		m_chkRequireRedactionType = GetDlgItem(IDC_CHECK_REQUIRE_REDACTION_TYPES);
		m_chkRequireExemptionCodes = GetDlgItem(IDC_CHECK_REQUIRE_EXEMPTION_CODES);

		// Data file
		m_stcDataFile = GetDlgItem(IDC_STATIC_DATA_FILE_DESCRIPTION);

		// Image output
		m_optionAlwaysOutputImage = GetDlgItem(IDC_RADIO_CREATE_IMAGE_ALL);
		m_optionOnlyRedactedImage = GetDlgItem(IDC_RADIO_CREATE_IMAGE_REDACTED);

		// Metadata output
		m_optionAlwaysOutputMeta = GetDlgItem(IDC_RADIO_META_OUT_ALL);
		m_optionOnlyRedactedMeta = GetDlgItem(IDC_RADIO_META_OUT_ONLY);

		// Feedback collection
		m_chkCollectFeedback = GetDlgItem(IDC_CHECK_COLLECT_FEEDBACK);
		m_btnFeedbackOptions = GetDlgItem(IDC_BUTTON_FEEDBACK);

		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IRedactionVerificationUIPtr ipUI(m_ppUnk[0]);
		if (ipUI)
		{
			// General settings
			m_checkReviewAllPages.SetCheck( 
				asCppBool(ipUI->ReviewAllPages) ? BST_CHECKED : BST_UNCHECKED);
			m_chkRequireRedactionType.SetCheck(
				asCppBool(ipUI->RequireRedactionTypes) ? BST_CHECKED : BST_UNCHECKED);
			m_chkRequireExemptionCodes.SetCheck(
				asCppBool(ipUI->RequireExemptionCodes) ? BST_CHECKED : BST_UNCHECKED);
			m_optionAlwaysOutputImage.SetCheck( 
				asCppBool(ipUI->AlwaysOutputImage) ? BST_CHECKED : BST_UNCHECKED );
			m_optionOnlyRedactedImage.SetCheck( 
				asCppBool(ipUI->AlwaysOutputImage) ? BST_UNCHECKED : BST_CHECKED );
			m_optionAlwaysOutputMeta.SetCheck( 
				asCppBool(ipUI->AlwaysOutputMeta) ? BST_CHECKED : BST_UNCHECKED );
			m_optionOnlyRedactedMeta.SetCheck( 
				asCppBool(ipUI->AlwaysOutputMeta) ? BST_UNCHECKED : BST_CHECKED );

			// ID Shield data file
			m_strInputFile = asString(ipUI->InputDataFile);

			updateDataFileDescription();

			// Set meta data items
			m_metadata.strOutputFile = asString(ipUI->MetaOutputName);

			// Set image output items
			m_imageOutput.bRetainAnnotations = asCppBool(ipUI->CarryForwardAnnotations);
			m_imageOutput.bApplyAsAnnotations =	asCppBool(ipUI->ApplyRedactionsAsAnnotations);
			m_imageOutput.strOutputFile = ipUI->OutputImageName;

			// Set the feedback collection checkbox and button
			bool bCollectFeedback = asCppBool(ipUI->CollectFeedback);
			m_chkCollectFeedback.SetCheck(bCollectFeedback ? BST_CHECKED : BST_UNCHECKED);
			m_btnFeedbackOptions.EnableWindow(asMFCBool(bCollectFeedback));

			// Set feedback data storage options
			m_feedback.strDataFolder = asString(ipUI->FeedbackDataFolder);
			m_feedback.bCollectImage = asCppBool(ipUI->CollectFeedbackImage);

			// Set feedback filename radio buttons
			m_feedback.bOriginalFilenames = asCppBool(ipUI->FeedbackOriginalFilenames);

			// Get the feedback collection options
			m_feedback.eCollectionOptions = ipUI->FeedbackCollectOption;

			// Get whether to retain redactions from the previous image
			m_imageOutput.bRetainRedactions = asCppBool(ipUI->InputRedactedImage);

			// Get the redaction text
			m_redactionAppearance.m_strText = asString(ipUI->RedactionText);

			// Get the redaction colors
			m_redactionAppearance.m_crBorderColor = ipUI->BorderColor;
			m_redactionAppearance.m_crFillColor = ipUI->FillColor;

			// Get the font
			lstrcpyn(m_redactionAppearance.m_lgFont.lfFaceName, asString(ipUI->FontName).c_str(), LF_FACESIZE);
			m_redactionAppearance.m_lgFont.lfWeight = 
				 ipUI->IsBold == VARIANT_TRUE ? FW_BOLD : FW_NORMAL;
			m_redactionAppearance.m_lgFont.lfItalic = 
				ipUI->IsItalic == VARIANT_TRUE ? gucIS_ITALIC : 0;
			m_redactionAppearance.m_iPointSize = ipUI->FontSize;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11517");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionVerificationUIPP::OnBnClickedCheckCollectFeedback(WORD wNotifyCode, WORD wID, 
																	HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Enable/disable the button based on whether feedback is being collected
		bool bCollectFeedback = (m_chkCollectFeedback.GetCheck() == BST_CHECKED);
		m_btnFeedbackOptions.EnableWindow(asMFCBool(bCollectFeedback));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24528");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionVerificationUIPP::OnBnClickedButtonFeedback(WORD wNotifyCode, WORD wID, 
															 HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CFeedbackDlg dialog(m_feedback);
		if (dialog.DoModal() == IDOK)
		{
			dialog.getOptions(m_feedback);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24529");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionVerificationUIPP::OnBnClickedButtonDataFile(WORD wNotifyCode, WORD wID, 
															  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get SelectTargetFile object
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::ISelectTargetFileUIPtr ipFileSelector(CLSID_SelectTargetFileUI);
		ASSERT_RESOURCE_ALLOCATION("ELI24872", ipFileSelector != __nullptr);

		// Initialize parameters
		ipFileSelector->Title = "Specify ID Shield data file path";
		ipFileSelector->Instructions = "ID Shield data file";
		ipFileSelector->DefaultFileName = gstrDEFAULT_TARGET_FILENAME.c_str();
		ipFileSelector->DefaultExtension = ".voa";
		ipFileSelector->FileTypes = "VOA Files (*.voa)|*.voa||";
		ipFileSelector->FileName = m_strInputFile.c_str();
		
		// Prompt for new data file setting
		if (ipFileSelector->PromptForFile() == VARIANT_TRUE)
		{
			m_strInputFile = asString(ipFileSelector->FileName);
			updateDataFileDescription();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24531");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionVerificationUIPP::OnBnClickedButtonImageOutput(WORD wNotifyCode, WORD wID, 
																 HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CImageOutputDlg dialog(m_imageOutput);
		if (dialog.DoModal() == IDOK)
		{
			dialog.getOptions(m_imageOutput);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24532");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionVerificationUIPP::OnBnClickedButtonMetadata(WORD wNotifyCode, WORD wID, 
															  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CMetadataDlg dialog(m_metadata);
		if (dialog.DoModal() == IDOK)
		{
			dialog.getOptions(m_metadata);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24533");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRedactionVerificationUIPP::OnBnClickedButtonRedactionAppearance(WORD wNotifyCode, 
	WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CRedactionAppearanceDlg dialog(m_redactionAppearance);
		if (dialog.DoModal() == IDOK)
		{
			dialog.getOptions(m_redactionAppearance);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24617");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRedactionVerificationUIPP::updateDataFileDescription()
{
	if (m_strInputFile == gstrDEFAULT_TARGET_FILENAME)
	{
		m_stcDataFile.SetWindowText(gstrDEFAULT_TARGET_MESSAGE.c_str());
	}
	else
	{
		m_stcDataFile.SetWindowText(m_strInputFile.c_str());
	}
}
//-------------------------------------------------------------------------------------------------
void CRedactionVerificationUIPP::validateLicense()
{
	static const unsigned long UIPP_COMPONENT_ID = gnIDSHIELD_VERIFICATION_OBJECT;

	VALIDATE_LICENSE( UIPP_COMPONENT_ID, "ELI11519", "Redaction Verification UI PP" );
}
