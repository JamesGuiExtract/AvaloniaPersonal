//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LFMiscUtils.h
//
// PURPOSE:	Utility functions for interacting with Laserfiche products
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================

#pragma once

#include "stdafx.h"
#include "LaserficheUtils.h"

#include <UCLIDException.h>

#include <string>
#include <vector>
#include <map>
#include <list>
#include <utility>
using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static LPCTSTR gzLFCLIENT_CLASS					= "LFDesktop";
static LPCTSTR gzDOCUMENT_CLASS					= "LFDocFrame";
static const string gzDOCUMENT_TITLE_SUFFIX		= " - Laserfiche";
static const WORD gwLF_ERROR_TAG_DOESNT_EXIST	= 529;
static const WORD gwLF_ERROR_TAG_ALREADY_EXISTS	= 530;
static const OLE_COLOR gcolorDEFAULT			= 0xFFFFFFFF;

//--------------------------------------------------------------------------------------------------
// Structs
//--------------------------------------------------------------------------------------------------
// A structure used to cache word data from a page so that it doesn't need to be re-processed
// with COM calls for each attribute on a page (which is slow).
struct WordData
{
	CRect rectWord;
	long nWordStart;
	long nWordEnd;
};

//--------------------------------------------------------------------------------------------------
// PURPOSE: To retrieve a list of available Laserfiche repositories
// RETURNS:
//			rvecRepositories-	Each entry is populated with the name of an available repository
//			rmapServers-		Maps each repository name to the server hosting the repository.
//								Can include port number. Example:
//								rmapServers["Repository1"] = "adderley:1888";
// NOTE:	rvecRepositories and rmapServers will be cleared prior to searching for repositories
void LASERFICHE_API getAvailableRepositories(vector<string> &rvecRepositories, 
											 map<string, string> &rmapServers);
//--------------------------------------------------------------------------------------------------
// PURPOSE: Establish a connection to the specified Laserfiche repository
// ARGUMENTS:
//			strServer - The name of the server hosting the target repository. The value can be
//				suffixed with ":[port]" such as "addreley:1888". If the port is not specified the 
//				default Laserfiche port will be used.
//			strRepository - The name of the target repository
//			strUser - The name of the Laserfiche user to use in establishing the connection
//			strPassword - The password for the specified Laserfiche user.
// RETURNS: An ILFConnectionPtr to the open connection.  An exception is thrown if the connection
//			cannot be established for any reason.
ILFConnectionPtr LASERFICHE_API connectToRepository(const string &strServer, 
	const string &strRepository, const string &strUser, const string &strPassword);
//--------------------------------------------------------------------------------------------------
// PURPOSE:	Add the specified tag to the specified document or folder
// ARGUMENTS:
//			ipObject- Either an ILFFolderPtr or an ILFDocumentPtr representing the object to tag
//			strTag- The name of the tag to apply
//			strComment- If specified, adds the supplied text as a comment for the tag
// NOTE:	Will throw an exception on failure unless the operation failed because the object
//			already has the specified tag. 
void LASERFICHE_API addTag(IUnknownPtr ipObject, const string &strTag, 
						   const string &strComment = "");
//--------------------------------------------------------------------------------------------------
// PURPOSE:	Remove the specified tag from the specified document or folder
// ARGUMENTS:
//			ipObject- Either an ILFFolderPtr or an ILFDocumentPtr representing the object to untag
//			strTag- The name of the tag to remove
// NOTE:	Will throw an exception on failure unless the operation failed because the object
//			does not contain the specified tag. 
void LASERFICHE_API removeTag(IUnknownPtr ipObject, const string &strTag);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To determine whether the specified document or folder possesses the specified tag.
// ARGUMENTS: 
//			ipObject- Either an ILFFolderPtr or an ILFDocumentPtr representing the object to check
//			strTag- The name of the tag to check for
// RETURNS: true if ipObject contains the tag, false if it does not (an exception is thrown if the
//			check fails).
bool LASERFICHE_API hasTag(IUnknownPtr ipObject, const string &strTag);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To determine whether the current connection (or logged-in user) possesses the specified
//			Access_Right for the specified document or folder
//			ipObject- Either an ILFFolderPtr or an ILFDocumentPtr representing the object to check
//			right- The LFSO72Lib::Access_Right to check for (See toolkit doc for "Access Rights")
//			strExceptionText- If the specified object does not posses the specified right,
//				the text that should be assigned to the resulting UCLIDException
// NOTE:	If right is found to exist, this function will simply return.  If the right is missing
//			an exception will be thrown
void LASERFICHE_API verifyHasRight(IUnknownPtr ipObject, Access_Right right,
					const string &strExceptionText = "Internal error: Inadequate object rights!");
