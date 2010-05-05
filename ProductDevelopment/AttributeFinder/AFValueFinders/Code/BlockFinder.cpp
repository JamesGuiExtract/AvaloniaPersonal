// BlockFinder.cpp : Implementation of CBlockFinder
#include "stdafx.h"
#include "AFValueFinders.h"
#include "BlockFinder.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CBlockFinder
//-------------------------------------------------------------------------------------------------
CBlockFinder::CBlockFinder()
: m_strBlockSeparator(""),
  m_bInputAsOneBlock(false),
  m_bFindAllBlocks(true),
  m_bIsClueRegularExpression(false),
  m_nMinNumberOfClues(0),
  m_bGetMaxOnly(false),
  m_bIsCluePartOfAWord(false),
  m_bDirty(false),
  m_bPairBeginEndStrings(false),
  m_eDefineBlocks(kSeparatorString)
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI19416", m_ipMiscUtils != NULL );

		m_ipClues.CreateInstance(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI05740", m_ipClues != NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05739")
}
//-------------------------------------------------------------------------------------------------
CBlockFinder::~CBlockFinder()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16339");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IBlockFinder,
		&IID_IAttributeFindingRule,
		&IID_IAttributeModifyingRule,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IAttributFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
										 IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Create local Document pointer
		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI30086", ipAFDoc != NULL);

		// Get the text out from the spatial string
		ISpatialStringPtr ipInputText(ipAFDoc->Text);
		ASSERT_RESOURCE_ALLOCATION("ELI30087", ipInputText != NULL);
		
		IIUnknownVectorPtr ipItems( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION("ELI11005", ipItems != NULL);

		if (ipInputText->IsEmpty() == VARIANT_FALSE)
		{
			ipItems = getBlocks(ipInputText);
			ipItems = chooseBlocks(ipItems, ipAFDoc);
		}

		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI10076", ipAttributes != NULL);

		// Populate collection of Attributes
		for (int n = 0; n < ipItems->Size(); n++)
		{
			// Create and populate an Attribute for this Item
			IAttributePtr ipAttribute(CLSID_Attribute);
			ISpatialStringPtr ipString = ipItems->At(n);
			ipAttribute->Value = ipString;

			// Store Attribute in the return vector
			ipAttributes->PushBack(ipAttribute);
		}

		// Provide the collected Attributes to the caller
		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05693");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput,
										   IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IAttributePtr ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI10079", ipAttribute != NULL);

		ISpatialStringPtr ipInputText(ipAttribute->Value);
		ASSERT_RESOURCE_ALLOCATION("ELI30088", ipAttribute != NULL);

		IAFDocumentPtr ipOriginInput(pOriginInput);
		// Not required to be non-NULL
		
		IIUnknownVectorPtr ipItems(NULL);
		if (ipInputText->IsEmpty() == VARIANT_FALSE)
		{
			ipItems = getBlocks(ipInputText);
			ipItems = chooseBlocks(ipItems, ipOriginInput);
		}

		ISpatialStringPtr ipNewValue(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI10080", ipNewValue != NULL);

		if(ipItems->Size() > 0)
		{
			ipNewValue = ipItems->At(0);
		}
		ipAttribute->Value = ipNewValue;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10075");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = true;

		if(m_eDefineBlocks == kSeparatorString)
		{
			if (m_strBlockSeparator.empty())
			{
				bConfigured = false;
			}
		}
		else if(m_eDefineBlocks == kBeginAndEndString)
		{
			if (m_strBlockBegin.empty() || m_strBlockBegin.empty())
			{
				bConfigured = false;
			}
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI10072");
		}

		if (!m_bFindAllBlocks)
		{
			if (m_nMinNumberOfClues <= 0 || m_ipClues->Size <= 0)
			{
				bConfigured = false;
			}
		}

		*pbValue = bConfigured ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05694");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19575", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Block finder").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05695")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08244", ipSource != NULL);

		m_strBlockSeparator = asString(ipSource->GetBlockSeparator());
		m_bInputAsOneBlock = (ipSource->GetInputAsOneBlock() == VARIANT_TRUE) ? true : false;
		m_bFindAllBlocks = (ipSource->GetFindAllBlocks() == VARIANT_TRUE) ? true : false;
		m_bIsClueRegularExpression = (ipSource->GetIsClueRegularExpression()==VARIANT_TRUE) ? true : false;
	
		m_nMinNumberOfClues = ipSource->GetMinNumberOfClues();

		m_bGetMaxOnly = (ipSource->GetGetMaxOnly() == VARIANT_TRUE) ? true : false;
		m_bIsCluePartOfAWord = (ipSource->GetIsCluePartOfAWord()==VARIANT_TRUE) ? true : false;

		IShallowCopyablePtr ipCopyableObject = ipSource->GetClues();
		ASSERT_RESOURCE_ALLOCATION("ELI08246", ipCopyableObject != NULL);
		m_ipClues = ipCopyableObject->ShallowCopy();

		m_bPairBeginEndStrings = (ipSource->PairBeginAndEnd == VARIANT_TRUE) ? true : false;
		m_strBlockEnd = ipSource->BlockEnd;
		m_strBlockBegin = ipSource->BlockBegin;
		m_eDefineBlocks = (EDefineBlocksType)ipSource->DefineBlocksType;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08245");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_BlockFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI08343", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05698");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_BlockFinder;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();
		
		m_strBlockSeparator = "";
		m_bInputAsOneBlock = false;
		m_bFindAllBlocks = true;
		m_bIsClueRegularExpression = false;
		m_nMinNumberOfClues = 1;
		m_bGetMaxOnly = false;
		m_bIsCluePartOfAWord = false;
		m_ipClues = NULL;
		m_eDefineBlocks = kSeparatorString;
		
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
			UCLIDException ue( "ELI07644", "Unable to load newer BlockFinder." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_strBlockSeparator;
			dataReader >> m_bInputAsOneBlock;
			dataReader >> m_bFindAllBlocks;
			dataReader >> m_bIsClueRegularExpression;
			dataReader >> m_nMinNumberOfClues;
			dataReader >> m_bGetMaxOnly;
			dataReader >> m_bIsCluePartOfAWord;
		}

		if(nDataVersion >= 2)
		{
			long nTmp;
			dataReader >> nTmp;
			m_eDefineBlocks = (EDefineBlocksType) nTmp;
			dataReader >> m_strBlockBegin;
			dataReader >> m_strBlockEnd;
			dataReader >> m_bPairBeginEndStrings;
		}

		// Read the clue list
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI09963");
		if (ipObj == NULL)
		{
			throw UCLIDException("ELI05732", "Clue list could not be read from stream!");
		}
		m_ipClues = ipObj;


		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05699");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;
		dataWriter << m_strBlockSeparator;
		dataWriter << m_bInputAsOneBlock;
		dataWriter << m_bFindAllBlocks;
		dataWriter << m_bIsClueRegularExpression;
		dataWriter << m_nMinNumberOfClues;
		dataWriter << m_bGetMaxOnly;
		dataWriter << m_bIsCluePartOfAWord;

		dataWriter << (long)m_eDefineBlocks;
		dataWriter << m_strBlockBegin;
		dataWriter << m_strBlockEnd;
		dataWriter << m_bPairBeginEndStrings;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Separately write the clue list to the IStream object
		IPersistStreamPtr ipObj(m_ipClues);
		if (ipObj == NULL)
		{
			throw UCLIDException("ELI05733", "Clues collection does not support persistence!" );
		}
		::writeObjectToStream(ipObj, pStream, "ELI09918", fClearDirty);
		

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05700");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IBlockFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_BlockSeparator(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strBlockSeparator.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05704");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_BlockSeparator(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strSep = asString( newVal );

		if (strSep.empty())
		{
			UCLIDException ue("ELI05830", "Please specify a non-empty block separator.");
			ue.addDebugInfo("Separator", strSep);
			throw ue;
		}

		m_strBlockSeparator = strSep;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05705");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_InputAsOneBlock(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bInputAsOneBlock ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05706");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_InputAsOneBlock(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bInputAsOneBlock = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05707");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_FindAllBlocks(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bFindAllBlocks ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05708");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_FindAllBlocks(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bFindAllBlocks = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05709");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_IsClueRegularExpression(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bIsClueRegularExpression ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05710");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_IsClueRegularExpression(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIsClueRegularExpression = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05711");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_MinNumberOfClues(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_nMinNumberOfClues;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05712");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_MinNumberOfClues(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (newVal <= 0)
		{
			UCLIDException ue("ELI05831", "Minimum number of clues must be greater than zero!");
			ue.addDebugInfo("Minimum number", newVal);
			throw ue;
		}

		m_nMinNumberOfClues = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05713");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_GetMaxOnly(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bGetMaxOnly ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05714");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_GetMaxOnly(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bGetMaxOnly = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05715");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::GetBlockScore(BSTR strBlockText, IAFDocument* pAFDoc, long *pScore)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		// Not required to be non-NULL

		// Get a regular expression parser
		IRegularExprParserPtr ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("BlockFinder");
		ASSERT_RESOURCE_ALLOCATION("ELI05738", ipParser != NULL);

		// All comparisons are case insensitive
		ipParser->IgnoreCase = VARIANT_TRUE;

		// Get a list of values that includes values from any specified files.
		IVariantVectorPtr ipExpandedClues = m_cachedListLoader.expandList(m_ipClues, ipAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI30089", ipExpandedClues != NULL);

		string strBlock = asString( strBlockText );
		long nCluesSize = ipExpandedClues->Size;
		string strClue("");
		long nScore = 0;
		for (int n = 0; n < nCluesSize; n++)
		{
			// get each clue
			strClue = asString(_bstr_t(ipExpandedClues->GetItem(n)));
			if (!m_bIsClueRegularExpression)
			{
				// replace any special characters
				convertStringToRegularExpression(strClue);
			}
			
			// if each clue is a word, i.e. not within another word
			if (!m_bIsCluePartOfAWord)
			{
				// add word boudary character (\b) to both sides of the clue
				strClue = "\\b(" + strClue + ")\\b";
			}

			// use a regular expression parser to search for clue match
			ipParser->Pattern = strClue.c_str();
			IIUnknownVectorPtr ipFoundMatches = ipParser->Find(strBlockText, VARIANT_FALSE, 
				VARIANT_FALSE);
			if (ipFoundMatches != NULL && ipFoundMatches->Size() > 0)
			{
				nScore += ipFoundMatches->Size();
			}
		}

		*pScore = nScore;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05716");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_Clues(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IVariantVectorPtr ipShallowCopy = m_ipClues;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05717");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_Clues(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipClues) m_ipClues = NULL;

		m_ipClues = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05718");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_IsCluePartOfAWord(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bIsCluePartOfAWord ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05736");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_IsCluePartOfAWord(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIsCluePartOfAWord = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05737");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_DefineBlocksType(EDefineBlocksType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_eDefineBlocks;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10061");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_DefineBlocksType(EDefineBlocksType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eDefineBlocks = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10062");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_BlockBegin(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strBlockBegin.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10064");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_BlockBegin(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string str = asString( newVal );

		if (str.empty())
		{
			UCLIDException ue("ELI10063", "Please specify a non-empty block begin string.");
			ue.addDebugInfo("Block Begin", str);
			throw ue;
		}

		m_strBlockBegin = str;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10065");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_BlockEnd(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strBlockEnd.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19379");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_BlockEnd(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string str = asString( newVal );

		if (str.empty())
		{
			UCLIDException ue("ELI19378", "Please specify a non-empty block end string.");
			ue.addDebugInfo("Block End", str);
			throw ue;
		}

		m_strBlockEnd = str;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19380");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::get_PairBeginAndEnd(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bPairBeginEndStrings ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10066");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBlockFinder::put_PairBeginAndEnd(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bPairBeginEndStrings = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10067");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CBlockFinder::validateLicense()
{
	static const unsigned long BLOCK_FINDER_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( BLOCK_FINDER_COMPONENT_ID, "ELI05692", "Block Finder" );
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CBlockFinder::getBlocksBeginEnd(ISpatialStringPtr ipSS)
{

	IIUnknownVectorPtr	ipItems(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI10074", ipItems != NULL);

	string strText = asString(ipSS->String);
	unsigned long ulCurrPos = 0;

	while (ulCurrPos < strText.length())
	{
		// get the beginning position of the next block
		unsigned long ulStartPos = strText.find( m_strBlockBegin, ulCurrPos );
		if (ulStartPos == string::npos)
		{
			break;
		}

		// get the ending position of the next block
		unsigned long ulEndPos = string::npos;
		if (m_bPairBeginEndStrings)
		{
			ulEndPos = getCloseScopePos( strText, ulStartPos, m_strBlockBegin, m_strBlockEnd );
		}
		else
		{
			ulEndPos = strText.find(m_strBlockEnd, ulStartPos + m_strBlockBegin.length());
			if (ulEndPos != string::npos)
			{
				ulEndPos += m_strBlockEnd.length();
			}
		}

		if (ulEndPos == string::npos)
		{
			break;
		}

		ISpatialStringPtr ipBlock = ipSS->GetSubString( ulStartPos + m_strBlockBegin.length(), 
			ulEndPos - m_strBlockEnd.length() - 1 );
		ipItems->PushBack(ipBlock);
		ulCurrPos = ulEndPos;
	}

	return ipItems;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CBlockFinder::getBlocksSeparator(ISpatialStringPtr ipSS)
{
	// Tokenize the string
	IIUnknownVectorPtr	ipItems = ipSS->Tokenize( 
		_bstr_t(m_strBlockSeparator.c_str()));

	// if there's no separator found and the entire input
	// text shall not be treated as one block
	if (ipItems->Size() == 0 && !m_bInputAsOneBlock)
	{
	}
	else if (ipItems->Size() == 0)
	{
		// if the entire input text shall be treated as 
		// one block
		ipItems->PushBack(ipSS);
	}
	return ipItems;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CBlockFinder::getBlocks(ISpatialStringPtr ipSS)
{
	IIUnknownVectorPtr ipItems(NULL);
	if(m_eDefineBlocks == kSeparatorString)
	{
		ipItems = getBlocksSeparator(ipSS);
	}
	else if(m_eDefineBlocks == kBeginAndEndString)
	{
		ipItems = getBlocksBeginEnd(ipSS);
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI10073");
	}
	return ipItems;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CBlockFinder::chooseBlocks(IIUnknownVectorPtr ipItems,
											  const IAFDocumentPtr& ipAFDoc)
{
	// final returning vec
	IIUnknownVectorPtr ipFoundBlocks(NULL);

	UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI10078", ipThis != NULL);

	// find what?
	if (!m_bFindAllBlocks)
	{
		
		ipFoundBlocks.CreateInstance(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI10077", ipFoundBlocks != NULL);
		if (m_nMinNumberOfClues <= 0)
		{
			UCLIDException ue("ELI05734", 
				"Minimum number of clues must be greater than zero!");
			ue.addDebugInfo("Minimum number", m_nMinNumberOfClues);
			throw ue;
		}
		
		long nTotalNumOfClues = m_ipClues->Size;
		if (nTotalNumOfClues <= 0)
		{
			throw UCLIDException("ELI05735", "Please define one or more clues.");
		}

		// look for clues in each block
		long nCurrentScore=0, nHighestScore=0;
		for (int n = 0; n < ipItems->Size(); n++)
		{
			
			// Get this Block of text
			ISpatialStringPtr	ipString = ipItems->At(n);
			_bstr_t	bstrBlock(ipString->GetString());

			// Get score of this Block
			nCurrentScore = ipThis->GetBlockScore(bstrBlock, ipAFDoc);

			// score must be at least same as minimum score
			if (nCurrentScore >= m_nMinNumberOfClues)
			{
				if (m_bGetMaxOnly)
				{
					if (nHighestScore > nCurrentScore)
					{
						// if highest score is greater than current
						// block score, this block shall not be placed
						// in the returning vector
						continue;
					}
					else if (nHighestScore < nCurrentScore)
					{
						// clear the vec in order to hold highest score blocks
						ipFoundBlocks->Clear();
						nHighestScore = nCurrentScore;
					}
				}

				// Add this item to the collection of Found blocks
				ipFoundBlocks->PushBack(ipString);
			}
		}
	}
	else
	{
		// Retain each block
		ipFoundBlocks = ipItems;
	}

	return ipFoundBlocks;
}
