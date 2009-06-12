//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LFMiscUtils.cpp
//
// PURPOSE:	Utility functions for interacting with Laserfiche products
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================

#include "stdafx.h"
#include "LaserficheUtils.h"
#include "LFMiscUtils.h"
#include "LFItemCollection.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <ComUtils.h>
#include <afxmt.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static const size_t nMAX_WORD_DATA_CACHES = 64;
static const WPARAM nLF_REFESH_COMMAND	= 30313;

//--------------------------------------------------------------------------------------------------
// Helper functions
//--------------------------------------------------------------------------------------------------
// PURPOSE: A helper function for addTag, removeTag and hasTag.  Retrieves an LFTag object 
//			associated with the name specified by strTag and and LFTagData object for the tags 
//			associated with the specified ipObject.  ipObject can be either an LFFolder or LFDocument.
void getTagData(IUnknownPtr ipObject, const string &strTag, 
				ILFTagDataPtr &ripTagData, ILFTagPtr &ripTag)
{
	ILFDatabasePtr ipDatabase = NULL;

	// Attempt to retrieve a ILFDocument interface
	ILFDocumentPtr ipDocument = ipObject;

	if (ipDocument != NULL)
	{
		// We have a document
		ipDatabase = ipDocument->Database;
		ripTagData = ipDocument->TagData;
	}
	else
	{
		// Attempt to retrieve a ILFFolder interface
		ILFFolderPtr ipFolder = ipObject;

		if (ipFolder == NULL)
		{
			throw UCLIDException("ELI20820", "Internal error: addTag called on invalid object type!");
		}

		// We have a folder
		ipDatabase = ipFolder->Database;
		ripTagData = ipFolder->TagData;	
	}


	ASSERT_RESOURCE_ALLOCATION("ELI20821", ipDatabase != NULL);
	ASSERT_RESOURCE_ALLOCATION("ELI21003", ripTagData != NULL);

	ripTag = ipDatabase->GetTagByName(get_bstr_t(strTag));
	ASSERT_RESOURCE_ALLOCATION("ELI20819", ripTag != NULL);
}
//--------------------------------------------------------------------------------------------------
// [FlexIDSIntegrations:21]
// A helper function for addAnnotation which loads cached word data for the specified page to 
// prevent having to reprocess via COM the word data for each annotation on a page (which is slow).
vector<WordData> getCachedWordData(ILFPagePtr ipPage, ILFTextPtr ipText)
{
	ASSERT_ARGUMENT("ELI21326", ipPage != NULL);
	ASSERT_ARGUMENT("ELI21327", ipText != NULL);

	// Mutex protect the loading of cachedWordData from mapWordData.
	static CMutex mutexWordDataAccess;
	CSingleLock lock(&mutexWordDataAccess, TRUE);

	static map<long long, vector<WordData> > mapWordData;
	vector<WordData> cachedWordData;
	long long nPageID = ipPage->ID;

	if (mapWordData.find(nPageID) != mapWordData.end())
	{
		// This page is cached-- retrieve the cache data.
		cachedWordData = mapWordData[nPageID];
	}
	else
	{
		// This page is not in the cache, add it to the cache.
		// Also, add this page cache to the active cache list.
		static list<long long> listActiveCaches;
		listActiveCaches.push_back(nPageID);

		// Check to see if we have exceeded the max allowable caches. If so, delete the oldest.
		if (listActiveCaches.size() > nMAX_WORD_DATA_CACHES)
		{
			mapWordData.erase(listActiveCaches.front());
			listActiveCaches.pop_front();
		}

		// To keep track of the character position of each word on the document page
		long nPosition = 0;
		long nWordCount = ipText->WordCount;
		bool bMissingSpatialInfo = false;

		// Loop through each word and store its spatial, start and end information
		for (int i = 1; i <= nWordCount; i ++)
		{
			WordData wordData;

			ILFWordPtr ipWord = ipText->Word[i];
			ASSERT_RESOURCE_ALLOCATION("ELI20980", ipWord != NULL);
			
			long nWordLen = ipWord->Text.length();
			wordData.nWordStart = nPosition;
			wordData.nWordEnd = nPosition + nWordLen;

			wordData.rectWord.SetRect(ipWord->Left, ipWord->Top, ipWord->Right, ipWord->Bottom);

			if (wordData.rectWord == CRect(0,0,0,0))
			{
				if (i > 1 && bMissingSpatialInfo == false)
				{
					// If any word other than the first word (which is typically non-spatial) has zerod
					// coordinates, flag this page as missing spatial information
					bMissingSpatialInfo = true;
				}
			}
			else
			{
				// Only add word data to the vector if we found spatial information for the word
				cachedWordData.push_back(wordData);
			}

			// Update the current position by adding the word length plus the number of trailing 
			// delimiters.
			nPosition += nWordLen;
			nPosition += (long) ipWord->TrailingDelimiters.length();
		}

		if (bMissingSpatialInfo)
		{
			// [FlexIDSIntegrations:66]
			// If the document did not contain complete spatial information, log an exception
			ILFDocumentPtr ipDocument = ipPage->Document;
			ASSERT_RESOURCE_ALLOCATION("ELI21610", ipDocument != NULL);

			// Figure out which document page we're on
			ILFDocumentPagesPtr ipPages = ipDocument->GetPages();
			ASSERT_RESOURCE_ALLOCATION("ELI21407", ipPages != NULL);
			
			long nCount = ipPages->Count;
			long nIndex = 1;
			for (nIndex = 1; nIndex <= nCount; nIndex++)
			{
				ILFPagePtr ipDocumentPage = ipPages->Item[nIndex];
				ASSERT_RESOURCE_ALLOCATION("ELI21612", ipDocumentPage != NULL);

				if (ipDocumentPage->ID == ipPage->ID)
				{
					break;
				}
			}

			UCLIDException ue("ELI21611", "Warning: Page text may not be properly redacted! "
				"Page text is missing the spatial information necessary to ensure text is "
				"properly redacted.");

			ue.addDebugInfo("Document", asString(ipDocument->Name));
			ue.addDebugInfo("Page", nIndex);
			ue.log();
		}

		// Add the data to the cache
		mapWordData[nPageID] = cachedWordData;
	}

	return cachedWordData;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: A helper function for addAnnotation. Tests to see whether there is an existing 
//			annotation that contains all the rectangles in ipAnnotation and that contains a matching
//			text annotation for any text annotation ipAnnotation is linked to.
bool annotationExists(ILFImageBlockAnnotationPtr ipAnnotation,
					  const vector<ILFImageBlockAnnotationPtr> &vecExistingAnnotations)
{
	ASSERT_ARGUMENT("ELI21392", ipAnnotation != NULL);

	// Loop through each existing annotation to find a match
	for each (ILFImageBlockAnnotationPtr ipExistingAnnotation in vecExistingAnnotations)
	{
		ILFTextAnnotationPtr ipTextAnnotation = ipAnnotation->LinkTo;
		ILFTextAnnotationPtr ipExistingTextLink = ipExistingAnnotation->LinkTo;

		// If ipAnnotation has a linked text annontation, test to see that the existing one does.
		if (ipTextAnnotation != NULL)
		{
			if (ipExistingTextLink == NULL ||
				ipExistingTextLink->Start != ipTextAnnotation->Start ||
				ipExistingTextLink->End != ipTextAnnotation->End)
			{
				// The existing annotation doesn't have a matching text annotation.  This is
				// not a match.
				continue;
			}
		}

		long nRectCount = ipAnnotation->Count;
		vector<ILFRectanglePtr> vecExistingAnnotationRects;

		// If the new annotation has at least one rect, retrieve a vector of rectangles
		// associated with the existing rectangle
		if (nRectCount > 0)
		{
			long nExistingRectCount = ipExistingAnnotation->Count;
			for (long i = 1; i <= nExistingRectCount; i++)
			{
				ILFRectanglePtr ipExistingRect = ipExistingAnnotation->Item[i];
				ASSERT_RESOURCE_ALLOCATION("ELI21394", ipExistingRect != NULL);

				vecExistingAnnotationRects.push_back(ipExistingRect);
			}
		}

		// Loop through all rectangles in ipAnnotation and ensure that the existing annotation
		// has a matching rectangle for each.
		bool bAllRectsMatch = true;
		for (long i = 1; i <= nRectCount; i++)
		{
			ILFRectanglePtr ipRect = ipAnnotation->Item[i];
			ASSERT_RESOURCE_ALLOCATION("ELI21393", ipRect != NULL);
			bool bFoundMatchingRect = false;

			// Loop though each rect in the exiting annotation to find a match.
			for each (ILFRectanglePtr ipExistingRect in vecExistingAnnotationRects)
			{
				if (ipRect->Left == ipExistingRect->Left &&
					ipRect->Top == ipExistingRect->Top &&
					ipRect->Right == ipExistingRect->Right &&
					ipRect->Bottom == ipExistingRect->Bottom)
				{
					bFoundMatchingRect = true;
					break;
				}
			}

			// If we couldn't find a matching rectangle.  Set bAllRectsMatch to false and break
			// out of the loop
			if (!bFoundMatchingRect)
			{
				bAllRectsMatch = false;
				break;
			}
		}

		// If this existing annotation is a match, return true now.  Otherwise, keep searching.
		if (bAllRectsMatch)
		{
			return true;
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: A helper function for addAnnotation. Tests to see whether there is an existing 
//			text annotation that includes the portion of text specified by nStart and nEnd.
bool isTextAnnotated(ILFPagePtr ipPage, long nStart, long nEnd, bool bRedaction)
{
	ASSERT_ARGUMENT("ELI21468", ipPage != NULL);

	// Loop through all text annotation on the page looking for a match.
	long nTextAnnotationCount = bRedaction ? ipPage->TextBlackoutCount : ipPage->TextHighlightCount;
	for (long i = 1; i <= nTextAnnotationCount; i++)
	{
		ILFTextAnnotationPtr ipTextAnnotation = bRedaction ? ipPage->TextBlackout[i]
														   : ipPage->TextHighlight[i];
		ASSERT_RESOURCE_ALLOCATION("ELI21372", ipTextAnnotation != NULL);

		// If the annotation spans at least the specified range, return true.
		if (nStart >= ipTextAnnotation->Start && 
			nEnd <= ipTextAnnotation->End)
		{
			return true;
		}
	}

	// No matching text annotation was found.
	return false;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: A helper function for ensureCorrespondingImageAnnotations and 
//			addCorrespondingTextAnnotations.  Creates a text annotation at the specified position
//			on the specified page.
ILFTextAnnotationPtr getTextAnnotation(ILFPagePtr ipPage, long nStart, long nEnd, bool bRedaction)
{
	ASSERT_ARGUMENT("ELI21092", ipPage != NULL);

	// Create a text annotation
	ILFTextAnnotationPtr ipAnnotation = bRedaction ? ipPage->AddTextBlackout() 
												   : ipPage->AddTextHighlight();
	ASSERT_RESOURCE_ALLOCATION("ELI21097", ipAnnotation != NULL);

	// Assign the start & end of the annotation.
	ipAnnotation->Start = nStart;
	ipAnnotation->End = nEnd;

	return ipAnnotation;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE:	Create an image annotation using either ipBounds or ipTextAnnotation or both.  If
//			ipTextAnnotation, annotation rectangles will be created for each included word.  
ILFImageBlockAnnotationPtr LASERFICHE_API getImageAnnotation(ILFPagePtr ipPage, 
	bool bRedaction, ILongRectanglePtr ipBounds = NULL, ILFTextAnnotationPtr ipTextAnnotation = NULL)
{
	ASSERT_ARGUMENT("ELI21080", ipPage != NULL);
	ASSERT_ARGUMENT("ELI21398", ipBounds != NULL || ipTextAnnotation != NULL);

	ILFImageBlockAnnotationPtr ipAnnotation = NULL;
	
	if (ipTextAnnotation != NULL)
	{
		ipAnnotation = ipPage->AddLinkedAnnotation(ipTextAnnotation);
	}
	else
	{
		ipAnnotation = (bRedaction ? ipPage->AddImageBlackout() : ipPage->AddImageHighlight());
	}

	ASSERT_RESOURCE_ALLOCATION("ELI21496", ipAnnotation != NULL);

	if (ipBounds)
	{
		// Set the bounds of the annotation as specified.
		ILFRectanglePtr ipRect = ipAnnotation->AddRectangle();
		ASSERT_RESOURCE_ALLOCATION("ELI20514", ipRect != NULL);

		ipRect->Left = ipBounds->Left;
		ipRect->Top = ipBounds->Top;
		ipRect->Right = ipBounds->Right;
		ipRect->Bottom = ipBounds->Bottom;
	}

	return ipAnnotation;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: A helper function for ensureCorrespondingTextAnnotations.  Populates a map of existing
//			image annotations on the page with the annotation ID as the key and the 
//			ILFImageBlockAnnotationPtr as the value.  mapAnnotationsToIgnore contains a collection
//			of annotations that should be ignored & not added to the return value.
map<long long, ILFImageBlockAnnotationPtr> getAnnotationsOnPage(ILFPagePtr ipPage, bool bRedactions, 
							const map<long long, ILFImageBlockAnnotationPtr> &mapAnnotationsToIgnore)
{
	long nAnnotationCount = bRedactions ? ipPage->ImageBlackoutCount 
										: ipPage->ImageHighlightCount;
	map<long long, ILFImageBlockAnnotationPtr> mapImageAnnotations;
	
	// Loop through each annotation and add it to the return value if it isn't in the
	// mapAnnotationsToIgnore collection.
	for (long i = 1; i <= nAnnotationCount; i++)
	{
		ILFImageBlockAnnotationPtr ipImageAnnotation = bRedactions ? ipPage->ImageBlackout[i]
																  : ipPage->ImageHighlight[i];
		ASSERT_RESOURCE_ALLOCATION("ELI21365", ipImageAnnotation != NULL);

		long long nImageAnnotationID = getAnnotationID(ipPage, ipImageAnnotation);

		if (mapAnnotationsToIgnore.find(nImageAnnotationID) == mapAnnotationsToIgnore.end())
		{
			mapImageAnnotations[nImageAnnotationID] = ipImageAnnotation;
		}
	}

	return mapImageAnnotations;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: A helper function for ensureCorrespondingImageAnnotations.  Populates a map of existing
//			text annotations on the page with the annotation ID as the key and the 
//			ILFTextAnnotationPtr as the value.  mapAnnotationsToIgnore contains a collection
//			of annotations that should be ignored & not added to the return value.
map<long long, ILFTextAnnotationPtr> getAnnotationsOnPage(ILFPagePtr ipPage, bool bRedactions, 
								const map<long long, ILFTextAnnotationPtr> &mapAnnotationsToIgnore)
{
	long nAnnotationCount = bRedactions ? ipPage->TextBlackoutCount 
										: ipPage->TextHighlightCount;
	map<long long, ILFTextAnnotationPtr> mapTextAnnotations;

	// Loop through each annotation and add it to the return value if it isn't in the
	// mapAnnotationsToIgnore collection.
	for (long i = 1; i <= nAnnotationCount; i++)
	{
		ILFTextAnnotationPtr ipTextAnnotation = bRedactions ? ipPage->TextBlackout[i]
														    : ipPage->TextHighlight[i];
		ASSERT_RESOURCE_ALLOCATION("ELI21471", ipTextAnnotation != NULL);

		long long nTextAnnotationID = getAnnotationID(ipPage, ipTextAnnotation);

		if (mapAnnotationsToIgnore.find(nTextAnnotationID) == mapAnnotationsToIgnore.end())
		{
			mapTextAnnotations[nTextAnnotationID] = ipTextAnnotation;
		}
	}

	return mapTextAnnotations;
}

//--------------------------------------------------------------------------------------------------
// LASERFICHE_API exports
//--------------------------------------------------------------------------------------------------
void LASERFICHE_API getAvailableRepositories(vector<string> &rvecRepositories, 
											 map<string, string> &rmapServers)
{
	// Ensure the return variables are empty
	rvecRepositories.clear();
	rmapServers.clear();

	// Query LFApplication for the available repositories
	ILFApplicationPtr ipApplication(CLSID_LFApplication);
	ASSERT_RESOURCE_ALLOCATION("ELI20483", ipApplication != NULL);

	ILFCollectionPtr ipRepositoryCollection = ipApplication->GetAllDatabases();
	ASSERT_RESOURCE_ALLOCATION("ELI20735", ipRepositoryCollection != NULL);
	
	// Cycle through each repository and add the name and path of each to the return variables
	long nCount = ipRepositoryCollection->Count;
	for (long i = 1; i <= nCount; i++)
	{
		ILFDatabasePtr ipRepository = ipRepositoryCollection->Item[i];
		ASSERT_RESOURCE_ALLOCATION("ELI20736", ipRepository != NULL);

		ILFServerPtr ipServer = ipRepository->Server;
		ASSERT_RESOURCE_ALLOCATION("ELI20741", ipServer != NULL);

		string strServer = asString(ipServer->Name);
		string strRepository = asString(ipRepository->Name);
		rvecRepositories.push_back(strRepository);
		rmapServers[strRepository] = strServer;
	}
}
//--------------------------------------------------------------------------------------------------
ILFConnectionPtr LASERFICHE_API connectToRepository(const string &strServer, 
	const string &strRepository, const string &strUser, const string &strPassword)
{
	try
	{
		try
		{
			// User LFApplication to validate the server name before connecting.
			ILFApplicationPtr ipApplication(CLSID_LFApplication);
			ASSERT_RESOURCE_ALLOCATION("ELI20999", ipApplication != NULL);

			if (ipApplication->ValidateServerName(get_bstr_t(strServer)) != S_OK)
			{
				UCLIDException ue("ELI20487", "Failed to connect to Laserfiche Server!");
				ue.addDebugInfo("Server", strServer);
				throw ue;
			}

			// If the server is valid, get a pointer to it
			ILFServerPtr ipServer = ipApplication->GetServerByName(get_bstr_t(strServer));
			ASSERT_RESOURCE_ALLOCATION("ELI20484", ipServer != NULL);

			// Attempt to get the specified repository from the server
			ILFDatabasePtr ipDatabase = ipServer->GetDatabaseByName(get_bstr_t(strRepository));
			ASSERT_RESOURCE_ALLOCATION("ELI20485", ipDatabase != NULL);

			// Attempt a connection using the provided user credentials
			ILFConnectionPtr ipConnection(CLSID_LFConnection);
			ASSERT_RESOURCE_ALLOCATION("ELI20486", ipConnection != NULL);

			ipConnection->Shared = VARIANT_TRUE;
			ipConnection->UserName = get_bstr_t(strUser);
			ipConnection->Password = get_bstr_t(strPassword);

			ipConnection->Create(ipDatabase);

			return ipConnection;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20732");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uexOuter("ELI21000", "Failed to connect to Laserfiche Repository!", ue);
		uexOuter.addDebugInfo("Server", strServer);
		uexOuter.addDebugInfo("Repository", strRepository);
		uexOuter.addDebugInfo("User", strUser);
		throw uexOuter;
	}
}
//--------------------------------------------------------------------------------------------------
void LASERFICHE_API addTag(IUnknownPtr ipObject, const string &strTag, 
						   const string &strComment/* = ""*/)
{
	ILFTagDataPtr ipTagData = NULL;
	ILFTagPtr ipTag = NULL;

	try
	{
		// Get the object's TagData and the repository tag by this name.
		// getTagData asserts that ipTagData and ipTag are properly allocated
		getTagData(ipObject, strTag, ipTagData, ipTag);

		ipTagData->LockObject(LOCK_TYPE_WRITE);

		try
		{
			ipTagData->AddTag(ipTag, strComment.c_str());
		}
		catch (_com_error &e)
		{
			safeDispose(ipTagData);

			// Don't pass on exception if the code indicates the object already has this tag
			if (e.WCode() == gwLF_ERROR_TAG_ALREADY_EXISTS)
			{
				return;
			}
			else
			{
				throw e;
			}
		}

		ipTagData->Update();
		ipTagData->UnlockObject();
	}
	catch (...)
	{
		safeDispose(ipTagData);
		throw;
	}
}
//--------------------------------------------------------------------------------------------------
void LASERFICHE_API removeTag(IUnknownPtr ipObject, const string &strTag)
{
	ILFTagDataPtr ipTagData = NULL;
	ILFTagPtr ipTag = NULL;

	try
	{
		// Get the object's TagData and the repository tag by this name.
		// getTagData asserts that ipTagData and ipTag are properly allocated
		getTagData(ipObject, strTag, ipTagData, ipTag);

		ipTagData->LockObject(LOCK_TYPE_WRITE);

		try
		{
			ipTagData->RemoveTag(ipTag);
		}
		catch (_com_error &e) 
		{
			safeDispose(ipTagData);

			// If the error code code indicates document doesn't contain the tag, 
			// don't re-throw the exception
			if (e.WCode() == gwLF_ERROR_TAG_DOESNT_EXIST)
			{
				return;
			}
			else
			{
				throw e;
			}

		}

		ipTagData->Update();
		ipTagData->UnlockObject();
	}
	catch (...)
	{
		safeDispose(ipTagData);
		throw;
	}
}
//--------------------------------------------------------------------------------------------------
bool LASERFICHE_API hasTag(IUnknownPtr ipObject, const string &strTag)
{
	ILFTagDataPtr ipTagData = NULL;
	ILFTagPtr ipTag = NULL;

	try
	{
		// Get the object's TagData and the repository tag by this name.
		// getTagData asserts that ipTagData and ipTag are properly allocated
		getTagData(ipObject, strTag, ipTagData, ipTag);

		// Loop through each tag the object has looking for one whose ID matches that of the specified
		// tag.
		long nCount = ipTagData->Count;
		for (int i = 1; i <= nCount; i++)
		{
			ILFTagPtr ipExistingTag = ipTagData->Item[i];
			ASSERT_RESOURCE_ALLOCATION("ELI20867", ipExistingTag != NULL);

			if (ipExistingTag->ID == ipTag->ID)
			{
				return true;
			}
		}
	}
	catch (...)
	{
		safeDispose(ipTagData);
		throw;
	}

	// The tag wasn't found
	return false;
}
//--------------------------------------------------------------------------------------------------
void LASERFICHE_API verifyHasRight(IUnknownPtr ipObject, Access_Right right, 
								   const string &strExceptionText)
{
	ASSERT_ARGUMENT("ELI20859", ipObject != NULL);

	ILFEffectiveRightsPtr ipRights = NULL;

	// Attempt to retrieve ILFDocument interface
	ILFDocumentPtr ipDocument = ipObject;
	
	if (ipDocument != NULL)
	{
		ipRights = ipDocument->EffectiveRights;
		
	}
	else
	{
		// Attempt to retrieve ILFFolder interface
		ILFFolderPtr ipFolder = ipObject;

		if (ipFolder == NULL)
		{
			throw UCLIDException("ELI20863", 
				"Internal error: verifyHasRight called on invalid object type!");
		}

		ipRights = ipFolder->EffectiveRights;
	}

	ASSERT_RESOURCE_ALLOCATION("ELI20774", ipRights != NULL);

	// Throw an exception with the specified text if the desired right is not found
	if (ipRights->GetHasRight(right) == false)
	{
		throw UCLIDException("ELI20861", strExceptionText);
	}
}
//--------------------------------------------------------------------------------------------------
HWND LASERFICHE_API showDocument(ILFClientPtr ipClient, HWND hwndClient, string strClientVersion,
								 string strDocumentName, long nDocumentID, int nTimeout/* = 5*/)
{
	HWND hwndDocWindow = NULL;

	ASSERT_ARGUMENT("ELI20886", ipClient != NULL);

	// Determine the window title we should search for
	string strTitleToFind = strDocumentName + gzDOCUMENT_TITLE_SUFFIX;
	string strTitleOfCopy = strDocumentName + ":1" + gzDOCUMENT_TITLE_SUFFIX;

	// [FlexIDSIntegrations:2] Check to see if the window is already showing
	hwndDocWindow = ::FindWindow(gzDOCUMENT_CLASS, strTitleToFind.c_str());
	if (hwndDocWindow != NULL)
	{
		// [FlexIDSIntegrations:77] In case the document window was already open, refresh the document. 
		if (strClientVersion.substr(0, 1) == "8")
		{
			// Since the F5 key doesn't seem to work to refresh Laserfiche 8.  Use the LF8 menu 
			// command instead.
			PostMessage(hwndDocWindow, WM_COMMAND, nLF_REFESH_COMMAND, 0);
		}
		
		// Send an F5 key event to reload the latest version of the document.
		PostMessage(hwndDocWindow, WM_KEYDOWN, VK_F5, 0);
		PostMessage(hwndDocWindow, WM_KEYUP, VK_F5, 0);

		return hwndDocWindow;
	}
	else if (::FindWindow(gzDOCUMENT_CLASS, strTitleOfCopy.c_str()) != NULL)
	{
		// If there are multiple copies of the document open, we can't distinguish them.  Return NULL.
		return NULL;
	}

	// Launch the document in the client (0 = the first page)
	clock_t clkStart = clock();
	clock_t clkNow;
	bool bDocumentLaunched = false;

	// Enter loop to find the document window
	do
	{
		// If the Client has not yet reported the document to be opened, call ViewDocumentPage.
		if (!bDocumentLaunched)
		{
			// Laserfiche 8 will not succeed with the ViewDocumentPage call unless it is the
			// the foreground window.
			::SetForegroundWindow(hwndClient);			

			bDocumentLaunched = asCppBool(ipClient->ViewDocumentPage(nDocumentID, 0));
		}
		
		if (bDocumentLaunched)
		{
			// Attempt to find the window
			hwndDocWindow = ::FindWindow(gzDOCUMENT_CLASS, strTitleToFind.c_str());

			// If we found it, break out of the loop.
			if (hwndDocWindow != NULL)
			{
				break;
			}
		}

		// Loop about 5 times / sec
		Sleep(200);
		clkNow = clock();
	}
	// Keep searching until the specified timeout period has elapsed
	while (clkNow < clkStart + (nTimeout * CLOCKS_PER_SEC));

	return hwndDocWindow;
}
//--------------------------------------------------------------------------------------------------
long long LASERFICHE_API getAnnotationID(ILFPagePtr ipPage, ILFImageBlockAnnotationPtr ipAnnotation)
{
	ASSERT_ARGUMENT("ELI21901", ipPage != NULL);
	ASSERT_ARGUMENT("ELI21902", ipAnnotation != NULL);

	// Allot the page ID 48 bits (overflow at 281 quadrillion) and the annotation ID 16 bits (65K)
	return (long long)(ipPage->ID << 16) | (long long)((0xffff) & ipAnnotation->ID);
}
//--------------------------------------------------------------------------------------------------
long long LASERFICHE_API getAnnotationID(ILFPagePtr ipPage, ILFTextAnnotationPtr ipAnnotation)
{
	ASSERT_ARGUMENT("ELI21903", ipPage != NULL);
	ASSERT_ARGUMENT("ELI21904", ipAnnotation != NULL);

	// Allot the page ID 48 bits (overflow at 281 quadrillion) and the annotation ID 16 bits (65K)
	return (long long)(ipPage->ID << 16) | (long long)((0xffff) & ipAnnotation->ID);
}
//--------------------------------------------------------------------------------------------------
vector<ILFTextAnnotationPtr> LASERFICHE_API addCorrespondingTextAnnotations(ILFPagePtr ipPage, 
						ILongRectanglePtr ipBounds,  bool bRedaction, bool bPreventDuplication)
{
	ASSERT_ARGUMENT("ELI21488", ipPage != NULL);
	ASSERT_ARGUMENT("ELI21489", ipBounds != NULL);

	vector<ILFTextAnnotationPtr> vecAnnotationsCreated;

	ILFTextPtr ipText = ipPage->Text;

	// If the page has no text, there is nothing to do.  Return an empty vector
	if (ipText == NULL || ipText->Text.length() == 0)
	{
		return vecAnnotationsCreated;
	}

	// Create a CRect representing the image area of the image annotation.
	CRect rectBounds(ipBounds->Left, ipBounds->Top, ipBounds->Right, ipBounds->Bottom);

	// Retrieve word data for this page
	vector<WordData> cachedWordData = getCachedWordData(ipPage, ipText);

	if (cachedWordData.empty())
	{
		// No spatial information was found for the document text, no need to try to create
		// linked redactions.
		return vecAnnotationsCreated;
	}
	
	// Keep track of the position of the actively tracked annotation.
	long nActiveAnnotationStart = -1;
	long nActiveAnnotationEnd = -1;
	CRect rectActiveAnnotation(0, 0, 0, 0);
	int nBestIntersectionArea = 0;
	
	// Loop through each word looking for words that intersect with the image area.
	// Loop one more time than items in cachedWordData to handle any text redaction
	// which ends at the end of the page.
	for (size_t i = 0; i <= cachedWordData.size(); i++)
	{
		// Determine if the word that corresponds to this index intersects with the provided
		// annotation bounds. (if such a word exists)
		bool bWordOverlapsBounds = false;
		if (i < cachedWordData.size())
		{
			CRect rectWord(cachedWordData[i].rectWord);
			bWordOverlapsBounds = (rectWord.IntersectRect(&rectWord, &rectBounds) == TRUE);
		}

		if (bWordOverlapsBounds)
		{
			// This word intersects with the image area
			if (nActiveAnnotationStart == -1)
			{
				// There is no active text annotation, start a new one.
				// Indicate the start position of the text annotation
				nActiveAnnotationStart = cachedWordData[i].nWordStart;
			}

			// Extend the active text annotation to include this word.
			nActiveAnnotationEnd = cachedWordData[i].nWordEnd;
			rectActiveAnnotation.UnionRect(rectActiveAnnotation, cachedWordData[i].rectWord);
		}
		else if (nActiveAnnotationStart != -1)
		{
			// This word is not included in the image area, but there is an active text
			// annotation. Create the active text annotation and reset the active text
			// annotation markers. 
			ILFTextAnnotationPtr ipTextAnnotation = NULL;
			
			// Create a text annotation only if bPreventDuplication is false or
			// the text is not already annotated.
			if (!bPreventDuplication ||
				!isTextAnnotated(ipPage, nActiveAnnotationStart, nActiveAnnotationEnd, bRedaction))
			{
				ipTextAnnotation = getTextAnnotation(ipPage, 
					nActiveAnnotationStart, nActiveAnnotationEnd, bRedaction);
				ASSERT_RESOURCE_ALLOCATION("ELI20981", ipTextAnnotation != NULL);

				// Calculate what percentage of the overlapping text is contained in the 
				// original redaction bounds
				CRect rectIntersection;
				rectIntersection.IntersectRect(rectBounds, rectActiveAnnotation);
				int nIntersectionArea = rectIntersection.Width() * rectIntersection.Height();

				// Keep track of the text redaction which best matches the image redaction as 
				// the eventual return value.
				if (vecAnnotationsCreated.empty() || nIntersectionArea > nBestIntersectionArea)
				{
					// Put the best matching text annotation at the start of the vector.
					vecAnnotationsCreated.insert(vecAnnotationsCreated.begin(), ipTextAnnotation);
					nBestIntersectionArea = nIntersectionArea;
				}
				else
				{
					// If the annotation is not the best match, add it to the back of the vector
					vecAnnotationsCreated.push_back(ipTextAnnotation);
				}
			}

			// Indicate there is no longer an active text annotation
			nActiveAnnotationStart = -1;
			nActiveAnnotationEnd = -1;
			rectActiveAnnotation.SetRect(0, 0, 0, 0);
		}
	}

	return vecAnnotationsCreated;
}
//--------------------------------------------------------------------------------------------------
void LASERFICHE_API addAnnotation(ILFPagePtr ipPage, ILongRectanglePtr ipBounds,  bool bRedaction, 
				   bool bLinked/* = true*/, OLE_COLOR color/* = gcolorDEFAULT*/,
				   vector<ILFImageBlockAnnotationPtr> *pvecExistingImageAnnotations/* = NULL*/,
				   map<long long, ILFTextAnnotationPtr> *pmapTextAnnotationsCreated/* = NULL*/,
				   map<long long, ILFImageBlockAnnotationPtr> *pmapImageAnnotationsCreated/* = NULL*/)
{
	ASSERT_ARGUMENT("ELI21368", ipPage != NULL);
	ASSERT_ARGUMENT("ELI21369", ipBounds != NULL);

	try
	{
		ipPage->LockObject(LOCK_TYPE_WRITE);

		ILFImageBlockAnnotationPtr ipPrimaryAnnotation = NULL;
		vector<ILFImageBlockAnnotationPtr> vecImageAnnotationsCreated;
		
		if (bLinked)
		{
			// If creating a linked annotation, create text annotation(s) corresponding 
			// to the image area.
			vector<ILFTextAnnotationPtr> vecTextAnnotations =
				addCorrespondingTextAnnotations(ipPage, ipBounds, bRedaction, false);

			for each (ILFTextAnnotationPtr ipTextAnnotation in vecTextAnnotations)
			{
				// Create a separate image redaction for each matching text redaction
				ILFImageBlockAnnotationPtr ipImageAnnotation = 
					getImageAnnotation(ipPage, bRedaction, NULL, ipTextAnnotation);
				ASSERT_RESOURCE_ALLOCATION("ELI21497", ipImageAnnotation != NULL);

				if (color != gcolorDEFAULT)
				{
					ipImageAnnotation->Color = color;
				}

				vecImageAnnotationsCreated.push_back(ipImageAnnotation);

				// The first item is the best match and should be linked to the primary 
				if (ipPrimaryAnnotation == NULL)
				{
					ipPrimaryAnnotation = ipImageAnnotation;
				}
			}
		}

		// If bLinked == false or matching text was not found, create a stand-alone image annotation.
		if (ipPrimaryAnnotation == NULL)
		{
			ipPrimaryAnnotation = getImageAnnotation(ipPage, bRedaction, ipBounds, NULL);
			ASSERT_RESOURCE_ALLOCATION("ELI21399", ipPrimaryAnnotation != NULL);

			if (color != gcolorDEFAULT)
			{
				ipPrimaryAnnotation->Color = color;
			}

			vecImageAnnotationsCreated.push_back(ipPrimaryAnnotation);
		}

		// Add the specified bounds to the primary annotation.
		ILFRectanglePtr ipRect = ipPrimaryAnnotation->AddRectangle();
		ASSERT_RESOURCE_ALLOCATION("ELI21381", ipRect != NULL);

		ipRect->Left = ipBounds->Left;
		ipRect->Top = ipBounds->Top;
		ipRect->Right = ipBounds->Right;
		ipRect->Bottom = ipBounds->Bottom;

		// Check all annotations that were created to see if the annotation already exists; delete
		// any of the new annotations for which matches are found.
		for each (ILFImageBlockAnnotationPtr ipAnnotation in vecImageAnnotationsCreated)
		{
			ILFTextAnnotationPtr ipTextAnnotation = ipAnnotation->LinkTo;

			if (pvecExistingImageAnnotations && 
				annotationExists(ipAnnotation, *pvecExistingImageAnnotations))
			{
				// The newly added annotation is a duplicate.  Remove it.
				if (ipTextAnnotation != NULL)
				{
					ipPage->RemoveAnnotation(ipTextAnnotation);
				}
				ipPage->RemoveAnnotation(ipAnnotation);
			}
			else 
			{
				// Update pmapImageAnnotationsCreated if it was provided.
				if (pmapImageAnnotationsCreated != NULL)
				{
					(*pmapImageAnnotationsCreated)[getAnnotationID(ipPage, ipAnnotation)] 
						= ipAnnotation;
				}
				// Update pmapTextAnnotationsCreated if it was provided.
				if (pmapTextAnnotationsCreated != NULL && ipTextAnnotation != NULL)
				{
					(*pmapTextAnnotationsCreated)[getAnnotationID(ipPage, ipTextAnnotation)] 
						= ipTextAnnotation;
				}
			}
		}
	
		ipPage->Update();
		ipPage->UnlockObject();
	}
	catch (...)
	{
		safeDispose(ipPage);
		throw;
	}
}
//--------------------------------------------------------------------------------------------------
bool LASERFICHE_API ensureCorrespondingTextAnnotations(ILFDocumentPtr ipDocument, bool bRedactions,
						map<long long, ILFTextAnnotationPtr> &rmapVerifiedTextAnnotations,
						map<long long, ILFImageBlockAnnotationPtr> &rmapVerifiedImageAnnotations)
{
	// Declared outside of try scope in case of an exception.
	ILFPagePtr ipPage = NULL;

	try
	{
		ASSERT_ARGUMENT("ELI21364", ipDocument != NULL);
	
		bool bDocModified = false;

		ILFDocumentPagesPtr ipPages = ipDocument->GetPages();
		ASSERT_RESOURCE_ALLOCATION("ELI21960", ipPages != NULL);

		// Loop through each page in the document and ensure the annotation for each.
		long nCount = ipPages->Count;
		for (long i = 1; i <= nCount; i++)
		{
			ipPage = ipPages->Item[i];
			ASSERT_RESOURCE_ALLOCATION("ELI21408", ipPage != NULL);

			ILFTextPtr ipText = ipPage->Text;

			// If the page does not contain any text, there is nothing to check.
			if (ipText == NULL || ipText->Text.length() == 0)
			{
				continue;
			}

			ipPage->LockObject(LOCK_TYPE_WRITE);

			// Retrieve a list of page annotations that excludes redactions that have already been
			// verified.
			map<long long, ILFImageBlockAnnotationPtr> mapImageAnnotations = 
				getAnnotationsOnPage(ipPage, bRedactions, rmapVerifiedImageAnnotations);

			// Cyle through each annotation on the page and add a corresponding text annotation if
			// the annotation is not linked and the corresponding text is not already annotated.
			for each (pair<long long, ILFImageBlockAnnotationPtr> annotation in mapImageAnnotations)
			{
				ILFImageBlockAnnotationPtr ipImageAnnotation = annotation.second;
				ASSERT_RESOURCE_ALLOCATION("ELI21472", ipImageAnnotation != NULL);

				// If the annotation is linked we can assume that it is linked to the correct 
				// text so don't process it.
				if (ipImageAnnotation->LinkTo == NULL)
				{
					// Loop though each rectangle in the annotation 
					long nRectCount = ipImageAnnotation->Count;
					for (long j = 1; j <= nRectCount; j++)
					{
						ILFRectanglePtr ipAnnotationRect = ipImageAnnotation->Item[j];
						ASSERT_RESOURCE_ALLOCATION("ELI21366", ipAnnotationRect != NULL);

						ILongRectanglePtr ipBounds(CLSID_LongRectangle);
						ASSERT_RESOURCE_ALLOCATION("ELI21367", ipBounds != NULL);

						ipBounds->Left = ipAnnotationRect->Left;
						ipBounds->Top = ipAnnotationRect->Top;
						ipBounds->Right = ipAnnotationRect->Right;
						ipBounds->Bottom = ipAnnotationRect->Bottom;

						// Add corresponding text annotation for each rectangle.  addAnnotation
						// will check for existing text annotations, so we don't need to worry
						// about duplicates.
						vector<ILFTextAnnotationPtr> vecAddedAnnotations = 
							addCorrespondingTextAnnotations(ipPage, ipBounds, bRedactions, true);

						// [FlexIDSIntegrations:92] Only flag document as updated if annotations
						// were created.
						if (!vecAddedAnnotations.empty())
						{
							// Update the page right away; otherwise the IDs of the annotations
							// won't be set to correctly populate rmapVerifiedTextAnnotations
							ipPage->Update();
							// The document requires updating.
							bDocModified = true;

							for each (ILFTextAnnotationPtr ipAddedAnnotation in vecAddedAnnotations)
							{
								rmapVerifiedTextAnnotations[getAnnotationID(ipPage, ipAddedAnnotation)] 
									= ipAddedAnnotation;
							}
						}
					}
				}

				// Add this annotation to the map of annotation that should be considered verified.
				rmapVerifiedImageAnnotations[getAnnotationID(ipPage, ipImageAnnotation)] = ipImageAnnotation;
			}

			ipPage->UnlockObject();
		}

		if (bDocModified)
		{
			ipDocument->Update();
		}

		return bDocModified;
	}
	catch (...)
	{
		safeDispose(ipPage);
		throw;
	}
}
//--------------------------------------------------------------------------------------------------
bool LASERFICHE_API ensureCorrespondingImageAnnotations(ILFDocumentPtr ipDocument, bool bRedactions,
								map<long long, ILFTextAnnotationPtr> &rmapVerifiedTextAnnotations,
								map<long long, ILFImageBlockAnnotationPtr> &rmapVerifiedImageAnnotations)
{
	// Declared outside of try scope in case of an exception.
	ILFPagePtr ipPage = NULL;

	try
	{
		ASSERT_ARGUMENT("ELI21476", ipDocument != NULL);

		ILFDocumentPagesPtr ipPages = ipDocument->GetPages();
		ASSERT_RESOURCE_ALLOCATION("ELI21477", ipPages != NULL);

		bool bDocModified = false;
		bool bMissingSpatialInfo = false;
		UCLIDException ueMissingSpatialInfo("ELI21781", "The text associated with this document does not "
					"contain necessary information to ensure all redactions have a corresponding text or "
					"image redaction!");

		// Loop through each page in the document and ensure the annotation for each.
		long nCount = ipPages->Count;
		for (long i = 1; i <= nCount; i++)
		{
			ipPage = ipPages->Item[i];
			ASSERT_RESOURCE_ALLOCATION("ELI21478", ipPage != NULL);

			ILFTextPtr ipText = ipPage->Text;

			// If the page does not contain any text, there is nothing to check.
			if (ipText == NULL || ipText->Text.length() == 0)
			{
				continue;
			}

			// [FlexIDSIntegrations:93] If we weren't able to retrieve spatial word
			// data, don't attempt to add any corresponding redactions.
			vector<WordData> cachedWordData = getCachedWordData(ipPage, ipText);
			if (cachedWordData.empty())
			{
				ueMissingSpatialInfo.addDebugInfo("Page", i);
				bMissingSpatialInfo = true;
				continue;
			}

			ipPage->LockObject(LOCK_TYPE_WRITE);

			bool bPageModified = false;

			// Retrieve a list of page annotations that excludes redactions that have already been
			// verified.
			map<long long, ILFTextAnnotationPtr> mapTextAnnotations = 
				getAnnotationsOnPage(ipPage, bRedactions, rmapVerifiedTextAnnotations);

			// Cyle through each redaction on the page and add a linked image redaction if it does
			// not yet have one.
			for each (pair<long long,ILFTextAnnotationPtr> annotation in mapTextAnnotations)
			{
				ILFTextAnnotationPtr ipTextAnnotation = annotation.second;
				ASSERT_RESOURCE_ALLOCATION("ELI21479", ipTextAnnotation != NULL);

				// If the annotation is linked we can assume that it is linked to the correct 
				// image area so don't process it.
				if (ipTextAnnotation->LinkTo == NULL)
				{
					// If the annotation isn't linked, replace it with a linked version and remove
					// the unlinked annotation.
					ILFTextAnnotationPtr ipReplacementAnnotation = getTextAnnotation(ipPage,
						ipTextAnnotation->Start, ipTextAnnotation->End, bRedactions);

					ILFImageBlockAnnotationPtr ipImageAnnotation =
						ipPage->AddLinkedAnnotation(ipReplacementAnnotation);
					ASSERT_RESOURCE_ALLOCATION("ELI21485", ipImageAnnotation != NULL);

					rmapVerifiedTextAnnotations[getAnnotationID(ipPage, ipReplacementAnnotation)] 
						= ipReplacementAnnotation;
					rmapVerifiedImageAnnotations[getAnnotationID(ipPage, ipImageAnnotation)] 
						= ipImageAnnotation;
					rmapVerifiedTextAnnotations.erase(getAnnotationID(ipPage, ipTextAnnotation));

					ipPage->RemoveAnnotation(ipTextAnnotation);
			
					bPageModified = true;
					bDocModified = true;
				}

				// Add this annotation to the map of annotation that should be considered verified.
				rmapVerifiedTextAnnotations[getAnnotationID(ipPage, ipTextAnnotation)] = ipTextAnnotation;
			}

			if (bPageModified)
			{
				ipPage->Update();
			}
			ipPage->UnlockObject();
		}

		if (bMissingSpatialInfo)
		{
			ueMissingSpatialInfo.addDebugInfo("Document", asString(ipDocument->FindFullPath()));
			ueMissingSpatialInfo.display();
		}
				
		if (bDocModified)
		{
			ipDocument->Update();
		}

		return bDocModified;
	}
	catch (...)
	{
		safeDispose(ipPage);
		throw;
	}
}
//--------------------------------------------------------------------------------------------------
void LASERFICHE_API removeAnnotations(ILFDocumentPtr ipDocument, OLE_COLOR color/* = gcolorDEFAULT*/)
{
	ASSERT_ARGUMENT("ELI20936", ipDocument != NULL)

	bool bModified = false;

	ILFDocumentPagesPtr ipPages = ipDocument->GetPages();
	ASSERT_RESOURCE_ALLOCATION("ELI20943", ipPages != NULL);

	// Cycle through each document page looking for image annotations, and removing them as 
	// appropriate.
	long nCount = ipPages->Count;
	for (int i = 1; i <= nCount; i++)
	{
		ILFPagePtr ipPage = ipPages->Item[i];
		ASSERT_RESOURCE_ALLOCATION("ELI20937", ipPage != NULL);
		
		try
		{
			ipPage->LockObject(LOCK_TYPE_WRITE);

			int nHighlightCount = ipPage->GetImageHighlightCount();
			int nHighlightsRemoved = 0;

			// Cyle through each annotation on the page and remove it as appropriate
			for (int j = 1; j <= nHighlightCount; j++)
			{
				ILFImageBlockAnnotationPtr ipHighlight = ipPage->ImageHighlight[j];
				ASSERT_ARGUMENT("ELI20938", ipHighlight != NULL);

				// If the gcolorDEFAULT color is specified, remove every annotation.  Otherwise,
				// remove the annotation only if the color matches.
				if (color == gcolorDEFAULT || ipHighlight->Color == color)
				{
					ipPage->RemoveAnnotation(ipHighlight);
					nHighlightsRemoved++;
					
					// Start the loop over since we don't know if the highlight will be guaranteed 
					// to be in the same order after calling RemoveAnnotation.
					nHighlightCount = ipPage->GetImageHighlightCount();
					j = 0;
				}
			}

			// Update the page only if any highlights were removed on the page.
			if (nHighlightsRemoved > 0)
			{
				bModified = true;
				ipPage->Update();
			}
			ipPage->UnlockObject();
		}
		catch (...)
		{
			safeDispose(ipPage);
			throw;
		}
	}

	// Update the document only if any highlights were removed.
	if (bModified)
	{
		ipDocument->Update();
	}
}
//--------------------------------------------------------------------------------------------------
long LASERFICHE_API performDocumentOperations(IUnknownPtr ipTarget,
											  const string &strType,
											  const vector<string> &vecTagsToAdd, 
											  const vector<string> &vecTagsToRemove,
											  const vector<string> &vecRequiredTags/* = vector<string>()*/, 
											  const vector<string> &vecDisallowedTags/* = vector<string>()*/,
											  OLE_COLOR clrHighlightsToRemove/* = gcolorDEFAULT*/)
{
	ASSERT_ARGUMENT("ELI23864", !strType.empty());

	int nFailedCount = 0;

	// Attempt to retrieve a ILFDatabasePtr interface
	ILFDatabasePtr ipDatabase = ipTarget;

	// Attempt to retrieve a ILFFolderPtr interface
	ILFFolderPtr ipFolder = ipTarget;

	if (ipDatabase != NULL)
	{
		// We have a database
		ipDatabase = ipTarget;
		ipFolder = NULL;
	}
	else
	{
		if (ipFolder == NULL)
		{
			throw UCLIDException("ELI21775", "Internal error: processDocuments called on invalid target type!");
		}

		// We have a folder
		ipDatabase = ipFolder->Database;
	}

	CLFItemCollection documentSearch(ipDatabase);

	// [FlexIDSIntegrations:37]
	// Use LF:Name * only for the use of the type parameter since this is the only way to limit
	// the search by entry type.
	string strSearch = "{LF:Name=\"*\", Type=" + strType + "}";

	// Apply path filter
	if (ipFolder == NULL)
	{
		ipFolder = ipDatabase->RootFolder;
		ASSERT_RESOURCE_ALLOCATION("ELI21773", ipTarget != NULL);
	}
	string strPath = asString(ipFolder->FindFullPath());
	if (strPath == "")
	{
		strPath = "\\";
	}

	strSearch += " & {LF:Lookin=\"" + strPath + "\", Subfolders=Y}";
	
	// Apply tag filter if specified.
	for each (string strRequiredTag in vecRequiredTags)
	{
		strSearch += " & {LF:Tags=\"" + strRequiredTag + "\"}";
	}

	for each (string strDisallowedTag in vecDisallowedTags)
	{
		strSearch += " - {LF:Tags=\"" + strDisallowedTag + "\"}";
	}
	
	documentSearch.find(strSearch);
	documentSearch.waitForSearchToComplete();

	ILFDocumentPtr ipDocument = NULL;

	try
	{
		// Loop through each document that meets the search criteria
		ipDocument = documentSearch.getNextItem();
		
		while (ipDocument != NULL)
		{
			try
			{
				try
				{
					ipDocument->LockObject(LOCK_TYPE_WRITE);

					// Remove highlight annotations if requested.
					if (clrHighlightsToRemove != gcolorDEFAULT)
					{
						removeAnnotations(ipDocument, clrHighlightsToRemove);
					}

					// Remove all tags specified to be removed
					for each (const string &strTag in vecTagsToRemove)
					{
						removeTag(ipDocument, strTag);
					}

					// Add all tags specified to be added.
					for each (const string &strTag in vecTagsToAdd)
					{
						addTag(ipDocument, strTag);
					}
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21024");
			}
			catch (UCLIDException &ue)
			{
				// Log and keep track of each failure untagging, then move on.
				nFailedCount++;

				string strMessage = "Failed to process document!";

				// Attempt to generate better debug info and tag the document as failed.
				try
				{
						strMessage = "Failed to process document \"" + 
							asString(ipDocument->Name) + "\"!";
				}
				catch (...) {}
		
				if (ipDocument != NULL)
				{
					try
					{
						ipDocument->Dispose();
					}
					catch (...) {}
				}

				UCLIDException uexOuter("ELI21779", strMessage, ue);
				uexOuter.log();

				ipDocument = documentSearch.getNextItem();
				continue;
			}

			ipDocument->Dispose();

			ipDocument = documentSearch.getNextItem();
		}
	}
	catch (...)
	{
		if (ipDocument != NULL)
		{
			try
			{
				ipDocument->Dispose();
			}
			catch (...) {}
		}
		throw;
	}

	return nFailedCount;
}
//--------------------------------------------------------------------------------------------------