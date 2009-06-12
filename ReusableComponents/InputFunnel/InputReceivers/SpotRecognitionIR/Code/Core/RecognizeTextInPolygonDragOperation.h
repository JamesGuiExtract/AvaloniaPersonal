//==================================================================================================
//
// COPYRIGHT (c) 2003 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RecognizeTextInPolygonDragOperation.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//==================================================================================================

#pragma once

#include "DragOperation.h"
#include "SpotRecognitionDlg.h"
#include <vector>

// IMPORTANT! CWnd must be inherited first.
class RecognizeTextInPolygonDragOperation : public CWnd,
											public DragOperation
{
public:

	RecognizeTextInPolygonDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl, 
									   SpotRecognitionDlg* pSpotRecDlg, 
									   ETool ePrevTool);

	~RecognizeTextInPolygonDragOperation();

	virtual bool autoRepeat();
	virtual void onMouseDown(short Button, short Shift, long x, long y);
	virtual void onMouseUp(short Button, short Shift, long x, long y);
	virtual bool isInProcess() {return m_bCreatingInProcess;}

protected:
	// Generated message map functions
	//{{AFX_MSG(RecognizeTextInPolygonDragOperation)
	afx_msg void OnMnuFinish();
	afx_msg void OnMnuCancel();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	SpotRecognitionDlg *m_pSpotRecDlg;

	// once this tool is done, the previous tool to go back
	ETool m_ePreviousTool;

	// internal stored vector of points as polygon vertices
	// POINT is a struct of two long integers
	std::vector<POINT> m_vecPolygonVertices;

	// whether or not creating the polygon is in progress
	bool m_bCreatingInProcess;

	// last point recorded when L-Mouse down event is called
	int m_nLastPointX, m_nLastPointY;

	//////////
	// Methods
	//////////
	void recognizeTextInPolygon();
};
