// DocumentClassifier.cpp : Implementation of CDocumentClassifier
#include "stdafx.h"
#include "AFUtils.h"
#include "DocumentClassifier.h"
#include "AddDocTypesDlg.h"
#include "SpecialStringDefinitions.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CDocumentClassifier
//-------------------------------------------------------------------------------------------------
CDocumentClassifier::CDocumentClassifier()
: m_bDirty(false),
  m_strIndustryCategoryName(""),
  m_ipAFUtility(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CDocumentClassifier::~CDocumentClassifier()
{
	try
	{
		// Ensure COM objects are released
		m_ipAFUtility = NULL;

		// Clear the maps
		m_mapNameToVecDocTypes.clear();
		m_mapNameToVecInterpreters.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16330");
}
//-------------------------------------------------------------------------------------------------
HRESULT CDocumentClassifier::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CDocumentClassifier::FinalRelease()
{
	try
	{
		// Ensure COM objects are released
		m_ipAFUtility = NULL;

		// Clear the maps
		m_mapNameToVecDocTypes.clear();
		m_mapNameToVecInterpreters.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28217");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDocumentClassifier,
		&IID_IDocumentClassificationUtils,
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_IDocumentPreprocessor
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IDocumentPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::raw_Process(IAFDocument* pDocument, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_strIndustryCategoryName.empty())
		{
			throw UCLIDException("ELI07124", "Please specify an Industry Category name before processing this document.");
		}

		// use a smart pointer for the progress status object
		IProgressStatusPtr ipProgressStatus = pProgressStatus;

		// calculate the progress status item count
		const long lPROGRESS_ITEMS_PER_LOAD_FILES = 1;
		const long lPROGRESS_ITEMS_PER_CREATE_TAGS = 1;
		const long lTOTAL_PROGRESS_ITEMS = lPROGRESS_ITEMS_PER_LOAD_FILES +
			lPROGRESS_ITEMS_PER_CREATE_TAGS;

		// initialize the progress status object if the caller requested progress updates
		if (ipProgressStatus)
		{
			ipProgressStatus->InitProgressStatus("Initializing document classifier",
				0, lTOTAL_PROGRESS_ITEMS, VARIANT_FALSE);

			ipProgressStatus->StartNextItemGroup("Loading document type files",
				lPROGRESS_ITEMS_PER_LOAD_FILES);
		}

		// load the index file containing all document type 
		// names for a given industry category name
		try
		{
			try
			{
				loadDocTypeFiles(m_strIndustryCategoryName);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15651");
		}
		catch(UCLIDException& ex)
		{
			ex.addDebugInfo("CategoryName", m_strIndustryCategoryName );
			throw ex;
		}

		// update the progress status if it exists
		if (ipProgressStatus)
		{
			ipProgressStatus->StartNextItemGroup("Create document tags", 
				lPROGRESS_ITEMS_PER_CREATE_TAGS);
		}
		
		// create tags in the document
		try
		{
			IAFDocumentPtr ipDocument(pDocument);
			ASSERT_RESOURCE_ALLOCATION("ELI29402", ipDocument != NULL);

			try
			{
				createDocTags(ipDocument, m_strIndustryCategoryName);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15652");
		}
		catch(UCLIDException& ex)
		{
			ex.addDebugInfo("CategoryName", m_strIndustryCategoryName );
			throw ex;
		}

		// mark the progress as complete if the progress status object exists
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07110");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDocumentClassifier
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::get_IndustryCategoryName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = get_bstr_t(m_strIndustryCategoryName).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07122")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::put_IndustryCategoryName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_strIndustryCategoryName = asString(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07123")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19568", pstrComponentDescription != NULL)

		// Provide description
		*pstrComponentDescription = bstr_t("Document classifier").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07111")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license first
		validateLicense();

		// Get Document Classifier interface on target object
		UCLID_AFUTILSLib::IDocumentClassifierPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08240", ipSource != NULL );

		// Copy industry category name
		m_strIndustryCategoryName = asString(ipSource->IndustryCategoryName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08241");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license first
		validateLicense();

		// Create a new object
		ICopyableObjectPtr ipObjCopy(CLSID_DocumentClassifier);
		ASSERT_RESOURCE_ALLOCATION("ELI08340", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07114");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_DocumentClassifier;
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_strIndustryCategoryName = "";
		
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI07641", "Unable to load newer DocumentClassifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_strIndustryCategoryName;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07115");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );

		dataWriter << gnCurrentVersion;
		dataWriter << m_strIndustryCategoryName;

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07116");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbValue = asVariantBool( !m_strIndustryCategoryName.empty() );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07117");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IDocumentClassificationUtils
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::GetDocumentIndustries(IVariantVector** ppIndustries)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Check license state
		validateLicense();

		// Create the VariantVector to contain industries
		IVariantVectorPtr ipVec( CLSID_VariantVector );
		ASSERT_RESOURCE_ALLOCATION("ELI11917", ipVec != NULL);

		// Add the industries
		ipVec->PushBack( get_bstr_t( gstrCOUNTY_DOC_INDUSTRY ) );
		ipVec->PushBack( get_bstr_t( gstrLEGAL_DESC_INDUSTRY ) );

		// Provide vector to caller
		*ppIndustries = ipVec.Detach();
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11916");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::GetSpecialDocTypeTags(VARIANT_BOOL bAllowMultiplyClassified,
														IVariantVector** ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Check license state
		validateLicense();

		// Get special tags
		vector<string>	vecSpecial;
		appendSpecialTags(vecSpecial, asCppBool(bAllowMultiplyClassified));

		// Create the VariantVector to contain tags
		IVariantVectorPtr ipVec( CLSID_VariantVector );
		ASSERT_RESOURCE_ALLOCATION("ELI11918", ipVec != NULL);

		// Add the tags
		unsigned int ui;
		for (ui = 0; ui < vecSpecial.size(); ui++)
		{
			ipVec->PushBack( vecSpecial[ui].c_str() );
		}

		// Provide vector to caller
		*ppTags = ipVec.Detach();
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11919");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::GetDocumentTypes(BSTR strIndustry, IVariantVector** ppTypes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Check license state
		validateLicense();

		// Validate the Industry name
		string strName = asString( strIndustry );
		ASSERT_ARGUMENT("ELI11922", strName != "");

		// Load the document types
		loadDocTypeFiles( strName );

		// Create the VariantVector to contain document types
		IVariantVectorPtr ipVec( CLSID_VariantVector );
		ASSERT_RESOURCE_ALLOCATION("ELI11920", ipVec != NULL);

		// Retrieve the industry-specific collection of document types
		vector<string>	vecTypes = m_mapNameToVecDocTypes[strName];

		// Add each doc type name to vector
		for (unsigned int ui = 0; ui < vecTypes.size(); ui++)
		{
			ipVec->PushBack( get_bstr_t(vecTypes[ui]) );
		}

		// Provide vector to caller
		*ppTypes = ipVec.Detach();
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11921");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifier::GetDocTypeSelection(BSTR* pbstrIndustry, 
													  VARIANT_BOOL bAllowIndustryModification,
													  VARIANT_BOOL bAllowMultipleSelection,
													  VARIANT_BOOL bAllowSpecialTags,
													  VARIANT_BOOL bAllowMultiplyClassified,
													  IVariantVector** ppTypes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Check license state
		validateLicense();

		// Validate the Industry name
		string strName = asString( *pbstrIndustry );

		// Load the document types
		loadDocTypeFiles( strName );

		// Create the VariantVector to contain selected document type(s)
		IVariantVectorPtr ipVec( CLSID_VariantVector );
		ASSERT_RESOURCE_ALLOCATION("ELI11925", ipVec != NULL);

		// Create the selection dialog with multi-selection parameter
		// Provide list of available types
		AddDocTypesDlg dlg(strName, asCppBool(bAllowSpecialTags), 
			asCppBool(bAllowMultipleSelection), asCppBool(bAllowMultiplyClassified));

		// Disable industry selection, if desired
		if ( asCppBool(bAllowIndustryModification) )
		{
			dlg.lockIndustrySelection();
		}

		// Display the selection dialog
		if (dlg.DoModal() == IDOK)
		{
			// Retrieve selection from dialog
			vector<string>	vecFinal = dlg.getChosenTypes();

			// Add each doc type name to vector
			long lSize = vecFinal.size();
			for (long n = 0; n < lSize; n++)
			{
				ipVec->PushBack( get_bstr_t(vecFinal[n]) );
			}

			// Store selected industry name
			string strReturnedIndustry = dlg.getSelectedIndustry();
			*pbstrIndustry = get_bstr_t(strReturnedIndustry).Detach();
		}
		else
		{
			// retain its previous value
			*pbstrIndustry = get_bstr_t(strName).Detach();
		}

		// Provide vector to caller
		*ppTypes = ipVec.Detach();
	}	
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11926");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// private / helper methods
//-------------------------------------------------------------------------------------------------
void CDocumentClassifier::appendSpecialTags(vector<string>& rvecTags, bool bIncludeMultiplyClassified)
{
	// Add each special tag
	rvecTags.push_back(gstrSPECIAL_ANY_UNIQUE);
	if (bIncludeMultiplyClassified)
	{
		rvecTags.push_back(gstrSPECIAL_MULTIPLE_CLASS);
	}
	rvecTags.push_back(gstrSPECIAL_UNKNOWN);

	return;
}
//-------------------------------------------------------------------------------------------------
void CDocumentClassifier::createDocTags(IAFDocumentPtr ipAFDoc, const string& strSpecificIndustryName)
{
	// make sure the vecDocTypeInterpreters is not empty
	map<string, vector<DocTypeInterpreter> >::iterator itMap 
		= m_mapNameToVecInterpreters.find(strSpecificIndustryName);
	if (itMap == m_mapNameToVecInterpreters.end() || itMap->second.empty())
	{
		UCLIDException ue("ELI07130", "Please load all interpreters before calling createDocTags().");
		ue.addDebugInfo("Industry Category Name", strSpecificIndustryName);
		throw ue;
	}

	vector<DocTypeInterpreter> vecDocTypeInterpreters = itMap->second;

	ASSERT_ARGUMENT("ELI05886", ipAFDoc != NULL);

	int nConfidenceLevel = 0;
	// the vector that stores all document types (in string) at the same level
	IVariantVectorPtr ipVecDocTypes(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI06030", ipVecDocTypes != NULL);

	// the vector that stores all Block IDs (in string) at the same level
	IVariantVectorPtr ipVecBlockIDs( CLSID_VariantVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI10978", ipVecBlockIDs != NULL );

	// the vector that stores all Rule IDs (in string) at the same level
	IVariantVectorPtr ipVecRuleIDs( CLSID_VariantVector );
	ASSERT_RESOURCE_ALLOCATION( "ELI10987", ipVecRuleIDs != NULL );

	// Create a cache for document page ranges.
	// Pattern holders often use the same page range i.e. 1, 1
	// so there is no need to call GetRelativePages(1, 1) on
	// the same text (ipAFDoc->Text) dozens of times.  Thus we will
	// store the returns from GetRelative pages in a cache
	DocPageCache cache;

	// string for component name and its associated prog id
	for (unsigned int ui = 0; ui < vecDocTypeInterpreters.size(); ui++)
	{
		DocTypeInterpreter& dtiInterp = vecDocTypeInterpreters[ui];

		// confidence level for each doc type
		int nCurrentLevel = dtiInterp.getDocConfidenceLevel(ipAFDoc->Text, cache);
		if (nConfidenceLevel > nCurrentLevel)
		{
			// skip this type
			continue;
		}
		else if (nConfidenceLevel < nCurrentLevel)
		{
			// if current level is higher, then 
			// replace the nConfidenceLevel
			nConfidenceLevel = nCurrentLevel;

			// clear the vectors
			ipVecDocTypes->Clear();
			ipVecBlockIDs->Clear();
			ipVecRuleIDs->Clear();
		}
		
		// as long as the confidence level is above kZeroLevel
		if (nConfidenceLevel > 0)
		{
			// Retrieve main Document Type
			string strDocTypeName = dtiInterp.m_strDocTypeName;

			// Check for defined sub-type
			string strSubType = dtiInterp.m_strDocSubType;
			if (strSubType.length() > 0)
			{
				// Append to doc type
				strDocTypeName += ".";
				strDocTypeName += strSubType.c_str();
			}

			// store the doc type in the vector
			ipVecDocTypes->PushBack( strDocTypeName.c_str() );

			// Store the Block ID and Rule ID in the vectors
			ipVecBlockIDs->PushBack( dtiInterp.m_strBlockID.c_str() );
			ipVecRuleIDs->PushBack( dtiInterp.m_strRuleID.c_str() );
		}
	}
	
	// now, we have all doc types stored in a vector AND
	// all associated block IDs stored in a vector.
	// let's update the AFDocument
	// store the confidence level in StringTags
	IStrToStrMapPtr ipStringTags(ipAFDoc->StringTags);
	if (ipStringTags != NULL)
	{
		CString zConfidenceLevel("");
		zConfidenceLevel.Format("%d", nConfidenceLevel);
		ipStringTags->Set( get_bstr_t(DOC_PROBABILITY), get_bstr_t(zConfidenceLevel) );
	}

	IStrToObjectMapPtr ipObjectTags(ipAFDoc->ObjectTags);
	if (ipObjectTags != NULL)
	{
		// Store the doc types, block IDs, and rule IDs in ObjectTags 
		// An assumption is made here that since we add to each vector
		// as a block than it is significant enough to check that one
		// of the vectors has a size > 0 and if one of the vectors
		// satisfies this condition then all vectors satisfy this condition.
		if (ipVecDocTypes->Size > 0)
		{
			ipObjectTags->Set( get_bstr_t(DOC_TYPE), ipVecDocTypes );
			ipObjectTags->Set( get_bstr_t(DCC_BLOCK_ID_TAG), ipVecBlockIDs );
			ipObjectTags->Set( get_bstr_t(DCC_RULE_ID_TAG), ipVecRuleIDs );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDocumentClassifier::loadDocTypeFiles(const string& strSpecificIndustryName)
{
	if (m_ipAFUtility == NULL)
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI07125", m_ipAFUtility != NULL);
	}
	
	// look for the industry name entry
	map<string, vector<DocTypeInterpreter> >::iterator itMap 
		= m_mapNameToVecInterpreters.find(strSpecificIndustryName);
	if (itMap != m_mapNameToVecInterpreters.end() && !itMap->second.empty())
	{
		// if LoadPerSession is on, then only load the document
		// type files once per session
		if ( asCppBool( m_ipAFUtility->GetLoadFilePerSession() ) )
		{
			return;
		}
	}

	// create a new vector of doc type interpreters
	vector<DocTypeInterpreter> vecDocTypeInterpreters;

	string strComponentDataFolder = m_ipAFUtility->GetComponentDataFolder();
	string strIndustrySpecificFolder = strComponentDataFolder 
								+ "\\" + DOC_CLASSIFIERS_FOLDER 
								+ "\\" + strSpecificIndustryName;
	// get doc type index file based on the industry category name
	string strDocTypeIndexFile = strIndustrySpecificFolder + "\\" + DOC_TYPE_INDEX_FILE;

	// Confirm presence of index file
	if (!isValidFile( strDocTypeIndexFile ))
	{
		// Create and throw Exception
		UCLIDException ue("ELI15650", "Unable to find Document Type index file!");
		ue.addDebugInfo( "IndexFile", strDocTypeIndexFile );
		throw ue;
	}

	// a vector of doc type names read from DocTypes.idx
	vector<string> vecDocTypeNames;

	// open the file
	ifstream ifs(strDocTypeIndexFile.c_str());

	// use CommentedTextFileReader to read the file line by line
	CommentedTextFileReader fileReader(ifs, "//", true);
	string strLine("");
	while (!fileReader.reachedEndOfStream())
	{
		strLine = fileReader.getLineText();
		strLine = ::trim(strLine, " \t", " \t");
		if (strLine.empty())
		{
			continue;
		}
		
		// store each document type name
		vecDocTypeNames.push_back(strLine);
	}

	// add/update the DocType entry
	m_mapNameToVecDocTypes[strSpecificIndustryName] = vecDocTypeNames;
	
	// Populate the collection of DocTypeInterpreters
	for (unsigned int ui = 0; ui < vecDocTypeNames.size(); ui++)
	{
		// Get full path to default file
		string strDefaultFile = strIndustrySpecificFolder + "\\" + 
			vecDocTypeNames[ui] + ".dcc.etf";

		// Get the appropriate prefixed file, if available
		string strDocTypeFileName = m_ipAFUtility->GetPrefixedFileName( 
			get_bstr_t(strDefaultFile) );
		
		// load the document type file into interpreter
		DocTypeInterpreter docTypeInterpreter;

		// store current doc type name
		docTypeInterpreter.m_strDocTypeName = vecDocTypeNames[ui];
		docTypeInterpreter.loadDocTypeFile(strDocTypeFileName, true);
		vecDocTypeInterpreters.push_back(docTypeInterpreter);
	}

	// add/update the DocTypeInterpreter entry
	m_mapNameToVecInterpreters[strSpecificIndustryName] = vecDocTypeInterpreters;
}
//-------------------------------------------------------------------------------------------------
void CDocumentClassifier::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI05884", "Document Classifier");
}
//-------------------------------------------------------------------------------------------------
