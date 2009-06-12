#include "stdafx.h"
#include "LaserficheUtils.h"
#include "LFItemCollection.h"
#include "LFMiscUtils.h"

#include <UCLIDException.h>
#include <ComUtils.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static const long gnSEARCH_RESULTS_TO_PRELOAD	= 100;

//--------------------------------------------------------------------------------------------------
// CLFItem
//--------------------------------------------------------------------------------------------------
CLFItem::CLFItem()
	: m_ipItem(NULL)
	, m_nID(-1)
{
}
//--------------------------------------------------------------------------------------------------
CLFItem::CLFItem(long nID)
	: m_ipItem(NULL)
	, m_nID(nID)
{
}
//--------------------------------------------------------------------------------------------------
CLFItem::~CLFItem()
{
	try
	{
		if (m_ipItem != NULL)
		{
			m_ipItem->Dispose();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21752");
}
//--------------------------------------------------------------------------------------------------
ILFEntryPtr CLFItem::getItem(ILFDatabasePtr ipDatabase)
{
	if (m_nID == -1)
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI21758");
	}

	if (m_ipItem == NULL)
	{
		ASSERT_ARGUMENT("ELI21757", ipDatabase != NULL);

		m_ipItem = ipDatabase->GetEntryByID(m_nID);
		ASSERT_RESOURCE_ALLOCATION("ELI21754", m_ipItem != NULL);
	}

	return m_ipItem;
}
//--------------------------------------------------------------------------------------------------
void CLFItem::unloadItem()
{
	if (m_ipItem != NULL)
	{
		try
		{
			m_ipItem->Dispose();
		}
		catch (...)
		{
			// If we failed to dispose of m_ipItem, still try to set it to NULL.
			m_ipItem = NULL;
			throw;
		}

		m_ipItem = NULL;
	}
}

//--------------------------------------------------------------------------------------------------
// CLFItemCollection
//--------------------------------------------------------------------------------------------------
CLFItemCollection::CLFItemCollection(ILFDatabasePtr ipDatabase)
: m_nResultPos(1)
, m_ipClient(NULL)
, m_ipSearch(NULL)
, m_ipResults(NULL)
, m_nAvailableResults(0)
, m_bSearchComplete(false)
, m_eMode(kNotStarted)
{
	try
	{
		m_ipDatabase = ipDatabase;
		ASSERT_RESOURCE_ALLOCATION("ELI21256", m_ipDatabase != NULL);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21255");
}
//--------------------------------------------------------------------------------------------------
CLFItemCollection::CLFItemCollection(ILFClientPtr ipClient, ILFDatabasePtr ipDatabase)
: m_nResultPos(1)
, m_ipSearch(NULL)
, m_ipResults(NULL)
, m_eMode(kNotStarted)
{
	try
	{
		m_ipClient = ipClient;
		ASSERT_RESOURCE_ALLOCATION("ELI21292", m_ipClient != NULL);

		m_ipDatabase = ipDatabase;
		ASSERT_RESOURCE_ALLOCATION("ELI21717", m_ipDatabase != NULL);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21294");
}
//--------------------------------------------------------------------------------------------------
CLFItemCollection::CLFItemCollection(CLFItemCollection &sourceCollection)
	: m_nResultPos(sourceCollection.m_nResultPos)
	, m_ipDatabase(sourceCollection.m_ipDatabase)
	, m_eMode(kSelected)
{
	try
	{
		sourceCollection.getResults(m_vecSelected);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21745");
}
//--------------------------------------------------------------------------------------------------
CLFItemCollection::~CLFItemCollection()
{
	try
	{
		reset();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21258");
}
//--------------------------------------------------------------------------------------------------
void CLFItemCollection::reset()
{
	CSingleLock lock(&m_mutex, TRUE);

	if (m_ipSearch != NULL)
	{
		m_ipSearch->Dispose();
		m_ipSearch = NULL;
	}
	m_vecSelected.clear();
	m_ipResults = NULL;
	m_nResultPos = 1;
	m_nAvailableResults = 0;
	m_bSearchComplete = false;
	m_eMode = kNotStarted;
}
//--------------------------------------------------------------------------------------------------
void CLFItemCollection::setDatabase(ILFDatabasePtr ipDatabase)
{
	ASSERT_ARGUMENT("ELI21747", ipDatabase != NULL);
	m_ipDatabase = ipDatabase;
}
//--------------------------------------------------------------------------------------------------
void CLFItemCollection::find(const string &strSearch)
{
	try
	{
		try
		{
			reset();

			CSingleLock lock(&m_mutex, TRUE);

			// Get a search object for the database
			m_ipSearch = m_ipDatabase->Search;
			ASSERT_RESOURCE_ALLOCATION("ELI21257", m_ipSearch != NULL);

			// Assign the search string and kick off the search
			m_ipSearch->Command = get_bstr_t(strSearch);
			m_ipSearch->BeginSearch(VARIANT_FALSE);

			m_eMode = kSearch;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23862")
	}
	catch (UCLIDException &ue)
	{
		ue.addDebugInfo("Search Query", strSearch);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void CLFItemCollection::find(ILFFolderPtr ipFolderToSearch, const string &strType, 
							 bool bRecursive, const string &strTag/* = ""*/)
{
	ASSERT_ARGUMENT("ELI23863", !strType.empty());

	if (ipFolderToSearch == NULL)
	{
		ipFolderToSearch = m_ipDatabase->RootFolder;
		ASSERT_RESOURCE_ALLOCATION("ELI21259", ipFolderToSearch != NULL);
	}

	// Generate the search string...

	// [FlexIDSIntegrations:37]
	// Use LF:Name * only for the use of the type parameter since this is the only way to limit
	// the search by entry type.
	string strSearch = "{LF:Name=\"*\", Type=" + strType + "}";

	// Apply path filter
	string strPath = asString(ipFolderToSearch->FindFullPath());
	if (strPath == "")
	{
		strPath = "\\";
	}

	strSearch += " & {LF:Lookin=\"" + strPath + "\"";
	if (bRecursive)
	{
		strSearch += ", Subfolders=Y";
	}
	else
	{
		strSearch += ", Subfolders=N";
	}
	strSearch += "}";

	// Apply tag filter if specified.
	if (strTag != "")
	{
		if (strTag[0] == '!')
		{
			strSearch += " - {LF:Tags=\"" + strTag.substr(1) + "\"}";
		}
		else
		{
			strSearch += " & {LF:Tags=\"" + strTag + "\"}";
		}
	}

	// Start the search using the compiled search string
	find(strSearch);
}
//--------------------------------------------------------------------------------------------------
void CLFItemCollection::findSelectedDocuments(const string &strType, const string &strTag/* = ""*/)
{
	CSingleLock lock(&m_mutex, TRUE);

	// The following variables are outside the try scope in order to clean up in the case of an
	// exception.
	ILFEntryPtr ipItem = NULL;
	variant_t varSelectedIDs;
	m_vecSelected.clear();

	try
	{
		if (m_ipClient == NULL)
		{
			throw UCLIDException("ELI21293", "Client not set!");
		}

		// Retrieve an array of selected IDs
		varSelectedIDs = m_ipClient->GetSelectedIds();
		if (varSelectedIDs.vt != (VT_ARRAY|VT_VARIANT))
		{
			UCLIDException ue("ELI20599", "Internal error: Unexpected selected document data type!");
			throw ue;
		}

		// Access the array
		_variant_t *pVarIDs = NULL;
		if (FAILED(SafeArrayAccessData(varSelectedIDs.parray, (void **) &pVarIDs)))
		{
			UCLIDException ue("ELI20601", "Internal error: Unable to access selected document list!");
			throw ue;
		}

		// Next a try block to properly unaccess the safe array in the case of an exception
		// extracting the data.
		try
		{
			// Loop through the array adding the ID of each document to the working set of 
			// selected documents and appending the search results of all documents in folder items.
			ASSERT_RESOURCE_ALLOCATION("ELI21013", varSelectedIDs.parray->rgsabound != NULL);
			
			ULONG nCount = varSelectedIDs.parray->rgsabound->cElements;
			m_vecSelected.reserve(nCount);
			for (ULONG i = 0; i < nCount; i++)
			{	
				long nID = (long) pVarIDs[i];

				ipItem = m_ipDatabase->GetEntryByID(nID);
				ASSERT_RESOURCE_ALLOCATION("ELI21264", ipItem != NULL);

				// If this item is a folder, search the folder for all documents it contains and
				// add those to m_vecSelected instead of the folder itself.
				if (ipItem->EntryType == ENTRY_TYPE_FOLDER)
				{
					ILFFolderPtr ipFolder = ipItem;
					ASSERT_RESOURCE_ALLOCATION("ELI21265", ipFolder != NULL);

					CLFItemCollection subFolderItems(m_ipDatabase);
					subFolderItems.find(ipFolder, strType, true, strTag);

					// Add the subfolder items to the end of m_vecSelected.
					subFolderItems.getResults(m_vecSelected);
				}
				// If this item is a shortcut, ignore it.  This prevents duplicate results
				// (FlexIDSIntegrations:14) and inconsistencies in including shortcuts since
				// Laserfiche's search does not find shortcuts but instead only the documents
				// themselves.
				else if (ipItem->EntryType == ENTRY_TYPE_DOCUMENT)
				{
					if (ipItem->IsShortcut == VARIANT_FALSE)
					{
						ILFDocumentPtr ipDocument = ipItem;
						ASSERT_RESOURCE_ALLOCATION("ELI21266", ipDocument != NULL);

						// If a tag filter has been specified, test to see if the document qualifies.
						if(strTag.empty())
						{
							// No tag filter; add the document
							m_vecSelected.push_back(CLFItem(nID));
						}
						else if (strTag[0] == '!')
						{
							// An exclusive tag filter; add the document only if it does not
							// contain the tag.
							if (!hasTag(ipDocument, strTag.substr(1)))
							{
								m_vecSelected.push_back(CLFItem(nID));
							}
						}
						else if (hasTag(ipDocument, strTag))
						{
							// An inclusive tag filter; add the document if it contains the tag.
							m_vecSelected.push_back(CLFItem(nID));
						}
					}
				}

				ipItem->Dispose();
			}
		}
		catch (...)
		{
			SafeArrayUnaccessData(varSelectedIDs.parray);
			throw;
		}

		SafeArrayUnaccessData(varSelectedIDs.parray);
	}
	catch (...)
	{
		// safeDispose is triggering an unresolved external issue here so it can't be used.
		try
		{
			if (ipItem != NULL)
			{
				ipItem->Dispose();
			}
		}
		catch (...) {}

		throw;
	}

	m_eMode = kSelected;
}
//--------------------------------------------------------------------------------------------------
IUnknownPtr CLFItemCollection::getNextItem()
{
	CSingleLock lock(&m_mutex, TRUE);

	long nID;

	// Declared outside the try scope so that it can be disposed of in case of an exception.
	ILFEntryPtr ipItem = NULL;
	IUnknownPtr ipResult = NULL;

	try
	{
		// Loop through results until we find a result that is not a shortcut
		do 
		{
			ipResult = NULL;

			if (m_eMode == kNotStarted)
			{
				throw UCLIDException("ELI21260", 
					"Cannot access undefined Laserfiche item collection!");
			}

			// If the collection is defined by a search, obtain results from m_ipResults
			if (m_eMode == kSearch)
			{
				retrieveNextResults(false);

				if (m_nResultPos > m_ipResults->RowCount)
				{
					return NULL;
				}
				else
				{
					nID = (long) m_ipResults->GetDatum(m_nResultPos++, COLUMN_TYPE_ID);
					ipItem = m_ipDatabase->GetEntryByID(nID);
				}
			}
			// If the collection is defined by the current selection, obtain results from 
			// m_vecSelected
			else if (m_eMode == kSelected)
			{
				if ((size_t) m_nResultPos > m_vecSelected.size())
				{
					return NULL;
				}
				else
				{
					ipItem = m_vecSelected[(m_nResultPos++) - 1].getItem(m_ipDatabase);
				}
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI21262");
			}

			ASSERT_RESOURCE_ALLOCATION("ELI20603", ipItem != NULL);

			// Cast the result item to ILFDocumentPtr or ILFFolderPtr as appropriate.
			switch (ipItem->EntryType)
			{
				case ENTRY_TYPE_FOLDER:	
				{
					ILFFolderPtr ipFolder = ipItem;
					ASSERT_RESOURCE_ALLOCATION("ELI21001", ipFolder!=NULL);

					ipResult = ipFolder;
				}
				break;

				case ENTRY_TYPE_DOCUMENT:	
				{
					if (ipItem->IsShortcut == VARIANT_TRUE)
					{
						// Don't handle shortcuts (searches don't find them anyway).
						// Move on to the next document.
						ipItem->Dispose();
						continue;
					}
					else
					{
						ILFDocumentPtr ipDocument = ipItem;
						ASSERT_RESOURCE_ALLOCATION("ELI20996", ipDocument != NULL);

						ipResult = ipDocument;
					}
				}
				break;

				default:
				{
					UCLIDException ue("ELI20607", "Application trace: Unknown entry type!");
					ue.log();
				}
			}

		 // Unless we encountered a shortcut, we will not loop
		break;
		}
		while (true); 
	}
	catch (...)
	{
		// safeDispose is triggering an unresolved external issue here so it can't be used.
		try
		{
			if (ipItem != NULL)
			{
				ipItem->Dispose();
			}
		}
		catch (...) {}

		throw;
	}

	return ipResult;
}
//--------------------------------------------------------------------------------------------------
long CLFItemCollection::getCount()
{
	CSingleLock lock(&m_mutex, TRUE);

	// In kSearch mode, m_ipResults represents the collecion,
	// in kSelected mode, m_vecSelected represents the collection.
	if (m_eMode == kSearch)
	{
		m_ipSearch->UpdateSearchStatus();

		if (m_ipSearch->SearchStatus == SEARCH_STATUS_IN_PROGRESS)
		{
			return -1;
		}
		else
		{
			retrieveNextResults(true);

			long nCount = m_ipResults->RowCount;
			if (nCount < 0)
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI21729");
			}

			return m_ipResults->RowCount;
		}
	}
	else if (m_eMode == kSelected)
	{
		return (long) m_vecSelected.size();
	}
	else
	{
		throw UCLIDException("ELI21718", "Cannot access undefined Laserfiche item collection!");
	}
}
//--------------------------------------------------------------------------------------------------
long CLFItemCollection::getResults(vector<CLFItem> &vecResults)
{
	CSingleLock lock(&m_mutex, TRUE);
	
	long nCount = 0;

	// In kSearch mode, m_ipResults represents the collecion,
	// in kSelected mode, m_vecSelected represents the collection.
	if (m_eMode == kSelected)
	{
		nCount = (long) m_vecSelected.size();
		vecResults = m_vecSelected;
	}
	else if (m_eMode == kSearch)
	{
		waitForSearchToComplete();

		nCount = getCount();
		vecResults.reserve(vecResults.capacity() + nCount);
		for (int i = 0; i < nCount; i++)
		{
			long nID = (long) m_ipResults->GetDatum(i + 1, COLUMN_TYPE_ID);
			vecResults.push_back(CLFItem(nID));
		}
	}
	else
	{
		throw UCLIDException("ELI21300", "Cannot access undefined Laserfiche item collection!");
	}

	return nCount;
}
//--------------------------------------------------------------------------------------------------
long CLFItemCollection::getResults(list<CLFItem> &listResults)
{
	vector<CLFItem> vecResults;
	long nCount = getResults(vecResults);

	listResults.insert(listResults.end(), vecResults.begin(), vecResults.end());

	return nCount;
}
//--------------------------------------------------------------------------------------------------
void CLFItemCollection::waitForSearchToComplete()
{
	if (m_eMode == kSearch)
	{
		while (m_ipSearch->SearchStatus == SEARCH_STATUS_IN_PROGRESS)
		{
			Sleep(1000);
			m_ipSearch->UpdateSearchStatus();
		}
	}
}
//--------------------------------------------------------------------------------------------------
long CLFItemCollection::getSelectedDocuments(ILFClientPtr ipClient, ILFDatabasePtr ipDatabase,
											 const string &strTag, const string &strType,
											 list<CLFItem> &listResults)
{
	CLFItemCollection selectedDocumentSearch(ipClient, ipDatabase);
	selectedDocumentSearch.findSelectedDocuments(strType, strTag);

	return selectedDocumentSearch.getResults(listResults);
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CLFItemCollection::retrieveNextResults(bool bRetrieveAllResults)
{
	while (!m_bSearchComplete &&
		   (bRetrieveAllResults || m_nResultPos > m_nAvailableResults))
	{
		while (m_ipSearch->UpdateSearchStatus() == SEARCH_STATUS_IN_PROGRESS && 
			m_nResultPos > m_ipSearch->NumObjectsFound)
		{
			Sleep(1000);
		}

		if (m_ipResults == NULL)
		{
			// Initialize the search results
			ILFSearchListingParamsPtr ipSearchListingParams(CLSID_LFSearchListingParams);
			ASSERT_RESOURCE_ALLOCATION("ELI21250", ipSearchListingParams != NULL);

			ipSearchListingParams->AddStandardColumn(COLUMN_TYPE_ID);

			m_ipResults = m_ipSearch->GetSearchResultListing(ipSearchListingParams, gnSEARCH_RESULTS_TO_PRELOAD);
			ASSERT_RESOURCE_ALLOCATION("ELI21251", m_ipResults != NULL);
		}
		else
		{
			m_ipResults->PreloadData(m_nResultPos, gnSEARCH_RESULTS_TO_PRELOAD);
		}

		m_bSearchComplete = (m_ipSearch->SearchStatus != SEARCH_STATUS_IN_PROGRESS &&
						     asCppBool(m_ipResults->IsCompleteResults));

		m_nAvailableResults = m_ipResults->RowCount;
	}
}
//--------------------------------------------------------------------------------------------------