// Note: This class originally by Stephen C. Steel
// http://www.codeproject.com/buttonctrl/imagebuttonwithstyle.asp?df=100&forumid=236995&exp=0&select=1938447
// The class was modified to fit Extract Systems formatting style and exception
// handling, to enable use as a child of an ATL window and to fix a bug in 
// image positioning when the image is larger than the button.

////////////////////////////////////////////////////////////////////////////////
//
// Class CImageButtonWithStyle
//
// The class CImageButtonWithStyle is a CButton dereived class that
// handles the NM_CUSTOMDRAW notification to provide XP visual styles themed
// appearance for buttons with BS_BITMAP or BS_ICON style set.
// This makes them match the appearance of normal buttons with text labels.
// The default Windows controls do not provide the themed appearance for
// these buttons.
//
// To use these buttons, simply subclass the corresponding control with an
// instance of a CImageButtonWithStyle. For buttons on a Dialog, you can
// simple declare CImageButtonWithStyle members for each button in the
// CDialog derived class, and then call DDX_Control() for each instance
// to subclass the corresponding control.
//
// This class uses the CVisualStylesXP class by David Yuheng Zhao
// to access the functions in the "uxtheme.dll" yet still load when
// the DLL isn't available.

#include "BaseUtils.h"

#pragma once

class EXPORT_BaseUtils CImageButtonWithStyle : public CButton
{
	DECLARE_DYNAMIC(CImageButtonWithStyle)

public:
	CImageButtonWithStyle();
	virtual ~CImageButtonWithStyle();

protected:
	DECLARE_MESSAGE_MAP()

// Windows Message Handlers

	// OnNotifyCustomDraw - Handle NM_CUSTOMDRAW notifications.
	// If an XP visual styles theme is active, use the uxtheme.dll functions
	// to draw buttons with BS_BITMAP or BS_ICON style so that they match the
	// appearance of normal text buttons.
	afx_msg void OnNotifyCustomDraw ( NMHDR * pNotifyStruct, LRESULT* result );

	// If the button is used as part of an ATL window, the ATL Window
	// will need to call REFLECT_NOTIFICATIONS() which allow this class
	// to receive them... however in this case they will be received as 
	// OCM_NOTIFY messages rather than WM_NOTIFY.  OnOCM_Notify is needed
	// to intercept these and redirect them to OnNotifyCustomDraw.
	LRESULT OnOCM_Notify (WPARAM wParam, LPARAM lParam);

private:
	///////////////
	// Methods
	///////////////

	// Draw a bitmap
	void draw_bitmap (HDC hDC, const CRect& Rect, DWORD style);

	// Draw an icon
	void draw_icon (HDC hDC, const CRect& Rect, DWORD style);

	// Calculate the left position of the image so it is drawn on left, right or centered (the default)
	// as dictated by the style settings.
	static int image_left (int cx, const CRect& Rect, DWORD style);
	
	// Calculate the top position of the image so it is drawn on top, bottom or vertically centered (the default)
	// as dictated by the style settings.
	static int image_top (int cy, const CRect& Rect, DWORD style);
};

