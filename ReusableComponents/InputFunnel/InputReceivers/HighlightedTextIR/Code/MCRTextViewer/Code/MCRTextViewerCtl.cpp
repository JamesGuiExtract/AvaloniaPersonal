//===========================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRTextViewerCtl.cpp
//
// PURPOSE:	This is an implementation file for CUCLIDMCRTextViewerCtrl() class.
//			The CUCLIDMCRTextViewerCtrl() class has been derived from 
//			CActiveXDocControl() which is derived from COleControl().  The 
//			code written in this file implements the various application 
//			methods in the control.
// NOTES:	
//
// AUTHORS:	
//
//===========================================================================

#include "stdafx.h"
#include "MCRTextViewer.h"
#include "MCRTextViewerCtl.h"
#include "MCRTextViewerPpg.h"
#include "MCRDocument.h"
#include "MCRFrame.h"
#include "MCRView.h"
#include "msword9.h"

#include <UCLIDException.h>
#include <io.h>
#include <math.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


IMPLEMENT_DYNCREATE(CUCLIDMCRTextViewerCtrl, CActiveXDocControl)


// Supported font sizes
// NOTE: Not all fonts support all sizes
static int iFontSizes[] =
	{6, 7, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72};

/////////////////////////////////////////////////////////////////////////////
// Message map

BEGIN_MESSAGE_MAP(CUCLIDMCRTextViewerCtrl, CActiveXDocControl)
	//{{AFX_MSG_MAP(CUCLIDMCRTextViewerCtrl)
	ON_WM_CREATE()
	//}}AFX_MSG_MAP
	ON_OLEVERB(AFX_IDS_VERB_PROPERTIES, OnProperties)
END_MESSAGE_MAP()


/////////////////////////////////////////////////////////////////////////////
// Dispatch map

BEGIN_DISPATCH_MAP(CUCLIDMCRTextViewerCtrl, CActiveXDocControl)
	//{{AFX_DISPATCH_MAP(CUCLIDMCRTextViewerCtrl)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "open", open, VT_EMPTY, VTS_BSTR)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "save", save, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "saveAs", saveAs, VT_EMPTY, VTS_BSTR)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "clear", clear, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "parseText", parseText, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "enableTextSelection", enableTextSelection, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "getEntityText", getEntityText, VT_BSTR, VTS_I4)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "pasteTextFromClipboard", pasteTextFromClipboard, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "copyTextToClipboard", copyTextToClipboard, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "setTextFontName", setTextFontName, VT_EMPTY, VTS_BSTR)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "getTextFontName", getTextFontName, VT_BSTR, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "setTextSize", setTextSize, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "getTextSize", getTextSize, VT_I4, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "increaseTextSize", increaseTextSize, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "decreaseTextSize", decreaseTextSize, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "appendTextFromClipboard", appendTextFromClipboard, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "setText", setText, VT_EMPTY, VTS_BSTR)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "appendText", appendText, VT_EMPTY, VTS_BSTR)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "isModified", isModified, VT_I4, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "print", print, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "getFileName", getFileName, VT_BSTR, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "setEntityText", setEntityText, VT_EMPTY, VTS_I4 VTS_BSTR)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "setTextHighlightColor", setTextHighlightColor, VT_EMPTY, VTS_I4 VTS_COLOR)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "isEntityIDValid", isEntityIDValid, VT_I4, VTS_I4)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "setInputFinder", setInputFinder, VT_EMPTY, VTS_UNKNOWN)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "getEntityColor", getEntityColor, VT_COLOR, VTS_I4)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "setEntityColor", setEntityColor, VT_EMPTY, VTS_I4 VTS_COLOR)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "getText", getText, VT_BSTR, VTS_NONE)
	DISP_FUNCTION(CUCLIDMCRTextViewerCtrl, "getSelectedText", getSelectedText, VT_BSTR, VTS_NONE)
	//}}AFX_DISPATCH_MAP
END_DISPATCH_MAP()


/////////////////////////////////////////////////////////////////////////////
// Event map

BEGIN_EVENT_MAP(CUCLIDMCRTextViewerCtrl, CActiveXDocControl)
	//{{AFX_EVENT_MAP(CUCLIDMCRTextViewerCtrl)
	EVENT_CUSTOM("TextSelected", FireTextSelected, VTS_I4)
	EVENT_CUSTOM("SelectedText", FireSelectedText, VTS_BSTR)
	//}}AFX_EVENT_MAP
END_EVENT_MAP()


/////////////////////////////////////////////////////////////////////////////
// Property pages

// TODO: Add more property pages as needed.  Remember to increase the count!
BEGIN_PROPPAGEIDS(CUCLIDMCRTextViewerCtrl, 1)
	PROPPAGEID(CMCRTextViewerPropPage::guid)
END_PROPPAGEIDS(CUCLIDMCRTextViewerCtrl)


/////////////////////////////////////////////////////////////////////////////
// Initialize class factory and guid

IMPLEMENT_OLECREATE_EX(CUCLIDMCRTextViewerCtrl, "UCLIDMCRTEXTVIEWER.UCLIDMCRTextViewerCtrl.1",
	0x7758f110, 0xd3, 0x4e95, 0x81, 0xce, 0x86, 0xc8, 0xf4, 0x83, 0xe3, 0xb3)


/////////////////////////////////////////////////////////////////////////////
// Type library ID and version

IMPLEMENT_OLETYPELIB(CUCLIDMCRTextViewerCtrl, _tlid, _wVerMajor, _wVerMinor)


/////////////////////////////////////////////////////////////////////////////
// Interface IDs

const IID BASED_CODE IID_DUCLIDMCRTextViewer =
		{ 0x68d14862, 0xf691, 0x4413, { 0xb6, 0xc4, 0x3d, 0x17, 0xa0, 0x9c, 0x6b, 0x11 } };
const IID BASED_CODE IID_DUCLIDMCRTextViewerEvents =
		{ 0x46b02873, 0x939c, 0x4a2e, { 0x88, 0x4c, 0x13, 0x5a, 0x20, 0xd8, 0x52, 0x5 } };


/////////////////////////////////////////////////////////////////////////////
// Control type information

static const DWORD BASED_CODE _dwUCLIDMCRTextViewerOleMisc =
	OLEMISC_ACTIVATEWHENVISIBLE |
	OLEMISC_SETCLIENTSITEFIRST |
	OLEMISC_INSIDEOUT |
	OLEMISC_CANTLINKINSIDE |
	OLEMISC_RECOMPOSEONRESIZE;

IMPLEMENT_OLECTLTYPE(CUCLIDMCRTextViewerCtrl, IDS_UCLIDMCRTEXTVIEWER, _dwUCLIDMCRTextViewerOleMisc)


/////////////////////////////////////////////////////////////////////////////
// CUCLIDMCRTextViewerCtrl::CUCLIDMCRTextViewerCtrlFactory::UpdateRegistry -
// Adds or removes system registry entries for CUCLIDMCRTextViewerCtrl

