//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LFItemCollection.h
//
// PURPOSE:	Class retrieve a specified collection of items (documents and/or folders) from a 
//			Laserfiche Repository
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================

#pragma once

#include "stdafx.h"
#include "LaserficheUtils.h"

#include <afxmt.h>

#include <string>
#include <vector>
#include <list>
#include <memory>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CLFItem
// 
// PURPOSE: This class serves as a wrapper to a Laserfiche Item (document or folder).  It allows for
// a document to be added to a list without loading the Laserfiche COM interface right away.
// This class can also be used to store information about the document that callers could access
// without making calls into the COM object. 
//--------------------------------------------------------------------------------------------------
class LASERFICHE_API CLFItem
{
public:
	// NOTE: Default constructor provided for compatibilty with stl containers only.  An instance
	// created with the default constructor will not be useable.
	CLFItem();
	CLFItem(long nID);
	// NOTE: The destructor will dispose of the COM object (if loaded).
	~CLFItem();

	// Returns the document ID
	long getID() { return m_nID; }

	// Returns a COM object pointer to the Laserfiche item.
	// Required: ipDatabase must correspond to the same database the item resides in.
	ILFEntryPtr getItem(ILFDatabasePtr ipDatabase);

	// Disposes of the COM object and releases all temporary locks on the item.
	void unloadItem();

private:

	//////////////////
	// Varibles
	//////////////////
	long m_nID;
	ILFEntryPtr m_ipItem;
};

//--------------------------------------------------------------------------------------------------
// CLFItemCollection
//--------------------------------------------------------------------------------------------------
class LASERFICHE_API CLFItemCollection
{
public:
	CLFItemCollection(ILFDatabasePtr ipDatabase);
	CLFItemCollection(ILFClientPtr ipClient, ILFDatabasePtr ipDatabase);
	// Using the constructor that takes a CLFItemCollection will create an instance of type
	// kSelected that contains all of the items in the source collection.  The new collection will
	// be detached from the Laserfiche search and will not contain any items that had not yet been
	// returned from a running source collection's search.
	CLFItemCollection(CLFItemCollection &sourceCollection);
	~CLFItemCollection();

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	Disposes of any open search object and resets all members.  Used to start another
	//			search with the same class instance or to free the members upon destruction
	void reset();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	Sets the database for the CItemCollection.  This is useful when associating an
	// existing collection with new database connection.
	void setDatabase(ILFDatabasePtr ipDatabase);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To initiate a search of a Laserfiche repository
	// ARGUMENTS:
	//			strSearch- The search string. See "Advanced Searches" in the Laserfiche Client 
	//				user's guide for search syntax.
	void find(const string &strSearch);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To initiate a search for all Laserfiche objects of the specified type with the 
	//			specified tag in the specified folder or repository.
	// ARGUMENTS:
	//			ipFolderToSearch- If specified, the search is conducted beneath the specified folder.
	//				If NULL, the entire repository is searched.
	//			strType- The type specifier to use for the search (can be combined, ie "BD"):
	//					  F = Folder
	//					  D = Document (applies only to documents with templates)
	//					  B = Batch (basically means documents without templates)
	//			bRecursive- true to search all child folders.  false to search only the root of  
	//				ipFolderToSearch
	//			strTag- The tag found documents are required to possess.
	void find(ILFFolderPtr ipFolderToSearch, const string &strType, bool bRecursive, 
			  const string &strTag = "");
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To initiate a search for all documents selected in the client.  This includes all
	//			documents beneath selected folders.
	// ARGUMENTS:
	//			strType- The type specifier to use for the search (can be combined, ie "BD"):
	//					  F = Folder
	//					  D = Document (applies only to documents with templates)
	//					  B = Batch (basically means documents without templates)
	//			strTag- If specified, only the selected documents that possess this tag will be
	//				retrieved.
	// REQUIRES: The instance must have been initiated with the constructor taking an ILFClientPtr.
	void findSelectedDocuments(const string &strType, const string &strTag = "");
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Retrieves the next search result
	// RETURNS: An IUnknownPtr to the next found object.  This may be an instance of either 
	//			ILFDocumentPtr or ILFFolderPtr depending upon the search.  The caller is required to
	//			determine the type or know which type to be expecting.  If there are no further 
	//			results, NULL is returned.
	IUnknownPtr getNextItem();
	//----------------------------------------------------------------------------------------------
	// RETURNS: Retrieves the count of the total items in the collection (including results already
	// returned).  -1 indicates the search is still in progress and the total number of results
	// is still unknown.
	long getCount();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Retrieves the results of the search or selection.
	// RETRUNS: A count of the number of items added to listResults
	// PARAMS:	listResults- A list of CLFItems for all documents that were found.  The COM object 
	//			member will not yet be loaded.
	long getResults(list<CLFItem> &listResults);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Retrieves the results of the search or selection.
	// RETRUNS: A count of the number of items added to vecResults
	// PARAMS:	vecResults- A vector of CLFItems for all documents that were found.  The COM object 
	//			member will not yet be loaded.
	// NOTE:	This version will append to the the existing vector and the vector capacity will be
	//			increased by the number of found results, regardless current capacity.
	long getResults(vector<CLFItem> &vecResults);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: This call doesn't complete until the active search completes.  If there is not an
	// active search, the call returns immediately.
	void waitForSearchToComplete();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To retrieve all selected objects in a client in one static call.  (No need to
	//			initiate an instance and a search).
	// RETURNS: A count of documents added to listResults.
	// ARGUMENTS:
	//			ipClient- The client instance to search.
	//			ipDatabase- The database associated with the client
	//			strTag- Can be empty or specify a single tag.
	//					If not empty, only selected documents that contain this tag will be returned.
	//				    If prefixed with "!", only documents that do not contain the following tag
	//					will be returned.
	//			strType- The type specifier to use for the search (can be combined, ie "BD"):
	//					  F = Folder
	//					  D = Document (applies only to documents with templates)
	//					  B = Batch (basically means documents without templates)
	//			listResults- All documents that are selected and meet the requirements of the strTag
	//				filter
	static long getSelectedDocuments(ILFClientPtr ipClient, ILFDatabasePtr ipDatabase, 
									 const string &strTag, const string &strType, 
									 list<CLFItem> &listResults);

private:

	//////////////////
	// Varibles
	//////////////////

	// Specifies whether a search has been initiated and of what type.
	enum EStatus
	{
		kNotStarted = 0,
		kSearch = 1,
		kSelected = 2
	} m_eMode;

	// Only allow an instance to be accessed by one thread at a time.
	CMutex m_mutex;

	// Laserfiche objects
	ILFClientPtr m_ipClient;
	ILFDatabasePtr m_ipDatabase;
	ILFSearchPtr m_ipSearch;
	ILFSearchResultListingPtr m_ipResults;

	// In kSearch mode, the number of results that are currently available (may not be all the 
	// eventual results)
	long m_nAvailableResults;

	// In kSearchMode, whether or not the search is complete.
	bool m_bSearchComplete;

	// The set of documents currently selected.  Used for mode kSelected only.
	vector<CLFItem> m_vecSelected;

	// The current position in m_ipResults.  Used for kSearch mode only.
	int m_nResultPos;

	//////////////////
	// Methods
	//////////////////

	// Prepares results of an active search for retrieval. The full set of results
	// may not be immediately available.  (This situation can be detected when getCount returns -1)
	// If bRetrieveAllResults is true, it will not return until it ensures all results from the
	// search have been retrieved.
	void retrieveNextResults(bool bRetrieveAllResults);
};