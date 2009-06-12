
#include "stdafx.h"
#include "UCLIDHighlightWindow.h"
#include "HighlightWindow.h"
#include "MFCHighlightWindow.h"

#include <UCLIDException.h>

using namespace std;

//--------------------------------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// CHighlightWindow
/////////////////////////////////////////////////////////////////////////////
//--------------------------------------------------------------------------------------------------
CHighlightWindow::CHighlightWindow()
:m_DefaultColor(RGB(255, 255, 64)), 
 m_AlternateColor1(RGB(255, 128, 128)),
 m_lParentWndHandle(NULL)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
}
//--------------------------------------------------------------------------------------------------
CHighlightWindow::~CHighlightWindow()
{
	try
	{
		m_apDlg.reset(NULL);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20409");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightWindow::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IHighlightWindow
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightWindow::Show(long hWndParent, long hWndChild)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		HWND hParent = (HWND) hWndParent;
		HWND hChild = (HWND) hWndChild;
		getHighlightWindow()->show(hParent, hChild);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04047")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightWindow::HideAndForget()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		getHighlightWindow()->hide();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04048")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightWindow::HideAndRemember()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		getHighlightWindow()->hide(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04049")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightWindow::Refresh()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		getHighlightWindow()->refresh();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04050")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightWindow::SetDefaultColor(OLE_COLOR color)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_DefaultColor = (COLORREF) color;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04062")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightWindow::SetAlternateColor1(OLE_COLOR color)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_AlternateColor1 = (COLORREF) color;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04063")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightWindow::UseDefaultColor()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		getHighlightWindow()->setColor(m_DefaultColor);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04064")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightWindow::UseAlternateColor1()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		getHighlightWindow()->setColor(m_AlternateColor1);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04065")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CHighlightWindow::put_ParentWndHandle(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_lParentWndHandle = newVal;

	return S_OK;
}

//////////////////////
// Helper functions
/////////////////////
//--------------------------------------------------------------------------------------------------
MFCHighlightWindow* CHighlightWindow::getHighlightWindow()
{
	if (m_apDlg.get() == NULL)
	{
		CWnd *pParentWnd = m_lParentWndHandle == NULL ? NULL : CWnd::FromHandle((HWND) m_lParentWndHandle);
		m_apDlg = auto_ptr<MFCHighlightWindow>(new MFCHighlightWindow(pParentWnd));
	}

	return m_apDlg.get();
}