BOOL CUCLIDMCRTextViewerCtrl::CUCLIDMCRTextViewerCtrlFactory::UpdateRegistry(BOOL bRegister)
{
	// TODO: Verify that your control follows apartment-model threading rules.
	// Refer to MFC TechNote 64 for more information.
	// If your control does not conform to the apartment-model rules, then
	// you must modify the code below, changing the 6th parameter from
	// afxRegApartmentThreading to 0.

	if (bRegister)
		return AfxOleRegisterControlClass(
			AfxGetInstanceHandle(),
			m_clsid,
			m_lpszProgID,
			IDS_UCLIDMCRTEXTVIEWER,
			IDB_UCLIDMCRTEXTVIEWER,
			afxRegApartmentThreading,
			_dwUCLIDMCRTextViewerOleMisc,
			_tlid,
			_wVerMajor,
			_wVerMinor);
	else
		return AfxOleUnregisterClass(m_clsid, m_lpszProgID);
}


/////////////////////////////////////////////////////////////////////////////
// CUCLIDMCRTextViewerCtrl::CUCLIDMCRTextViewerCtrl - Constructor

CUCLIDMCRTextViewerCtrl::CUCLIDMCRTextViewerCtrl()
	:m_pMCRFrame(NULL), 
	m_pMCRView(NULL),
	m_ipFinder(NULL)
{
	// GUID initialization
	InitializeIIDs( &IID_DUCLIDMCRTextViewer, &IID_DUCLIDMCRTextViewerEvents );

	// Default size
    SetInitialSize( 200, 200 );

	// Add the single document template
    AddDocTemplate(new CActiveXDocTemplate(
        RUNTIME_CLASS(CMCRDocument),
        RUNTIME_CLASS(CMCRFrame),
        RUNTIME_CLASS(CMCRView)));

	// Set the background color of text to YELLLOW
	m_MCRBkColor = RGB( 255, 255, 0 );

	// Set the highlight color to RED
	m_MCRHighlightColor = RGB( 255, 0, 0 );

	// Reset flag
	m_bFileOpened = false;

	// Operate in "view-text" mode.
	m_bViewMode = true;

	// Set the default value for enabling the selection as TRUE
	m_bEnableSelection = false;  // as per SCR 151

	// clear the token start/end position vectors in the map
	m_mapInputFinderToTokenPositions.clear();

	// Set the default font name
	m_strFontName = "Arial";

	// Set the default font size
	m_iTextSize = 10;
}

/////////////////////////////////////////////////////////////////////////////
// CUCLIDMCRTextViewerCtrl::~CUCLIDMCRTextViewerCtrl - Destructor

CUCLIDMCRTextViewerCtrl::~CUCLIDMCRTextViewerCtrl()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20403");
}

/////////////////////////////////////////////////////////////////////////////
// CUCLIDMCRTextViewerCtrl::OnDraw - Drawing function

void CUCLIDMCRTextViewerCtrl::OnDraw(
			CDC* pdc, const CRect& rcBounds, const CRect& rcInvalid)
{
	// Default display is okay since it is only visible in design mode
	pdc->FillRect(rcBounds, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	pdc->Ellipse(rcBounds);
}

/////////////////////////////////////////////////////////////////////////////
// CUCLIDMCRTextViewerCtrl::DoPropExchange - Persistence support

void CUCLIDMCRTextViewerCtrl::DoPropExchange(CPropExchange* pPX)
{
	ExchangeVersion(pPX, MAKELONG(_wVerMinor, _wVerMajor));
	COleControl::DoPropExchange(pPX);

	// TODO: Call PX_ functions for each persistent custom property.

}

/////////////////////////////////////////////////////////////////////////////
// CUCLIDMCRTextViewerCtrl::OnResetState - Reset control to default state

void CUCLIDMCRTextViewerCtrl::OnResetState()
{
	COleControl::OnResetState();  // Resets defaults found in DoPropExchange

	// TODO: Reset any other control state here.
}

/////////////////////////////////////////////////////////////////////////////
// CUCLIDMCRTextViewerCtrl message handlers

int CUCLIDMCRTextViewerCtrl::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	// Create UCLIDTextFinder and insert into the project
	CRect rect( 0, 0, 10, 10 );
	int iStatus = 0;

	// Do base class creation
	if (CActiveXDocControl::OnCreate(lpCreateStruct) == -1)
	{
		return -1;
	}

	// Set frame member
	m_pMCRFrame = (CMCRFrame*)CActiveXDocControl::m_pFrameWnd;

	// Set view member
	m_pMCRView = (CMCRView*)/*(pmFrame)*/m_pMCRFrame->GetActiveView();
	m_pMCRView->setMCRTextViewerCtrl( this );

	// Success
	return 0;
}

