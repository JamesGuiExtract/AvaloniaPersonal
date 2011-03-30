// CachedListLoader.cpp : Implementation of CCachedListLoader

#include "stdafx.h"
#include "CachedListLoader.h"
#include "Common.h"

#include <StringTokenizer.h>
#include <UCLIDException.h>
#include <ComUtils.h>

//-------------------------------------------------------------------------------------------------
// Statics
//-------------------------------------------------------------------------------------------------
map<string, CachedObjectFromFile<IVariantVectorPtr, StringLoader> >
	CCachedListLoader::ms_mapCachedLists;
map<string, long> CCachedListLoader::ms_mapReferenceCounts;
CMutex CCachedListLoader::ms_Mutex;

//-------------------------------------------------------------------------------------------------
// CCachedListLoader
//-------------------------------------------------------------------------------------------------
CCachedListLoader::CCachedListLoader()
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI30042", m_ipMiscUtils != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30043");
}
//-------------------------------------------------------------------------------------------------
CCachedListLoader::~CCachedListLoader()
{
	try
	{
		m_ipMiscUtils = __nullptr;

		CSingleLock lg(&ms_Mutex, TRUE);

		// For each list that is referenced by this instance, decrement the reference count for the
		// list as this instance is destructed.
		map<string, CachedObjectFromFile<IVariantVectorPtr, StringLoader>* >::iterator iter;
		for (iter = m_mapReferencedLists.begin(); iter != m_mapReferencedLists.end(); iter++)
		{
			ms_mapReferenceCounts[iter->first]--;
			
			// If there are no more references to the list, free the list.
			if (ms_mapReferenceCounts[iter->first] == 0)
			{
				ms_mapCachedLists.erase(iter->first);
				ms_mapReferenceCounts.erase(iter->first);
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI30044");
}
//--------------------------------------------------------------------------------------------------
IVariantVectorPtr CCachedListLoader::getList(const _bstr_t& bstrListSpecification,
											 IAFDocumentPtr ipAFDoc/* = NULL*/,
											 char* pcDelimeter/* = NULL*/)
{
	try
	{
		try
		{
			IVariantVectorPtr ipList(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI30045", ipList != __nullptr);

			// Remove the header of the string if it is a file name,
			// return the original string if it is not a file name
			_bstr_t bstrAfterRemoveHeader =
				m_ipMiscUtils->GetFileNameWithoutHeader(bstrListSpecification);

			// Compare the new string with the original string; if they are the same, this is not a file
			// specification.
			if (bstrAfterRemoveHeader == bstrListSpecification)
			{
				return NULL;
			}
			else
			{
				string strAfterRemoveHeader = asString(bstrAfterRemoveHeader);
				
				if (pcDelimeter != __nullptr)
				{
					int nIndexEndFileName = strAfterRemoveHeader.find_first_of(*pcDelimeter);

					if (nIndexEndFileName == string::npos)
					{
						UCLIDException ue("ELI30046",
							"No delimeter character was found in the file specifcation.");
						ue.addDebugInfo("File specification", asString(bstrListSpecification));
						ue.addDebugInfo("Expected delimeter", *pcDelimeter);
						throw ue;
					}

					// Get the delimiter string
					string strDelimiter = strAfterRemoveHeader.substr(nIndexEndFileName + 1);
					strDelimiter = trim(strDelimiter, " ", "");

					// Check if the delimiter is more than one character
					if (strDelimiter.length() != 1)
					{
						UCLIDException ue("ELI30047", "File delimiter shall be one character long.");
						ue.addDebugInfo("Delimiter", strDelimiter);
						throw ue;
					}
					
					*pcDelimeter = strDelimiter[0];
					strAfterRemoveHeader = strAfterRemoveHeader.substr(0, nIndexEndFileName);
				}

				// Expand tags and functions in the file name
				if (ipAFDoc != __nullptr)
				{
					strAfterRemoveHeader =
						m_tagManager.expandTagsAndFunctions(strAfterRemoveHeader, ipAFDoc);
				}

				// Retrieve the list
				return getCachedList(strAfterRemoveHeader);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30049");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uexOuter("ELI30050", "Error loading list from file.", ue);
		uexOuter.addDebugInfo("File specification", asString(bstrListSpecification));
		throw uexOuter;
	}
}
//--------------------------------------------------------------------------------------------------
IVariantVectorPtr CCachedListLoader::expandList(IVariantVectorPtr ipSourceList, 
												IAFDocumentPtr ipAFDoc/* = NULL*/)
{
	_bstr_t bstrCurrentEntry = "";

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI30051", ipSourceList != __nullptr);

			IVariantVectorPtr ipExpandedList(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI30052", ipExpandedList != __nullptr);

			// Iterate each item in the list.
			long nSize = ipSourceList->Size;
			for (long i = 0; i < nSize; i++)
			{
				bstrCurrentEntry = _bstr_t(ipSourceList->Item[i]);

				// Attempt to load the entry as a list from file.
				IVariantVectorPtr ipFileValues = getList(bstrCurrentEntry, ipAFDoc);

				if (ipFileValues == __nullptr)
				{
					// Not a file; add this entry to the expanded list as is
					ipExpandedList->PushBack(bstrCurrentEntry);
				}
				else
				{
					// Add the loaded values to the expanded clue list
					ipExpandedList->Append(ipFileValues);
				}
			}

			return ipExpandedList;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30053");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uexOuter("ELI30054", "Failed to expand list.", ue);
		if (bstrCurrentEntry.length() != 0)
		{
			uexOuter.addDebugInfo("List item", asString(bstrCurrentEntry));
		}
		throw uexOuter;
	}
}
//--------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CCachedListLoader::expandTwoColumnList(IIUnknownVectorPtr ipSourceList,
														  char cDelimeter,
														  IAFDocumentPtr ipAFDoc/* = NULL*/)
{
	_bstr_t bstrCurrentEntry = "";

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI30055", ipSourceList != __nullptr);

			// Create IIUnknownVectorPtr object to put string pairs
			IIUnknownVectorPtr ipExpandedList(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI30056", ipExpandedList != __nullptr);

			set<_bstr_t> setStringKeys;

			// Iterate each item in the list.
			long nSize = ipSourceList->Size();
			for (long i = 0; i < nSize; i++)
			{
				// Get the string key value pair in the first row of the list box
				IStringPairPtr ipKeyValuePair(ipSourceList->At(i));
				ASSERT_RESOURCE_ALLOCATION("ELI30057", ipKeyValuePair != __nullptr);

				bstrCurrentEntry = ipKeyValuePair->StringKey;

				// Attempt to load the entry as a list from file.
				char cColumnDelimeter = cDelimeter;
				IVariantVectorPtr ipFileValues = getList(bstrCurrentEntry, ipAFDoc, &cColumnDelimeter);

				if (ipFileValues == __nullptr)
				{
					checkForDuplicateKey(setStringKeys, bstrCurrentEntry);

					// Not a file; add this KeyValuePair to the expanded list as is
					ipExpandedList->PushBack(ipKeyValuePair);
				}
				else
				{
					// Create a tokenizer
					StringTokenizer tokenizer(cColumnDelimeter);

					long nSize2 = ipFileValues->Size;
					for (long j = 0; j < nSize2; j++)
					{
						// Get one line of text of string
						string strLine = asString(ipFileValues->Item[j].bstrVal);

						// Tokenize the line text
						vector<string> vecTokens;
						tokenizer.parse(strLine, vecTokens);

						// If the size of the tokenized vector is not 2
						if (vecTokens.size() != 2)
						{
							UCLIDException ue("ELI30058",
								"This line of text can not be parsed successfully with provided delimiter.");
							ue.addDebugInfo("LineText", strLine);
							ue.addDebugInfo("Delimiter", cColumnDelimeter);
							throw ue;
						}
						
						// Create an IStringPairPtr object
						IStringPairPtr ipReplacement(CLSID_StringPair);
						ASSERT_RESOURCE_ALLOCATION("ELI30059", ipReplacement != __nullptr);

						_bstr_t bstrStringKey = get_bstr_t(vecTokens[0]);

						// Put the tokens into string pair object
						ipReplacement->StringKey = bstrStringKey;
						ipReplacement->StringValue = get_bstr_t(vecTokens[1]);

						checkForDuplicateKey(setStringKeys, bstrStringKey);

						// Push the string pair object into IUnknownVector
						ipExpandedList->PushBack(ipReplacement);
					}
				}
			}

			return ipExpandedList;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30060");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uexOuter("ELI30061", "Failed to expand two-column list.", ue);
		if (bstrCurrentEntry.length() != 0)
		{
			uexOuter.addDebugInfo("List item", asString(bstrCurrentEntry));
		}
		throw uexOuter;
	}
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
IVariantVectorPtr CCachedListLoader::getCachedList(const string& strFileName)
{
	string strFileNameUpper = strFileName;

	// Any separate instances that reference the same file name case-insensitively will share the
	// same cached instance.
	makeUpperCase(strFileNameUpper);

	CSingleLock lg(&ms_Mutex, TRUE);

	if (m_mapReferencedLists.find(strFileNameUpper) == m_mapReferencedLists.end())
	{
		// If this instance doesn't yet reference this file, check to see if the list has been
		// cached by any instance
		if (ms_mapCachedLists.find(strFileNameUpper) == ms_mapCachedLists.end())
		{
			// If the list hasn't been cached, cache it now.
			ms_mapCachedLists[strFileNameUpper] =
				CachedObjectFromFile<IVariantVectorPtr, StringLoader>(
					gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str());
			ms_mapCachedLists[strFileNameUpper].m_obj = NULL;
		}
		
		// Add a reference to the list.
		m_mapReferencedLists[strFileNameUpper] = &ms_mapCachedLists[strFileNameUpper];
		ms_mapReferenceCounts[strFileNameUpper]++;
	}

	ASSERT_RESOURCE_ALLOCATION("ELI30062", m_mapReferencedLists[strFileNameUpper] != __nullptr);

	// Load the list items
	m_mapReferencedLists[strFileNameUpper]->loadObjectFromFile(strFileName);

	IVariantVectorPtr ipList = m_mapReferencedLists[strFileNameUpper]->m_obj;
	ASSERT_RESOURCE_ALLOCATION("ELI30048", ipList != __nullptr);

	return ipList;
}
//--------------------------------------------------------------------------------------------------
void CCachedListLoader::checkForDuplicateKey(set<_bstr_t>& rsetStringKeys, _bstr_t bstrKey)
{
	if (rsetStringKeys.find(bstrKey) != rsetStringKeys.end())
	{
		UCLIDException ue("ELI30063", "Duplicate list entry found.");
		ue.addDebugInfo("Value", asString(bstrKey));
		throw ue;
	}

	rsetStringKeys.insert(bstrKey);
}
//--------------------------------------------------------------------------------------------------