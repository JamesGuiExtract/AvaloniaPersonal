#include "stdafx.h"
#include "afcore.h"
#include "RuleSetEditor.h"
#include "RuleTesterDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CRuleSetEditor
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnEditCut() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// First copy the item to the Clipboard
		OnEditCopy();

		// Delete the item
		OnEditDelete();

		// Update button states
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05422")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnEditCopy() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check control
		switch (m_eContextMenuCtrl)
		{
		case kAttributes:
		{
			// Create local Attribute Name to Info map object
			IStrToObjectMapPtr	ipTempMap( CLSID_StrToObjectMap );

			// Retrieve name of selected Attribute
			CString	zName;
			int iIndex = -1;
			iIndex = m_comboAttr.GetCurSel();
			m_comboAttr.GetLBText( iIndex, zName );

			// Retrieve the associated AttributeFindInfo object
			IUnknownPtr	ipInfo = m_ipAttributeNameToInfoMap->GetValue( 
				get_bstr_t( zName.operator LPCTSTR() ) );
			if (ipInfo != __nullptr)
			{
				// Add Name and Info object to local map
				ipTempMap->Set( get_bstr_t( zName.operator LPCTSTR() ), ipInfo );

				// ClipboardManager will handle the Copy
				m_ipClipboardMgr->CopyObjectToClipboard( ipTempMap );
			}
		}
		break;

		case kRulesList:
		{
			// Get index of first selection
			int iIndex = m_listRules.GetFirstSelectedRow();

			if (iIndex == -1)
			{
				// Throw exception
				throw UCLIDException( "ELI05495", 
					"Unable to determine selected Attribute Rule!" );
			}

			// Retrieve vector of existing rules
			IIUnknownVectorPtr	ipRules = m_ipInfo->GetAttributeRules();
			if (ipRules == __nullptr)
			{
				// Create and throw exception
				throw UCLIDException( "ELI05493", 
					"Unable to retrieve Attribute Rules!" );
			}

			// Create a vector for selected rules
			IIUnknownVectorPtr	ipCopiedRules( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI05525", ipCopiedRules != __nullptr );

			// Add each selected rule to vector
			while (iIndex != -1)
			{
				// Retrieve the selected rule
				IUnknownPtr	ipObject = ipRules->At( iIndex );
				ASSERT_RESOURCE_ALLOCATION( "ELI05494", ipObject != __nullptr );

				// Add the rule to the vector
				ipCopiedRules->PushBack( ipObject );

				// Get the next selection
				iIndex = m_listRules.GetNextSelectedRow();
			}

			// ClipboardManager will handle the Copy
			m_ipClipboardMgr->CopyObjectToClipboard( ipCopiedRules );
		}
		break;

		case kValidator:
		{
			// Retrieve existing Input Validator
			IUnknownPtr	ipObject = m_ipInfo->InputValidator;
			ASSERT_RESOURCE_ALLOCATION( "ELI05496", ipObject != __nullptr );

			// ClipboardManager will handle the Copy
			m_ipClipboardMgr->CopyObjectToClipboard( ipObject );
		}
		break;

		case kSplitter:
		{
			// Retrieve existing Attribute Splitter
			IUnknownPtr	ipObject = m_ipInfo->AttributeSplitter;
			ASSERT_RESOURCE_ALLOCATION( "ELI05497", ipObject != __nullptr );

			// ClipboardManager will handle the Copy
			m_ipClipboardMgr->CopyObjectToClipboard( ipObject );
		}
		break;

		case kPreprocessor:
		{
			// Retrieve existing Document Preprocessor
			IUnknownPtr	ipObject = m_ipRuleSet->GlobalDocPreprocessor;
			ASSERT_RESOURCE_ALLOCATION( "ELI06064", ipObject != __nullptr );

			// ClipboardManager will handle the Copy
			m_ipClipboardMgr->CopyObjectToClipboard( ipObject );
		}
		break;

		case kOutputHandler:
		{
			// Retrieve existing Output Handler
			IUnknownPtr	ipObject = m_ipRuleSet->GlobalOutputHandler;
			ASSERT_RESOURCE_ALLOCATION( "ELI07732", ipObject != __nullptr );

			// ClipboardManager will handle the Copy
			m_ipClipboardMgr->CopyObjectToClipboard( ipObject );
		}
		break;

		default:
			// Throw exception
			UCLIDException ue( "ELI05491", "Unable to Copy item." );
			ue.addDebugInfo( "Item Type Number", m_eContextMenuCtrl );
			throw ue;
			break;
		}
  	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05423")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnEditPaste() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check control
		switch (m_eContextMenuCtrl)
		{
		case kAttributes:
		{
			// Test ClipboardManager object to see if it is an Attribute
			IUnknownPtr	ipObject( NULL );
			if (clipboardObjectIsAttribute())
			{
				// Retrieve object from ClipboardManager
				ipObject = m_ipClipboardMgr->GetObjectInClipboard();
				ASSERT_RESOURCE_ALLOCATION( "ELI05500", ipObject != __nullptr );
			}
			else
			{
				// Throw exception, object is not an Attribute
				throw UCLIDException( "ELI05501", "Clipboard object is not an Attribute." );
			}

			// Retrieve collection of Attributes
			IStrToObjectMapPtr ipMap = ipObject;
			if (ipMap != __nullptr)
			{
				// Get map size
				long lMapSize = ipMap->GetSize();

				// Loop through map items
				IUnknownPtr	ipItem;
				int			iFirstNewIndex = -1;
				IUnknownPtr	ipFirstInfo( NULL );
				for (int i = 0; i < lMapSize; i++)
				{
					CComBSTR bstrKey;
					// Retrieve this item
					ipMap->GetKeyValue( i, &bstrKey, &ipItem );

					// Get name of Attribute being Pasted
					string strName = asString( bstrKey );

					// Check to see if this name is already present
					string strNewName( strName );
					while (m_ipAttributeNameToInfoMap->Contains( 
						get_bstr_t(strNewName.c_str()) ))
					{
						// Find an unused Name
						strNewName = incrementNumericalSuffix( strNewName );
					}

					// Add the attribute to the map
					m_ipAttributeNameToInfoMap->Set( get_bstr_t( strNewName.c_str() ), 
						ipItem );

					// Add the attribute to the combo box
					int iIndex = m_comboAttr.AddString( strNewName.c_str() );

					// Save index and Info pointer of first added Attribute
					if (i == 0)
					{
						iFirstNewIndex = iIndex;
						ipFirstInfo = ipItem;
					}
				}

				// Default to selection of the new attribute
				if (iFirstNewIndex >= 0)
				{
					// Get Name of first added Attribute
					CString	zName;
					m_comboAttr.SetCurSel( iFirstNewIndex );
					m_comboAttr.GetLBText( iFirstNewIndex, zName );

					// Tell the Rule Tester dialog know what the current attribute is
					m_apRuleTesterDlg->setCurrentAttributeName( zName.operator LPCTSTR() );

					// Also store Info pointer
					m_ipInfo = ipFirstInfo;

					// Update the display for this Attribute
					refreshUIFromAttribute();
				}
			}
		}
		break;

		case kRulesList:
		{
			// Test ClipboardManager object to see if it is a vector of AttributeRules
			IUnknownPtr	ipObject( NULL );
			if (m_ipClipboardMgr->ObjectIsIUnknownVectorOfType( IID_IAttributeRule ))
			{
				// Retrieve object from ClipboardManager
				ipObject = m_ipClipboardMgr->GetObjectInClipboard();
				ASSERT_RESOURCE_ALLOCATION( "ELI05507", ipObject != __nullptr );
			}
			else
			{
				// Throw exception, object is not an Attribute
				throw UCLIDException( "ELI05508", 
					"Clipboard object is not a vector of Attribute Rules!" );
			}

			// Get index of first selection
			int iIndex = m_listRules.GetFirstSelectedRow();

			// Check for item count if no selection
			if (iIndex == -1)
			{
				iIndex = m_listRules.GetNumberRows();
			}

			// Retrieve vector of existing Rules
			IIUnknownVectorPtr	ipRules = m_ipInfo->GetAttributeRules();
			if (ipRules == __nullptr)
			{
				// Create and throw exception
				throw UCLIDException("ELI19128", 
					"Unable to retrieve Attribute Rules!");
			}

			// Get count of Rules in Clipboard vector
			IIUnknownVectorPtr	ipPastedRules = ipObject;
			ASSERT_RESOURCE_ALLOCATION( "ELI05527", ipPastedRules != __nullptr );
			int iCount = ipPastedRules->Size();

			clearListSelection();
			// Add each Rule to the list and the vector
			for (int i = 0; i < iCount; i++)
			{
				// Retrieve rule description
				UCLID_AFCORELib::IAttributeRulePtr	ipNewRule = ipPastedRules->At( i );
				ASSERT_RESOURCE_ALLOCATION( "ELI19129", ipNewRule != __nullptr );
				bool bIgnoreErrors = asCppBool( ipNewRule->IgnoreErrors );
				string	strDescription( ipNewRule->GetDescription() );

				// Add the item without text in Enabled column
				m_listRules.InsertRow(iIndex + i);
				// Default to enabled, but keep previous rule's ignore error state.
				m_listRules.SetRowInfo(iIndex + i, true, bIgnoreErrors, strDescription.c_str());

				// Insert the new Rule
				ipRules->Insert( iIndex + i, ipNewRule );

				// select the new item
				m_listRules.SelectRow(iIndex+i);
			}
			m_listRules.SetFocus();

			// Refresh the display
			UpdateData( TRUE );
		}
		break;

		case kValidator:
		{
			// Test ClipboardManager object to see if it is an Input Validator
			IUnknownPtr	ipObject( NULL );
			if (m_ipClipboardMgr->ObjectIsTypeWithDescription( IID_IInputValidator ))
			{
				// Retrieve object from ClipboardManager
				ipObject = m_ipClipboardMgr->GetObjectInClipboard();
				ASSERT_RESOURCE_ALLOCATION( "ELI05502", ipObject != __nullptr );
			}
			else
			{
				// Throw exception, object is not an Input Validator
				throw UCLIDException( "ELI05503", "Clipboard object is not an Input Validator." );
			}

			if (m_ipInfo != __nullptr)
			{
				// Set Input Validator
				IObjectWithDescriptionPtr	ipIV = ipObject;
				if (ipIV != __nullptr)
				{
					m_ipInfo->InputValidator = ipIV;
					
					// Display the IV description
					m_zIVDescription = (char *) ipIV->GetDescription();

					// Retrieve the Input Validator
					IObjectWithDescriptionPtr ipOrigIV = m_ipInfo->InputValidator;
					ASSERT_RESOURCE_ALLOCATION("ELI15514", ipOrigIV != __nullptr);

					// enable and set the value of the checkbox
					GetDlgItem( IDC_CHECK_INPUT_VALIDATOR )->EnableWindow( TRUE );
					if( ipOrigIV->GetEnabled() == VARIANT_TRUE )
					{
						m_bInputValidator= TRUE;
					}
					else
					{
						m_bInputValidator = FALSE;
					}
					UpdateData( FALSE );
				}
			}
		}
		break;

		case kSplitter:
		{
			// Test ClipboardManager object to see if it is an Attribute Splitter
			IUnknownPtr	ipObject( NULL );
			if (m_ipClipboardMgr->ObjectIsTypeWithDescription( IID_IAttributeSplitter ))
			{
				// Retrieve object from ClipboardManager
				ipObject = m_ipClipboardMgr->GetObjectInClipboard();
				ASSERT_RESOURCE_ALLOCATION( "ELI05504", ipObject != __nullptr );
			}
			else
			{
				// Throw exception, object is not an Attribute Splitter
				throw UCLIDException( "ELI05505", "Clipboard object is not an Attribute Splitter." );
			}

			if (m_ipInfo != __nullptr)
			{
				// Set Attribute Splitter
				IObjectWithDescriptionPtr	ipSplit = ipObject;
				if (ipSplit != __nullptr)
				{
					m_ipInfo->AttributeSplitter = ipSplit;
					
					// Display the Attribute Splitter description
					m_zAttributeSplitterDescription = (char *) ipSplit->GetDescription();

					// Retrieve the existing Splitter
					IObjectWithDescriptionPtr ipSplitter = m_ipInfo->AttributeSplitter;
					ASSERT_RESOURCE_ALLOCATION("ELI15515", ipSplitter != __nullptr);

					// enable and set the value of the checkbox
					GetDlgItem( IDC_CHECK_ATT_SPLITTER )->EnableWindow( TRUE );
					if( ipSplitter->GetEnabled() == VARIANT_TRUE )
					{
						m_bAttSplitter = TRUE;
					}
					else
					{
						m_bAttSplitter = FALSE;
					}
					
					UpdateData( FALSE );
				}
			}
		}
		break;

		case kPreprocessor:
		{
			// Test ClipboardManager object to see if it is a Document Preprocessor
			IUnknownPtr	ipObject(NULL);
			if (m_ipClipboardMgr->ObjectIsTypeWithDescription(IID_IDocumentPreprocessor))
			{
				// Retrieve object from ClipboardManager
				ipObject = m_ipClipboardMgr->GetObjectInClipboard();
				ASSERT_RESOURCE_ALLOCATION( "ELI06065", ipObject != __nullptr );
			}
			else
			{
				// Throw exception, object is not a Document Preprocessor
				throw UCLIDException( "ELI06066", "Clipboard object is not a Document Preprocessor." );
			}

			// Set Document Preprocessor
			IObjectWithDescriptionPtr ipPreprocessor = ipObject;
			if (ipPreprocessor != __nullptr)
			{
				m_ipRuleSet->GlobalDocPreprocessor = ipPreprocessor;

				// Display the Document Preprocessor description
				m_zPPDescription = (char *) ipPreprocessor->GetDescription();

				// enable and set the value of the checkbox
				GetDlgItem( IDC_CHECK_DOCUMENT_PP )->EnableWindow( TRUE );
				if( ipPreprocessor->GetEnabled() == VARIANT_TRUE )
				{
					m_bDocumentPP = TRUE;
				}
				else
				{
					m_bDocumentPP = FALSE;
				}
				UpdateData(FALSE);
			}
		}
		break;

		case kOutputHandler:
		{
			// Test ClipboardManager object to see if it is an Output Handler
			IUnknownPtr	ipObject(NULL);
			if (m_ipClipboardMgr->ObjectIsTypeWithDescription(IID_IOutputHandler))
			{
				// Retrieve object from ClipboardManager
				ipObject = m_ipClipboardMgr->GetObjectInClipboard();
				ASSERT_RESOURCE_ALLOCATION( "ELI07733", ipObject != __nullptr );
			}
			else
			{
				// Throw exception, object is not an Output Handler
				throw UCLIDException( "ELI07734", "Clipboard object is not an Output Handler." );
			}

			// Set Output Handlerr
			IObjectWithDescriptionPtr ipOH = ipObject;
			if (ipOH != __nullptr)
			{
				m_ipRuleSet->GlobalOutputHandler = ipOH;

				// Display the Output Handler description
				m_zOutputHandlerDescription = (char *) ipOH->GetDescription();
				
				// enable and set the value of the checkbox
				GetDlgItem( IDC_CHECK_OUTPUT_HANDLER )->EnableWindow( TRUE );
				if( ipOH->GetEnabled() == VARIANT_TRUE )
				{
					m_bOutputHandler = TRUE;
				}
				else
				{
					m_bOutputHandler = FALSE;
				}
				UpdateData( FALSE );
			}
		}
		break;

		default:
			// Throw exception
			UCLIDException ue( "ELI05498", "Unable to Paste item." );
			ue.addDebugInfo( "Item Type Number", m_eContextMenuCtrl );
			throw ue;
			break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05424")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnEditDelete() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check control
		switch (m_eContextMenuCtrl)
		{
		case kAttributes:
			OnBtnDeleteAttribute();
			break;

		case kRulesList:
			OnBtnDeleteRule();
			break;

		case kValidator:
		{
			// Retrieve Input Validator
			IObjectWithDescriptionPtr ipIV = m_ipInfo->InputValidator;
			ASSERT_RESOURCE_ALLOCATION("ELI15516", ipIV != __nullptr);

			// Retrieve existing IV description
			CString	zDesc;
			zDesc = (char *)ipIV->Description;

			// Request confirmation
			CString	zPrompt;
			int		iResult;
			zPrompt.Format( "Are you sure that Input Validator '%s' should be deleted?", 
				zDesc );
			iResult = MessageBox( zPrompt.operator LPCTSTR(), "Confirm Delete", 
				MB_YESNO | MB_ICONQUESTION );

			// Act on response
			if (iResult == IDYES)
			{
				// Clear Input Validator object
				m_ipInfo->InputValidator = NULL;

				// clear the description
				m_zIVDescription.Empty();

				// disable the checkbox
				GetDlgItem( IDC_CHECK_INPUT_VALIDATOR )->EnableWindow( FALSE );
				m_bInputValidator = FALSE;
				UpdateData(FALSE);
			}
		}
		break;

		case kSplitter:
		{
			// Retrieve the Splitter
			IObjectWithDescriptionPtr ipSplitter = m_ipInfo->AttributeSplitter;
			ASSERT_RESOURCE_ALLOCATION("ELI15517", ipSplitter != __nullptr);

			// Retrieve existing Splitter description
			CString	zDesc;
			zDesc = (char *)ipSplitter->Description;

			// Request confirmation
			CString	zPrompt;
			int		iResult;
			zPrompt.Format( "Are you sure that Attribute Splitter '%s' should be deleted?", 
				zDesc );
			iResult = MessageBox( zPrompt.operator LPCTSTR(), "Confirm Delete", 
				MB_YESNO | MB_ICONQUESTION );

			// Act on response
			if (iResult == IDYES)
			{
				// Clear Attribute Splitter object and description
				m_ipInfo->AttributeSplitter = NULL;
				m_zAttributeSplitterDescription.Empty();

				// disable the checkbox
				GetDlgItem( IDC_CHECK_ATT_SPLITTER )->EnableWindow( FALSE );
				m_bAttSplitter = FALSE;
				UpdateData(FALSE);
			}
		}
		break;

		case kPreprocessor:
		{
			// Request confirmation
			CString	zPrompt;
			int		iResult;
			zPrompt.Format("Are you sure that Document Preprocessor '%s' should be deleted?", 
				m_zPPDescription);
			iResult = MessageBox( zPrompt.operator LPCTSTR(), "Confirm Delete", 
				MB_YESNO | MB_ICONQUESTION );

			// Act on response
			if (iResult == IDYES)
			{
				// Clear Document Preprocessor object and description
				m_ipRuleSet->GlobalDocPreprocessor = NULL;
				m_zPPDescription.Empty();

				// disable the checkbox
				GetDlgItem( IDC_CHECK_DOCUMENT_PP )->EnableWindow( FALSE );
				m_bDocumentPP = FALSE;
				UpdateData(FALSE);
			}
		}
		break;

		case kOutputHandler:
		{
			// Request confirmation
			CString	zPrompt;
			int		iResult;
			zPrompt.Format("Are you sure that Output Handler '%s' should be deleted?", 
				m_zOutputHandlerDescription);
			iResult = MessageBox( zPrompt.operator LPCTSTR(), "Confirm Delete", 
				MB_YESNO | MB_ICONQUESTION );

			// Act on response
			if (iResult == IDYES)
			{
				// Clear Output Handler object and description
				m_ipRuleSet->GlobalOutputHandler = NULL;
				m_zOutputHandlerDescription.Empty();

				// disable the checkbox
				GetDlgItem( IDC_CHECK_OUTPUT_HANDLER )->EnableWindow( FALSE );
				m_bOutputHandler = FALSE;
				UpdateData( FALSE );
			}
		}
		break;

		default:
			// Throw exception
			UCLIDException ue( "ELI05425", "Unable to Delete item." );
			ue.addDebugInfo( "Item Type Number", m_eContextMenuCtrl );
			throw ue;
			break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05404")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnEditDuplicate() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check control
		switch (m_eContextMenuCtrl)
		{
		case kAttributes:
		{
			// Copy the Attribute and then Paste it
			OnEditCopy();
			OnEditPaste();
		}
		break;

		default:
			// Throw exception
			UCLIDException ue( "ELI05535", "Unable to Duplicate item." );
			ue.addDebugInfo( "Item Type Number", m_eContextMenuCtrl );
			throw ue;
			break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05536")
}
//-------------------------------------------------------------------------------------------------