//--------------------------------------------------------------------------------------------------
// PURPOSE: Open a document in the Laserfiche Client
// ARGUMENTS:
//			ipClient- The client to use to open the document
//			hwndClient- The window handle of the main Laserfiche Client window
//			strClientVersion- The Laserfiche Client Version.
//			strDocumentName- The name of the document to open
//			nDocumentID- The ID of the document to open.
//			nTimeout- The number of seconds to wait for the document window to be found
// RETURNS:	An HWND to the Laserfiche Document window displaying the document
// NOTE:	This call is asynchronous-- it returns as soon as it finds a handle to the window.
HWND LASERFICHE_API showDocument(ILFClientPtr ipClient, HWND hwndClient, string strClientVersion,
								 string strDocumentName, long nDocumentID, int nTimeout = 5);
//--------------------------------------------------------------------------------------------------
// PURPOSE: Returns an ID which uniquely identifies an annotation across multiple pages.
//			(LFSO Annotation IDs as unique only on a given page)
// ARGUMENTS:
//			ipPage- The page the annotation is on
//			ipAnnotation- The annotation the ID is for
long long LASERFICHE_API getAnnotationID(ILFPagePtr ipPage, ILFImageBlockAnnotationPtr ipAnnotation);
long long LASERFICHE_API getAnnotationID(ILFPagePtr ipPage, ILFTextAnnotationPtr ipAnnotation);
//--------------------------------------------------------------------------------------------------
// PURPOSE: Adds an unlinked text annotation for each contiguous block of text whose spatial
//			information overlaps with the provided bounds.
// ARGUMENTS:
//			ipPage- The page to add the annotation(s) to
//			ipBounds- The bounds for which corresponding text annotations are needed.
//			bRedaction- true to add redaction annotation, false to add highlights
//			bPreventDuplication- if true, annotations will not be added to text which is already
//				annotated.
// RETURNS:	A vector of ILFTextAnnotations that were added to the page. The first item
//			in the vector is considered the best match for the provided bounds as determined
//			by the total area of coverage.  The vector may be empty if there is no corresponding
//			text.
// NOTE:	This function does not lock, update or unlock the page.  It expects the caller to
//			have locked the page and to update and unlock the page following the call.
vector<ILFTextAnnotationPtr> LASERFICHE_API addCorrespondingTextAnnotations(ILFPagePtr ipPage, 
						ILongRectanglePtr ipBounds, bool bRedaction, bool bPreventDuplication);
//--------------------------------------------------------------------------------------------------
// PURPOSE: Adds an annotation (highlight or redaction) to the specified document page.
// ARGUMENTS:
//			ipPage- The page to add the annotation to
//			ipBounds- The rectangle describing the location of the annotation on the page
//			bRedaction- true to add a redaction, false to add a highlight
//			bLinked- true to annotate the corresponding text (if present) and link the image and
//				text annotations
//			color- The color to use for the annotation.  If gcolorDEFAULT, the default color for
//				the specified annotation type is used.
//			pvecExistingImageAnnotations- If this vector is provided, if any resulting
//				annotations from this match one of the annotation in this list, it won't be
//				duplicated.
//			pmapTextAnnotationsCreated- If supplied it is populated with all text
//				annotations that were added.  The key is the ID of each annotation and the
//				value is the annotation object.
//			pmapImageAnnotationsCreated- If supplied it is populated with all image
//				annotations that were added.  The key is the ID of each annotation and the
//				value is the annotation object.
// REQUIRE: The caller must lock and update the document as necessary.
// NOTE:	Following this call the page will be updated. (but not the document)
void LASERFICHE_API addAnnotation(ILFPagePtr ipPage, ILongRectanglePtr ipBounds,  bool bRedaction, 
				   bool bLinked = true, OLE_COLOR color = gcolorDEFAULT,
				   vector<ILFImageBlockAnnotationPtr> *pvecExistingImageAnnotations = NULL,
				   map<long long, ILFTextAnnotationPtr> *pmapTextAnnotationsCreated = NULL,
				   map<long long, ILFImageBlockAnnotationPtr> *pmapImageAnnotationsCreated = NULL);
//--------------------------------------------------------------------------------------------------
// PURPOSE: Ensures for each image annotation that is in the document that any text corresponding
//			with the image area is redacted.  Any required redactions are added as unlinked text 
//			annotations.
// ARGUMENTS:
//			ipDocument- The document for which the annotations are to be checked.
//			bRedactions- true to check redactions, false to check highlights
//			rmapVerifiedImageAnnotations- A ID to annotation map of annotations that should be 
//				considered verified.  Any entries passed in when the function is called will
//				not be checked. When the function returns, rmapVerifiedImageAnnotations will be
//				populated with all annotations that were checked.
//			rmapVerifiedTextAnnotations- A ID to annotation map of annotations that should be 
//				considered verified.When the function returns, rmapVerifiedAnnotations will include 
//				all text annotations that were added as a result of this call.
//	RETURNS: true if any text annotations were added, false if no modification of the document was
//			 required.
//	NOTE:	The document will be updated as required, but it will not be locked or unlocked.  The
//			caller should lock the document prior to this call and unlock it afterward.
bool LASERFICHE_API ensureCorrespondingTextAnnotations(ILFDocumentPtr ipDocument, bool bRedactions,
								map<long long, ILFTextAnnotationPtr> &rmapVerifiedTextAnnotations,
								map<long long, ILFImageBlockAnnotationPtr> &rmapVerifiedImageAnnotations);