void CUCLIDMCRTextViewerCtrl::open(LPCTSTR strFileName) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// clear out the input text positions map
	m_mapInputFinderToTokenPositions.clear();

	// Get the length of the file path
	int iLength = strlen ( strFileName);
	
	// If no file path, return
	if (iLength == 0)
	{
		// Provide error message
		CString zError("ELI03332, Could not open file - file is empty. ");
		zError += "FileName - ";
		zError += strFileName;
		AfxThrowOleDispatchException(0, zError);
	}
	
	CMCRDocument* pDoc = NULL;
	
	// Generally file extensions are only 3 characters. The following code works well
	// even if the extension of files are more than 3 characters like *.abcd
	
	// Get the extension of the file to be opened.
	CString zFileNameWithExtension( strFileName );
	
	// Find the position of the .(period) in the file path
	int iExtPos = zFileNameWithExtension.ReverseFind ('.');
	
	// Get the file extension into a string
	CString zExtension = zFileNameWithExtension.Right( iLength - iExtPos - 1 );
	
	// If the extension is txt get the file into a buffer and set the 
	// text into the rich edit control
	if (_strcmpi(zExtension, "txt")==0)
	{	
		CString zData, zTemp;
		
		// FILE pointer to the text file which has to be opened
		FILE *fp = NULL;
		
		int iAsciiValue;
		long lCount = 0, lCharIndex = 0;
		
		// Check file existence
		if (_access( strFileName, 0 ) != 0)
		{
			// Provide error message
			CString zError("ELI03333, specified file doesn't exist. FileName - ");
			zError += strFileName;
			AfxThrowOleDispatchException(0, zError);
		}
		
		// Check permission for read access
		if (_access( strFileName, 04 ) != 0)
		{
			// Provide error message
			CString zError("ELI03334, specified file can't be open for read. FileName - ");
			zError += strFileName;
			AfxThrowOleDispatchException(0, zError);
		}
		
		// Open text file
		int iError = fopen_s( &fp, strFileName, "r" );
		if (fp == NULL)
		{
			// Provide error message
			CString zError("ELI03335, specified file can't be open for write. FileName - ");
			zError += strFileName;
			AfxThrowOleDispatchException(0, zError);
		}
		
		// Find the text length in the file
		fseek( fp, 0L, SEEK_END );
		lCount = ftell( fp );
		fseek( fp, 0L, SEEK_SET );

		// Character buffer to store the text file data
		char* pszBuf = new char[lCount+5]; 
		
		// Checking for the existence of character buffer
		if (pszBuf == NULL)
		{
			AfxThrowOleDispatchException(0, "ELI03361, Failed to allocate memory for char.");
		}
		
		// Setting buffer to NULL
		memset( pszBuf, '\0', lCount + 5 );
		
		// Store file data into the buffer
		while ((iAsciiValue = fgetc(fp)) != EOF)
		{
			if (iAsciiValue != 0x0D)
			{
				pszBuf[lCharIndex++] = iAsciiValue;
			}
		}
		
		// Send the buffer data to the control
		m_pMCRView->SetWindowText( pszBuf );
		
		// Remove the buffer.
		if (pszBuf)
		{
			delete (pszBuf);
		}
		
		// Close the file 
		if (fp)
		{
			fclose( fp );
			fp = NULL;
		}
	}
	else
	{
		// If the file is not a text file, copy the contents through 
		// MS Word to the Clipboard
		
		// Modify example from Q220911
		_Application	oWord;
		Documents		oDocs;
		_Document		oDoc;
		Range			oRange;
		COleVariant		vtOptional( (long)DISP_E_PARAMNOTFOUND, VT_ERROR );
		COleVariant		vtFalse( (short)FALSE );
		COleVariant		vtFile( strFileName, VT_BSTR );
		
		// Create an instance of Word
		if (!oWord.CreateDispatch("Word.Application")) 
		{
			CString zError("ELI07315, Microsoft Word failed to start or is not installed on this machine.");
			AfxThrowOleDispatchException(0, zError);
		} 
		else 
		{
			// Set the visible property
			oWord.SetVisible(FALSE);
			
			// Add a new document
			oDocs = oWord.GetDocuments();
			
			// Open the file
			oDoc = oDocs.Open( vtFile, vtOptional, vtOptional, vtOptional, 
				vtOptional, vtOptional, vtOptional, vtOptional, vtOptional, 
				vtOptional, vtOptional, vtOptional );
			
			// Create a Range object encompassing the entire text area
			oRange = oDoc.Range( vtOptional, vtOptional );
			
			// Copy the text to the Clipboard
			oRange.Copy();
			
			// Paste from the Clipboard - replacing current text
			pasteTextFromClipboard();
			
			// Close the document and application
			oDoc.Close( vtFalse, vtOptional, vtOptional );
			oWord.Quit( vtFalse, vtOptional, vtOptional );
		}
	}
	
	// Assign the opened file name with path to current file member
	setCurrentFile( strFileName );
	
	// Set the file status as opened.
	setFileStatus( TRUE );
	
	// Hide window, so that scroll is not visible
	m_pMCRView->GetRichEditCtrl().ShowWindow( SW_HIDE );
	
	// Parse and highlight the text
	parseText();
	
	// Clear the modified flag
	m_pMCRView->GetRichEditCtrl().SetModify( FALSE );	
	
	// Show window
	m_pMCRView->GetRichEditCtrl().ShowWindow( SW_SHOW );
}

void CUCLIDMCRTextViewerCtrl::save() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// If a file is already opened save into that
		CString zFilePath;

		if (getFileStatus() && !getCurFilePath().IsEmpty())	
		{
			// Get the current opened file path
			zFilePath.Format( "%s", getCurFilePath() );
			saveAs( getCurFilePath().operator LPCTSTR() );
			return;
		}
		else
		{
			// Open SAVE AS file dialog
			CFileDialog fileDlg( FALSE, "*.txt", NULL, OFN_READONLY | OFN_HIDEREADONLY,
				"Text Files (*.txt)|*.txt||", this );

			if (fileDlg.DoModal() == IDOK)
			{
				// Get the selected file complete path
				zFilePath = fileDlg.GetPathName();
				setFileStatus( TRUE );
				setCurrentFile( zFilePath );
				saveAs( zFilePath.operator LPCTSTR() );
				return;
			}
			else
			{
				return;
			}
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1457, "ELI01457 : Failure in save()" );
	}
}

void CUCLIDMCRTextViewerCtrl::saveAs(LPCTSTR strFileName) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// File name must be provided
		if (strlen( strFileName ) == 0)
		{
			AfxThrowOleDispatchException(0, "ELI03362, File name can't be empty.");
		}

		// Get length of the text in the view
		long lTextLength = m_pMCRView->GetRichEditCtrl().GetTextLength();

		// Text must be provided in order to save
		if (lTextLength == 0)
		{
			AfxThrowOleDispatchException(0, "ELI03363, Unable to save empty file.");
		}

		// Open the file
		FILE *fp = NULL;
		int iError = fopen_s( &fp, strFileName, "w" );
		if (fp == NULL)
		{
			CString sMsg("");
			sMsg.Format("ELI03364, Could not open file for save. File name - %s", strFileName);
			AfxThrowOleDispatchException(0, sMsg);
		}

		// Write the text to the file
		CString zMCRText;
		m_pMCRView->GetRichEditCtrl().GetWindowText( zMCRText );
		fwrite( (LPCTSTR)zMCRText, sizeof(char), lTextLength, fp );

		// Close the file
		fclose( fp );
		fp = NULL;

		// Store the new file name
		setCurrentFile( strFileName );
		setFileStatus( TRUE );

		// Reset modify flag
		m_pMCRView->GetRichEditCtrl().SetModify( FALSE );
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1456, "ELI01456 : Failure in saveAs()" );
	}
}

void CUCLIDMCRTextViewerCtrl::clear() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// clear out the input text positions map
		m_mapInputFinderToTokenPositions.clear();

		// Clear text in the control
		m_pMCRView->GetRichEditCtrl().SetWindowText("");

		// Set file status
		setFileStatus( FALSE );
		m_zCurFile = "";

		// Reset modify flag
		m_pMCRView->GetRichEditCtrl().SetModify( FALSE );

		// Set Focus to the control
		m_pMCRView->GetRichEditCtrl().SetFocus();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1455, "ELI01455 : Failure in clear()" );
	}
}

void CUCLIDMCRTextViewerCtrl::enableTextSelection(long ulValue) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	m_bEnableSelection = ulValue ? true : false;
	
	if( ulValue >= 0 )
	{
		//	If text selection is enabled, and 
		//	the ActiveX control is in "view-text" mode
		//	then the ActiveX control's mouse cursor will change to the "SelectTextCursor"
		//	whenever the mouse is on top of a highlighted text (i.e. any MCR'able text).
		//	when text selection is enabled, if the user clicks on highlighted text
		//	the ActiveX control will send out a TextSelected event with the ID of the 
		//	selected text as the event parameter.
	}
}

