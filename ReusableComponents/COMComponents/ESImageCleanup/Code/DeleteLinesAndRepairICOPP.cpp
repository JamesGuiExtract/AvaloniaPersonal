// DeleteLinesAndRepairICOPP.cpp : Implementation of CDeleteLinesAndRepairICOPP

#include "stdafx.h"
#include "DeleteLinesAndRepairICOPP.h"

#include <UCLIDException.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// default settings for line length and gap
const string gstrDEFAULT_LINE_LENGTH = "100";
const string gstrDEFAULT_LINE_GAP = "0";

//-------------------------------------------------------------------------------------------------
// CDeleteLinesAndRepairICOPP
//-------------------------------------------------------------------------------------------------
CDeleteLinesAndRepairICOPP::CDeleteLinesAndRepairICOPP()
{
	try
	{
		m_dwTitleID = IDS_TITLEDELETELINESANDREPAIRICOPP;
		m_dwHelpFileID = IDS_HELPFILEDELETELINESANDREPAIRICOPP;
		m_dwDocStringID = IDS_DOCSTRINGDELETELINESANDREPAIRICOPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17646");
}
//-------------------------------------------------------------------------------------------------
CDeleteLinesAndRepairICOPP::~CDeleteLinesAndRepairICOPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17647");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICOPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ATLTRACE(_T("CDeleteLinesAndRepairICOPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			ESImageCleanupLib::IDeleteLinesAndRepairICOPtr ipDeleteLines(m_ppUnk[i]);
			if (ipDeleteLines)
			{
				// get the strings from the property page
				CString zLineLength, zLineGap;
				m_editLineLength.GetWindowText(zLineLength);
				m_editLineGap.GetWindowText(zLineGap);

				// set to std::string
				string strLineLength = zLineLength;
				string strLineGap = zLineGap;

				// convert the strings to a long
				long lLineLength = asLong(strLineLength);
				long lLineGap = asLong(strLineGap);

				// check for invalid values
				if (lLineLength < 0)
				{
					UCLIDException ue("ELI17648", "Line length must not be less than 0!");
					ue.addDebugInfo("Line length", lLineLength);
					throw ue;
				}
				if (lLineGap < 0)
				{
					UCLIDException ue("ELI17649", "Line gap must not be less than 0!");
					ue.addDebugInfo("Line gap", lLineGap);
					throw ue;
				}

				// set the delete lines objects settings
				ipDeleteLines->LineLength = lLineLength;
				ipDeleteLines->LineGap = lLineGap;
				ipDeleteLines->LineDirection = getLineDirection();
			}
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17650");

	// an exception was caught
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CDeleteLinesAndRepairICOPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ESImageCleanupLib::IDeleteLinesAndRepairICOPtr ipDeleteLines = m_ppUnk[0];
		if (ipDeleteLines)
		{
			// get the edit boxes
			m_editLineLength = GetDlgItem(IDC_EDIT_DELETELINES_LENGTH);
			m_editLineGap = GetDlgItem(IDC_EDIT_DELETELINES_GAP);

			// set the edit boxes max characters
			m_editLineLength.SetLimitText(3);
			m_editLineGap.SetLimitText(2);
			
			// get the LineLength and LineGap from the DeleteLines object
			string strLineLength = asString(ipDeleteLines->LineLength);
			string strLineGap = asString(ipDeleteLines->LineGap);

			// set the strings for the edit boxes
			if (strLineLength == "-1")
			{
				strLineLength = gstrDEFAULT_LINE_LENGTH;
			}
			if (strLineGap == "-1")
			{
				strLineGap = gstrDEFAULT_LINE_GAP;
			}

			// set the edit boxes
			m_editLineLength.SetWindowText(strLineLength.c_str());
			m_editLineGap.SetWindowText(strLineGap.c_str());

			// default line removal to horizontal lines
			int nButtonID = IDC_RADIO_DELETELINESANDREPAIR_HORZ;

			unsigned long ulDirection = ipDeleteLines->LineDirection;

			// set the line direction radio button
			switch(ipDeleteLines->LineDirection)
			{
			case ciLineVert:
				nButtonID = IDC_RADIO_DELETELINESANDREPAIR_VERT;
				break;

			case ciLineHorz:
				nButtonID = IDC_RADIO_DELETELINESANDREPAIR_HORZ;
				break;

			case ciLineVertAndHorz:
				nButtonID = IDC_RADIO_DELETELINESANDREPAIR_BOTH;
				break;
			}
			CheckRadioButton(IDC_RADIO_DELETELINESANDREPAIR_VERT,
				IDC_RADIO_DELETELINESANDREPAIR_BOTH, nButtonID);
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17651");

	return 0;
}
//-------------------------------------------------------------------------------------------------
unsigned long CDeleteLinesAndRepairICOPP::getLineDirection()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// default value to a value that will throw an unconfigured error
	// in case of an error in this method
	unsigned long lReturn = ciLineUnknown;
	try
	{
		ATLControls::CButton radioVert = GetDlgItem(IDC_RADIO_DELETELINESANDREPAIR_VERT);
		ATLControls::CButton radioHorz = GetDlgItem(IDC_RADIO_DELETELINESANDREPAIR_HORZ);
		ATLControls::CButton radioBoth = GetDlgItem(IDC_RADIO_DELETELINESANDREPAIR_BOTH);

		if (radioVert.GetCheck() == BST_CHECKED)
		{
			lReturn = ciLineVert;
		}
		else if (radioHorz.GetCheck() == BST_CHECKED)
		{
			lReturn = ciLineHorz;
		}
		else if (radioBoth.GetCheck() == BST_CHECKED)
		{
			lReturn = ciLineVertAndHorz;
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI17652");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17653");

	return lReturn;
}
//-------------------------------------------------------------------------------------------------