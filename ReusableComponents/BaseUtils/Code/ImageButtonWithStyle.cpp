#include "stdafx.h"
#include "VisualStylesXP.h"
#include "UCLIDException.h"
#include "ImageButtonWithStyle.h"

//-------------------------------------------------------------------------------------------------
// CImageButtonWithStyle
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CImageButtonWithStyle, CButton)
CImageButtonWithStyle::CImageButtonWithStyle()
{
}
//-------------------------------------------------------------------------------------------------
CImageButtonWithStyle::~CImageButtonWithStyle()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18191")
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CImageButtonWithStyle, CButton)
	ON_NOTIFY_REFLECT (NM_CUSTOMDRAW, OnNotifyCustomDraw)
	ON_MESSAGE (OCM_NOTIFY, OnOCM_Notify)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
void CImageButtonWithStyle::OnNotifyCustomDraw ( NMHDR * pNotifyStruct, LRESULT* result )
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18193", pNotifyStruct);
		LPNMCUSTOMDRAW pCustomDraw = (LPNMCUSTOMDRAW) pNotifyStruct;

		ASSERT_ARGUMENT("ELI18195", pCustomDraw->hdr.hwndFrom == m_hWnd);
		ASSERT_ARGUMENT("ELI18196", pCustomDraw->hdr.code == NM_CUSTOMDRAW);

		DWORD style = GetStyle ();
		if ((style & (BS_BITMAP | BS_ICON)) == 0 || !g_xpStyle.IsAppThemed () || !g_xpStyle.IsThemeActive ())
		{
			// not icon or bitmap button, or themes not active - draw normally
			*result = CDRF_DODEFAULT;
			return;
		}

		if (pCustomDraw->dwDrawStage == CDDS_PREERASE)
		{
			// erase background (according to parent window's themed background
			g_xpStyle.DrawThemeParentBackground (m_hWnd, pCustomDraw->hdc, &pCustomDraw->rc);
		}

		if (pCustomDraw->dwDrawStage == CDDS_PREERASE || pCustomDraw->dwDrawStage == CDDS_PREPAINT)
		{
			// get theme handle
			HTHEME hTheme = g_xpStyle.OpenThemeData (m_hWnd, L"BUTTON");
			ASSERT_RESOURCE_ALLOCATION("ELI18197", hTheme != __nullptr);
			if (hTheme == NULL)
			{
				// fail gracefully
				*result = CDRF_DODEFAULT;
				return;
			}

			// determine state for DrawThemeBackground()
			// note: order of these tests is significant
			int state_id = PBS_NORMAL;
			if (style & WS_DISABLED)
				state_id = PBS_DISABLED;
			else if (pCustomDraw->uItemState & CDIS_SELECTED)
				state_id = PBS_PRESSED;
			else if (pCustomDraw->uItemState & CDIS_HOT)
				state_id = PBS_HOT;
			else if (style & BS_DEFPUSHBUTTON)
				state_id = PBS_DEFAULTED;

			// draw themed button background appropriate to button state
			g_xpStyle.DrawThemeBackground (hTheme,
				pCustomDraw->hdc, BP_PUSHBUTTON,
				state_id,
				&pCustomDraw->rc, NULL);

			// get content rectangle (space inside button for image)
			CRect content_rect (pCustomDraw->rc); 

			g_xpStyle.GetThemeBackgroundContentRect (hTheme,
				pCustomDraw->hdc, BP_PUSHBUTTON,
				state_id,
				&pCustomDraw->rc,
				&content_rect);

			// we're done with the theme
			g_xpStyle.CloseThemeData(hTheme);

			// draw the image
			if (style & BS_BITMAP)
			{
				draw_bitmap (pCustomDraw->hdc, &content_rect, style);
			}
			else
			{
				draw_icon (pCustomDraw->hdc, &content_rect, style);
			}

			// finally, draw the focus rectangle if needed
			if (pCustomDraw->uItemState & CDIS_FOCUS)
			{
				// draw focus rectangle
				DrawFocusRect (pCustomDraw->hdc, &content_rect);
			}

			*result = CDRF_SKIPDEFAULT;
			return;
		}

		// we should never get here, since we should only get CDDS_PREERASE or CDDS_PREPAINT
		THROW_LOGIC_ERROR_EXCEPTION("ELI18198");
		*result = CDRF_DODEFAULT;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18192");
}
//-------------------------------------------------------------------------------------------------
LRESULT CImageButtonWithStyle::OnOCM_Notify(WPARAM wParam, LPARAM lParam)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18200", lParam != NULL);

		LRESULT lRes = 0;

		LPNMHDR lpNMHDR = (LPNMHDR)lParam;
		if (lpNMHDR->code == NM_CUSTOMDRAW)
		{
			// Catch events received via reflection from an ATL window and 
			// forward it to the OnNotifyCustomDraw handler.
			OnNotifyCustomDraw(lpNMHDR, &lRes);
		}

		return lRes;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18199");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CImageButtonWithStyle::draw_bitmap (HDC hDC, const CRect& Rect, DWORD style)
{
	HBITMAP hBitmap = GetBitmap ();
	if (hBitmap == NULL)
		return;

	// determine size of bitmap image
	BITMAPINFO bmi;
	memset (&bmi, 0, sizeof (BITMAPINFO));
	bmi.bmiHeader.biSize = sizeof (BITMAPINFOHEADER);
	GetDIBits(hDC, hBitmap, 0, 0, NULL, &bmi, DIB_RGB_COLORS);

	// determine position of top-left corner of bitmap (positioned according to style)
	int x = image_left (bmi.bmiHeader.biWidth, Rect, style);
	int y = image_top (bmi.bmiHeader.biHeight, Rect, style);

	// Draw the bitmap
	DrawState(hDC, NULL, NULL, (LPARAM) hBitmap, 0, x, y, bmi.bmiHeader.biWidth, bmi.bmiHeader.biHeight,
		(style & WS_DISABLED) != 0 ? (DST_BITMAP | DSS_DISABLED) : (DST_BITMAP | DSS_NORMAL));
}
//-------------------------------------------------------------------------------------------------
void CImageButtonWithStyle::draw_icon (HDC hDC, const CRect& Rect, DWORD style)
{
	HICON hIcon = GetIcon ();
	if (hIcon == NULL)
		return;

	// determine size of icon image
	ICONINFO ii;
	GetIconInfo (hIcon, &ii);
	BITMAPINFO bmi;
	memset (&bmi, 0, sizeof (BITMAPINFO));
	bmi.bmiHeader.biSize = sizeof (BITMAPINFOHEADER);
	int cx = 0;
	int cy = 0;
	if (ii.hbmColor != __nullptr)
	{
		// icon has separate image and mask bitmaps - use size directly
		GetDIBits(hDC, ii.hbmColor, 0, 0, NULL, &bmi, DIB_RGB_COLORS);
		cx = bmi.bmiHeader.biWidth;
		cy = bmi.bmiHeader.biHeight;
	}
	else
	{
		// icon has singel mask bitmap which is twice as high as icon
		GetDIBits(hDC, ii.hbmMask, 0, 0, NULL, &bmi, DIB_RGB_COLORS);
		cx = bmi.bmiHeader.biWidth;
		cy = bmi.bmiHeader.biHeight/2;
	}

	// determine position of top-left corner of icon
	int x = image_left (cx, Rect, style);
	int y = image_top (cy, Rect, style);

	// Draw the icon
	DrawState(hDC, NULL, NULL, (LPARAM) hIcon, 0, x, y, cx, cy,
		(style & WS_DISABLED) != 0 ? (DST_ICON | DSS_DISABLED) : (DST_ICON | DSS_NORMAL));
}
//-------------------------------------------------------------------------------------------------
int CImageButtonWithStyle::image_left (int cx, const CRect& Rect, DWORD style)
{
	int x = Rect.left;
	if ((style & BS_CENTER) == BS_LEFT)
		x = Rect.left;
	else if ((style & BS_CENTER) == BS_RIGHT)
		x = Rect.right - cx;
	else
		x = Rect.left + (Rect.Width () - cx)/2;
	return (x);
}
//-------------------------------------------------------------------------------------------------
int CImageButtonWithStyle::image_top (int cy, const CRect& Rect, DWORD style)
{
	int y = Rect.top;
	if ((style & BS_VCENTER) == BS_TOP)
		y = Rect.top;
	else if ((style & BS_VCENTER) == BS_BOTTOM)
		y = Rect.bottom - cy;
	else
		y = Rect.top + (Rect.Height () - cy)/2;
	return (y);
}
//-------------------------------------------------------------------------------------------------
