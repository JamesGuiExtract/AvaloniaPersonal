// InputCorrectionDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ifcore.h"
#include "InputCorrectionDlg.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <FileDialogEx.h>

#include <memory>

using namespace std;
extern CComModule _Module;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// InputCorrectionDlg dialog


InputCorrectionDlg::InputCorrectionDlg(IInputValidator* ipInputValidator, 
									   ITextInput* ipTextInput,
									   CWnd* pParent /*=NULL*/)
: CDialog(InputCorrectionDlg::IDD, pParent),
  m_ipInputValidator(ipInputValidator),
  m_ipTextInput(ipTextInput),
  m_bIsInputCorrect(false)
{
	//{{AFX_DATA_INIT(InputCorrectionDlg)
	m_editInputType = _T("");
	m_editInputText = _T("");
	//}}AFX_DATA_INIT

}


void InputCorrectionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(InputCorrectionDlg)
	DDX_Control(pDX, IDC_BTN_SaveImageAs, m_btnSaveImage);
	DDX_Control(pDX, IDC_IMAGE, m_ctrlImageLoader);
	DDX_Control(pDX, IDC_EDIT_INPUT_TEXT, m_ctrlInputText);
	DDX_Text(pDX, IDC_EDIT_INPUT_TYPE, m_editInputType);
	DDX_Text(pDX, IDC_EDIT_INPUT_TEXT, m_editInputText);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(InputCorrectionDlg, CDialog)
	//{{AFX_MSG_MAP(InputCorrectionDlg)
	ON_EN_CHANGE(IDC_EDIT_INPUT_TEXT, OnChangeEditInputText)
//	ON_WM_GETMINMAXINFO()
	ON_BN_CLICKED(IDC_BTN_SaveImageAs, OnBTNSaveImageAs)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// InputCorrectionDlg message handlers

int InputCorrectionDlg::DoModal() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );
	
	return CDialog::DoModal();
}

void InputCorrectionDlg::OnCancel() 
{
	// user doesn't want to correct the text
	// do nothing
	
	CDialog::OnCancel();
}

void InputCorrectionDlg::OnChangeEditInputText() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	// TODO: If this is a RICHEDIT control, the control will not
	// send this notification unless you override the CDialog::OnInitDialog()
	// function and call CRichEditCtrl().SetEventMask()
	// with the ENM_CHANGE flag ORed into the mask.
	
	try
	{
		UpdateData(TRUE);

		// while the input text is changing, check its validity on the fly
		m_ipTempTextInput->InitTextInput(NULL, _bstr_t(m_editInputText));

		VARIANT_BOOL bValid = VARIANT_FALSE;
		m_ipInputValidator->ValidateInput(m_ipTempTextInput, &bValid);

		m_bIsInputCorrect = (bValid == VARIANT_TRUE) ? true : false;

		// update indicator
		updateInputStatusBitmap();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02991")
}