long CUCLIDMCRTextViewerCtrl::isEntityIDValid(unsigned long ulTextEntityID) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	bool	bResult = false;

	try
	{
		// Validate entity ID
		if ((ulTextEntityID >= 0) && (ulTextEntityID < 
			m_mapInputFinderToTokenPositions[m_ipFinder].size()))
		{
			bResult = true;
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 2760, 
			"ELI02760 : Failure in isEntityValid()" );
	}

	return bResult ? 1L : 0L;
}

BSTR CUCLIDMCRTextViewerCtrl::getEntityText(long ulTextEntityID) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CString strResult;
	
	try
	{
		// Validate entity ID
		if (isEntityIDValid(ulTextEntityID))
		{
			// Select the text between the start and end positions 
			m_pMCRView->GetRichEditCtrl().SetSel(
				getCurrentTokenPositions().at(ulTextEntityID).m_lStartPos, 
				getCurrentTokenPositions().at(ulTextEntityID).m_lEndPos + 1);
			
			// Get the selected text into strResult
			strResult = m_pMCRView->GetRichEditCtrl().GetSelText();
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1453, "ELI01453 : Failure in getEntityText()" );
	}

	return strResult.AllocSysString();
}

BSTR CUCLIDMCRTextViewerCtrl::getText() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CString zErr;
	
	try
	{
		// Get the text
		CString	zText;
		m_pMCRView->GetRichEditCtrl().GetWindowText( zText );
		return zText.AllocSysString();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 2808, "ELI02808 : Failure in getText()" );
	}

	// Return empty string as error
	return zErr.AllocSysString();
}

void CUCLIDMCRTextViewerCtrl::setTextHighlightColor(long ulTextEntityID, 
													OLE_COLOR newTextHighlightColor) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		long lStPoint = 0;
		long lEndPoint = 0;

		// Store current modified state
		BOOL bTempModify = m_pMCRView->GetRichEditCtrl().GetModify();

		// Get cursor position
		m_pMCRView->GetRichEditCtrl().GetSel( lStPoint, lEndPoint );
		
		// Validate entity ID
		if (isEntityIDValid(ulTextEntityID))
		{
			// Select the desired text
			m_pMCRView->GetRichEditCtrl().SetSel(
				getCurrentTokenPositions().at(ulTextEntityID).m_lStartPos,
				getCurrentTokenPositions().at(ulTextEntityID).m_lEndPos + 1);

			// Prepare the background color of the selected MCR'able text
			CHARFORMAT2 cf2;
			
			// Set the size equal to size of CHARFORMAT2 as background color is required
			cf2.cbSize = sizeof(CHARFORMAT2);

			// Set the masking flags to validate color and background color	
			cf2.dwMask = CFM_COLOR | CFM_BACKCOLOR | CFM_PROTECTED;
			cf2.dwEffects = CFE_ALLCAPS | CFE_PROTECTED | CFE_EMBOSS;

			// Set text color as Black
			cf2.crTextColor = RGB(0, 0, 0);

			// Set the background color to the newTextHighlightColor 
			cf2.crBackColor = newTextHighlightColor;

			// Set the background color of the selected text
			::SendMessage( m_pMCRView->m_hWnd, EM_SETCHARFORMAT, SCF_SELECTION, (LPARAM)&cf2 );

			// Set cursor to the previous position
			m_pMCRView->GetRichEditCtrl().SetSel( lStPoint, lEndPoint );

			// Replace control modify state
			m_pMCRView->GetRichEditCtrl().SetModify( bTempModify );
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1452, "ELI01452 : Failure in setTextHighlightColor()" );
	}
}

OLE_COLOR CUCLIDMCRTextViewerCtrl::getEntityColor(long ulTextEntityID) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	OLE_COLOR	color( RGB(255, 255, 0) );

	try
	{
		// Validate entity ID
		if (isEntityIDValid(ulTextEntityID))
		{
			// Select the text between the start and end positions 
			m_pMCRView->GetRichEditCtrl().SetSel(
				getCurrentTokenPositions().at(ulTextEntityID).m_lStartPos,
				getCurrentTokenPositions().at(ulTextEntityID).m_lEndPos + 1);
			
			// Prepare a CHARFORMAT structure for retrieving the background color 
			// of the selected text
			CHARFORMAT2 cf2;

			// Set the size equal to size of CHARFORMAT2 as background color is required
			cf2.cbSize = sizeof(CHARFORMAT2);

			// Set the masking flags to validate color and background color	
			cf2.dwMask = CFM_COLOR | CFM_BACKCOLOR | CFM_PROTECTED;
			cf2.dwEffects = CFE_ALLCAPS | CFE_PROTECTED | CFE_EMBOSS;

			// Retrieve the background color of the selected text
			::SendMessage( m_pMCRView->m_hWnd, EM_GETCHARFORMAT, SCF_SELECTION, (LPARAM)&cf2 );

			// Return color in OLE_COLOR format
			color = cf2.crBackColor;
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 2788, "ELI02788 : Failure in getEntityColor()" );
	}

	// Return retrieved color or default
	return color;
}

void CUCLIDMCRTextViewerCtrl::setEntityColor(long ulTextEntityID, OLE_COLOR newTextColor) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		long lStPoint = 0;
		long lEndPoint = 0;

		// Store current modified state
		BOOL bTempModify = m_pMCRView->GetRichEditCtrl().GetModify();

		// Get cursor position
		m_pMCRView->GetRichEditCtrl().GetSel( lStPoint, lEndPoint );
		
		if (isEntityIDValid(ulTextEntityID))
		{
			// Select the desired text
			m_pMCRView->GetRichEditCtrl().SetSel(
				getCurrentTokenPositions().at(ulTextEntityID).m_lStartPos,
				getCurrentTokenPositions().at(ulTextEntityID).m_lEndPos + 1);

			// Prepare the background color of the selected MCR'able text
			CHARFORMAT2 cf2;
			
			// Set the size equal to size of CHARFORMAT2
			cf2.cbSize = sizeof(CHARFORMAT2);

			// Set the masking flags to validate color
			cf2.dwMask = CFM_COLOR | CFM_PROTECTED;
			cf2.dwEffects = CFE_ALLCAPS | CFE_PROTECTED | CFE_EMBOSS;

			// Set text color to the new color
			cf2.crTextColor = newTextColor;

			// Set the color of the selected text
			::SendMessage( m_pMCRView->m_hWnd, EM_SETCHARFORMAT, SCF_SELECTION, (LPARAM)&cf2 );

			// Set cursor to the previous position
			m_pMCRView->GetRichEditCtrl().SetSel( lStPoint, lEndPoint );

			// Replace control modify state
			m_pMCRView->GetRichEditCtrl().SetModify( bTempModify );
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 2794, "ELI02794 : Failure in setEntityColor()" );
	}
}

