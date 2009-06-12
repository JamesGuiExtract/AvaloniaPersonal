// DespeckleICOPP.cpp : Implementation of CDespeckleICOPP
#include "stdafx.h"
#include "DespeckleICOPP.h"

#include <UCLIDException.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// CDespeckleICOPP
//-------------------------------------------------------------------------------------------------
CDespeckleICOPP::CDespeckleICOPP()
{
	try
	{
		m_dwTitleID = IDS_TITLEDESPECKLEICOPP;
		m_dwHelpFileID = IDS_HELPFILEDESPECKLEICOPP;
		m_dwDocStringID = IDS_DOCSTRINGDESPECKLEICOPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17422");
}
//-------------------------------------------------------------------------------------------------
CDespeckleICOPP::~CDespeckleICOPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17423");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICOPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ATLTRACE(_T("CDespeckleICOPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			ESImageCleanupLib::IDespeckleICOPtr ipDespeckle(m_ppUnk[i]);
			if (ipDespeckle)
			{
				// get the string from the property page
				CString zNoiseSize;
				m_editNoiseSize.GetWindowText(zNoiseSize);

				// set to std::string
				string strNoiseSize = zNoiseSize;

				// convert the string to a long
				long lNoiseSize = asLong(strNoiseSize);

				if (lNoiseSize < 0)
				{
					UCLIDException ue("ELI17424", "Noise size must not be less than 0!");
					ue.addDebugInfo("Noise size", lNoiseSize);
					throw ue;
				}

				ipDespeckle->NoiseSize = lNoiseSize;
			}
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19443");

	// an exception was caught
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CDespeckleICOPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ESImageCleanupLib::IDespeckleICOPtr ipDespeckle = m_ppUnk[0];
		if (ipDespeckle)
		{
			// get the edit box
			m_editNoiseSize = GetDlgItem(IDC_EDIT_DESPECKLE_SIZE);

			// set the edit box max characters
			m_editNoiseSize.SetLimitText(2);
			
			// set the edit box text
			m_editNoiseSize.SetWindowText(asString(ipDespeckle->NoiseSize).c_str());
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17425");

	return 0;
}
//-------------------------------------------------------------------------------------------------