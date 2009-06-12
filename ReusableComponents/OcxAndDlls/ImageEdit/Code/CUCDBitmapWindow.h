//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CUCDBitmapWindow.h
//
// PURPOSE:	This is an header file for CUCDBitmapWindow class
//			where this has been derived from the LBitmapWindow()
//			class.  The code written in this file makes it possible for
//			initialize to set the window.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
//
#ifndef _LBITMAPWINDOWDERIVED_

#define _LBITMAPWINDOWDERIVED_

#include <ltWrappr.h>

class CImageEditCtrl;
//==================================================================================================
//
// CLASS:	CUCDBitmapWindow
//
// PURPOSE:	This class is used to derive CUCDBitmapWindow from Lead Tools class LBitmapWindow.
//			This derived control is used in control.  Object of this class is 
//			created in ImageEditCtl.cpp
// REQUIRE:	Nothing
// 
// INVARIANTS:
//			
// EXTENSIONS:	Nothing
//			
//
// NOTES:	
//
class CUCDBitmapWindow : public LBitmapWindow
//class CUCDBitmapWindow : public LAnnotationWindow
{
// Construction
public:
	CUCDBitmapWindow();

	// Attributes
public:

// Operations
public:
	
    virtual ~CUCDBitmapWindow();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: call back function for LBitmapWindow message handling.
	// REQUIRE: Nothing
	// PROMISE: Sets the cursor based on the Mouse pointer value of the control
    virtual LRESULT MsgProcCallBack(HWND hWnd,UINT uMsg,WPARAM wParam,LPARAM lParam);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to set the handle to the cursor the application wants to set 
	// REQUIRE: 
	// PROMISE: 
	void SetCursorHandle(HANDLE hCursor);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to set the mouse pointer based on which the cursor will be loaded.
	// REQUIRE: the number should be within 0 and 99. currently all the cursors are
	//			not supported. The supported cursors are
	//
	//         Mouse pointer No			Cursor shape
	//		   ----------------         ------------
	//				0					Default cursor (Arrow)
	//				1					Arrow
	//				2					+ cursor
	//				3					I beam cursor
	//				4					Up Arrow cursor
	//				5					Size All cursor
	//				6					North South cursor
	//				7					East west cursor
	//				8					Hour glass cursor
	//				99					Custom cursor ( from a file)
	// PROMISE: 
	void SetMousePointer (UINT iMousePtrNumer);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: returns the mouse pointer value which is one among the values mentioned 
	//          GetIconHandleForBitmap() above
	// REQUIRE: Nothing
	// PROMISE: returns integer value. 0 means default. 99 means custom defined
	int GetMousePointer ();
	//----------------------------------------------------------------------------------------------
	void SetImageControl(CImageEditCtrl * pCtrl) { m_pImageCtrl = pCtrl;}
	//----------------------------------------------------------------------------------------------

	// PURPOSE: updates the cursor with currently loaded cursor. Sets the cursor when ever
	//          called by any class by ::SetCursor() API
	// REQUIRE: Nothing
	// PROMISE: Shows the currently loaded cursor
	void UpdateCursor();
	//----------------------------------------------------------------------------------------------

	 void SetShowCursor (bool bShow);

private:
	// indicates whether default cursor is set or not
	bool m_bDefaultCursor;
	// indicates the mouse pointer value
	int m_iMousePointer;
	// handle to the cursor currently loaded. and not the default one
	HCURSOR m_hCursor;
	// handle to the icon
	HICON m_hIcon;
	// currently loaded cursor file name if any. default is null
	CString m_zCursorFileName;
	// handle to the default cursor
	HCURSOR m_hDefaultCursor;
	// pointer to the control class
	CImageEditCtrl * m_pImageCtrl;

	bool m_bShowCursor;
};
//==================================================================================================
#endif //#ifndef _LBITMAPWINDOWDERIVED_