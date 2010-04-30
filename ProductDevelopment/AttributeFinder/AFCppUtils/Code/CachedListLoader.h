// CachedListLoader.h : Declaration of the CCachedListLoader

#pragma once
#include "resource.h"       // main symbols
#include "AFCppUtils.h"
#include "AFTagManager.h"

#include <CachedObjectFromFile.h>
#include <StringLoader.h>

#include <afxmt.h>

#include <string>
#include <map>
#include <set>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CCachedListLoader
//--------------------------------------------------------------------------------------------------
class EXPORT_AFCppUtils CCachedListLoader
{
public:
	CCachedListLoader();
	~CCachedListLoader();

	// PROMISE: Retrieves the specified file into a VariantVector list. The list file will be
	//			auto-encrypted as necessary and the list will be cached and re-loaded only if the
	//			file is updated. The same cached list can be shared between multiple instances that
	//			are referencing the same file.
	// RETURNS: If the list specification begins with "file://", the filename that follows will be
	//			loaded into a list. If the "file://" designation is not used, NULL will be returned
	//			to indicate a file was not specified.
	// REQUIRES: Any specified file must be compatible with StringLoader.
	IVariantVectorPtr getList(const _bstr_t& bstrListSpecification, IAFDocumentPtr ipAFDoc = NULL,
		char* pcDelimeter = NULL);

	// PROMISE: Given the specified list of values, any included value that specifies a file will be
	//			replaced with the values in that file. If ipAFDoc is specified it will be used to
	//			expand any AFTags, otherwise any AFTags will not be expanded.
	// RETURNS: A VariantVector list where any item that begins with "file://" is replaced with the
	//			values in the specified file.
	// REQUIRES: Any specified file must be compatible with StringLoader.
	IVariantVectorPtr expandList(IVariantVectorPtr ipSourceList, IAFDocumentPtr ipAFDoc = NULL);

	// PROMISE: Given the specified two column list of values, any included item where the first
	//			column specifies a file will be replaced with the values in that file. If ipAFDoc is
	//			specified it will be used to expand any AFTags, otherwise any AFTags will not be
	//			expanded.
	// PARAMS:	cDelimeter: Specifies the separator used to designate where the filename ends and
	//			the delimeter specification begins. For example "file://list.dat;*" specifies
	//			that the character in list.dat that separates the two columns is "*".
	// RETURNS: A IIUnknownVectorPtr list of StringPairs where any item specifying a file is
	//			replaced with the values in the specified file.
	// REQUIRES: Any specified file must be compatible with StringLoader.
	IIUnknownVectorPtr expandTwoColumnList(IIUnknownVectorPtr ipSourceList, char cDelimeter,
		IAFDocumentPtr ipAFDoc = NULL);

private:

	//////////////
	// Variables
	//////////////

	// Used to check for file list specification.
	IMiscUtilsPtr m_ipMiscUtils;

	// Used to expand AFTags
	AFTagManager m_tagManager;

	// Ensures only one list is retrieve at a time in case multiple instances are referencing the
	// same list.
	static CMutex ms_Mutex;

	// Keeps track of the lists referenced by this instance.
	map<string, CachedObjectFromFile<IVariantVectorPtr, StringLoader>* > 
		m_mapReferencedLists;

	// Keeps track of all lists currently referenced by any instance.
	static map<string, CachedObjectFromFile<IVariantVectorPtr, StringLoader> > 
		ms_mapCachedLists;

	// Keeps track of the number of current references to each list.
	static map<string, long> ms_mapReferenceCounts;

	//////////////
	// Methods
	//////////////

	// Retrieves the list from the specified file.
	IVariantVectorPtr getCachedList(const string& strFileName);

	// Checks to see if the specified key has already been used in the specified set of keys and
	// adds it to the set if it is not already there.
	void checkForDuplicateKey(set<_bstr_t>& rsetStringKeys, _bstr_t bstrKey);
};