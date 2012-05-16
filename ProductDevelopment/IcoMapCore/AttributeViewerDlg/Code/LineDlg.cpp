//===========================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LineDlg.cpp
//
// PURPOSE:	To provide user interface allowing View/Edit of a new or an 
//				existing Line segment.
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//===========================================================================

#include "stdafx.h"
#include "LineDlg.h"

#include <DirectionHelper.h>
#include <DistanceCore.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CLineDlg dialog
/////////////////////////////////////////////////////////////////////////////

CLineDlg::CLineDlg(LPCTSTR pszBearing, LPCTSTR pszDistance, bool bReadOnly, 
				   bool bNew, CWnd* pParent /*=NULL*/)
	: CDialog(CLineDlg::IDD, pParent),
	m_bReadOnly(bReadOnly),
	m_bNew(bNew)
{
	//{{AFX_DATA_INIT(CLineDlg)
	m_zBearing = pszBearing;
	m_zDistance = pszDistance;
	//}}AFX_DATA_INIT
}


void CLineDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CLineDlg)
	DDX_Text(pDX, IDC_EDIT_BEARING, m_zBearing);
	DDX_Text(pDX, IDC_EDIT_DISTANCE, m_zDistance);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CLineDlg, CDialog)
	//{{AFX_MSG_MAP(CLineDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CLineDlg message handlers
/////////////////////////////////////////////////////////////////////////////
BOOL CLineDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	// Check to see if edit boxes should be read-only
	if (m_bReadOnly)
	{
		// Set dialog caption - ignores m_bNew member
		SetWindowText( "View Line" );

		// Set Bearing to read-only
		CEdit*	pEdit = NULL;
		pEdit = (CEdit *)GetDlgItem( IDC_EDIT_BEARING );
		if (pEdit != NULL)
		{
			pEdit->SetReadOnly( TRUE );
		}

		// Set Distance to read-only
		pEdit = (CEdit *)GetDlgItem( IDC_EDIT_DISTANCE );
		if (pEdit != NULL)
		{
			pEdit->SetReadOnly( TRUE );
		}

		// Also set focus to the OK button
		CButton*	pButton = NULL;
		pButton = (CButton *)GetDlgItem( IDOK );
		if (pButton != NULL)
		{
			pButton->SetFocus();

			return FALSE;
		}
	}
	else
	{
		// Check New flag
		if (m_bNew)
		{
			// Set dialog caption
			SetWindowText( "New Line" );
		}
		else
		{
			// Set dialog caption
			SetWindowText( "Edit Line" );
		}
	}

	// Removed this modification 1/7/02 - WEL
	// Ensure that distance string has non-standard precision
//	if (!m_zDistance.IsEmpty())
//	{
//		Distance	dist;
//		dist.evaluate( m_zDistance.operator LPCTSTR() );
//		double dTemp = dist.getDistanceInCurrentUnit();
//		m_zDistance.Format( "%.4f", dTemp );
//		UpdateData( FALSE );
//	}

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

/////////////////////////////////////////////////////////////////////////////
void CLineDlg::OnOK() 
{
	CEdit*	pEdit = NULL;

	// Just return if dialog was read-only
	if (!m_bReadOnly)
	{
		// Update data members
		UpdateData( TRUE );

		///////////////////
		// Validate Bearing
		///////////////////

		// Check to see that edit box is not empty
		if (m_zBearing.IsEmpty())
		{
			// Display error message
			MessageBox( "Direction must be defined.", "Error" );

			// Also set focus to the edit box
			pEdit = (CEdit *)GetDlgItem( IDC_EDIT_BEARING );
			if (pEdit != NULL)
			{
				pEdit->SetFocus();
			}

			// Back to the dialog
			return;
		}				// end if Bearing empty
		else
		{
			// Create a Bearing object and check validity
			DirectionHelper direction;
			direction.evaluateDirection( (LPCTSTR)m_zBearing );
			if (!direction.isDirectionValid())
			{
				// Present error message
				MessageBox( "Direction is not valid.", "Error" );

				// Also set focus to the edit box
				pEdit = (CEdit *)GetDlgItem( IDC_EDIT_BEARING );
				if (pEdit != NULL)
				{
					pEdit->SetFocus();
				}

				// Back to the dialog
				return;
			}			// end if Bearing not valid
		}				// end else Bearing not empty

		///////////////////
		// Validate Distance
		///////////////////

		// Check to see that edit box is not empty
		if (m_zDistance.IsEmpty())
		{
			// Display error message
			MessageBox( "Distance must be defined.", "Error" );

			// Also set focus to the edit box
			pEdit = (CEdit *)GetDlgItem( IDC_EDIT_DISTANCE );
			if (pEdit != NULL)
			{
				pEdit->SetFocus();
			}

			// Back to the dialog
			return;
		}				// end if Distance empty
		else
		{
			// Create a Distance object and check validity
			DistanceCore distanceObject;
			distanceObject.evaluate( m_zDistance.operator LPCTSTR() );
			if (!distanceObject.isValid())
			{
				// Present error message
				MessageBox( "Distance is not valid.", "Error" );

				// Also set focus to the edit box
				pEdit = (CEdit *)GetDlgItem( IDC_EDIT_DISTANCE );
				if (pEdit != NULL)
				{
					pEdit->SetFocus();
				}

				// Back to the dialog
				return;
			}			// end if Distance not valid
			// Check for Distance <= 0
			else if (distanceObject.getDistanceInCurrentUnit() <= 0.0)
			{
				// Present error message
				MessageBox( "Distance is not valid.", "Error" );

				// Also set focus to the edit box
				pEdit = (CEdit *)GetDlgItem( IDC_EDIT_DISTANCE );
				if (pEdit != NULL)
				{
					pEdit->SetFocus();
				}

				// Back to the dialog
				return;
			}			// end else if Distance <= 0
		}				// end else Distance not empty
	}					// end else not ReadOnly
	
	CDialog::OnOK();
}

/////////////////////////////////////////////////////////////////////////////
LPCTSTR CLineDlg::getBearing()
{
	return m_zBearing.operator LPCTSTR();
}

/////////////////////////////////////////////////////////////////////////////
LPCTSTR CLineDlg::getDistance()
{
	return m_zDistance.operator LPCTSTR();
}
