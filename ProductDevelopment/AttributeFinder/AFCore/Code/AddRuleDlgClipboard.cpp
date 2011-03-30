#include "stdafx.h"
#include "afcore.h"
#include "AddRuleDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// Column widths
const int giENABLE_LIST_COLUMN = 0;
const int giDESC_LIST_COLUMN = 1;
//-------------------------------------------------------------------------------------------------
// CAddRuleDlg
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnEditCut() 
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05518")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnEditCopy() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check control
		switch (m_eContextMenuCtrl)
		{
			case kRulesList:
			{
				// Check for current rule selection
				int iIndex = -1;
				POSITION pos = m_listRules.GetFirstSelectedItemPosition();
				if (pos != __nullptr)
				{
					// Get index of first selection
					iIndex = m_listRules.GetNextSelectedItem( pos );
				}

				if (iIndex == -1)
				{
					// Throw exception
					throw UCLIDException( "ELI05519", 
						"Unable to determine selected Attribute Modifying Rule!" );
				}

				// Create a vector for selected rules
				IIUnknownVectorPtr	ipCopiedRules( CLSID_IUnknownVector );
				ASSERT_RESOURCE_ALLOCATION( "ELI05529", ipCopiedRules != __nullptr );

				// Add each selected rule to vector
				while (iIndex != -1)
				{
					// Retrieve the selected rule
					IUnknownPtr	ipObject = m_ipAMRulesVector->At( iIndex );
					ASSERT_RESOURCE_ALLOCATION( "ELI05576", ipObject != __nullptr );

					// Add the rule to the vector
					ipCopiedRules->PushBack( ipObject );

					// Get the next selection
					iIndex = m_listRules.GetNextSelectedItem( pos );
				}

				// ClipboardManager will handle the Copy
				m_ipClipboardMgr->CopyObjectToClipboard( ipCopiedRules );
			}
			break;

			// Document Preprocessor
			case kPreprocessor:
			{
				// Retrieve existing Document Preprocessor
				IUnknownPtr	ipObject = m_ipDocPreprocessor;
				ASSERT_RESOURCE_ALLOCATION( "ELI06116", ipObject != __nullptr );

				// ClipboardManager will handle the Copy
				m_ipClipboardMgr->CopyObjectToClipboard( ipObject );
			}
			break;

			default:
				// Create and throw exception
				UCLIDException ue( "ELI06176", "Unable to Copy item." );
				ue.addDebugInfo( "Item Type Number", m_eContextMenuCtrl );
				throw ue;
				break;
		}
  	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05522")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnEditPaste() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check control
		switch (m_eContextMenuCtrl)
		{
			// AM Rules List
			case kRulesList:
			{
				// Test ClipboardManager object
				IUnknownPtr	ipObject( __nullptr );
				bool	bSingleRule = false;
				if (m_ipClipboardMgr->ObjectIsIUnknownVectorOfType( 
					IID_IObjectWithDescription ))
				{
					// Object is a vector of ObjectWithDescription items
					// We expect each embedded object to be an AM Rule
					ipObject = m_ipClipboardMgr->GetObjectInClipboard();
					ASSERT_RESOURCE_ALLOCATION( "ELI05523", ipObject != __nullptr );
				}
				else if (m_ipClipboardMgr->ObjectIsTypeWithDescription( 
					IID_IAttributeModifyingRule ))
				{
					// Object is a single ObjectWithDescription item
					// We expect the embedded object to be an AM Rule
					ipObject = m_ipClipboardMgr->GetObjectInClipboard();
					ASSERT_RESOURCE_ALLOCATION( "ELI08068", ipObject != __nullptr );
					bSingleRule = true;
				}
				else
				{
					// Throw exception, object is not a vector of ObjectWithDescription items
					throw UCLIDException( "ELI05524", 
						"Clipboard object is not a Attribute Modifying Rule!" );
				}

				// Check for current rule selection
				int iIndex = -1;
				POSITION pos = m_listRules.GetFirstSelectedItemPosition();
				if (pos != __nullptr)
				{
					// Get index of first selection
					iIndex = m_listRules.GetNextSelectedItem( pos );
				}

				// Check for item count if no selection
				if (iIndex == -1)
				{
					iIndex = m_listRules.GetItemCount();
				}

				clearListSelection();

				// Handle single-rule case
				if (bSingleRule)
				{
					// Retrieve rule and description
					IObjectWithDescriptionPtr	ipNewRule = ipObject;
					ASSERT_RESOURCE_ALLOCATION( "ELI08069", ipNewRule != __nullptr );
					string	strDescription( ipNewRule->GetDescription() );

					// Insert the text into the list
					m_listRules.InsertItem( iIndex, "");
					CString zDescription = strDescription.c_str();

					// Restore the description
					m_listRules.SetItemText( iIndex , giDESC_LIST_COLUMN, 
						zDescription.operator LPCTSTR() );

					// Manage the checkbox
					VARIANT_BOOL vbEnabled = ipNewRule->GetEnabled();
					if(vbEnabled == VARIANT_TRUE)
					{
						m_listRules.SetCheck( iIndex, TRUE );
					}
					else
					{
						m_listRules.SetCheck( iIndex, FALSE );
					}

					// Insert the Modifying Rule object-with-description into the vector
					m_ipAMRulesVector->Insert( iIndex, ipNewRule );

					// Select the new item
					m_listRules.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
				}
				// Handle vector of one-or-more rules case
				else
				{
					// Get count of Rules in Clipboard vector
					IIUnknownVectorPtr	ipPastedRules = ipObject;
					ASSERT_RESOURCE_ALLOCATION( "ELI05520", ipPastedRules != __nullptr );
					int iCount = ipPastedRules->Size();

					// Add each Rule to the list and the vector
					for (int i = 0; i < iCount; i++)
					{
						// Retrieve rule and description
						IObjectWithDescriptionPtr	ipNewRule = ipPastedRules->At( i );
						ASSERT_RESOURCE_ALLOCATION( "ELI05577", ipNewRule != __nullptr );
						string	strDescription( ipNewRule->GetDescription() );

						// Insert the text into the list
						m_listRules.InsertItem( iIndex, "");
						CString zDescription = strDescription.c_str();

						// Restore the description
						m_listRules.SetItemText( iIndex , giDESC_LIST_COLUMN, 
							zDescription.operator LPCTSTR() );

						// Manage the checkbox
						VARIANT_BOOL vbEnabled = ipNewRule->GetEnabled();
						if(vbEnabled == VARIANT_TRUE)
						{
							m_listRules.SetCheck( iIndex, TRUE );
						}
						else
						{
							m_listRules.SetCheck( iIndex, FALSE );
						}
						// Insert the Modifying Rule object-with-description into the vector
						m_ipAMRulesVector->Insert( iIndex + i, ipNewRule );

						// select the new item
						m_listRules.SetItemState( iIndex+i, LVIS_SELECTED, LVIS_SELECTED );
					}
				}

				m_listRules.SetFocus();

				// Update the display
				UpdateData( FALSE );

				// Update button states
				setButtonStates();
			}
			break;

			// Document Preprocessor
			case kPreprocessor:
			{
				// Test ClipboardManager object to see if it is an Document Preprocessor
				IUnknownPtr	ipObject( __nullptr );
				if (m_ipClipboardMgr->ObjectIsTypeWithDescription( IID_IDocumentPreprocessor ))
				{
					// Retrieve object from ClipboardManager
					ipObject = m_ipClipboardMgr->GetObjectInClipboard();
					ASSERT_RESOURCE_ALLOCATION( "ELI06117", ipObject != __nullptr );
				}
				else
				{
					// Throw exception, object is not an Document Preprocessor
					throw UCLIDException( "ELI06127", 
						"Clipboard object is not an Document Preprocessor." );
				}

				if (m_ipRule != __nullptr)
				{
					// Set Document Preprocessor
					IObjectWithDescriptionPtr	ipPre = ipObject;
					if (ipPre != __nullptr)
					{
						m_ipDocPreprocessor = ipPre;
						
						// Display the Document Preprocessor description
						m_zPPDescription = (char *) ipPre->GetDescription();

						// enable the checkbox, and set it's value accordingly
						GetDlgItem( IDC_CHECK_AFRULE_DOC_PP )->EnableWindow( TRUE );
						if( ipPre->GetEnabled() == VARIANT_TRUE )
						{
							m_bDocPP = TRUE;
						}
						else
						{
							m_bDocPP = FALSE;
						}

						UpdateData( FALSE );
					}
				}
			}
			break;

			default:
				// Create and throw exception
				UCLIDException ue( "ELI06177", "Unable to Paste item." );
				ue.addDebugInfo( "Item Type Number", m_eContextMenuCtrl );
				throw ue;
				break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05526")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnEditDelete() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check control
		switch (m_eContextMenuCtrl)
		{
			// AM Rules List
			case kRulesList:
				// Call the existing handler
				OnBtnDeleteRule();
				break;

			// Document Preprocessor
			case kPreprocessor:
			{
				// Retrieve existing Preprocessor description
				CString	zDesc;
				zDesc = (char *)m_ipDocPreprocessor->Description;

				// Request confirmation
				CString	zPrompt;
				int		iResult;
				zPrompt.Format( "Are you sure that Document Preprocessor '%s' should be deleted?", 
					zDesc );
				iResult = MessageBox( zPrompt.operator LPCTSTR(), "Confirm Delete", 
					MB_YESNO | MB_ICONQUESTION );

				// Act on response
				if (iResult == IDYES)
				{
					// Clear Document Preprocessor object
					m_ipDocPreprocessor = __nullptr;
					m_ipRule->RuleSpecificDocPreprocessor = __nullptr;
						
					// Display the empty Document Preprocessor description
					m_zPPDescription = "";
					GetDlgItem( IDC_CHECK_AFRULE_DOC_PP )->EnableWindow( FALSE );
					m_bDocPP = FALSE;
					UpdateData( FALSE );
				}
			}
			break;

			default:
				// Create and throw exception
				UCLIDException ue( "ELI06178", "Unable to Delete item." );
				ue.addDebugInfo( "Item Type Number", m_eContextMenuCtrl );
				throw ue;
				break;
		}

		// Update button states
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05528")
}
//-------------------------------------------------------------------------------------------------
