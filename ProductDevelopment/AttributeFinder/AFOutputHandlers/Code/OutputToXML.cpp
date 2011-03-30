// OutputToXML.cpp : Implementation of COutputToXML
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "OutputToXML.h"
#include "XMLVersion1Writer.h"
#include "XMLVersion2Writer.h"

#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>
#include <comutils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <TextFunctionExpander.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 6;

//-------------------------------------------------------------------------------------------------
// COutputToXML
//-------------------------------------------------------------------------------------------------
COutputToXML::COutputToXML()
:	m_bDirty(false),
	m_eOutputFormat(kXMLSchema),
	m_bUseNamedAttributes(false),
	m_bFAMTags(false),
	m_bRemoveSpatialInfo(false),
	m_bSchemaName(false)
{
	try
	{
		m_ipAFUtils.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI07902", m_ipAFUtils != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07903")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOutputToXML,
		&IID_IOutputHandler,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ISpecifyPropertyPages,
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
// IOutputToXML
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::get_FileName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license and arguments
		validateLicense();
		ASSERT_ARGUMENT("ELI07890", pVal != __nullptr);

		// return the filename
		*pVal = _bstr_t(m_strFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07887")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::put_FileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		string strFile = asString(newVal);

		// make sure the file exists
		// or that it contains valid string tags
		pair<bool, bool> prTagsCheck = getContainsTagsAndTagsAreValid(strFile);

		// Check if there were any doc tags
		if (prTagsCheck.first)
		{
			// Check if the doc tags were valid
			if (!prTagsCheck.second)
			{
				UCLIDException ue("ELI07899", "The xml file name contains invalid tags!");
				ue.addDebugInfo("XML File", strFile);
				throw ue;
			}
		}
		else
		{
			// if no tags are defined, make sure the filename represents
			// an absolute path that can be written to
			if (!isAbsolutePath(strFile))
			{
				UCLIDException ue("ELI07900", "Specification of a relative path is not allowed!");
				ue.addDebugInfo("XML File", strFile);
				throw ue;
			}
			else if (!canCreateFile(strFile)) 
			{
				UCLIDException ue("ELI07901", "The specified file cannot be written to!");
				ue.addDebugInfo("XML File", strFile);
				throw ue;
			}
		}

		// store the filename
		m_strFileName = strFile;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07888")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::get_Format(EXMLOutputFormat *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI12895", pVal != __nullptr );

		*pVal = m_eOutputFormat;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12896")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::put_Format(EXMLOutputFormat newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_eOutputFormat = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12897")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::get_NamedAttributes(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI23636", pVal != __nullptr);

		// Check license
		validateLicense();

		// Retrieve setting
		*pVal = asVariantBool(m_bUseNamedAttributes);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12905");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::put_NamedAttributes(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store setting
		m_bUseNamedAttributes = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12906");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::get_UseSchemaName(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI23635", pVal != __nullptr);

		// Check license
		validateLicense();

		// Retrieve setting
		*pVal = asVariantBool(m_bSchemaName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12910");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::put_UseSchemaName(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store setting
		m_bSchemaName = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12911");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::get_SchemaName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license and argument
		validateLicense();
		ASSERT_ARGUMENT("ELI12912", pVal != __nullptr);

		// Return the schema name
		*pVal = _bstr_t( m_strSchemaName.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12913")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::put_SchemaName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		// Store the schema name - without validation
		m_strSchemaName = asString( newVal );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12914")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::get_FAMTags(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI26323", pVal != __nullptr);

		// Validate license
		validateLicense();

		*pVal = asVariantBool(m_bFAMTags);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26324");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::put_FAMTags(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		// Validate license
		validateLicense();

		bool bNewVal = asCppBool(newVal);

		// Only set the dirty flag if the value is changing
		if (bNewVal != m_bFAMTags)
		{
			m_bFAMTags = bNewVal;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26318");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::get_RemoveSpatialInfo(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI26325", pVal != __nullptr);

		// Validate license
		validateLicense();

		*pVal = asVariantBool(m_bRemoveSpatialInfo);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26326");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::put_RemoveSpatialInfo(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		// Validate license
		validateLicense();

		bool bNewVal = asCppBool(newVal);

		// Only set the dirty flag if the value is changing
		if (bNewVal != m_bRemoveSpatialInfo)
		{
			m_bRemoveSpatialInfo = bNewVal;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26327");
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::raw_ProcessOutput(IIUnknownVector *pAttributes, 
											 IAFDocument *pAFDoc, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_ARGUMENT("ELI26298", ipAFDoc != __nullptr);

		// ensure  valid parameters
		ASSERT_ARGUMENT("ELI10489", pAttributes != __nullptr);

		// Expand the file name based on the Doc tags
		string strFileName = expandFileName(ipAFDoc);

		// Pass Attributes to appropriate XMLWriter object
		if (m_eOutputFormat == kXMLOriginal)
		{
			XMLVersion1Writer	xml1(m_bRemoveSpatialInfo);
			xml1.WriteFile( strFileName.c_str(), pAttributes );
		}
		else if (m_eOutputFormat == kXMLSchema)
		{
			XMLVersion2Writer	xml2(m_bRemoveSpatialInfo);

			// Provide UseNamedAttributes value
			xml2.UseNamedAttributes( m_bUseNamedAttributes );

			// Provide Schema Name
			if (m_bSchemaName)
			{
				xml2.UseSchemaName( m_strSchemaName.c_str() );
			}

			// Write the XML file
			xml2.WriteFile( strFileName.c_str(), pAttributes );
		}
		else
		{
			// Unsupported output format
			THROW_LOGIC_ERROR_EXCEPTION("ELI12898")
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07852")
		
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI23634", pbValue != __nullptr);

		// Check license
		validateLicense();

		// This object is considered configured 
		//	if the filename is specified
		*pbValue = m_strFileName.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04842");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19549", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Output data to XML file").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07855")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI23633", pbValue != __nullptr);

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
STDMETHODIMP COutputToXML::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IOutputToXMLPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI07898", ipSource != __nullptr);
		
		// Set filename
		m_strFileName = asString(ipSource->FileName);

		// Set format
		m_eOutputFormat = (EXMLOutputFormat) ipSource->Format;

		// Set named attributes
		m_bUseNamedAttributes = asCppBool(ipSource->NamedAttributes);

		// Set schema name flag and text
		m_bSchemaName = asCppBool(ipSource->UseSchemaName);
		m_strSchemaName = asString(ipSource->SchemaName);

		// Copy the FAM tags flag
		m_bFAMTags = asCppBool(ipSource->FAMTags);

		// Copy the Remove spatial info value
		m_bRemoveSpatialInfo = asCppBool(ipSource->RemoveSpatialInfo);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12816");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI23637", pObject != __nullptr);

		// Validate license first
		validateLicense();

		// create another instance of this object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_OutputToXML);
		ASSERT_RESOURCE_ALLOCATION("ELI07858", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07857");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_OutputToXML;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 1: Store m_strFileName
// Version 2: Store m_eOutputFormat (version 1 or version 2)
// Version 3: Store m_bUseNamedAttributes
// Version 4: Store m_bSchemaName
//            Store m_strSchemaName
// Version 5: Store m_bFAMTags
// Version 6: Store m_bRemoveSpatialInfo
STDMETHODIMP COutputToXML::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// reset member variables
		m_strFileName = "";
		// Version 2 format by default
		m_eOutputFormat = kXMLSchema;
		m_bUseNamedAttributes = false;

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
			UCLIDException ue("ELI07861", 
				"Unable to load newer OutputToXML Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Revert to Original format for Version 1 of object
		if (nDataVersion == 1)
		{
			m_eOutputFormat = kXMLOriginal;
		}

		/////////////////////
		// Read data elements
		/////////////////////
		if (nDataVersion >= 1)
		{
			// read the filename
			dataReader >> m_strFileName;
		}

		if (nDataVersion >= 2)
		{
			// Read the format
			long nTmp;
			dataReader >> nTmp;
			m_eOutputFormat = (EXMLOutputFormat) nTmp;
		}

		if (nDataVersion >= 3)
		{
			// Read named attributes
			dataReader >> m_bUseNamedAttributes;
		}

		if (nDataVersion >= 4)
		{
			// Read schema flag
			dataReader >> m_bSchemaName;

			// Read schema name
			dataReader >> m_strSchemaName;
		}

		if (nDataVersion >= 5)
		{
			dataReader >> m_bFAMTags;
		}

		if (nDataVersion >= 6)
		{
			dataReader >> m_bRemoveSpatialInfo;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07862");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strFileName;
		dataWriter << (long)m_eOutputFormat;
		if (m_eOutputFormat == kXMLOriginal)
		{
			// For Original format:
			// - Never use named attributes
			// - Never use a schema name (flag plus name)
			dataWriter << false;
			dataWriter << false;
			string strEmpty = "";
			dataWriter << strEmpty;
		}
		else
		{
			dataWriter << m_bUseNamedAttributes;
			dataWriter << m_bSchemaName;
			dataWriter << m_strSchemaName;
		}
		dataWriter << m_bFAMTags;
		dataWriter << m_bRemoveSpatialInfo;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07863");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXML::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void COutputToXML::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI07892", "OutputToXML Output Handler" );
}
//-------------------------------------------------------------------------------------------------
string COutputToXML::expandFileName(IAFDocumentPtr ipDoc)
{
	try
	{
		ASSERT_ARGUMENT("ELI26300", ipDoc != __nullptr);

		string strFileName;

		// Check for expanding FAM tags or AF tags
		if (m_bFAMTags)
		{
			// Get the FAM tag manager
			IFAMTagManagerPtr ipFamTags(CLSID_FAMTagManager);
			ASSERT_RESOURCE_ALLOCATION("ELI26301", ipFamTags != __nullptr);

			// Get the source doc name from the AF doc object
			ISpatialStringPtr ipString = ipDoc->Text;
			ASSERT_RESOURCE_ALLOCATION("ELI26302", ipString != __nullptr);
			_bstr_t bstrSourceDoc = ipString->SourceDocName;

			// Get the expanded file name
			strFileName = asString(ipFamTags->ExpandTags(m_strFileName.c_str(), bstrSourceDoc));
		}
		else
		{
			// Get the AFUtils tag manager
			IAFUtilityPtr ipAFTags(CLSID_AFUtility);
			ASSERT_RESOURCE_ALLOCATION("ELI26303", ipAFTags != __nullptr);

			// Get the expanded file name
			strFileName = asString(ipAFTags->ExpandTags(m_strFileName.c_str(), ipDoc));
		}

		// Expand the text functions
		TextFunctionExpander tfe;
		strFileName = tfe.expandFunctions(strFileName);

		return strFileName;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26304");
}
//-------------------------------------------------------------------------------------------------
pair<bool, bool> COutputToXML::getContainsTagsAndTagsAreValid(const string &strFileName)
{
	try
	{
		// Default the return to no tags and valid tags
		pair<bool, bool> prResult = pair<bool, bool>(false, true);

		// Check for FAM tags
		if (m_bFAMTags)
		{
			// Using FAM tags so check via FAM tag manager
			IFAMTagManagerPtr ipFamTags(CLSID_FAMTagManager);
			ASSERT_RESOURCE_ALLOCATION("ELI26311", ipFamTags != __nullptr);

			prResult.first = asCppBool(ipFamTags->StringContainsTags(strFileName.c_str()));
			if (prResult.first)
			{
				prResult.second = !asCppBool(ipFamTags->StringContainsInvalidTags(
					strFileName.c_str()));
			}
		}
		else
		{
			// Not FAM tags, use AF tag manager
			IAFUtilityPtr ipAFTags(CLSID_AFUtility);
			ASSERT_RESOURCE_ALLOCATION("ELI26312", ipAFTags != __nullptr);

			prResult.first = asCppBool(ipAFTags->StringContainsTags(strFileName.c_str()));
			if (prResult.first)
			{
				prResult.second = !asCppBool(ipAFTags->StringContainsInvalidTags(
					strFileName.c_str()));
			}
		}

		return prResult;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26313");
}
//-------------------------------------------------------------------------------------------------