void CUCLIDMCRTextViewerCtrl::setEntityText(long ulTextEntityID, LPCTSTR strNewText) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CString strResult;

	long lStIndex = 0;
	long lEndIndex = 0;

	try
	{
		// Get pointer position
		m_pMCRView->GetRichEditCtrl().GetSel( lStIndex, lEndIndex );

		// ulTextEntityID must have been obtained as the parameter to 
		// a fired "TextSelected" event
		if (isEntityIDValid(ulTextEntityID))
		{
			// Select the text between the start and end positions 
			m_pMCRView->GetRichEditCtrl().SetSel(
				getCurrentTokenPositions().at(ulTextEntityID).m_lStartPos, 
				getCurrentTokenPositions().at(ulTextEntityID).m_lEndPos + 1);

			// Get the selected text into strResult
			strResult = m_pMCRView->GetRichEditCtrl().GetSelText();
			
			// Get the difference between the old string and the new string
			int iOldStrLen = strlen (strResult);
			int iNewStrLen = strlen (strNewText);
			int iStrLenDiff = iNewStrLen - iOldStrLen;
			
			// Replace the selection with the new text. 
			// It will automatically set the background 
			m_pMCRView->GetRichEditCtrl().ReplaceSel( strNewText );

			// Update the rest of start and end positions for MCR texts
			getCurrentTokenPositions()[ulTextEntityID].m_lEndPos += iStrLenDiff;

			long lMCRCount = getCurrentTokenPositions().size();
			if (ulTextEntityID < lMCRCount)
			{
				for (int i = ulTextEntityID + 1; i < lMCRCount; i++)
				{
					getCurrentTokenPositions()[i].m_lStartPos += iStrLenDiff;
					getCurrentTokenPositions()[i].m_lEndPos += iStrLenDiff;
				}
			}

			////////////////////////////////////////////////////////////////
			// Remove other InputFinder entries from the map as their
			// associated start/end positions may no longer be valid because 
			// of the text change.
			////////////////////////////////////////////////////////////////

			// Store current vector
			std::vector<InputTokenPosition>	vecCurrent = getCurrentTokenPositions();

			// Remove all map entries
			m_mapInputFinderToTokenPositions.clear();

			// Replace current vector
			m_mapInputFinderToTokenPositions[m_ipFinder] = vecCurrent;
		}

		// Set pointer position
		m_pMCRView->GetRichEditCtrl().SetSel( lEndIndex, lEndIndex );
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1450, "ELI01450 : Failure in setEntityText()" );
	}
}

void CUCLIDMCRTextViewerCtrl::pasteTextFromClipboard() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// clear out the input text positions map
		m_mapInputFinderToTokenPositions.clear();

		// Set selection to entire text
		m_pMCRView->GetRichEditCtrl().SetSel( 0, -1 );

		// Set options to allow additional text
		m_pMCRView->GetRichEditCtrl().SetReadOnly( FALSE );

		// Paste from the clipboard
		m_pMCRView->GetRichEditCtrl().PasteSpecial( CF_TEXT );	

		// Reset read-only option
		m_pMCRView->GetRichEditCtrl().SetReadOnly( TRUE );

		// Parse and highlight the text
		parseText();

		// Set modified flag
		m_pMCRView->GetRichEditCtrl().SetModify( TRUE );	
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1445, "ELI01445 : Failure in pasteTextFromClipboard()" );
	}
}

void CUCLIDMCRTextViewerCtrl::copyTextToClipboard() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// Per the new UCLID specification, the entire text in 
	// the control should get copied into the clipboard.
	try
	{
		// Select all the text
		m_pMCRView->GetRichEditCtrl().SetSel( 0, -1 );

		// Copy selected text
		m_pMCRView->GetRichEditCtrl().Copy();	

		// Remove the selection 
		m_pMCRView->GetRichEditCtrl().SetSel( 0, 0 );
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1444, "ELI01444 : Failure in copyTextToClipboard()" );
	}
}

void CUCLIDMCRTextViewerCtrl::setTextFontName(LPCTSTR strFontName) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Store font name and apply the change
		m_strFontName = strFontName;
		resetTextFontAndSize();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1443, "ELI01443 : Failure in setTextFontName()" );
	}
}

BSTR CUCLIDMCRTextViewerCtrl::getTextFontName() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CString	zErr;

	try
	{
		CString zFontName( m_strFontName.c_str() );
		return zFontName.AllocSysString();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1442, "ELI01442 : Failure in getTextFontName()" );
	}

	// Return empty string as error
	return zErr.AllocSysString();
}

void CUCLIDMCRTextViewerCtrl::setTextSize(long ulTextSize) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Store font size and apply the change
		m_iTextSize = ulTextSize;
		resetTextFontAndSize();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1441, "ELI01441 : Failure in setTextSize()" );
	}
}

long CUCLIDMCRTextViewerCtrl::getTextSize() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_iTextSize;
}

void CUCLIDMCRTextViewerCtrl::increaseTextSize() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get cursor position
		long	lStartChar = 0;
		long	lEndChar = 0;
		m_pMCRView->GetRichEditCtrl().GetSel( lStartChar, lEndChar );

		// Store current modified state
		BOOL bTempModify = m_pMCRView->GetRichEditCtrl().GetModify();

		// Prepare new format structure
		CHARFORMAT2 cfIncrease;
		cfIncrease.cbSize = sizeof(CHARFORMAT2);	

		// Get the default CHARFORMAT
		m_pMCRView->GetRichEditCtrl().GetDefaultCharFormat( cfIncrease );

		// Get the current size
		int iCharHeight = cfIncrease.yHeight;

		// Convert from TWIPS
		iCharHeight /= 20;

		// Get the next character size
		int i = 0;
		for (i = 0; i < 17; i++)
		{
			if ((i < 18) && (iCharHeight == iFontSizes[i]))
			{
				iCharHeight = iFontSizes[i + 1];
				break;
			}
		}

		if (i > 16)
		{
			return;
		}

		// Set Heights
		setTextSize( iCharHeight );

		// Convert the char size to TWIPS and assign
		cfIncrease.yHeight = iCharHeight * 20;

		// Set Mask 
		cfIncrease.dwMask = CFM_SIZE;

		// Provide updated CHARFORMAT to the control
		m_pMCRView->GetRichEditCtrl().SetDefaultCharFormat( cfIncrease );

		// Hide the selection
		m_pMCRView->GetRichEditCtrl().HideSelection( TRUE, FALSE );

		// Select all the text in the control
		m_pMCRView->GetRichEditCtrl().SetSel( 0, -1 );

		// Apply the changed CHARFORMAT to entire text
		m_pMCRView->GetRichEditCtrl().SetSelectionCharFormat( cfIncrease );

		// Deselect all the text
		m_pMCRView->GetRichEditCtrl().SetSel( lStartChar, lEndChar );

		// Remove hide selection
		m_pMCRView->GetRichEditCtrl().HideSelection( FALSE, FALSE );

		// Replace modify status
		m_pMCRView->GetRichEditCtrl().SetModify( bTempModify );
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1440, "ELI01440 : Failure in increaseTextSize()" );
	}
}

