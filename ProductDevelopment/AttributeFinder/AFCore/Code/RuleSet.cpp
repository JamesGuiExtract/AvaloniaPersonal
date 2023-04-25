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
#include <MiscLeadUtils.h>
#include <UPI.h>
#include <LicenseUtils.h>
#include "Attribute.h"

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 17;
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
// Version 14: Added m_bUsePagesIndexingCounter and m_ipCustomCounters
// Version 15: Removed m_strKeySerialNumbers
// Version 16: Added RunMode properties
// Version 17: Added m_ipOCRParameters

const string gstrRULESET_FILE_SIGNATURE = "UCLID AttributeFinder RuleSet Definition (RSD) File";
const string gstrRULESET_FILE_SIGNATURE_2 = "UCLID AttributeFinder RuleSet Definition (RSD) File 2";

// Used to protect the data in CounterData instances.
CCriticalSection CRuleSet::ms_criticalSectionCounterData;

// Used to synchronize construction/destruction of rulesets (for threadsafe checks against
// ms_referenceCount)
CCriticalSection CRuleSet::ms_criticalSectionConstruction;

// Max accumulation = 10 (use char index 4 followed by char index 2 as a long);
#define _MAX_COUNTER_ACCUMULATION "D804155D-471C-4C81-A07D-9392AD45661C"

int CRuleSet::ms_referenceCount = 0;

const string gstrPAGE_CONTENT_TAG = "<PageContent>";
const string gstrPAGE_NUMBER_TAG = "<PageNumber>";

