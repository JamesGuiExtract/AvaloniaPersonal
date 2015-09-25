// RuleSet.cpp : Implementation of CRuleSet
#include "stdafx.h"
#include "AFCore.h"
#include "RuleSet.h"
#include "EditorLicenseID.h"
#include "Common.h"
#include "AttributeFinderEngine.h"
#include "RuleSetProfiler.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <EncryptedFileManager.h>
#include <StringTokenizer.h>
#include <ComponentLicenseIDs.h>
#include <VectorOperations.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 13;
// Version 3:
//   Added Output Handler persistence
// Version 7:
//	 Added ability to decrement the redaction counter per document. Modified existing decrement 
//   method to be called pages to distinguish between the two.
// Version 8:
//   Added swiping rule flag
// Version 9:
//	 Added FKB version
// Version 10:
//	 Added Ignore preprocessor and output handler error options.
// Version 11: Added CIdentifiableObject
// Version 12: Added m_strPreviousFileName
// Version 13: Added Comments

const string gstrRULESET_FILE_SIGNATURE = "UCLID AttributeFinder RuleSet Definition (RSD) File";
const string gstrRULESET_FILE_SIGNATURE_2 = "UCLID AttributeFinder RuleSet Definition (RSD) File 2";

// Mutex for thread safety for the License Manager
// Only used in the constructor and destructor to make sure that the 
// License Manager is only created once or destroyed once
CMutex CRuleSet::ms_mutexLM;

// Used to synchronize construction/destruction of rulesets (for threadsafe checks against
// ms_referenceCount)
CMutex CRuleSet::ms_mutexConstruction;

// License manager instance
unique_ptr<SafeNetLicenseMgr> CRuleSet::m_apSafeNetMgr(__nullptr);

// Max accumulation = 10 (use char index 4 followed by char index 2 as a long);
#define _MAX_COUNTER_ACCUMULATION "D804155D-471C-4C81-A07D-9392AD45661C"

CRuleSet::CounterData CRuleSet::ms_indexingCounterData = { 0 };
CRuleSet::CounterData CRuleSet::ms_paginationCounterData = { 0 };
CRuleSet::CounterData CRuleSet::ms_redactionCounterData = { 0 };

int CRuleSet::ms_referenceCount = 0;

