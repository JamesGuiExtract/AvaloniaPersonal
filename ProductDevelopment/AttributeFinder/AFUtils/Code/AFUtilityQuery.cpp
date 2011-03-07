
#include "stdafx.h"
#include "AFUtils.h"
#include "AFUtility.h"

#include <UCLIDException.h>
#include <StringTokenizer.h>
#include <ComUtils.h>
#include <cpputil.h>

const string gstrAnyValue = "*";

//-------------------------------------------------------------------------------------------------
// QueryPattern class
//-------------------------------------------------------------------------------------------------
CAFUtility::QueryPattern::QueryPattern(string strName)
: m_strName(strName), m_strType(""), m_bTypeSpecified(false)
{
}
//-------------------------------------------------------------------------------------------------
CAFUtility::QueryPattern::QueryPattern(string strName, string strType)
: m_strName(strName), m_strType(strType), m_bTypeSpecified(true)
{
}

//-------------------------------------------------------------------------------------------------
// COM-exposed methods related to querying
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::QueryAttributes(IIUnknownVector *pvecAttributes, 
										 BSTR strQuery,
										 VARIANT_BOOL bRemoveMatches,
										 IIUnknownVector** ppAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// wrap the input vector of attributes in a smart pointer
		IIUnknownVectorPtr ipInput(pvecAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI07936", ipInput != NULL);

		string strMainQuery = asString(strQuery);
		IIUnknownVectorPtr ipResult = getCandidateAttributes(ipInput, strMainQuery,
			asCppBool(bRemoveMatches));

		// return results to the caller
		*ppAttributes = ipResult.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07925");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetAttributeParent(IIUnknownVector *pvecAttributes, 
											IAttribute *pAttribute, 
											IAttribute** pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipvecAttributes(pvecAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI09452", ipvecAttributes != NULL);

		IAttributePtr ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI09453", ipAttribute != NULL);

		// Default the return value to NULL
		*pRetVal = NULL;

		// Search for the parent attribute
		IAttributePtr ipParent = getAttributeParent(ipvecAttributes, ipAttribute);

		// If a parent attribute was found, return the parent
		if (ipParent != NULL)
		{
			*pRetVal = ipParent.Detach();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09448");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetAttributeRoot(IIUnknownVector *pvecAttributes, 
										  IAttribute *pAttribute, 
										  IAttribute** pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipvecAttributes(pvecAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI19365", ipvecAttributes != NULL);

		IAttributePtr ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI19366", ipAttribute != NULL);

		IAttributePtr ipParent = NULL;
		long lSize = ipvecAttributes->Size();
		for (long i = 0; i < lSize; i++)
		{
			IAttributePtr ipTmpAttr = ipvecAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI19367", ipTmpAttr != NULL);

			// If the attribute itself is a root return itself
			if (ipTmpAttr == ipAttribute)
			{
				*pRetVal = ipTmpAttr.Detach();
				return S_OK;
			}

			ipParent = getParent(ipTmpAttr, ipAttribute);
			if (ipParent != NULL)
			{
				// return the topmost ancestor
				*pRetVal = ipTmpAttr.Detach();
				return S_OK;
			}
		}

		*pRetVal = NULL;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09449");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetMinQueryDepth(BSTR bstrQuery, long *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		validateLicense();

		string strQuery = asString(bstrQuery);
		vector<string> vecQueries;
		StringTokenizer::sGetTokens(strQuery, "|", vecQueries);
		long nMinDepth = -1;
		unsigned int ui;
		for (ui = 0; ui < vecQueries.size(); ui++)
		{
			vector<string> vecAttrLevels;
			string strTemp = vecQueries[ui];
			StringTokenizer::sGetTokens(strTemp, "/", vecAttrLevels);
			long nDepth = 0;
			unsigned int uj;
			for (uj = 0; uj < vecAttrLevels.size(); uj++)
			{
				if (vecAttrLevels[uj] == "")
				{
					continue;
				}
				nDepth++;
			}
			if (nDepth < nMinDepth || nMinDepth == -1)
			{
				nMinDepth = nDepth;
			}
		}
		*pRetVal = nMinDepth;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09495");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::GetMaxQueryDepth(BSTR bstrQuery, long *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strQuery = asString(bstrQuery);
		vector<string> vecQueries;
		StringTokenizer::sGetTokens(strQuery, "|", vecQueries);
		long nMaxDepth = -1;
		unsigned int ui;
		for (ui = 0; ui < vecQueries.size(); ui++)
		{
			vector<string> vecAttrLevels;
			string strTemp = vecQueries[ui];
			StringTokenizer::sGetTokens(strTemp, "/", vecAttrLevels);
			long nDepth = 0;
			unsigned int uj;
			for (uj = 0; uj < vecAttrLevels.size(); uj++)
			{
				if (vecAttrLevels[uj] == "")
				{
					continue;
				}
				nDepth++;
			}
			if (nDepth > nMaxDepth || nMaxDepth == -1)
			{
				nMaxDepth = nDepth;
			}
		}
		*pRetVal = nMaxDepth;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09496");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::RemoveAttribute(IIUnknownVector *pvecAttributes, 
										  IAttribute *pAttribute)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		validateLicense();

		IIUnknownVectorPtr ipvecAttributes(pvecAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI09465", ipvecAttributes != NULL);

		IAttributePtr ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI19368", ipAttribute != NULL);

		removeAttribute(ipvecAttributes, ipAttribute);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19364");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::RemoveAttributes(IIUnknownVector *pvecAttributes, 
										  IIUnknownVector *pvecRemove)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pvecAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI09519", ipAttributes != NULL);
		IIUnknownVectorPtr ipRemove(pvecRemove);
		ASSERT_RESOURCE_ALLOCATION("ELI09520", ipRemove != NULL);

		long lSize = ipRemove->Size();
		for (long i = 0; i < lSize; i++)
		{
			IAttributePtr ipAttr = ipRemove->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI09521", ipAttr != NULL);
			removeAttribute(ipAttributes, ipAttr);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09517");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::IsValidQuery(BSTR bstrQuery, VARIANT_BOOL* pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{

		validateLicense();

		// Assume its valid then disprove
		*pRetVal = VARIANT_TRUE;

		string strMainQuery = asString(bstrQuery);
		vector<string> vecQueries;
		StringTokenizer st('|');
		st.parse(strMainQuery, vecQueries);

		if(vecQueries.empty())
		{
			*pRetVal = VARIANT_FALSE;
			return S_OK;
		}

		// iterate through each of the queries and check for
		// matches.  Add any found matches to ipResult;
		vector<string>::iterator queryIter;
		for (queryIter = vecQueries.begin(); queryIter != vecQueries.end(); queryIter++)
		{
			// get the current query and the patterns in it
			string stdstrQuery = *queryIter;
			vector<CAFUtility::QueryPattern> vecPatterns;
			vector<CAFUtility::QueryPattern> vecNonSelectPatterns;
			try
			{
				splitQuery(stdstrQuery, vecPatterns, vecNonSelectPatterns);
				// ensure that there's at least one pattern
				if (vecPatterns.empty())
				{
					UCLIDException ue("ELI19346", "Invalid query!");
					ue.addDebugInfo("Query", stdstrQuery);
					throw ue;
				}
			}
			catch(...)
			{
				*pRetVal = VARIANT_FALSE;
				return S_OK;
			}
			

			// Create a temporary attribute to test names and types for validity
			IAttributePtr ipTmp(CLSID_Attribute);
			ASSERT_RESOURCE_ALLOCATION("ELI10432", ipTmp != NULL);
			try
			{
				unsigned int ui;
				for (ui = 0; ui < vecPatterns.size(); ui++)
				{	
					if (vecPatterns[ui].m_strName != "*")
					{
						ipTmp->Name = get_bstr_t(vecPatterns[ui].m_strName);
					}

					if (vecPatterns[ui].m_strType != "")
					{
						ipTmp->Type = get_bstr_t(vecPatterns[ui].m_strType);	
					}
				}

				for (ui = 0; ui < vecNonSelectPatterns.size(); ui++)
				{	
					if (vecNonSelectPatterns[ui].m_strName != "*")
					{
						ipTmp->Name = get_bstr_t(vecNonSelectPatterns[ui].m_strName);
					}

					if (vecNonSelectPatterns[ui].m_strType != "")
					{
						ipTmp->Type = get_bstr_t(vecNonSelectPatterns[ui].m_strType);	
					}
				}
			}
			catch(...)
			{
				*pRetVal = VARIANT_FALSE;
				return S_OK;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10431");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtility::RemoveMetadataAttributes(IIUnknownVector *pvecAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipAttributeVector(pvecAttributes);
		ASSERT_ARGUMENT("ELI28439", ipAttributeVector != NULL);

		removeMetadataAttributes(ipAttributeVector);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28440");
}

//-------------------------------------------------------------------------------------------------
// private  methods related to querying
//-------------------------------------------------------------------------------------------------
void CAFUtility::processAttributeForMatches(IAttributePtr& ripAttribute, 
	const vector<CAFUtility::QueryPattern>& vecPatterns, 
	const vector<CAFUtility::QueryPattern>& vecNonSelectPatterns, 
	long nCurrentMatchPos,
	IIUnknownVectorPtr& ripMatches, bool bRemoveMatchFromParent, 
	bool& rbAttributeWasMatched)
{
	try
	{
		ASSERT_ARGUMENT("ELI19873", ripAttribute != NULL);
		ASSERT_ARGUMENT("ELI19874", ripMatches != NULL);

		// default this returned value to false(attribute is not matched yet)
		rbAttributeWasMatched = false;

		// get the pattern at the current match pos
		const CAFUtility::QueryPattern& pt = vecPatterns[nCurrentMatchPos];

		// get the name and type from the attribute
		string strAttrName = asString(ripAttribute->Name);
		string strAttrType = asString(ripAttribute->Type);

		// get the name and type from the query
		string strQueryAttrName = pt.m_strName;
		string strQueryAttrType = pt.m_strType;

		// [p16 #2680] - case insensitive compare
		makeLowerCase(strAttrName);
		makeLowerCase(strQueryAttrName);
		makeLowerCase(strAttrType);
		makeLowerCase(strQueryAttrType);

		// check for name match
		bool bNamesMatch = strAttrName == strQueryAttrName || strQueryAttrName == gstrAnyValue;

		// check for type match
		bool bTypesMatch = false;
		if (!pt.m_bTypeSpecified || // Type match is not necessary
			strAttrType == strQueryAttrType || // The types match
			gstrAnyValue == strQueryAttrType) // The specified type is any
		{
			bTypesMatch = true;
		}
		else if (strQueryAttrType == "") // the specified type is none
		{
			if (strAttrType.length() <= 0) // the attribute has no type
			{
				bTypesMatch = true;
			}
		}
		// Note that ContainsType() will except if pt.m_strType == "", but that is 
		// handled by the previous if
		else if (asCppBool(ripAttribute->ContainsType(strQueryAttrType.c_str()))) // the specified type is present
		{
			bTypesMatch = true;
		}

		// Ensure the names and types match
		if ( bNamesMatch && bTypesMatch)
		{
			// check if we have satisfied all match requirements.
			// if so, add to result, remove from parent if requested, and return
			if (nCurrentMatchPos == vecPatterns.size() - 1)
			{
				// Now we need to process the non-selecting attributes
				if (vecNonSelectPatterns.size() > 0)
				{
					IIUnknownVectorPtr ipSubAttributes = ripAttribute->SubAttributes;
					ASSERT_RESOURCE_ALLOCATION("ELI10220", ipSubAttributes != NULL);

					IIUnknownVectorPtr ipTmpAttributes(CLSID_IUnknownVector);
					ASSERT_RESOURCE_ALLOCATION("ELI19872", ipTmpAttributes != NULL);

					vector<CAFUtility::QueryPattern> tmpVec;
					processAttributesForMatches( vecNonSelectPatterns, tmpVec, 0, 
						ipTmpAttributes, false, ipSubAttributes);
					if(ipTmpAttributes->Size() != 0)
					{
						ripMatches->PushBack(ripAttribute);
						rbAttributeWasMatched = true;
					}
				}
				else
				{
					// add to result
					ripMatches->PushBack(ripAttribute);
					rbAttributeWasMatched = true;
				}
				return;
			}

			// get the sub-attributes of the attribute
			IIUnknownVectorPtr ipSubAttributes = ripAttribute->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI07941", ipSubAttributes != NULL);

			processAttributesForMatches(vecPatterns, vecNonSelectPatterns, 
				nCurrentMatchPos + 1, ripMatches, bRemoveMatchFromParent, ipSubAttributes);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26426");
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::processAttributesForMatches( const vector<CAFUtility::QueryPattern>& vecPatterns, 
	const vector<CAFUtility::QueryPattern>& vecNonSelectPatterns,
	long nCurrentMatchPos, IIUnknownVectorPtr& ripMatches, 
	bool bRemoveMatchFromParent, const IIUnknownVectorPtr& ripAttributes)
{
	try
	{
		// this is used in conjuction with indexing
		long nNumMatches = 0;
		// iterate through the sub-attributes and
		// process for the next level of match
		long lSize = ripAttributes->Size();
		for (long i = 0; i < lSize; i++)
		{
			// get the attribute at the current position and attempt matching
			IAttributePtr ipAttribute = ripAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI07938", ipAttribute != NULL);

			bool bAttrMatched = false;
			// process the sub-attribute for the next level of match
			processAttributeForMatches(ipAttribute, vecPatterns, vecNonSelectPatterns,
				nCurrentMatchPos, ripMatches, bRemoveMatchFromParent,
				bAttrMatched);

			// remove from parent if requested
			if (bRemoveMatchFromParent && bAttrMatched)
			{
				//ripParentOfAttribute->RemoveValue(ripAttribute);
				ripAttributes->RemoveValue(ipAttribute);
				i--;
				lSize--;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26427");
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::splitQuery(const string& strQuery, 
							vector<QueryPattern>& rvecPatterns, 
							vector<QueryPattern>& rvecNonSelectPatterns)
{
		// first get any non-selecting patterns from the end of the query
	long nOpen = strQuery.find_first_of('{');
	if(nOpen != string::npos)
	{
		if(strQuery.at(strQuery.length() - 1) != '}')
		{
			UCLIDException ue("ELI10219", "Invalid query pattern.  No closing }.");
			ue.addDebugInfo("Pattern", strQuery);
			throw ue;
		}
		// get the non selecting patterns
		string strTmpQuery = strQuery.substr(nOpen+1, (strQuery.length()-1) - (nOpen+1));
		getQueryPatterns(strTmpQuery, rvecNonSelectPatterns);
	}
	string strTmpQuery = strQuery.substr(0, nOpen);
	// get the non selecting patterns
	getQueryPatterns(strTmpQuery, rvecPatterns);
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::getQueryPatterns(string strQuery, 
								  vector<QueryPattern>& rvecPatterns)
{
	// clear the vector
	rvecPatterns.clear();
	
	// return if strQuery is an empty string
	if (strQuery.empty())
	{
		return;
	}

	// if the query contains a leading slash, delete the
	// leading slash
	if (strQuery[0] == '/')
	{
		strQuery.erase(0, 1);
	}
	
	// tokenize the query into individual parts which
	// are separated by slashes
	vector<string> vecTokens;
	StringTokenizer st('/');
	st.parse(strQuery, vecTokens);

	// build the vector of pattern structures that need to be
	// matched
	vector<string>::iterator iter;
	for (iter = vecTokens.begin(); iter != vecTokens.end(); iter++)
	{

		// get the token
		string strToken = *iter;

		// check if the @ character was used. If so, the type
		// information is expected to follow it
		long nAtCharPos = strToken.find_first_of('@');

		CAFUtility::QueryPattern queryPattern;
		if (nAtCharPos == string::npos)
		{
			// the @ char was not used.  So, the token
			// represents the name of the attribute
			queryPattern.m_strName = strToken;
		}
		else
		{
			// the @ char was found.  Find the name and type parts
			string strName = strToken.substr(0, nAtCharPos);
			string strType = strToken.substr(nAtCharPos + 1);
			queryPattern.m_strName = strName;
			queryPattern.m_strType = strType;
			queryPattern.m_bTypeSpecified = true;
		}

		rvecPatterns.push_back(queryPattern);
	}
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CAFUtility::getParent(const IAttributePtr& ipTestParent,
									const IAttributePtr& ipAttribute)
{
	try
	{
		IIUnknownVectorPtr ipSubAttributes = ipTestParent->SubAttributes;
		ASSERT_RESOURCE_ALLOCATION("ELI26438", ipSubAttributes != NULL);

		// First check if the sub attributes collection contains this attribute
		long lFoundIndex = -1;
		ipSubAttributes->FindByReference(ipAttribute, 0, &lFoundIndex);
		if (lFoundIndex != -1)
		{
			// The attribute was found so return test parent
			return ipTestParent;
		}

		// The attribute was not in the sub attributes collection, need to look down
		// another level (check each of the sub sub attributes)
		long lSubSize = ipSubAttributes->Size();
		for (long i = 0; i < lSubSize; i++)
		{
			IAttributePtr ipTmpAttr = ipSubAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI09450", ipTmpAttr != NULL);

			IAttributePtr ipParent = getParent(ipTmpAttr, ipAttribute);
			if (ipParent != NULL)
			{
				return ipParent;
			}
		}
		return NULL;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26437");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CAFUtility::getCandidateAttributes(const IIUnknownVectorPtr& ipInput,
													  const string& strMainQuery,
													  bool bRemoveMatches)
{
	try
	{
		// create the results vector
		IIUnknownVectorPtr ipResult(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI07932", ipResult != NULL);

		// if the query contains a pipe character then an OR query
		// has been defined - in which case, get each of the OR'ed 
		// parts as a separate query in vecQueries
		vector<string> vecQueries;
		StringTokenizer st('|');
		st.parse(strMainQuery, vecQueries);

		// iterate through each of the queries and check for
		// matches.  Add any found matches to ipResult;
		vector<string>::iterator queryIter;
		for (queryIter = vecQueries.begin(); queryIter != vecQueries.end(); queryIter++)
		{
			// get the current query and the patterns in it
			string& stdstrQuery = *queryIter;
			vector<CAFUtility::QueryPattern> vecPatterns;
			vector<CAFUtility::QueryPattern> vecNonSelectPatterns;
			splitQuery(stdstrQuery, vecPatterns, vecNonSelectPatterns);

			// ensure that there's at least one pattern
			if (vecPatterns.empty())
			{
				UCLIDException ue("ELI07940", "Invalid query!");
				ue.addDebugInfo("Query", stdstrQuery);
				throw ue;
			}

			// the query has been broken into the individual
			// patterns that need to be matched.
			// next iterate through the attributes and perform
			// the matching
			processAttributesForMatches(vecPatterns, vecNonSelectPatterns, 0,
				ipResult, bRemoveMatches, ipInput);
		}

		return ipResult;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26425");
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::removeAttribute(const IIUnknownVectorPtr& ipAttributes,
								 const IAttributePtr& ipAttribute)
{
	try
	{
		ASSERT_ARGUMENT("ELI26435", ipAttributes != NULL);
		ASSERT_ARGUMENT("ELI26436", ipAttribute != NULL);

		// Attempt to find the parent attribute
		IAttributePtr ipParent = getAttributeParent(ipAttributes, ipAttribute);
		if (ipParent != NULL)
		{
			// Get the sub attributes collection and remove the value
			IIUnknownVectorPtr ipSubAttributes = ipParent->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI26429", ipSubAttributes != NULL);
			ipSubAttributes->RemoveValue(ipAttribute);
		}
		else
		{
			// the attribute may be a root level attribute remove it from the root collection
			ipAttributes->RemoveValue(ipAttribute);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26430");
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CAFUtility::getAttributeParent(const IIUnknownVectorPtr& ipAttributes,
											 const IAttributePtr& ipAttribute)
{
	try
	{
		ASSERT_ARGUMENT("ELI26431", ipAttributes != NULL);
		ASSERT_ARGUMENT("ELI26432", ipAttribute != NULL);

		IAttributePtr ipParent = NULL;
		long lSize = ipAttributes->Size();
		for (long i = 0; i < lSize; i++)
		{
			IAttributePtr ipTmpAttr = ipAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI26433", ipTmpAttr != NULL);

			ipParent = getParent(ipTmpAttr, ipAttribute);
			if (ipParent != NULL)
			{
				break;
			}
		}

		return ipParent;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26434");
}
//-------------------------------------------------------------------------------------------------
void CAFUtility::removeMetadataAttributes(const IIUnknownVectorPtr &ripAttributeVector)
{
	try
	{
		long lSize = ripAttributeVector->Size();
		for (long i = 0; i < lSize; i++)
		{
			IAttributePtr ipAttribute = ripAttributeVector->At(i);
			ASSERT_ARGUMENT("ELI28451", ipAttribute != NULL);

			// Per discussion with Arvind, assume any attribute whose name begins with an underscore
			// is metadata.
			string strAttributeName = asString(ipAttribute->Name);
			if (!strAttributeName.empty() && strAttributeName[0] == '_')
			{
				ripAttributeVector->Remove(i);
				lSize--;
				i--;
			}
			// Search for and remove any child metadata attributes.
			else
			{
				removeMetadataAttributes(ipAttribute->SubAttributes);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28493");
}
//-------------------------------------------------------------------------------------------------