//--------------------------------------------------------------------------------------------------
// PURPOSE: Ensures for each text annotation that is in the document that the corresponding area
//			within the image area is redacted.  Any unlinked text redaction will be replaced
//			with a text redaction linked to a corresponding image redaction.
// ARGUMENTS:
//			ipDocument- The document for which the annotations are to be checked.
//			bRedactions- true to check redactions, false to check highlights
//			rmapVerifiedTextAnnotations- A ID to annotation map of annotations that should be considered
//				verified.  Any entries passed in when the function is called will not be checked.
//				When the function returns, rmapVerifiedAnnotations will be populated with all
//				annotations that were checked.
//	RETURNS: true if any annotations were replaced, false if no modification of the document was
//			 required.
//	NOTE:	The document will be updated as required, but it will not be locked or unlocked.  The
//			caller should lock the document prior to this call and unlock it afterward.
bool LASERFICHE_API ensureCorrespondingImageAnnotations(ILFDocumentPtr ipDocument, bool bRedactions,
								map<long long, ILFTextAnnotationPtr> &rmapVerifiedTextAnnotations,
								map<long long, ILFImageBlockAnnotationPtr> &rmapVerifiedImageAnnotations);
//--------------------------------------------------------------------------------------------------
// PURPOSE:	Remove annotations from the specified document.
// ARGUMENTS:
//			ipDocument- The document from which to remove annotations.
//			color-		If gcolorDEFAULT, all annotations are removed from the document. For any
//						other value, only annotations with the specified color will be removed.
// REQUIRE: Caller must lock the document prior to this call and unlock it afterwards.
// NOTE:	This function will update the document as necessary.
void LASERFICHE_API removeAnnotations(ILFDocumentPtr ipDocument, OLE_COLOR color = gcolorDEFAULT);
//--------------------------------------------------------------------------------------------------
// PURPOSE:	To perform a sequence of operations (addTag, removeTag, removeAnnoations) on all
//			documents that meet a tag search criteria
// ARGUMENTS:
//			ipTarget-			Either a ILFDatabasePtr or ILFFolderPtr specifying the repository or
//								folder in which documents will be operated upon.
//			strType-			The type specifier to use for the search (can be combined, ie "BD"):
//								F = Folder
//								D = Document (applies only to documents with templates)
//								B = Batch (basically means documents without templates)
//			vecTagsToAdd-		A set of tags to be added to target documents (can be empty)
//			vecTagsToRemove-	A set of tags to be removed from target documents (can be empty)
//			vecRequiredTags-	A set of tags documents must contain to be processed (optional, can be empty)
//			vecDisallowedTags-	A set of tags documents must not contain to be processed (optional, can be empty)
//			clrHighlightsToRemove - If specified, highlights of this color will be removed from the document
long LASERFICHE_API performDocumentOperations(IUnknownPtr ipTarget,
											  const string &strType,
											  const vector<string> &vecTagsToAdd, 
											  const vector<string> &vecTagsToRemove,
											  const vector<string> &vecRequiredTags = vector<string>(), 
											  const vector<string> &vecDisallowedTags = vector<string>(),
											  OLE_COLOR clrHighlightsToRemove = gcolorDEFAULT);
//--------------------------------------------------------------------------------------------------
// PURPOSE:	To ensure no locks or handles are left behind on LFSO objects after catching an
//			exception.  The object will be disposed or unlocked as appropriate and set to NULL.
// ARGUMENTS:
//			ripObject- The object to be disposed of.
// NOTE:	Intended to work on any LFSOLib object that implements either a Dispose or UnlockObject
//			method.  Examples: ILFDocumentPtr, ILFPagePtr, ILFSearchPtr, ILFTagDataPtr.
template <typename T>
void safeDispose(T &ripObject)
{
	// Ensure the object passed in has an interface
	__if_exists (T::Interface)
	{
		if (ripObject == NULL)
		{
			// If the object is NULL, nothing to do
			return;
		}
		else
		{
			// First check for a Dispose method.  It should be called when available.
			__if_exists (T::Interface::Dispose)
			{
				try
				{
					ripObject->Dispose();
					ripObject = NULL;
				}
				// Ignore any exception.  
				// This is only an attempt to clean up from a previous exception.  
				catch (...) {}  
				return;
			}
			// If a dispose method isn't available, call UnlockObject
			__if_exists (T::Interface::UnlockObject)
			{
				try
				{
					ripObject->UnlockObject();
					ripObject = NULL;
				}
				// Ignore any exception.  
				// This is only an attempt to clean up from a previous exception.  
				catch (...) {}
				return;
			}
		}
	}

	// If the object wasn't valid, log the problem.
	UCLIDException ue("ELI21006", "Internal error: safeDispose called on invalid type!");
	ue.log();
}
//--------------------------------------------------------------------------------------------------