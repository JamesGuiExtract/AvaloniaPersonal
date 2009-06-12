//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SpotRecDlgToolBar.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#ifndef SPOT_REC_DLG_TOOLBAR_H
#define SPOT_REC_DLG_TOOLBAR_H

#include <string>

enum SpotRecToolBarImageIndex
{
	kSRBmpOpenImage = 0,
	kSRBmpSave,
	kSRBmpPrint,
	kSRBmpZoomWindow,
	kSRBmpZoomIn,
	kSRBmpZoomOut,
	kSRBmpZoomPrev,
	kSRBmpZoomNext,
	kSRBmpFitPage,
	kSRBmpFitWidth,
	kSRBmpPan,
	kSRBmpSelectHighlight,
	kSRBmpSelectText,
	kSRBmpSetHighlightHeight,
	kSRBmpEditZoneText,
	kSRBmpDeleteEntities,
	kSRBmpRecognizeTextAndProcess,
	kSRBmpOpenSubImage,
	kSRBmpRotateLeft,
	kSRBmpRotateRight,
	kSRBmpFirstPage,
	kSRBmpPreviousPage,
	kSRBmpGoToPage,
	kSRBmpNextPage,
	kSRBmpLastPage,
	kSRBmpSwipe,
	kSRBmpRect
};

class SpotRecDlgToolBar : public CToolBar
{
public:
	SpotRecDlgToolBar();
	virtual ~SpotRecDlgToolBar();
	void createGoToPageEditBox();
	void enableGoToEditBox(bool bEnable);
	std::string getCurrentGoToPageText();
	void setCurrentGoToPageText(const std::string& strText);
	void clearGoToPageText();
	void showToolbarCtrl(ESRIRToolbarCtrl eCtrl, bool bShow);
	void updateGotoEditBoxPos();

private:
	CEdit *m_wndSnap;

	int getButtonId(ESRIRToolbarCtrl eCtrl);
};

#endif // SPOT_REC_DLG_TOOLBAR_H