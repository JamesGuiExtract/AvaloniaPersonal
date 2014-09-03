// FileProcessingDlgProcessingPage.cpp : implementation file
//

#include "stdafx.h"
#include "FileProcessingDlgProcessingPage.h"
#include "WorkItemsPage.h"
#include "afxdialogex.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <SuspendWindowUpdates.h>

// constants
const string gstrCOUNT_VARIABLE_TO_REPLACE = "<count>";
const string gstrFILE_TAB_TEXT = "Files (<count>)";
const string gstrWORK_ITEM_TAB_TEXT = "Work Items (<count>)";

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgProcessingPage dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(FileProcessingDlgProcessingPage, CPropertyPage)

FileProcessingDlgProcessingPage::FileProcessingDlgProcessingPage()
	: CPropertyPage(FileProcessingDlgProcessingPage::IDD), 
	m_bInitialized(false), 
	m_nNumCurrentProcessingFiles(0),
	m_nNumCurrentProcessingWorkItems(0)
{
}
//-------------------------------------------------------------------------------------------------
FileProcessingDlgProcessingPage::~FileProcessingDlgProcessingPage()
{
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
}

//-------------------------------------------------------------------------------------------------
// Message map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingDlgProcessingPage, CPropertyPage)
	ON_WM_SIZE()
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileProcessingDlgProcessingPage message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingDlgProcessingPage::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		try
		{
			// Call the base method
			CDialog::OnInitDialog();

			// Create the property pages for this page
			createPropertyPages();

			// Set the initialized flag to true
			m_bInitialized = true;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37267")
	}
	catch (UCLIDException &ue)
	{
		ue.log();
		if (!m_bInitialized)
		{
			CDialog::OnCancel();
		}
	}

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::OnSize(UINT nType, int cx, int cy) 
{
	try
	{
		AFX_MANAGE_STATE(AfxGetModuleState());
		TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

		CPropertyPage::OnSize(nType, cx, cy);

		// first call to this function shall be ignored
		if (!m_bInitialized) 
		{
			return;
		}

		// Resize the property sheet to the size of the entire page
		CRect rect;
		this->GetWindowRect(&rect);
		ScreenToClient(&rect);
		m_propSheet.resize(rect);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37223")
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::refresh()
{
	// refreshs the dispay of pages
	set<EProcessingTabPage> setPages;
	setPages.insert(kFilesPage);
	setPages.insert(kWorkItemsPage);
	updateTabs(setPages);
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::clear()
{
	m_propProcessLogPage.clear();
	m_propWorkItemsPage.clear();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::onStatusChange(const FileProcessingRecord *pTask, ERecordStatus eOldStatus)
{
	// Pass the call to the log page
	m_propProcessLogPage.onStatusChange(pTask, eOldStatus);

	// update the tab text
	long tmp = m_propProcessLogPage.getCurrentlyProcessingCount();
	if (tmp != m_nNumCurrentProcessingFiles)
	{
		m_nNumCurrentProcessingFiles = tmp;
		changeTabTextCount(kFilesPage, gstrFILE_TAB_TEXT, m_nNumCurrentProcessingFiles);
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::onWorkItemStatusChange(const FPWorkItem *pWorkItem, EWorkItemStatus eOldStatus)
{
	// Pass the call to the work items page
	m_propWorkItemsPage.onStatusChange(pWorkItem, eOldStatus);

	// update the tab text
	long tmp = m_propWorkItemsPage.getCurrentlyProcessingCount();
	if (tmp != m_nNumCurrentProcessingWorkItems)
	{
		m_nNumCurrentProcessingWorkItems = tmp;
		changeTabTextCount(kWorkItemsPage, gstrWORK_ITEM_TAB_TEXT, m_nNumCurrentProcessingWorkItems);
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::setRecordManager(FPRecordManager* pRecordMgr)
{
	// set the FPRecordManager for both pages
	m_propProcessLogPage.setRecordManager(pRecordMgr);
	m_propWorkItemsPage.setRecordManager(pRecordMgr);
}
//-------------------------------------------------------------------------------------------------
unsigned long FileProcessingDlgProcessingPage::getTotalProcTime()
{
	return m_propProcessLogPage.getTotalProcTime();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::setConfigMgr(FileProcessingConfigMgr* cfgMgr)
{
	m_propProcessLogPage.setConfigMgr(cfgMgr);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::setAutoScroll(bool bAutoScroll)
{
	m_propProcessLogPage.setAutoScroll(bAutoScroll);
	m_propWorkItemsPage.setAutoScroll(bAutoScroll);
}
//-------------------------------------------------------------------------------------------------
long FileProcessingDlgProcessingPage::getCurrentlyProcessingCount()
{
	return m_propProcessLogPage.getCurrentlyProcessingCount();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::getLocalStats(LONGLONG& nTotalBytes, long& nTotalDocs, long& nTotalPages)
{
	m_propProcessLogPage.getLocalStats(nTotalBytes, nTotalDocs, nTotalPages);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::ResetInitialized()
{
	// Remove the pages since they will be added again when this page is initialized
	m_propSheet.RemovePage(kWorkItemsPage);
	m_propSheet.RemovePage(kFilesPage);

	// Reinitialize the pages
	m_propProcessLogPage.ResetInitialized();
	m_propWorkItemsPage.ResetInitialized();
	m_bInitialized = false;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::startProgressUpdates()
{
	// Start progress updates for both tabs
	m_propProcessLogPage.startProgressUpdates();
	m_propWorkItemsPage.startProgressUpdates();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::stopProgressUpdates()
{
	// Stop progress updates for both tabs
	m_propProcessLogPage.stopProgressUpdates();
	m_propWorkItemsPage.stopProgressUpdates();
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::createPropertyPages()
{
	// Create the Database page others will be created as they are required
	m_propSheet.AddPage(&m_propProcessLogPage);
	m_propSheet.AddPage(&m_propWorkItemsPage);

	// Create the property sheet
	m_propSheet.Create(this, WS_CHILD | WS_VISIBLE, 0);
	m_propSheet.ModifyStyleEx(0, WS_EX_CONTROLPARENT);
	m_propSheet.SetWindowPos(NULL,0,0,0,0,SWP_NOZORDER | SWP_NOSIZE | SWP_NOACTIVATE);
	
	// Lock Window Updates
	SuspendWindowUpdates wndUpdatelocker(m_propSheet);

	// Need to make each of the pages active so that they will be created
	m_propSheet.SetActivePage(&m_propWorkItemsPage);
	
	// set active page back to the Database tab
	m_propSheet.SetActivePage(&m_propProcessLogPage);

	// Updated the text to the initial
	changeTabTextCount(kWorkItemsPage, gstrWORK_ITEM_TAB_TEXT, 0);
	changeTabTextCount(kFilesPage, gstrFILE_TAB_TEXT, 0);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::changeTabTextCount(int nTabIndex, string strTitleText, long nCount)
{
	// Replace the count variable in the title text
	replaceVariable(strTitleText, gstrCOUNT_VARIABLE_TO_REPLACE, asString(nCount));
	CTabCtrl * tabs;
    if((tabs=m_propSheet.GetTabControl())!=NULL) 
    {    //set the text on tabs
        TCHAR *stab2= (TCHAR*) strTitleText.c_str();

        TC_ITEM ti;
        ti.mask=TCIF_TEXT;
        ti.pszText=stab2;
        if( !tabs->SetItem(nTabIndex,&ti)) //set the text for 2nd entry
        {
            UCLIDException ue("ELI37351", "Unable to set the  tab text.");
			ue.addDebugInfo("Tab text", strTitleText);

			// No need to throw, just log
			ue.log();
        }
    }
}
//-------------------------------------------------------------------------------------------------
CPropertyPage * FileProcessingDlgProcessingPage::getPropertyPage(EProcessingTabPage ePage)
{
	switch(ePage)
	{
	case kFilesPage:
		return &m_propProcessLogPage;
	case kWorkItemsPage:
		return &m_propWorkItemsPage;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI37269");
		break;
	}
	THROW_LOGIC_ERROR_EXCEPTION("ELI37270");
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingDlgProcessingPage::isPageDisplayed(EProcessingTabPage ePage)
{
	long lPageIndex = m_propSheet.GetPageIndex(getPropertyPage(ePage));
	return lPageIndex >= 0;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::removePage(EProcessingTabPage ePage)
{
	// if the page is not displayed just return
	if (!isPageDisplayed(ePage))
	{
		return;
	}

	// Remove the page
	m_propSheet.RemovePage(getPropertyPage(ePage));

	// Call ResetInitialized for all pages the need it called
	switch(ePage)
	{
	case kFilesPage:
		m_propProcessLogPage.ResetInitialized();
		break;
	case kWorkItemsPage:
		m_propWorkItemsPage.ResetInitialized();
		break;

	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI37271");
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::displayPage(EProcessingTabPage ePage)
{
	if (isPageDisplayed(ePage))
	{
		// page is already displayed
		return;
	}

	// Add the page
	CPropertyPage *pPage = getPropertyPage(ePage);
	m_propSheet.AddPage(pPage);

	// Set the page to active so that it gets created.
	m_propSheet.SetActivePage(pPage);

}
//-------------------------------------------------------------------------------------------------
void FileProcessingDlgProcessingPage::updateTabs(const set<EProcessingTabPage>& setPages)
{
	// Save the active page so it can be restored later if possible
	CPropertyPage *pActivePage = m_propSheet.GetActivePage();
	
	// Lock Window Updates
	SuspendWindowUpdates wndUpdatelocker(m_propSheet);
	
	// set remove flag to false
	bool bRemove = false;
	
	// Remove only the pages that are not to be displayed or after the first page that should be
	// displayed but is currently not displayed - this is so it this is called while processing,
	// the pages don't lose the data currently displayed. When processing the only page that will
	// be added or removed is the statistics page which is the last page.
	for ( EProcessingTabPage ePage = kFilesPage; ePage <= kWorkItemsPage; ePage = (EProcessingTabPage)(ePage + 1))
	{
		bool bDisplayPage =  setPages.find(ePage) != setPages.end();

		bool bPageDisplayed = isPageDisplayed(ePage);

		if (!bRemove && bDisplayPage && !bPageDisplayed)
		{
			bRemove = true;
		}
		else if (bRemove || !bDisplayPage)
		{
			// remove the page
			removePage(ePage);
		}
	}

	// Add pages that should be displayedd
	for ( EProcessingTabPage ePage = kFilesPage; ePage <= kWorkItemsPage; ePage = (EProcessingTabPage)(ePage + 1))
	{
		// Check if the current page is in the set of pages to display
		if (setPages.find(ePage) != setPages.end())
		{
			// Display the page
			displayPage(ePage);
		}
	}

	// If there was an active page 
	if ( m_propSheet.GetPageIndex(pActivePage) >= 0 )
	{
		// set back to the active page
		m_propSheet.SetActivePage(pActivePage);
	}
	else if (isPageDisplayed(kFilesPage))
	{
		// Then set it back to the Action Page
		m_propSheet.SetActivePage(&m_propProcessLogPage);
	}
	else
	{
		// if the action page is not displayed set the database page as the active page
		m_propSheet.SetActivePage(&m_propWorkItemsPage);
	}
}
//-------------------------------------------------------------------------------------------------