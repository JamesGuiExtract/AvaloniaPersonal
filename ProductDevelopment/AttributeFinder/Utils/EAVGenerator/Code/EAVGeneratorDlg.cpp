// EAVGeneratorDlg.cpp : implementation file
//

#include "stdafx.h"
#include "EAVGenerator.h"
#include "EAVGeneratorDlg.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <comutils.h>
#include <SuspendWindowUpdates.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>
#include <MiscLeadUtils.h>

#include <io.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrVOAVIEWER_SECTION = "\\AttributeFinder\\Utils\\EAVGenerator";
const string gstrAUTOOPENIMAGE_KEY = "AutoOpenImage";

const string gstrDEFAULT_NEW_SWIPE_NAME = "Manual";

//-------------------------------------------------------------------------------------------------
// CEAVGeneratorDlg dialog
//-------------------------------------------------------------------------------------------------
CEAVGeneratorDlg::CEAVGeneratorDlg(CWnd* pParent /*=NULL*/)
: CDialog(CEAVGeneratorDlg::IDD, pParent),
  m_bFileModified(false),
  m_lRefCount(0), 
  m_bReplaceValueText(true),
  m_bEmptyStringOpened(false),
  m_strCurrentFileName("")
{
	try
	{
		//{{AFX_DATA_INIT(CEAVGeneratorDlg)
		m_zName = _T("");
		m_zType = _T("");
		m_zValue = _T("");
		//}}AFX_DATA_INIT
		// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
		m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
		
		// Initiates the use of singleton input manager.
		UseSingletonInputManager();
		ASSERT_RESOURCE_ALLOCATION("ELI06190", getInputManager() != NULL);

		// get registry persistance manager
		RegistryPersistenceMgr rpmVOA(HKEY_CURRENT_USER, gstrREG_ROOT_KEY);

		// check for the AutoOpenImage key
		if (!rpmVOA.keyExists(gstrVOAVIEWER_SECTION, gstrAUTOOPENIMAGE_KEY))
		{
			// create the key and default to off
			rpmVOA.createKey(gstrVOAVIEWER_SECTION, gstrAUTOOPENIMAGE_KEY, "0");
			
			// set m_bAutoOpenImageEnabled = false;
			m_bAutoOpenImageEnabled = false;
		}
		else
		{
			// key exists, so read from the registry
			string strAutoOpenImageKey = 
				rpmVOA.getKeyValue(gstrVOAVIEWER_SECTION, gstrAUTOOPENIMAGE_KEY);

			// set the AutoOpenImage value
			m_bAutoOpenImageEnabled = (strAutoOpenImageKey == "1");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06189")
}
//-------------------------------------------------------------------------------------------------
CEAVGeneratorDlg::~CEAVGeneratorDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16437");
}

//-------------------------------------------------------------------------------------------------
//  InputManagerEventHandler
//-------------------------------------------------------------------------------------------------
HRESULT CEAVGeneratorDlg::NotifyInputReceived(ITextInput* pTextInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI06225", pTextInput != NULL);
			ITextInputPtr ipInput(pTextInput);
			ASSERT_RESOURCE_ALLOCATION("ELI18086", ipInput != NULL);

			// create a new attribute
			IAttributePtr ipNewAttribute(CLSID_Attribute);
			ASSERT_RESOURCE_ALLOCATION("ELI18112", ipNewAttribute != NULL);

			// set new attribute name to default value
			ipNewAttribute->Name = gstrDEFAULT_NEW_SWIPE_NAME.c_str();

			// Get the input entity
			IInputEntityPtr ipInputEntity = ipInput->GetInputEntity();
			ASSERT_RESOURCE_ALLOCATION("ELI18114", ipInputEntity != NULL);

			// Get the raster zones from the input entity
			IIUnknownVectorPtr ipRasterZoneVector = ipInputEntity->GetOCRZones();
			ASSERT_RESOURCE_ALLOCATION("ELI18113", ipRasterZoneVector != NULL);

			// get the input text
			string strInputText = asString(ipInput->GetText());

			// create a new SpatialString
			ISpatialStringPtr ipValue(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI18116", ipValue != NULL);
			
			// get the source doc name from the inputEntity
			string strSourceDocName = "";
			if (ipInputEntity->IsFromPersistentSource() == VARIANT_TRUE)
			{
				strSourceDocName = asString(ipInputEntity->GetPersistentSourceName());
			}
			else if (ipInputEntity->HasIndirectSource() == VARIANT_TRUE)
			{
				strSourceDocName = asString(ipInputEntity->GetIndirectSource());
			}
			
			// Get the first raster zone
			IRasterZonePtr ipRZone = ipRasterZoneVector->At(0);
			ASSERT_RESOURCE_ALLOCATION("ELI23799", ipRZone != NULL);

			// get the dimensions of the raster zone's page
			long lPageNum = ipRZone->PageNumber;
			int iHeight(0), iWidth(0);
			getImagePixelHeightAndWidth(strSourceDocName, iHeight, iWidth, lPageNum);

			// create the spatial page info for this raster zone
			ISpatialPageInfoPtr ipPageInfo(CLSID_SpatialPageInfo);
			ASSERT_RESOURCE_ALLOCATION("ELI20246", ipPageInfo != NULL);
			ipPageInfo->SetPageInfo(iWidth, iHeight, kRotNone, 0.0);
			
			// create the spatial page info map
			ILongToObjectMapPtr ipPageInfoMap(CLSID_LongToObjectMap);
			ASSERT_RESOURCE_ALLOCATION("ELI20245", ipPageInfoMap != NULL);
			ipPageInfoMap->Set(lPageNum, ipPageInfo);

			// build the spatial string from the raster zone vector and the text
			ipValue->CreateHybridString(ipRasterZoneVector, strInputText.c_str(), 
				strSourceDocName.c_str(), ipPageInfoMap);

			// set the spatial string value of the new attribute to the new spatial string
			ipNewAttribute->Value = ipValue;

			// append or replace the currently selected attribute in the list
			appendOrReplaceAttribute(ipNewAttribute);

			// Highlight the newly created attribute [FlexIDSCore #3169]
			highlightAttributeInRow(gbDO_NOT_OPEN_SRW);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI06192");
	}
	// this is here because CATCH_ALL_AND_DISPLAY ends up 
	// with an ASSERT error when it attempts to do the display.  
	// instead CATCH_ALL_AND_RETHROW_AS_UCLID and catch
	// the UCLIDException, get the exception as a standard string and
	// display it in an AfxMessageBox, then log the exception
	catch (UCLIDException& ue)
	{
		string strException;
		ue.asString(strException);
		AfxMessageBox(strException.c_str(), MB_OK | MB_ICONSTOP | MB_APPLMODAL, 0);
		ue.log();
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT_VALUE, m_editValue);
	DDX_Control(pDX, IDC_EDIT_TYPE, m_editType);
	DDX_Control(pDX, IDC_EDIT_NAME, m_editName);
	DDX_Control(pDX, IDC_LIST_DISPLAY, m_listAttributes);
	DDX_Control(pDX, IDC_BTN_UP, m_btnUp);
	DDX_Control(pDX, IDC_BTN_ADD, m_btnAdd);
	DDX_Control(pDX, IDC_BTN_COPY, m_btnCopy);
	DDX_Control(pDX, IDC_BTN_DOWN, m_btnDown);
	DDX_Control(pDX, IDC_BTN_DELETE, m_btnDelete);
	DDX_Control(pDX, IDC_BTN_SPLIT, m_btnSplit);
	DDX_Text(pDX, IDC_EDIT_NAME, m_zName);
	DDX_Text(pDX, IDC_EDIT_TYPE, m_zType);
	DDX_Text(pDX, IDC_EDIT_VALUE, m_zValue);
	DDX_Control(pDX, IDC_BTN_MERGE, m_btnMerge);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CEAVGeneratorDlg, CDialog)
	ON_WM_DROPFILES()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BTN_ADD, OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_COPY, OnBtnCopy)
	ON_BN_CLICKED(IDC_BTN_DELETE, OnBtnDelete)
	ON_BN_CLICKED(IDC_BTN_SPLIT, OnBtnSplit)
	ON_BN_CLICKED(IDC_BTN_DOWN, OnBtnDown)
	ON_BN_CLICKED(IDC_BTN_UP, OnBtnUp)
	ON_BN_CLICKED(IDC_BTN_HIGHLIGHT, OnBtnHighlight)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_DISPLAY, OnItemchangedListDisplay)
	ON_EN_CHANGE(IDC_EDIT_NAME, OnChangeEditName)
	ON_EN_CHANGE(IDC_EDIT_TYPE, OnChangeEditType)
	ON_EN_CHANGE(IDC_EDIT_VALUE, OnChangeEditValue)
	ON_WM_CLOSE()
	ON_BN_CLICKED(IDC_RADIO_REPLACE, OnBtnReplace)
	ON_BN_CLICKED(IDC_RADIO_APPEND, OnBtnAppend)
	ON_COMMAND(IDC_BTN_IMAGEWINDOW, OnBtnImagewindow)
	ON_COMMAND(IDC_BTN_NEW, OnBtnNew)
	ON_COMMAND(IDC_BTN_OPEN, OnBtnOpen)
	ON_COMMAND(IDC_BTN_SAVEFILE, OnBtnSavefile)
	ON_NOTIFY(LVN_KEYDOWN, IDC_LIST_DISPLAY, OnKeydownListDisplay)
	ON_COMMAND(IDC_BTN_SAVEAS, OnBtnSaveas)
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXT,0x0000,0xFFFF,OnToolTipNotify)
	ON_NOTIFY(NM_CLICK, IDC_LIST_DISPLAY, &CEAVGeneratorDlg::OnNMClickListDisplay)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_DISPLAY, &CEAVGeneratorDlg::OnNMDblclkListDisplay)
	ON_BN_CLICKED(IDC_BTN_MERGE, &CEAVGeneratorDlg::OnBtnMerge)