void CUCLIDMCRTextViewerCtrl::decreaseTextSize() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get cursor postion
		long	lStartChar = 0;
		long	lEndChar = 0;
		m_pMCRView->GetRichEditCtrl().GetSel( lStartChar, lEndChar );

		// Store current modified state
		BOOL bTempModify = m_pMCRView->GetRichEditCtrl().GetModify();

		CHARFORMAT2 cfDecrease;
		cfDecrease.cbSize = sizeof(CHARFORMAT2);	

		// Get the default CHARFORMAT
		m_pMCRView->GetRichEditCtrl().GetDefaultCharFormat( cfDecrease );

		// Get the current size
		int iCharHeight = cfDecrease.yHeight;

		// Convert from TWIPS
		iCharHeight /= 20;

		// Get the next character size
		int i;
		for (i = 0; i < 18; i++)
		{
			if ((i > 0) && (iCharHeight == iFontSizes[i]))
			{
				iCharHeight = iFontSizes[i-1];
				break;
			}
		}

		if (i > 17)
		{
			return;
		}

		// Set Heights
		setTextSize( iCharHeight );

		// Convert the char size to TWIPS and assign
		cfDecrease.yHeight = iCharHeight * 20;

		// Set Mask 
		cfDecrease.dwMask = CFM_SIZE;

		// Provide updated CHARFORMAT to the control
		m_pMCRView->GetRichEditCtrl().SetDefaultCharFormat( cfDecrease );

		// Hide the selection
		m_pMCRView->GetRichEditCtrl().HideSelection( TRUE, FALSE );

		// Select all the text in the control
		m_pMCRView->GetRichEditCtrl().SetSel( 0, -1 );

		// Apply the changed CHARFORMAT to entire text
		m_pMCRView->GetRichEditCtrl().SetSelectionCharFormat( cfDecrease );

		// Deselect all the text
		m_pMCRView->GetRichEditCtrl().SetSel( lStartChar, lEndChar );

		// Remove hide selection
		m_pMCRView->GetRichEditCtrl().HideSelection( FALSE, FALSE );

		// Restore modify value
		m_pMCRView->GetRichEditCtrl().SetModify( bTempModify );
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1439, "ELI01439 : Failure in decreaseTextSize()" );
	}
}

void CUCLIDMCRTextViewerCtrl::appendTextFromClipboard() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// clear out the input text positions map
		m_mapInputFinderToTokenPositions.clear();

		// Determine text length
		long	lLength = m_pMCRView->GetTextLength();

		// Set selection to end
		m_pMCRView->GetRichEditCtrl().SetSel( lLength, lLength );

		// Set options to allow additional text
		m_pMCRView->GetRichEditCtrl().SetReadOnly( FALSE );

		// Paste from the clipboard
		m_pMCRView->GetRichEditCtrl().PasteSpecial( CF_TEXT );	

		// Reset read-only option
		m_pMCRView->GetRichEditCtrl().SetReadOnly( TRUE );

		// Parse and highlight the text
		parseText();

		// Set modified flag
		m_pMCRView->GetRichEditCtrl().SetModify( TRUE );	
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1438, "ELI01438 : Failure in appendTextFromClipboard()" );
	}
}

void CUCLIDMCRTextViewerCtrl::setText(LPCTSTR strNewText) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Clear any existing start and end positions
		m_mapInputFinderToTokenPositions.clear();

		// Select entire text
		m_pMCRView->GetRichEditCtrl().SetSel( 0, -1 );

		// Replace the selection with the new text. It will automatically set the background 
		m_pMCRView->GetRichEditCtrl().ReplaceSel( strNewText );

		// Update the rest of start and end positions
		parseText();

		// Set modified flag
		m_pMCRView->GetRichEditCtrl().SetModify( TRUE );	
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1449, "ELI01449 : Failure in setText()" );
	}
}

void CUCLIDMCRTextViewerCtrl::appendText(LPCTSTR strNewText) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Clear any existing start and end positions
		m_mapInputFinderToTokenPositions.clear();

		// Determine text length
		long	iLength = m_pMCRView->GetRichEditCtrl().GetTextLength();

		// Move selection point to end of text
		m_pMCRView->GetRichEditCtrl().SetSel( iLength, iLength );

		// Add the new text. It will automatically set the background 
		m_pMCRView->GetRichEditCtrl().SetReadOnly( FALSE );
		m_pMCRView->GetRichEditCtrl().ReplaceSel( strNewText );
		m_pMCRView->GetRichEditCtrl().SetReadOnly( TRUE );

		// Update the rest of start and end positions
		parseText();

		// Set modified flag
		m_pMCRView->GetRichEditCtrl().SetModify( TRUE );	
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1448, "ELI01448 : Failure in appendText()" );
	}
}

long CUCLIDMCRTextViewerCtrl::isModified() 
{
	// Check modified flag from the Rich Edit Control
	return static_cast<long>(m_pMCRView->GetRichEditCtrl().GetModify());
}

