// NewHeaderCtrl.cpp : implementation file
//

#include "stdafx.h"
#include "NewHeaderCtrl.h"
#include "TLFrame.h"
#include "UCLIDException.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CNewHeaderCtrl
//-------------------------------------------------------------------------------------------------
CNewHeaderCtrl::CNewHeaderCtrl()
{
}
//-------------------------------------------------------------------------------------------------
CNewHeaderCtrl::~CNewHeaderCtrl()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16369");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CNewHeaderCtrl, CHeaderCtrl)
	//{{AFX_MSG_MAP(CNewHeaderCtrl)
	ON_WM_PAINT()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CNewHeaderCtrl message handlers
//-------------------------------------------------------------------------------------------------
void CNewHeaderCtrl::DrawItem( LPDRAWITEMSTRUCT lpDrawItemStruct )
{
	CDC dc;

	dc.Attach( lpDrawItemStruct->hDC );

	// Save DC
	int nSavedDC = dc.SaveDC();

	// Get the column rect
	CRect rcLabel( lpDrawItemStruct->rcItem );

	// Set clipping region to limit drawing within column
	CRgn rgn;
	rgn.CreateRectRgnIndirect( &rcLabel );
	dc.SelectObject( &rgn );
	rgn.DeleteObject();

	// Labels are offset by a certain amount  
	// This offset is related to the width of a space character
	int offset = dc.GetTextExtent(_T(" "), 1 ).cx*2;

	// Get the column text and format
	TCHAR buf[256];
	HD_ITEM hditem;
	
	hditem.mask = HDI_TEXT | HDI_FORMAT;
	hditem.pszText = buf;
	hditem.cchTextMax = 255;

	GetItem( lpDrawItemStruct->itemID, &hditem );

	// Determine format for drawing column label
	UINT uFormat = DT_SINGLELINE | DT_NOPREFIX | DT_NOCLIP 
						| DT_VCENTER | DT_END_ELLIPSIS ;

	if (hditem.fmt & HDF_CENTER)
	{
		uFormat |= DT_CENTER;
	}
	else if (hditem.fmt & HDF_RIGHT)
	{
		uFormat |= DT_RIGHT;
	}
	else
	{
		uFormat |= DT_LEFT;
	}

	if (!(uFormat & DT_RIGHT))
	{
		// Adjust the rect if the mouse button is pressed on it
		if (lpDrawItemStruct->itemState == ODS_SELECTED)
		{
			rcLabel.left++;
			rcLabel.top += 2;
			rcLabel.right++;
		}

		rcLabel.left += offset;
		rcLabel.right -= offset;

		// Draw column label
		if (rcLabel.left < rcLabel.right)
		{
			dc.DrawText(buf,-1,rcLabel, uFormat);
		}
	}

	if (uFormat & DT_RIGHT)
	{
		// Adjust the rect if the mouse button is pressed on it
		if (lpDrawItemStruct->itemState == ODS_SELECTED)
		{
			rcLabel.left++;
			rcLabel.top += 2;
			rcLabel.right++;
		}

		rcLabel.left += offset;
		rcLabel.right -= offset;

		// Draw column label
		if (rcLabel.left < rcLabel.right)
		{
			dc.DrawText(buf,-1,rcLabel, uFormat);
		}
	}

	// Restore dc
	dc.RestoreDC( nSavedDC );

	// Detach the dc before returning
	dc.Detach();
}
//-------------------------------------------------------------------------------------------------
void CNewHeaderCtrl::Autofit(int nOverrideItemData /*= -1*/, int nOverrideWidth /*= 0*/)
{
	int nItemCount = GetItemCount();
	int nTotalWidthOfColumns = 0;
	int nDifferenceInWidth;
	int nItem;
	HD_ITEM hi;
	CRect rClient;

	if (!m_bAutofit)
	{
		return;
	}

	SetRedraw(FALSE);

	GetParent()->GetClientRect(&rClient);

	if (-1 != nOverrideItemData)
	{
		rClient.right -= nOverrideWidth;
	}

	// Get total width of all columns
	for (nItem = 0; nItem < nItemCount; nItem++)
	{
		// Don't mess with the item being resized by the user
		if (nItem == nOverrideItemData)	
		{
			continue;
		}

		hi.mask = HDI_WIDTH;
		GetItem(nItem, &hi);

		nTotalWidthOfColumns += hi.cxy;
	}

	if (nTotalWidthOfColumns != rClient.Width())
	{
		// We need to shrink/expand all columns!
		nDifferenceInWidth = abs(nTotalWidthOfColumns-rClient.Width());	
		
		// Shrink/expand all columns proportionally based on their current size
		for (nItem = 0; nItem < nItemCount; nItem++)
		{
			// Skip the overrride column if there is one!
			if (nItem == nOverrideItemData)	
			{
				continue;
			}
			
			hi.mask = HDI_WIDTH;
			GetItem(nItem, &hi);

			hi.mask = HDI_WIDTH;
			hi.cxy = (hi.cxy * rClient.Width()) / nTotalWidthOfColumns;

			SetItem(nItem, &hi);
		}
	}

	SetRedraw(TRUE);
	Invalidate();
}
//-------------------------------------------------------------------------------------------------
void CNewHeaderCtrl::OnPaint() 
{
	CPaintDC dc(this); // device context for painting

	// Do not call CHeaderCtrl::OnPaint() for painting messages
	CWnd::DefWindowProc( WM_PAINT, (WPARAM)dc.m_hDC, 0 );
}
//-------------------------------------------------------------------------------------------------
