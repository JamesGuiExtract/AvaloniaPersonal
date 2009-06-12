
#pragma once

#include <FileProcessingConfigMgr.h>

#include <string>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// default folder column widths for columns common to multiple lists
static const int giDATE_COL_WIDTH = 45;
static const int giTIME_COL_WIDTH = 65;
static const int giFILENAME_COL_WIDTH = 100;
static const int giFOLDER_COL_WIDTH = 120;
static const int giEXCEPTION_COL_WIDTH = 180;
static const int giFILE_ID_COL_WIDTH = 65;
static const int giNUM_PAGES_COL_WIDTH = 60;

// Default Action name used by FPS File Converter if old FPS file did not use actions
static const std::string gstrCONVERTED_FPS_ACTION_NAME = "Converted____FPS____Action";

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------

// This function will resize the 3 list controls and their associated labels so that
// they each occupy equal space on the screen, where the vertical space among them is shared
// and all the horizontal space is equally used up.  This has been setup as a reusable function
// because there are two different property pages that each have exactly three list controls and
// their respective labels, and we want the resizing behavior to be the same in the two property
// pages.
// By default, only a label is placed above each of the list controls.  Standard inter-control
// spacing is used between the top of the label and the bottom the the list above it.  Half
// of that inter-control spacing is used between the bottom of the label and the top of the list
// below it.  To allocate more height to the label area (which the caller may fill
// with other controls such as buttons, checkboxes, etc), the caller can pass in non-zero values
// for the corresponding iMinHeightOfLabelAreaX argument.  If 0 is passed in for the
// iMinHeightOfLabelAreaX argument, the label area's height will default to the height of the
// corresponding label
void resize3LabelsAndLists(CPropertyPage *pPropPage, CListCtrl& rList1, CListCtrl& rList2, CListCtrl& rList3,
	UINT nLabel1ID, UINT nLabel2ID, UINT nLabel3ID, const int iMinHeightOfLabelArea1 = 0, 
	const int iMinHeightOfLabelArea2 = 0, const int iMinHeightOfLabelArea3 = 0);
//-------------------------------------------------------------------------------------------------
// PROMISE:	To return today's date in MM/DD format
std::string getMonthDayDateString();
//-------------------------------------------------------------------------------------------------
// PROMISE:	To limit the size of the list control so that the number of entries in the list
//			will not exceed the user-configurable maximum allowable size if one more record is
//			added to the list.  If an entry needs to be removed from the list, the earliest
//			entry will be removed.  True is returned if an item had to be removed removed and 
//			false is returned otherwise.
bool limitListSizeIfNeeded(CListCtrl& rList, FileProcessingConfigMgr* pCfgMgr);
//-------------------------------------------------------------------------------------------------
