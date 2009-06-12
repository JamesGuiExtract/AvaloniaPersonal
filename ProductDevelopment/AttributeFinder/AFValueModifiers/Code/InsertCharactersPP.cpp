// InsertCharactersPP.cpp : Implementation of CInsertCharactersPP
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "InsertCharactersPP.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CInsertCharactersPP
//-------------------------------------------------------------------------------------------------
CInsertCharactersPP::CInsertCharactersPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEInsertCharactersPP;
		m_dwHelpFileID = IDS_HELPFILEInsertCharactersPP;
		m_dwDocStringID = IDS_DOCSTRINGInsertCharactersPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07714")
}
//-------------------------------------------------------------------------------------------------
CInsertCharactersPP::~CInsertCharactersPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16358");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharactersPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CInsertCharactersPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEMODIFIERSLib::IInsertCharactersPtr ipInsertChars = m_ppUnk[i];
			if (ipInsertChars)
			{
				try
				{
					// get chars for insertion
					CComBSTR bstrCharsToInsert;
					GetDlgItemText(IDC_EDIT_CHAR_INSERT, bstrCharsToInsert.m_str);
					try
					{
						ipInsertChars->CharsToInsert = _bstr_t(bstrCharsToInsert);
					}
					CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05805")
				}
				catch (...)
				{
					ATLControls::CEdit editCharsToInsert(GetDlgItem(IDC_EDIT_CHAR_INSERT));
					editCharsToInsert.SetSel(0, -1);
					editCharsToInsert.SetFocus();
					return S_FALSE;
				}
				
				// get position
				bool bAppendToEnd = IsDlgButtonChecked(IDC_RADIO_APPEND)==BST_CHECKED;
				ipInsertChars->AppendToEnd = bAppendToEnd ? VARIANT_TRUE :VARIANT_FALSE;
				int nPosition = -1;
				if (!bAppendToEnd)
				{
					try
					{
						// position must be specified
						nPosition = GetDlgItemInt(IDC_EDIT_POSITION, NULL, FALSE);
						try
						{
							ipInsertChars->InsertAt = nPosition;
						}
						CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05806")
					}
					catch (...)
					{
						ATLControls::CEdit editPosition(GetDlgItem(IDC_EDIT_POSITION));
						editPosition.SetSel(0, -1);
						editPosition.SetFocus();
						return S_FALSE;
					}
				}

				// get length type
				// if current type is "Any"
				int nLengthType = m_cmbLengthType.GetCurSel();
				ipInsertChars->LengthType = (UCLID_AFVALUEMODIFIERSLib::EInsertCharsLengthType)nLengthType;
				long nNumOfCharsLong = -1;
				if (nLengthType > 0)
				{
					try
					{
						// position must be specified
						nNumOfCharsLong = GetDlgItemInt(IDC_EDIT_NUM_OF_CHARS, NULL, FALSE);
						try
						{
							ipInsertChars->NumOfCharsLong = nNumOfCharsLong;
						}
						CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05807")
					}
					catch (...)
					{
						// if length type is not "Any" number of characters
						// chars length must be specified
						ATLControls::CEdit editNumOfChars(GetDlgItem(IDC_EDIT_NUM_OF_CHARS));
						editNumOfChars.SetSel(0, -1);
						editNumOfChars.SetFocus();
						return S_FALSE;
					}
				}
				
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04986");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharactersPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Windows Message Handlers
//-------------------------------------------------------------------------------------------------
LRESULT CInsertCharactersPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_cmbLengthType = GetDlgItem(IDC_CMB_LENGTH_TYPE);
		m_editNumOfCharsLong = GetDlgItem(IDC_EDIT_NUM_OF_CHARS);
		// populate the contents for the combo box
		m_cmbLengthType.AddString("Any");
		m_cmbLengthType.AddString("=");
		m_cmbLengthType.AddString("<=");
		m_cmbLengthType.AddString("<");
		m_cmbLengthType.AddString(">=");
		m_cmbLengthType.AddString(">");
		m_cmbLengthType.AddString("< >");

		// init picture
		m_picPositionInfo = GetDlgItem(IDC_POSITION_INFO);

		// create tooltip object
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		// set no delay.
		m_infoTip.SetShowDelay(0);

		UCLID_AFVALUEMODIFIERSLib::IInsertCharactersPtr ipInsertChars(m_ppUnk[0]);
		if (ipInsertChars)
		{
			string strCharsToInsert = ipInsertChars->CharsToInsert;
			SetDlgItemText(IDC_EDIT_CHAR_INSERT, strCharsToInsert.c_str());
			
			bool bAppendToEnd = ipInsertChars->AppendToEnd==VARIANT_TRUE;
			if (bAppendToEnd)
			{
				CheckDlgButton(IDC_RADIO_APPEND, BST_CHECKED);
				CheckDlgButton(IDC_RADIO_SPECIFIED, BST_UNCHECKED);
				ATLControls::CEdit editPosition(GetDlgItem(IDC_EDIT_POSITION));
				editPosition.EnableWindow(FALSE);
			}
			else
			{
				long nPosition = ipInsertChars->InsertAt;
				CheckDlgButton(IDC_RADIO_SPECIFIED, BST_CHECKED);
				SetDlgItemInt(IDC_EDIT_POSITION, nPosition, FALSE);
			}
			
			long nLengthType = (long)ipInsertChars->LengthType;
			m_cmbLengthType.SetCurSel(nLengthType);
			if (nLengthType == 0)
			{
				// if length type is "Any" number of characters
				// Disable the edit box for entering number of characters
				m_editNumOfCharsLong.EnableWindow(FALSE);
			}
			else
			{
				long nNumOfChars = ipInsertChars->NumOfCharsLong;
				SetDlgItemInt(IDC_EDIT_NUM_OF_CHARS, nNumOfChars, FALSE);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04987");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CInsertCharactersPP::OnSelchangeCmbLengthType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// if current type is "Any"
		int nLengthType = m_cmbLengthType.GetCurSel();
		if (nLengthType == 0)
		{
			// if length type is "Any" number of characters
			// Disable the edit box for entering number of characters
			SetDlgItemText(IDC_EDIT_NUM_OF_CHARS, "");
			m_editNumOfCharsLong.EnableWindow(FALSE);
		}
		else
		{
			m_editNumOfCharsLong.EnableWindow(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04988");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CInsertCharactersPP::OnClickedRadioAppend(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// disable edit box
		ATLControls::CEdit editPosition(GetDlgItem(IDC_EDIT_POSITION));
		editPosition.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04989");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CInsertCharactersPP::OnClickedRadioSpecified(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// enable edit box
		ATLControls::CEdit editPosition(GetDlgItem(IDC_EDIT_POSITION));
		editPosition.EnableWindow(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04990");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CInsertCharactersPP::OnClickedPositionInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Position is 1-based. i.e. First character"
					  "\nof the input string is at position 1.\n\n"
					  "Characters will not be inserted beyond the\n"
					  "end of the string - this Value Modifier\n"
					  "cannot be used to pad the string.");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05531");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CInsertCharactersPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07688", "InsertCharacters Modifier PP" );
}
//-------------------------------------------------------------------------------------------------
