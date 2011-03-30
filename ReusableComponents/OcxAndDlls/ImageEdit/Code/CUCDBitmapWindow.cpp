//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CUCDBitmapWindow.cpp
//
// PURPOSE:	This is an implementation file for CUCDBitmapWindow() class.
//			Where the CUCDBitmapWindow() class has been derived from LBitmapWindow() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//-------------------------------------------------------------------------------------------------
// CUCDBitmapWindow.cpp : implementation file
//
#include "stdafx.h"
#include "resource.h"
#include "CUCDBitmapWindow.h"
#include "ImageEditCtl.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
CUCDBitmapWindow::CUCDBitmapWindow()
{
	m_hCursor = NULL;
	m_hDefaultCursor = NULL;
	m_bDefaultCursor = true;
	m_iMousePointer = 0;

	m_hDefaultCursor = ::GetCursor();

	m_pImageCtrl = NULL;
	m_bShowCursor = true;

	EnableDragAcceptFiles(FALSE);
}
//-------------------------------------------------------------------------------------------------
/*virtual*/ CUCDBitmapWindow::~CUCDBitmapWindow()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16585");
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
/*virtual*/ LRESULT CUCDBitmapWindow::MsgProcCallBack(HWND hWnd,UINT uMsg,WPARAM wParam,LPARAM lParam)
{
	CPoint pt((long)wParam, (long)lParam);

	switch (uMsg)
	{
	//case WM_LTANNEVENT:
	//	{
	//		if (wParam == LTANNEVENT_MOUSEMOVE)
	//		{
	//			// Do ordinary MouseMove handling
	//			::SetCursor(m_hCursor);
	//			if (!m_bShowCursor)
	//			{
	//				SetShowCursor(true);
	//			}

	//			// Retrieve mouse position
	//			ANNMOUSEPOS *pAMP = (ANNMOUSEPOS *)lParam;
	//			pt = pAMP->pt;

	//			// Fire a stock MouseMove event
	//			if (m_pImageCtrl != __nullptr)
	//			{
	//				m_pImageCtrl->FireMouseMove( 0, 0, pAMP->pt.x, pAMP->pt.y );
	//			}
	//		}
	//		break;
	//	}

	case WM_NCLBUTTONDOWN:
	case WM_NCLBUTTONDBLCLK:
		if(wParam == HTVSCROLL)
		{
			// only set the vertical line step when explicitly allowing the base class to handle
			// mousewheel events. [P16 #2338] This allows the base class to handle the user 
			// clicking the vertical scrollbar arrows. [P13 #4935]
			SetVertLineStep(5);
		}
		break;

	case WM_SIZE:
	case WM_NCLBUTTONUP:
	case WM_NCMOUSELEAVE:
		{
			// prevent the base class from handling mousewheel events. [P16 #2338]
			// if this is a size event, scrollbars may have just been created. [P13 #4849]
			// if this is a non-client mouse action, the user may have just finished clicking
			// the vertical scroll bar arrows. [P13 #4935]
			SetVertLineStep(0);
			break;
		}

	case WM_MOUSEMOVE:
		{
			::SetCursor(m_hCursor);
			if (!m_bShowCursor)
			{
				SetShowCursor(true);
			}
			
			break;
		}
	case WM_MOUSEWHEEL:
		{
			if (m_pImageCtrl != __nullptr)
			{
				m_pImageCtrl->FireMouseWheelFromCtrlClass((long) wParam, (long) lParam);
			}
		}
	case WM_LBUTTONDOWN:
	case WM_RBUTTONDOWN:
	case WM_LBUTTONUP:
	case WM_RBUTTONUP:
	case WM_LBUTTONDBLCLK:
	case WM_RBUTTONDBLCLK:
	case WM_MBUTTONDOWN:
	case WM_MBUTTONUP:
	case WM_MBUTTONDBLCLK:
	case WM_MOUSEHOVER:
		{
			::SetCursor(m_hCursor);

			break;
		}
	case WM_HSCROLL:
	case WM_VSCROLL:
		{
			if (m_pImageCtrl != __nullptr)
				m_pImageCtrl->FireScrollFromCtrlClass();
		}
		break;
	case WM_KEYUP:
	case WM_SYSKEYUP:
		{
			// what's the current shift state (1 - shift, 2 - Ctrl, 4 - Alt, etc)
			short shiftState = 0;
			shiftState += (::GetKeyState(VK_SHIFT) == -127 || ::GetKeyState(VK_SHIFT) == -128) ? 1 : 0;
			shiftState += (::GetKeyState(VK_CONTROL) == -127 || ::GetKeyState(VK_CONTROL) == -128) ? 2 : 0;
			shiftState += (::GetKeyState(VK_MENU) == -127 || ::GetKeyState(VK_MENU) == -128) ? 4 : 0;

			if (m_pImageCtrl != __nullptr)
			{
				m_pImageCtrl->FireKeyUp((USHORT*)&wParam, shiftState);
			}
		}
		break;
	default :
		break;
	}

	// Allow base class to do standard handling
	return LBitmapWindow::MsgProcCallBack( hWnd, uMsg, wParam, lParam);
//	return LAnnotationWindow::MsgProcCallBack( hWnd, uMsg, wParam, lParam);
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void CUCDBitmapWindow::SetCursorHandle(HANDLE hCursor)
{
	m_hCursor = (HCURSOR)hCursor;

	UpdateCursor();
}
//-------------------------------------------------------------------------------------------------
void CUCDBitmapWindow::SetMousePointer (UINT iMousePtrNumer)
{
	switch (iMousePtrNumer)
	{
	case 0:
		{
			if (m_bDefaultCursor == 1)
				return;
			else
				::SetCursor(m_hDefaultCursor);

			m_bDefaultCursor = true;
			break;
		}
	case 1:
		{
			// Arrow cursor
			if (m_bDefaultCursor == 1)
				return;
			else
				::SetCursor(m_hDefaultCursor);
			m_bDefaultCursor = true;
			break;
		}
	case 2:
		{
			if (m_iMousePointer == 2)
				return;
			// + cursor
			m_hCursor = ::LoadCursor(NULL, IDC_CROSS);
			::SetCursor (m_hCursor);
			m_iMousePointer = 2;
			m_bDefaultCursor = false;
			break;
		}
	case 3:
		{
			if (m_iMousePointer == 3)
				return;
			// I beam cursor
			m_hCursor = ::LoadCursor(NULL, IDC_IBEAM);
			::SetCursor (m_hCursor);
			m_iMousePointer = 3;
			m_bDefaultCursor = false;
			break;
		}
	case 4:
		{
			if (m_iMousePointer == 4)
				return;
			// Up Arrow cursor
			m_hCursor = ::LoadCursor(NULL, IDC_UPARROW);
			::SetCursor (m_hCursor);
			m_iMousePointer = 4;
			m_bDefaultCursor = false;
			break;
		}
	case 5:
		{
			if (m_iMousePointer == 5)
				return;
			// Size All cursor
			m_hCursor = ::LoadCursor(NULL, IDC_SIZEALL);
			::SetCursor (m_hCursor);
			m_iMousePointer = 5;
			m_bDefaultCursor = false;
			break;
		}
	case 6:
		{
			if (m_iMousePointer == 6)
				return;
			// North South cursor
			m_hCursor = ::LoadCursor(NULL, IDC_SIZENS);
			::SetCursor (m_hCursor);
			m_iMousePointer = 6;
			m_bDefaultCursor = false;
			break;
		}
	case 7:
		{
			if (m_iMousePointer == 7)
				return;
			// East west cursor
			m_hCursor = ::LoadCursor(NULL, IDC_SIZEWE);
			::SetCursor (m_hCursor);
			m_iMousePointer = 7;
			m_bDefaultCursor = false;
			break;
		}
	case 8:
		{
			if (m_iMousePointer == 8)
				return;
			// North South cursor
			m_hCursor = ::LoadCursor(NULL, IDC_WAIT);
			::SetCursor (m_hCursor);
			m_iMousePointer = 8;
			m_bDefaultCursor = false;
			break;
		}
	case 99:
		{
			if (m_iMousePointer == 99)
				return;

			m_iMousePointer = 99;

			break;
		}
	default:
		break;
	}
}
//-------------------------------------------------------------------------------------------------
int CUCDBitmapWindow::GetMousePointer ()
{
	return m_iMousePointer;
}
//-------------------------------------------------------------------------------------------------
void CUCDBitmapWindow::UpdateCursor()
{
	::SetCursor (m_hCursor);
}
//-------------------------------------------------------------------------------------------------
void CUCDBitmapWindow::SetShowCursor (bool bShow)
{ 
	if (bShow)
	{
		if (!m_bShowCursor)			
		{
			ShowCursor(true);
			m_bShowCursor = true;
		}
	}
	else if (bShow == false)
	{
		if (m_bShowCursor)
		{
			ShowCursor(false);
			m_bShowCursor = false;
		}
	}
}
//-------------------------------------------------------------------------------------------------
