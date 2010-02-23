// AddressFinder.cpp : Implementation of CAddressFinder
#include "stdafx.h"
#include "AFValueFinders.h"
#include "AddressFinder.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CAddressFinder
//-------------------------------------------------------------------------------------------------
CAddressFinder::CAddressFinder() 
:	m_bDirty(false), m_ipRegExpParser(NULL)
{
	m_ipAFUtility.CreateInstance(CLSID_AFUtility);
	ASSERT_RESOURCE_ALLOCATION("ELI08823", m_ipAFUtility != NULL);
	
	m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI08824", m_ipMiscUtils != NULL);
}
//-------------------------------------------------------------------------------------------------
CAddressFinder::~CAddressFinder()
{
	try
	{
		m_ipAFUtility = NULL;
		m_ipMiscUtils = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16338");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAddressFinder,
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
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::raw_ParseText(IAFDocument * pAFDoc, IProgressStatus *pProgressStatus,
										   IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// This finder is obsolete so throw exception if this method is called
		UCLIDException ue("ELI28697", "Address finder is obsolete.");
		throw ue;

		// Wrap the AFDoc in a smart pointer
		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_ARGUMENT("ELI25631", ipAFDoc != NULL);

		// Validate the license [FlexIDSCore #3534]
		validateLicense();

		// Create the return vector
		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI08883", ipAttributes != NULL);

		// Create a spatial string searcher for later use
		ISpatialStringSearcherPtr ipSearcher(CLSID_SpatialStringSearcher);
		ASSERT_RESOURCE_ALLOCATION("ELI08710", ipSearcher != NULL);
		ipSearcher->SetIncludeDataOnBoundary(VARIANT_TRUE);
		ipSearcher->SetBoundaryResolution(kLine);

		// Load the suffix regular expression from its file
		string strFilenameSuffix = "<ComponentDataDir>\\ReturnAddrFinder\\ReturnAddrSuffix.dat.etf";
		string strRegExpSuffix = loadRegExp(strFilenameSuffix, ipAFDoc);
		m_ipRegExpParser->Pattern = strRegExpSuffix.c_str();

		// Retrieve the AFDocument text
		ISpatialStringPtr ipText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI15587", ipText != NULL);

		// Find all the suffixes
		IIUnknownVectorPtr ipSuffixes = m_ipRegExpParser->Find(ipText->String, VARIANT_FALSE, 
			VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI15588", ipSuffixes != NULL);

		long lSize = ipSuffixes->Size();
		for (long i = 0; i < lSize; i++)
		{
			// Get the suffix string 
			long nTokStart;
			long nTokEnd;
			m_ipMiscUtils->GetRegExpData(ipSuffixes, i, -1, &nTokStart, &nTokEnd);
			ISpatialStringPtr ipSuffixString = ipText->GetSubString(nTokStart, nTokEnd);
			ASSERT_RESOURCE_ALLOCATION("ELI15589", ipSuffixString != NULL);

			// Use the suffix to build a region that could contain an address
			long lLineHeight = ipSuffixString->GetAverageLineHeight();
			long lCharWidth = ipSuffixString->GetAverageCharWidth();

			long lPageNum = ipSuffixString->GetFirstPageNumber();
			ISpatialStringPtr ipPage = ipText->GetSpecifiedPages(lPageNum, lPageNum);
			ASSERT_RESOURCE_ALLOCATION("ELI15590", ipPage != NULL);

			// re-line the page
			IIUnknownVectorPtr ipTmpLines = ipPage->GetSplitLines(4);
			ASSERT_RESOURCE_ALLOCATION("ELI15591", ipTmpLines != NULL);
			ipPage->CreateFromLines(ipTmpLines);
			
			ipSearcher->InitSpatialStringSearcher(ipPage);

			// Get the bounds of the state and zip code
			ILongRectanglePtr ipRect = ipSuffixString->GetOCRImageBounds();
			ASSERT_RESOURCE_ALLOCATION("ELI15592", ipRect != NULL);
			long lLeft, lTop, lRight, lBottom;
			ipRect->GetBounds(&lLeft, &lTop, &lRight, &lBottom);

			// Expand the area up and left to find the address
			lLeft -= 10*lCharWidth;
			lTop -= 4*lLineHeight;
			ipRect->SetBounds(lLeft, lTop, lRight, lBottom);

			// Rotate the rectangle per the OCR
			ISpatialStringPtr ipRegion = ipSearcher->GetDataInRegion(ipRect, VARIANT_TRUE);
			ASSERT_RESOURCE_ALLOCATION("ELI15593", ipRegion != NULL);

			// Only work on strings in kSpatialMode
			// [FlexIDSCore #3589]
			if (ipRegion->GetMode() == kSpatialMode)
			{
				IIUnknownVectorPtr ipBlocks = ipRegion->GetJustifiedBlocks(3);
				ASSERT_RESOURCE_ALLOCATION("ELI15594", ipBlocks != NULL);

				ipBlocks = chooseAddressBlocks(m_ipRegExpParser, ipBlocks);

				long lBlocksSize = ipBlocks->Size();
				for (long j = 0; j < lBlocksSize; j++)
				{
					// Create a ReturnAddress Attribute
					IAttributePtr ipAttribute(CLSID_Attribute);
					ASSERT_RESOURCE_ALLOCATION("ELI15595", ipAttribute != NULL);

					// Add Name and Value
					ipAttribute->Name = "ReturnAddress";
					ISpatialStringPtr ipTmpString = ipBlocks->At(j);
					ASSERT_RESOURCE_ALLOCATION("ELI15596", ipTmpString != NULL);
					ipAttribute->Value = ipTmpString;

					// Add to Attributes collection
					ipAttributes->PushBack(ipAttribute);
				}
			}
		}

		*pAttributes = ipAttributes.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08805");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19574", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Address finder").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08806")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::raw_CopyFrom(IUnknown * pObject)
{
	// nothing to copy
	try
	{
		// validate license first
		validateLicense();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08807");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_AddressFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI08808", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08809");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_AddressFinder;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::Load(IStream * pStream)
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

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08810");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();
		const unsigned long nCurrentVersion = 1;

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << nCurrentVersion;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08811");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	if (pcbSize == NULL)
		return E_POINTER;
		
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CAddressFinder::validateLicense()
{
	VALIDATE_LICENSE( gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI08812", "Address Finder" );
}
//-------------------------------------------------------------------------------------------------
string CAddressFinder::loadRegExp(string strFileName, IAFDocumentPtr ipAFDoc)
{
	// Expand any tags in the file name
	string strRegExpFile = m_ipAFUtility->ExpandTags(strFileName.c_str(), ipAFDoc );
	autoEncryptFile(strRegExpFile, gstrAF_AUTO_ENCRYPT_KEY_PATH);
	
	// getRegExpFromFile is obsolete as well.
	//return getRegExpFromFile(strRegExpFile);
	return "";
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CAddressFinder::chooseAddressBlocks(IRegularExprParserPtr ipSuffixParser, IIUnknownVectorPtr ipBlocks)
{
	// This vector will hold the address blocks that will be returned
	IIUnknownVectorPtr ipRetVec(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI08935", ipRetVec != NULL);

	vector<ISpatialStringPtr> vecNewBlocks;
	int j;
	unsigned int uj;
	for (j = 0; j < ipBlocks->Size(); j++)
	{
		ISpatialStringPtr ipBlock = ipBlocks->At( j );
		ASSERT_RESOURCE_ALLOCATION("ELI08933", ipBlock != NULL);
		vecNewBlocks.push_back(ipBlock);
	}

	if (vecNewBlocks.size() == 0)
	{
		return ipRetVec;
	}

	// Eliminate any blocks that are too small to be addresses
	vector<ISpatialStringPtr> vecTmpBlocks;
	for (uj = 0; uj < vecNewBlocks.size(); uj++)
	{
		if (vecNewBlocks[uj]->GetSize() < 20)
		{
			continue;
		}
		vecTmpBlocks.push_back(vecNewBlocks[uj]);
	}

	if (vecTmpBlocks.size() == 0)
	{
		return ipRetVec;
	}

	vecNewBlocks.clear();
	for (uj = 0; uj < vecTmpBlocks.size(); uj++)
	{
		vecNewBlocks.push_back(vecTmpBlocks[uj]);
	}
	vecTmpBlocks.clear();

	// Eliminate any blocks without enough lines
	// or with lines that are too blocks
	for (uj = 0; uj < vecNewBlocks.size(); uj++)
	{
		IIUnknownVectorPtr ipTmpLines = vecNewBlocks[uj]->GetLines();
		if (ipTmpLines->Size() < 3 || ipTmpLines->Size() > 6)
		{
			continue;
		}

		int k;
		for (k = 0; k < ipTmpLines->Size(); k++)
		{
			ISpatialStringPtr ipLine = ipTmpLines->At(k);
			if (ipLine->GetSize() > 50)
			{
				continue;
			}
		}	
		vecTmpBlocks.push_back(vecNewBlocks[uj]);
	}

	if (vecTmpBlocks.size() == 0)
	{
		return ipRetVec;
	}
	
	vecNewBlocks.clear();
	for (uj = 0; uj < vecTmpBlocks.size(); uj++)
	{
		vecNewBlocks.push_back(vecTmpBlocks[uj]);
	}
	vecTmpBlocks.clear();

	// eliminate any blocks that don't have a state, zip combo
	for (uj = 0; uj < vecNewBlocks.size(); uj++)
	{
		// Get this block
		ISpatialStringPtr ipSS = vecNewBlocks[uj];
		ASSERT_RESOURCE_ALLOCATION("ELI15597", ipSS != NULL);

		IIUnknownVectorPtr ipTmpVec = ipSuffixParser->Find(ipSS->GetString(), VARIANT_TRUE, 
			VARIANT_FALSE);
		if (ipTmpVec->Size() == 0)
		{
			continue;
		}

		// Add this block to the collection
		vecTmpBlocks.push_back(vecNewBlocks[uj]);
	}

	// Move temp blocks into new blocks
	vecNewBlocks.clear();
	for (uj = 0; uj < vecTmpBlocks.size(); uj++)
	{
		vecNewBlocks.push_back(vecTmpBlocks[uj]);
	}
	vecTmpBlocks.clear();

	if (vecNewBlocks.size() == 0)
	{
		return ipRetVec;
	}

	for (uj = 0; uj < vecNewBlocks.size(); uj++)
	{
		ipRetVec->PushBack(vecNewBlocks[uj]);
	}

	return ipRetVec;
}
//-------------------------------------------------------------------------------------------------