//-------------------------------------------------------------------------------------------------
// CRuleSet
//-------------------------------------------------------------------------------------------------
CRuleSet::CRuleSet()
:m_ipAttributeNameToInfoMap(__nullptr), 
m_bstrStreamName("RuleSet"),
m_bDirty(false),
m_bIsEncrypted(false),
m_bUseDocsIndexingCounter(false),
m_bUsePaginationCounter(false),
m_bUsePagesRedactionCounter(false),
m_bUseDocsRedactionCounter(false),
m_bUsePagesIndexingCounter(false),
m_bRuleSetOnlyForInternalUse(true),	// ISSUE-13087 - RDT: Make 'Internal Use Only' property checked by default
m_bSwipingRule(false),
m_strFKBVersion(""),
m_bIgnorePreprocessorErrors(false),
m_bIgnoreOutputHandlerErrors(false),
m_nVersionNumber(gnCurrentVersion), // by default, all rulesets are the current version number
m_eRuleSetRunMode(kRunPerDocument),
m_bInsertAttributesUnderParent(false),
m_strInsertParentName(""),
m_strInsertParentValue(""),
m_bDeepCopyInput(false),
m_ipParallelRuleSet(__nullptr),
m_sProgressCounts(),
m_ipOCRParameters(__nullptr),
m_ipRuleSetSerializer(__nullptr),
m_bIsPDFSupportInitialized(false)
{
	try
	{
		{
			CSingleLock lg(&ms_criticalSectionConstruction, TRUE);
			ms_referenceCount++;
		}

		// If full RDT is not licensed, we may be able to preset a counter
		if (!isRdtLicensed())
		{
			// If FLEX Index rule writing is licensed and not ID Shield rule writing
			if (LicenseManagement::isLicensed( gnFLEXINDEX_RULE_WRITING_OBJECTS ) && 
				!LicenseManagement::isLicensed( gnIDSHIELD_RULE_WRITING_OBJECTS ))
			{
				// Can preset Indexing counter
				m_bUseDocsIndexingCounter = true;
			}

			// If ID Shield rule writing is licensed and not FLEX Index rule writing
			if (!LicenseManagement::isLicensed( gnFLEXINDEX_RULE_WRITING_OBJECTS ) && 
				LicenseManagement::isLicensed( gnIDSHIELD_RULE_WRITING_OBJECTS ))
			{
				// Can preset Redaction By Pages counter
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
		{
			CSingleLock lg(&ms_criticalSectionConstruction, TRUE);

			if (ms_referenceCount <= 0)
			{
				UCLIDException("ELI38721", "Failed RuleSet consistency check.").log();
				ms_referenceCount = 0;
			}
			else
			{
				ms_referenceCount--;
			}
		}

		// if this is the last reference flush any accumulated values that have not yet been decremented.
		if (ms_referenceCount == 0)
		{
			try
			{
				CSingleLock lg(&ms_criticalSectionCounterData, TRUE);

				flushCounters();
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
		&IID_IRunMode,
		&IID_ILicensedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_ISupportErrorInfo,
		&IID_IHasOCRParameters,
		&IID_ILoadOCRParameters
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
		CSingleLock lg( &m_criticalSection, TRUE);

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

		try
		{
			try
			{
				// First check for JSON to load from
				UCLID_AFCORELib::IRuleSetPtr ipRuleSet;
				if (getRuleSetSerializer()->TryLoadRuleSet(strFullFileName, &ipRuleSet))
				{
					ASSERT_RESOURCE_ALLOCATION("ELI49691", ipRuleSet != __nullptr);

					ICopyableObjectPtr ipThis = getThisAsCOMPtr();
					ipThis->CopyFrom(ipRuleSet);
				}
				else
				{
					// Load the ruleset from IPersistStream
					IPersistStreamPtr ipPersistStream = getThisAsCOMPtr();
					ASSERT_RESOURCE_ALLOCATION("ELI16904", ipPersistStream != __nullptr);

					readObjectFromFile(ipPersistStream, strFullFileName, m_bstrStreamName, bIsEncrypted);

					getRuleSetSerializer()->InitializeUnmodified(getThisAsCOMPtr());
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI50093")
		}
		catch (UCLIDException & ue)
		{
			ue.addDebugInfo("File to load", strFileName);
			throw ue;
		}

		// https://extract.atlassian.net/browse/ISSUE-15984
		// Don't consider encrypted if loaded internally and a full RDT license is present.
		m_bIsEncrypted = bIsEncrypted &&
			(!isRdtLicensed() || !isInternalToolsLicensed());

		// mark this object as dirty depending upon bSetDirtyFlagToTrue
		m_bDirty = asCppBool(bSetDirtyFlagToTrue);

		// update the filename associated with this ruleset
		m_strFileName = strFileName;
	
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

		getRuleSetSerializer()->SaveRuleSet(getThisAsCOMPtr(), strFullFileName, bClearDirty);

		string strFileName = asString(strFullFileName);
		if (bClearDirty)
		{
			// update the filename associated with this ruleset
			// NOTE: we only want to update the filename when bClearDirty is
			// true because this method gets called for "temporary saving" 
			// with the auto-save-on-timer feature.  The auto-save method
			// calls to this method will have bClearDirty set to false.
			m_strFileName = strFileName;
			m_strPreviousFileName = m_strFileName;
		}

		// mark this object as dirty depending upon bDontChangeDirtyFlag
		if (asCppBool(bClearDirty))
		{
			m_bDirty = false;
		}

		// Wait until the file is readable
		waitForFileAccess(strFileName, giMODE_READ_ONLY);
	
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
				// Check whether rule running is allowed
				validateRunability();

				// Wrap pvecAttributeNames in a smart pointer
				IVariantVectorPtr ipvecAttributeNames = pvecAttributeNames;

				// Wrap the progress status object in a smart pointer
				IProgressStatusPtr ipProgressStatus(pProgressStatus);

				// determine whether enabled pre-processor and output handlers exist
				bool bEnabledDocumentPreprocessorExists = enabledDocumentPreprocessorExists();
				bool bEnabledOutputHandlerExists = enabledOutputHandlerExists();

				// Get access to the names of attributes in the ruleset
				IVariantVectorPtr ipDefinedAttributeNames = m_ipAttributeNameToInfoMap->GetKeys();
				if (ipDefinedAttributeNames == __nullptr)
				{
					throw UCLIDException("ELI04375", "Unable to retrieve names of attributes from ruleset.");
				}

				// Figure out which attributes will be run
				IVariantVectorPtr ipAttributesToRun = (ipvecAttributeNames != __nullptr)
					? ipvecAttributeNames
					: ipDefinedAttributeNames;
				long nNumAttributesToRunRulesFor = ipAttributesToRun->Size;

				// Get AFDoc (and associated pages if RunMode is PerPage)
				// Number of pages is used for progress status calculations
				UCLID_AFCORELib::IAFDocumentPtr ipAFDoc(pAFDoc);
				ASSERT_ARGUMENT("ELI05874", ipAFDoc != __nullptr);

				// Start progress status assuming a single document (recalculate later if needed)
				if (ipProgressStatus)
				{
					calculateProgressItems(nNumAttributesToRunRulesFor, 1);

					ipProgressStatus->InitProgressStatus("", 0, m_sProgressCounts.nTotal, VARIANT_TRUE);
					ipProgressStatus->StartNextItemGroup("Initializing RuleSet execution...", m_sProgressCounts.nInitialization);
				}

				// Record a new rule execution session (Used by addCurrentRSDFileToDebugInfo())
				UCLID_AFCORELib::IRuleExecutionSessionPtr ipSession(CLSID_RuleExecutionSession);
				ASSERT_RESOURCE_ALLOCATION("ELI07493", ipSession != __nullptr);
				ipSession->SetRSDFileName(m_strFileName.c_str());

				// Set current RSD in AFDoc (used in place of above REE to support multi-threading)
				long nStackSize = ipAFDoc->PushRSDFileName(_bstr_t(m_strFileName.c_str()));

				// Setup a PopRSDFileName call for when this block exits
				shared_ptr<void> popRSD(__nullptr, [&](void*){ ipAFDoc->PopRSDFileName(); });

				// [FlexIDSCore:5318]
				// If an alternate component data directory root has been specified to be used in
				// addition to the default component data directory, apply that directory.
				if (!asString(bstrAlternateComponentDataDir).empty())
				{
					ipAFDoc->AlternateComponentDataDir = bstrAlternateComponentDataDir;
				}

				// If an FKB version is specified, apply it.
				if (!m_strFKBVersion.empty())
				{
					ipAFDoc->FKBVersion = _bstr_t(m_strFKBVersion.c_str());
				}
				// If an FKB version is not specified, but this is a top level rule, clear any
				// previously assigned FKB version. If there is a legacy FKB version
				// installed and the component data directory is needed by the rule the legacy
				// version will be implicitly assigned at that time.
				else if (nStackSize == 1)
				{
					ipAFDoc->FKBVersion = "";
				}

				// Set OCRParameters in the AFDoc if any are configured in this ruleset
				if (m_ipOCRParameters != __nullptr
					&& m_ipOCRParameters->Size > 0)
				{
					IHasOCRParametersPtr ipHasOCRParameters(ipAFDoc);
					ipHasOCRParameters->OCRParameters = m_ipOCRParameters;
				}

				// If the ruleset is marked as a to-be-used-internally ruleset, then ensure 
				// that stacksize > 1.
				// Direct execution of internal-use rules now requires an RDT license [FlexIDSCore #3200]
				if (m_bRuleSetOnlyForInternalUse && nStackSize == 1 && !isRdtLicensed())
				{
					UCLIDException ue("ELI11546", "This ruleset cannot be used directly by an application.");
					throw ue;
				}

				// https://extract.atlassian.net/browse/ISSUE-12265
				// Ensure the dimensions (in pixels) of each page as reported by the OCR engine
				// match the page dimensions to be used by the rules so that redactions appear where
				// they are supposed to appear.
				if (nStackSize == 1)
				{
					string sourceDocName = asString(ipAFDoc->Text->SourceDocName);
					if (isFileOrFolderValid(sourceDocName) && isPDF(sourceDocName))
					{
						m_bIsPDFSupportInitialized = m_bIsPDFSupportInitialized || initPDFSupport();

						if (!m_bIsPDFSupportInitialized)
						{
							// If PDF support wasn't initialized then verify that the license is OK
							LicenseManagement::verifyFileTypeLicensedRO(sourceDocName);

							throw UCLIDException("ELI53552", "Could not initialize PDF support!");
						}
					}
				}

				// Determine whether rules should be run in parallel
				bool bCanUseParallel = false;
				bool bSharedSemaphoreExists = false;
				if (ipAFDoc->ParallelRunMode == kUnspecifiedParallelization)
				{
					bCanUseParallel = isParallelProcessingEnabled();
					if (bCanUseParallel)
					{
						ipAFDoc->ParallelRunMode = (UCLID_AFCORELib::EParallelRunMode)kPoliteParallelization;
					}
					else
					{
						ipAFDoc->ParallelRunMode = (UCLID_AFCORELib::EParallelRunMode)kNoParallelization;
					}
				}
				else if (ipAFDoc->ParallelRunMode != kNoParallelization)
				{
					bCanUseParallel = true;
					bSharedSemaphoreExists = true;
				}


				// Create a vector to keep all the attribute search results to return to caller later
				IIUnknownVectorPtr ipFoundAttributes(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI04374", ipFoundAttributes != __nullptr);

				// If any counters are set decrement them here
				decrementCounters(ipAFDoc);

				// Pre-process the doc if there's any preprocessor
				// Do this _before_ setupRunMode
				// https://extract.atlassian.net/browse/ISSUE-14455
				// Do this for kPassInputVOAToOutput too so that an F# Preprocessor can be used without using an Input Finder
				// https://extract.atlassian.net/browse/ISSUE-18135
				if (bEnabledDocumentPreprocessorExists)
				{
					runGlobalDocPreprocessor(ipAFDoc, ipProgressStatus);
				}

				if (m_eRuleSetRunMode == kPassInputVOAToOutput)
				{
					// If RunMode is kPassInputVOAToOutput then set the input attributes to the found attributes
					ipFoundAttributes = passVOAToOutput(ipAFDoc);
				}
				else if (m_eRuleSetRunMode == kRunPerPaginationDocument)
				{
					// If RunMode is kRunPerPaginationDocument then the existing hierarchy will be passed on
					ipFoundAttributes = ipAFDoc->Attribute->SubAttributes;
				}

				// If RunMode is not kPassInputVOAToOutput then run finding rules
				if (m_eRuleSetRunMode != kPassInputVOAToOutput)
				{
					IIUnknownVectorPtr ipAFDocsToRun = setupRunMode(ipAFDoc);
					ASSERT_RESOURCE_ALLOCATION("ELI39437", ipAFDocsToRun != __nullptr);

					// Get the number of AFDocs that will need to run rules on
					int nAFDocs = ipAFDocsToRun->Size();

					// Need to recalculate progress status counts if the number of documents is not what it was assumed to be above
					// (This calculation needs to happen after the global preprocessor has run)
					if (nAFDocs != 1 && ipProgressStatus)
					{
						// Calculate the progress that has already been done
						ipProgressStatus->CompleteCurrentItemGroup();
						int numCompletedItems = ipProgressStatus->GetNumItemsCompleted();

						calculateProgressItems(nNumAttributesToRunRulesFor, nAFDocs);
						ipProgressStatus->InitProgressStatus("", numCompletedItems, m_sProgressCounts.nTotal, VARIANT_TRUE);
					}

					IIUnknownVectorPtr ipResults(CLSID_IUnknownVector);
					long nNumTasks = nAFDocs * nNumAttributesToRunRulesFor;
					if (nNumTasks > 1 && bCanUseParallel)
					{
						// Assert that both parallel processing and profiling are not simultaneously enabled.
						ASSERT_RUNTIME_CONDITION("ELI42055", !CRuleSetProfiler::ms_bEnabled,
							"Rule set profiling does not work correctly with parallel processing enabled!");

						_bstr_t bstrSemaphoreName = "";
						// Create the parallel ruleset object if null
						if (m_ipParallelRuleSet == __nullptr)
						{
							m_ipParallelRuleSet.CreateInstance("Extract.AttributeFinder.Rules.ParallelRuleSet");
							ASSERT_RESOURCE_ALLOCATION("ELI41962", m_ipParallelRuleSet != __nullptr);
						}
						if (bSharedSemaphoreExists)
						{
							UPI upi = UPI::getCurrentProcessUPI();
							bstrSemaphoreName = _bstr_t(upi.getProcessSemaphoreName().c_str());
						}

						ipResults = m_ipParallelRuleSet->RunAttributeFinders(ipAFDocsToRun, getThisAsCOMPtr(), ipvecAttributeNames,
							bstrSemaphoreName, ipProgressStatus);
					}
					else
					{
						for (long i = 0; i < nAFDocs; ++i)
						{
							UCLID_AFCORELib::IAFDocumentPtr ipAFDoc = ipAFDocsToRun->At(i);
							ipResults->PushBack(runAttributeFinders(ipAFDoc, ipAttributesToRun, ipProgressStatus));
						}
					}

					// Put results together
					for (long i = 0; i < nAFDocs; ++i)
					{
						int nLastUsedPageNumber = 0;
						UCLID_AFCORELib::IAFDocumentPtr ipCurrAFDoc = ipAFDocsToRun->At(i);
						IIUnknownVectorPtr ipFoundOnCurrent = ipResults->At(i);

						if (m_eRuleSetRunMode == kRunPerPaginationDocument)
						{
							// Set the subattributes of the DocumentData node
							ipCurrAFDoc->Attribute->SubAttributes = ipFoundOnCurrent;
						}
						else if (m_bInsertAttributesUnderParent)
						{
							ISpatialStringPtr ipValue(CLSID_SpatialString);

							// if non-spatial just assume the page number is 1 + last used page number
							int nPageNumber = nLastUsedPageNumber + 1;
							int nLastPageNumber = nPageNumber;

							// If the document has spatial info get the first and last page
							if (ipCurrAFDoc->Text->HasSpatialInfo() == VARIANT_TRUE)
							{
								nPageNumber = ipCurrAFDoc->Text->GetFirstPageNumber();
								nLastPageNumber = ipCurrAFDoc->Text->GetLastPageNumber();
							}
							nLastUsedPageNumber = nLastPageNumber;

							// if the First and last page are the same use that page number otherwise
							// use the word "All"
							string strPageNumber = (nPageNumber == nLastPageNumber) ? asString(nPageNumber) : "All";
							ipValue = createParentValueFromAFDocAttributes(ipCurrAFDoc, strPageNumber);

							ipFoundAttributes->PushBack(createParentAttribute(m_strInsertParentName, ipValue, ipFoundOnCurrent));
						}
						else
						{
							ipFoundAttributes->Append(ipFoundOnCurrent);
						}
					}
				}

				if (bEnabledOutputHandlerExists)
				{
					runOutputHandler(ipAFDoc, ipFoundAttributes, ipProgressStatus);
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

		flushCounters();

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
STDMETHODIMP CRuleSet::get_UseDocsIndexingCounter(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = m_bUseDocsIndexingCounter ? VARIANT_TRUE : VARIANT_FALSE;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11354")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_UseDocsIndexingCounter(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// NOTE: Unlike other methods/properties, calling this method requires
		// RuleSet Editor license.
		void validateUILicense();

		m_bUseDocsIndexingCounter = newVal == VARIANT_TRUE;
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
			// Change the setting and set the dirty flag
			m_bRuleSetOnlyForInternalUse = bNewValue;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11548")
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
		
		// Force configured counter info to be re-initialized on next use to take into account the
		// provided rule execution counters.
		m_apmapCounters = __nullptr;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38720")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_UsePagesIndexingCounter(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = m_bUsePagesIndexingCounter ? VARIANT_TRUE : VARIANT_FALSE;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38999")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_UsePagesIndexingCounter(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// NOTE: Unlike other methods/properties, calling this method requires
		// RuleSet Editor license.
		void validateUILicense();

		m_bUsePagesIndexingCounter = newVal == VARIANT_TRUE; 
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39000")
}

//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_CustomCounters(IIUnknownVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI39001", pVal != __nullptr);

		validateLicense();

		IIUnknownVectorPtr ipShallowCopy = (m_ipCustomCounters == nullptr)
			? IIUnknownVectorPtr(CLSID_IUnknownVector)
			: m_ipCustomCounters;
		*pVal = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39002")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_CustomCounters(IIUnknownVector *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// NOTE: Unlike other methods/properties, calling this method requires
		// RuleSet Editor license.
		void validateUILicense();

		m_ipCustomCounters = pNewVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39003")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::FlushCounters()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		flushCounters();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39191")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_IsDirty(VARIANT_BOOL *pbIsDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pbIsDirty = asVariantBool(m_bDirty || !getRuleSetSerializer()->EqualsUnmodified(getThisAsCOMPtr()));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49953");
}

//-------------------------------------------------------------------------------------------------
// IRunMode
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_RunMode(ERuleSetRunMode *pRunMode)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		validateLicense();

		*pRunMode = m_eRuleSetRunMode;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39386")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_RunMode(ERuleSetRunMode runMode)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		validateLicense();

		if (m_eRuleSetRunMode != runMode)
		{
			m_eRuleSetRunMode = runMode;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39387")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_InsertAttributesUnderParent(VARIANT_BOOL *pbInsertAttributesUnderParent)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		validateLicense();

		*pbInsertAttributesUnderParent = asVariantBool(m_bInsertAttributesUnderParent);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39388")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_InsertAttributesUnderParent(VARIANT_BOOL bInsertAttributesUnderParent)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		validateLicense();

		bool newValue = asCppBool(bInsertAttributesUnderParent);
		
		if (m_bInsertAttributesUnderParent != newValue)
		{
			m_bInsertAttributesUnderParent = newValue;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39389")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_InsertParentName(BSTR* pInsertParentName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		validateLicense();

		*pInsertParentName = _bstr_t(m_strInsertParentName.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39390")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_InsertParentName(BSTR InsertParentName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		validateLicense();

		string newValue = asString(InsertParentName);
		if (newValue != m_strInsertParentName)
		{
			m_strInsertParentName = newValue;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39391")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_InsertParentValue(BSTR* pInsertParentValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		validateLicense();

		*pInsertParentValue = _bstr_t(m_strInsertParentValue.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39392")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_InsertParentValue(BSTR InsertParentValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		validateLicense();

		string newValue = asString(InsertParentValue);
		if (m_strInsertParentValue != newValue)
		{
			m_strInsertParentValue = newValue;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39393")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_DeepCopyInput(VARIANT_BOOL *pbDeepCopyInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		validateLicense();

		*pbDeepCopyInput = asVariantBool(m_bDeepCopyInput);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39394")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_DeepCopyInput(VARIANT_BOOL bDeepCopyInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		validateLicense();

		bool newValue = asCppBool(bDeepCopyInput);
		if (m_bDeepCopyInput != newValue)
		{
			m_bDeepCopyInput = newValue;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39395")
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
		return getThisAsCOMPtr()->IsDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04768");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock lg( &m_criticalSection, TRUE);

		// Check license
		validateLicense();

		clear();
		bool bHasCustomCounters = false;

		// Previously, by default, rulesets could be used internally or externally
		m_bRuleSetOnlyForInternalUse = false;

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
				dataReader >> m_bUseDocsIndexingCounter;
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

			if ( m_nVersionNumber >= 6 && m_nVersionNumber < 15)
			{
				// USB serial numbers were persisted here from version 6 to version 14, but are no
				// longer used. Disregard.
				string strTemp;
				dataReader >> strTemp;
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

			if (m_nVersionNumber >= 14)
			{
				dataReader >> m_bUsePagesIndexingCounter;
				dataReader >> bHasCustomCounters;
			}

			if (m_nVersionNumber >= 16)
			{
				long lRuleSetRunMode;
				dataReader >> lRuleSetRunMode;
				m_eRuleSetRunMode = (ERuleSetRunMode) lRuleSetRunMode;
		
				dataReader >> m_bInsertAttributesUnderParent;
		
				dataReader >> m_strInsertParentName;
		
				dataReader >> m_strInsertParentValue;
		
				dataReader >> m_bDeepCopyInput;
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

		// Read custom counter data if present (version 14 or higher)
		if (bHasCustomCounters)
		{
			ipObj = __nullptr;
			readObjectFromStream(ipObj, pStream, "ELI39004");
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI39005", 
					"Custom counter data could not be read from stream.");
			}
			m_ipCustomCounters = ipObj;
		}

		if (m_nVersionNumber >= 11)
		{
			// Load the GUID for the IIdentifiableObject interface.
			loadGUID(pStream);
		}

		// Read the OCR parameters
		if (m_nVersionNumber >= 17)
		{
			IPersistStreamPtr ipObj;

			::readObjectFromStream(ipObj, pStream, "ELI45944");
			ASSERT_RESOURCE_ALLOCATION("ELI45945", ipObj != __nullptr);
			m_ipOCRParameters = ipObj;
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

		// Conditionally save with version 16 to avoid needlessly breaking compatibility for an unused feature
		if (gnCurrentVersion == 17
			&& (m_ipOCRParameters == __nullptr || m_ipOCRParameters->Size == 0))
		{
			m_nVersionNumber = 16;
		}
		else
		{
			m_nVersionNumber = gnCurrentVersion; // save with the latest version
		}
		dataWriter << m_nVersionNumber;

		// Write counter flags to stream ( added in version 4 of the file )
		dataWriter << m_bUseDocsIndexingCounter;
		dataWriter << m_bUsePaginationCounter;

		// Write the redaction counter flags
		dataWriter << m_bUsePagesRedactionCounter;
		dataWriter << m_bUseDocsRedactionCounter;

		// flag for internal-use-only added in version 5.
		dataWriter << m_bRuleSetOnlyForInternalUse;

		dataWriter << m_bSwipingRule;

		dataWriter << m_strFKBVersion;

		dataWriter << m_bIgnorePreprocessorErrors;
		dataWriter << m_bIgnoreOutputHandlerErrors;

		dataWriter << m_strFileName;

		dataWriter << m_strComments;

		dataWriter << m_bUsePagesIndexingCounter;
		
		bool bHasCustomCounters = (m_ipCustomCounters != nullptr && m_ipCustomCounters->Size() > 0);
		dataWriter << bHasCustomCounters;

		long lRuleSetRunMode = (int) m_eRuleSetRunMode;
		dataWriter << lRuleSetRunMode;

		dataWriter << m_bInsertAttributesUnderParent;

		dataWriter << m_strInsertParentName;

		dataWriter << m_strInsertParentValue;

		dataWriter << m_bDeepCopyInput;

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

		// Separately any the custom counters to the stream
		if (bHasCustomCounters)
		{
			ipObj = m_ipCustomCounters;
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI39006", "Unable to persist custom counters.");
			}
			writeObjectToStream( ipObj, pStream, "ELI39007", fClearDirty );
		}

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		// Only write OCR parameters if the version hasn't been decremented to 16
		if (m_nVersionNumber >= 17)
		{
			IPersistStreamPtr ipPIObj = getOCRParameters();
			ASSERT_RESOURCE_ALLOCATION("ELI45946", ipPIObj != __nullptr);
			writeObjectToStream(ipPIObj, pStream, "ELI45947", fClearDirty);
		}

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

		clear();

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
		m_bUseDocsIndexingCounter = ipSource->UseDocsIndexingCounter == VARIANT_TRUE;
		m_bUsePaginationCounter = ipSource->UsePaginationCounter == VARIANT_TRUE;
		m_bUsePagesRedactionCounter = ipSource->UsePagesRedactionCounter == VARIANT_TRUE;
		m_bUseDocsRedactionCounter = ipSource->UseDocsRedactionCounter == VARIANT_TRUE;
		m_bUsePagesIndexingCounter = ipSource->UsePagesIndexingCounter == VARIANT_TRUE;
		m_ipCustomCounters = ipSource->CustomCounters->Clone();

		// copy internal-use-only flag
		m_bRuleSetOnlyForInternalUse = (ipSource->ForInternalUseOnly == VARIANT_TRUE);

		// Copy the swiping rule flag
		m_bSwipingRule = asCppBool(ipSource->IsSwipingRule);

		m_strFKBVersion = asString(ipSource->FKBVersion);

		// copy the version number
		m_nVersionNumber = ipSource->VersionNumber;

		m_bIgnorePreprocessorErrors = asCppBool(ipSource->IgnorePreprocessorErrors);
		m_bIgnoreOutputHandlerErrors = asCppBool(ipSource->IgnoreOutputHandlerErrors);

		m_strComments = asString(ipSource->Comments);
		UCLID_AFCORELib::IRunModePtr ipRunMode(ipSource);
		m_eRuleSetRunMode = (ERuleSetRunMode)ipRunMode->RunMode;
		m_bInsertAttributesUnderParent = asCppBool(ipRunMode->InsertAttributesUnderParent);
		m_strInsertParentName = asString(ipRunMode->InsertParentName);
		m_strInsertParentValue = asString(ipRunMode->InsertParentValue);
		m_bDeepCopyInput = asCppBool(ipRunMode->DeepCopyInput);

		// Copy OCR parameters
		IHasOCRParametersPtr ipOCRParams(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI45942", ipOCRParams != __nullptr);
		ICopyableObjectPtr ipMap = ipOCRParams->OCRParameters;
		ASSERT_RESOURCE_ALLOCATION("ELI45943", ipMap != __nullptr);
		m_ipOCRParameters = ipMap->Clone();

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
STDMETHODIMP CRuleSet::RunAttributeFinder(IAFDocument* pAFDoc, 
										   BSTR bstrAttributeName,
										   BSTR bstrAlternateComponentDataDir,
										   IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Record a new rule execution session (Used by addCurrentRSDFileToDebugInfo())
		UCLID_AFCORELib::IRuleExecutionSessionPtr ipSession(CLSID_RuleExecutionSession);
		ASSERT_RESOURCE_ALLOCATION("ELI41977", ipSession != __nullptr);
		ipSession->SetRSDFileName(m_strFileName.c_str());

		UCLID_AFCORELib::IAFDocumentPtr ipAFDoc = pAFDoc;

		if (!asString(bstrAlternateComponentDataDir).empty())
		{
			ipAFDoc->AlternateComponentDataDir = bstrAlternateComponentDataDir;
		}

		IIUnknownVectorPtr ipFoundAttributes = runAttributeFinder(ipAFDoc, bstrAttributeName);

		*pAttributes = ipFoundAttributes.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41961");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_DefinedAttributeNames(IVariantVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IVariantVectorPtr ipvecAttributeNames = m_ipAttributeNameToInfoMap->GetKeys();
		ASSERT_RESOURCE_ALLOCATION("ELI42020", ipvecAttributeNames != __nullptr);

		*pVal = ipvecAttributeNames.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42021")
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
shared_ptr<map<long, CounterInfo>> CRuleSet::getCounterInfo()
{
	if (!m_apmapCounters)
	{
		m_apmapCounters = make_shared<map<long, CounterInfo>>(CounterInfo::GetCounterInfo(getThisAsCOMPtr()));

		// If m_ipRuleExecutionCounters has been specified, assign these to the appropriate
		// CounterInfo instances for use when decrementing.
		if (m_ipRuleExecutionCounters != nullptr)
		{
			long nCount = m_ipRuleExecutionCounters->Size();
			for (long i = 0; i < nCount; i++)
			{
				ISecureCounterPtr ipRuleExecutionCounter = m_ipRuleExecutionCounters->At(i);
				long nCounterID = ipRuleExecutionCounter->ID;
				if (m_apmapCounters->find(nCounterID) != m_apmapCounters->end())
				{
					CounterInfo& counterInfo = m_apmapCounters->at(nCounterID);
					if (counterInfo.m_bEnabled)
					{
						ASSERT_RUNTIME_CONDITION("ELI39043",
							asCppBool(ipRuleExecutionCounter->IsValid),
							"Specified counter is corrupted.");

						counterInfo.SetSecureCounter(ipRuleExecutionCounter);
					}
				}
			}
		}
	}

	return m_apmapCounters;
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::decrementCounters(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc)
{
	// Check to see if counters are to be ignored 
	// This is machine-level locking P16 #1905 - WEL 10/17/06
	if (countersDisabled())
	{
		return;
	}

	try
	{
		auto mapCounters = getCounterInfo();
		for (auto entry = mapCounters->begin(); entry != mapCounters->end(); entry++)
		{
			CounterInfo& counterInfo = entry->second;

			if (counterInfo.m_bEnabled)
			{
				int nNumToDecrement = 1;
				if (counterInfo.m_bByPage)
				{
					nNumToDecrement = getNumberOfPagesInImage(
						asString(ipAFDoc->Text->SourceDocName));
				}

				decrementCounter(counterInfo, nNumToDecrement, true, ipAFDoc);
			}
		}
	}
	// https://extract.atlassian.net/browse/ISSUE-13451
	// WARNING: DO NOT REMOVE OR CHANGE ELI39180 WITHOUT ALSO MODIFYING THE SPOT IT IS CHECKED
	// IN CAFEngineFileProcessor::raw_ProcessFile.
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39180");
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::decrementCounter(CounterInfo& counter, int nNumToDecrement, bool bAllowAccumulation,
	UCLID_AFCORELib::IAFDocumentPtr ipAFDoc)
{
	ASSERT_RUNTIME_CONDITION("ELI38998", nNumToDecrement >= 0,
		"Invalid counter decrement attempted.");

	// Lock to ensure thread safety of counterData.
	CSingleLock lg(&ms_criticalSectionCounterData, TRUE);

	CounterData& counterData = counter.GetCounterData();

	try
	{
		// The max allowed accumulation for the counter value before decrementing the counter is
		// defined as Min(A,B,C) where:
		// A = 10 (_MAX_COUNTER_ACCUMULATION)
		// B = D / 10
		// C = E / 100
		// D = number of counts successfully decremented off the counter from in the current process
		// E = number of counts on the counter at the time of the last successful decrement of the
		// counter from the current process.

		string strMaxAccumulation(_MAX_COUNTER_ACCUMULATION);
		int nAllowedAccumulation = 0;
		
		if (bAllowAccumulation)
		{
			nAllowedAccumulation = asLong(strMaxAccumulation.substr(4, 1) + strMaxAccumulation[2]);
			nAllowedAccumulation = min(nAllowedAccumulation, counterData.m_nCountsDecrementedInProcess / 10);
			nAllowedAccumulation = min(nAllowedAccumulation, counterData.m_nLastCountValue / 100);
		}

		int nCurrentAccumulation = counterData.m_nCountDecrementAccumulation + nNumToDecrement;
		if (nCurrentAccumulation == 0 || nCurrentAccumulation < nAllowedAccumulation)
		{
			counterData.m_nCountDecrementAccumulation = nCurrentAccumulation;
			return;
		}

		ISecureCounterPtr ipSecureCounter = counter.GetSecureCounter();

		// First try to decrement using m_ipRuleExecutionCounters.
		int nLastCount = (ipSecureCounter == nullptr)
			? -1
			: ipSecureCounter->DecrementCounter(nCurrentAccumulation);

		if (nLastCount < 0)
		{
			UCLIDException ue("ELI39008", "Rule execution counter not available.");
			ue.addDebugInfo("ID", counter.m_nID);
			ue.addDebugInfo("Name", counter.m_strName);

			// Only top-level rules files can decrement counters so perhaps that is why
			// this has failed.
			// https://extract.atlassian.net/browse/ISSUE-14386
			if (ipAFDoc != __nullptr && ipAFDoc->RSDFileStack != __nullptr)
			{
				ue.addDebugInfo("Is top-level rules file", ipAFDoc->RSDFileStack->Size == 1);
			}
			throw ue;
		}

		counterData.m_nLastCountValue = nLastCount;
		counterData.m_nCountsDecrementedInProcess += nCurrentAccumulation;
		counterData.m_nCountDecrementAccumulation = 0;
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
void CRuleSet::flushCounters()
{
	auto mapCounters = getCounterInfo();
	for (auto entry = mapCounters->begin(); entry != mapCounters->end(); entry++)
	{
		// Lock to ensure thread safety of counterData.
		CSingleLock lg(&ms_criticalSectionCounterData, TRUE);

		CounterInfo& counterInfo = entry->second;
		CounterData& counterData = counterInfo.GetCounterData();

		if (counterData.m_nCountDecrementAccumulation > 0)
		{
			decrementCounter(counterInfo, 0, false, __nullptr);
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::countersDisabled()
{
	return LicenseManagement::isLicensed(gnIGNORE_RULE_EXECUTION_COUNTER_DECREMENTS); 
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::isRuleExecutionAllowed()
{
	// [FlexIDSCore #3061] Requires:
	// - A counter OR
	// - Internal-use flag OR
	// - Is a swiping rule OR
	// - Full RDT license
	// https://extract.atlassian.net/browse/ISSUE-14407 - added countersDisabled() call first
	return countersDisabled() || isUsingCounter() || m_bRuleSetOnlyForInternalUse || m_bSwipingRule || isRdtLicensed();
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
	auto mapCounters = getCounterInfo();
	for (auto entry = mapCounters->begin(); entry != mapCounters->end(); entry++)
	{
		CounterInfo& counterInfo = entry->second;
		if (counterInfo.m_bEnabled)
		{
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::isLicensedToSave()
{
	return isUsingCounter() || m_bRuleSetOnlyForInternalUse || isRdtLicensed();
}
//-------------------------------------------------------------------------------------------------
UCLID_AFCORELib::IAttributePtr CRuleSet::createParentAttribute(string strName, ISpatialStringPtr ipValue, 
		IIUnknownVectorPtr ipAttributes)
{
	UCLID_AFCORELib::IAttributePtr ipParent(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI39438", ipParent != __nullptr);

	ipParent->Name = strName.c_str();
	ipParent->Value = ipValue;
	ipParent->SubAttributes = ipAttributes;

	return ipParent;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CRuleSet::setupRunMode(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc)
{
	if (m_eRuleSetRunMode == kRunPerDocument)
	{
		IIUnknownVectorPtr ipAFDocsToRun(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI40369", ipAFDocsToRun != __nullptr);

		ipAFDocsToRun->PushBack(ipAFDoc);

		return ipAFDocsToRun;
	}
	else if (m_eRuleSetRunMode == kRunPerPage)
	{
		IIUnknownVectorPtr ipPages = ipAFDoc->Text->GetPages(VARIANT_TRUE, gstrDEFAULT_EMPTY_PAGE_STRING.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI42019", ipPages != __nullptr);

		return IUnknownVectorMethods::map<ISpatialStringPtr, UCLID_AFCORELib::IAFDocumentPtr>(
			ipPages,
			[&](ISpatialStringPtr page) -> auto
			{
				auto ipPageDoc = ipAFDoc->PartialClone(VARIANT_FALSE, VARIANT_FALSE);
				ipPageDoc->Text = page;

				return ipPageDoc;
			});
	}
	else if (m_eRuleSetRunMode == kRunPerPaginationDocument)
	{
		auto documentAttributeSubattributes =
			voa::concat(
				voa::choose<IIUnknownVectorPtr>(
					ipAFDoc->Attribute->SubAttributes,
					[&](UCLID_AFCORELib::IAttributePtr item) -> IIUnknownVectorPtr
					{
						if (asString(item->Name) == "Document")
						{
							return item->SubAttributes;
						}
						else
						{
							return __nullptr;
						}
					})
			);

		return voa::choose<UCLID_AFCORELib::IAFDocumentPtr>(documentAttributeSubattributes,
			[&](UCLID_AFCORELib::IAttributePtr item) -> UCLID_AFCORELib::IAFDocumentPtr
			{
				if (asString(item->Name) == "DocumentData")
				{
					auto logicalDoc = ipAFDoc->PartialClone(VARIANT_FALSE, VARIANT_FALSE);
					logicalDoc->Attribute = item;
					return logicalDoc;
				}
				else
				{
					return __nullptr;
				}
			});
	}

	THROW_LOGIC_ERROR_EXCEPTION("ELI53348");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CRuleSet::passVOAToOutput(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc)
{
	IIUnknownVectorPtr ipOutput(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI39467", ipOutput != __nullptr);	
	
	// if run mode is not kPassInputVOAToOutput return empty vector
	if (m_eRuleSetRunMode != kPassInputVOAToOutput)
	{
		return ipOutput;
	}

	UCLID_AFCORELib::IAttributePtr ipAttribute(ipAFDoc->Attribute);

	// The documents attribute is null
	if (ipAttribute == __nullptr)
	{
		// return empty vector
		return ipOutput;
	}

	// return the attributes under the configured parent
	if (m_bInsertAttributesUnderParent)
	{
		ISpatialStringPtr ipValue(CLSID_SpatialString);

		// Create the value from the AFDocAttributes
		ipValue = createParentValueFromAFDocAttributes(ipAFDoc, "All");

		IIUnknownVectorPtr ipSubAttributes;
						 
		if (m_bDeepCopyInput)
		{
			ipSubAttributes = ipAttribute->SubAttributes->Clone();
		}
		else
		{
			ipSubAttributes = ipAttribute->SubAttributes;
		}
		ipOutput->PushBack(
			createParentAttribute(m_strInsertParentName, ipValue, ipSubAttributes));
	}
	else
	{
		if (m_bDeepCopyInput)
		{
			ipOutput = ipAttribute->SubAttributes->Clone();
		}
		else
		{
			ipOutput = ipAttribute->SubAttributes;
		}
	}
	return ipOutput;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CRuleSet::createParentValueFromAFDocAttributes(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc, string pageString)
{
	ISpatialStringPtr ipValue(CLSID_SpatialString);
	IMiscUtilsPtr ipMiscUtils = getMiscUtils();
	ITagUtilityPtr ipTagUtility = ipMiscUtils;

	ipMiscUtils->AddTag(gstrPAGE_NUMBER_TAG.c_str(), pageString.c_str());

	// If the only tag is the page content tag clone the text of the document
	if (m_strInsertParentValue == gstrPAGE_CONTENT_TAG)
	{
		// Clone the text of the document for the attribute
		ICopyableObjectPtr ipClone(ipAFDoc->Text);
		ipValue = ipClone->Clone();
	}
	else
	{
		// Add the text of the document non spatially
		ipMiscUtils->AddTag(gstrPAGE_CONTENT_TAG.c_str(), ipAFDoc->Text->String);
		ipValue->AppendString(ipMiscUtils->ExpandTagsAndFunctions(
			m_strInsertParentValue.c_str(), ipTagUtility,	ipAFDoc->Text->SourceDocName,NULL));
		ipValue->SourceDocName = ipAFDoc->Text->SourceDocName;
	}
	return ipValue;
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::shouldAddAttributeHistory()
{
	if (m_ipRuleExecutionEnv == __nullptr)
	{
		m_ipRuleExecutionEnv.CreateInstance(CLSID_RuleExecutionEnv);
		ASSERT_RESOURCE_ALLOCATION("ELI41796", m_ipRuleExecutionEnv != __nullptr);
	}
	return m_ipRuleExecutionEnv->ShouldAddAttributeHistory == VARIANT_TRUE;
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::addAttributeHistoryInfo(UCLID_AFCORELib::IAttributePtr ipAttribute, string strAttributeName)
{
	IIUnknownVectorPtr ipTrace = ipAttribute->DataObject;
	if (ipTrace == __nullptr)
	{
		ipTrace.CreateInstance(CLSID_IUnknownVector);
		ipAttribute->DataObject = ipTrace;
	}
	ASSERT_RESOURCE_ALLOCATION("ELI41764", ipTrace != __nullptr);

	// Add these values to the last item in the collection if there is one...
	IStrToStrMapPtr ipMap;
	long nSize = ipTrace->Size();
	if (nSize > 0)
	{
		ipMap = ipTrace->At(nSize - 1);
	}

	// ...and if it doesn't already have a Ruleset key
	// Else, create a new map
	string key = "Ruleset";
	_bstr_t bstrKey = _bstr_t(key.c_str());
	if (ipMap == __nullptr || ipMap->Contains(bstrKey))
	{
		ipMap.CreateInstance(CLSID_StrToStrMap);
		ipTrace->PushBack(ipMap);
	}
	ipMap->Set(bstrKey, _bstr_t(m_strFileName.c_str()));

	key = "Attribute";
	bstrKey = _bstr_t(key.c_str());
	ipMap->Set(bstrKey, _bstr_t(strAttributeName.c_str()));

	// Process subattributes
	IIUnknownVectorPtr ipSubattributes = ipAttribute->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI41767", ipSubattributes != __nullptr);

	nSize = ipSubattributes->Size();
	for (long i = 0; i < nSize; i++)
	{
		UCLID_AFCORELib::IAttributePtr ipSubattribute = ipSubattributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI41768", ipSubattribute != __nullptr);
		addAttributeHistoryInfo(ipSubattribute, strAttributeName);
	}
}
//-------------------------------------------------------------------------------------------------
bool CRuleSet::isParallelProcessingEnabled()
{
	if (m_ipRuleExecutionEnv == __nullptr)
	{
		m_ipRuleExecutionEnv.CreateInstance(CLSID_RuleExecutionEnv);
		ASSERT_RESOURCE_ALLOCATION("ELI42012", m_ipRuleExecutionEnv != __nullptr);
	}
	return asCppBool(m_ipRuleExecutionEnv->EnableParallelProcessing);
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::runGlobalDocPreprocessor(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc, IProgressStatusPtr ipProgressStatus)
{
	// Try/catch for preprocessors
	try
	{
		try
		{
			// Update the progress status
			if (ipProgressStatus)
			{
				ipProgressStatus->StartNextItemGroup("Executing the pre-processor rules...",
					m_sProgressCounts.nPreprocessor);
			}

			// Execute the document preprocessor
			UCLID_AFCORELib::IDocumentPreprocessorPtr ipDocPreprocessor(m_ipDocPreprocessor->Object);
			if (ipDocPreprocessor)
			{
				PROFILE_RULE_OBJECT(
					asString(m_ipDocPreprocessor->Description), "", ipDocPreprocessor, 0)

				ipDocPreprocessor->Process(ipAFDoc, __nullptr);
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
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CRuleSet::runAttributeFinder(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc, BSTR bstrAttributeName)
{
	try
	{
		IIUnknownVectorPtr ipResults(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI39441", ipResults != __nullptr);

		// Get access to the names of attributes in the ruleset
		if (m_ipAttributeNameToInfoMap->Contains(bstrAttributeName))
		{
			// get the attribute finding information for the current attribute
			UCLID_AFCORELib::IAttributeFindInfoPtr ipAttributeFindInfo =
				m_ipAttributeNameToInfoMap->GetValue(bstrAttributeName);

			if (ipAttributeFindInfo == __nullptr)
			{
				UCLIDException ue("ELI04376", "Unable to retrieve attribute rules info.");
				string stdstrAttribute = asString(bstrAttributeName);
				ue.addDebugInfo("Attribute", stdstrAttribute);
				throw ue;
			}

			PROFILE_RULE_OBJECT(asString(bstrAttributeName), "Attribute finder block",
				ipAttributeFindInfo, 0)

				// find all attributes values for the current attribute
				IIUnknownVectorPtr ipAttributes =
				ipAttributeFindInfo->ExecuteRulesOnText(ipAFDoc, __nullptr);

			// for each attribute that was found, update the "name" part of the
			// attribute object to be the name of the current attribute
			long nNumAttributes = ipAttributes->Size();
			for (int j = 0; j < nNumAttributes; j++)
			{
				UCLID_AFCORELib::IAttributePtr ipAttribute = ipAttributes->At(j);
				ipAttribute->Name = bstrAttributeName;

				// Update data object with trace info
				if (shouldAddAttributeHistory())
				{
					addAttributeHistoryInfo(ipAttribute, asString(bstrAttributeName));
				}
			}

			// append results to the result vector of found attributes.
			ipResults->Append(ipAttributes);
		}
		return ipResults;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42009");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CRuleSet::runAttributeFinders(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc,
	IVariantVectorPtr ipAttributeNames, IProgressStatusPtr ipProgressStatus)
{
	try
	{
		IIUnknownVectorPtr ipResults(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI42010", ipResults != __nullptr);

		long nNumAttributesToRun = ipAttributeNames->Size;
		for (long i = 0; i < nNumAttributesToRun; ++i)
		{
			_bstr_t bstrAttributeName = ipAttributeNames->Item[i];
			if (ipProgressStatus != __nullptr)
			{
				string strStatusText = "Executing rules for field " + asString(bstrAttributeName) + "...";
				ipProgressStatus->StartNextItemGroup(strStatusText.c_str(), m_sProgressCounts.nPerAttribute);
			}

			ipResults->Append(runAttributeFinder(ipAFDoc, bstrAttributeName));
		}
		return ipResults;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI42011");
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::runOutputHandler(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc, IIUnknownVectorPtr ipAttributes,
	IProgressStatusPtr ipProgressStatus)
{
	// Try/catch for output handlers
	try
	{
		try
		{
			// Update the progress status
			if (ipProgressStatus)
			{
				ipProgressStatus->StartNextItemGroup("Executing the output handler rules...",
					m_sProgressCounts.nOutputHandler);
			}

			// Execute the output hander
			UCLID_AFCORELib::IOutputHandlerPtr ipOH( m_ipOutputHandler->Object );
			if (ipOH)
			{
				PROFILE_RULE_OBJECT(asString(m_ipOutputHandler->Description), "", ipOH, 0)

				ipOH->ProcessOutput( ipAttributes, ipAFDoc, __nullptr );
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
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::calculateProgressItems(long nNumAttributesToRun, long nNumDocs)
{
	m_sProgressCounts.nInitialization = 1;
	m_sProgressCounts.nPreprocessor = enabledDocumentPreprocessorExists()
		? 1 : 0;
	m_sProgressCounts.nPerAttribute = m_eRuleSetRunMode == kPassInputVOAToOutput
		? 0 : 2;
	m_sProgressCounts.nTotalAttribute =
		m_sProgressCounts.nPerAttribute * nNumAttributesToRun * nNumDocs;
	m_sProgressCounts.nOutputHandler = enabledOutputHandlerExists()
		? 1 : 0;
	m_sProgressCounts.nTotal =
		m_sProgressCounts.nInitialization
		+ m_sProgressCounts.nPreprocessor
		+ m_sProgressCounts.nTotalAttribute
		+ m_sProgressCounts.nOutputHandler;
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::validateRunability()
{
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

	// Throw exception if rule execution is not allowed [FlexIDSCore #3061]
	if (!isRuleExecutionAllowed())
	{
		throw UCLIDException("ELI21520", 
			"Rule execution is not allowed - make sure that a counter is selected.");
	}

	if (m_strFKBVersion.empty() && (isUsingCounter() || m_bSwipingRule))
	{
		throw UCLIDException("ELI33506", "An FKB version must be specified for a swiping rule or a "
			"ruleset that decrements counters.");
	}
}
//-------------------------------------------------------------------------------------------------
// IHasOCRParameters
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::get_OCRParameters(IOCRParameters** ppOCRParameters)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*ppOCRParameters = getOCRParameters().Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45950");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::put_OCRParameters(IOCRParameters* pOCRParameters)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_ipOCRParameters = pOCRParameters;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45951");
}
//-------------------------------------------------------------------------------------------------
// ILoadOCRParameters
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleSet::raw_LoadOCRParameters(BSTR strRuleSetName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFCORELib::IRuleSetPtr ipRuleSet = getThisAsCOMPtr();
		ipRuleSet->LoadFrom(strRuleSetName, VARIANT_FALSE);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45957");
}
//-------------------------------------------------------------------------------------------------
IOCRParametersPtr CRuleSet::getOCRParameters()
{
	if (m_ipOCRParameters == __nullptr)
	{
		m_ipOCRParameters.CreateInstance(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI45949", m_ipOCRParameters != __nullptr);
	}

	return m_ipOCRParameters;
}
//-------------------------------------------------------------------------------------------------
UCLID_AFCORELib::IRuleSetSerializerPtr CRuleSet::getRuleSetSerializer()
{
	if (m_ipRuleSetSerializer == __nullptr)
	{
		m_ipRuleSetSerializer.CreateInstance("Extract.AttributeFinder.Rules.Json.RuleSetJsonSerializer");
		ASSERT_RESOURCE_ALLOCATION("ELI49686", m_ipRuleSetSerializer != __nullptr);
	}

	return m_ipRuleSetSerializer;
}
//-------------------------------------------------------------------------------------------------
void CRuleSet::clear()
{
	m_ipAttributeNameToInfoMap = __nullptr; 
	m_bDirty = false;
	m_bIsEncrypted = false;
	m_bUseDocsIndexingCounter = false;
	m_bUsePaginationCounter = false;
	m_bUsePagesRedactionCounter = false;
	m_bUseDocsRedactionCounter = false;
	m_bUsePagesIndexingCounter = false;
	m_ipCustomCounters = __nullptr;
	m_apmapCounters = __nullptr;
	m_bRuleSetOnlyForInternalUse = true;
	m_bSwipingRule = false;
	m_strFKBVersion = "";
	m_bIgnorePreprocessorErrors = false;
	m_bIgnoreOutputHandlerErrors = false;
	m_nVersionNumber = gnCurrentVersion;
	m_eRuleSetRunMode = kRunPerDocument;
	m_bInsertAttributesUnderParent = false;
	m_strInsertParentName = "";
	m_strInsertParentValue = "";
	m_bDeepCopyInput = false;
	m_ipOCRParameters = __nullptr;
}
//-------------------------------------------------------------------------------------------------
