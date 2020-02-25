// GrantorGranteeFinderV2PP.cpp : Implementation of CGrantorGranteeFinderV2PP
#include "stdafx.h"
#include "CountyCustomComponents.h"
#include "GrantorGranteeFinderV2PP.h"
#include "DatFileIterator.h"

#include <CommentedTextFileReader.h>
#include <common.h>
#include <comutils.h>
#include <cpputil.h>
#include <misc.h>
#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
	
#include <fstream>

using namespace std;

const int g_FindPartiesColumn = 1;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// GrantorGranteeFinder folder under ComponentData folder
const static string GRANTOR_GRANTEE_FINDER = "GrantorGranteeFinder";


//-------------------------------------------------------------------------------------------------
// CGrantorGranteeFinderV2PP
//-------------------------------------------------------------------------------------------------
CGrantorGranteeFinderV2PP::CGrantorGranteeFinderV2PP() 
: m_strCurrDocType ("")
{
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2PP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CGrantorGranteeFinderV2PP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_COUNTYCUSTOMCOMPONENTSLib::IGrantorGranteeFinderV2Ptr ipGGFinderV2(m_ppUnk[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI09311", ipGGFinderV2 != __nullptr);

			// whether or not to use specified entity types
			bool bSpecifiedEntityTypes = m_radioSpecified.GetCheck()==1;
			ipGGFinderV2->UseSelectedDatFiles = bSpecifiedEntityTypes ? VARIANT_TRUE : VARIANT_FALSE;
			if (bSpecifiedEntityTypes)
			{
				// Save selected dat files to Grantor Grantee finder v2
				saveSelDatFilesForDocType(ipGGFinderV2);
			}
		}

		m_bDirty = FALSE;
		// if we reached here, then the data was successfully transfered
		// from the UI to the object.
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09273")

	// if we reached here, it's because of an exception
	// An error message has already been displayed to the user.
	// Return S_FALSE to indicate to the outer scope that the
	// Apply was not successful.r
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CGrantorGranteeFinderV2PP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_COUNTYCUSTOMCOMPONENTSLib::IGrantorGranteeFinderV2Ptr ipGGFinderV2(m_ppUnk[0]);
		if (ipGGFinderV2)
		{	
			m_listFindParties = GetDlgItem( IDC_LIST_FINDPARTIES );
			
			// Enable full row selection plus grid lines and checkboxes
			m_listFindParties.SetExtendedListViewStyle(LVS_EX_GRIDLINES|LVS_EX_FULLROWSELECT|LVS_EX_CHECKBOXES);

			// Get value of UseSelectedDatFiles
			bool bUseSelectedDatFiles = ipGGFinderV2->UseSelectedDatFiles == VARIANT_TRUE;

			// Set up Colums for Parties list
			CRect rect;
			m_listFindParties.GetClientRect(&rect);
			m_listFindParties.InsertColumn(0, "Select", LVCFMT_LEFT, 25, 0);
			m_listFindParties.InsertColumn(g_FindPartiesColumn, "Find Parties", LVCFMT_LEFT, rect.Width()-25, g_FindPartiesColumn);
			
			
			// Setup All Radio button
			m_radioAll = GetDlgItem(IDC_ALL);
			// Set All radio button if UseSelected Dat files if false
			m_radioAll.SetCheck(!bUseSelectedDatFiles);

			// Setup Specified radio button
			m_radioSpecified = GetDlgItem(IDC_AS_SPECIFIED);
			// Set Specified if Use Selected is true
			m_radioSpecified.SetCheck(bUseSelectedDatFiles);

			// Setup Document type list
			m_listDocTypes = GetDlgItem(IDC_LIST_DOC_TYPES);

			// Set AF Utility to GrantorGrantee finders AF Utility object
			m_ipAFUtil = ipGGFinderV2->GetAFUtility();
			ASSERT_RESOURCE_ALLOCATION("ELI09281", m_ipAFUtil != __nullptr);
			
			// Load DocTypes
			loadDocTypeList();

			// Enable appropriate controls
			updateControls();

			// Load selected files from GGFinderV2
			loadSelDatFilesForDocType(ipGGFinderV2);
			// Loads map of doctype to all dat files for doc type
			loadDocTypeToVecFinderFile();
			if (!bUseSelectedDatFiles)
			{
				// if using All files set selected to map of all files
				m_mapSelDocTypeToVecFinderFile = m_mapDocTypeToVecFinderFile;
			}
		}
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09277");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CGrantorGranteeFinderV2PP::OnClickedRadioSpecified(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19341");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CGrantorGranteeFinderV2PP::OnClickedRadioAll(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09609");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CGrantorGranteeFinderV2PP::OnSelChangeListDocType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_listFindParties.EnableWindow(TRUE);
		int nCurrSel = m_listDocTypes.GetCurSel();
		loadFindPartiesList(m_vecDocTypes[nCurrSel]);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09613");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2PP::loadDocTypeList()
{
	m_listDocTypes.ResetContent();
	m_vecDocTypes.clear();

	// This object is obsolete so don't bother to update to correctly call this method
	IAFDocumentPtr dummy(CLSID_AFDocument);
	string strComponentDataFolder = m_ipAFUtil->GetComponentDataFolder(dummy);

	string strIndustrySpecificFolder = strComponentDataFolder 
								+ "\\" + DOC_CLASSIFIERS_FOLDER 
								+ "\\" + "County Document";
	// get doc type index file based on the industry category name
	string strDocTypeIndexFile = strIndustrySpecificFolder + "\\" + DOC_TYPE_INDEX_FILE;

	// open the file
	ifstream ifs(strDocTypeIndexFile.c_str());
	// use CommentedTextFileReader to read the file line by line
	CommentedTextFileReader fileReader(ifs, "//", true);
	string strLine("");
	while (!fileReader.reachedEndOfStream())
	{
		strLine = fileReader.getLineText();
		strLine = ::trim(strLine, " \t", " \t");
		if (strLine.empty())
		{
			continue;
		}
		
		// store each document type name
		m_vecDocTypes.push_back(strLine);
		m_listDocTypes.AddString(strLine.c_str());
	}
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2PP::loadFindPartiesList(const string& strDocType )
{
	getSelectedFromList();

	vector< string > vecDatFiles = m_mapDocTypeToVecFinderFile[strDocType];
	vector< string > vecPrevSelected;
	map< string, vector< string> > ::iterator iterCurr = m_mapSelDocTypeToVecFinderFile.find(strDocType);
	if ( iterCurr != m_mapSelDocTypeToVecFinderFile.end() )
	{
		vecPrevSelected = iterCurr->second;
	}
	
	m_listFindParties.DeleteAllItems();
	for (unsigned int n = 0; n < vecDatFiles.size(); n++)
	{
		string strDatFile = vecDatFiles[n];

		m_listFindParties.InsertItem(n, "");
		m_listFindParties.SetItemText(n, g_FindPartiesColumn, strDatFile.c_str());

		vector<string> :: iterator vecIter;
		for ( vecIter = vecPrevSelected.begin(); vecIter != vecPrevSelected.end(); vecIter++ )
		{
			string strValue = *vecIter;
			if( strValue == vecDatFiles[n] )
			{
				break;
			}
		}
		if ( vecIter != vecPrevSelected.end() )
		{
			m_listFindParties.SetCheckState( n, true);
		}
	}

	m_strCurrDocType = strDocType;
}
//-------------------------------------------------------------------------------------------------
string CGrantorGranteeFinderV2PP::getRulesFolder()
{
	// get component data folder
	// This object is obsolete so don't bother to update to correctly call this method
	IAFDocumentPtr dummy(CLSID_AFDocument);
	string strRulesFolder = m_ipAFUtil->GetComponentDataFolder(dummy);

	return strRulesFolder;
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2PP::updateControls()
{
	if (m_radioSpecified.GetCheck())
	{
		// Enable doc type combo and party list for selection
		m_listDocTypes.EnableWindow(TRUE);
		m_listFindParties.EnableWindow(TRUE);
	}
	else
	{
		// Disable doc type combo and party list
		m_listDocTypes.EnableWindow(FALSE);
		m_listFindParties.EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
vector<string> CGrantorGranteeFinderV2PP::getDatFileNamesForDocType(const string& strDocType)
{
	string strDatFileFolder = getRulesFolder() + "\\" + GRANTOR_GRANTEE_FINDER;

	// Search for dat files
	vector<string> vecDatFiles;
	DatFileIterator iter(strDatFileFolder, strDocType);
	while (iter.moveNext())
	{
		vecDatFiles.push_back(iter.getFileName());
	}

	return vecDatFiles;
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2PP::loadDocTypeToVecFinderFile()
{
	m_mapDocTypeToVecFinderFile.clear();
	if ( m_vecDocTypes.size() == 0 )
	{
		return;
	}
	vector<string>::iterator iterCurr;
	for (iterCurr = m_vecDocTypes.begin(); iterCurr != m_vecDocTypes.end(); iterCurr++ )
	{
		m_mapDocTypeToVecFinderFile[*iterCurr] = getDatFileNamesForDocType ( *iterCurr );
	}
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2PP::saveSelDatFilesForDocType(UCLID_COUNTYCUSTOMCOMPONENTSLib::IGrantorGranteeFinderV2Ptr ipGGFinderV2)
{
	IStrToObjectMapPtr ipDocTypeToFileMap = ipGGFinderV2->DocTypeToFileMap;
	ASSERT_RESOURCE_ALLOCATION("ELI09318", ipDocTypeToFileMap != __nullptr);

	ipDocTypeToFileMap->Clear();

	if (m_vecDocTypes.size() == 0)
	{
		ipGGFinderV2->DocTypeToFileMap = ipDocTypeToFileMap;
		return;
	}

	// Get the latest changes from the list control
	getSelectedFromList();
		
	vector<string>::iterator iterCurr = m_vecDocTypes.begin();
	for (; iterCurr != m_vecDocTypes.end(); iterCurr++)
	{
		vector<string>& vecSelected = m_mapSelDocTypeToVecFinderFile[*iterCurr];
		int nNumSelected = vecSelected.size();

		IVariantVectorPtr ipDatFileName(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI09320", ipDatFileName != __nullptr);

		for (int i = 0; i < nNumSelected; i++)
		{
			ipDatFileName->PushBack(_bstr_t(vecSelected[i].c_str()));
		}

		ipDocTypeToFileMap->Set(_bstr_t((*iterCurr).c_str()), ipDatFileName);
	}

	ipGGFinderV2->DocTypeToFileMap = ipDocTypeToFileMap;
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2PP::getSelectedFromList()
{
	// save checked items as selected dat files the list box
	if (!m_strCurrDocType.empty())
	{
		vector<string> vecSelected;
		vector<string> vecPrevFiles = m_mapDocTypeToVecFinderFile[m_strCurrDocType];
		int nItemCount = m_listFindParties.GetItemCount();
		for ( int i = 0; i < nItemCount; i++ )
		{
			if ( m_listFindParties.GetCheckState(i) )
			{
				string strItemText= vecPrevFiles[i];
				vecSelected.push_back( strItemText);
			}
		}
		m_mapSelDocTypeToVecFinderFile[m_strCurrDocType].clear();
		m_mapSelDocTypeToVecFinderFile[m_strCurrDocType] = vecSelected;
	}
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2PP::loadSelDatFilesForDocType( UCLID_COUNTYCUSTOMCOMPONENTSLib::IGrantorGranteeFinderV2Ptr ipGGFinderV2 )
{	
	IStrToObjectMapPtr ipDocTypeToFileMap = ipGGFinderV2->DocTypeToFileMap;
	ASSERT_RESOURCE_ALLOCATION("ELI09355", ipDocTypeToFileMap != __nullptr );

	//TODO: make sure the cases of either a new doc type or removed doc type 

	// iterate through the doc types
	vector<string>::iterator iterCurr = m_vecDocTypes.begin();
	for (; iterCurr != m_vecDocTypes.end(); iterCurr++)
	{
		if ( ipDocTypeToFileMap->Contains(_bstr_t((*iterCurr).c_str()))) 
		{
			vector<string> vecSelDatFiles;

			IVariantVectorPtr ipDatFiles = ipDocTypeToFileMap->GetValue(_bstr_t((*iterCurr).c_str()));

			int nNumDatFiles = ipDatFiles->Size;
			for ( int i = 0; i < nNumDatFiles; i++ )
			{
				string strSelDatFile = asString(_bstr_t(ipDatFiles->GetItem(i)));
				vecSelDatFiles.push_back(strSelDatFile);
			}
			m_mapSelDocTypeToVecFinderFile[*iterCurr].clear();
			m_mapSelDocTypeToVecFinderFile[*iterCurr] = vecSelDatFiles;
		}
	}
}
//-------------------------------------------------------------------------------------------------
