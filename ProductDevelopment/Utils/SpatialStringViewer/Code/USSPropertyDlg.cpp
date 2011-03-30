// USSProperty.cpp : implementation file
//

#include "stdafx.h"
#include "SpatialStringViewer.h"
#include "USSPropertyDlg.h"
#include "UCLIDException.h"

#include <cpputil.h>
#include <mathUtil.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstr_ORIENT_NONE = "0°";
const string gstr_ORIENT_RIGHT = "90°";
const string gstr_ORIENT_DOWN = "180°";
const string gstr_ORIENT_LEFT = "270°";
const string gstr_ORIENT_NONE_FLIPPED = "0° Flipped";
const string gstr_ORIENT_RIGHT_FLIPPED = "90° Flipped";
const string gstr_ORIENT_DOWN_FLIPPED = "180° Flipped";
const string gstr_ORIENT_LEFT_FLIPPED = "270° Flipped";

const int gnNUMBER_OF_COLUMNS = 5;
const int gnPAGE_NUMBER_COLUMN = 0;
const int gnHEIGHT_COLUMN = 1;
const int gnWIDTH_COLUMN = 2;
const int gnDESKEW_COLUMN = 3;
const int gnORIENTATION_COLUMN = 4;
LPSTR gstrPAGE_NUMBER_HEADER = "Page #";
LPSTR gstrHEIGHT_HEADER = "Height";
LPSTR gstrWIDTH_HEADER = "Width";
LPSTR gstrDESKEW_HEADER = "Deskew";
LPSTR gstrORIENTATION_HEADER = "Orientation";

//-------------------------------------------------------------------------------------------------
// USSProperty dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(USSPropertyDlg, CDialog)
//-------------------------------------------------------------------------------------------------
USSPropertyDlg::USSPropertyDlg(const string& strSrc, const string& strOrig, const string& strFile, 
							   const string& strOCREngineVersion,
							   const ILongToObjectMapPtr& ipISpatialPageInfoCollection,
							   CWnd* pParent /*=NULL*/)
