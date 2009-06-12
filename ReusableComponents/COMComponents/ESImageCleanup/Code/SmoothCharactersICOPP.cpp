// SmoothCharactersICOPP.cpp : Implementation of CSmoothCharactersICOPP
#include "stdafx.h"
#include "SmoothCharactersICOPP.h"

#include <UCLIDException.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// CSmoothCharactersICOPP
//-------------------------------------------------------------------------------------------------
CSmoothCharactersICOPP::CSmoothCharactersICOPP()
{
	try
	{
		m_dwTitleID = IDS_TITLESMOOTHCHARACTERSICOPP;
		m_dwHelpFileID = IDS_HELPFILESMOOTHCHARACTERSICOPP;
		m_dwDocStringID = IDS_DOCSTRINGSMOOTHCHARACTERSICOPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17800");
}
//-------------------------------------------------------------------------------------------------
CSmoothCharactersICOPP::~CSmoothCharactersICOPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17801");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICOPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ATLTRACE(_T("CSmoothCharactersICOPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			ESImageCleanupLib::ISmoothCharactersICOPtr ipSmoothCharacters(m_ppUnk[i]);
			if (ipSmoothCharacters)
			{
				// get the current check button and set the SmoothType based on the button
				if (m_radioLighten.GetCheck() == BST_CHECKED)
				{
					ipSmoothCharacters->SmoothType = ciSmoothLightenEdges;
				}
				else if (m_radioDarken.GetCheck() == BST_CHECKED)
				{
					ipSmoothCharacters->SmoothType = ciSmoothDarkenEdges;
				}
				else
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI17797");
				}
			}
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17798");

	// an exception was caught
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CSmoothCharactersICOPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ESImageCleanupLib::ISmoothCharactersICOPtr ipSmoothCharacters = m_ppUnk[0];
		if (ipSmoothCharacters)
		{
			// get the radio buttons
			m_radioLighten = GetDlgItem(IDC_RADIO_SMOOTHCHARACTERS_LIGHTEN);
			m_radioDarken = GetDlgItem(IDC_RADIO_SMOOTHCHARACTERS_DARKEN);

			// set the radio button based on the currently set smooth type
			switch (ipSmoothCharacters->SmoothType)
			{
			case ciSmoothLightenEdges:
				m_radioLighten.SetCheck(BST_CHECKED);
				break;

			case ciSmoothDarkenEdges:
				m_radioDarken.SetCheck(BST_CHECKED);
				break;

			default:
				THROW_LOGIC_ERROR_EXCEPTION("ELI17826");
			}
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17799");

	return 0;
}
//-------------------------------------------------------------------------------------------------