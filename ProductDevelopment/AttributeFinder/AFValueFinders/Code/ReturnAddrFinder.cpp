// ReturnAddrFinder.cpp : Implementation of CReturnAddrFinder

#include "stdafx.h"
#include "AFValueFinders.h"
#include "ReturnAddrFinder.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#import "..\..\AFSplitters\Code\AFSplitters.tlb" named_guids
using namespace UCLID_AFSPLITTERSLib;

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <EncryptedFileManager.h>
#include <CommentedTextFileReader.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
#define MAX_SPACE_MULTIPLIER				3
#define MAX_LEFT_JUST_MULTIPLIER			4
#define MAX_CENTER_JUST_MULTIPLIER			4
#define MAX_LETTER_SPACE_MULTIPLIER			1

const string FIND_WILL_CALLS = "FindWillCalls";
const string USE_CREATE_BLOCKS = "UseCreateBlocks";
const string USE_ADDRESS_FINDER = "UseAddressFinder";
const string USE_FROM_TO_MAIL = "UseFromToMail";
const string USE_ZONE_CORRECTION = "UseZoneCorrection";

const string RETURN_ADDRESS_FINDER = "\\ReturnAddressFinder";

const unsigned long gnCurrentVersion = 2;
const string gstrAF_VALUE_FINDERS_KEY_PATH = gstrAF_REG_ROOT_FOLDER_PATH + string("\\AFValueFinders");