void CUCLIDMCRTextViewerCtrl::print() 
{
	long	lFormatted = 0; 
	int		iPage = 1;
	int		ipageFrom = 0; 
	int		ipageTo = 0; 
	int		iLen = 0;
	int		iCount = 0; 
	int		iTemp = 0;

	try
	{
		// Structure for defining a printer page 
		FORMATRANGE FormatRange; 

		// Create a CDC object for the device context we got 
		CDC dcPrinter; 

		// Use a CPrintInfo to get the default printer information
		CPrintInfo printdlg;

		// Retrieve the length of the text in the rich edit view
		iLen = m_pMCRView->GetRichEditCtrl().GetTextLength();
		
		// Display the dialog box and allow the user to make a selection
		if (printdlg.m_pPD->DoModal() == IDOK)
		{
			// Attach a Windows device context to CDC object
			dcPrinter.Attach( printdlg.m_pPD->m_pd.hDC );

			// Save old mode to restore 
			int iOldMapMode = dcPrinter.GetMapMode(); 

			// Map to twips for rich edit control's measurement
			dcPrinter.SetMapMode( MM_TWIPS );

			// Measure the printable page 
			int iOffsetX = dcPrinter.GetDeviceCaps( PHYSICALOFFSETX ); 
			int iOffsetY = -dcPrinter.GetDeviceCaps( PHYSICALOFFSETY ); 

			int iHorzRes = dcPrinter.GetDeviceCaps( HORZRES ); 
			int inVertRes = -dcPrinter.GetDeviceCaps( VERTRES );	

			// Create a rect for the whole printable page 
			CRect rect( iOffsetX, iOffsetY, iHorzRes, inVertRes );
			
			// Convert device units into logical units.
			dcPrinter.DPtoLP( &rect ); 

			// Reset mapping mode or nothing prints
			dcPrinter.SetMapMode( iOldMapMode ); 

			// Describe print job to print spooler 
			DOCINFO docInfo; 
			docInfo.cbSize = sizeof(docInfo); 
			docInfo.fwType = 0;
		
			if (getCurFilePath().IsEmpty())
			{
				docInfo.lpszDocName = "Untitled.txt";
			}
			else
			{
				//	Any string you like here for document name
				docInfo.lpszDocName = getCurFilePath().operator LPCTSTR();
			}

			docInfo.lpszOutput = NULL; 
			docInfo.lpszDatatype = NULL;		

			// Set formatting parameters for the rich edit control 
			FormatRange.hdc = dcPrinter.GetSafeHdc(); 
			FormatRange.hdcTarget = dcPrinter.GetSafeHdc(); 
			FormatRange.rc = rect; 
			FormatRange.rcPage = rect;
			
			// Print all pages in the document
			if (printdlg.m_pPD->PrintAll())       
			{
				ipageFrom = printdlg.m_pPD->m_pd.nMinPage;
				ipageTo = printdlg.m_pPD->m_pd.nMaxPage;

				iCount = 0;
				iTemp = 0;
				lFormatted = 0;

				// Start a document 
				if (!dcPrinter.StartDoc(&docInfo))
				{ 
					// Provide error message
					AfxThrowOleDispatchException(0, "ELI03365, Failed to print the document.");
				}		
				
				// Print until all text has been processed 
				while(iLen > 0)
				{
					if(!dcPrinter.StartPage())
					{
						AfxThrowOleDispatchException(0, "ELI03366, Error starting new page during print.");
					}				

					// Start from where last page ended 
					FormatRange.chrg.cpMin = lFormatted; 

					// Try to format all remaining text
					FormatRange.chrg.cpMax = -1;				

					// Create the image to be printed 
					lFormatted = m_pMCRView->GetRichEditCtrl().FormatRange( 
						&FormatRange, TRUE ); 

					iCount = lFormatted - iTemp;
					if(iCount <= 0)
					{
						break;
					}

					iLen -= iCount;
					iTemp += iCount;

					// Print the page
					m_pMCRView->GetRichEditCtrl().DisplayBand( &rect );

					// Close the page 
					dcPrinter.EndPage();				

					// Trap error with formatting
					if (lFormatted < 0)
					{
						break;
					}
				}

				// Free cached information about target printer 
				m_pMCRView->GetRichEditCtrl().FormatRange( NULL, TRUE ); 

				// Close the document. Printing actually starts now 
				dcPrinter.EndDoc();
			}
			// Print only a range of pages in the document
			else if (printdlg.m_pPD->PrintRange())   
			{
				// Retrieve the FromPage from the Dialogbox  
				ipageFrom = printdlg.m_pPD->GetFromPage();

				// Retrieve the ToPage from the Dialogbox
				ipageTo = printdlg.m_pPD->GetToPage();

				iCount = 0;
				iTemp = 0;
				lFormatted = 0;

				// Start a document 
				if (!dcPrinter.StartDoc(&docInfo))
				{ 
					AfxThrowOleDispatchException(0, "ELI03367, Could not print the document.");
				} 

				// Print the pages one at a time in this loop
				do 
				{
					// Start from where last page ended 
					FormatRange.chrg.cpMin = lFormatted; 

					// Try to format all remaining text
					FormatRange.chrg.cpMax = -1; 

					if (ipageTo < iPage)
					{
						break;
					}

					// Create the image to be printed 
					lFormatted = m_pMCRView->GetRichEditCtrl().FormatRange( 
						&FormatRange, TRUE ); 

					iCount = lFormatted - iTemp;

					if ((iCount <= 0) || (ipageTo < iPage))
					{
						break;
					}

					iLen -= iCount;
					iTemp += iCount;

					if (iPage >= ipageFrom)
					{
						// Inform the device driver that a new page is starting
						if (!dcPrinter.StartPage())
						{
							AfxThrowOleDispatchException(0, "ELI03368, Error starting new page during print.");
						}
					
						if ((ipageFrom <= iPage) && (ipageTo >= iPage))
						{
							//	Print the page
							m_pMCRView->GetRichEditCtrl().DisplayBand( &rect );
						}
						
						// Close the page 
						dcPrinter.EndPage();					
					}

					// Update the Page Count in the Document
					iPage++; 

					// Trap error with formatting	
					if (lFormatted < 0)
					{
						break; 
					}
				}
				// Print until all text has been processed 
				while (iLen > 0); 

				// Free cached information about target printer 
				m_pMCRView->GetRichEditCtrl().FormatRange( NULL, TRUE );

				// Close the document. Printing actually starts now
				dcPrinter.EndDoc();
			}
		}			// Main if loop end
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1437, "ELI01437 : Failure in print()" );
	}
}

BSTR CUCLIDMCRTextViewerCtrl::getFileName() 
{
	CString	zErr;

	try
	{
		// Provide data member - otherwise an empty string
		if (getCurFilePath().IsEmpty())
		{
			CString	zTemp;
			return zTemp.AllocSysString();
		}
		else
		{
			return getCurFilePath().AllocSysString();
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1436, "ELI01436 : Failure in getFileName()" );
	}

	// Return empty string as error
	return zErr.AllocSysString();
}

void CUCLIDMCRTextViewerCtrl::resetTextFontAndSize() 
{
	try
	{
		// Store current modified state
		BOOL bTempModify = m_pMCRView->GetRichEditCtrl().GetModify();

		// Get the font name 
		CString cstrFontName( getTextFontName() );

		int	iFontSize = getTextSize();

		CHARFORMAT2 cfFont;

		// Set the size for CHARFORMAT2
		cfFont.cbSize = sizeof(CHARFORMAT2);	

		// Get the default CHARFORMAT
		m_pMCRView->GetRichEditCtrl().GetDefaultCharFormat( cfFont );

		// Font name must not be empty
		if (!cstrFontName.IsEmpty())
		{
			// Set the mask value
			cfFont.dwMask = CFM_SIZE | CFM_FACE | CFM_CHARSET;
			
			// Set the Font face name
			lstrcpynA( cfFont.szFaceName, cstrFontName, LF_FACESIZE );
			cfFont.yHeight = iFontSize*20;

			// Set the changed CHARFORMAT
			m_pMCRView->GetRichEditCtrl().SetDefaultCharFormat( cfFont );
			
			// Hide selection
			m_pMCRView->GetRichEditCtrl().HideSelection( TRUE, FALSE );

			// Select all the text in the control
			m_pMCRView->GetRichEditCtrl().SetSel( 0, -1 );

			// Set the changed CHARFORMAT to the selected text
			m_pMCRView->GetRichEditCtrl().SetSelectionCharFormat( cfFont );

			// Deselect all the text
			m_pMCRView->GetRichEditCtrl().SetSel( 0, 0 );

			// Remove hide selection state
			m_pMCRView->GetRichEditCtrl().HideSelection( FALSE, FALSE );

			// Replace control modify state
			m_pMCRView->GetRichEditCtrl().SetModify( bTempModify );
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 1451, "ELI01451 : Failure in resetTextFontAndSize()" );
	}
}

void CUCLIDMCRTextViewerCtrl::setInputFinder(LPUNKNOWN pInputFinder) 
{
	// Replace outdated Finder
	m_ipFinder = pInputFinder;

	// Parse the current text using the new Finder
	parseText();
}

