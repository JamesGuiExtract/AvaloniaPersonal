#include "stdafx.h"
#include "resource.h"
#include "RecognizeTextInPolygonDragOperation.h"

#include <UCLIDException.h>
#include <TemporaryFileName.h>
#include <cpputil.h>
#include <mathUtil.h>
#include <TemporaryResourceOverride.h>
#include <ExtractZoneAsImage.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


using namespace std;

//--------------------------------------------------------------------------------------------------
RecognizeTextInPolygonDragOperation::RecognizeTextInPolygonDragOperation(
					CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl, 
					SpotRecognitionDlg* pSpotRecDlg, 
					ETool ePrevTool)
:DragOperation(rUCLIDGenericDisplayCtrl), 
 m_bCreatingInProcess(false),
 m_pSpotRecDlg(pSpotRecDlg),
 m_ePreviousTool(ePrevTool)
{
	try
	{
		if (!CreateEx(NULL, AfxRegisterWndClass(NULL), "", NULL, 0, 0, 0, 0, NULL, NULL))
		{
			throw UCLIDException("ELI04864", "Unable to create window!");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04863")
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(RecognizeTextInPolygonDragOperation, CWnd)
	//{{AFX_MSG_MAP(RecognizeTextInPolygonDragOperation)
	ON_COMMAND(ID_MNU_FINISH_POLYGON, OnMnuFinish)
	ON_COMMAND(ID_MNU_CANCEL_POLYGON, OnMnuCancel)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
RecognizeTextInPolygonDragOperation::~RecognizeTextInPolygonDragOperation()
{
	try
	{
		// make the rubberband go away
		m_UCLIDGenericDisplayCtrl.enableRubberbanding(FALSE);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16535");
}
//--------------------------------------------------------------------------------------------------
void RecognizeTextInPolygonDragOperation::onMouseDown(short Button, short Shift, long x, long y)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// left mouse button is down
		if (Button == MK_LBUTTON)
		{
			// store the point as last clicked point
			m_nLastPointX = x;
			m_nLastPointY = y;

			double dX, dY;
			// get the starting point in world coordinates
			m_UCLIDGenericDisplayCtrl.convertClientWindowPixelToWorldCoords(
				x, y, &dX, &dY, m_UCLIDGenericDisplayCtrl.getCurrentPageNumber());
			if (m_vecPolygonVertices.empty())
			{
				// set rubberband to polygon
				m_UCLIDGenericDisplayCtrl.setRubberbandingParameters(3, dX, dY);
				m_UCLIDGenericDisplayCtrl.enableZoneEntityCreation(FALSE);
				m_UCLIDGenericDisplayCtrl.enableRubberbanding(TRUE);
			}
			
			// convert from world to image pixels for getting the region in the image
			m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(dX, dY, &x, &y, m_UCLIDGenericDisplayCtrl.getCurrentPageNumber());
			POINT point;
			point.x = x;
			point.y = y;
			// store the point
			m_vecPolygonVertices.push_back(point);

			m_bCreatingInProcess = true;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04833");
}
//--------------------------------------------------------------------------------------------------
void RecognizeTextInPolygonDragOperation::onMouseUp(short Button, short Shift, long x, long y)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		if (Button == MK_RBUTTON)
		{
			if (!m_vecPolygonVertices.empty())
			{
				// Show its own context menu
				CMenu menu;
				menu.LoadMenu(IDR_MENU_POLYGON_CONTEXT);
				CMenu *pContextMenu = menu.GetSubMenu(0);				

				// show context menu at current mouse position
				POINT p;
				GetCursorPos(&p);
				pContextMenu->TrackPopupMenu(TPM_LEFTALIGN|TPM_LEFTBUTTON|TPM_VERTICAL, p.x, p.y, this);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04838");
}
//--------------------------------------------------------------------------------------------
bool RecognizeTextInPolygonDragOperation::autoRepeat()
{
	// want to stay in current tool
	return true;
}
//--------------------------------------------------------------------------------------------------

///////////////////
// Message handlers
////////////////////
//--------------------------------------------------------------------------------------------------
void RecognizeTextInPolygonDragOperation::OnMnuFinish()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		try
		{
			recognizeTextInPolygon();
		}
		catch(UCLIDException& ue)
		{
			ue.display();
			// start over again
			m_UCLIDGenericDisplayCtrl.enableRubberbanding(FALSE);
			m_vecPolygonVertices.clear();
			return;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04818");
}
//--------------------------------------------------------------------------------------------------
void RecognizeTextInPolygonDragOperation::OnMnuCancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		if (!m_vecPolygonVertices.empty())
		{
			// make the rubberband go away
			m_UCLIDGenericDisplayCtrl.enableRubberbanding(FALSE);
			m_bCreatingInProcess = false;
			m_vecPolygonVertices.clear();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04819");
}
//--------------------------------------------------------------------------------------------------
void RecognizeTextInPolygonDragOperation::recognizeTextInPolygon()
{
	// make the rubberband go away
	m_UCLIDGenericDisplayCtrl.enableRubberbanding(FALSE);
	
	// finish current polygon and crop the image in-bound
	// get image file name
	string strImageFileName = (LPCTSTR)m_UCLIDGenericDisplayCtrl.getImageName();
	
	// now save the new bitmap to a temp file
	string strExtension(::getExtensionFromFullPath(strImageFileName));
	TemporaryFileName tempImgFile(true, NULL, strExtension.c_str());
	::extractPolygonAsImage(strImageFileName, m_vecPolygonVertices, tempImgFile.getName());
	
	// process the image for paragraph text as appropriate
	m_pSpotRecDlg->processImageForParagraphText(tempImgFile.getName(), 1, -1);
	
	// reset the flag
	m_bCreatingInProcess = false;

	if (m_pSpotRecDlg)
	{
		// set back to previous tool at spot rec dialog level
		m_pSpotRecDlg->setCurrentTool(m_ePreviousTool);
	}
}
//--------------------------------------------------------------------------------------------------