END_MESSAGE_MAP()


//-------------------------------------------------------------------------------------------------
// IUnknown
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) CEAVGeneratorDlg::AddRef()
{
	InterlockedIncrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) CEAVGeneratorDlg::Release()
{
	InterlockedDecrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEAVGeneratorDlg::QueryInterface( REFIID iid, void FAR* FAR* ppvObj)
{
	IParagraphTextHandler *pTmp = this;

	if (iid == IID_IUnknown)
		*ppvObj = static_cast<IUnknown *>(pTmp);
	else if (iid == IID_IDispatch)
		*ppvObj = static_cast<IDispatch *>(pTmp);
	else if (iid == IID_IParagraphTextHandler)
		*ppvObj = static_cast<IParagraphTextHandler *>(this);
	else
		*ppvObj = NULL;

	if (*ppvObj != NULL)
	{
		AddRef();
		return S_OK;
	}
	else
	{
		return E_NOINTERFACE;
	}
}

//-------------------------------------------------------------------------------------------------
// IParagraphTextHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEAVGeneratorDlg::raw_NotifyParagraphTextRecognized(
	ISpotRecognitionWindow *pSourceSRWindow, ISpatialString *pText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// modified as per P16 #2631 - JDS 12/21/2007
	try
	{
		// check the argument
		ISpatialStringPtr ipText(pText);
		ASSERT_RESOURCE_ALLOCATION("ELI07356", ipText != NULL);

		// create a new attribute
		IAttributePtr ipNewAttribute(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI18736", ipNewAttribute != NULL);

		// set new attribute name to default value
		ipNewAttribute->Name = gstrDEFAULT_NEW_SWIPE_NAME.c_str();

		// set the new attributes spatial string
		ipNewAttribute->Value = ipText;

		// append or replace the currently selected attribute in the list
		appendOrReplaceAttribute(ipNewAttribute);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07369")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEAVGeneratorDlg::raw_GetPTHDescription(BSTR *pstrDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pstrDescription = _bstr_t("Send text to VOA File Viewer window").copy();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07370")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEAVGeneratorDlg::raw_IsPTHEnabled(VARIANT_BOOL *pbEnabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18727", pbEnabled != NULL);

		// PTH is enabled if there is an attribute selected (P16 #2651)
		if (m_listAttributes.GetSelectedCount() > 0)
		{
			*pbEnabled = VARIANT_TRUE;
		}
		else
		{
			*pbEnabled = VARIANT_FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07371")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper Functions
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::addAttributeLevel(IAttributePtr ipNewAttribute, int iLevel)
{
	ASSERT_ARGUMENT("ELI18182", ipNewAttribute != NULL);

	// Check existence of vector at specified level
	long lSize = m_vecCurrentLevels.size();
	if (lSize <= iLevel)
	{
		// Throw exception, Cannot add Attribute at this level
		UCLIDException	ue( "ELI07907", "Unable to add Attribute." );
		ue.addDebugInfo( "Desired Attribute level", iLevel );
		throw ue;
	}

	// Retrieve vector of Attributes or SubAttributes at specified level
	IIUnknownVectorPtr	ipLevelVector = m_vecCurrentLevels.at( iLevel );
	if (ipLevelVector == NULL)
	{
		// Throw exception, Cannot retrieve specified Attribute collection
		UCLIDException	ue( "ELI07908", "Unable to retrieve Attributes." );
		ue.addDebugInfo( "Desired Attribute level", iLevel );
		throw ue;
	}

	// Remove outdated child vectors
	int iNumRemoved = lSize - iLevel - 1;
	for (int i = 0; i < iNumRemoved; i++)
	{
		m_vecCurrentLevels.pop_back();
	}

	// Add new Attribute at specified level
	ipLevelVector->PushBack( ipNewAttribute );

	// Update current levels vector
	m_vecCurrentLevels.push_back( ipNewAttribute->GetSubAttributes() );
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::addToSetIfNonEmpty(set<string>& rsetStrings, const _bstr_t& bstrString)
{
	// The string is empty, we are done
	if (bstrString.length() == 0)
	{
		return;
	}

	// Add the string to the set
	rsetStrings.insert( asString(bstrString) );
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::addSubAttributes(IAttributePtr ipAttribute, 
										int iNumItemsUnderInsertionPoint, 
										int iSubLevel)
{
	ASSERT_ARGUMENT("ELI18183", ipAttribute != NULL);

	// Retrieve collection of sub-attributes
	IIUnknownVectorPtr	ipSubAttributes = ipAttribute->GetSubAttributes();
	long lCount = 0;
	if (ipSubAttributes != NULL)
	{
		lCount = ipSubAttributes->Size();
	}

	// Add each sub-attribute to list
	for (int i = 0; i < lCount; i++)
	{
		// Retrieve this sub-attribute
		IAttributePtr	ipThisSub = ipSubAttributes->At( i );

		// Get Value
		ISpatialStringPtr ipValue = ipThisSub->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI15605", ipValue != NULL);

		// Get Name, Value, and Type as STL strings
		string	strName = ipThisSub->Name;
		string	strValue = ipValue->String;
		string	strType = ipThisSub->Type;

		// convert cpp string to user-displayable string
		::convertCppStringToNormalString(strValue);

		// Add prefix periods to Name string
		for (int j = 0; j < iSubLevel; j++)
		{
			strName = "." + strName;
		}

		// Insert index is always relative to current list size
		long lInsertIndex = m_listAttributes.GetItemCount() - iNumItemsUnderInsertionPoint;

		// Add the item to the list
		int nNewItemIndex = m_listAttributes.InsertItem( lInsertIndex, strName.c_str() );
		m_listAttributes.SetItemText( nNewItemIndex, VALUE_COLUMN, strValue.c_str() );
		m_listAttributes.SetItemText( nNewItemIndex, TYPE_COLUMN, strType.c_str() );

		// Add any grandchildren items
		addSubAttributes( ipThisSub, iNumItemsUnderInsertionPoint, iSubLevel + 1 );

		// get the attribute from the smart pointer
		IAttribute* pipSubAttribute = ipThisSub.Detach();
		ASSERT_RESOURCE_ALLOCATION("ELI18274", pipSubAttribute != NULL);

		// add the attribute to the list
		m_listAttributes.SetItemData(nNewItemIndex, (DWORD_PTR)pipSubAttribute);
	}

	// "flatten" the attribute hierarchy before storing it into the attribute list. [P16 #2861]
	ipAttribute->SubAttributes = NULL;
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::createToolBar()
{
	// Create the toolbar
	m_apToolBar = auto_ptr<CToolBar>(new CToolBar());
	if (m_apToolBar->CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_TOP
    | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) )
	{
		m_apToolBar->LoadToolBar( IDR_EAV_TOOLBAR );
	}

	m_apToolBar->SetBarStyle(m_apToolBar->GetBarStyle() |
		CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC);

	// must set TBSTYLE_TOOLTIPS here in order to get tool tips
	m_apToolBar->ModifyStyle(0, TBSTYLE_TOOLTIPS);

	// We need to resize the dialog to make room for control bars.
	// First, figure out how big the control bars are.
	CRect rcClientStart;
	CRect rcClientNow;
	GetClientRect(rcClientStart);
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST,
				   0, reposQuery, rcClientNow);

	// Now move all the controls so they are in the same relative
	// position within the remaining client area as they would be
	// with no control bars.
	CPoint ptOffset(rcClientNow.left - rcClientStart.left,
					rcClientNow.top - rcClientStart.top);

	CRect  rcChild;
	CWnd* pwndChild = GetWindow(GW_CHILD);
	while (pwndChild)
	{
		pwndChild->GetWindowRect(rcChild);
		ScreenToClient(rcChild);
		rcChild.OffsetRect(ptOffset);
		pwndChild->MoveWindow(rcChild, FALSE);
		pwndChild = pwndChild->GetNextWindow();
	}

	// Adjust the dialog window dimensions
	CRect rcWindow;
	GetWindowRect(rcWindow);
	rcWindow.right += rcClientStart.Width() - rcClientNow.Width();
	rcWindow.bottom += rcClientStart.Height() - rcClientNow.Height();
	MoveWindow(rcWindow, FALSE);

	// And position the control bars
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);

	// Refresh display
	UpdateWindow();
	Invalidate();
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::deleteTemporaryHighlights()
{
	// Iterate through the currently selected images
	for (unsigned int i = 0; i < m_vecstrCurrentImageWithHighlights.size(); i++)
	{
		string& strImage = m_vecstrCurrentImageWithHighlights[i];

		// Get a handle to the SRW with the current highlighted image
		ISpotRecognitionWindowPtr ipSRIR = getImageViewer(strImage, gbDO_NOT_OPEN_SRW);
		if (ipSRIR)
		{
			// If we successfully got the handle, clear the highlights
			ipSRIR->DeleteTemporaryHighlight();
		}
	}

	// None of the images have highlights anymore
	m_vecstrCurrentImageWithHighlights.clear();
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::displayAttributes(IIUnknownVectorPtr ipAttributes)
{
	try
	{
		ASSERT_ARGUMENT("ELI18205", ipAttributes != NULL);
		
		// Clear the list box first
		clearListControl();

		// Step through the collection of Attributes
		long nTotalCount = 0;
		long lCount = ipAttributes->Size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve the Attribute object
			IAttributePtr	ipAttribute = ipAttributes->At( i );
			ASSERT_RESOURCE_ALLOCATION("ELI18142", ipAttribute != NULL);

			// Get Value
			ISpatialStringPtr ipValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI19437", ipValue != NULL);

			// Extract Name, Value, Type as STL strings
			string strName = asString(ipAttribute->Name);
			string strValue = asString(ipValue->String);
			string strType = asString(ipAttribute->Type);

			// convert the attribute value to user displayable text [P16 #2860]
			convertCppStringToNormalString(strValue);

			string strSourceDocName = asString(ipValue->SourceDocName);

			// Get current list count
			long nTotalCount = m_listAttributes.GetItemCount();

			// Add Name entry below current items
			int nNewItemIndex = m_listAttributes.InsertItem( nTotalCount, 
				strName.c_str() );

			// Add Value
			m_listAttributes.SetItemText( nNewItemIndex, VALUE_COLUMN, 
				strValue.c_str());

			// Add Type, if present
			if (strType.length() > 0)
			{
				m_listAttributes.SetItemText( nNewItemIndex, TYPE_COLUMN, 
					strType.c_str());
			}

			// Add any sub attributes to end of list
			addSubAttributes( ipAttribute, 0, 1 );

			// get the attribute pointer from the smartpointer
			IAttribute* pipAttribute = ipAttribute.Detach();
			ASSERT_RESOURCE_ALLOCATION("ELI18275", pipAttribute != NULL);

			// store the attribute in the list
			m_listAttributes.SetItemData(nNewItemIndex, (DWORD_PTR)pipAttribute);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18087");
}
//-------------------------------------------------------------------------------------------------
unsigned int CEAVGeneratorDlg::getAttributeLevel(int iIndex)
{
	// The attribute level is represented by the number of periods that precede the attribute name
	CString zName = m_listAttributes.GetItemText(iIndex, NAME_COLUMN);
	string strName = LPCTSTR(zName);
	unsigned int iResult = strName.find_first_not_of('.', 0);

	// If the name consists entirely of periods (or is the empty string) its length is its level
	return iResult == string::npos ? strName.length() : iResult;
}
//-------------------------------------------------------------------------------------------------
int CEAVGeneratorDlg::getCurrentSelectedListItemIndex()
{
	// get the first selected item position
	POSITION pos = m_listAttributes.GetFirstSelectedItemPosition();

	// default the selection to -1
	int nSelectedItemIndex = -1;
	if (pos != NULL)
	{
		// ensure pos is not NULL before calling GetNextSelectedItem
		nSelectedItemIndex = m_listAttributes.GetNextSelectedItem(pos);
	}

	return nSelectedItemIndex;
}
//-------------------------------------------------------------------------------------------------
ISRIRUtilsPtr CEAVGeneratorDlg::getImageUtils()
{
	// Create a new SRIRUtils if not already created
	if (m_ipSRIRUtils == NULL)
	{
		m_ipSRIRUtils.CreateInstance(CLSID_SRIRUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI18083", m_ipSRIRUtils != NULL);
	}

	return m_ipSRIRUtils;
}
//-------------------------------------------------------------------------------------------------
ISpotRecognitionWindowPtr CEAVGeneratorDlg::getImageViewer(const string& strImage, bool bOpenWindow)
{
	// Return an image viewer
	return getImageUtils()->GetSRIRWithImage(
		strImage.c_str(), getInputManager(), asVariantBool(bOpenWindow));
}
//-------------------------------------------------------------------------------------------------
bool CEAVGeneratorDlg::isAnyEmptyAttribute()
{
	bool	bReturn = false;

	// Get Attribute count
	int nCount = m_listAttributes.GetItemCount();

	// Loop through Attributes
	CString	zName;
	CString	zValue;
	CString	zType;
	for (int i = 0; i < nCount; i++)
	{
		// Retrieve text strings for this Attribute
		zName = m_listAttributes.GetItemText( i, NAME_COLUMN );
		zValue = m_listAttributes.GetItemText( i, VALUE_COLUMN );
		zType = m_listAttributes.GetItemText( i, TYPE_COLUMN );

		// Check for all empty strings
		if (zName.IsEmpty() && zValue.IsEmpty() && zType.IsEmpty())
		{
			// Set flag and break out of loop
			bReturn = true;
			break;
		}
	}

	// Return result
	return bReturn;
}
//-------------------------------------------------------------------------------------------------
bool CEAVGeneratorDlg::isSiblingsSelected()
{
	// If no items are selected we are done
	POSITION pos = m_listAttributes.GetFirstSelectedItemPosition();
	if (pos == NULL)
	{
		// Vacuously true (only siblings are selected when nothing is selected)
		return true;
	}

	// Get the index of the first selected item
	int iSelected = m_listAttributes.GetNextSelectedItem(pos);

	// Get the level of the first selected item
	unsigned int uiLevel = getAttributeLevel(iSelected);

	// Check all the items between the first selected item and the last selected item
	while (pos != NULL)
	{
		// Get the next selected item
		int iNextSelected = m_listAttributes.GetNextSelectedItem(pos);
		
		// All the items between the selected items must be on the same level or higher
		// (Otherwise, they have different parents)
		for (int i = iSelected + 1; i < iNextSelected; i++)
		{
			if (getAttributeLevel(i) < uiLevel)
			{
				return false;
			}
		}

		// The selected attributes must be on the same level to be siblings
		if (getAttributeLevel(iNextSelected) != uiLevel)
		{
			return false;
		}
		
		// Iterate
		iSelected = iNextSelected;
	}
	
	// If we reached this point they are siblings
	return true;
}
//-------------------------------------------------------------------------------------------------
string CEAVGeneratorDlg::joinStrings(const set<string>& setStringsToJoin, char cDelimiter)
{
	// If there are no strings to join, return the empty string
	set<string>::const_iterator iter = setStringsToJoin.begin();
	if (iter == setStringsToJoin.end())
	{
		return "";
	}

	// Combine the strings separated by the delimiter
	string strResult = *iter;
	for (iter++; iter != setStringsToJoin.end(); iter++)
	{
		strResult += cDelimiter + *iter;
	}

	// Return the result
	return strResult;
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::openEAVFile(const CString& zFileName)
{
	// clear the list box first
	clearListControl();

	ifstream ifs(zFileName);
	CommentedTextFileReader fileReader(ifs, "//", true);

	// total number of items in the list
	int nTotalCount = 0;
	while (!ifs.eof())
	{
		string strLine(fileReader.getLineText());
		if (!strLine.empty())
		{
			// parse the line into tokens, make sure there're
			// exact 2 or 3 tokens
			vector<string> vecTokens;
			StringTokenizer::sGetTokens(strLine, "|", vecTokens);
			int nNumOfTokens = vecTokens.size();
			if (nNumOfTokens != 2 && nNumOfTokens != 3)
			{
				UCLIDException ue("ELI06228", "Invalid line text.");
				ue.addDebugInfo("Line Text", strLine);
				throw ue;
			}

			// create a new attribute
			IAttributePtr ipAttribute(CLSID_Attribute);
			ASSERT_RESOURCE_ALLOCATION("ELI18206", ipAttribute != NULL);

			// get the new spatial string
			ISpatialStringPtr ipValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI18207", ipValue != NULL);

			// add the data to the attribute
			// NOTE: subattribute names are prepended with one or more periods. 
			// Remove these before storing. [P16 #2848]
			ipAttribute->Name = trim(vecTokens[0], ".", "").c_str();

			// convert the escaped characters to their literal values [P16 #2860]
			string strValue = vecTokens[1].c_str();
			convertNormalStringToCppString(strValue);
			ipValue->ReplaceAndDowngradeToNonSpatial(strValue.c_str());

			// add the record to the list
			nTotalCount = m_listAttributes.GetItemCount();
			int nNewItemIndex = m_listAttributes.InsertItem(nTotalCount, vecTokens[0].c_str());

			m_listAttributes.SetItemText(nNewItemIndex, VALUE_COLUMN, vecTokens[1].c_str());
			if (nNumOfTokens == 3)
			{
				m_listAttributes.SetItemText(nNewItemIndex, TYPE_COLUMN, vecTokens[2].c_str());
				ipAttribute->Type = vecTokens[2].c_str();
			}

			// get the attribute from the smart pointer
			IAttribute* pipAttribute = ipAttribute.Detach();
			ASSERT_RESOURCE_ALLOCATION("ELI18276", pipAttribute != NULL);

			m_listAttributes.SetItemData(nNewItemIndex, (DWORD_PTR)pipAttribute);
		}
	}

	if (nTotalCount > 0)
	{
		// select first item of the list
		selectListItem(0);
	}
	
	updateButtons();

	// update current file name
	m_strCurrentFileName = (LPCTSTR)zFileName;

	// Update the window caption
	updateWindowCaption(zFileName);

	// reset flag
	setModified(false);
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::openVOAFile(const CString& zFileName)
{
	// Read the file into IUnknownVector object
	IIUnknownVectorPtr	ipAttributes( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI07886", ipAttributes != NULL );
	ipAttributes->LoadFrom( _bstr_t( LPCTSTR(zFileName) ), VARIANT_FALSE );

	// Display the Attributes
	displayAttributes( ipAttributes );

	// update current file name
	m_strCurrentFileName = (LPCTSTR)zFileName;

	if (m_listAttributes.GetItemCount() > 0)
	{
		// select first item of the list
		selectListItem(0);

		// if auto open is enabled then attempt to open the SRW with
		// the first attribute
		if (m_bAutoOpenImageEnabled)
		{
			IAttributePtr ipAttribute((IAttribute*) m_listAttributes.GetItemData(0));
			ASSERT_RESOURCE_ALLOCATION("ELI18177", ipAttribute != NULL);

			ISpatialStringPtr ipValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI18178", ipValue != NULL);

			// if the first attribute has spatial info then open the image
			// in SRW and highlight the attribute
			if (ipValue->HasSpatialInfo() == VARIANT_TRUE)
			{
				highlightAttributeInRow(gbOPEN_SRW);
			}
			else
			{
				// no spatial info, try to open the image file based
				// on the VOA file name 
				openSRWFromVOAFileName();
			}
		}
	}
	else
	{
		// 0 attributes, if auto highlight is on then attempt to open the
		// image file based on the VOA file name 
		if (m_bAutoOpenImageEnabled)
		{
			openSRWFromVOAFileName();
	
		}
	}
	
	updateButtons();

	// Update the window caption
	updateWindowCaption(zFileName);

	// reset flag
	setModified(false);

	// set the focus to the list control
	m_listAttributes.SetFocus();
}
//-------------------------------------------------------------------------------------------------
bool CEAVGeneratorDlg::promptForSaving()
{
	if (m_bFileModified)
	{
		int res = MessageBox("This file is modified. Do you wish to save it?",
			"Save File?", MB_YESNOCANCEL);
		if (res == IDCANCEL)
		{
			return false;
		}
		else if (res == IDYES)
		{
			OnBtnSavefile();
		}
		// if the user choose NO, that's also considered as
		// a successful operation
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::saveAttributes(const CString& zFileName)
{
	// make sure the file has read/write permission if exists
	if (fileExistsAndIsReadOnly(zFileName.GetString()))
	{
		// change the mode to read/write
		if (_chmod(zFileName, _S_IREAD | _S_IWRITE) == -1)
		{
			CString zMsg("Failed to save ");
			zMsg += zFileName;
			zMsg += ". Please make sure the file is not shared by another program.";
			// failed to change the mode
			AfxMessageBox(zMsg);
			return;
		}
	}

	// Check file type
	string strExt = getExtensionFromFullPath( LPCTSTR(zFileName), true );
	if (strExt == ".eav")
	{
		// EAV file
		saveAttributesToEAV( zFileName );
	}
	else if (strExt == ".voa" || strExt == ".evoa")
	{
		// VOA file
		saveAttributesToVOA( zFileName );
	}
	else
	{
		// Throw exception
		UCLIDException ue( "ELI07911", "Unable to save file.");
		ue.addDebugInfo( "File To Save", LPCTSTR(zFileName) );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::saveAttributesToEAV(const CString& zFileName)
{
	// open the output EAV file
	ofstream ofs(zFileName, ios::out | ios::trunc);

	// retrieve all attributes and walk through them to validate
	int nCount = m_listAttributes.GetItemCount();
	for (int n=0; n<nCount; n++)
	{
		// Retrieve text from edit boxes
		CString zName = m_listAttributes.GetItemText(n, NAME_COLUMN);
		CString zValue = m_listAttributes.GetItemText(n, VALUE_COLUMN);
		CString zType = m_listAttributes.GetItemText(n, TYPE_COLUMN);
		
		// put name|value<|type>
		string strOutputLine = (LPCTSTR)zName;
		strOutputLine += "|";
		strOutputLine += (LPCTSTR)zValue;
		if (!zType.IsEmpty())
		{
			strOutputLine += "|";
			strOutputLine += (LPCTSTR)zType;
		}
		
		// Just skip adding this line to the EAV file if Value is empty
		if (!zValue.IsEmpty())
		{
			ofs << strOutputLine << endl;
		}
	}

	// Close file and wait for file to be readable
	ofs.close();
	waitForFileToBeReadable((LPCTSTR)zFileName);
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::saveAttributesToVOA(const CString& zFileName)
{
	// Create the IIUnknownVector for Attributes
	IIUnknownVectorPtr	ipAttributes( CLSID_IUnknownVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI07905", ipAttributes != NULL );

	// Retrieve each attribute to build vector
	int nCount = m_listAttributes.GetItemCount();
	for (int n = 0; n < nCount; n++)
	{		
		// Get the level of the nth attribute
		unsigned int uiLevel = getAttributeLevel(n);

		IAttributePtr ipAttribute((IAttribute*) m_listAttributes.GetItemData(n));
		ASSERT_RESOURCE_ALLOCATION( "ELI07906", ipAttribute != NULL );

		// Skip nameless and all-period entries 
		string strName = asString(ipAttribute->Name);
		if (uiLevel < strName.length())
		{
			// trim leading periods from the name
			string strAttribName = trim(strName, ".", "");
			ipAttribute->Name = strAttribName.c_str();
		}

		// Add Attribute to collection
		if (uiLevel > 0)
		{
			// Add this subattribute to the appropriate collection of (Sub)Attributes
			addAttributeLevel( ipAttribute, uiLevel );
		}
		else
		{
			// This is a main-level attribute
			ipAttributes->PushBack( ipAttribute );

			// Clear the previous collection of Attribute nodes
			m_vecCurrentLevels.clear();

			// Level zero collection is the new main-level Attribute
			m_vecCurrentLevels.push_back( ipAttribute );

			// Level one collection is the collection of sub-attributes
			// of the new attribute
			m_vecCurrentLevels.push_back( ipAttribute->GetSubAttributes() );
		}
	}

	// Write the VOA file
	ipAttributes->SaveTo( _bstr_t( LPCTSTR(zFileName) ), VARIANT_TRUE );
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::selectListItem(int iIndex)
{
	int iCount = m_listAttributes.GetItemCount();
	for (int i = 0; i < iCount; i++)
	{
		if (i == iIndex)
		{
			// Select the specified item
			m_listAttributes.SetItemState(i, LVIS_SELECTED, LVIS_SELECTED);
		}
		else
		{
			// Deselect all other items
			m_listAttributes.SetItemState(i, 0, LVIS_SELECTED);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::setModified(bool bModified)
{
	m_bFileModified = bModified;

	// update the state of the Save button
	m_apToolBar->GetToolBarCtrl().EnableButton(IDC_BTN_SAVEFILE, m_bFileModified ? TRUE : FALSE);
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::updateButtons()
{
	// suspend the window updates so that the buttons don't flash as we update them
	SuspendWindowUpdates temp(*this);

	int iCount = m_listAttributes.GetItemCount();
	int iSelectedCount = m_listAttributes.GetSelectedCount();

	// Enable Delete if there is at least one item selected in the list
	m_btnDelete.EnableWindow( asMFCBool(iSelectedCount > 0) );

	// Enable the split button if exactly one item is selected in the list
	BOOL bEnable = asMFCBool(iSelectedCount == 1);
	m_btnSplit.EnableWindow(bEnable);

	// Check if only one item selected
	if (iSelectedCount == 1)
	{
		// Update Name and Type edit boxes
		int iSelectedItemIndex = getCurrentSelectedListItemIndex();
		m_zName = m_listAttributes.GetItemText(iSelectedItemIndex, NAME_COLUMN);
		m_zType = m_listAttributes.GetItemText(iSelectedItemIndex, TYPE_COLUMN);

		// Convert Value string to Cpp style
		string strValue = m_listAttributes.GetItemText(iSelectedItemIndex, VALUE_COLUMN);
		::convertNormalStringToCppString(strValue);
		m_zValue = strValue.c_str();

		// First item cannot move up
		m_btnUp.EnableWindow( asMFCBool(iSelectedItemIndex != 0) );

		// Last item cannot move down
		m_btnDown.EnableWindow( asMFCBool(iSelectedItemIndex != iCount-1) );
	}
	else
	{
		// Clear edit box text
		m_zName = "";
		m_zValue = "";
		m_zType = "";

		// Disable the up and down arrows
		m_btnUp.EnableWindow(FALSE);
		m_btnDown.EnableWindow(FALSE);
	}

	// Disable Copy button if selected Attribute is empty
	m_btnCopy.EnableWindow( asMFCBool(!m_zName.IsEmpty() || !m_zValue.IsEmpty() || !m_zType.IsEmpty()) );

	// Disable Add button if already an empty Attribute
	m_btnAdd.EnableWindow( asMFCBool(!isAnyEmptyAttribute()) );

	// Enable the merge button if multiple sibling attributes are selected
	m_btnMerge.EnableWindow( asMFCBool(iSelectedCount > 1 && isSiblingsSelected()) );

	// also update three edit boxes
	m_editName.EnableWindow(bEnable);
	m_editValue.EnableWindow(bEnable);
	m_editType.EnableWindow(bEnable);

	UpdateData(FALSE);
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::updateList(int nColumnNumber, const CString& zText)
{
	// current selected item
	int nSelectedItemIndex = getCurrentSelectedListItemIndex();

	// Modify Value column text
	string strText = LPCTSTR(zText);

	// check if there is an attribute associated with this entry
	IAttributePtr ipAttribute((IAttribute*) m_listAttributes.GetItemData(nSelectedItemIndex));
	ASSERT_RESOURCE_ALLOCATION("ELI18132", ipAttribute != NULL);

	switch(nColumnNumber)
	{
	case NAME_COLUMN:

		// Trim leading periods from attribute name [FlexIDSCore #3336]
		ipAttribute->Name = trim(strText, ".", "").c_str();
		break;

	case VALUE_COLUMN:
		{
			ISpatialStringPtr ipValue = ipAttribute->Value;
			if (!ipValue)
			{
				ipValue.CreateInstance(CLSID_SpatialString);
				ASSERT_RESOURCE_ALLOCATION("ELI18208", ipValue != NULL);
				ipAttribute->Value = ipValue;
			}
			
			// if current mode is spatial, downgrade to hybrid
			if (ipValue->GetMode() == UCLID_RASTERANDOCRMGMTLib::kSpatialMode)
			{
				ipValue->DowngradeToHybridMode();
			}

			// check for empty string, if string is empty then just set text
			// [p16 #2727]
			if (ipValue->IsEmpty() == VARIANT_FALSE)
			{
				// replace the current string (only works if ipValue->String != "")
				ipValue->Replace(ipValue->String, strText.c_str(), VARIANT_TRUE, 0, NULL);	
			}
			else
			{
				ipValue->ReplaceAndDowngradeToNonSpatial(strText.c_str());
			}
		}
		break;

	case TYPE_COLUMN:
		ipAttribute->Type = strText.c_str();
		break;
	}

	if (nColumnNumber == VALUE_COLUMN)
	{
		::convertCppStringToNormalString(strText);
	}

	// update each column
	m_listAttributes.SetItemText(nSelectedItemIndex, nColumnNumber, strText.c_str());
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::updateWindowCaption(const CString& zFileName)
{
	// Default caption
	const string strWINDOW_TITLE = "VOA File Viewer";
	
	// Compute the window caption
	string strResult;
	if (zFileName.GetLength() > 0)
	{
		// Only display the filename and not the full path.
		strResult = getFileNameFromFullPath( LPCTSTR(zFileName) );
		strResult += " - ";
		strResult += strWINDOW_TITLE;
	}
	else
	{
		strResult = strWINDOW_TITLE;
	}

	// Update the window caption
	SetWindowText(strResult.c_str());

	string strFileName = (LPCTSTR)zFileName;
	if (!strFileName.empty())
	{
		// make sure the file name is a long path name
		::getLongPathName((LPCTSTR)zFileName, strFileName);
	}
	// update current file name
	GetDlgItem(IDC_STATIC_FILENAME)->SetWindowText(strFileName.c_str());
}
//-------------------------------------------------------------------------------------------------
bool CEAVGeneratorDlg::validateAttributes()
{
	// retrieve all attributes and walk through them to validate
	int nCount = m_listAttributes.GetItemCount();

	// Allow zero attributes in collection (P16 #2711)
	for (int n=0; n<nCount; n++)
	{
		CString zName = m_listAttributes.GetItemText(n, NAME_COLUMN);
		CString zValue = m_listAttributes.GetItemText(n, VALUE_COLUMN);

		// name shall not be empty
		if (zName.IsEmpty())
		{
			CString zMsg("");
			zMsg.Format("Please complete Attribute #%d before saving it to a file.", n+1);
			AfxMessageBox(zMsg);
			// select the item
			selectListItem(n);

			return false;
		}
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::highlightAttributeInRow(bool bOpenWindow)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Delete all temporary highlights
		deleteTemporaryHighlights();

		// Create map of image names to the spatial strings that are highlighted on them
		map<string, ISpatialStringPtr> mapImageToSpatialString;
		map<string, ISpatialStringPtr>::iterator iter;

		// Iterate through the selected items
		POSITION pos = m_listAttributes.GetFirstSelectedItemPosition();
		while (pos != NULL)
		{
			// Get the next selected item index
			int nSelectedItemIndex = m_listAttributes.GetNextSelectedItem(pos);

			// Get pointer to associated Attribute
			IAttributePtr ipAttribute( 
				(IAttribute*) m_listAttributes.GetItemData(nSelectedItemIndex));
			ASSERT_RESOURCE_ALLOCATION("ELI18081", ipAttribute != NULL);

			// get the spatial string representing the attribute value
			ISpatialStringPtr ipValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI18082", ipValue != NULL);

			// get the source image name
			string strImage = getSourceImageName(ipValue);

			// if the value is not a spatial string, or if the spatial string
			// does not have a source document associated with it, then skip this
			if (ipValue->HasSpatialInfo() == VARIANT_FALSE || strImage.empty())
			{
				continue;
			}

			// Clone the spatial string and set the source doc name to
			// the computed source image name [FlexIDSCore #3497]
			ICopyableObjectPtr ipCopy = ipValue;
			ASSERT_RESOURCE_ALLOCATION("ELI25160", ipCopy != NULL);
			ISpatialStringPtr ipClone = ipCopy->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI25593", ipClone != NULL);
			ipClone->SourceDocName = strImage.c_str();

			// Check if a highlight from this image has already been added to the map
			iter = mapImageToSpatialString.find(strImage);
			if (iter == mapImageToSpatialString.end())
			{
				// It doesn't already exist. Add it.
				mapImageToSpatialString[strImage] = ipClone;
			}
			else
			{
				// It already exists. Append to it.
				ISpatialStringPtr ipOldValue = iter->second;
				ASSERT_RESOURCE_ALLOCATION("ELI25594", ipOldValue != NULL);
				ipOldValue->AppendString("\r\n");
				ipOldValue->Append(ipClone);
			}
		}

		// Iterate through the highlights of each image
		for (iter = mapImageToSpatialString.begin(); iter != mapImageToSpatialString.end(); iter++)
		{
			// Get the image name
			const string& strImage = iter->first;

			// if there is an SRW with an empty string and the new image to open is not empty
			ISpotRecognitionWindowPtr ipSRIR(NULL);
			if (m_bEmptyStringOpened)
			{
				// get the handle to the SRW with the empty string
				ipSRIR = getImageViewer("", bOpenWindow);
						
				// make sure we have an SRW and the flag bOpenWindow==true open the image
				// in the SRW
				if (ipSRIR && bOpenWindow)
				{
					ipSRIR->OpenImageFile(strImage.c_str());
					m_bEmptyStringOpened = false;
				}
				else
				{
					// if bOpenWindow is false then reset ipSRIR to NULL
					// [P16 #2635] - JDS
					ipSRIR = NULL;
				}
			}
			else
			{
				// get the SRIR with the current attributes image 
				ipSRIR = getImageViewer(strImage, bOpenWindow);
			}

			if (ipSRIR)
			{
				// Turn off auto fitting
				ipSRIR->FittingMode = 0;

				// highlight the spatial string in the SRIR window
				ipSRIR->CreateTemporaryHighlight(iter->second);

				// set the current image with highlights to current attributes image
				m_vecstrCurrentImageWithHighlights.push_back(strImage);

				// if we have a handle to the SRW then we need to make sure we have a paragraph text handler
				// attached to the window
				IIUnknownVectorPtr ipVecPTHs = ipSRIR->GetParagraphTextHandlers();
				if (ipVecPTHs == NULL)
				{
					ipVecPTHs.CreateInstance(CLSID_IUnknownVector);
					ASSERT_RESOURCE_ALLOCATION("ELI18084", ipVecPTHs != NULL);
				}
				if (ipVecPTHs->Size() == 0)
				{
					// this dialog is a paragraph text handler
					IParagraphTextHandler *pPTH = this;
					ipVecPTHs->PushBack(pPTH);
					ipSRIR->SetParagraphTextHandlers(ipVecPTHs);
				}			

				// bring the window with the highlights to the top
				::BringWindowToTop(getWindowHandleFromSRIR(ipSRIR));
				::BringWindowToTop(this->m_hWnd);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18085");
}
//-------------------------------------------------------------------------------------------------
string CEAVGeneratorDlg::getSourceImageName(ISpatialStringPtr ipValue)
{
	ASSERT_ARGUMENT("ELI18324", ipValue != NULL);

	string strImageName = "";

	// For each document name we are looking for, first check if there is an open SRW
	// with that name [FlexIDSCore #2648]

	// get the source doc name
	string strSourceDocName = asString(ipValue->SourceDocName);

	// first check to see if the image file is in the same directory as the voa file
	string strLocalImageName = getDirectoryFromFullPath(m_strCurrentFileName) +
		"\\" + getFileNameFromFullPath(strSourceDocName);
	if (getImageViewer(strLocalImageName, false) == NULL && !isValidFile(strLocalImageName))
	{
		// check the path stored in the attribute
		if(getImageViewer(strSourceDocName, false) == NULL && !isValidFile(strSourceDocName))
		{
			// check for the image from the open VOA file name
			string strImageNameFromVOAFileName1 = generateImageNameFromOpenVOAFile();
			if (getImageViewer(strImageNameFromVOAFileName1, false) == NULL
				&& !isValidFile(strImageNameFromVOAFileName1))
			{
				// as per P16 #2669 - drop extension before .voa
				// (e.g. 123.tif.testoutput -> 123.tif)
				string strImageNameFromVOAFileName2 = 
					getPathAndFileNameWithoutExtension(strImageNameFromVOAFileName1);

				if (getImageViewer(strImageNameFromVOAFileName2, false) == NULL 
					&& !isValidFile(strImageNameFromVOAFileName2))
				{
					// as per P16 #2649 - throw exception if the attribute is spatial
					// and the source image cannot be found
					if (ipValue->HasSpatialInfo() == VARIANT_TRUE)
					{
						UCLIDException ue("ELI19499", "Cannot locate source image!");
						ue.addDebugInfo("Local source", strLocalImageName);
						ue.addDebugInfo("Original source", strSourceDocName);
						ue.addDebugInfo("FileName from VOA #1", strImageNameFromVOAFileName1);
						ue.addDebugInfo("FileName from VOA #2", strImageNameFromVOAFileName2);
						throw ue;
					}
				}
				else
				{
					strImageName = strImageNameFromVOAFileName2;
				}
			}
			else
			{
				strImageName = strImageNameFromVOAFileName1;
			}
		}
		else
		{
			strImageName = strSourceDocName;
		}
	}
	else
	{
		strImageName = strLocalImageName;
	}

	return strImageName;
}
//-------------------------------------------------------------------------------------------------
HWND CEAVGeneratorDlg::getWindowHandleFromSRIR(ISpotRecognitionWindowPtr ipSRIR)
{
	// get the input receiver from the SRIR
	IInputReceiverPtr ipReceiver = ipSRIR;
	ASSERT_RESOURCE_ALLOCATION("ELI18176", ipReceiver != NULL);

	// get the HWND from the input receiver
	HWND hWndSRIR = (HWND)ipReceiver->WindowHandle;

	return hWndSRIR;
}
//-------------------------------------------------------------------------------------------------
string CEAVGeneratorDlg::generateImageNameFromOpenVOAFile()
{
	string strImageFile = "";

	// check for a currently open VOA file
	if (!m_strCurrentFileName.empty())
	{
		// build the file name: C:\test\123.tif.voa -> C:\test\123.tif
		strImageFile = getPathAndFileNameWithoutExtension(m_strCurrentFileName);
	}

	// return the file name
	return strImageFile;
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::openSRWWithImage(const std::string &strImageFile)
{
	try
	{
		// Delete any previously drawn highlights
		deleteTemporaryHighlights();

		// open an SRIR with the image file
		ISpotRecognitionWindowPtr ipSRIR = getImageViewer(strImageFile.c_str(), gbOPEN_SRW);
		if (ipSRIR)
		{
			// Turn off auto fitting
			ipSRIR->FittingMode = 0;

			// check if we opened an empty SRIR
			if (strImageFile.empty())
			{
				m_bEmptyStringOpened = true;
			}

			// get the current vector of paragraph handlers
			IIUnknownVectorPtr ipVecPTHs = ipSRIR->GetParagraphTextHandlers();

			// if no vector of paragraph handlers, then create one
			if (ipVecPTHs == NULL)
			{
				ipVecPTHs.CreateInstance(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI06206", ipVecPTHs != NULL);
			}

			// if the vector size is zero (we just created it),
			// create a paragraph handler, add it to the vector and set the
			// vector to be the paragraph handler for this instance of the SRIR
			if (ipVecPTHs->Size() == 0)
			{
				IParagraphTextHandler *pPTH = this;
				ipVecPTHs->PushBack(pPTH);
				ipSRIR->SetParagraphTextHandlers(ipVecPTHs);
			}

			// get the window handle and bring it to the front
			HWND hWndSRIR = getWindowHandleFromSRIR(ipSRIR);
			::BringWindowToTop(hWndSRIR);
			::BringWindowToTop(this->m_hWnd);
		}
		// could not open the SRW so throw exception
		else
		{
			UCLIDException ue("ELI18118", "Could not open Image Viewer.");
			ue.addDebugInfo("File Name", strImageFile);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17840");
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::clearListControl()
{
	INIT_EXCEPTION_AND_TRACING("MLI00016");

	try
	{
		int nCount = m_listAttributes.GetItemCount();
		_lastCodePos = "10";
		for (int i=0; i < nCount; i++)
		{
			IAttribute* pipAttribute = (IAttribute*) m_listAttributes.GetItemData(0);
			ASSERT_RESOURCE_ALLOCATION("ELI18181", pipAttribute != NULL);

			// release the attribute pointer
			pipAttribute->Release();

			m_listAttributes.DeleteItem(0);
			_lastCodePos = "10: " + asString(i+1) + " of " + asString(nCount);
		}
		_lastCodePos = "20";
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18209");
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::appendOrReplaceAttribute(IAttributePtr ipNewAttribute)
{
	ASSERT_ARGUMENT("ELI18728", ipNewAttribute);

	int nSelectedItemIndex = getCurrentSelectedListItemIndex();
	if (nSelectedItemIndex >= 0)
	{
		// get the current attribute data
		IAttribute* pipOldAttribute = (IAttribute*) m_listAttributes.GetItemData(nSelectedItemIndex);
		IAttributePtr ipOldAttribute(pipOldAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI18117", ipOldAttribute != NULL);

		// get the spatial string from the new attribute
		ISpatialStringPtr ipNewValue = ipNewAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI18735", ipNewValue != NULL);

		// get the new input text from the new spatial string
		string strInputText = asString(ipNewValue->String);

		// Check Replace/Append setting
		if (m_bReplaceValueText)
		{
			// get the old attribute name and check for empty string
			// if the old attribute had a non-empty name then set the new attribute
			// name to the old attribute and leave the list and edit box name fields unchanged
			// if the old attribute was the empty string then set the list and edit box name
			// fields to the new attribute name
			// added as per P16 #2632 - JDS 12/21/2007
			string strOldAttributeName = asString(ipOldAttribute->Name);
			if (!strOldAttributeName.empty())
			{
				ipNewAttribute->Name = strOldAttributeName.c_str();
			}
			else
			{
				string strAttributeName = asString(ipNewAttribute->Name);	
				m_listAttributes.SetItemText(nSelectedItemIndex, 
					NAME_COLUMN, strAttributeName.c_str());
				m_editName.SetWindowText(strAttributeName.c_str());
			}

			// replace the text in the edit boxes
			m_editValue.SetWindowText(strInputText.c_str());
			m_editType.SetWindowText("");

			// convert to normal string before put it in the grid
			::convertCppStringToNormalString(strInputText);
			m_listAttributes.SetItemText(nSelectedItemIndex, VALUE_COLUMN, strInputText.c_str());

			// replace the text in the attribute list
			m_listAttributes.SetItemText(nSelectedItemIndex, TYPE_COLUMN, "");

			// get the attribute from the smart pointer
			IAttribute* pipNewAttribute = ipNewAttribute.Detach();
			ASSERT_RESOURCE_ALLOCATION("ELI18270", pipNewAttribute != NULL);

			// since we are replacing then we need to replace the attribute in list item
			m_listAttributes.SetItemData(nSelectedItemIndex, 
				(DWORD_PTR)pipNewAttribute);

			// we also need to release the resources held by the old attribute in the list
			pipOldAttribute->Release();
		}
		// Append to existing text
		else
		{
			// get the spatial string from the old attribute
			ISpatialStringPtr ipOldValue = ipOldAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI18210", ipOldValue != NULL);

			// get the source doc names from the attributes
			string strSourceDocName = asString(ipNewValue->SourceDocName);
			string strOldSourceDocName = asString(ipOldValue->SourceDocName);

			if (strOldSourceDocName != strSourceDocName)
			{
				if (strOldSourceDocName != "")
				{
					UCLIDException ue("ELI18211", "Cannot append attributes from different documents!");
					ue.addDebugInfo("Original Document", strOldSourceDocName);
					ue.addDebugInfo("New Document", strSourceDocName);
					throw ue;
				}
				else
				{
					// the old source name was unknown - set the source doc name to be
					// the source document for this swipe
					ipOldValue->SourceDocName = strSourceDocName.c_str();
				}
			}

			// Retrieve current text
			CString	zText;
			m_editValue.GetWindowText(zText);

			// Append a space and then the new text
			zText += " ";
			zText += strInputText.c_str();

			// Update the edit box
			m_editValue.SetWindowText(zText);

			// convert to standard string
			string strText = (LPCTSTR)zText;

			// append the spatial string to the old spatial string
			ipOldValue->Append(ipNewValue);

			// convert to normal string before put it in the grid
			::convertCppStringToNormalString(strText);
			m_listAttributes.SetItemText(nSelectedItemIndex, VALUE_COLUMN, strText.c_str());

			// Set the appropriate radio button and reset the flag
			CheckRadioButton(IDC_RADIO_REPLACE, IDC_RADIO_APPEND, IDC_RADIO_REPLACE);
			GetDlgItem(IDC_RADIO_REPLACE)->SetFocus();
			m_bReplaceValueText = true;
		}
		
		setModified(true);
	}
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::enableOrDisableInputManager()
{
	IInputManagerPtr ipInputManager = getInputManager();
	ASSERT_RESOURCE_ALLOCATION("ELI18272", ipInputManager != NULL);

	// If exactly one item is selected then enable the input manager
	if (m_listAttributes.GetSelectedCount() == 1)
	{
		ipInputManager->EnableInput1(_bstr_t("Text"), "Select Text", NULL);
	}
	else
	{
		ipInputManager->DisableInput();
	}
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::openSRWFromVOAFileName()
{
	// generate a filename from the open VOA file name
	string strImageToOpen = generateImageNameFromOpenVOAFile();

	// make sure the string is not empty
	if (!strImageToOpen.empty())
	{
		// check if the file is valid or not
		if (!isValidFile(strImageToOpen))
		{
			// as per P16 #2669 - drop the extension before .voa
			strImageToOpen = getPathAndFileNameWithoutExtension(strImageToOpen);

			// ensure the filename is valid before attempting to open it
			if (isValidFile(strImageToOpen))
			{
				openSRWWithImage(strImageToOpen);
			}
		}
		else
		{
			openSRWWithImage(strImageToOpen);
		}
	}
}
//-------------------------------------------------------------------------------------------------