BOOL InputCorrectionDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		CDialog::OnInitDialog();
	
		//////////////////
		// set input type
		CComBSTR bstrInput;
		HRESULT hr = m_ipInputValidator->GetInputType(&bstrInput);
		if (FAILED(hr))
		{
			UCLIDException uclidException("ELI02990", "Failed to get input type.");
			uclidException.addDebugInfo("HRESULT", hr);
			throw uclidException;
		}
		m_editInputType = bstrInput;

		//////////////////
		// set input text
		bstrInput.Empty();
		hr = m_ipTextInput->GetText(&bstrInput);
		if (FAILED(hr))
		{
			UCLIDException uclidException("ELI02989", "Failed to get input text.");
			uclidException.addDebugInfo("HRESULT", hr);
			throw uclidException;
		}
		m_editInputText = bstrInput;

		////////////////////////
		// create temp text input
		hr = m_ipTempTextInput.CoCreateInstance(__uuidof(TextInput));
		if (FAILED(hr))
		{
			UCLIDException uclidException("ELI02992", "Failed to create text input object.");
			uclidException.addDebugInfo("HRESULT", hr);
			throw uclidException;
		}
		m_ipTempTextInput->InitTextInput(NULL, bstrInput);
		
		/////////////////////////
		// Init dialog size
		// first check if there's any image needs to be displayed.
		CComQIPtr<IInputEntity> ipInputEntity;
		hr = m_ipTextInput->GetInputEntity(&ipInputEntity);
		if (SUCCEEDED(hr) && ipInputEntity != NULL)
		{	
			bstrInput.Empty();
			ipInputEntity->GetOCRImage(&bstrInput);
			m_zImageFileName = bstrInput;
			m_zImageFileName.TrimLeft(" \t");
			m_zImageFileName.TrimRight(" \t");
		}

		initDialogSize(m_zImageFileName);

		// initially input is invalid
		m_bIsInputCorrect = false;
		// update bitmap indicator
		updateInputStatusBitmap();

		
		UpdateData(FALSE);

		m_ctrlInputText.SetFocus();

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02988")
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void InputCorrectionDlg::OnOK() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// if the input is valid
		if (m_bIsInputCorrect)
		{
			CComBSTR bstrText;
			m_ipTempTextInput->GetText(&bstrText);
			// update the original text input object with the temp text input
			m_ipTextInput->SetText(bstrText);
			
			// set validated input object
			IUnknownPtr ipValidatedInput;
			HRESULT hr = m_ipTempTextInput->GetValidatedInput(&ipValidatedInput);
			if (FAILED(hr))
			{
				UCLIDException uclidException("ELI02994", "Failed to get validated input object.");
				uclidException.addDebugInfo("HRESULT", hr);
				throw uclidException;
			}
			m_ipTextInput->SetValidatedInput(ipValidatedInput);
			
			CDialog::OnOK();
			
			return;
		}
		
		// otherwise, keep this input correction dialog on screen
		MessageBox("Please enter a valid input.", "Error", MB_ICONEXCLAMATION | MB_OK );
		// set focus on the input edit box
		m_ctrlInputText.SetSel(-1);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02993")
}

void InputCorrectionDlg::OnBTNSaveImageAs() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );
	
	try
	{
		// open save as dialog
		CFileDialogEx saveDialog
			(FALSE, 
			"bmp", 
			NULL, 
			OFN_NOREADONLYRETURN | OFN_OVERWRITEPROMPT | OFN_NOCHANGEDIR,	// no read only file can be overwritten
			"bmp files (*.bmp)|*.bmp|"
			"||", 
			NULL);

		if (saveDialog.DoModal() == IDOK)
		{
			// save the graphical contents of this control to the specified file
			string strSaveFileName = saveDialog.GetPathName();
			
			// copy the image file 
			copyFile((LPCTSTR) m_zImageFileName, strSaveFileName);
		}

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03780")
}