: CDialog(USSPropertyDlg::IDD, pParent),
m_strMsgSrc(strSrc),
m_strMsgOrig(strOrig),
m_strOCREngineVersion(strOCREngineVersion),
m_USSFileName(strFile),
m_ipSpatialPageInfoCollection(ipISpatialPageInfoCollection)
{
}
//-------------------------------------------------------------------------------------------------
USSPropertyDlg::~USSPropertyDlg()
{
	try
	{
		m_ipSpatialPageInfoCollection = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16495");
}
//-------------------------------------------------------------------------------------------------
void USSPropertyDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST_PAGE_INFO, m_lstPageInfo);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(USSPropertyDlg, CDialog)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// USSProperty message handlers
//-------------------------------------------------------------------------------------------------
BOOL USSPropertyDlg::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();
		// Update the dialog title
		updateDlgTitle();

		// Set the source and original doc name in the edit boxes
		GetDlgItem(IDC_EDIT_SOURCE)->SetWindowText(m_strMsgSrc.c_str());
		GetDlgItem(IDC_EDIT_ORIGINAL)->SetWindowText(m_strMsgOrig.c_str());
		GetDlgItem(IDC_EDIT_OCR_VERSION)->SetWindowText(
			m_strOCREngineVersion.empty() ? "Unknown" : m_strOCREngineVersion.c_str());

		setUpListControl();
		populateListControl();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16834")

	return FALSE;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void USSPropertyDlg::updateDlgTitle()
{
	// Set the title of the property dialog box
	const string strPROPERTY_TITLE = "Properties";
	
	// Compute the window caption
	string strResult;
	if (!m_USSFileName.empty())
	{
		// if a file is currently loaded, then only display the filename and
		// not the full path.
		strResult = getFileNameFromFullPath( m_USSFileName );
		strResult += " ";
		strResult += strPROPERTY_TITLE;
	}
	else
	{
		strResult = strPROPERTY_TITLE;
	}

	// Update the dialog title
	SetWindowText( strResult.c_str() );
}
//-------------------------------------------------------------------------------------------------
void USSPropertyDlg::setUpListControl()
{
	// set up the list control to display the page information
	CRect recListRec;
	m_lstPageInfo.GetClientRect(&recListRec);
	int nTotalWidth = recListRec.Width();
	
	// divide by number of columns+1 so that the orientation column can be twice as big
	int nMyWidth = nTotalWidth/(gnNUMBER_OF_COLUMNS+1);
	int nCol = 0;
	
	// now define the column structure and add the columns to the list control
	LVCOLUMN lvColumn;
	lvColumn.mask = LVCF_FMT | LVCF_TEXT | LVCF_WIDTH;
	lvColumn.fmt = LVCFMT_LEFT;
	lvColumn.cx = nMyWidth;

	// format the page number, height, width, and deskew columns
	lvColumn.pszText = gstrPAGE_NUMBER_HEADER;
	m_lstPageInfo.InsertColumn(nCol++, &lvColumn);
	lvColumn.pszText = gstrHEIGHT_HEADER;
	m_lstPageInfo.InsertColumn(nCol++, &lvColumn);
	lvColumn.pszText = gstrWIDTH_HEADER;
	m_lstPageInfo.InsertColumn(nCol++, &lvColumn);
	lvColumn.pszText = gstrDESKEW_HEADER;
	m_lstPageInfo.InsertColumn(nCol++, &lvColumn);

	// set the orientation column
	lvColumn.pszText = gstrORIENTATION_HEADER;
	
	// for the orientation column, set the width to be the remaining pixels since the data
	// could be longer in this column
	lvColumn.cx = nTotalWidth - ((gnNUMBER_OF_COLUMNS-1)*nMyWidth);
	m_lstPageInfo.InsertColumn(nCol++, &lvColumn);
	
	// set the style for the list control
	m_lstPageInfo.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
}
//-------------------------------------------------------------------------------------------------
void USSPropertyDlg::populateListControl()
{
	// get vector of keys from the map of SpatialPageInfoPtrs.
	IVariantVectorPtr ipKeyVect = m_ipSpatialPageInfoCollection->GetKeys();
	ASSERT_RESOURCE_ALLOCATION("ELI16820", ipKeyVect != __nullptr);
	
	long lVectorSize = ipKeyVect->GetSize();
	
	// iterate through the keys and get the corresponding SpatialPageInfoPtrs.
	// add the spatial page info to our list control
	for (long i=0; i < lVectorSize; i++)
	{
		// get the key value
		long lPage = ipKeyVect->GetItem(i).lVal;
		
		// now retrieve the ISpatialPageInfoPtr
		ISpatialPageInfoPtr ipPageInfo = m_ipSpatialPageInfoCollection->GetValue(lPage);
		if (ipPageInfo == __nullptr)
		{
			UCLIDException ue("ELI16821", "No spatial page info for page!");
			ue.addDebugInfo("Page", lPage);
			throw ue;
		}
		
		// add the page number, height, width and deskew
		// setting the row number to be the page number, this will  
		// sort the list by default.  the list control will automatically
		// not display blank lines, so this covers the case of having page
		// numbers that are not sequential as well
		int nItem = m_lstPageInfo.InsertItem(lPage, asString(lPage).c_str());
		m_lstPageInfo.SetItemText(nItem, gnHEIGHT_COLUMN, 
			asString(ipPageInfo->GetHeight()).c_str());
		m_lstPageInfo.SetItemText(nItem, gnWIDTH_COLUMN,
			asString(ipPageInfo->GetWidth()).c_str());
		m_lstPageInfo.SetItemText(nItem, gnDESKEW_COLUMN,
			getDeskew(ipPageInfo->GetDeskew()).c_str());

		// get the enum for the orientation and set our string based on that
		EOrientation eoOrient = ipPageInfo->GetOrientation();
		
		string strOrient = getOrientationString(eoOrient);
		
		// now set the orientation column
		m_lstPageInfo.SetItemText(nItem, gnORIENTATION_COLUMN, strOrient.c_str());
	}
	
	// get the range of the scroll bar
	int nMin(0), nMax(0);
	m_lstPageInfo.GetScrollRange(SB_VERT, &nMin, &nMax);
	
	// if the min and max of the scroll bar range are the same then there is no scroll bar
	// if they are different then we need to make room for the scroll bar
	if (nMin != nMax)
	{
		int nVScrollWidth = GetSystemMetrics(SM_CXVSCROLL);
		// check for return of 0, this indicates an error, there is not other information available.
		// do not expect to see this case, if we do, create an exception and log it then proceed
		// to display the properties dialog - NOTE: There will not be extra room added for the scroll
		// bar in this case
		if (nVScrollWidth == 0)
		{
			UCLIDException ue("ELI16836", "Unable to determine scroll bar width from system metric!");
			ue.log();
		}
		int nColumnWidth = m_lstPageInfo.GetColumnWidth(gnORIENTATION_COLUMN);
		m_lstPageInfo.SetColumnWidth( gnORIENTATION_COLUMN, (nColumnWidth - nVScrollWidth) );
	}
}
//-------------------------------------------------------------------------------------------------
std::string USSPropertyDlg::getOrientationString(const EOrientation& reOrientation)
{
	string strOrient("");
	switch(reOrientation)
	{
	case kRotNone:
		strOrient = gstr_ORIENT_NONE;
		break;
	case kRotRight:
		strOrient = gstr_ORIENT_RIGHT;
		break;
	case kRotDown:
		strOrient = gstr_ORIENT_DOWN;
		break;
	case kRotLeft:
		strOrient = gstr_ORIENT_LEFT;
		break;

		// we do not expect to see these flags, but we do support them
	case kRotFlipped:
		strOrient = gstr_ORIENT_NONE_FLIPPED;
		break;
	case kRotFlippedRight:
		strOrient = gstr_ORIENT_RIGHT_FLIPPED;
		break;
	case kRotFlippedDown:
		strOrient = gstr_ORIENT_DOWN_FLIPPED;
		break;
	case kRotFlippedLeft:
		strOrient = gstr_ORIENT_LEFT_FLIPPED;
		break;
	default:
		UCLIDException ue("ELI16822", "Unsupported orientation!");
		ue.addDebugInfo("Orientation", reOrientation);
		throw ue;
	}

	return strOrient;
}
//-------------------------------------------------------------------------------------------------
std::string USSPropertyDlg::getDeskew(double dDeskewInDegrees)
{
	// convert to a string with 1 decimal place
	string strReturn = asString(dDeskewInDegrees, 1);
	
	// append the degree symbol
	strReturn.append("°");

	return strReturn;
}
//-------------------------------------------------------------------------------------------------