//-------------------------------------------------------------------------------------------------
// CRuleSet
//-------------------------------------------------------------------------------------------------
CRuleSet::CRuleSet()
:m_ipAttributeNameToInfoMap(__nullptr), 
m_bstrStreamName("RuleSet"),
m_bDirty(false),
m_bIsEncrypted(false),
m_bUseIndexingCounter(false),
m_bUsePaginationCounter(false),
m_bUsePagesRedactionCounter(false),
m_bUseDocsRedactionCounter(false),
m_bRuleSetOnlyForInternalUse(false),
m_bSwipingRule(false),
m_strKeySerialNumbers(""),
m_strFKBVersion(""),
m_bIgnorePreprocessorErrors(false),
m_bIgnoreOutputHandlerErrors(false),
m_nVersionNumber(gnCurrentVersion) // by default, all rulesets are the current version number
{
	try
	{
		CSingleLock lg(&ms_mutexConstruction, TRUE);

		ms_referenceCount++;

		// If full RDT is not licensed, we may be able to preset a USB counter
		if (!isRdtLicensed())
		{
			// If FLEX Index rule writing is licensed and not ID Shield rule writing
			if (LicenseManagement::isLicensed( gnFLEXINDEX_RULE_WRITING_OBJECTS ) && 
				!LicenseManagement::isLicensed( gnIDSHIELD_RULE_WRITING_OBJECTS ))
			{
				// Can preset Indexing USB counter
				m_bUseIndexingCounter = true;
			}

			// If ID Shield rule writing is licensed and not FLEX Index rule writing
			if (!LicenseManagement::isLicensed( gnFLEXINDEX_RULE_WRITING_OBJECTS ) && 
				LicenseManagement::isLicensed( gnIDSHIELD_RULE_WRITING_OBJECTS ))
			{
				// Can preset Redaction By Pages USB counter
				m_bUsePagesRedactionCounter = true;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12959");
}
//-------------------------------------------------------------------------------------------------
CRuleSet::~CRuleSet()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg(&ms_mutexConstruction, TRUE);

		if (ms_referenceCount <= 0)
		{
			UCLIDException("ELI38721", "Failed RuleSet consistency check.").log();
			ms_referenceCount = 0;
		}
		else
		{
			ms_referenceCount--;
		}

		// Before releasing the connection to m_apSafeNetMgr, flush any accumulated values that have
		// not yet been decremented.
		if (ms_referenceCount == 0)
		{
			try
			{
				CSingleLock lg(&ms_mutexLM, TRUE);

				flushCounter(gdcellFlexIndexingCounter, ms_indexingCounterData);
				flushCounter(gdcellFlexPaginationCounter, ms_paginationCounterData);
				flushCounter(gdcellIDShieldRedactionCounter, ms_redactionCounterData);

				// If no more instances of RuleSet exist release the SafeNetMgr ( license )
				m_apSafeNetMgr.reset(__nullptr);
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33414")
		}

		// clean up the dialog resource in this scope so that the
		// code executes in the correct AFX state
		m_apDlg.reset( __nullptr );
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI05534")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRuleSet,
		&IID_IRuleSetUI,
		&IID_ILicensedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_ISupportErrorInfo
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IRuleSet
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::LoadFrom(BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		CSingleLock lg( &m_mutex, TRUE);

		// Check if the file is encrypted
		string strFileName = asString(strFullFileName);
		string strExt = ::getExtensionFromFullPath(strFileName, true);
		bool bIsEncrypted = (strExt == ".etf");

		// Perform any appropriate auto-encrypt actions on the input file
		if (bIsEncrypted)
		{
			getMiscUtils()->AutoEncryptFile(strFullFileName,
				get_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));
		}

		IPersistStreamPtr ipPersistStream = getThisAsCOMPtr();
		ASSERT_RESOURCE_ALLOCATION("ELI16904", ipPersistStream != __nullptr);

		// Load the ruleset
		readObjectFromFile(ipPersistStream, strFullFileName, m_bstrStreamName, bIsEncrypted);
		m_bIsEncrypted = bIsEncrypted;

		// mark this object as dirty depending upon bSetDirtyFlagToTrue
		m_bDirty = (bSetDirtyFlagToTrue == VARIANT_TRUE);

		// update the filename associated with this ruleset
		m_strFileName = asString(strFullFileName);

		// Wait for the file to be accessible
		waitForFileAccess(m_strFileName, giMODE_READ_ONLY);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04155");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::SaveTo(BSTR strFullFileName, VARIANT_BOOL bClearDirty,
	VARIANT_BOOL* pbGUIDsRegenerated)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33826", pbGUIDsRegenerated != __nullptr);
		*pbGUIDsRegenerated = VARIANT_FALSE;

		validateLicense();

		if (bClearDirty == VARIANT_TRUE)
		{
			// [FlexIDSCore:4865]
			// If the filename has changed, in order to prevent the IdentifiableObject GUIDs in a
			// copied ruleset from conflicting with the GUIDs in the original ruleset, regenerate the
			// GUIDs for all rule objects in this ruleset.
			if (_strcmpi(m_strPreviousFileName.c_str(), asString(strFullFileName).c_str()) != 0)
			{
				// Regenerate the GUID for the ruleset itself.
				getGUID(true);

				// Create a clone off this ruleset, then copy the date from that clone. This causes all
				// contained rule objects to be re-created which, in turn, creates new GUIDs for them.
				ICopyableObjectPtr ipCopyThis = getThisAsCOMPtr();
				ICopyableObjectPtr ipCopy = ipCopyThis->Clone();
				CopyFrom(ipCopy);

				*pbGUIDsRegenerated = VARIANT_TRUE;
			}

			// update the filename associated with this ruleset
			// NOTE: we only want to update the filename when bClearDirty is
			// true because this method gets called for "temporary saving" 
			// with the auto-save-on-timer feature.  The auto-save method
			// calls to this method will have bClearDirty set to false.
			m_strFileName = asString(strFullFileName);
			m_strPreviousFileName = m_strFileName;
		}

		writeObjectToFile(this, strFullFileName, m_bstrStreamName, asCppBool(bClearDirty));

		// mark this object as dirty depending upon bDontChangeDirtyFlag
		if (bClearDirty == VARIANT_TRUE)
		{
			m_bDirty = false;
		}

		// Wait until the file is readable
		waitForStgFileAccess(strFullFileName);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04156");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::ExecuteRulesOnText(IAFDocument* pAFDoc, 
										  IVariantVector *pvecAttributeNames,
										  BSTR bstrAlternateComponentDataDir,
										  IProgressStatus *pProgressStatus,
										  IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		PROFILE_RULE_OBJECT(m_strFileName, "RuleSet", this, 0)

		// Use double try...catch block so if any exceptions are thrown the Rule file name can
		// be added to the exception. 
		try
		{
			try
			{
				// Throw exception if rule execution is not allowed [FlexIDSCore #3061]
				if (!isRuleExecutionAllowed())
				{
					throw UCLIDException("ELI21520", 
						"Rule execution is not allowed - make sure that a USB counter is selected.");
				}

				if (m_strFKBVersion.empty() && (isUsingCounter() || m_bSwipingRule))
				{
					throw UCLIDException("ELI33506", "An FKB version must be specified for a swiping rule or a "
						"ruleset that decrements counters.");
				}

				// Wrap pvecAttributeNames in a smart pointer
				IVariantVectorPtr ipvecAttributeNames = pvecAttributeNames;

				// Get access to the names of attributes in the ruleset
				IVariantVectorPtr ipAttributeNames = m_ipAttributeNameToInfoMap->GetKeys();
				if (ipAttributeNames == __nullptr)
				{
					throw UCLIDException("ELI04375", "Unable to retrieve names of attributes from ruleset.");
				}

				// Determine the total number of attributes for which rules will need to be run
				// The number of attributes need to be known now so that we can do a good job
				// of providing progress information.
				long nNumAttributesToRunRulesFor = (ipvecAttributeNames != __nullptr) ?
					ipvecAttributeNames->Size : ipAttributeNames->Size;

				// Wrap the progress status object in a smart pointer
				IProgressStatusPtr ipProgressStatus(pProgressStatus);

				// determine whether enabled pre-processor and output handlers exist
				bool bEnabledDocumentPreprocessorExists = enabledDocumentPreprocessorExists();
				bool bEnabledOutputHandlerExists = enabledOutputHandlerExists();

				// Progress related constants
				// NOTE: the constants below are weighted such that the time it takes to run the rules for an average attribute
				// is approximately double the amount of time it takes to execute either the pre-processor or the output handler.
				const long nNUM_PROGRESS_ITEMS_INITIALIZE = 1;
				const long nNUM_PROGRESS_ITEMS_PRE_PROCESSOR = 1;
				const long nNUM_PROGRESS_ITEMS_PER_ATTRIBUTE = 2;
				const long nNUM_PROGRESS_ITEMS_ATTRIBUTES = nNumAttributesToRunRulesFor * nNUM_PROGRESS_ITEMS_PER_ATTRIBUTE;
				const long nNUM_PROGRESS_ITEMS_OUTPUT_HANDLER = 1;
				long nTOTAL_PROGRESS_ITEMS = nNUM_PROGRESS_ITEMS_INITIALIZE + // initializing is always going to happen
					nNUM_PROGRESS_ITEMS_ATTRIBUTES; // attribute rules are always going to be run
				nTOTAL_PROGRESS_ITEMS += bEnabledDocumentPreprocessorExists ? nNUM_PROGRESS_ITEMS_PRE_PROCESSOR : 0;
				nTOTAL_PROGRESS_ITEMS += bEnabledOutputHandlerExists ? nNUM_PROGRESS_ITEMS_OUTPUT_HANDLER : 0;

				// Update the progress status
				if (ipProgressStatus)
				{
					ipProgressStatus->InitProgressStatus("", 0, nTOTAL_PROGRESS_ITEMS, VARIANT_TRUE);
					ipProgressStatus->StartNextItemGroup("Initializing RuleSet execution...", nNUM_PROGRESS_ITEMS_INITIALIZE);
				}

				// Only execute rules with version number >=4 when counter support was added
				// for security reasons, don't store the number 4 in code...compute 4 as I minus E
				char c1 = 'E';
				char c2 = 'I';
				if (m_nVersionNumber < (c2 - c1))
				{
					UCLIDException ue("ELI11653", "This version of the rule execution engine is not backward compatible with older rules.");
					ue.addDebugInfo("VersionNumber", m_nVersionNumber);
					throw ue;
				}

				// Record a new rule execution session
				UCLID_AFCORELib::IRuleExecutionSessionPtr ipSession(CLSID_RuleExecutionSession);
				ASSERT_RESOURCE_ALLOCATION("ELI07493", ipSession != __nullptr);
				long nStackSize = ipSession->SetRSDFileName(m_strFileName.c_str());

				// [FlexIDSCore:5318]
				// If an alternate component data directory root has been specified to be used in
				// addition to the default component data directory, apply that directory.
				if (!asString(bstrAlternateComponentDataDir).empty())
				{
					ipSession->SetAlternateComponentDataDir(bstrAlternateComponentDataDir);
				}

				// If the ruleset is marked as an to-be-used-internally ruleset, then ensure 
				// that stacksize > 1.
				// Can directly execute internal-use rules if USB Key Serial Numbers are disabled (P13 #3474)
				// Direct execution of internal-use rules now requires an RDT license [FlexIDSCore #3200]
				if (m_bRuleSetOnlyForInternalUse && nStackSize == 1 && !isRdtLicensed())
				{
					UCLIDException ue("ELI11546", "This ruleset cannot be used directly by an application.");
					throw ue;
				}

				// If an FKB version is specified, apply it.
				if (!m_strFKBVersion.empty())
				{
					ipSession->SetFKBVersion(m_strFKBVersion.c_str());
				}
				// If an FKB version is not specified, but this is a top level rule, clear any
				// previously assigned FKB version on this thread. If there is a legacy FKB version
				// installed and the component data directory is needed by the rule the legacy
				// version will be implicitly assigned at that time.
				else if (nStackSize == 1)
				{
					ipSession->SetFKBVersion("");
				}

				// Create an AFDocument and pass it along to the attribute rule info
				UCLID_AFCORELib::IAFDocumentPtr ipAFDoc(pAFDoc);
				ASSERT_ARGUMENT("ELI05874", ipAFDoc != __nullptr);

				// https://extract.atlassian.net/browse/ISSUE-12265
				// Ensure the dimensions (in pixels) of each page as reported by the OCR engine
				// match the page dimensions to be used by the rules so that redactions appear where
				// they are supposed to appear.
				if (nStackSize == 1)
				{
					ISpatialStringPtr ipDocText(ipAFDoc->Text);
					ASSERT_RESOURCE_ALLOCATION("ELI37086", ipDocText != __nullptr);

					ipDocText->ValidatePageDimensions();
				}

				// If any counters are set decrement them here
				decrementCounters(ipAFDoc->Text);

				// Try/catch for preprocessors
				try
				{
					try
					{
						// Pre-process the doc if there's any preprocessor
						if (bEnabledDocumentPreprocessorExists)
						{
							// Update the progress status
							if (ipProgressStatus)
							{
								ipProgressStatus->StartNextItemGroup(
									"Executing the pre-processor rules...",
									nNUM_PROGRESS_ITEMS_PRE_PROCESSOR);
							}

							// Create a pointer to the Sub-ProgressStatus object, depending upon
							// whether the caller requested progress information
							IProgressStatusPtr ipSubProgressStatus = (ipProgressStatus == __nullptr) ? 
								__nullptr : ipProgressStatus->SubProgressStatus;

							// Execute the document preprocessor
							UCLID_AFCORELib::IDocumentPreprocessorPtr ipDocPreprocessor(m_ipDocPreprocessor->Object);
							if (ipDocPreprocessor)
							{
								PROFILE_RULE_OBJECT(
									asString(m_ipDocPreprocessor->Description), "", ipDocPreprocessor, 0)

								ipDocPreprocessor->Process(ipAFDoc, ipSubProgressStatus);
							}
						}
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32952");
				}
				catch (UCLIDException &ue)
				{
					if (m_bIgnorePreprocessorErrors)
					{
						ue.log();
					}
					else
					{
						throw ue;
					}
				}

				// Create a vector to keep all the attribute search results to return to caller later
				IIUnknownVectorPtr ipFoundAttributes(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI04374", ipFoundAttributes != __nullptr);

				// Iterate through all the attributes and execute their associated rules
				int iNumAttributeNames = ipAttributeNames->Size;
				for (int i = 0; i < iNumAttributeNames; i++)
				{
					// get the attribute find info associated with the current attribute
					_bstr_t _bstrAttributeName = ipAttributeNames->GetItem(i);

					// Before continuing, make sure that either all the attributes were
					// requested to be processed, or that the current attribute is one 
					// among the list of attributes requested to be processed.
					if (pvecAttributeNames != __nullptr && 
						pvecAttributeNames->Contains(_bstrAttributeName) == VARIANT_FALSE)
					{
						// skip processing this attribute
						continue;
					}

					// Update Progress Status
					if (ipProgressStatus)
					{
						string strStatusText = "Executing rules for field " + asString(_bstrAttributeName) + string("...");
						ipProgressStatus->StartNextItemGroup(strStatusText.c_str(), nNUM_PROGRESS_ITEMS_PER_ATTRIBUTE);
					}

					// get the attribute finding information for the current attribute
					UCLID_AFCORELib::IAttributeFindInfoPtr ipAttributeFindInfo = 
						m_ipAttributeNameToInfoMap->GetValue(_bstrAttributeName);
					if (ipAttributeFindInfo == __nullptr)
					{
						UCLIDException ue("ELI04376", "Unable to retrieve attribute rules info.");
						string stdstrAttribute = _bstrAttributeName;
						ue.addDebugInfo("Attribute", stdstrAttribute);
						throw ue;
					}

					// Create a pointer to the Sub-ProgressStatus object, depending upon whether
					// the caller requested progress information
					IProgressStatusPtr ipSubProgressStatus = (ipProgressStatus == __nullptr) ? 
						__nullptr : ipProgressStatus->SubProgressStatus;

					PROFILE_RULE_OBJECT(asString(_bstrAttributeName), "Attribute finder block",
						ipAttributeFindInfo, 0)

					// find all attributes values for the current attribute
					IIUnknownVectorPtr ipAttributes = 
						ipAttributeFindInfo->ExecuteRulesOnText(ipAFDoc, ipSubProgressStatus);

					// for each attribute that was found, update the "name" part of the
					// attribute object to be the name of the current attribute
					long nNumAttributes = ipAttributes->Size();
					for (int j = 0; j < nNumAttributes; j++)
					{
						UCLID_AFCORELib::IAttributePtr ipAttribute = ipAttributes->At(j);
						ipAttribute->Name = _bstrAttributeName;
					}

					// append results to the result vector of found attributes.
					ipFoundAttributes->Append(ipAttributes);
				}

				// Try/catch for output handlers
				try
				{
					try
					{
						// Pass the found attributes to a defined Output Handler
						if (bEnabledOutputHandlerExists)
						{
							// Update the progress status
							if (ipProgressStatus)
							{
								ipProgressStatus->StartNextItemGroup(
									"Executing the output handler rules...",
									nNUM_PROGRESS_ITEMS_OUTPUT_HANDLER);
							}

							// Execute the output hander
							UCLID_AFCORELib::IOutputHandlerPtr ipOH( m_ipOutputHandler->Object );
							if (ipOH)
							{
								// Create a pointer to the Sub-ProgressStatus object, depending upon
								// whether the caller requested progress information
								IProgressStatusPtr ipSubProgressStatus = (ipProgressStatus == __nullptr) ? 
									__nullptr : ipProgressStatus->SubProgressStatus;

								PROFILE_RULE_OBJECT(asString(m_ipOutputHandler->Description), "", ipOH, 0)

								ipOH->ProcessOutput( ipFoundAttributes, ipAFDoc, ipSubProgressStatus );
							}
						}
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32953");
				}
				catch(UCLIDException& ue)
				{
					if (m_bIgnoreOutputHandlerErrors)
					{
						ue.log();
					}
					else
					{
						throw ue;
					}
				}

				// Update the progress status
				if (ipProgressStatus)
				{
					ipProgressStatus->CompleteCurrentItemGroup();
				}

				// Provide final results to caller
				*pAttributes = ipFoundAttributes.Detach();
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24524");
		}
		catch(UCLIDException &ue)
		{
			ue.addDebugInfo("RuleFileName", m_strFileName);
			throw ue;
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04157");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::Cleanup()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		flushCounter(gdcellFlexIndexingCounter, ms_indexingCounterData);
		flushCounter(gdcellFlexPaginationCounter, ms_paginationCounterData);
		flushCounter(gdcellIDShieldRedactionCounter, ms_redactionCounterData);

		CRuleSetProfiler::GenerateOuput();
		CRuleSetProfiler::Reset();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33402");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_AttributeNameToInfoMap(IStrToObjectMap **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// if the ruleset is encrypted, this method should not be
		// called.
		if (m_bIsEncrypted)
		{
			throw UCLIDException("ELI07610", "Cannot retrieve requested information - RuleSet object is encrypted.");
		}

		// if the map object has not yet been created, create it
		if (m_ipAttributeNameToInfoMap == __nullptr)
		{
			m_ipAttributeNameToInfoMap.CreateInstance(CLSID_StrToObjectMap);
			ASSERT_RESOURCE_ALLOCATION("ELI04383", m_ipAttributeNameToInfoMap != __nullptr);
		}

		IStrToObjectMapPtr ipShallowCopy = m_ipAttributeNameToInfoMap;
		*pVal = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04377")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_AttributeNameToInfoMap(IStrToObjectMap *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// store the pointer internally after ensuring that it is not NULL.
		if (newVal == NULL)
		{
			throw UCLIDException("ELI04382", "Invalid String-to-Object map.");
		}

		m_ipAttributeNameToInfoMap = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04378")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_GlobalDocPreprocessor(IObjectWithDescription **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// if the ruleset is encrypted, this method should not be
		// called.
		if (m_bIsEncrypted)
		{
			throw UCLIDException( "ELI07632", 
				"Cannot retrieve requested information - RuleSet object is encrypted." );
		}

		if (m_ipDocPreprocessor == __nullptr)
		{
			m_ipDocPreprocessor.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI07390", m_ipDocPreprocessor != __nullptr);
		}

		IObjectWithDescriptionPtr ipShallowCopy = m_ipDocPreprocessor;
		*pVal = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07388")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_GlobalDocPreprocessor(IObjectWithDescription *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipDocPreprocessor = newVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07389")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_GlobalOutputHandler(IObjectWithDescription **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Cannot provide this information if encrypted rule set
		if (m_bIsEncrypted)
		{
			throw UCLIDException( "ELI07723", 
				"Cannot retrieve requested information - RuleSet object is encrypted." );
		}

		if (m_ipOutputHandler == __nullptr)
		{
			m_ipOutputHandler.CreateInstance( CLSID_ObjectWithDescription );
			ASSERT_RESOURCE_ALLOCATION( "ELI07724", m_ipOutputHandler != __nullptr );
		}

		IObjectWithDescriptionPtr ipShallowCopy = m_ipOutputHandler;
		*pVal = ipShallowCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07725")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_GlobalOutputHandler(IObjectWithDescription *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_ipOutputHandler = newVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07726")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_FileName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = get_bstr_t(m_strFileName.c_str()).copy();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07477")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_FileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strFileName = asString(newVal);

		// NOTE: we don't need to set the dirty flag because the filename
		// property is a "dynamic" and "non-persistent" property, and therefore
		// doesn't affect the dirty flag of this object
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07478")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_IsEncrypted(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIsEncrypted);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15159")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_UseIndexingCounter(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = m_bUseIndexingCounter ? VARIANT_TRUE : VARIANT_FALSE;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11354")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_UseIndexingCounter(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// NOTE: Unlike other methods/properties, calling this method requires
		// RuleSet Editor license.
		void validateUILicense();

		m_bUseIndexingCounter = newVal == VARIANT_TRUE;
		m_bDirty = true;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11355")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_UsePagesRedactionCounter(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = m_bUsePagesRedactionCounter ? VARIANT_TRUE : VARIANT_FALSE;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11356")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_UsePagesRedactionCounter(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// NOTE: Unlike other methods/properties, calling this method requires
		// RuleSet Editor license.
		void validateUILicense();

		m_bUsePagesRedactionCounter = newVal == VARIANT_TRUE; 
		m_bDirty = true;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11357")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_UseDocsRedactionCounter(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = m_bUseDocsRedactionCounter ? VARIANT_TRUE : VARIANT_FALSE;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14494")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_UseDocsRedactionCounter(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// NOTE: Unlike other methods/properties, calling this method requires
		// RuleSet Editor license.
		void validateUILicense();

		m_bUseDocsRedactionCounter = newVal == VARIANT_TRUE; 
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14495")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_UsePaginationCounter(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = m_bUsePaginationCounter ? VARIANT_TRUE : VARIANT_FALSE;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11358")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_UsePaginationCounter(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// NOTE: Unlike other methods/properties, calling this method requires
		// RuleSet Editor license.
		void validateUILicense();

		m_bUsePaginationCounter = newVal == VARIANT_TRUE; 
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11359")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_ForInternalUseOnly(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = m_bRuleSetOnlyForInternalUse ? VARIANT_TRUE : VARIANT_FALSE;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11547")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_ForInternalUseOnly(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Check to see if this setting has changed
		bool bNewValue = asCppBool( newVal );
		if (bNewValue != m_bRuleSetOnlyForInternalUse)
		{
			// NOTE: Unlike other methods/properties, calling this method requires
			// full RDT license.
			if (!isRdtLicensed())
			{
				UCLIDException ue( "ELI21522", "Modifying ForInternalUseOnly is not licensed." );
				ue.addDebugInfo( "Rule Set Filename", m_strFileName );
				throw ue;
			}

			// Change the setting and set the dirty flag
			m_bRuleSetOnlyForInternalUse = bNewValue;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11548")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_KeySerialList(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strKeySerialNumbers.c_str()).copy();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11631")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_KeySerialList(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// NOTE: Unlike other methods/properties, calling this method requires
		// RuleSet Editor license.
		void validateUILicense();

		m_strKeySerialNumbers = asString(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11632")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_VersionNumber(long *nVersion)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*nVersion = m_nVersionNumber;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11651")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_IsSwipingRule(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bSwipingRule);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27013")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_IsSwipingRule(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Check to see if this setting has changed
		bool bNewValue = asCppBool(newVal);
		if (bNewValue != m_bSwipingRule)
		{
			// Setting this property requires full RDT license.
			if (!isRdtLicensed())
			{
				UCLIDException ue("ELI27012", "Not licensed to modify swiping rule status.");
				ue.addDebugInfo("Rule set filename", m_strFileName);
				throw ue;
			}

			// Change the setting and set the dirty flag
			m_bSwipingRule = bNewValue;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27014")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_CanSave(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(!m_bIsEncrypted && isLicensedToSave());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27019")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_FKBVersion(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32471", pVal != __nullptr);

		validateLicense();

		*pVal = _bstr_t(m_strFKBVersion.c_str()).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32482")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_FKBVersion(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strFKBVersion = asString(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32483")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_IgnorePreprocessorErrors(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIgnorePreprocessorErrors);
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32954")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_IgnorePreprocessorErrors(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIgnorePreprocessorErrors = asCppBool(newVal); 

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32955")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_IgnoreOutputHandlerErrors(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIgnoreOutputHandlerErrors);
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32956")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_IgnoreOutputHandlerErrors(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIgnoreOutputHandlerErrors = asCppBool(newVal); 

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32957")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_Comments(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strComments.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34019")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_Comments(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// NOTE: Unlike other methods/properties, calling this method requires
		// RuleSet Editor license.
		void validateUILicense();

		m_strComments = asString(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34020")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_RuleExecutionCounters(IIUnknownVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI38718", pVal != __nullptr);

		validateLicense();

		IIUnknownVectorPtr ipShallowCopy = m_ipRuleExecutionCounters;
		*pVal = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38719")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_RuleExecutionCounters(IIUnknownVector *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		void validateUILicense();

		m_ipRuleExecutionCounters = pNewVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38720")
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	*pClassID = CLSID_RuleSet;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// check m_bDirty flag first, if it's not dirty then
		// check all objects owned by this object
		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		if (!m_bDirty)
		{
			IPersistStreamPtr ipPersistStream(m_ipAttributeNameToInfoMap);
			if (ipPersistStream==NULL)
			{
				throw UCLIDException("ELI04784", "Object does not support persistence.");
			}

			hr = ipPersistStream->IsDirty();
			if (hr == S_OK)
			{
				return hr;
			}

			if (m_ipDocPreprocessor)
			{
				// Check Document Preprocessor
				ipPersistStream = __nullptr;
				ipPersistStream = m_ipDocPreprocessor;
				if (ipPersistStream == __nullptr)
				{
					throw UCLIDException( "ELI06130", "Object does not support persistence." );
				}

				hr = ipPersistStream->IsDirty();
				if (hr == S_OK)
				{
					return hr;
				}
			}

			if (m_ipOutputHandler)
			{
				// Check Output Handler
				ipPersistStream = __nullptr;
				ipPersistStream = m_ipOutputHandler;
				if (ipPersistStream == __nullptr)
				{
					throw UCLIDException( "ELI07928", "Object does not support persistence." );
				}

				hr = ipPersistStream->IsDirty();
				if (hr == S_OK)
				{
					return hr;
				}
			}
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04768");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg( &m_mutex, TRUE);

		// Check license
		validateLicense();

		m_ipDocPreprocessor = __nullptr;

		// Set all counters to default of false
		m_bUseIndexingCounter = false;
		m_bUsePaginationCounter = false;
		m_bUsePagesRedactionCounter = false;
		m_bUseDocsRedactionCounter = false;

		// by default rulesets can be used internally or externally
		m_bRuleSetOnlyForInternalUse = false;

		// By default rulesets are not swiping rules
		m_bSwipingRule = false;

		m_strFKBVersion = "";

		// whether or not this is a version 2 and beyond
		bool bVersion2AndBeyond = false;

		// read signature from stream and ensure that it is correct
		CComBSTR bstrSignature;
		bstrSignature.ReadFromStream(pStream);

		string strSignatureFromFile = CString(bstrSignature);
		if (strSignatureFromFile == gstrRULESET_FILE_SIGNATURE)
		{
			bVersion2AndBeyond = false;
		}
		else if (strSignatureFromFile == gstrRULESET_FILE_SIGNATURE_2)
		{
			bVersion2AndBeyond = true;
		}
		else
		{
			UCLIDException ue("ELI04481", "Invalid RuleSet Definition file.");
			ue.addDebugInfo("Signature", strSignatureFromFile);
			throw ue;
		}

		m_nVersionNumber = 0; // default to current version

		// it's safe to read version if it's version 2 and beyond
		if (bVersion2AndBeyond)
		{
			// Read the bytestream data from the IStream object
			long nDataLength = 0;
			pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
			ByteStream data( nDataLength );
			pStream->Read( data.getData(), nDataLength, NULL );
			ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

			/////////////////////////////////////////////////////
			// Read the individual data items from the bytestream
			/////////////////////////////////////////////////////
			dataReader >> m_nVersionNumber;

			// Check for newer version
			if (m_nVersionNumber > gnCurrentVersion)
			{
				// Throw exception
				UCLIDException ue("ELI07628", "Unable to load newer RuleSet.");
				ue.addDebugInfo("Current Version", gnCurrentVersion);
				ue.addDebugInfo("Version to Load", m_nVersionNumber);
				throw ue;
			}

			if ( m_nVersionNumber >= 4 )
			{
				// Read counter settings
				dataReader >> m_bUseIndexingCounter;
				dataReader >> m_bUsePaginationCounter;
				dataReader >> m_bUsePagesRedactionCounter;

				if( m_nVersionNumber >= 7 )
				{
					// Version 7 adds documents redaction counter
					dataReader >> m_bUseDocsRedactionCounter;
				}
			}

			if ( m_nVersionNumber >= 5 )
			{
				// Read internal use flag
				dataReader >> m_bRuleSetOnlyForInternalUse;
			}

			if (m_nVersionNumber >= 8)
			{
				// Read swiping rule flag
				dataReader >> m_bSwipingRule;
			}

			if ( m_nVersionNumber >= 6 )
			{
				// Read USB serial numbers
				dataReader >> m_strKeySerialNumbers;
			}

			if (m_nVersionNumber >= 9)
			{
				dataReader >> m_strFKBVersion;
			}

			if (m_nVersionNumber >= 10)
			{
				dataReader >> m_bIgnorePreprocessorErrors;
				dataReader >> m_bIgnoreOutputHandlerErrors;
			}

			if (m_nVersionNumber >= 12)
			{
				dataReader >> m_strPreviousFileName;
			}

			if (m_nVersionNumber >= 13)
			{
				dataReader >> m_strComments;
			}
		}

		// load the string-to-object map (attribute name to attribute find info map)
		// from the stream
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI09955");
		m_ipAttributeNameToInfoMap = ipObj;

		// if the version # is 2 or higher, then read the
		// DocumentPreprocessor object-with-description in
		if (m_nVersionNumber >= 2)
		{
			ipObj = __nullptr;
			readObjectFromStream(ipObj, pStream, "ELI09956");
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI07391", 
					"DocumentPreprocessor object could not be read from stream.");
			}
			m_ipDocPreprocessor = ipObj;
		}

		// If version # is 3 or higher, then read the
		// OutputHandler object-with-description
		if (m_nVersionNumber >= 3)
		{
			ipObj = __nullptr;
			readObjectFromStream( ipObj, pStream, "ELI09957" );
			if (ipObj == __nullptr)
			{
				throw UCLIDException( "ELI07736", 
					"Output Handler object could not be read from stream.");
			}
			m_ipOutputHandler = ipObj;
		}

		if (m_nVersionNumber >= 11)
		{
			// Load the GUID for the IIdentifiableObject interface.
			loadGUID(pStream);
		}

		// clear the dirty flag as we just loaded a fresh object
		m_bDirty = false;

		// if we just finished loading this object from a stream,
		// reset the filename field to ""
		m_strFileName = "";

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04770");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// If this file was loaded from an encrypted file
		// it may not be saved
		if (m_bIsEncrypted)
		{
			throw UCLIDException("ELI11581", "Encrypted RuleSet files may not be saved.");
		}

		// Must be using a counter or have the RDT licensed to save external rules [FIDSC #3592]
		if (!isLicensedToSave())
		{
			throw UCLIDException("ELI21505", "A USB counter must be selected.");
		}

		if (fClearDirty == TRUE && m_strFKBVersion.empty() && (isUsingCounter() || m_bSwipingRule))
		{
			// [FlexIDSCore:4744]
			// We only want to throw and exception when fClearDirty is true because this method gets
			// called for "temporary saving" with the auto-save-on-timer feature. The auto-save
			// method calls to this method will have fClearDirty set to false.

			throw UCLIDException("ELI32484", "An FKB version must be specified for a swiping rule or a "
				"ruleset that decrements counters.");
		}

		// write signature to stream
		CComBSTR bstrSignature(gstrRULESET_FILE_SIGNATURE_2.c_str());
		bstrSignature.WriteToStream(pStream);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		m_nVersionNumber = gnCurrentVersion; // save always with the latest version
		dataWriter << m_nVersionNumber;

		// Write counter flags to stream ( added in version 4 of the file )
		dataWriter << m_bUseIndexingCounter;
		dataWriter << m_bUsePaginationCounter;

		// Write the redaction counter flags
		dataWriter << m_bUsePagesRedactionCounter;
		dataWriter << m_bUseDocsRedactionCounter;

		// flag for internal-use-only added in version 5.
		dataWriter << m_bRuleSetOnlyForInternalUse;

		dataWriter << m_bSwipingRule;

		dataWriter << m_strKeySerialNumbers;

		dataWriter << m_strFKBVersion;

		dataWriter << m_bIgnorePreprocessorErrors;
		dataWriter << m_bIgnoreOutputHandlerErrors;

		dataWriter << m_strFileName;

		dataWriter << m_strComments;

		// flush bytes
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// save the string-to-object map (attribute name to attribute find info map)
		// to the stream
		IPersistStreamPtr ipObj = m_ipAttributeNameToInfoMap;
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI04700", "String-To-Object Map component does not support persistence.");
		}
		writeObjectToStream(ipObj, pStream, "ELI09910", fClearDirty);

		// Separately write the DocumentPreprocessor object-with-description to the stream
		ipObj = getDocPreprocessor();
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI07392", "DocumentPreprocessor object does not support persistence.");
		}
		writeObjectToStream(ipObj, pStream, "ELI09911", fClearDirty);

		// Separately write the Output Handler object-with-description to the stream
		ipObj = getOutputHandler();
		if (ipObj == __nullptr)
		{
			throw UCLIDException( "ELI07737", 
				"Output Handler object does not support persistence." );
		}
		writeObjectToStream( ipObj, pStream, "ELI09912", fClearDirty );

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04769");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IRuleSetUI
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::ShowUIForEditing(BSTR strFileName, BSTR strBinFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing for this object
		validateLicense();

		// Check licensing for Rule Set Editor object
		validateUILicense();

		// create the dialog object if it has not yet been created
		if (m_apDlg.get() == NULL)
		{
			string stdstrFileName = asString(strFileName);
			string stdstrFolder = asString(strBinFolder);

			m_apDlg = std::unique_ptr<CRuleSetEditor>( 
				new CRuleSetEditor( stdstrFileName, stdstrFolder ) );
		}

		// Show the Rule Set Editor dialog
		m_apDlg->DoModal();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04159");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

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
STDMETHODIMP CRuleSet::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate license first
		validateLicense();

		// Create the other RuleSet object
		UCLID_AFCORELib::IRuleSetPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08224", ipSource != __nullptr);

		// Set this object's map of Attribute Names to Infos
		ICopyableObjectPtr ipCopyableObject = ipSource->AttributeNameToInfoMap;
		ASSERT_RESOURCE_ALLOCATION("ELI08225", ipCopyableObject != __nullptr);

		m_ipAttributeNameToInfoMap = ipCopyableObject->Clone();

		// Set this object's filename
		m_strFileName = asString(ipSource->FileName);

		// Set the other object's global pre-processor
		ipCopyableObject = ipSource->GlobalDocPreprocessor;
		ASSERT_RESOURCE_ALLOCATION("ELI08226", ipCopyableObject != __nullptr);
		m_ipDocPreprocessor = ipCopyableObject->Clone();

		// Set the other object's global output handler
		ipCopyableObject = ipSource->GlobalOutputHandler;
		ASSERT_RESOURCE_ALLOCATION("ELI08227", ipCopyableObject != __nullptr);
		m_ipOutputHandler = ipCopyableObject->Clone();

		// Copy Counter flags
		m_bUseIndexingCounter = ipSource->UseIndexingCounter == VARIANT_TRUE;
		m_bUsePaginationCounter = ipSource->UsePaginationCounter == VARIANT_TRUE;
		m_bUsePagesRedactionCounter = ipSource->UsePagesRedactionCounter == VARIANT_TRUE;
		m_bUseDocsRedactionCounter = ipSource->UseDocsRedactionCounter == VARIANT_TRUE;

		// copy internal-use-only flag
		m_bRuleSetOnlyForInternalUse = (ipSource->ForInternalUseOnly == VARIANT_TRUE);

		// Copy the swiping rule flag
		m_bSwipingRule = asCppBool(ipSource->IsSwipingRule);

		m_strKeySerialNumbers = asString(ipSource->KeySerialList);
		m_vecSerialNumbers.clear();

		m_strFKBVersion = asString(ipSource->FKBVersion);

		// copy the version number
		m_nVersionNumber = ipSource->VersionNumber;

		m_bIgnorePreprocessorErrors = asCppBool(ipSource->IgnorePreprocessorErrors);
		m_bIgnoreOutputHandlerErrors = asCppBool(ipSource->IgnoreOutputHandlerErrors);

		m_strComments = asString(ipSource->Comments);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08228");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate license first
		validateLicense();

		// Create a new IRuleSet object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_RuleSet );
		ASSERT_RESOURCE_ALLOCATION( "ELI05022", ipObjCopy != __nullptr );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05023");
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33528")
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
UCLID_AFCORELib::IRuleSetPtr CRuleSet::getThisAsCOMPtr()
{
	UCLID_AFCORELib::IRuleSetPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16963", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
IObjectWithDescriptionPtr CRuleSet::getDocPreprocessor()
{
	try
	{
		// if the ruleset is encrypted, this method should not be
		// called.
		if (m_bIsEncrypted)
		{
			throw UCLIDException( "ELI16931", 
				"Cannot retrieve requested information - RuleSet object is encrypted." );
		}

		if (m_ipDocPreprocessor == __nullptr)
		{
			m_ipDocPreprocessor.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI16932", m_ipDocPreprocessor != __nullptr);
		}

		return m_ipDocPreprocessor;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16933")
}
//-------------------------------------------------------------------------------------------------
IObjectWithDescriptionPtr CRuleSet::getOutputHandler()
{
	try
	{
		// Cannot provide this information if encrypted rule set
		if (m_bIsEncrypted)
		{
			throw UCLIDException( "ELI16934", 
				"Cannot retrieve requested information - RuleSet object is encrypted." );
		}

		if (m_ipOutputHandler == __nullptr)
		{
			m_ipOutputHandler.CreateInstance( CLSID_ObjectWithDescription );
			ASSERT_RESOURCE_ALLOCATION( "ELI16935", m_ipOutputHandler != __nullptr );
		}

		return m_ipOutputHandler;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16936")
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04901", "Rule Set" );
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::validateUILicense()
{
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI06413", "Rule Set Editor" );
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CRuleSet::getMiscUtils()
{
	if (m_ipMiscUtils == __nullptr)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI08484", m_ipMiscUtils != __nullptr );
	}

	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::decrementCounters( ISpatialStringPtr ipText )
{
	// Check to see if USB Counters are to be ignored 
	// This is machine-level locking P16 #1905 - WEL 10/17/06
	if (usbCountersDisabled())
	{
		return;
	}

	// Only check counters if at least one counter is checked
	if (isUsingCounter())
	{
		// Update counters as needed
		if ( m_bUseIndexingCounter )
		{
			decrementCounter(gdcellFlexIndexingCounter, 1, ms_indexingCounterData);
		}

		if ( m_bUsePaginationCounter )
		{
			decrementCounter(gdcellFlexPaginationCounter, 1, ms_paginationCounterData);
		}

		if ( m_bUsePagesRedactionCounter )
		{
			// Decrement counter once if non-spatial (P16 #1907)
			long nNumberOfPages = 1;

			// Decrement counter once for each page if spatial
			if (ipText->HasSpatialInfo() == VARIANT_TRUE)
			{
				nNumberOfPages = ipText->GetLastPageNumber() - ipText->GetFirstPageNumber() + 1;
			}

			decrementCounter(gdcellIDShieldRedactionCounter, nNumberOfPages, ms_redactionCounterData);
		}

		if( m_bUseDocsRedactionCounter )
		{
			// Decrement counter once for the document.
			decrementCounter(gdcellIDShieldRedactionCounter, 1, ms_redactionCounterData);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::decrementCounter(DataCell cell, int nNumToDecrement, CounterData& counterData)
{
	try
	{
		// The max allowed accumulation for the counter value before decrementing the key is
		// defined as Min(A,B,C) where:
		// A = 10 (_MAX_COUNTER_ACCUMULATION)
		// B = D / 10
		// C = E / 100
		// D = number of counts successfully decremented off the USB key from in the current process
		// E = number of counts on the USB key at the time of the last successful decrement of the
		// USB key from the current process.

		string strMaxAccumulation(_MAX_COUNTER_ACCUMULATION);
		int allowedAccumulation = asLong(strMaxAccumulation.substr(4, 1) + strMaxAccumulation[2]);
		allowedAccumulation = min(allowedAccumulation, counterData.m_nCountsDecrementedInProcess / 10);
		allowedAccumulation = min(allowedAccumulation, counterData.m_nLastCountValue / 100);

		int nCurrentAccumulation = counterData.m_nCountDecrementAccumulation + nNumToDecrement;
		if (nCurrentAccumulation < allowedAccumulation)
		{
			counterData.m_nCountDecrementAccumulation = nCurrentAccumulation;
		}
		else
		{
			// First try to decrement using m_ipRuleExecutionCounters.
			int nLastCount = decrementRuleExecutionCounter(cell, nCurrentAccumulation);

			// For legacy purposes, allow a USB key as a fallback if there is no proper counter
			// available in m_ipRuleExecutionCounters.
			if (nLastCount < 0)
			{
				// Lock the mutex for USB License manager to ensure thread safety of counterData.
				CSingleLock lg( &ms_mutexLM, TRUE);

				// if this is the first instance of RuleSet will need to allocate the pointer
				if ( m_apSafeNetMgr.get() == __nullptr )
				{
					m_apSafeNetMgr = unique_ptr<SafeNetLicenseMgr>(new SafeNetLicenseMgr(gusblFlexIndex));
				}

				// Check to see if USB Key serial numbers should be ignored
				bool bDisableUSBSNs = usbSerialNumbersDisabled();
				if ( !bDisableUSBSNs && !m_strKeySerialNumbers.empty())
				{
					// Serial numbers found and must be considered
					validateKeySerialNumber();
				}

				nLastCount = m_apSafeNetMgr->decreaseCellValue(cell, nCurrentAccumulation);
			}

			counterData.m_nLastCountValue = nLastCount;
			counterData.m_nCountsDecrementedInProcess += nCurrentAccumulation;
			counterData.m_nCountDecrementAccumulation = 0;
		}
	}
	catch (...)
	{
		// If there was an exception decrementing counts, ensure the next attempts to decrement
		// aren't accumulated.
		counterData.m_nCountsDecrementedInProcess = 0;

		throw;
	}
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::flushCounter(DataCell cell, CounterData& counterData)
{
	// Lock the mutex for USB License manager to ensure thread safety of counterData.
	CSingleLock lg( &ms_mutexLM, TRUE);

	if (counterData.m_nCountDecrementAccumulation > 0)
	{
		if (m_apSafeNetMgr.get() == nullptr)
		{
			UCLIDException ue("ELI38722", "Unexpected counter state.");
			ue.addDebugInfo("Cell", cell.getCellName());
			ue.log();
		}
		else
		{
			m_apSafeNetMgr->decreaseCellValue(cell, counterData.m_nCountDecrementAccumulation);
			counterData.m_nCountDecrementAccumulation = 0;
		}
	}
}
//-------------------------------------------------------------------------------------------------
int CRuleSet::decrementRuleExecutionCounter(DataCell cell, int nNumToDecrement)
{
	if (m_ipRuleExecutionCounters == nullptr)
	{
		return -1;
	}

	int nResult = -1;

	// Iterate each counter for the counter with the correct ID to decrement.
	long nCount = m_ipRuleExecutionCounters->Size();
	for (long i = 0; nResult < 0 && i < nCount; i++)
	{
		ISecureCounterPtr ipCounter = m_ipRuleExecutionCounters->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI38759", ipCounter != nullptr);

		long nCounterID = ipCounter->CounterID;
		switch (nCounterID)
		{
			case 1: 
				if (cell.getCellName() == "Indexing")
				{
					nResult = ipCounter->DecrementCounter(nNumToDecrement);
				}
				break;

			case 2:
				if (cell.getCellName() == "Pagination")
				{
					nResult = ipCounter->DecrementCounter(nNumToDecrement);
				}
				break;

			case 3:
			case 4:
				if (cell.getCellName() == "Redaction")
				{
					nResult = ipCounter->DecrementCounter(nNumToDecrement);
				}
				break;
		}
	}

	return nResult;
}
//-------------------------------------------------------------------------------------------------
std::vector<DWORD>& CRuleSet::getSerialListAsDWORDS()
{
	m_vecSerialNumbers.clear();
	addRangeToVector(m_vecSerialNumbers, m_strKeySerialNumbers);

	return m_vecSerialNumbers;
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::validateKeySerialNumber()
{
	DWORD dwLicenseSN = m_apSafeNetMgr->getKeySN();
	getSerialListAsDWORDS();
	bool bValidSerial = false;
	long nNumSerials = m_vecSerialNumbers.size();
	for ( long i = 0; i < nNumSerials && !bValidSerial; i++ )
	{
		bValidSerial = m_vecSerialNumbers[i] == dwLicenseSN;
	}

	// Throw exception if rules require a different S/N 
	if ( !bValidSerial )
	{
		UCLIDException ue("ELI11635", "Counter Key Serial # is not allowed with current rules." );
		ue.addDebugInfo( "Serial #", dwLicenseSN);
		ue.addDebugInfo( "Valid Serial Numbers", m_strKeySerialNumbers );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::usbCountersDisabled()
{
	return LicenseManagement::isLicensed(gnIGNORE_USB_DECREMENT_FEATURE); 
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::usbSerialNumbersDisabled()
{
	return LicenseManagement::isLicensed(gnIGNORE_USB_IDCHECK_FEATURE); 
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::isRuleExecutionAllowed()
{
	// [FlexIDSCore #3061] Requires:
	// - A USB counter OR
	// - Internal-use flag OR
	// - Is a swiping rule OR
	// - Full RDT license
	return isUsingCounter() || m_bRuleSetOnlyForInternalUse || m_bSwipingRule || isRdtLicensed();
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::enabledDocumentPreprocessorExists()
{
	return (m_ipDocPreprocessor != __nullptr) && (m_ipDocPreprocessor->Object != __nullptr) &&
		(m_ipDocPreprocessor->GetEnabled() == VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::enabledOutputHandlerExists()
{
	return (m_ipOutputHandler) && (m_ipOutputHandler->Object != __nullptr) && 
		(m_ipOutputHandler->GetEnabled() == VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::isRdtLicensed()
{
	return LicenseManagement::isLicensed(gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS);
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::isUsingCounter()
{
	return m_bUseIndexingCounter || m_bUsePaginationCounter || m_bUsePagesRedactionCounter || 
		m_bUseDocsRedactionCounter;
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::isLicensedToSave()
{
	return isUsingCounter() || m_bRuleSetOnlyForInternalUse || isRdtLicensed();
}
//-------------------------------------------------------------------------------------------------
