// ObjPropertiesDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "uclidcomutils.h"
#include "ObjPropertiesDlg.h"

#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// ObjPropertiesDlg dialog
//-------------------------------------------------------------------------------------------------
ObjPropertiesDlg::ObjPropertiesDlg(IUnknown *pObjWithPropPage, 
								   const char *pszWindowTitle, CWnd* pParent)
:CDialog(ObjPropertiesDlg::IDD, pParent), m_pCurrentPropPage(NULL), 
 m_ipObjWithPropPage(pObjWithPropPage), m_strWindowTitle(pszWindowTitle)
{
	//{{AFX_DATA_INIT(ObjPropertiesDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void ObjPropertiesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(ObjPropertiesDlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(ObjPropertiesDlg, CDialog)
	//{{AFX_MSG_MAP(ObjPropertiesDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
BEGIN_INTERFACE_MAP(ObjPropertiesDlg, CDialog)
    INTERFACE_PART(ObjPropertiesDlg, IID_IPropertyPageSite, PropertyPageSite)
END_INTERFACE_MAP()
//-------------------------------------------------------------------------------------------------
BOOL ObjPropertiesDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		CDialog::OnInitDialog();
		
		// update the window title
		SetWindowText(m_strWindowTitle.c_str());

		// display the property page of the object here
		SetCurrentPage(m_ipObjWithPropPage);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18615");
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void ObjPropertiesDlg::OnOK() 
{
//	try
	{
		// The call to Apply() fails if the settings could not
		// be validated.  If the settings could not be validated, then
		// just return;
		if (m_pCurrentPropPage != __nullptr && m_pCurrentPropPage->Apply() == S_FALSE)
			return;
		
		// check if the component implements IMustBeConfiguredObject
		// if it does, make sure that the object has been configured successfully
		// before dismissing the dialog
		UCLID_COMUTILSLib::IMustBeConfiguredObjectPtr ipObj = m_ipObjWithPropPage;
		if (ipObj != __nullptr)
		{
			if (ipObj->IsConfigured() == VARIANT_FALSE)
			{
				MessageBox("Object has not been configured completely.  Please specify all required properties.", "Error", MB_ICONEXCLAMATION);
				return;
			}
		}
	}
//	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08132");

	// default handling + closing of dialog
	CDialog::OnOK();
}

//-------------------------------------------------------------------------------------------------
// ObjPropertiesDlg COM interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_( ULONG ) ObjPropertiesDlg::XPropertyPageSite::AddRef()
{
    METHOD_PROLOGUE( ObjPropertiesDlg, PropertyPageSite )
	return pThis->ExternalAddRef();
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_( ULONG ) ObjPropertiesDlg::XPropertyPageSite::Release()
{
    METHOD_PROLOGUE( ObjPropertiesDlg, PropertyPageSite )
	return pThis->ExternalRelease();
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP ObjPropertiesDlg::XPropertyPageSite::QueryInterface( REFIID iid, void FAR* FAR* ppvObj)
{
    METHOD_PROLOGUE( ObjPropertiesDlg, PropertyPageSite )
    return (HRESULT)pThis->ExternalQueryInterface( &iid, ppvObj );
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP ObjPropertiesDlg::XPropertyPageSite::OnStatusChange( DWORD dwFlags )
{
    METHOD_PROLOGUE( ObjPropertiesDlg, PropertyPageSite )
	
	// TODO: enable the APPLY button...but for now there is no such button on our UI

	return NOERROR;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP ObjPropertiesDlg::XPropertyPageSite::GetLocaleID( LCID *pLocaleID )
{
    METHOD_PROLOGUE( ObjPropertiesDlg, PropertyPageSite )
	*pLocaleID = GetThreadLocale();
	return NOERROR;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP ObjPropertiesDlg::XPropertyPageSite::GetPageContainer( IUnknown **ppUnk )
{
    METHOD_PROLOGUE( ObjPropertiesDlg, PropertyPageSite )
	return E_FAIL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP ObjPropertiesDlg::XPropertyPageSite::TranslateAccelerator( MSG *pMsg )
{
    METHOD_PROLOGUE( ObjPropertiesDlg, PropertyPageSite )

	return pThis->PreTranslateMessage( pMsg ) ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
void ObjPropertiesDlg::SetCurrentPage(IUnknown * pUnknown)
{
	CRect rect;
	GetDlgItem(IDC_STATIC_PROPERTIES)->GetWindowRect(&rect);
  	ScreenToClient(&rect);

	// hide previous page and release page pointer
	if( m_pCurrentPropPage != __nullptr )
	{
		m_pCurrentPropPage->Show(SW_HIDE);
		m_pCurrentPropPage->Deactivate();
		m_pCurrentPropPage = NULL;

		if(pUnknown == NULL)
		{
			throw UCLIDException("ELI04166", "Invalid object!");
		}
	}
	
	// get access to the ISpecifyPropertyPages interface of the object.
	ISpecifyPropertyPagesPtr pSpecifyPropertyPages = pUnknown;
	if (pSpecifyPropertyPages == NULL)
	{
		// the object does not support property pages...throw exception
		throw UCLIDException("ELI04167", "The object does not support property pages!");
	}

	// find the number of property pages the object has
	CAUUID pages;
	if (FAILED(pSpecifyPropertyPages->GetPages(&pages)))
	{
		throw UCLIDException("ELI04168", "Unable to retrieve the number of property pages supported by the object!");
	}

	// make sure that EXACTLY ONE property page is available
	// NOTE: In the future, when this object supports displaying of multiple
	// property pages for the same object, change ==1 in the code below to >= 1
	if (!(pages.cElems == 1 && pages.pElems != __nullptr ))
	{
		throw UCLIDException("ELI04169", "Invalid number of property pages supported by the object!");	
	}

	// create an instance of the first property page of the object
	if (FAILED(CoCreateInstance( pages.pElems[0], NULL, CLSCTX_INPROC,
		IID_IPropertyPage, (void**) &m_pCurrentPropPage)))
	{
		throw UCLIDException("ELI04170", "Unable to create property page!");	
	}

	// set this dialog as the container of the property page
	if (FAILED(m_pCurrentPropPage->SetPageSite((IPropertyPageSite*) 
		GetInterface(&IID_IPropertyPageSite))))
	{
		throw UCLIDException("ELI04171", "Unable to set container for the property page!");	
	}

	// set the object associated with the property page
	if (FAILED(m_pCurrentPropPage->SetObjects(1, &pUnknown)))
	{
		throw UCLIDException("ELI04172", "Unable to set object associated with the property page!");	
	}

	PROPPAGEINFO pageInfo;
	m_pCurrentPropPage->GetPageInfo(&pageInfo);
	// resize the current dialog to fit the property page
	updateDialogSize(pageInfo.size, rect);
	rect.left += 3;
	rect.top += 10;
	rect.bottom -= 3;
	rect.right -= 3;


	// activate the property page
	if (FAILED(m_pCurrentPropPage->Activate(GetSafeHwnd(), &rect, TRUE)))
	{
		throw UCLIDException("ELI04173", "Unable to activate the property page!");	
	}

	// show the property page
	if (FAILED(m_pCurrentPropPage->Show(SW_SHOW)))
	{
		throw UCLIDException("ELI04174", "Unable to show the property page!");	
	}

	// TODO: when an apply button is present in this UI in the future,
	// disable it at this time

	//Clean up memory allocated in call to GetPageInfo
	CoTaskMemFree(pageInfo.pszTitle);
	CoTaskMemFree(pageInfo.pszDocString);
	CoTaskMemFree(pageInfo.pszHelpFile);

	// Clean up memory allocated in call to GetPages
	CoTaskMemFree(pages.pElems);
}
//-------------------------------------------------------------------------------------------------
void ObjPropertiesDlg::updateDialogSize(SIZE propPageSize, RECT &rectForHoldingPP)
{
	// Get current size and spacing of controls
	CRect rectOK, rectCancel, rectBorder, rectDlg;
	GetWindowRect(&rectDlg);
	GetDlgItem(IDC_STATIC_PROPERTIES)->GetWindowRect(&rectBorder);
	GetDlgItem(IDOK)->GetWindowRect(&rectOK);
	GetDlgItem(IDCANCEL)->GetWindowRect(&rectCancel);
	int iDiffX = rectBorder.left - rectDlg.left;
	int iDiffYTop = rectBorder.top - rectDlg.top;
	int iDiffYBottom = rectOK.top - rectBorder.bottom;

	ScreenToClient(rectOK);
	ScreenToClient(rectCancel);
	ScreenToClient(rectBorder);
	int nButtonHeight = rectOK.Height();
	int nButtonWidth = rectOK.Width();
	int nButtonSpace = rectCancel.left - rectOK.right;
	int nBorderSpace = 3;

	// resize the outer-bound rect, which will hold the property page
	rectBorder.right = rectBorder.left + propPageSize.cx + nBorderSpace;
	rectBorder.bottom = 2*rectBorder.top + propPageSize.cy + nBorderSpace;
	GetDlgItem(IDC_STATIC_PROPERTIES)->MoveWindow(rectBorder);

	// Adjust size of the OK and Cancel buttons
	rectOK.top = rectBorder.bottom + nButtonSpace;
	rectOK.bottom = rectOK.top + nButtonHeight;
	rectOK.left = rectBorder.right - nButtonWidth*2 - nButtonSpace;
	rectOK.right = rectBorder.right - nButtonWidth - nButtonSpace;
	rectCancel.top = rectOK.top;
	rectCancel.bottom = rectCancel.top + nButtonHeight;
	rectCancel.left = rectBorder.right - nButtonWidth;
	rectCancel.right = rectBorder.right;

	// move the OK and Cancel buttons
	GetDlgItem(IDOK)->MoveWindow(&rectOK);
	GetDlgItem(IDCANCEL)->MoveWindow(&rectCancel);

	// resize the window to account for the PP, OK & Cancel buttons, appropriate space
	SetWindowPos( &wndTop, rectDlg.left, 
		rectDlg.top, rectBorder.Width() + 2*iDiffX, 
		rectBorder.Height() + nButtonHeight + iDiffYTop + 2*iDiffYBottom, 
		SWP_NOZORDER );

	// Provide border rectangle to caller
	rectForHoldingPP = rectBorder;
}
//-------------------------------------------------------------------------------------------------
