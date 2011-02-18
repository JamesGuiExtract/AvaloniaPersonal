// RedactionTask.cpp : Implementation of CRedactionTask
#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "RedactionTask.h"
#include "RedactionCCUtils.h"
#include "RedactionCCConstants.h"
#include "IDShieldData.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <StopWatch.h>
#include <MiscLeadUtils.h>

#include <string>
#include <set>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version 2 - Added support for PDF security
// Version 3 - Added TextToReplace, ReplacementText, and AutoAdjustCase settings for
//		redaction appearance
// Version 4 - Changed replace text from single replacement to group of replacement settings,
//		moved advanced text settings to seperate dialog, added settings for prefix and suffix
//		text for the first instance of a type
const unsigned long gnCurrentVersion = 4;

//-------------------------------------------------------------------------------------------------
// CRedactionTask
//-------------------------------------------------------------------------------------------------
CRedactionTask::CRedactionTask()
:	m_ipAFUtility(NULL),
    m_ipAttributeNames(CLSID_VariantVector),
    m_bDirty(false),
    m_bCarryForwardAnnotations(false),
    m_bApplyRedactionsAsAnnotations(false),
    m_ipIDShieldDB(NULL),
    m_ipPdfSettings(NULL),
    m_bUseRedactedImage(false)
{
    ASSERT_RESOURCE_ALLOCATION("ELI19993", m_ipAttributeNames != NULL);

    // set members to their inital states
    clear();

    // Add the default selected Attributes to the collection (P16 #2751)
    m_ipAttributeNames->PushBack("HCData");
    m_ipAttributeNames->PushBack("MCData");
    m_ipAttributeNames->PushBack("LCData");
    m_ipAttributeNames->PushBack("Manual");
}
//-------------------------------------------------------------------------------------------------
CRedactionTask::~CRedactionTask()
{
    try
    {
        m_ipAFUtility = NULL;
        m_ipAttributeNames = NULL;
        m_ipIDShieldDB = NULL;
        m_ipPdfSettings = NULL;
    }
    CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16479");
}
//-------------------------------------------------------------------------------------------------
HRESULT CRedactionTask::FinalConstruct()
{
    return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CRedactionTask::FinalRelease()
{
    try
    {
        m_ipAFUtility = NULL;
        m_ipAttributeNames = NULL;
        m_ipIDShieldDB = NULL;
        m_ipPdfSettings = NULL;
    }
    CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28331");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::InterfaceSupportsErrorInfo(REFIID riid)
{
    static const IID* arr[] = 
    {
        &IID_IRedactionTask,
        &IID_IFileProcessingTask,
        &IID_ICategorizedComponent,
        &IID_ICopyableObject,
        &IID_IMustBeConfiguredObject,
        &IID_ILicensedComponent,
        &IID_IAccessRequired
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
// IFileProcessingTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
    IFileProcessingDB *pDB)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())
    
    try
    {
        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17789");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
    IFAMTagManager* pTagManager, IFileProcessingDB* pDB, IProgressStatus* pProgressStatus,
    VARIANT_BOOL bCancelRequested, EFileProcessingResult* pResult)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())
    INIT_EXCEPTION_AND_TRACING("MLI00529");

    try
    {
        // Check license
        validateLicense();
        _lastCodePos = "10";

		// Set of type names that have been seen already (used to track first instance
		// of a type when using prefix and suffix text
		set<string> setTypes;

        // Start the a stop watch to track processing time
        StopWatch swProcessingTime;
        swProcessingTime.start();
        _lastCodePos = "20";

        ASSERT_ARGUMENT("ELI17929", pResult != NULL);
        IFileRecordPtr ipFileRecord(pFileRecord);
        ASSERT_ARGUMENT("ELI31344", ipFileRecord != __nullptr);

        // Create an smart FAM Tag Pointer
        IFAMTagManagerPtr ipFAMTagManager = pTagManager;
        ASSERT_RESOURCE_ALLOCATION("ELI15012", ipFAMTagManager != NULL);
        _lastCodePos = "30";

        // input file for processing
        string strInputFile = asString(ipFileRecord->Name);
        ASSERT_ARGUMENT("ELI17930", !strInputFile.empty());
        _lastCodePos = "40";

        long nFileID = ipFileRecord->FileID;

        // Default to successful completion
        *pResult = kProcessingSuccessful;
        _lastCodePos = "50";

        // check if file was uss and if so get image name by removing the .uss
        string strExt = getExtensionFromFullPath(strInputFile, true);
        string strImageName = strInputFile;
        if (strExt == ".uss")
        {
            _lastCodePos = "60";
            strImageName = getFileNameWithoutExtension(strInputFile, false);
            strImageName = getAbsoluteFileName(strInputFile, strImageName);
        }
        else if (strExt == ".voa")
        {
            _lastCodePos = "70";
            strImageName = getFileNameWithoutExtension(strInputFile, false);
            strImageName = getAbsoluteFileName(strInputFile, strImageName);
        }
        _lastCodePos = "100";

		// If adjusting the redaction text casing
		// and there is a valid USS file, initialize the spatial string searcher
		ISpatialStringSearcherPtr ipSearcher = __nullptr;
		ISpatialStringSearcherPtr ipAttrSearcher = __nullptr;
		if (m_redactionAppearance.m_bAdjustTextCasing
			&& isValidFile(strImageName + ".uss"))
		{
			ISpatialStringPtr ipText(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI31676", ipText != __nullptr);

			ipText->LoadFrom((strImageName + ".uss").c_str(), VARIANT_FALSE);
			if (ipText->HasSpatialInfo() == VARIANT_TRUE)
			{
				ipSearcher.CreateInstance(CLSID_SpatialStringSearcher);
				ASSERT_RESOURCE_ALLOCATION("ELI31677", ipSearcher != __nullptr);

				ipSearcher->InitSpatialStringSearcher(ipText);

				// This searcher will be used to search the context for each
				// attribute and will be initialized with the expanded context
				// region for the attribute. In order to be more performative, 
				// this searcher is created once here, but will be initialized each
				// time it is needed with the expanded attribute region.
				ipAttrSearcher.CreateInstance(CLSID_SpatialStringSearcher);
				ASSERT_RESOURCE_ALLOCATION("ELI31678", ipSearcher != __nullptr);
			}
		}

        // Expand tags and text functions to get the output name
        string strOutputName = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(
            ipFAMTagManager, m_strOutputFileName, strImageName);

        // Set the image to redact to the input image name
        string strImageToRedact = strImageName;

        // Use redacted image as backdrop if option is specified
        bool bOutputFileExists = isFileOrFolderValid(strOutputName);
        if (m_bUseRedactedImage && bOutputFileExists)
        {
            strImageToRedact = strOutputName;
        }

        // Validate input image exists
        ::validateFileOrFolderExistence(strImageName);

        _lastCodePos = "110";
        // Create Found attributes vector
        IIUnknownVectorPtr ipFoundAttr (CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI08997", ipFoundAttr != NULL);
        _lastCodePos = "120";

        IIUnknownVectorPtr ipVOAAttr(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI12718", ipVOAAttr != NULL);

        // Expand tags and text functions to get the VOA file name
        string strVOAFileName = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(
            ipFAMTagManager, m_strVOAFileName, strImageName);
        _lastCodePos = "130";

        // Throw exception if VOA file doesn't exist
        if (!isValidFile(strVOAFileName))
        {
            UCLIDException ue("ELI28237", "VOA file not found.");
            ue.addDebugInfo("File name", strVOAFileName);
            throw ue;
        }
        _lastCodePos = "140";

        // Load voa
        ipVOAAttr->LoadFrom(strVOAFileName.c_str(), VARIANT_FALSE);

        // Calculate the counts from the loaded voa file
        IDShieldData idsData;
        if (m_ipAttributeNames == NULL)
        {
            // Count all the attributes
            idsData.calculateFromVector(ipVOAAttr);
        }
        else
        {
            // Count the selected attributes
            idsData.calculateFromVector(ipVOAAttr, m_setAttributeNames);
        }
        _lastCodePos = "150";

        // check to see if all of the loaded attributes will be used
        if (m_ipAttributeNames != NULL)
        {
            _lastCodePos = "160";
            long nNumAttr = m_ipAttributeNames->Size;
            string strQuery = "";
            for (int i = 0; i < nNumAttr; i++)
            {
                _lastCodePos = "170-" + asString(i);

                if (i > 0)
                {
                    strQuery += "|";
                }
                string strCurrName = asString(_bstr_t(m_ipAttributeNames->GetItem(i)));
                strQuery += strCurrName;
            }
            _lastCodePos = "180";
            ipFoundAttr = getAFUtility()->QueryAttributes(ipVOAAttr, 
                strQuery.c_str(),VARIANT_FALSE);
        }
        else
        {
            _lastCodePos = "190";
            ipFoundAttr = ipVOAAttr;
        }
        _lastCodePos = "200";

        // Get the text color
        COLORREF crTextColor = invertColor(m_redactionAppearance.m_crFillColor);

        // Process Results
        // Create vector for zones to redact
        vector<PageRasterZone> vecZones;
        long nNumAttr = ipFoundAttr->Size();
        for (int i = 0; i < nNumAttr; i++)
        {
            _lastCodePos = "250-" + asString(i);
            IAttributePtr ipAttr = ipFoundAttr->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI09001", ipAttr != NULL);

            // Get the found value
            ISpatialStringPtr ipValue = ipAttr->Value;
            ASSERT_RESOURCE_ALLOCATION("ELI09002", ipValue != NULL);
            _lastCodePos = "260";

            // Only cover area if value is spatial
            if (ipValue->HasSpatialInfo() == VARIANT_TRUE)
            {
                string strCodes = getExemptionCodes(ipAttr);

				string strType = asString(ipAttr->Type);

                // Get the text associated with this attribute
                string strText = CRedactionCustomComponentsUtils::ExpandRedactionTags(
                    m_redactionAppearance.m_strText, strCodes, strType);
				string strPrefixText = "";
				string strSuffixText = "";
                _lastCodePos = "270";

				if (setTypes.find(strType) == setTypes.end())
				{
					strPrefixText = CRedactionCustomComponentsUtils::ExpandRedactionTags(
						m_redactionAppearance.m_strPrefixText, strCodes, strType);
					strSuffixText = CRedactionCustomComponentsUtils::ExpandRedactionTags(
						m_redactionAppearance.m_strSuffixText, strCodes, strType);
					setTypes.insert(strType);
				}

				// Handle all replacement values
				if (m_redactionAppearance.m_vecReplacements.size())
				{
					for(vector<pair<string, string>>::iterator it =
						m_redactionAppearance.m_vecReplacements.begin();
						it != m_redactionAppearance.m_vecReplacements.end();
						it++)
					{
						replaceVariable(strText, it->first, it->second, kReplaceAll);
					}
				}

                // Get the Raster zones to redact
                IIUnknownVectorPtr ipRasterZones = ipValue->GetOriginalImageRasterZones();
                ASSERT_RESOURCE_ALLOCATION("ELI09180", ipRasterZones != NULL);
                _lastCodePos = "280";

				IIUnknownVectorPtr ipOcrZones = __nullptr;
				if (ipSearcher != __nullptr && isSentenceCase(strText))
				{
					ipOcrZones = ipValue->GetOCRImageRasterZones();
					ASSERT_RESOURCE_ALLOCATION("ELI31664", ipOcrZones != __nullptr);
				}

                // Add to the vector of zones to redact
                long lZoneCount = ipRasterZones->Size();
                for (long j = 0; j < lZoneCount; j++)
                {
					// Get the raster zone
                    IRasterZonePtr ipRasterZone = ipRasterZones->At(j);
                    ASSERT_RESOURCE_ALLOCATION("ELI24862", ipRasterZone != NULL);

					// Store the text value locally (this value may be changed
					// if auto-adjust case is true)
					string strRedactionText = strText;

					// If there is a valid spatial string searcher, then attempt to update
					// the case of the text based on the searcher results
					if (ipOcrZones != __nullptr)
					{
		                _lastCodePos = "280_10";

						// Get the ocr zone
						IRasterZonePtr ipOcrZone = ipOcrZones->At(j);
						ASSERT_RESOURCE_ALLOCATION("ELI31669", ipOcrZone != __nullptr);

						ILongRectanglePtr ipBounds = ipValue->GetOCRImagePageBounds(
							ipOcrZone->PageNumber);
						ASSERT_RESOURCE_ALLOCATION("ELI31670", ipBounds != __nullptr);

						// Get the bounds
						ILongRectanglePtr ipRect = ipOcrZone->GetRectangularBounds(ipBounds);
						ASSERT_RESOURCE_ALLOCATION("ELI31671", ipRect != __nullptr);

						// Get the spatial string for the attribute
						ISpatialStringPtr ipTemp = ipSearcher->GetDataInRegion(ipRect, VARIANT_FALSE);
						ASSERT_RESOURCE_ALLOCATION("ELI31680", ipTemp != __nullptr);

						// Only continue if the returned region has spatial information
						if (ipTemp->HasSpatialInfo() == VARIANT_TRUE)
						{
							string strTemp = asString(ipTemp);
							size_t nIndex = strTemp.find_last_of(".!?");
							_lastCodePos = "280_15";

							bool bMakeLowerCase = false;
							if (nIndex == string::npos || nIndex < (strTemp.length() - 1))
							{
								_lastCodePos = "280_16";

								// Get the page bounds
								long nPageLeft(0), nPageTop(0), nPageBottom(0), nPageRight(0);
								ipBounds->GetBounds(&nPageLeft, &nPageTop, &nPageRight, &nPageBottom);

								// Get the bounds of the zone
								long nRectLeft(0), nRectTop(0), nRectBottom(0), nRectRight(0);
								ipRect->GetBounds(&nRectLeft, &nRectTop, &nRectRight, &nRectBottom);

								// Store the original zone bounds
								ILongRectanglePtr ipOrigRect(CLSID_LongRectangle);
								ASSERT_RESOURCE_ALLOCATION("ELI31679", ipOrigRect);
								ipOrigRect->SetBounds(nRectLeft, nRectTop, nRectRight, nRectBottom);

								// Get the average height. This is the height we
								// will use to expand the region (both up and down) to encompass other
								// lines in the region.
								long nHeightIncrease = ipTemp->GetAverageCharHeight();

								// First set the bounds left and right and check for words
								ipRect->SetBounds(nPageLeft, nRectTop,
									nPageRight, nRectBottom);
								ipRect->Clip(nPageLeft, nPageTop, nPageRight, nPageBottom);

								ipTemp = ipSearcher->GetDataInRegion(ipRect, VARIANT_FALSE);
								ASSERT_RESOURCE_ALLOCATION("ELI31681", ipTemp != __nullptr);

								// Initialize the attribute searcher with this string
								ipAttrSearcher->InitSpatialStringSearcher(ipTemp);
								_lastCodePos = "280_17";

								// Look left and right, if empty in either direction, expand
								// the zone of searching up and/or down based on missing words
								ISpatialStringPtr ipLeftWord = ipAttrSearcher->GetLeftWord(ipOrigRect);
								ISpatialStringPtr ipRightWord = ipAttrSearcher->GetRightWord(ipOrigRect);
								if (ipLeftWord == __nullptr || ipRightWord == __nullptr)
								{
									_lastCodePos = "280_18";
									nRectTop -= ipLeftWord == __nullptr ? nHeightIncrease : 0;
									nRectBottom += ipRightWord == __nullptr ? nHeightIncrease : 0;
									ipRect->SetBounds(nPageLeft, nRectTop, nPageRight, nRectBottom);
									ipRect->Clip(nPageLeft, nPageTop, nPageRight, nPageBottom);

									// Get the data from the expanded region
									ipTemp = ipSearcher->GetDataInRegion(ipRect, VARIANT_FALSE);
									ASSERT_RESOURCE_ALLOCATION("ELI31682", ipTemp != __nullptr);

									ipAttrSearcher->InitSpatialStringSearcher(ipTemp);

									// Get the new left and right words
									ipLeftWord = ipAttrSearcher->GetLeftWord(ipOrigRect);
									ipRightWord = ipAttrSearcher->GetRightWord(ipOrigRect);
								}
								_lastCodePos = "280_19";

								// If there is a word to the left, look for punctuation
								if (ipLeftWord != __nullptr)
								{
									_lastCodePos = "280_19_A";
									string strLeft = asString(ipLeftWord->String);
									if (strLeft.substr(strLeft.length() - 1)
										.find_first_of(".!?") == string::npos)
									{
										// No punctuation to the left, look for lower case letters
										if (strLeft.find_first_of(gstrLOWER_ALPHA) != string::npos)
										{
											// found lower case letters
											bMakeLowerCase = true;
										}
									}
									else
									{
										// Sentence punctuation to the left, set right word to
										// null, no need to check further
										ipRightWord = __nullptr;
									}
								}
								if (!bMakeLowerCase && ipRightWord != __nullptr)
								{
									_lastCodePos = "280_19_B";
									string strRight = asString(ipRightWord->String);
									if (strRight.find_first_of(gstrLOWER_ALPHA) != string::npos)
									{
										bMakeLowerCase = true;
									}
								}
							}
							else
							{
								bMakeLowerCase = true;
							}

							if (bMakeLowerCase)
							{
								makeLowerCase(strRedactionText);
							}

			                _lastCodePos = "280_20";
						}
					}
	                _lastCodePos = "285";

                    // Construct the page raster zone
                    PageRasterZone zone;
                    zone.m_crBorderColor = m_redactionAppearance.m_crBorderColor;
                    zone.m_crFillColor = m_redactionAppearance.m_crFillColor;
                    zone.m_crTextColor = crTextColor;
                    zone.m_font = m_redactionAppearance.m_lgFont;
                    zone.m_iPointSize = m_redactionAppearance.m_iPointSize;

					// Set the appropriate text for the redaction
					// If the first redaction, add prefix and suffix text
					if (j == 0)
					{
						zone.m_strText = strPrefixText + strRedactionText + strSuffixText;
					}
					else
					{
	                    zone.m_strText = strRedactionText;
					}
                    ipRasterZone->GetData(&(zone.m_nStartX), &(zone.m_nStartY), &(zone.m_nEndX),
                        &(zone.m_nEndY), &(zone.m_nHeight), &(zone.m_nPage));

                    // Add to the vector of zones to redact
                    vecZones.push_back(zone);
                }
            }
            _lastCodePos = "290";
        }
        _lastCodePos = "300";

        // check to see if output path exists, if not try to create
        string strOutputPath = getDirectoryFromFullPath(strOutputName, false);
        if (!::isFileOrFolderValid(strOutputPath))
        {
            _lastCodePos = "340";

            // Create OutputPath
            createDirectory(strOutputPath);
        }
        _lastCodePos = "350";

        // Get the pdf security settings
        string strUser = "";
        string strOwner = "";
        int nPermissions = 0;
        if (m_ipPdfSettings != NULL)
        {
            _bstr_t bstrUserPass, bstrOwnerPass;
            PdfOwnerPermissions permissions;
            m_ipPdfSettings->GetSettings(bstrUserPass.GetAddress(), bstrOwnerPass.GetAddress(),
                &permissions);
            strUser = asString(bstrUserPass);
            strOwner = asString(bstrOwnerPass);
            nPermissions = (int) permissions;
        }

        // Save redactions
        fillImageArea(strImageToRedact.c_str(), strOutputName.c_str(), vecZones, 
            m_bCarryForwardAnnotations, m_bApplyRedactionsAsAnnotations,
            strUser, strOwner, nPermissions);
        _lastCodePos = "400";

        // Stop the stop watch
        swProcessingTime.stop();
        CTime tStartTime = swProcessingTime.getBeginTime();
        double dElapsedSeconds = swProcessingTime.getElapsedTime();

        // Add a metadata attribute to the VOA file
        storeMetaData(strVOAFileName, ipVOAAttr, ipFoundAttr, tStartTime, dElapsedSeconds, 
            strImageName, strOutputName, bOutputFileExists);

        // Add ID Shield data if a database is provided
        IFileProcessingDBPtr ipFAMDB(pDB);
        if (ipFAMDB != NULL)
        {
            // Set the FAMDB pointer
            UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr ipIDSDB = getIDShieldDBPtr();
            ipIDSDB->FAMDB = ipFAMDB;

            // Add the IDShieldData record to the database
            ipIDSDB->AddIDShieldData(nFileID, VARIANT_FALSE, swProcessingTime.getElapsedTime(), 
                idsData.m_lNumHCDataFound, idsData.m_lNumMCDataFound, idsData.m_lNumLCDataFound, 
                idsData.m_lNumCluesFound, idsData.m_lTotalRedactions, idsData.m_lTotalManualRedactions,
                idsData.m_lNumPagesAutoAdvanced);
        }
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28604")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_Cancel()
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // nothing to do
        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17787");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_Close()
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // nothing to do
        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17788");
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
        ASSERT_ARGUMENT("ELI31199", pbResult != __nullptr);

        *pbResult = VARIANT_FALSE;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31200");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check parameter
        ASSERT_ARGUMENT("ELI19820", pbValue != NULL);

        try
        {
            // Check license
            validateLicense();

            // If no exception, then pbValue is true
            *pbValue = VARIANT_TRUE;
        }
        catch (...)
        {
            *pbValue = VARIANT_FALSE;
        }

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19821");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_GetComponentDescription(BSTR* pstrComponentDescription)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI28222", pstrComponentDescription != NULL);

        *pstrComponentDescription = _bstr_t("Redaction: Create redacted image").Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12813");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_Clone(IUnknown** pObject)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check parameter
        ASSERT_ARGUMENT("ELI28321", pObject != NULL);

        // validate license first
        validateLicense();

        // create another instance of this object
        ICopyableObjectPtr ipObjCopy(CLSID_RedactionTask);
        ASSERT_RESOURCE_ALLOCATION("ELI09864", ipObjCopy != NULL);

        IUnknownPtr ipUnk(this);
        ipObjCopy->CopyFrom(ipUnk);
    
        // Return the new object to the caller
        *pObject = ipObjCopy.Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09865");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_CopyFrom(IUnknown* pObject)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // validate license first
        validateLicense();
        UCLID_REDACTIONCUSTOMCOMPONENTSLib::IRedactionTaskPtr ipSource(pObject);
        ASSERT_RESOURCE_ALLOCATION("ELI09876", ipSource != NULL);

        m_strOutputFileName = asString(ipSource->OutputFileName);

        // Clear the Attributes set
        m_setAttributeNames.clear();

        if (ipSource->AttributeNames != NULL)
        {
            m_ipAttributeNames = ipSource->AttributeNames;
            fillAttributeSet(m_ipAttributeNames, m_setAttributeNames);
        }
        else
        {
            m_ipAttributeNames = NULL;
        }

        // Retrieve VOA file name
        m_strVOAFileName = asString(ipSource->VOAFileName);

        // Retrieve annotation settings
        m_bCarryForwardAnnotations = asCppBool(ipSource->CarryForwardAnnotations);
        m_bApplyRedactionsAsAnnotations = asCppBool(ipSource->ApplyRedactionsAsAnnotations);

        // Retrieve whether to use redacted image
        m_bUseRedactedImage = asCppBool(ipSource->UseRedactedImage);

        // Retrieve redaction appearance settings
        m_redactionAppearance.m_strText = asString(ipSource->RedactionText);
        m_redactionAppearance.m_bAdjustTextCasing = asCppBool(ipSource->AutoAdjustTextCasing);
		m_redactionAppearance.updateReplacementsFromVector(ipSource->ReplacementValues);
		m_redactionAppearance.m_strPrefixText = asString(ipSource->PrefixText);
		m_redactionAppearance.m_strSuffixText = asString(ipSource->SuffixText);
        m_redactionAppearance.m_crBorderColor = ipSource->BorderColor;
        m_redactionAppearance.m_crFillColor = ipSource->FillColor;
        
        // Retrieve font settings
        _bstr_t bstrFontName;
        VARIANT_BOOL vbIsBold, vbIsItalic;
        long nFontSize;
        ipSource->GetFontData(bstrFontName.GetAddress(), &vbIsBold, &vbIsItalic, &nFontSize);
        string strFontName = asString(bstrFontName);
        m_redactionAppearance.m_lgFont.lfItalic = vbIsItalic == VARIANT_TRUE ? gucIS_ITALIC : 0;
        m_redactionAppearance.m_lgFont.lfWeight = vbIsBold == VARIANT_TRUE ? FW_BOLD : FW_NORMAL;
        m_redactionAppearance.m_iPointSize = nFontSize;
        LPTSTR result = lstrcpyn(m_redactionAppearance.m_lgFont.lfFaceName, strFontName.c_str(), LF_FACESIZE);
        if (result == NULL)
        {
            UCLIDException uex("ELI29772", "Unable to copy Font name.");
            uex.addDebugInfo("Font Name To Copy", strFontName);
            uex.addDebugInfo("Font Name Length", strFontName.length());
            uex.addDebugInfo("Max Length", LF_FACESIZE);
            throw uex;
        }

        // Retrieve PDF settings
        ICopyableObjectPtr ipCopy = ipSource->PdfPasswordSettings;
        if (ipCopy != NULL)
        {
            m_ipPdfSettings = ipCopy->Clone();
            ASSERT_RESOURCE_ALLOCATION("ELI29770", m_ipPdfSettings != NULL);
        }
        else
        {
            m_ipPdfSettings = NULL;
        }

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09866");
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_IsConfigured(VARIANT_BOOL* pbValue)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check parameter
        ASSERT_ARGUMENT("ELI28322", pbValue != NULL);

        // Check license
        validateLicense();

        // Configured if:
        // 1. Output file name is not empty
        // 2. Pdf settings are either NULL or properly configured
        IMustBeConfiguredObjectPtr ipConfigure = m_ipPdfSettings;
        bool bConfigured = !m_strOutputFileName.empty()
            && (ipConfigure == NULL || ipConfigure->IsConfigured() == VARIANT_TRUE);
        *pbValue = asVariantBool(bConfigured);

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09005");
}

//-------------------------------------------------------------------------------------------------
// IRedactionTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_OutputFileName(BSTR* pVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check parameter
        ASSERT_ARGUMENT("ELI28324", pVal != NULL);

        validateLicense();

        *pVal = _bstr_t(m_strOutputFileName.c_str()).Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09871");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_OutputFileName(BSTR newVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())
    try
    {
        validateLicense();
            
        string strFileName = asString(newVal);

        // Create a local IFAMTagManagerPtr object
        UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
        ASSERT_RESOURCE_ALLOCATION("ELI15030", ipFAMTagManager != NULL);

        // Make sure the file name contains valid string tags
        if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
        {

            UCLIDException ue("ELI15031", "The output file name contains invalid tags.");
            ue.addDebugInfo("Output file", strFileName);
            throw ue;
        }

        // Assign the output file name
        m_strOutputFileName = strFileName;
        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09872");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_AttributeNames(IVariantVector** ppVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    try
    {
        ASSERT_ARGUMENT("ELI26085", ppVal != NULL);

        validateLicense();
        
        *ppVal = NULL;
        if (m_ipAttributeNames != NULL)
        {
            // Get a ShallowCopyableObject ptr for the current name list
            IShallowCopyablePtr ipObjSource = m_ipAttributeNames;
            ASSERT_RESOURCE_ALLOCATION("ELI15339", ipObjSource != NULL);

            // Shallow copy the attribute names
            IVariantVectorPtr ipObjCloned = ipObjSource->ShallowCopy();
            ASSERT_RESOURCE_ALLOCATION("ELI15340", ipObjCloned != NULL);

            // set the return value to the shallow copied object
            *ppVal = ipObjCloned.Detach();
        }

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11783");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_AttributeNames(IVariantVector *pVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())
    try
    {
        validateLicense();
        
        m_ipAttributeNames = pVal;
        
        m_setAttributeNames.clear();
        if (m_ipAttributeNames != NULL)
        {
            fillAttributeSet(m_ipAttributeNames, m_setAttributeNames);
        }
        
        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11784");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_VOAFileName(BSTR* pVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI28326", pVal != NULL);

        validateLicense();

        *pVal = _bstr_t(m_strVOAFileName.c_str()).Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12717");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_VOAFileName(BSTR newVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        validateLicense();
            
        string strFileName = asString(newVal);

        // Create a local IFAMTagManagerPtr object
        UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
        ASSERT_RESOURCE_ALLOCATION("ELI15032", ipFAMTagManager != NULL);

        // Make sure the file name contains valid string tags
        if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
        {

            UCLIDException ue("ELI15033", "The VOA file name contains invalid tags.");
            ue.addDebugInfo("VOA file", strFileName);
            throw ue;
        }

        // Assign the voa file name
        m_strVOAFileName = strFileName;
        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12716");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_CarryForwardAnnotations(VARIANT_BOOL* pVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI28327", pVal != NULL);

        // Check license state
        validateLicense();

        // Return setting to caller
        *pVal = asVariantBool(m_bCarryForwardAnnotations);

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14597");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_CarryForwardAnnotations(VARIANT_BOOL newVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())
    
    try
    {
        // Check license state
        validateLicense();

        // Save new setting
        m_bCarryForwardAnnotations = asCppBool(newVal);
        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14598");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_ApplyRedactionsAsAnnotations(VARIANT_BOOL* pVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI28328", pVal != NULL);

        // Check license state
        validateLicense();

        // Return setting to caller
        *pVal = asVariantBool(m_bApplyRedactionsAsAnnotations);

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14599");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_ApplyRedactionsAsAnnotations(VARIANT_BOOL newVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())
    
    try
    {
        // Check license state
        validateLicense();

        // Save new setting
        m_bApplyRedactionsAsAnnotations = asCppBool(newVal);
        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14600");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_UseRedactedImage(VARIANT_BOOL* pvbUseRedactedImage)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI24751", pvbUseRedactedImage != NULL);

        // Check license state
        validateLicense();

        // Return setting to caller
        *pvbUseRedactedImage = asVariantBool(m_bUseRedactedImage);

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24720")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_UseRedactedImage(VARIANT_BOOL vbUseRedactedImage)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license state
        validateLicense();

        m_bUseRedactedImage = asCppBool(vbUseRedactedImage);

        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24721")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_RedactionText(BSTR* pbstrRedactionText)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI24752", pbstrRedactionText != NULL);

        // Check license state
        validateLicense();

        // Return setting to caller
        *pbstrRedactionText = _bstr_t(m_redactionAppearance.m_strText.c_str()).Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24722")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_RedactionText(BSTR bstrRedactionText)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license state
        validateLicense();

        m_redactionAppearance.m_strText = asString(bstrRedactionText);

        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24723")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_BorderColor(long* plBorderColor)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI24753", plBorderColor != NULL);

        // Check license state
        validateLicense();

        // Return setting to caller
        *plBorderColor = m_redactionAppearance.m_crBorderColor;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24724")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_BorderColor(long lBorderColor)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license state
        validateLicense();

        m_redactionAppearance.m_crBorderColor = lBorderColor;

        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24725")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_FillColor(long* plFillColor)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI24754", plFillColor != NULL);

        // Check license state
        validateLicense();

        // Return setting to caller
        *plFillColor = m_redactionAppearance.m_crFillColor;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24726")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_FillColor(long lFillColor)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license state
        validateLicense();

        m_redactionAppearance.m_crFillColor = lFillColor;

        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24727")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_FontName(BSTR* pbstrFontName)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI24755", pbstrFontName != NULL);

        // Check license state
        validateLicense();

        // Return setting to caller
        *pbstrFontName = _bstr_t(m_redactionAppearance.m_lgFont.lfFaceName).Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24728")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_FontName(BSTR bstrFontName)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license state
        validateLicense();

        string strFontName = asString(bstrFontName);
        LPTSTR result = lstrcpyn(m_redactionAppearance.m_lgFont.lfFaceName, strFontName.c_str(), LF_FACESIZE);
        if (result == NULL)
        {
            UCLIDException uex("ELI29773", "Unable to copy Font name.");
            uex.addDebugInfo("Font Name To Copy", strFontName);
            uex.addDebugInfo("Font Name Length", strFontName.length());
            uex.addDebugInfo("Max Length", LF_FACESIZE);
            throw uex;
        }

        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24729")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_IsBold(VARIANT_BOOL* pvbBold)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI24867", pvbBold != NULL);

        // Check license state
        validateLicense();

        // Return setting to caller
        *pvbBold = asVariantBool(m_redactionAppearance.m_lgFont.lfWeight >= FW_BOLD);

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24730")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_IsBold(VARIANT_BOOL vbBold)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license state
        validateLicense();

        m_redactionAppearance.m_lgFont.lfWeight = vbBold == VARIANT_TRUE ? FW_BOLD : FW_NORMAL;

        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24731")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_IsItalic(VARIANT_BOOL* pvbItalic)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI24868", pvbItalic != NULL);
        
        // Check license state
        validateLicense();

        // Return setting to caller
        *pvbItalic = asVariantBool(m_redactionAppearance.m_lgFont.lfItalic == gucIS_ITALIC);

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24732")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_IsItalic(VARIANT_BOOL vbItalic)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license state
        validateLicense();

        m_redactionAppearance.m_lgFont.lfItalic = vbItalic == VARIANT_TRUE ? gucIS_ITALIC : 0;

        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24733")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_FontSize(long* plFontSize)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        ASSERT_ARGUMENT("ELI24869", plFontSize != NULL);

        // Check license state
        validateLicense();

        // Return setting to caller
        *plFontSize = m_redactionAppearance.m_iPointSize;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24734")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_FontSize(long lFontSize)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license state
        validateLicense();

        m_redactionAppearance.m_iPointSize = lFontSize;

        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29804");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_PdfPasswordSettings(IPdfPasswordSettings* pPdfSettings)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
        // Check license state
        validateLicense();

        // Store the settings (NULL is acceptable setting value)
        m_ipPdfSettings = pPdfSettings;

        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29803");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_PdfPasswordSettings(IPdfPasswordSettings** ppPdfSettings)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
        // Check license state
        validateLicense();

        ASSERT_RESOURCE_ALLOCATION("ELI29785", ppPdfSettings != NULL);

        IPdfPasswordSettingsPtr ipShallowCopy = m_ipPdfSettings;

        *ppPdfSettings = ipShallowCopy.Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29786");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::GetFontData(BSTR* pbstrFontName, VARIANT_BOOL* pvbIsBold,
        VARIANT_BOOL* pvbIsItalic, long* plFontSize)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
        // Check license state
        validateLicense();

        ASSERT_ARGUMENT("ELI29787", pbstrFontName != NULL);
        ASSERT_ARGUMENT("ELI29788", pvbIsBold != NULL);
        ASSERT_ARGUMENT("ELI29789", pvbIsItalic != NULL);
        ASSERT_ARGUMENT("ELI29790", plFontSize != NULL);

        // Return the font data
        _bstr_t bstrFontName(m_redactionAppearance.m_lgFont.lfFaceName);
        *pbstrFontName = bstrFontName.Detach();
        *pvbIsBold = asVariantBool(m_redactionAppearance.m_lgFont.lfWeight >= FW_BOLD);
        *pvbIsItalic = asVariantBool(m_redactionAppearance.m_lgFont.lfItalic == gucIS_ITALIC);
        *plFontSize = m_redactionAppearance.m_iPointSize;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29791");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_AutoAdjustTextCasing(VARIANT_BOOL* pvbAdjustCasing)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
        ASSERT_ARGUMENT("ELI24868", pvbAdjustCasing != NULL);
        
        // Check license state
        validateLicense();

        // Return setting to caller
        *pvbAdjustCasing = asVariantBool(m_redactionAppearance.m_bAdjustTextCasing);

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31661");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_AutoAdjustTextCasing(VARIANT_BOOL vbAdjustCasing)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
        // Check license state
        validateLicense();

        m_redactionAppearance.m_bAdjustTextCasing = asCppBool(vbAdjustCasing);

        m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31662");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_ReplacementValues(IIUnknownVector** ppvecReplacements)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
		ASSERT_ARGUMENT("ELI31751", ppvecReplacements != __nullptr);

        // Check license state
        validateLicense();

		IIUnknownVectorPtr ipReplacements = m_redactionAppearance.getReplacements();
		ASSERT_RESOURCE_ALLOCATION("ELI31752", ipReplacements != __nullptr);

		*ppvecReplacements = ipReplacements.Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31753");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_ReplacementValues(IIUnknownVector* pvecReplacements)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
		IIUnknownVectorPtr ipReplacements(pvecReplacements);
		ASSERT_ARGUMENT("ELI31754", ipReplacements != __nullptr);

        // Check license state
        validateLicense();

		m_redactionAppearance.updateReplacementsFromVector(ipReplacements);
		m_bDirty = true;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31755");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_PrefixText(BSTR* pbstrPrefixText)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
		ASSERT_ARGUMENT("ELI31756", pbstrPrefixText != __nullptr);

        // Check license state
        validateLicense();

		*pbstrPrefixText = _bstr_t(m_redactionAppearance.m_strPrefixText.c_str()).Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31757");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_PrefixText(BSTR bstrPrefixText)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
        // Check license state
        validateLicense();

		m_redactionAppearance.m_strPrefixText = asString(bstrPrefixText);

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31758");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_SuffixText(BSTR* pbstrSuffixText)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
		ASSERT_ARGUMENT("ELI31759", pbstrSuffixText != __nullptr);

        // Check license state
        validateLicense();

		*pbstrSuffixText = _bstr_t(m_redactionAppearance.m_strSuffixText.c_str()).Detach();

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31760");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_SuffixText(BSTR bstrSuffixText)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
        // Check license state
        validateLicense();

		m_redactionAppearance.m_strSuffixText = asString(bstrSuffixText);

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31761");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::GetClassID(CLSID* pClassID)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    *pClassID = CLSID_RedactionTask;

    return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::IsDirty(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::Load(IStream* pStream)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
        // Check license state
        validateLicense();

        // Reset member variables
        clear();

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
            UCLIDException ue("ELI11002", 
                "Unable to load newer redaction task.");
            ue.addDebugInfo("Current Version", gnCurrentVersion);
            ue.addDebugInfo("Version to Load", nDataVersion);
            throw ue;
        }

        // Input/output files
        dataReader >> m_strOutputFileName;
        dataReader >> m_strVOAFileName;

        // Annotation settings
        dataReader >> m_bCarryForwardAnnotations;
        dataReader >> m_bApplyRedactionsAsAnnotations;

        // Attribute names
        bool bAttributeNames;
        dataReader >> bAttributeNames;
        if (bAttributeNames)
        {
            IPersistStreamPtr ipObj;
            readObjectFromStream(ipObj, pStream, "ELI11779");
            m_ipAttributeNames = ipObj;
            fillAttributeSet(m_ipAttributeNames, m_setAttributeNames);
        }
        else
        {
            m_ipAttributeNames = NULL;
        }

        // Legislation guard
        dataReader >> m_bUseRedactedImage;

        // Redaction appearance
        dataReader >> m_redactionAppearance.m_strText;

		// Get advanced redaction appearance settings
        if (nDataVersion >= 3)
        {
			vector<pair<string,string>>& vecReplacements = m_redactionAppearance.m_vecReplacements;

			string strTemp1(""), strTemp2("");
			unsigned long ulTemp(1);
			if (nDataVersion >= 4)
			{
				dataReader >> ulTemp;
			}
			for(unsigned long i=0; i < ulTemp; i++)
			{
				dataReader >> strTemp1;
				dataReader >> strTemp2;
				vecReplacements.push_back(make_pair(strTemp1, strTemp2));
			}
            dataReader >> m_redactionAppearance.m_bAdjustTextCasing;
        }
		if (nDataVersion >= 4)
		{
			dataReader >> m_redactionAppearance.m_strPrefixText;
			dataReader >> m_redactionAppearance.m_strSuffixText;
		}

        dataReader >> m_redactionAppearance.m_crBorderColor;
        dataReader >> m_redactionAppearance.m_crFillColor;

        string strFontName;
        dataReader >> strFontName;
        LPTSTR result = lstrcpyn(m_redactionAppearance.m_lgFont.lfFaceName, strFontName.c_str(), LF_FACESIZE);
        if (result == NULL)
        {
            UCLIDException uex("ELI29771", "Unable to copy Font name.");
            uex.addDebugInfo("Font Name To Copy", strFontName);
            uex.addDebugInfo("Font Name Length", strFontName.length());
            uex.addDebugInfo("Max Length", LF_FACESIZE);
            throw uex;
        }

        bool bItalic;
        dataReader >> bItalic;
        m_redactionAppearance.m_lgFont.lfItalic = bItalic ? gucIS_ITALIC : 0;

        bool bBold;
        dataReader >> bBold;
        m_redactionAppearance.m_lgFont.lfWeight = bBold ? FW_BOLD : FW_NORMAL;

        long lTemp;
        dataReader >> lTemp;
        m_redactionAppearance.m_iPointSize = (int) lTemp;

        if (nDataVersion >= 2)
        {
            // Read the Pdf password settings from the stream
            bool bSettings;
            dataReader >> bSettings;
            if (bSettings)
            {
                IPersistStreamPtr ipObj;
                readObjectFromStream(ipObj, pStream, "ELI29774");
                m_ipPdfSettings = ipObj;
                ASSERT_RESOURCE_ALLOCATION("ELI29775", m_ipPdfSettings != NULL);

                // Ensure the require passwords value is set [LRCAU #5749]
                m_ipPdfSettings->RequireUserAndOwnerPassword = VARIANT_TRUE;
            }
        }

        // Clear the dirty flag as we've loaded a fresh object
        m_bDirty = false;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11003");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::Save(IStream* pStream, BOOL fClearDirty)
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

        // Input/output files
        dataWriter << m_strOutputFileName;
        dataWriter << m_strVOAFileName;

        // Annotation settings
        dataWriter << m_bCarryForwardAnnotations;
        dataWriter << m_bApplyRedactionsAsAnnotations;

        // Attribute names
        bool bAttributeNames = (m_ipAttributeNames != NULL);
        dataWriter << bAttributeNames;

        // Legislation guard
        dataWriter << m_bUseRedactedImage;

        // Redaction text
        dataWriter << m_redactionAppearance.m_strText;

		// Advanced redaction text settings
		dataWriter << (unsigned long)m_redactionAppearance.m_vecReplacements.size();
		for(vector<pair<string,string>>::iterator it = m_redactionAppearance.m_vecReplacements.begin();
			it != m_redactionAppearance.m_vecReplacements.end(); it++)
		{
			dataWriter << it->first;
			dataWriter << it->second;
		}
        dataWriter << m_redactionAppearance.m_bAdjustTextCasing;

		dataWriter << m_redactionAppearance.m_strPrefixText;
		dataWriter << m_redactionAppearance.m_strSuffixText;

        // Save redaction color options
        dataWriter << m_redactionAppearance.m_crBorderColor;
        dataWriter << m_redactionAppearance.m_crFillColor;

        // Save font options
        dataWriter << string(m_redactionAppearance.m_lgFont.lfFaceName);
        dataWriter << (m_redactionAppearance.m_lgFont.lfItalic == gucIS_ITALIC);
        dataWriter << asCppBool(m_redactionAppearance.m_lgFont.lfWeight >= FW_BOLD);
        dataWriter << (long) m_redactionAppearance.m_iPointSize;


        // Write a bool to indicate whether there is a Pdf password settings object
        bool bSettings = m_ipPdfSettings != NULL;
        dataWriter << bSettings;

        dataWriter.flushToByteStream();

        // Write the bytestream data into the IStream object
        long nDataLength = data.getLength();
        pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
        pStream->Write(data.getData(), nDataLength, NULL);

        // Only save Attribute Names if they exist
        if (bAttributeNames)
        {
            IPersistStreamPtr ipObj = m_ipAttributeNames;
            writeObjectToStream(ipObj, pStream, "ELI11780", fClearDirty);
        }

        // Write the PDF settings object if it exists
        if (bSettings)
        {
            IPersistStreamPtr ipObj = m_ipPdfSettings;
            ASSERT_RESOURCE_ALLOCATION("ELI29776", ipObj != NULL);
            writeObjectToStream(ipObj, pStream, "ELI29777", fClearDirty);
        }

        // Clear the flag as specified
        if (fClearDirty)
        {
            m_bDirty = false;
        }

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11004");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())
    
    return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRedactionTask::clear()
{
    m_strOutputFileName = gstrDEFAULT_REDACTED_IMAGE_FILENAME;
    m_strVOAFileName = gstrDEFAULT_TARGET_FILENAME;
    m_bCarryForwardAnnotations = false;
    m_bApplyRedactionsAsAnnotations = false;

    // Use the original image
    m_bUseRedactedImage = false;
    
    // Reset to default values
    m_redactionAppearance.reset();
    m_setAttributeNames.clear();

    // Clear PDF settings
    m_ipPdfSettings = NULL;
}
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr CRedactionTask::getAFUtility()
{
    if (m_ipAFUtility == NULL)
    {
        m_ipAFUtility.CreateInstance(CLSID_AFUtility);
        ASSERT_RESOURCE_ALLOCATION("ELI09877", m_ipAFUtility != NULL);
    }
    return m_ipAFUtility;
}
//-------------------------------------------------------------------------------------------------
string CRedactionTask::getExemptionCodes(IAttributePtr ipAttribute)
{
    // Iterate over each sub attribute
    IIUnknownVectorPtr subAttributes = ipAttribute->SubAttributes;
    if (subAttributes != NULL)
    {
        int count = subAttributes->Size();
        for (int i = 0; i < count; i++)
        {
            // Find the exemption codes attribute
            IAttributePtr subAttribute = subAttributes->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI28802", subAttribute != NULL);
            if (asString(subAttribute->Name) == "ExemptionCodes")
            {
                // Get the exemption codes value
                ISpatialStringPtr ipValue = subAttribute->Value;
                ASSERT_RESOURCE_ALLOCATION("ELI28801", ipValue != NULL);

                return asString(ipValue->String);
            }
        }
    }

    // There are no exemption codes
    return "";
}
//-------------------------------------------------------------------------------------------------
void CRedactionTask::storeMetaData(const string& strVoaFile, IIUnknownVectorPtr ipAttributes, 
    IIUnknownVectorPtr ipRedactedAttributes, CTime tStartTime, double dSeconds, 
    const string& strSourceDocument, const string& strRedactedImage, bool bOverwroteOutput)
{
    try
    {
        ASSERT_ARGUMENT("ELI28429", ipAttributes != NULL);

        // Calculate the next id
        long lNextId = getNextId(ipAttributes);
        assignIds(ipRedactedAttributes, lNextId, strSourceDocument);

        // Calculate the next redaction session
        long lNextSession = getNextSessionId(ipAttributes);

        // Create and append the metadata attribute
        IAttributePtr ipMetaData = createMetaDataAttribute(lNextSession, strVoaFile, 
            ipRedactedAttributes, tStartTime, dSeconds, strSourceDocument, strRedactedImage, 
            bOverwroteOutput);
        ASSERT_RESOURCE_ALLOCATION("ELI28349", ipMetaData != NULL);
        ipAttributes->PushBack(ipMetaData);

        // Save the voa with the new metadata
        ipAttributes->SaveTo(strVoaFile.c_str(), false);
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28350")
}
//-------------------------------------------------------------------------------------------------
long CRedactionTask::getNextId(IIUnknownVectorPtr ipAttributes)
{
    try
    {
        ASSERT_ARGUMENT("ELI28430", ipAttributes != NULL);

        // Iterate over each attribute, looking for the largest id
        long lMaxId = 0;
        int count = ipAttributes->Size();
        for	(int i = 0; i < count; i++)
        {
            IAttributePtr ipAttribute = ipAttributes->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI28355", ipAttribute != NULL);

            string strName = asString(ipAttribute->Name);
            makeUpperCase(strName);

            // Check for the old revisions attribute
            long lCurrentId = 0;
            if (strName == "_OLDREVISIONS")
            {
                // Find the largest id of the old revisions
                lCurrentId = getNextId(ipAttribute->SubAttributes) - 1;
            }
            else
            {
                // Get the id of this attribute
                lCurrentId = getAttributeId(ipAttribute);
            }

            // If this the largest id thus far, store it
            if (lMaxId < lCurrentId)
            {
                lMaxId = lCurrentId;
            }
        }

        // The next id is the largest plus 1
        return lMaxId + 1;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28351")
}
//-------------------------------------------------------------------------------------------------
long CRedactionTask::getAttributeId(IAttributePtr ipAttribute)
{
    try
    {
        // -1 indicates no id found
        long lId = -1;
        
        // Get the value of the id attribute if it exists
        IAttributePtr ipIdAttribute = getIdAttribute(ipAttribute);
        if (ipIdAttribute != NULL)
        {
            ISpatialStringPtr ipValue = ipIdAttribute->Value;
            ASSERT_RESOURCE_ALLOCATION("ELI28358", ipValue != NULL);
            
            string strId = asString(ipValue->String);
            lId = asLong(strId);
        }

        return lId;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28356")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::getIdAttribute(IAttributePtr ipAttribute)
{
    try
    {
        ASSERT_ARGUMENT("ELI28431", ipAttribute != NULL);

        IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
        ASSERT_RESOURCE_ALLOCATION("ELI28360", ipSubAttributes != NULL);

        // Iterate over the sub attributes of the attribute
        int count = ipSubAttributes->Size();
        for (int i = 0; i < count; i++)
        {
            IAttributePtr ipSubAttribute = ipSubAttributes->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI28361", ipSubAttribute != NULL);

            string strName = asString(ipSubAttribute->Name);
            makeUpperCase(strName);

            // Check if this is the ID and revision attribute
            if (strName == "_IDANDREVISION")
            {
                return ipSubAttribute;
            }
        }

        // The attribute was not found
        return NULL;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28359")
}
//-------------------------------------------------------------------------------------------------
void CRedactionTask::assignIds(IIUnknownVectorPtr ipAttributes, long lNextId, 
                               const string& strSourceDocument)
{
    try
    {
        ASSERT_ARGUMENT("ELI28432", ipAttributes != NULL);

        // Iterate over each attribute
        int count = ipAttributes->Size();
        for (int i = 0; i < count; i++)
        {
            IAttributePtr ipAttribute = ipAttributes->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI28362", ipAttribute);

            // Check if this attribute already has an attribute id
            IAttributePtr ipIdAttribute = getIdAttribute(ipAttribute);
            if (ipIdAttribute == NULL)
            {
                // Create an attribute id for this attribute
                ipIdAttribute = createIdAttribute(strSourceDocument, lNextId);
                ASSERT_RESOURCE_ALLOCATION("ELI28421", ipIdAttribute != NULL)
                
                // Add the id attribute
                IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
                ASSERT_RESOURCE_ALLOCATION("ELI28423", ipSubAttributes != NULL);
                ipSubAttributes->PushBack(ipIdAttribute);

                // Increment the next id
                lNextId++;
            }
        }
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28352")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::createIdAttribute(const string& strSourceDocument, long lId)
{
    try
    {
        // Get the id as a string
        string strId = asString(lId);

        // Create the id attribute
        IAttributePtr ipIdAttribute = 
            createAttribute(strSourceDocument, "_IDAndRevision", strId, "_1");
        ASSERT_RESOURCE_ALLOCATION("ELI28422", ipIdAttribute != NULL);

        return ipIdAttribute;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28353")
}
//-------------------------------------------------------------------------------------------------
long CRedactionTask::getNextSessionId(IIUnknownVectorPtr ipAttributes)
{
    try
    {
        ASSERT_ARGUMENT("ELI28433", ipAttributes != NULL);

        // Iterate over each attribute, looking for the largest redaction session id
        long lMaxSession = 0;
        int count = ipAttributes->Size();
        for	(int i = 0; i < count; i++)
        {
            IAttributePtr ipAttribute = ipAttributes->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI28603", ipAttribute != NULL);

            string strName = asString(ipAttribute->Name);
            makeUpperCase(strName);

            // Check for the redaction session attribute
            long lCurrentSession = 0;
            if (strName == "_REDACTEDFILEOUTPUTSESSION")
            {
                // Get the session id from the attribute
                ISpatialStringPtr ipValue = ipAttribute->Value;
                ASSERT_RESOURCE_ALLOCATION("ELI28365", ipValue);

                string strSession = asString(ipValue->String);
                lCurrentSession = asLong(strSession);
            }

            // If this is the largest session thus far, store it
            if (lMaxSession < lCurrentSession)
            {
                lMaxSession = lCurrentSession;
            }
        }

        // The next session is the largest session plus 1
        return lMaxSession + 1;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28602")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::createMetaDataAttribute(long lSession, const string& strVoaFile,
    IIUnknownVectorPtr ipRedactedAttributes, CTime tStartTime, double dElapsedSeconds, 
    const string& strSourceDocument, const string& strRedactedImage, bool bOverwroteOutput)
{
    try
    {
        // User information
        IAttributePtr ipUserInfo = createUserInfoAttribute(strSourceDocument);
        ASSERT_RESOURCE_ALLOCATION("ELI28369", ipUserInfo != NULL);

        // Time and duration
        IAttributePtr ipTimeInfo = 
            createTimeInfoAttribute(strSourceDocument, tStartTime, dElapsedSeconds);
        ASSERT_RESOURCE_ALLOCATION("ELI28370", ipTimeInfo != NULL);

        // Source document
        IAttributePtr ipSourceDocument = 
            createAttribute(strSourceDocument, "_SourceDocName", strSourceDocument);
        ASSERT_RESOURCE_ALLOCATION("ELI28371", ipSourceDocument != NULL);

        // Data file (VOA)
        IAttributePtr ipDataFile = 
            createAttribute(strSourceDocument, "_IDShieldDataFile", strVoaFile);
        ASSERT_RESOURCE_ALLOCATION("ELI28372", ipDataFile != NULL);

        // Output image
        IAttributePtr ipOutputFile = 
            createAttribute(strSourceDocument, "_OutputFile", strRedactedImage);
        ASSERT_RESOURCE_ALLOCATION("ELI28373", ipOutputFile != NULL);

        // Attribute types redacted
        IAttributePtr ipRedactedCategories = createRedactedCategoriesAttribute(strSourceDocument);
        ASSERT_RESOURCE_ALLOCATION("ELI28374", ipRedactedCategories != NULL);

        // Output options
        IAttributePtr ipOptions = createOptionsAttribute(strSourceDocument, bOverwroteOutput);
        ASSERT_RESOURCE_ALLOCATION("ELI28375", ipOptions != NULL); 

        // Attributes redacted
        IAttributePtr ipRedactedEntries = 
            createRedactedEntriesAttribute(strSourceDocument, ipRedactedAttributes);
        ASSERT_RESOURCE_ALLOCATION("ELI28376", ipRedactedEntries != NULL);

        // Metadata attribute
        string strValue = asString(lSession);
        IAttributePtr ipMetaData = 
            createAttribute(strSourceDocument, "_RedactedFileOutputSession", strValue);
        ASSERT_RESOURCE_ALLOCATION("ELI28377", ipMetaData != NULL);

        IIUnknownVectorPtr ipSubAttributes = ipMetaData->SubAttributes;
        ASSERT_RESOURCE_ALLOCATION("ELI28424", ipSubAttributes != NULL);

        // Append the attributes that belong to the metadata attribute
        ipSubAttributes->PushBack(ipUserInfo);
        ipSubAttributes->PushBack(ipTimeInfo);
        ipSubAttributes->PushBack(ipSourceDocument);
        ipSubAttributes->PushBack(ipDataFile);
        ipSubAttributes->PushBack(ipOutputFile);
        ipSubAttributes->PushBack(ipRedactedCategories);
        ipSubAttributes->PushBack(ipOptions);
        ipSubAttributes->PushBack(ipRedactedEntries);

        return ipMetaData;		
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28354")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::createUserInfoAttribute(const string& strSourceDocument)
{
    try
    {
        // User name
        string strUserName = getCurrentUserName();
        IAttributePtr ipLogin = createAttribute(strSourceDocument, "_LoginID", strUserName);
        ASSERT_RESOURCE_ALLOCATION("ELI28386", ipLogin != NULL);

        // Computer name
        string strComputerName = getComputerName();
        IAttributePtr ipComputer = createAttribute(strSourceDocument, "_Computer", strComputerName);
        ASSERT_RESOURCE_ALLOCATION("ELI28387", ipComputer != NULL);

        // User information
        IAttributePtr ipUserInfo = createAttribute(strSourceDocument, "_UserInfo");
        ASSERT_RESOURCE_ALLOCATION("ELI28389", ipUserInfo != NULL);

        IIUnknownVectorPtr ipSubAttributes = ipUserInfo->SubAttributes;
        ASSERT_RESOURCE_ALLOCATION("ELI28425", ipUserInfo != NULL);

        // Append subattributes
        ipSubAttributes->PushBack(ipLogin);
        ipSubAttributes->PushBack(ipComputer);

        return ipUserInfo;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28381")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::createTimeInfoAttribute(const string& strSourceDocument, 
                                                      CTime tStartTime, double dElapsedSeconds)
{
    try
    {
        // Start date
        string strDate = tStartTime.Format("%#m/%#d/%Y");
        IAttributePtr ipDate = createAttribute(strSourceDocument, "_Date", strDate);
        ASSERT_RESOURCE_ALLOCATION("ELI28390", ipDate != NULL);

        // Start time
        string strTimeStarted = tStartTime.Format("%I:%M:%S %p");
        IAttributePtr ipTime = createAttribute(strSourceDocument, "_TimeStarted", strTimeStarted);
        ASSERT_RESOURCE_ALLOCATION("ELI28391", ipTime != NULL);

        // Elapsed seconds
        string strSeconds = asString(dElapsedSeconds, 3);
        IAttributePtr ipSeconds = createAttribute(strSourceDocument, "_TotalSeconds", strSeconds);
        ASSERT_RESOURCE_ALLOCATION("ELI28392", ipSeconds != NULL);

        // Time info attribute
        IAttributePtr ipTimeInfo = createAttribute(strSourceDocument, "_TimeInfo");
        ASSERT_RESOURCE_ALLOCATION("ELI28393", ipTimeInfo != NULL);

        IIUnknownVectorPtr ipSubAttributes = ipTimeInfo->SubAttributes;
        ASSERT_RESOURCE_ALLOCATION("ELI28426", ipSubAttributes != NULL);

        // Append subattributes
        ipSubAttributes->PushBack(ipDate);
        ipSubAttributes->PushBack(ipTime);
        ipSubAttributes->PushBack(ipSeconds);

        return ipTimeInfo;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28382")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::createRedactedCategoriesAttribute(const string& strSourceDocument)
{
    try
    {
        // Iterate over each attribute name
        string strNames = "";
        for each (string strName in m_setAttributeNames)
        {
            // Append them in a comma separated list
            if (!strNames.empty())
            {
                strNames += ",";
            }
            strNames += strName;
        }

        // Create categories redacted attribute
        IAttributePtr ipCategories = 
            createAttribute(strSourceDocument, "_AttributesToRedact", strNames);

        return ipCategories;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28383")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::createOptionsAttribute(const string& strSourceDocument,
    bool bOverwroteOutput)
{
    try
    {
        // Retain redactions
        string strRetainRedactions = m_bUseRedactedImage ? "Yes" : "No";
        IAttributePtr ipRetainRedactions = createAttribute(strSourceDocument, 
            "_RetainExistingRedactionsInOutputFile", strRetainRedactions);
        ASSERT_RESOURCE_ALLOCATION("ELI28394", ipRetainRedactions != NULL);

        // Overwrote output file
        string strOverwrote = bOverwroteOutput ? "Yes" : "No";
        IAttributePtr ipOverwrote = createAttribute(strSourceDocument, 
            "_OutputFileExistedPriorToOutputOperation", strOverwrote);
        ASSERT_RESOURCE_ALLOCATION("ELI28395", ipOverwrote != NULL);

        // Retain annotations
        string strRetainAnnotations = m_bCarryForwardAnnotations ? "Yes" : "No";
        IAttributePtr ipRetainAnnotations = 
            createAttribute(strSourceDocument, "_RetainExistingAnnotations", strRetainAnnotations);
        ASSERT_RESOURCE_ALLOCATION("ELI28396", ipRetainAnnotations != NULL);

        // Apply as annotations
        string strApplyAsAnnotations = m_bApplyRedactionsAsAnnotations ? "Yes" : "No";
        IAttributePtr ipApplyAsAnnotations = createAttribute(strSourceDocument, 
            "_ApplyRedactionsAsAnnotations", strApplyAsAnnotations);
        ASSERT_RESOURCE_ALLOCATION("ELI28397", ipApplyAsAnnotations != NULL);

        // Redaction appearance settings
        IAttributePtr ipRedactionAppearance = createRedactionAppearanceAttribute(strSourceDocument);
        ASSERT_RESOURCE_ALLOCATION("ELI28398", ipRedactionAppearance != NULL);

        // Output options
        IAttributePtr ipOutputOptions = createAttribute(strSourceDocument, "_OutputOptions");
        ASSERT_RESOURCE_ALLOCATION("ELI28400", ipOutputOptions != NULL);

        IIUnknownVectorPtr ipSubAttributes = ipOutputOptions->SubAttributes;
        ASSERT_RESOURCE_ALLOCATION("ELI28427", ipSubAttributes != NULL);

        // Append sub attributes
        ipSubAttributes->PushBack(ipRetainRedactions);
        ipSubAttributes->PushBack(ipOverwrote);
        ipSubAttributes->PushBack(ipRetainAnnotations);
        ipSubAttributes->PushBack(ipApplyAsAnnotations);
        ipSubAttributes->PushBack(ipRedactionAppearance);

        return ipOutputOptions;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28384")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::createRedactionAppearanceAttribute(const string& strSourceDocument)
{
    try
    {
        // Text format
        IAttributePtr ipTextFormat = 
            createAttribute(strSourceDocument, "_TextFormat", m_redactionAppearance.m_strText);
        ASSERT_RESOURCE_ALLOCATION("ELI28411", ipTextFormat != NULL);

        // Fill color
        string strFillColor = getColorAsString(m_redactionAppearance.m_crFillColor);
        IAttributePtr ipFillColor = 
            createAttribute(strSourceDocument, "_FillColor", strFillColor);
        ASSERT_RESOURCE_ALLOCATION("ELI28412", ipFillColor != NULL);

        // Border color
        string strBorderColor = getColorAsString(m_redactionAppearance.m_crBorderColor);
        IAttributePtr ipBorderColor = 
            createAttribute(strSourceDocument, "_BorderColor", strBorderColor);
        ASSERT_RESOURCE_ALLOCATION("ELI28413", ipBorderColor != NULL);

        // Font
        string strFont = m_redactionAppearance.getFontAsString();
        IAttributePtr ipFont = createAttribute(strSourceDocument, "_Font", strFont);
        ASSERT_RESOURCE_ALLOCATION("ELI28414", ipFont != NULL);

        // Redaction appearance attribute
        IAttributePtr ipRedactionAppearance = 
            createAttribute(strSourceDocument, "_RedactionTextAndColorSettings");
        ASSERT_RESOURCE_ALLOCATION("ELI28415", ipRedactionAppearance != NULL);

        IIUnknownVectorPtr ipSubAttributes = ipRedactionAppearance->SubAttributes;
        ASSERT_RESOURCE_ALLOCATION("ELI28428", ipSubAttributes != NULL);

        // Append sub attributes
        ipSubAttributes->PushBack(ipTextFormat);
        ipSubAttributes->PushBack(ipFillColor);
        ipSubAttributes->PushBack(ipBorderColor);
        ipSubAttributes->PushBack(ipFont);

        return ipRedactionAppearance;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28399")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::createRedactedEntriesAttribute(const string& strSourceDocument, 
    IIUnknownVectorPtr ipRedactedAttributes)
{
    try
    {
        ASSERT_ARGUMENT("ELI28434", ipRedactedAttributes != NULL);

        // Redacted entries attribute
        IAttributePtr ipRedacted = createAttribute(strSourceDocument, "_EntriesRedacted");
        ASSERT_RESOURCE_ALLOCATION("ELI28416", ipRedacted != NULL);

        IIUnknownVectorPtr ipSubAttributes = ipRedacted->SubAttributes;
        ASSERT_RESOURCE_ALLOCATION("ELI28419", ipSubAttributes != NULL);

        // Iterate over each redacted attribute
        int count = ipRedactedAttributes->Size();
        for (int i = 0; i < count; i++)
        {
            IAttributePtr ipAttribute = ipRedactedAttributes->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI28417", ipAttribute != NULL);

            IAttributePtr ipIdAttribute = getIdAttribute(ipAttribute);
            ASSERT_RESOURCE_ALLOCATION("ELI28418", ipIdAttribute != NULL);

            // Append the ID attribute
            ipSubAttributes->PushBack(ipIdAttribute);
        }

        return ipRedacted;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28385")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::createAttribute(const string& strSourceDocument, 
    const string& strName, const string& strValue)
{
    try
    {
        // Create a non spatial string to represent the value
        ISpatialStringPtr ipValue(CLSID_SpatialString);
        ASSERT_RESOURCE_ALLOCATION("ELI28363", ipValue != NULL);
        ipValue->CreateNonSpatialString(strValue.c_str(), strSourceDocument.c_str());

        // Create an attribute with the specified name and value
        IAttributePtr ipAttribute(CLSID_Attribute);
        ASSERT_RESOURCE_ALLOCATION("ELI28364", ipAttribute != NULL);
        ipAttribute->Name = strName.c_str();
        ipAttribute->Value = ipValue;

        return ipAttribute;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28366")
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CRedactionTask::createAttribute(const string& strSourceDocument, 
    const string& strName, const string& strValue, const string& strType)
{
    try
    {
        // Create an attribute with the specified name and value
        IAttributePtr ipAttribute = createAttribute(strSourceDocument, strName, strValue);
        ASSERT_RESOURCE_ALLOCATION("ELI28368", ipAttribute);

        // Also set the type
        ipAttribute->Type = strType.c_str();

        return ipAttribute;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28367")
}
//-------------------------------------------------------------------------------------------------
string CRedactionTask::getColorAsString(COLORREF crColor)
{
    try
    {
        return crColor == RGB(255, 255, 255) ? "White" : "Black";
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28420")
}
//-------------------------------------------------------------------------------------------------
void CRedactionTask::validateLicense()
{
    VALIDATE_LICENSE(gnIDSHIELD_AUTOREDACTION_OBJECT, "ELI09999", "Redaction File Processor");
}
//-------------------------------------------------------------------------------------------------
void CRedactionTask::fillAttributeSet(IVariantVectorPtr ipAttributeNames, set<string>& rsetAttributeNames)
{
    long nSize = ipAttributeNames->Size;
    for (long n = 0; n < nSize; n++)
    {
        rsetAttributeNames.insert(asString(ipAttributeNames->Item[n].bstrVal));
    }
}
//-------------------------------------------------------------------------------------------------
UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr CRedactionTask::getIDShieldDBPtr()
{
    if (m_ipIDShieldDB == NULL)
    {
        m_ipIDShieldDB.CreateInstance(CLSID_IDShieldProductDBMgr);
        ASSERT_RESOURCE_ALLOCATION("ELI19794", m_ipIDShieldDB != NULL);		
    }
    return m_ipIDShieldDB;
}
//-------------------------------------------------------------------------------------------------