void CUCLIDMCRTextViewerCtrl::parseText() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// Set the rich edit control to read only
	m_pMCRView->GetRichEditCtrl().SetReadOnly();

	// Store current modified state
	BOOL bTempModify = m_pMCRView->GetRichEditCtrl().GetModify();

	try
	{
		// Present a Wait cursor
		CWaitCursor	wait;

		// Retrieve text from control
		CString zMCRText;
		m_pMCRView->GetRichEditCtrl().GetWindowText( zMCRText );

		// Check for empty string
		if (zMCRText.IsEmpty())
		{
			return;
		}

		if (m_mapInputFinderToTokenPositions.find(m_ipFinder) == m_mapInputFinderToTokenPositions.end())
		{
			// Delete all carriage return characters
			zMCRText.Remove('\r');	

			// Convert to BSTR
			_bstr_t	bstrText("");
			try
			{
				// Since BSTR is a length-prefixed string (wide, 
				// double-byte (Unicode) strings), if zMCRText
				// is too big, the stack will overflow.
				double dSizeBSTR = pow(2.0, 16.0);
				int nFileSize = zMCRText.GetLength();
				if (dSizeBSTR > nFileSize)
				{
					bstrText = zMCRText;
				}
				else
				{
					MessageBox("Text will not be highlighted because the file size exceeds 65K.", "Highlight Text");
					bstrText = "";
				}
			}
			catch (...)
			{
				MessageBox("Text will not be highlighted because the file size exceeds 65K.", "Highlight Text");
				bstrText = "";
			}

			// Use the current Finder and determine token positions
			IIUnknownVectorPtr ipTokenPositions( m_ipFinder->ParseString( bstrText ) );

			// Decompose collection of tokens into vectors 
			// of start and end positions
			ITokenPtr	ipPosition;
			for (int i = 0; i < ipTokenPositions->Size(); i++)
			{
				// Get this token position object
				long	lStart = 0;
				long	lEnd = 0;
				ipPosition = ipTokenPositions->At( i );

				// Retrieve start and end points - don't care about BSTR's
				ipPosition->GetTokenInfo( &lStart, &lEnd, NULL, NULL );

				// It was decided in an engineering meeting on 1-11-2002 that any MCR text
				// such as bearings, distances, and angles should be a minimum of two 
				// characters long so that unnecessary MCR-like noise can be eliminated
				// if the length of the MCR-text to be added is not a minimum of two
				// characters, then do not add it to the vector
				if (lEnd == lStart)
				{
					continue;
				}

				// Add positions to vectors
				InputTokenPosition tokenPosition;
				tokenPosition.m_lStartPos = lStart;
				tokenPosition.m_lEndPos = lEnd;
				getCurrentTokenPositions().push_back( tokenPosition );
			}

			bstrText = "";
		}

		// Remove any current background coloration
		clearTextBackground();
		m_pMCRView->ShowWindow(SW_HIDE);
		for (unsigned int ui = 0; ui < getCurrentTokenPositions().size(); ui++)
		{
			long lStart = getCurrentTokenPositions()[ui].m_lStartPos;
			long lEnd = getCurrentTokenPositions()[ui].m_lEndPos;

			// Select the text between the start and end positions 
			m_pMCRView->GetRichEditCtrl().SetSel( lStart, lEnd + 1 );

			// Prepare the background color of the selected MCR'able text
			CHARFORMAT2 cf2;
			
			// Set the size equal to size of CHARFORMAT2 as 
			// background color is required
			cf2.cbSize = sizeof(CHARFORMAT2);

			// Set the masking flags to validate color and background color	
			cf2.dwMask = CFM_COLOR | CFM_BACKCOLOR | CFM_PROTECTED;
			cf2.dwEffects = CFE_ALLCAPS | CFE_PROTECTED | CFE_EMBOSS;

			cf2.crTextColor = RGB( 0, 0, 0 );
			cf2.crBackColor = m_MCRBkColor;

			// Set the background color of the selected text
			::SendMessage( m_pMCRView->m_hWnd, EM_SETCHARFORMAT, SCF_SELECTION, 
				(LPARAM)&cf2 );

			// Set the selection to TRUE
			m_pMCRView->GetRichEditCtrl().HideSelection( FALSE, TRUE );
		}

		m_pMCRView->ShowWindow(SW_SHOW);

		// Remove the selection 
		m_pMCRView->GetRichEditCtrl().SetSel( 0, 0 );

		// Replace control modify state
		m_pMCRView->GetRichEditCtrl().SetModify( bTempModify );
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 2778, "ELI02778: Failure in parseText()" );
	}
}

void CUCLIDMCRTextViewerCtrl::clearTextBackground() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// Set the rich edit control to read only
	m_pMCRView->GetRichEditCtrl().SetReadOnly();

	// Store current modified state
	BOOL bTempModify = m_pMCRView->GetRichEditCtrl().GetModify();

	try
	{
		// Retrieve text from control
		CString zMCRText;
		m_pMCRView->GetRichEditCtrl().GetWindowText( zMCRText );

		// Check for empty string
		if (zMCRText.IsEmpty())
		{
			return;
		}

		// Select all of the text
		m_pMCRView->GetRichEditCtrl().SetSel( 0, -1 );

		// Prepare the background color of the selected MCR'able text
		CHARFORMAT2 cf2;
		
		// Set the size equal to size of CHARFORMAT2 as 
		// background color is required
		cf2.cbSize = sizeof(CHARFORMAT2);

		// Set the masking flags to validate color and background color	
		cf2.dwMask = CFM_COLOR | CFM_BACKCOLOR | CFM_PROTECTED;
		cf2.dwEffects = CFE_ALLCAPS | CFE_PROTECTED | CFE_EMBOSS;

		// Black text
		cf2.crTextColor = RGB( 0, 0, 0 );

		// Use current system color as background
		DWORD	dwColor = GetSysColor( COLOR_WINDOW );
		cf2.crBackColor = dwColor;

		// Set the background color of the selected text
		::SendMessage( m_pMCRView->m_hWnd, EM_SETCHARFORMAT, SCF_SELECTION, 
			(LPARAM)&cf2 );

		// Set the selection to TRUE
		m_pMCRView->GetRichEditCtrl().HideSelection( FALSE, TRUE );

		// Remove the selection 
		m_pMCRView->GetRichEditCtrl().SetSel( 0, 0 );

		// Replace control modify state
		m_pMCRView->GetRichEditCtrl().SetModify( bTempModify );
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException( 2790, "ELI02790: Failure in clearTextBackground()" );
	}
}

BSTR CUCLIDMCRTextViewerCtrl::getSelectedText() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CString zResult("");
	
	try
	{
		// get current selected text
		zResult = m_pMCRView->GetRichEditCtrl().GetSelText();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();
		AfxThrowOleDispatchException(0, str);
	}
	catch(...)
	{
		AfxThrowOleDispatchException(4883, "ELI04883: Unknown exception in getSelectedText()");
	}

	return zResult.AllocSysString();
}