//-------------------------------------------------------------------------------------------------
// CReturnAddrFinder
//-------------------------------------------------------------------------------------------------
CReturnAddrFinder::CReturnAddrFinder()
:	m_bFindNonReturnAddresses(false), m_bDirty(false)
{
	reset();

	m_ipAFUtility.CreateInstance(CLSID_AFUtility);
	ASSERT_RESOURCE_ALLOCATION("ELI08911", m_ipAFUtility != __nullptr);
	m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI08912", m_ipMiscUtils != __nullptr);

	// Get Configuration Manager for dialog
	ma_pUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
		new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrAF_VALUE_FINDERS_KEY_PATH ));
}
//-------------------------------------------------------------------------------------------------
CReturnAddrFinder::~CReturnAddrFinder()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16348");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IReturnAddrFinder,
		&IID_IAttributeFindingRule,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent

	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IReturnAddrFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::get_FindNonReturnAddresses(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bFindNonReturnAddresses ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08943");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::put_FindNonReturnAddresses(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bFindNonReturnAddresses = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08944");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::raw_ParseText(IAFDocument *pAFDoc, IProgressStatus *pProgressStatus,
											  IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// This finder is obsolete so throw exception if this method is called
		UCLIDException ue("ELI28703", "Return address finder is obsolete.");
		throw ue;

		validateLicense();

		// wrap the AF document in a smart pointer
		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI25636", ipAFDoc != __nullptr);

		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI08659", ipAttributes != __nullptr);

		IAttributeSplitterPtr ipSplitter(CLSID_AddressSplitter);
		ASSERT_RESOURCE_ALLOCATION("ELI08730", ipSplitter != __nullptr);
		
		// Load the body regular expression
		string strRegExpBody =
			getRegExp("<ComponentDataDir>\\ReturnAddrFinder\\ReturnAddrBody.dat.etf", ipAFDoc);

		// Load the suffix regular expression
		string strRegExpSuffix =
			getRegExp("<ComponentDataDir>\\ReturnAddrFinder\\ReturnAddrSuffix.dat.etf", ipAFDoc);
		
		// Load the prefix regular expressions
		vector<string> vecPrefixRegExp;
		vecPrefixRegExp.push_back(
			getRegExp("<ComponentDataDir>\\ReturnAddrFinder\\ReturnAddrPrefix.dat.etf", ipAFDoc));
		vecPrefixRegExp.push_back(
			getRegExp("<ComponentDataDir>\\ReturnAddrFinder\\ReturnAddrPrefix2.dat.etf", ipAFDoc));
		vecPrefixRegExp.push_back(
			getRegExp("<ComponentDataDir>\\ReturnAddrFinder\\ReturnAddrPrefix3.dat.etf", ipAFDoc));
		
		// create regular expression parsers
		IRegularExprParserPtr ipPrefixParser =
			m_ipMiscUtils->GetNewRegExpParserInstance("ReturnAddrFinder");
		ASSERT_RESOURCE_ALLOCATION("ELI08621", ipPrefixParser != __nullptr);
		IRegularExprParserPtr ipAddressParser =
			m_ipMiscUtils->GetNewRegExpParserInstance("ReturnAddrFinder");
		ASSERT_RESOURCE_ALLOCATION("ELI25637", ipAddressParser != __nullptr);
		IRegularExprParserPtr ipSuffixParser =
			m_ipMiscUtils->GetNewRegExpParserInstance("ReturnAddrFinder");
		ASSERT_RESOURCE_ALLOCATION("ELI08942", ipSuffixParser != __nullptr);

		// Add the pattern to the suffix parser
		ipSuffixParser->Pattern = strRegExpSuffix.c_str();
	
		bool bUseCreateBlocks = checkRegistryBool(USE_CREATE_BLOCKS, true);

		// Get the Spatial String that represents the entire document
		ISpatialStringPtr ipTempSS = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI15600", ipTempSS != __nullptr);

		// Split the lines of the spatial string (removing unnecessary space)
		IIUnknownVectorPtr ipTmpVec = ipTempSS->GetSplitLines(4);
		ASSERT_RESOURCE_ALLOCATION("ELI15602", ipTmpVec != __nullptr);

		// Build a new spatial string from the split lines
		ISpatialStringPtr ipSS(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI23640", ipSS != __nullptr);
		ipSS->CreateFromLines(ipTmpVec);
		bool bHasSpatialInfo = asCppBool(ipSS->HasSpatialInfo());

		///////////////////////////////////////////////////////////////
		// Look for regular return addresses (those with a prefix)
		///////////////////////////////////////////////////////////////
		// for each set of prefixes
		unsigned int uiPrefix = 0;
		for (uiPrefix = 0; uiPrefix < vecPrefixRegExp.size(); uiPrefix++)
		{
			string strPrefixExp = vecPrefixRegExp[uiPrefix];
			string strAddressRegExp = strPrefixExp + strRegExpBody + strRegExpSuffix;
			
			// Set the patterns for the parsers
			ipPrefixParser->Pattern = strPrefixExp.c_str();
			ipAddressParser->Pattern = strAddressRegExp.c_str();

			///////////////////////////////////////////////////////////////
			// If the string is not spatial just search it for the return address RegExp
			///////////////////////////////////////////////////////////////
			if (!bHasSpatialInfo)
			{
				// Search the string for the address
				IIUnknownVectorPtr ipFoundAddresses = ipAddressParser->Find(ipSS->String, 
					VARIANT_FALSE, VARIANT_FALSE);
				ASSERT_RESOURCE_ALLOCATION("ELI15398", ipFoundAddresses != __nullptr);

				long lFoundSize = ipFoundAddresses->Size();
				for (long i = 0; i < lFoundSize; i++)
				{
					// Get the start and end points
					long nStart, nEnd;
					m_ipMiscUtils->GetRegExpData(ipFoundAddresses, i, -1, &nStart, &nEnd);
					if ( nStart < 0 || nEnd < 0)
					{
						continue;
					}
					
					// Get the sub-string from the start and end points
					ISpatialStringPtr ipAddressString = ipSS->GetSubString(nStart, nEnd);
					ASSERT_RESOURCE_ALLOCATION("ELI15399", ipAddressString != __nullptr);

					// Now remove the prefix from the string
					IIUnknownVectorPtr ipPrefixes = ipPrefixParser->Find(ipAddressString->GetString(),
						VARIANT_TRUE, VARIANT_FALSE);
					ASSERT_RESOURCE_ALLOCATION("ELI25638", ipPrefixes != __nullptr);

					// Get the substing
					long nPrefixStart;
					long nPrefixEnd;
					m_ipMiscUtils->GetRegExpData(ipPrefixes, 0, -1, &nPrefixStart, &nPrefixEnd);

					// Now that we have the prefix, remove it from the found address
					ipAddressString = ipAddressString->GetSubString(nPrefixEnd+1,
						ipAddressString->Size-1);
					ASSERT_RESOURCE_ALLOCATION("ELI25639", ipAddressString != __nullptr);

					IAttributePtr ipAttribute(CLSID_Attribute);
					ASSERT_RESOURCE_ALLOCATION("ELI08960", ipAttribute != __nullptr);
					ipAttribute->Name = "ReturnAddress";
					ipAttribute->Value = ipAddressString;
					ipAttributes->PushBack(ipAttribute);
				}
			}
			else
			{
				// Create a spatial string searcher for the input text
				ISpatialStringSearcherPtr ipSearcher(CLSID_SpatialStringSearcher);
				ASSERT_RESOURCE_ALLOCATION("ELI08701", ipSearcher != __nullptr);
				ipSearcher->SetIncludeDataOnBoundary(VARIANT_TRUE);
				ipSearcher->SetBoundaryResolution(kLine);

				// get the pages of the spatial string
				IIUnknownVectorPtr ipPages(ipSS->GetPages());
				ASSERT_RESOURCE_ALLOCATION("ELI20429", ipPages != __nullptr);

				// find prefixes on each page of the document [P16 #2943]
				IIUnknownVectorPtr ipFound(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI20432", ipFound != __nullptr);
				vector<long> vecPageNumbers;
				long lSize = ipPages->Size();
				for(long i=0; i < lSize; i++)
				{
					// get the ith page
					ISpatialStringPtr ipPage(ipPages->At(i));
					ASSERT_RESOURCE_ALLOCATION("ELI20428", ipPage != __nullptr);

					// find the prefixes on this page
					IIUnknownVectorPtr ipFoundOnPage = 
						ipPrefixParser->Find(ipPage->String, VARIANT_FALSE, VARIANT_FALSE);
					ASSERT_RESOURCE_ALLOCATION("ELI20433", ipFoundOnPage != __nullptr);

					// check if any prefixes were found
					long lFoundSize = ipFoundOnPage->Size();
					if(lFoundSize > 0)
					{
						// add them to the vector of found prefixes
						ipFound->Append(ipFoundOnPage);

						// store the page number associated with each item added
						vecPageNumbers.resize(vecPageNumbers.size()+lFoundSize, 
							ipPage->GetFirstPageNumber());
					}	
				}

				// lCurrPage keeps track of which page the last prefix was found on
				// so that if a second prefix is found on the same page we do not
				// have to re-line twice or create a second spatial string searcher
				long lCurrPage = -1;
				ISpatialStringPtr ipPage = __nullptr;

				// for each found address
				long lFoundSize = ipFound->Size();
				for (long i = 0; i < lFoundSize; i++)
				{
					long lPageNum = vecPageNumbers[i];

					// Set up the page for searching unless we already found a prefix on this page 
					// in which case it is already set up
					if (lPageNum != lCurrPage)
					{
						ipPage = ipSS->GetSpecifiedPages(lPageNum, lPageNum);
						ASSERT_RESOURCE_ALLOCATION("ELI15601", ipPage != __nullptr);

						// prepare to extract a region of text from the page
						ipSearcher->InitSpatialStringSearcher(ipPage);
						lCurrPage = lPageNum;
					}

					// Get the prefix as a spatial string
					long nTokStart;
					long nTokEnd;
					m_ipMiscUtils->GetRegExpData(ipFound, i, -1, &nTokStart, &nTokEnd);
					ISpatialStringPtr ipPrefix = ipPage->GetSubString(nTokStart, nTokEnd);
					ASSERT_RESOURCE_ALLOCATION("ELI15402", ipPrefix != __nullptr);

					ISpatialStringPtr ipAddressString = __nullptr;
					ipAddressString = getAddressNearPrefix(ipSearcher, ipPrefix, ipAddressParser); 

					if(ipAddressString == __nullptr)
					{
						continue;
					}

					ISpatialStringPtr ipNewBlock = __nullptr;
					if (bUseCreateBlocks)
					{
						//////////////////////////////////////////////////////////
						// Now that we have found an address its time to repair it
						//////////////////////////////////////////////////////////
						IIUnknownVectorPtr ipTmpLines = ipAddressString->GetSplitLines(4);
						ASSERT_RESOURCE_ALLOCATION("ELI15396", ipTmpLines != __nullptr);

						// only build blocks if there are more than two lines
						// a non-formatted address could begin at the end of a line
						// on the right side of a page and end on the next line on the 
						// left side of the page
						if (ipTmpLines->Size() > 2)
						{
							// re-line
							// this should be unecessary but I'm lazy and don't 
							// want to think about it right now
							ipAddressString->CreateFromLines(ipTmpLines);
							IIUnknownVectorPtr ipBlocks = ipAddressString->GetBlocks(0);
							ASSERT_RESOURCE_ALLOCATION("ELI15395", ipBlocks != __nullptr)

							// Now choose from among all the blocks, which is the most correct
							ipNewBlock = chooseBlock(ipSuffixParser, ipBlocks);
						}
						else
						{
							ipNewBlock = ipAddressString;
						}
					}
					else
					{
						ipNewBlock = ipAddressString;
					}

					// Now get the prefix again from the new address so we may remove the prefix
					// We only want the first match
					IIUnknownVectorPtr ipPrefixes = ipPrefixParser->Find(ipNewBlock->GetString(), 
						VARIANT_TRUE, VARIANT_FALSE);
					ASSERT_RESOURCE_ALLOCATION("ELI15403", ipPrefixes != __nullptr);

					if(ipPrefixes->Size() > 0)
					{
						long nPrefixStart;
						long nPrefixEnd;
						m_ipMiscUtils->GetRegExpData(ipPrefixes, 0, -1, &nPrefixStart, &nPrefixEnd);
						ipPrefix  = ipNewBlock->GetSubString(nPrefixStart, nPrefixEnd);

						// Now that we have the prefix, remove it from the found address
						ipNewBlock = ipNewBlock->GetSubString(nPrefixEnd+1, ipNewBlock->GetSize()-1);
					}
					// If an Address was found
					if (ipNewBlock)
					{
						///////////////////////////////////////////////////////////////
						// Create an attribute to store the data
						///////////////////////////////////////////////////////////////
						IAttributePtr ipAttribute(CLSID_Attribute);
						ASSERT_RESOURCE_ALLOCATION("ELI08961", ipAttribute != __nullptr);
						ipAttribute->Name = "ReturnAddress";
						ipAttribute->Value = ipNewBlock;
						ipAttributes->PushBack(ipAttribute);
					}
				}
			}

			// If attributes were found break from the loop
			if (ipAttributes->Size()  > 0)
			{
				break;
			}
		}
		
		// If no return addresses were found
		////////////////////////////////////////////////////////////////
		// Find addresses with no return prefix
		////////////////////////////////////////////////////////////////
		if (ipAttributes->Size() <= 0 && // No attributes are found
			m_bFindNonReturnAddresses && // we are supposed to use the backup
			bHasSpatialInfo) // this is a spatial string
		{
			IAttributeFindingRulePtr ipAddressFinder(CLSID_AddressFinder);
			ASSERT_RESOURCE_ALLOCATION("ELI08934", ipAddressFinder != __nullptr);
			ipAttributes = ipAddressFinder->ParseText(ipAFDoc, NULL);
			ASSERT_RESOURCE_ALLOCATION("ELI25640", ipAttributes != __nullptr);
		}

		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07542");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19583", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Return address finder").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07543")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::raw_CopyFrom(IUnknown * pObject)
{
	// nothing to copy
	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IReturnAddrFinderPtr ipCopyObj(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08946", ipCopyObj != __nullptr);

		m_bFindNonReturnAddresses = (ipCopyObj->GetFindNonReturnAddresses() == VARIANT_TRUE) ? true : false;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08258");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_ReturnAddrFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI08348", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07546");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ReturnAddrFinder;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();
		
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI07629", "Unable to load newer ReturnAddressFinder." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Set Defaults
		reset();

		// Read Combined Name Address setting
		if (nDataVersion >= 2)
		{
			dataReader >> m_bFindNonReturnAddresses;
		}
		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07545");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();
		const unsigned long nCurrentVersion = 1;

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );

		dataWriter << gnCurrentVersion;
		dataWriter << m_bFindNonReturnAddresses;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07544");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	if (pcbSize == NULL)
		return E_POINTER;
		
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CReturnAddrFinder::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI07548", "Return Address Finder Rule");
}
//-------------------------------------------------------------------------------------------------
string CReturnAddrFinder::getRegExp(const char* strFileName, IAFDocumentPtr ipAFDoc)
{
	// Expand any tags in the file name
	string strRegExpFile = m_ipAFUtility->ExpandTags(strFileName, ipAFDoc );

	autoEncryptFile(strRegExpFile, gstrAF_AUTO_ENCRYPT_KEY_PATH);

	// getRegExpFromFile is now obsolete as well.
	string strRegExp = "";//getRegExpFromFile(strRegExpFile);

	return strRegExp;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CReturnAddrFinder::chooseBlock(IRegularExprParserPtr ipSuffixParser, IIUnknownVectorPtr ipBlocks)
{
	try
	{
		ASSERT_ARGUMENT("ELI25641", ipSuffixParser != __nullptr);
		ASSERT_ARGUMENT("ELI25642", ipBlocks != __nullptr);

		vector<ISpatialStringPtr> vecNewBlocks;
		unsigned long ulBlockCount = ipBlocks->Size();
		unsigned long uj;
		for (uj = 0; uj < ulBlockCount; uj++)
		{
			ISpatialStringPtr ipBlock = ipBlocks->At(uj);
			ASSERT_RESOURCE_ALLOCATION("ELI08929", ipBlock != __nullptr);
			vecNewBlocks.push_back(ipBlock);
		}

		if (vecNewBlocks.size() == 0)
		{
			return NULL;
		}
		if (vecNewBlocks.size() == 1)
		{
			return vecNewBlocks[0];
		}

		ISpatialStringPtr ipDefaultBlock = vecNewBlocks[0];

		// Eliminate any blocks that are too small to be addresses
		vector<ISpatialStringPtr> vecTmpBlocks;
		for (uj = 0; uj < vecNewBlocks.size(); uj++)
		{
			if (vecNewBlocks[uj]->Size < 20)
			{
				continue;
			}

			vecTmpBlocks.push_back(vecNewBlocks[uj]);
		}
		if (vecTmpBlocks.size() == 1)
		{
			return vecTmpBlocks[0];
		}
		if (vecTmpBlocks.size() == 0)
		{
			return ipDefaultBlock;
		}

		vecNewBlocks.clear();
		for (uj = 0; uj < vecTmpBlocks.size(); uj++)
		{
			vecNewBlocks.push_back(vecTmpBlocks[uj]);
		}
		vecTmpBlocks.clear();

		ipDefaultBlock = vecNewBlocks[0];

		// eliminate any blocks that don't have a valid suffix on them 
		for (uj = 0; uj < vecNewBlocks.size(); uj++)
		{
			ISpatialStringPtr ipSS = vecNewBlocks[uj];
			IIUnknownVectorPtr ipTmpVec = ipSuffixParser->Find(ipSS->String, VARIANT_TRUE, 
				VARIANT_FALSE);
			ASSERT_RESOURCE_ALLOCATION("ELI25643", ipTmpVec != __nullptr);

			if (ipTmpVec->Size() > 0)
			{
				vecTmpBlocks.push_back(vecNewBlocks[uj]);
			}
		}

		if (vecTmpBlocks.size() == 1)
		{
			return vecTmpBlocks[0];
		}

		return ipDefaultBlock;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25644");
}
//-------------------------------------------------------------------------------------------------
void CReturnAddrFinder::reset()
{
	m_bFindNonReturnAddresses = true;
}
//-------------------------------------------------------------------------------------------------
bool CReturnAddrFinder::checkRegistryBool(string key, bool defaultValue)
{
	bool bRet = false;
	if (!ma_pUserCfgMgr->keyExists( RETURN_ADDRESS_FINDER, key ))
	{
		ma_pUserCfgMgr->createKey( RETURN_ADDRESS_FINDER, key, defaultValue?"1":"0" );
	}
	if (ma_pUserCfgMgr->getKeyValue( RETURN_ADDRESS_FINDER, key, defaultValue?"1":"0" ) != "0")
	{
		bRet = true;
	}
	return bRet;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CReturnAddrFinder::getAddressNearPrefix(ISpatialStringSearcherPtr ipSearcher, ISpatialStringPtr ipPrefix,
		IRegularExprParserPtr ipRegExpParser)
{
	try
	{
		ASSERT_ARGUMENT("ELI25645", ipSearcher != __nullptr);
		ASSERT_ARGUMENT("ELI25646", ipPrefix != __nullptr);
		ASSERT_ARGUMENT("ELI25647", ipRegExpParser != __nullptr);

		ISpatialStringPtr ipAddressString = __nullptr;

		// Now build a spatial region based on the location of the 
		// prefix
		ILongRectanglePtr ipPrefixRect = ipPrefix->GetOriginalImageBounds();
		ASSERT_RESOURCE_ALLOCATION("ELI25648", ipPrefixRect != __nullptr);

		long lPrefixCharWidth = ipPrefix->GetAverageCharWidth();
		long lPrefixLineHeight = ipPrefix->GetAverageLineHeight();

		ipPrefixRect->PutBottom(ipPrefixRect->GetBottom() + 10*lPrefixLineHeight);
		ipPrefixRect->PutTop(ipPrefixRect->GetTop() - 5*lPrefixLineHeight);

		// gradually expand the box aroud the prefix until it contains an address
		// or 
		long nRightInc = 40;
		long nLeftInc = 40;
		int i;
		for(i = 0; i < 2; i++)
		{
			ipPrefixRect->PutLeft(ipPrefixRect->GetLeft() - (i*nLeftInc*lPrefixCharWidth));
			ipPrefixRect->PutRight(ipPrefixRect->GetRight() + (i*nRightInc*lPrefixCharWidth));

			// Do not rotate the rectangle per the OCR
			ISpatialStringPtr ipRegion = ipSearcher->GetDataInRegion( ipPrefixRect, VARIANT_FALSE );
			ASSERT_RESOURCE_ALLOCATION("ELI25649", ipRegion != __nullptr);

			// Now that we have the region of the document where the address is likely to be 
			// run the full address regexp on it
			IIUnknownVectorPtr ipAddresses = ipRegExpParser->Find(ipRegion->String, 
				VARIANT_TRUE, VARIANT_FALSE);
			ASSERT_RESOURCE_ALLOCATION("ELI25650", ipAddresses != __nullptr);

			if (ipAddresses->Size() > 0)
			{
				// Get the return address string
				long nTokStart, nTokEnd;
				m_ipMiscUtils->GetRegExpData(ipAddresses, 0, -1, &nTokStart, &nTokEnd);
				ipAddressString = ipRegion->GetSubString(nTokStart, nTokEnd);

				// We have found an address so break (no need to keep expanding the box)
				break;
			}
		}

		return ipAddressString;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25651");
}
//-------------------------------------------------------------------------------------------------