//////////////////////////////////////////////////////////////////////////////
// Helper functions
void InputCorrectionDlg::initDialogSize(const CString& cstrImageFileName)
{
	try
	{
		// if it's not empty
		if (!cstrImageFileName.IsEmpty())
		{
			// rect of the dialog
			CRect dlgCurrentRect;		
			GetWindowRect(&dlgCurrentRect);
			
			// Load the bitmap without modifying image dimensions
			HANDLE hBmp = ::LoadImage( _Module.m_hInstResource, cstrImageFileName, 
				IMAGE_BITMAP, 0, 0, LR_DEFAULTCOLOR | LR_LOADFROMFILE );
			
			// Get bitmap size
			CBitmap	*pBM = CBitmap::FromHandle( (HBITMAP)hBmp );
			BITMAP	bm;
			pBM->GetBitmap( &bm );
			
			// Get image ratios
			long	lMaxWidth = dlgCurrentRect.Width() - 30;
			long	lMaxHeight = 50;
			double	dLimit = (double)lMaxWidth / (double)lMaxHeight;
			double	dActual = (double)bm.bmWidth / (double)bm.bmHeight;
			
			// Compute new size for best fit to dialog
			HANDLE hBmp2 = hBmp;
			long	lNewWidth = bm.bmWidth;
			long	lNewHeight = bm.bmHeight;
			if (dLimit > dActual)
			{
				// Adjust bitmap height
				lNewHeight = lMaxHeight;
				lNewWidth = (long)(dActual * lNewHeight);
			}
			else
			{
				// Adjust bitmap width
				lNewWidth = lMaxWidth;
				lNewHeight = (long)((double)lNewWidth / dActual);
			}
			
			// Resize bitmap for best fit to dialog
			hBmp2 = ::CopyImage( hBmp, IMAGE_BITMAP, lNewWidth, lNewHeight, 
				LR_COPYDELETEORG | LR_COPYRETURNORG );
			
			// display the image on screen
			m_ctrlImageLoader.SetBitmap( (HBITMAP)hBmp2 );
			m_ctrlImageLoader.ShowWindow(TRUE);
			m_btnSaveImage.ShowWindow(TRUE);
			
			
			CRect rectInput;
			m_ctrlInputText.GetWindowRect(&rectInput);
			
			// get rect of image loader and base on its position to settle
			// the rest of the buttons like Save Image, OK and Cancel
			CRect rectImage;
			m_ctrlImageLoader.GetWindowRect(&rectImage);
			
			// move the Save Image, OK and Cancel buttons
			CRect rectSaveImage;
			m_btnSaveImage.GetWindowRect(&rectSaveImage);
			int dx, dy;
			dx = 0;
			dy = rectImage.bottom +5 - rectSaveImage.top;
			ScreenToClient(rectSaveImage);
			rectSaveImage.OffsetRect(dx, dy);
			m_btnSaveImage.MoveWindow(&rectSaveImage);
			m_btnSaveImage.InvalidateRect(&rectSaveImage);
			m_btnSaveImage.Invalidate(false);
			
			CRect rectOK;
			GetDlgItem(IDOK)->GetWindowRect(&rectOK);
			ScreenToClient(rectOK);
			rectOK.OffsetRect(dx, dy);
			GetDlgItem(IDOK)->MoveWindow(&rectOK);
			GetDlgItem(IDOK)->InvalidateRect(&rectOK);
			GetDlgItem(IDOK)->Invalidate(false);
			
			CRect rectCancel;
			GetDlgItem(IDCANCEL)->GetWindowRect(&rectCancel);
			ScreenToClient(rectCancel);
			rectCancel.OffsetRect(dx, dy);
			GetDlgItem(IDCANCEL)->MoveWindow(&rectCancel);
			GetDlgItem(IDCANCEL)->InvalidateRect(&rectCancel);
			GetDlgItem(IDCANCEL)->Invalidate(false);
			
			// last, let's move the image loader to the center of the dialog horizontally
			dx = (lMaxWidth - lNewWidth)/2;
			dy = 0;
			ScreenToClient(rectImage);
			rectImage.OffsetRect(dx, dy);
			GetDlgItem(IDC_IMAGE)->MoveWindow(&rectImage);
			GetDlgItem(IDC_IMAGE)->InvalidateRect(&rectImage);
			GetDlgItem(IDC_IMAGE)->Invalidate(false);
			
			SetWindowPos (&wndTop, dlgCurrentRect.left, dlgCurrentRect.top, dlgCurrentRect.Width(), rectOK.bottom + 35, SWP_NOZORDER);
			
			return;
		}
	}
	catch (...)
	{
		// ignore any exception and execute the next
	}

	// show the dialog at its minimum
	// make the image loader invisible
	m_ctrlImageLoader.ShowWindow(FALSE);
	m_btnSaveImage.ShowWindow(FALSE);
}

void InputCorrectionDlg::updateInputStatusBitmap()
{
	//	If the current string is valid, then show the "good" bitmap.
	//	Otherwise  show the "bad" bitmap
	BOOL bCorrect = m_bIsInputCorrect ? TRUE : FALSE;
	
	GetDlgItem(IDC_GOOD)->ShowWindow(bCorrect);
	GetDlgItem(IDC_BAD)->ShowWindow(!bCorrect);
}

