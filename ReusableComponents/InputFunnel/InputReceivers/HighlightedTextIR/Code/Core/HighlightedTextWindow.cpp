//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	HighlightedTextWindow.cpp
//
// PURPOSE:	Implementation of HighlightedTextWindow class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "HighlightedTextIR.h"
#include "HighlightedTextWindow.h"
#include "HighlightedTextDlg.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IHighlightedTextWindow,
		&IID_IInputEntityManager,
		&IID_IInputReceiver,
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent
	};
	
	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}
	
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// CHighlightedTextWindow
//-------------------------------------------------------------------------------------------------
CHighlightedTextWindow::CHighlightedTextWindow()
: m_lParentWndHandle(0)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_apDlg.reset(NULL);
}
//-------------------------------------------------------------------------------------------------
CHighlightedTextWindow::~CHighlightedTextWindow()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Force deletion of the dialog object within the scope of this 
		// destructor so that we know the destruction is happening with the 
		// correct AFX module state
		m_apDlg.reset(NULL);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20396");
}

//-------------------------------------------------------------------------------------------------
// IHighlightedTextWindow
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::Open(BSTR strFilename)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Convert the filename to std format
		string	strName = asString( strFilename );

		// Pass the converted filename on to the dialog
		getHighlightedTextDlg()->Open( strName );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02670")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::Save()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Pass the command on to the dialog
		getHighlightedTextDlg()->Save();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02848")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::SaveAs(BSTR strFilename)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Convert the filename to std format
		string	strName = asString( strFilename );

		// Pass the converted filename on to the dialog
		getHighlightedTextDlg()->SaveAs( strName );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02671")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Pass the instruction on to the dialog
		getHighlightedTextDlg()->Clear();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02672")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::SetText(BSTR strText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Convert the text to std format
		string	stdstrText = asString( strText );

		// Pass the converted text on to the dialog
		getHighlightedTextDlg()->SetText( stdstrText );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02673")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::SetInputFinder(BSTR strName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Convert the finder name to std format
		string	stdstrName = asString( strName );

		// Pass the converted name on to the dialog
		getHighlightedTextDlg()->SetInputFinder( stdstrName );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02674")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::IsModified(VARIANT_BOOL *pbIsModified)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Check parameter
		if (pbIsModified == NULL)
		{
			return E_POINTER;
		}

		// Provide the dialog's modified status
		*pbIsModified = getHighlightedTextDlg()->IsModified() == TRUE ? 
				VARIANT_TRUE : VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02675")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::GetFileName(BSTR *pstrFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pstrFileName == NULL)
		{
			return E_POINTER;
		}

		// Convert string format
		_bstr_t _bstrName( getHighlightedTextDlg()->GetSourceName().c_str() );
		*pstrFileName = _bstrName.copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02805")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::GetInputFinderName(BSTR *pstrName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pstrName == NULL)
		{
			return E_POINTER;
		}

		// Retrieve Finder name from dialog
		string strName = getHighlightedTextDlg()->GetFinderName();

		// Convert string format
		_bstr_t _bstrName = strName.c_str();
		*pstrName = _bstrName.copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02806")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::GetText(BSTR *pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pstrText == NULL)
		{
			return E_POINTER;
		}

		// Retrieve text from dialog
		string strText = getHighlightedTextDlg()->GetText();

		// Convert string format
		_bstr_t _bstrText = strText.c_str();
		*pstrText = _bstrText.copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02807")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::SetIndirectSource(BSTR strIndirectSourceName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		getHighlightedTextDlg()->setIndirectSourceName( asString( strIndirectSourceName ) );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03130")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::ShowOpenDialogBox()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		getHighlightedTextDlg()->showOpenDialog();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03280")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::AddInputFinder(BSTR strInputFinderName, 
													IInputFinder *pInputFinder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		getHighlightedTextDlg()->AddInputFinder( asString( strInputFinderName ), pInputFinder );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03851")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::SetTextProcessors(IIUnknownVector *pvecTextProcessors)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		getHighlightedTextDlg()->setTextProcessors(pvecTextProcessors);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04820")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::ClearTextProcessors()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		getHighlightedTextDlg()->clearTextProcessors();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04821")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputEntityManager
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_CanBeDeleted(BSTR strID, 
													  VARIANT_BOOL *pbCanBeDeleted)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Check parameter
		if (pbCanBeDeleted == NULL)
		{
			return E_POINTER;
		}
		
		/////////////////////////////////////
		// Ignore strID, all entities are 
		// assumed to be non-deletable
		/////////////////////////////////////

		// Provide false
		*pbCanBeDeleted = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02855")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_Delete(BSTR strID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Check for empty ID string
		string stdstrID = asString( strID );
		if (stdstrID.length() == 0)
		{
			// ID string is empty --> cannot be deleted
			UCLIDException ue( "ELI03079", "Entity ID is an empty string.");
			throw ue;
		}

		// Convert ID to long
		long lEntityID = asLong( stdstrID );

		// Confirm that entity ID is valid
		if (getHighlightedTextDlg()->IsEntityValid( lEntityID ))
		{
			// Only continue if this entity can be deleted
			VARIANT_BOOL	vbCanBeDeleted;
			raw_CanBeDeleted( strID, &vbCanBeDeleted );

			if (vbCanBeDeleted == VARIANT_TRUE)
			{
				// Provide new (blank) entity text to dialog
				string	strNewText = "";

				getHighlightedTextDlg()->SetEntityText( lEntityID, strNewText );
			}
		}
		else
		{
			// Invalid ID
			UCLIDException ue( "ELI02849", "Invalid entity ID.");
			ue.addDebugInfo( "Entity ID", lEntityID );
			throw ue;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02676")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_SetText(BSTR strID, BSTR strText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Check for empty ID string
		string stdstrID = asString( strID );
		if (stdstrID.length() == 0)
		{
			// ID string is empty --> cannot set text
			UCLIDException ue( "ELI03080", "Entity ID is an empty string.");
			throw ue;
		}

		// Convert ID to long
		long lEntityID = asLong( stdstrID );

		// Confirm that entity ID is valid
		if (getHighlightedTextDlg()->IsEntityValid( lEntityID ))
		{
			// Convert the new text to std format
			_bstr_t bstrNewText = strText;
			string	strNewText = bstrNewText.operator char *();

			// Provide new entity text to dialog
			getHighlightedTextDlg()->SetEntityText( lEntityID, strNewText );
		}
		else
		{
			// Invalid ID
			UCLIDException ue( "ELI02850", "Invalid entity ID.");
			ue.addDebugInfo( "Entity ID", lEntityID );
			throw ue;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02677")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_GetText(BSTR strID, BSTR *pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Check parameter
		if (pstrText == NULL)
		{
			return E_POINTER;
		}
		
		// Check for empty ID string
		string stdstrID = asString( strID );
		if (stdstrID.length() == 0)
		{
			// ID string is empty --> cannot get text
			UCLIDException ue( "ELI03081", "Entity ID is an empty string.");
			throw ue;
		}

		// Convert ID to long
		long lEntityID = asLong( stdstrID );

		// Confirm that entity ID is valid
		if (getHighlightedTextDlg()->IsEntityValid( lEntityID ))
		{
			// Retrieve entity text from dialog
			string strEntityText = getHighlightedTextDlg()->GetEntityText( 
				lEntityID );

			// Convert entity text string format
			_bstr_t _bstrText = strEntityText.c_str();
			*pstrText = _bstrText.copy();
		}
		else
		{
			// Invalid ID
			UCLIDException ue( "ELI02851", "Invalid entity ID.");
			ue.addDebugInfo( "Entity ID", lEntityID );
			throw ue;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02678")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_CanBeMarkedAsUsed(BSTR strID, 
														   VARIANT_BOOL *pbCanBeMarkedAsUsed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Check parameter
		if (pbCanBeMarkedAsUsed == NULL)
		{
			return E_POINTER;
		}
		
		// Check for empty ID string
		string stdstrID = asString( strID );
		if (stdstrID.length() == 0)
		{
			// ID string is empty --> cannot be marked as used
			*pbCanBeMarkedAsUsed = VARIANT_FALSE;
			return S_OK;
		}

		// Convert ID to long
		long lEntityID = asLong( stdstrID );

		// Validate strID
		if (getHighlightedTextDlg()->IsEntityValid( lEntityID ))
		{
			// Entity is valid, so can be marked as used
			*pbCanBeMarkedAsUsed = VARIANT_TRUE;
		}
		else
		{
			// Invalid ID
			UCLIDException ue( "ELI02852", "Invalid entity ID.");
			ue.addDebugInfo( "Entity ID", lEntityID );
			throw ue;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02679")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_MarkAsUsed(BSTR strID, 
													VARIANT_BOOL bValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Check for empty ID string
		string stdstrID = asString( strID );
		if (stdstrID.length() == 0)
		{
			// ID string is empty --> cannot mark as used
			UCLIDException ue( "ELI03082", "Entity ID is an empty string.");
			throw ue;
		}

		// Convert ID to long
		long lEntityID = asLong( stdstrID );

		// Convert VARIANT_BOOL
		bool	bUsed = false;
		if (bValue == VARIANT_TRUE)
		{
			bUsed = true;
		}

		// Validate strID
		if (getHighlightedTextDlg()->IsEntityValid( lEntityID ))
		{
			// Entity is valid, so mark as used
			getHighlightedTextDlg()->MarkAsUsed( lEntityID, bUsed );
		}
		else
		{
			// Invalid ID
			UCLIDException ue( "ELI02853", "Invalid entity ID.");
			ue.addDebugInfo( "Entity ID", lEntityID );
			throw ue;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02680")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_IsMarkedAsUsed(BSTR strID, 
														VARIANT_BOOL *pbIsMarkedAsUsed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Check parameter
		if (pbIsMarkedAsUsed == NULL)
		{
			return E_POINTER;
		}

		// Check for empty ID string
		string stdstrID = asString( strID );
		if (stdstrID.length() == 0)
		{
			// ID string is empty --> cannot check marked as used
			UCLIDException ue( "ELI03083", "Entity ID is an empty string.");
			throw ue;
		}

		// Convert ID to long
		long lEntityID = asLong( stdstrID );

		// Validate strID
		if (getHighlightedTextDlg()->IsEntityValid( lEntityID ))
		{
			// Entity is valid, so check status
			if (getHighlightedTextDlg()->IsMarkedAsUsed( lEntityID ))
			{
				// Entity is valid and used
				*pbIsMarkedAsUsed = VARIANT_TRUE;
			}
			else
			{
				// Entity is valid and not used
				*pbIsMarkedAsUsed = VARIANT_FALSE;
			}
		}
		else
		{
			// Invalid ID
			UCLIDException ue( "ELI02854", "Invalid entity ID.");
			ue.addDebugInfo( "Entity ID", lEntityID );
			throw ue;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02681")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_IsFromPersistentSource(BSTR strID, 
															VARIANT_BOOL *pbIsFromPersistentSource)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Check parameter
		if (pbIsFromPersistentSource == NULL)
		{
			return E_POINTER;
		}

		/////////////////////////////////////
		// Ignore strID, all entities are 
		// assumed to be from the same source
		/////////////////////////////////////

		// Provide the dialog's flag state
		*pbIsFromPersistentSource = getHighlightedTextDlg()->
			IsPersistentSource() ? VARIANT_TRUE : VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02682")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_GetPersistentSourceName(BSTR strID, 
															 BSTR *pstrSourceName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		if (pstrSourceName == NULL)
		{
			return E_POINTER;
		}

		/////////////////////////////////////
		// Ignore strID, all entities are 
		// assumed to be from the same source
		/////////////////////////////////////

		// Retrieve source name from dialog
		string strSourceName(getHighlightedTextDlg()->GetSourceName());

		// Convert source name string format
		_bstr_t _bstrName (strSourceName.c_str());
		*pstrSourceName = _bstrName.copy();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02683")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_GetOCRImage(BSTR strID, BSTR* pbstrImageFileName)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState())
			
		validateLicense();

		// does not have any ocr image
		*pbstrImageFileName = _bstr_t("").copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03001")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_HasBeenOCRed(BSTR strID, 
												  VARIANT_BOOL *pbHasBeenOCRed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		if (pbHasBeenOCRed == NULL)
		{
			return E_POINTER;
		}

		// Always return false, as any effect from OCR status will be resolved 
		// before creating a Highlighted Text window as an input receiver
		*pbHasBeenOCRed = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02684")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_HasIndirectSource(BSTR strID, VARIANT_BOOL *pbHasIndirectSource)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbHasIndirectSource = VARIANT_FALSE;

		if (!getHighlightedTextDlg()->getIndirectSourceName().empty())
		{
			*pbHasIndirectSource = VARIANT_TRUE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03128")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_GetIndirectSource(BSTR strID, BSTR *pstrIndirectSourceName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pstrIndirectSourceName = _bstr_t(getHighlightedTextDlg()->getIndirectSourceName().c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03129")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_GetOCRZones(BSTR strID, IIUnknownVector **pRasterZones)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// this CoClass is not capable of returning OCR Zone information
		*pRasterZones = NULL;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03308")
}

//-------------------------------------------------------------------------------------------------
// IInputReceiver
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_get_ParentWndHandle(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_lParentWndHandle;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03025")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_put_ParentWndHandle(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_lParentWndHandle = newVal;

		// if the dialog object has already been created, then update its Parent
		if (m_apDlg.get() != NULL)
		{
			CWnd *pParentWnd = m_lParentWndHandle == NULL ? NULL : CWnd::FromHandle((HWND) m_lParentWndHandle);
			m_apDlg->SetParent(pParentWnd);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03026")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_get_WindowShown(VARIANT_BOOL * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// Provide the dialog's visibility status
		*pVal = getHighlightedTextDlg()->IsWindowVisible() == TRUE ? 
				VARIANT_TRUE : VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02656")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_get_InputIsEnabled(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		if (pbVal == NULL)
		{
			return E_POINTER;
		}

		// Check dialog to see if input is enabled
		*pbVal = getHighlightedTextDlg()->IsInputEnabled() ? 
			VARIANT_TRUE : VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02657")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_get_HasWindow(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		if (pbVal == NULL)
		{
			return E_POINTER;
		}

		// Always return true, as the Highlighted Text window as an 
		// input receiver always has a window
		*pbVal = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02658")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_get_WindowHandle(LONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// Retrieve dialog window handle
		HWND hWnd = getHighlightedTextDlg()->m_hWnd;
		*pVal = (long) hWnd;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02659")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_EnableInput(BSTR strInputType,
													 BSTR strPrompt)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Convert prompt from COM-style to string
		string stdstrPrompt = asString( strPrompt );

		// Enable input at the UI level
		getHighlightedTextDlg()->EnableText( stdstrPrompt );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02660")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_DisableInput()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		// Disable input at the UI level
		getHighlightedTextDlg()->DisableText();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02661")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_SetEventHandler(IIREventHandler * pEventHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		getHighlightedTextDlg()->SetEventHandler( pEventHandler );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02662")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_ShowWindow(VARIANT_BOOL bShow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		getHighlightedTextDlg()->ShowWindow(
			bShow == VARIANT_TRUE ? SW_SHOW : SW_HIDE);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02663")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_get_UsesOCR(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		*pVal = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03461")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_SetOCRFilter(IOCRFilter *pFilter)
{
	try
	{
		// Confirm that component is licensed
		validateLicense();

		throw UCLIDException("ELI19483", "The HighlightedTextWindow does not use OCR technologies!");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03462")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_SetOCREngine(IOCREngine *pEngine)
{
	try
	{
		// Confirm that component is licensed
		validateLicense();

		throw UCLIDException("ELI03586", "The HighlightedTextWindow does not use OCR technologies!");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03587")
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		if (pbstrComponentDescription == NULL)
		{
			return E_POINTER;
		}
		
		*pbstrComponentDescription = _bstr_t("Highlighted Text Window").copy();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02664")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightedTextWindow::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Failure of validateLicense is indicated by an exception being 
		// thrown
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
HighlightedTextDlg* CHighlightedTextWindow::getHighlightedTextDlg()
{
	if (m_apDlg.get() == NULL)
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState())
		TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);
		
		// Create the modeless dialog
		m_apDlg = auto_ptr<HighlightedTextDlg>(new HighlightedTextDlg( this ));
		CWnd *pParentWnd = m_lParentWndHandle == NULL ? NULL : CWnd::FromHandle((HWND) m_lParentWndHandle);
		if (!m_apDlg->Create( HighlightedTextDlg::IDD, pParentWnd))
		{
			UCLIDException ue( "ELI02712", 
			"Failed to Create the Highlighted Text dialog.");
			throw ue;
		}
	}
	
	return m_apDlg.get();
}
//-------------------------------------------------------------------------------------------------
void CHighlightedTextWindow::validateLicense()
{
	static const unsigned long HIGHLIGHTEDTEXTWINDOW_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( HIGHLIGHTEDTEXTWINDOW_COMPONENT_ID, 
		"ELI02668", "Highlighted Text Window" );
}
//-------------------------------------------------------------------------------------------------
