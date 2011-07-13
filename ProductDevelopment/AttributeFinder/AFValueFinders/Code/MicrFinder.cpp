
// MicrFinder.cpp : Implementation of CMicrFinder
#include "stdafx.h"
#include "MicrFinder.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <Common.h>
#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <InliteNamedMutexConstants.h>
#include <LicenseMgmt.h>
#include <MathUtil.h>
#include <Misc.h>
#include <RegistryPersistenceMgr.h>
#include <UCLIDException.h>
#include <TemporaryFileName.h>
#include <MiscLeadUtils.h>

#include <vector>

using namespace::std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const unsigned long gulCURRENT_VERSION = 2;

// File containing a regex to use for finding the old style routing number near the top of
// the check.  If this file exists, this regex will override the one defined below.
static const string gstrREGEX_FILE = "FindOtherRoutingRegex.dat.etf";

// Regex for matching the old style routing number found near the top of a check
static const string gstrOTHER_ROUTING_REGEX = "(?<=\\b(?:\\d\\w{0,2}|\\w?\\d\\w?|\\w{0,2}\\d)(?:\\s?-\\s?|\\s{1,3}))"
											 "\\b(\\d\\w{0,3}|\\w?\\d\\w{0,2}?|\\w{0,2}\\d\\w|\\w{0,3}\\d)"
											 "(?:\\s?[\\\\!\\/|]\\s?|\\s+)"
											 "(\\d\\w{0,3}|\\w?\\d\\w{0,2}?|\\w{0,2}\\d\\w|\\w{0,3}\\d)\\b";

// Registry keys for MICR finder settings
static const string gstrMICR_FINDER_KEY =  gstrAF_REG_ROOT_FOLDER_PATH
	+ string("\\AFValueFinders\\MicrFinder");
static const string gstrMICR_FINDER_FLAGS_KEY = "Flags";

// Default value for the MICR flags registry key
static const int giMICR_FINDER_FLAGS_DEFAULT = emrfExtendedMicrSearch + emrfEnforceAbaParsing;

//-------------------------------------------------------------------------------------------------
// Local helper functions
//-------------------------------------------------------------------------------------------------
void rotateMicrZone(MicrZone& rMicrZone, int iRotation, long lWidth, long lHeight)
{
	// Rotate the zone if needed
	if (iRotation != 0)
	{
		// Get the rectangle from the zone
		RECT rect = rMicrZone.m_rectZone;

		// Rotate the points
		int iLeft(0), iTop(0), iRight(0), iBottom(0);
		switch(iRotation)
		{
		case 90:
			{
				iLeft = rect.top;
				iTop = lHeight - rect.right;
				iRight = rect.bottom;
				iBottom = lHeight - rect.left;
			}
			break;

		case 180:
			{
				iLeft = lWidth - rect.right;
				iTop = lHeight - rect.bottom;
				iRight = lWidth - rect.left;
				iBottom = lHeight - rect.top;
			}
			break;

		case 270:
			{
				iLeft = lWidth - rect.bottom;
				iTop = rect.left;
				iRight = lWidth - rect.top;
				iBottom = rect.right;
			}
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI25049");
		}

		// Set the rectangle from the rotated points
		rect.left = iLeft;
		rect.top = iTop;
		rect.right = iRight;
		rect.bottom = iBottom;

		// Store the rectangle back in the micr zone
		rMicrZone.m_rectZone = rect;
	}
}
//-------------------------------------------------------------------------------------------------
bool allLinesHaveSameRotation(const vector<MicrLine>& vecLines)
{
	if (vecLines.size() == 0)
	{
		return false;
	}
	else
	{
		// Get the rotation from the first line
		int nRotation = vecLines[0].m_Info.m_nRotation;
		for (size_t i=1; i < vecLines.size(); i++)
		{
			if (vecLines[i].m_Info.m_nRotation != nRotation)
			{
				return false;
			}
		}

		// All lines have the same rotation, return true
		return true;
	}
}
//-------------------------------------------------------------------------------------------------
RECT getOtherRoutingSearchZone(const vector<MicrLine>& vecLines, const MicrZone& micrZone,
							   const MicrPage& micrPage)
{
	// Rectangle to return
	RECT rectArea;

	// Check if all lines have the same rotation
	bool bLinesSameRotation = allLinesHaveSameRotation(vecLines);

	// Handle the case where there is only 1 MICR line on the page
	// or all MICR lines have the same rotation
	if (vecLines.size() == 1 || bLinesSameRotation)
	{
		switch(micrZone.m_nRotation)
		{
		case 0:
			{
				rectArea.left = micrZone.m_rectZone.left;
				rectArea.right = micrPage.m_lWidth;
				rectArea.bottom = micrZone.m_rectZone.top;
				rectArea.top = vecLines.size() == 1 ? 0 : micrZone.m_rectZone.top;
				if (bLinesSameRotation)
				{
					// Look for MICR line above the current line
					for(vector<MicrLine>::const_iterator it = vecLines.begin();
						it != vecLines.end(); it++)
					{
						int iTop = it->m_Info.m_rectZone.bottom;
						if(iTop < rectArea.top)
						{
							rectArea.top = iTop;
						}
					}
				}
			}
			break;

		case 90:
			{
				rectArea.left = vecLines.size() == 1 ? 0 : micrZone.m_rectZone.left;
				rectArea.top = 0;
				rectArea.right = micrZone.m_rectZone.left;
				rectArea.bottom = micrZone.m_rectZone.bottom;
				if (bLinesSameRotation)
				{
					// Look for MICR line to the left of the current line
					for(vector<MicrLine>::const_iterator it = vecLines.begin();
						it != vecLines.end(); it++)
					{
						int iLeft = it->m_Info.m_rectZone.right;
						if(iLeft < rectArea.left)
						{
							rectArea.left = iLeft;
						}
					}
				}
			}
			break;

		case 180:
			{
				rectArea.left = 0;
				rectArea.top = micrZone.m_rectZone.bottom;
				rectArea.right = micrZone.m_rectZone.right;
				rectArea.bottom =
					vecLines.size() == 1 ? micrPage.m_lHeight : micrZone.m_rectZone.bottom;
				if (bLinesSameRotation)
				{
					// Look for MICR line below the current line
					for(vector<MicrLine>::const_iterator it = vecLines.begin();
						it != vecLines.end(); it++)
					{
						int iBottom = it->m_Info.m_rectZone.top;
						if(iBottom > rectArea.bottom)
						{
							rectArea.bottom = iBottom;
						}
					}
				}
			}
			break;

		case 270:
			{
				rectArea.left = micrZone.m_rectZone.left;
				rectArea.top = micrZone.m_rectZone.top;
				rectArea.right =
					vecLines.size() == 1 ? micrPage.m_lWidth : micrZone.m_rectZone.left;
				rectArea.bottom = micrPage.m_lHeight;
				if (bLinesSameRotation)
				{
					// Look for MICR line to the right of the current line
					for(vector<MicrLine>::const_iterator it = vecLines.begin();
						it != vecLines.end(); it++)
					{
						int iRight = it->m_Info.m_rectZone.left;
						if(iRight > rectArea.right)
						{
							rectArea.right = iRight;
						}
					}
				}
			}
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI25009");
		}
	}
	// Else - more than one line and they have differing orientations
	else
	{
		switch(micrZone.m_nRotation)
		{
		case 0:
			{
				rectArea.left = micrZone.m_rectZone.left;
				rectArea.bottom = micrZone.m_rectZone.top;
				rectArea.right = micrPage.m_lWidth;
				rectArea.top = 0;

				// Now look for another MICR line that is above the current MICR line, or
				// to the right of the current MICR line
				for (vector<MicrLine>::const_iterator it = vecLines.begin();
					it != vecLines.end(); it++)
				{
					// Look for zone to the right of the current MICR line
					if (it->m_Info.m_rectZone.left > micrZone.m_rectZone.right
						&& it->m_Info.m_rectZone.bottom < micrZone.m_rectZone.top
						&& it->m_Info.m_rectZone.top > micrZone.m_rectZone.bottom)
					{
						// Get the rotation of the check
						if (it->m_Info.m_nRotation == 90)
						{
							// Compute half the distance between zone right and other zone left
							rectArea.right =
								(it->m_Info.m_rectZone.left - micrZone.m_rectZone.right) / 2;
							rectArea.right += micrZone.m_rectZone.right;
						}
						else
						{
							rectArea.right = it->m_Info.m_rectZone.left;
						}
					}
					else if (it->m_Info.m_rectZone.bottom < micrZone.m_rectZone.top
						&& it->m_Info.m_rectZone.left < micrZone.m_rectZone.right
						&& it->m_Info.m_rectZone.right > micrZone.m_rectZone.left)
					{
						if (it->m_Info.m_nRotation == 180)
						{
							// Compute half the distance between zone top and other zone bottom
							rectArea.top =
								(micrZone.m_rectZone.top - it->m_Info.m_rectZone.bottom) / 2;
							rectArea.top += micrZone.m_rectZone.top;
						}
						else
						{
							rectArea.top = it->m_Info.m_rectZone.bottom;
						}
					}
				}
			}
			break;

		case 90:
			{
				rectArea.left = 0;
				rectArea.top = 0;
				rectArea.right = micrZone.m_rectZone.left;
				rectArea.bottom = micrZone.m_rectZone.bottom;

				// Now look for another MICR line that is above the current MICR line, or
				// to the left of the current MICR line
				for (vector<MicrLine>::const_iterator it = vecLines.begin();
					it != vecLines.end(); it++)
				{
					// Look for zone to the left of the current MICR line
					if (it->m_Info.m_rectZone.right < micrZone.m_rectZone.left
						&& it->m_Info.m_rectZone.bottom < micrZone.m_rectZone.top
						&& it->m_Info.m_rectZone.top > micrZone.m_rectZone.bottom)
					{
						// Get the rotation of the check
						if (it->m_Info.m_nRotation == 270)
						{
							// Compute half the distance between zone left and other zone right
							rectArea.left =
								(micrZone.m_rectZone.left - it->m_Info.m_rectZone.right) / 2;
							rectArea.left = micrZone.m_rectZone.left - rectArea.left;
						}
						else
						{
							rectArea.left = it->m_Info.m_rectZone.right;
						}
					}
					else if (it->m_Info.m_rectZone.bottom < micrZone.m_rectZone.top
						&& it->m_Info.m_rectZone.left < micrZone.m_rectZone.right
						&& it->m_Info.m_rectZone.right > micrZone.m_rectZone.left)
					{
						if (it->m_Info.m_nRotation == 180)
						{
							// Compute half the distance between zone top and other zone bottom
							rectArea.top =
								(micrZone.m_rectZone.top - it->m_Info.m_rectZone.bottom) / 2;
							rectArea.top = micrZone.m_rectZone.top - rectArea.top;
						}
						else
						{
							rectArea.top = it->m_Info.m_rectZone.bottom;
						}
					}
				}
			}
			break;

		case 180:
			{
				rectArea.left = 0;
				rectArea.top = micrZone.m_rectZone.bottom;
				rectArea.right = micrZone.m_rectZone.right;
				rectArea.bottom = micrPage.m_lHeight;

				// Now look for another MICR line that is below the current MICR line, or
				// to the left of the current MICR line
				for (vector<MicrLine>::const_iterator it = vecLines.begin();
					it != vecLines.end(); it++)
				{
					// Look for zone to the left of the current MICR line
					if (it->m_Info.m_rectZone.right < micrZone.m_rectZone.left
						&& it->m_Info.m_rectZone.bottom < micrZone.m_rectZone.top
						&& it->m_Info.m_rectZone.top > micrZone.m_rectZone.bottom)
					{
						// Get the rotation of the check
						if (it->m_Info.m_nRotation == 270)
						{
							// Compute half the distance between zone left and other zone right
							rectArea.left =
								(micrZone.m_rectZone.left - it->m_Info.m_rectZone.right) / 2;
							rectArea.left = micrZone.m_rectZone.left - rectArea.left;
						}
						else
						{
							rectArea.left = it->m_Info.m_rectZone.right;
						}
					}
					else if (it->m_Info.m_rectZone.top > micrZone.m_rectZone.bottom
						&& it->m_Info.m_rectZone.left < micrZone.m_rectZone.right
						&& it->m_Info.m_rectZone.right > micrZone.m_rectZone.left)
					{
						if (it->m_Info.m_nRotation == 0)
						{
							// Compute half the distance between zone bottom and other zone top
							rectArea.bottom =
								(it->m_Info.m_rectZone.top - micrZone.m_rectZone.bottom) / 2;
							rectArea.bottom += micrZone.m_rectZone.bottom;
						}
						else
						{
							rectArea.bottom = it->m_Info.m_rectZone.top;
						}
					}
				}
			}
			break;

		case 270:
			{
				rectArea.left = micrZone.m_rectZone.left;
				rectArea.top = micrZone.m_rectZone.top;
				rectArea.right = micrPage.m_lWidth;
				rectArea.bottom = micrPage.m_lHeight;

				// Now look for another MICR line that is below the current MICR line, or
				// to the right of the current MICR line
				for (vector<MicrLine>::const_iterator it = vecLines.begin();
					it != vecLines.end(); it++)
				{
					// Look for zone to the right of the current MICR line
					if (it->m_Info.m_rectZone.left > micrZone.m_rectZone.right
						&& it->m_Info.m_rectZone.bottom < micrZone.m_rectZone.top
						&& it->m_Info.m_rectZone.top > micrZone.m_rectZone.bottom)
					{
						// Get the rotation of the check
						if (it->m_Info.m_nRotation == 90)
						{
							// Compute half the distance between zone right and other zone left
							rectArea.right =
								(it->m_Info.m_rectZone.left - micrZone.m_rectZone.right) / 2;
							rectArea.right += micrZone.m_rectZone.right;
						}
						else
						{
							rectArea.right = it->m_Info.m_rectZone.left;
						}
					}
					else if (it->m_Info.m_rectZone.top > micrZone.m_rectZone.bottom
						&& it->m_Info.m_rectZone.left < micrZone.m_rectZone.right
						&& it->m_Info.m_rectZone.right > micrZone.m_rectZone.left)
					{
						if (it->m_Info.m_nRotation == 0)
						{
							// Compute half the distance between zone bottom and other zone top
							rectArea.bottom =
								(it->m_Info.m_rectZone.top - micrZone.m_rectZone.bottom) / 2;
							rectArea.bottom += micrZone.m_rectZone.bottom;
						}
						else
						{
							rectArea.bottom = it->m_Info.m_rectZone.top;
						}
					}
				}
			}
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI25048");
		}
	}

	// Rotate the area based on the MICR line rotation [FlexIDSCore #3525]
	rotateRectangle(rectArea, micrPage.m_lWidth, micrPage.m_lHeight, micrZone.m_nRotation);
	
	return rectArea;
}

//-------------------------------------------------------------------------------------------------
// CMicrFinder
//-------------------------------------------------------------------------------------------------
CMicrFinder::CMicrFinder() 
:	m_bDirty(false),
m_bSplitRoutingNumber(true),
m_bSplitAccountNumber(true),
m_bSplitCheckNumber(false),
m_bSplitAmount(false),
m_cachedRegExLoader(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str())
{
	try
	{
		// Default to most accurate search (all rotations of image)
		m_mapRotations[0] = true;
		m_mapRotations[90] = true;
		m_mapRotations[180] = true;
		m_mapRotations[270] = true;

		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29475", m_ipMiscUtils != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29476");
}
//-------------------------------------------------------------------------------------------------
CMicrFinder::~CMicrFinder()
{
	try
	{
		m_ipMiscUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24338");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IMicrFinder,
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24391");
}

//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::raw_ParseText(IAFDocument * pAFDoc, IProgressStatus *pProgressStatus,
										   IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate the license
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI24411", ipAFDoc != __nullptr);

		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI24339", ipAttributes != __nullptr);

		// Get the spatial string
		ISpatialStringPtr ipSS = ipAFDoc->Text;

		// Check that there is a spatial string and it has spatial info
		if (ipSS != __nullptr && ipSS->HasSpatialInfo() == VARIANT_TRUE)
		{
			// Get the pages from the spatial string [FlexIDSCore #3522]
			IIUnknownVectorPtr ipPages = ipSS->GetPages();
			ASSERT_RESOURCE_ALLOCATION("ELI25600", ipPages != __nullptr);

			// Build a vector of pages to process
			long lSize = ipPages->Size();
			vector<long> vecPages;
			for (long i=0; i < lSize; i++)
			{
				// Get the page
				ISpatialStringPtr ipPage = ipPages->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI25601", ipPage != __nullptr);

				// Get the page number for the page
				long lPageNum = ipPage->GetFirstPageNumber();

				// Add the page number to the vector
				vecPages.push_back(lPageNum);
			}

			// Find the MICR zones
			findMICRZones(ipSS, vecPages, ipAttributes);
		}
		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24341");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24342", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("MICR finder").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24343")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::raw_CopyFrom(IUnknown * pObject)
{
	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IMicrFinderPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI24344", ipSource != __nullptr);

		m_bSplitRoutingNumber = asCppBool(ipSource->SplitRoutingNumber);
		m_bSplitAccountNumber = asCppBool(ipSource->SplitAccountNumber);
		m_bSplitCheckNumber = asCppBool(ipSource->SplitCheckNumber);
		m_bSplitAmount = asCppBool(ipSource->SplitAmount);
		m_mapRotations[0] = asCppBool(ipSource->Rotate0);
		m_mapRotations[90] = asCppBool(ipSource->Rotate90);
		m_mapRotations[180] = asCppBool(ipSource->Rotate180);
		m_mapRotations[270] = asCppBool(ipSource->Rotate270);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24345");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI24346", pObject != __nullptr);

		ICopyableObjectPtr ipObjCopy(CLSID_MicrFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI24347", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new micr finder to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24348");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_MicrFinder;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// version 2 - Added rotations to search for
STDMETHODIMP CMicrFinder::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset settings to their defaults
		resetSettings();

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		if (nDataVersion > gulCURRENT_VERSION)
		{
			UCLIDException uex("ELI24349", "Unable to load newer MICR finder!");
			uex.addDebugInfo("Current Version", gulCURRENT_VERSION);
			uex.addDebugInfo("Version To Load", nDataVersion);
			throw uex;
		}

		// Load the data values
		dataReader >> m_bSplitRoutingNumber;
		dataReader >> m_bSplitAccountNumber;
		dataReader >> m_bSplitCheckNumber;
		dataReader >> m_bSplitAmount;

		if (nDataVersion == 2)
		{
			dataReader >> m_mapRotations[0];
			dataReader >> m_mapRotations[90];
			dataReader >> m_mapRotations[180];
			dataReader >> m_mapRotations[270];
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24350");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Store the version number
		dataWriter << gulCURRENT_VERSION;

		// Store the data values
		dataWriter << m_bSplitRoutingNumber;
		dataWriter << m_bSplitAccountNumber;
		dataWriter << m_bSplitCheckNumber;
		dataWriter << m_bSplitAmount;

		// Store the rotations to search
		dataWriter << m_mapRotations[0];
		dataWriter << m_mapRotations[90];
		dataWriter << m_mapRotations[180];
		dataWriter << m_mapRotations[270];

		// Flush the data to the byte stream
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24351");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	if (pcbSize == NULL)
		return E_POINTER;
		
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI24352", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24353");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMicrFinder Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::get_SplitRoutingNumber(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI24354", pVal != __nullptr);

		*pVal = asVariantBool(m_bSplitRoutingNumber);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24355");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::put_SplitRoutingNumber(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bSplitRoutingNumber = asCppBool(newVal);

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24356");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::get_SplitAccountNumber(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI24357", pVal != __nullptr);

		*pVal = asVariantBool(m_bSplitAccountNumber);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24358");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::put_SplitAccountNumber(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bSplitAccountNumber = asCppBool(newVal);

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24359");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::get_SplitCheckNumber(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI24360", pVal != __nullptr);

		*pVal = asVariantBool(m_bSplitCheckNumber);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24361");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::put_SplitCheckNumber(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bSplitCheckNumber = asCppBool(newVal);

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24362");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::get_SplitAmount(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI24363", pVal != __nullptr);

		*pVal = asVariantBool(m_bSplitAmount);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24364");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::put_SplitAmount(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bSplitAmount = asCppBool(newVal);

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24365");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::get_Rotate0(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI25010", pVal != __nullptr);

		*pVal = asVariantBool(m_mapRotations[0]);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25011");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::put_Rotate0(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_mapRotations[0] = asCppBool(newVal);

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25012");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::get_Rotate90(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI25013", pVal != __nullptr);

		*pVal = asVariantBool(m_mapRotations[90]);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25014");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::put_Rotate90(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_mapRotations[90] = asCppBool(newVal);

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25015");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::get_Rotate180(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI25016", pVal != __nullptr);

		*pVal = asVariantBool(m_mapRotations[180]);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25017");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::put_Rotate180(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_mapRotations[180] = asCppBool(newVal);

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25018");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::get_Rotate270(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI25019", pVal != __nullptr);

		*pVal = asVariantBool(m_mapRotations[270]);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25020");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinder::put_Rotate270(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_mapRotations[270] = asCppBool(newVal);

		// Set dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25021");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CMicrFinder::findMICRZones(ISpatialStringPtr ipSpatialString, const vector<long>& vecPages,
											IIUnknownVectorPtr ipAttributes)
{
	INIT_EXCEPTION_AND_TRACING("MLI02313");
	try
	{
		ASSERT_ARGUMENT("ELI24941", ipSpatialString != __nullptr);
		ASSERT_ARGUMENT("ELI24415", ipAttributes != __nullptr);

		// Ensure the image exists
		string strImageName = asString(ipSpatialString->SourceDocName);
		ASSERT_ARGUMENT("ELI24414", isValidFile(strImageName));

		// Handle PDF's (cannot load PDF directly into Inlite engine without PDF license)
		string strWorkingFile = strImageName;
		unique_ptr<TemporaryFileName> pTempFile(__nullptr);
		if (isPDF(strImageName))
		{
			pTempFile.reset(new TemporaryFileName(true, __nullptr, ".tif"));
			strWorkingFile = pTempFile->getName();
			convertPDFToTIF(strImageName, strWorkingFile);
		}

		_bstr_t bstrImageName = _bstr_t(strWorkingFile.c_str());
		_lastCodePos = "100";

		// Check if there is a need to create sub attributes
		bool bSplitSubattributes = m_bSplitRoutingNumber || m_bSplitAccountNumber
			|| m_bSplitCheckNumber || m_bSplitAmount;

		// Create a map to hold a collection of MicrPage items keyed by their page
		map<long, MicrPage> mapMicrPages;
		for (vector<long>::const_iterator it = vecPages.begin(); it != vecPages.end(); it++)
		{
			string strCount = asString(*it);
			_lastCodePos = "110_A_" + strCount;

			// Lock over inlite objects
			CSingleLock lock(&sg_mutexINLITE_MUTEX, TRUE);

			// Create a map to hold the image pointers
			map<int, ICiImagePtr> mapRotationToImage;
			try
			{

				// Create an inlite server for creating images
				ICiServerPtr ipServer(CLSID_CiServer);
				ASSERT_RESOURCE_ALLOCATION("ELI25053", ipServer != __nullptr);

				// Load the image (with no rotation)
				ICiImagePtr ipImage = ipServer->CreateImage();
				ASSERT_RESOURCE_ALLOCATION("ELI25022", ipImage != __nullptr);
				ipImage->Open(bstrImageName, *it);
				_lastCodePos = "110_B_" + strCount;

				// Check for searching normal orientation
				if (m_mapRotations[0])
				{
					// Add the image to the rotated image map
					mapRotationToImage[0] = ipImage;
				}

				// Get the image dimensions
				long lWidth = ipImage->Width;
				long lHeight = ipImage->Height;
				_lastCodePos = "110_C_" + strCount;

				// Get the image in its rotated forms (if needed)
				if (m_mapRotations[90])
				{
					ICiImagePtr ipImage90 = ipServer->CreateImage();
					ASSERT_RESOURCE_ALLOCATION("ELI25023", ipImage90 != __nullptr);
					ipImage90->Copy(ipImage);
					ipImage90->RotateRight();
					mapRotationToImage[90] = ipImage90;
				}
				if (m_mapRotations[180])
				{
					ICiImagePtr ipImage180 = ipServer->CreateImage();
					ASSERT_RESOURCE_ALLOCATION("ELI25024", ipImage180 != __nullptr);
					ipImage180->Copy(ipImage);
					ipImage180->RotateRight();
					ipImage180->RotateRight();
					mapRotationToImage[180] = ipImage180;
				}
				if (m_mapRotations[270])
				{
					ICiImagePtr ipImage270 = ipServer->CreateImage();
					ASSERT_RESOURCE_ALLOCATION("ELI25025", ipImage270 != __nullptr);
					ipImage270->Copy(ipImage);
					ipImage270->RotateLeft();
					mapRotationToImage[270] = ipImage270;
				}
				_lastCodePos = "110_D_" + strCount;

				// Create the MICR reader
				_CcMicrReaderPtr ipReader(CLSID_CcMicrReader);
				ASSERT_RESOURCE_ALLOCATION("ELI24416", ipReader != __nullptr);

				// Get the finding flags value from the registry [FlexIDSCore #3490]
				ipReader->Flags = getMicrReaderFlags();
				_lastCodePos = "110_E_" + strCount;

				// Now loop through the each version of the image and attempt to find the MICR
				for(map<int, ICiImagePtr>::iterator mapIt = mapRotationToImage.begin();
					mapIt != mapRotationToImage.end(); mapIt++)
				{
					try
					{
						try
						{
							// Load the image into the MICR finder
							ipReader->Image = mapIt->second;
							_lastCodePos = "110_E_" + strCount + "_5";

							// Find MICR
							ipReader->FindMICR();
							_lastCodePos = "110_E_" + strCount + "_10";

							// Get the count of MICR objects (only process MICR if at least one found)
							long lCount = ipReader->MicrCount;
							if (lCount > 0)
							{
								_lastCodePos = "110_E_" + strCount + "_20";
								// Find the micr page item for this page (if it exists)
								map<long, MicrPage>::iterator tempIt = mapMicrPages.find(*it);
								if (tempIt == mapMicrPages.end())
								{
									// Micr page does not exist yet, create it and add it to the map
									MicrPage micrPage(lWidth, lHeight);
									tempIt = mapMicrPages.insert(pair<long, MicrPage>(*it, micrPage)).first;
								}

								// Get the Micr line vector for the current page
								vector<MicrLine>& vecLines = tempIt->second.m_vecMicrLines;

								// Loop through each MICR line and build the vector lines
								for (long i=1; i <= lCount; i++)
								{
									// Get the MICR line
									_CcMicrPtr ipMicrLine = ipReader->GetMicrLine(i);
									ASSERT_RESOURCE_ALLOCATION("ELI25026", ipMicrLine != __nullptr);

									// Build up the MicrLine object for this line
									MicrLine micrLine;
									_CcMicrInfoPtr ipInfo = ipMicrLine->Info;
									if (ipInfo != __nullptr && ipInfo->IsRead == VARIANT_TRUE)
									{
										// Set the has info line to true based on buildMicrZone results
										micrLine.m_bHasInfo = buildMicrZone(ipInfo, mapIt->first,
											lWidth, lHeight, micrLine.m_Info);
									}
									ipInfo = ipMicrLine->Routing;
									if (ipInfo != __nullptr && ipInfo->IsRead == VARIANT_TRUE)
									{
										// Set the has routing line to true based on buildMicrZone results
										micrLine.m_bHasRouting = buildMicrZone(ipInfo, mapIt->first,
											lWidth, lHeight, micrLine.m_Routing);
									}
									ipInfo = ipMicrLine->Account;
									if (ipInfo != __nullptr && ipInfo->IsRead == VARIANT_TRUE)
									{
										// Set the has account line to true based on buildMicrZone results
										micrLine.m_bHasAccount = buildMicrZone(ipInfo, mapIt->first,
											lWidth, lHeight, micrLine.m_Account);
									}
									ipInfo = ipMicrLine->CheckNumber;
									if (ipInfo != __nullptr && ipInfo->IsRead == VARIANT_TRUE)
									{
										// Set the has check number line to true based on buildMicrZone results
										micrLine.m_bHasCheckNumber = buildMicrZone(ipInfo, mapIt->first,
											lWidth, lHeight, micrLine.m_CheckNumber);
									}
									ipInfo = ipMicrLine->Amount;
									if (ipInfo != __nullptr && ipInfo->IsRead == VARIANT_TRUE)
									{
										// Set the has amount line to true based on buildMicrZone results
										micrLine.m_bHasAmount = buildMicrZone(ipInfo, mapIt->first,
											lWidth, lHeight, micrLine.m_Amount);
									}

									// Add the micr line to the vector of lines
									vecLines.push_back(micrLine);
								} // End for each micr line
							} // End if lCount > 0
							_lastCodePos = "110_E_" + strCount + "_30";

							// Close the open image
							ICiImagePtr ipImage = ipReader->Image;
							ASSERT_RESOURCE_ALLOCATION("ELI25236", ipImage);
							if (ipImage->IsValid == ciTrue)
							{
								ipImage->Close();
							}
							_lastCodePos = "110_E_" + strCount + "_40";
						}
						CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25430");
					}
					catch(UCLIDException& uex)
					{
						// Add debug info
						uex.addDebugInfo("Image File", strImageName);
						uex.addDebugInfo("Rotation", mapIt->first);
						uex.addDebugInfo("Page Number", *it);
						throw uex;
					}
				} // End for each rotated image

				// Ensure each opened image is closed and clear the map
				for (map<int, ICiImagePtr>::iterator mapIt = mapRotationToImage.begin();
					mapIt != mapRotationToImage.end(); mapIt++)
				{
					ICiImagePtr ipImage = mapIt->second;
					if (ipImage != __nullptr && ipImage->IsValid == ciTrue)
					{
						ipImage->Close();
					}
				}
				mapRotationToImage.clear();
			}
			catch(...)
			{
				try
				{
					// Ensure the opened images are closed
					for (map<int, ICiImagePtr>::iterator mapIt = mapRotationToImage.begin();
						mapIt != mapRotationToImage.end(); mapIt++)
					{
						ICiImagePtr ipImage = mapIt->second;
						if (ipImage != __nullptr && ipImage->IsValid == ciTrue)
						{
							ipImage->Close();
						}
					}
				}
				CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25431");

				throw;
			}
		} // End for each specified page in image
		_lastCodePos = "120";

		IIUnknownVectorPtr ipNewAttributes =
			buildAttributesFromPages(mapMicrPages, strImageName, ipSpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI25032", ipNewAttributes != __nullptr);

		// Append the new attributes
		ipAttributes->Append(ipNewAttributes);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24422");
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CMicrFinder::buildAttribute(const RECT& recZone, ILongToObjectMapPtr ipMap,
										long lPage, string strMicrText,
										const string& strSourceImage,
										const string& strAttributeName)
{
	INIT_EXCEPTION_AND_TRACING("MLI02314");
	try
	{
		// Check arguments
		ASSERT_ARGUMENT("ELI24424", ipMap != __nullptr);

		// Create a new rectangle
		ILongRectanglePtr ipRect(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI24426", ipRect != __nullptr);
		ipRect->SetBounds(recZone.left, recZone.top, recZone.right, recZone.bottom);
		_lastCodePos = "10";

		// Create a new raster zone
		IRasterZonePtr ipRasterZone(CLSID_RasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI24427", ipRasterZone != __nullptr);
		ipRasterZone->CreateFromLongRectangle(ipRect, lPage);
		_lastCodePos = "20";

		// Replace all Inlite unrecognized characters ('?') with
		// Extract unrecognized characters ('^')
		replaceWord(strMicrText, "?", "^", false, false);
		_lastCodePos = "50";

		// Create a spatial string
		ISpatialStringPtr ipSS(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI24431", ipSS != __nullptr);
		ipSS->CreatePseudoSpatialString(ipRasterZone, strMicrText.c_str(),
			strSourceImage.c_str(), ipMap);
		_lastCodePos = "60";

		// Create an attribute
		IAttributePtr ipAttribute(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI24432", ipAttribute != __nullptr);
		ipAttribute->Value = ipSS;
		_lastCodePos = "70";
		
		// Set the name if specified
		if (strAttributeName != "")
		{
			ipAttribute->Name = get_bstr_t(strAttributeName);
		}
		_lastCodePos = "80";

		// Return the attribute
		return ipAttribute;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24433");
}
//-------------------------------------------------------------------------------------------------
bool CMicrFinder::buildMicrZone(_CcMicrInfoPtr ipMicrInfo, int nRotation,
									long lWidth, long lHeight, MicrZone& rMicrZone)
{
	try
	{
		ASSERT_ARGUMENT("ELI25033", ipMicrInfo != __nullptr);

		// Get the text and if empty return false [FlexIDSCore #3502]
		string strText = asString(ipMicrInfo->TextRaw);
		if (strText.empty())
		{
			return false;
		}

		// Build the rectangle and return false if height or width are 0
		RECT rect;
		rect.left = ipMicrInfo->Left;
		rect.top = ipMicrInfo->Top;
		rect.right = ipMicrInfo->Right;
		rect.bottom = ipMicrInfo->Bottom;
		if (rect.bottom - rect.top == 0 || rect.right - rect.left == 0)
		{
			return false;
		}

		rMicrZone.m_rectZone = rect;
		rMicrZone.m_strMicrText = strText;
		rMicrZone.m_nRotation = nRotation;

		// Rotate the zone based on the rotation value
		rotateMicrZone(rMicrZone, nRotation, lWidth, lHeight);

		// Return the MICR zone
		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25034");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CMicrFinder::buildAttributesFromPages(const map<long, MicrPage>& mapPages,
											const string& strImageName, ISpatialStringPtr ipSS)
{
	try
	{
		// Ensure the spatial string is not null
		ASSERT_ARGUMENT("ELI25035", ipSS != __nullptr);

		IIUnknownVectorPtr ipNewAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI25036", ipNewAttributes != __nullptr);

		for(map<long, MicrPage>::const_iterator mapIt = mapPages.begin();
			mapIt != mapPages.end(); mapIt++)
		{
			MicrPage micrPage = mapIt->second;

			// Set up the spatial page info
			ISpatialPageInfoPtr ipPageInfo(CLSID_SpatialPageInfo);
			ASSERT_RESOURCE_ALLOCATION("ELI25037", ipPageInfo != __nullptr);
			ipPageInfo->Deskew = 0.0;
			ipPageInfo->Width = micrPage.m_lWidth;
			ipPageInfo->Height = micrPage.m_lHeight;
			ipPageInfo->Orientation = kRotNone;

			// Create a new Spatial Page Info map
			ILongToObjectMapPtr ipMap(CLSID_LongToObjectMap);
			ASSERT_RESOURCE_ALLOCATION("ELI25038", ipMap != __nullptr);
			ipMap->Set(mapIt->first, ipPageInfo);

			// Loop through each of the MICR lines for the page
			vector<MicrLine>& vecLines = micrPage.m_vecMicrLines;
			long lCount = vecLines.size();

			// Go through the vector and remove extra overlapping elements
			for (long i=0; i < lCount; i++)
			{
				MicrZone tempZone1 = vecLines[i].m_Info;
				CRect rectFirst(tempZone1.m_rectZone);
				int iAreaFirst = rectFirst.Width() * rectFirst.Height();
				for (long j=i+1; j < lCount; j++)
				{
					MicrZone tempZone2 = vecLines[j].m_Info;
					CRect rectSecond(tempZone2.m_rectZone);
					CRect rectInterSection;
					if (rectInterSection.IntersectRect(rectFirst, rectSecond) != FALSE)
					{
						// Look for overlap
						double dOverlap =
							(double)(rectInterSection.Width() * rectInterSection.Height());
						dOverlap /= 
							(double)(min(iAreaFirst, rectSecond.Width() * rectSecond.Height()));
						if (dOverlap >= 0.90)
						{
							// Get the index of the item with the least number of subattributes
							int iIndexToRemove =
								vecLines[i].countSubAttributes() > vecLines[j].countSubAttributes()
								? j : i;

							// Remove the element with the least number of subattributes
							vecLines.erase(vecLines.begin() + iIndexToRemove);

							// Decrement i and the count, break from the loop and continue the search
							lCount--;
							i--;
							break;
						}
					}
				}
			}

			// For each MICR line, build an attribute and associated sub attributes
			for(long i=0; i < lCount; i++)
			{
				// Get the MICR line from the vector
				MicrLine& micrLine = vecLines[i];

				// Create a new main attribute if there was a main MICR line found
				if (micrLine.m_bHasInfo)
				{
					// Get the MICR zone and build the attribute from it
					MicrZone& micrZone = micrLine.m_Info;
					IAttributePtr ipMain = buildAttribute(micrZone.m_rectZone, ipMap, mapIt->first,
						micrZone.m_strMicrText, strImageName, "MICR");
					ASSERT_RESOURCE_ALLOCATION("ELI25039", ipMain);
					IIUnknownVectorPtr ipSubAttributes = ipMain->SubAttributes;
					ASSERT_RESOURCE_ALLOCATION("ELI25040", ipSubAttributes != __nullptr);

					// Now check for sub attributes
					if (m_bSplitRoutingNumber && micrLine.m_bHasRouting)
					{
						MicrZone& routingZone = micrLine.m_Routing;
						IAttributePtr ipRouting = buildAttribute(routingZone.m_rectZone, ipMap,
							mapIt->first, routingZone.m_strMicrText, strImageName, "Routing");
						ASSERT_RESOURCE_ALLOCATION("ELI25041", ipRouting != __nullptr);
						ipSubAttributes->PushBack(ipRouting);

						// ----------------------------------
						// Look for the other routing number
						// ----------------------------------

						ISpatialStringPtr ipSSTemp =
							ipSS->GetSpecifiedPages(mapIt->first, mapIt->first);

						// Ensure the substring is spatial
						if (ipSSTemp != __nullptr && ipSSTemp->HasSpatialInfo() == VARIANT_TRUE)
						{
							// Create a bounding rectangle to search for the other routing number
							RECT rectArea = getOtherRoutingSearchZone(vecLines, micrZone, micrPage);
							IAttributePtr ipOtherRouting = findOtherRoutingNumber(ipSSTemp,
								routingZone.m_strMicrText, rectArea);
							if (ipOtherRouting != __nullptr)
							{
								ipSubAttributes->PushBack(ipOtherRouting);
							}
						}
					}
					if (m_bSplitAccountNumber && micrLine.m_bHasAccount)
					{
						MicrZone& micrAccount = micrLine.m_Account;
						IAttributePtr ipAccount = buildAttribute(micrAccount.m_rectZone, ipMap,
							mapIt->first, micrAccount.m_strMicrText, strImageName, "Account");
						ASSERT_RESOURCE_ALLOCATION("ELI25042", ipAccount != __nullptr);
						ipSubAttributes->PushBack(ipAccount);
					}
					if(m_bSplitCheckNumber && micrLine.m_bHasCheckNumber)
					{
						MicrZone& micrCheck = micrLine.m_CheckNumber;
						IAttributePtr ipCheck = buildAttribute(micrCheck.m_rectZone, ipMap,
							mapIt->first, micrCheck.m_strMicrText, strImageName, "CheckNumber");
						ASSERT_RESOURCE_ALLOCATION("ELI25043", ipCheck != __nullptr);
						ipSubAttributes->PushBack(ipCheck);
					}
					if (m_bSplitAmount && micrLine.m_bHasAmount)
					{
						MicrZone& micrAmount = micrLine.m_Amount;
						IAttributePtr ipAmount = buildAttribute(micrAmount.m_rectZone, ipMap,
							mapIt->first, micrAmount.m_strMicrText, strImageName, "Amount");
						ASSERT_RESOURCE_ALLOCATION("ELI25044", ipAmount != __nullptr);
						ipSubAttributes->PushBack(ipAmount);
					}

					ipNewAttributes->PushBack(ipMain);
				}
			}
		}

		return ipNewAttributes;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25045");
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CMicrFinder::findOtherRoutingNumber(ISpatialStringPtr ipSpatialString,
												  const string &strRoutingNumber,
												  const RECT& rectToSearch)
{
	ASSERT_ARGUMENT("ELI24944", ipSpatialString != __nullptr);

	try
	{
		// The attribute that will be returned
		IAttributePtr ipNewAttribute = __nullptr;

		// Check for 8 digit routing number
		if (strRoutingNumber.length() != 8)
		{
			return ipNewAttribute;
		}

		// Get the pieces to search for
		string strFirstPiece = strRoutingNumber.substr(4, 4);
		string strSecondPiece = strRoutingNumber.substr(0, 4);

		// Need to trim leading 0's from the first piece
		strFirstPiece = trim(strFirstPiece, "0", "");

		// Trim only the first zero (if it is there) for
		// the second piece
		if ((strSecondPiece.length() > 1) && strSecondPiece[0] == '0')
		{
			strSecondPiece = strSecondPiece.substr(1, 3);
		}

		// Create a long rectangle based on the rectangle to search
		ILongRectanglePtr ipRect(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI24945", ipRect != __nullptr);
		ipRect->SetBounds(rectToSearch.left, rectToSearch.top, rectToSearch.right,
			rectToSearch.bottom);

		// Create a spatial string searcher and initialize it
		ISpatialStringSearcherPtr ipSSSearcher(CLSID_SpatialStringSearcher);
		ASSERT_RESOURCE_ALLOCATION("ELI24946", ipSSSearcher != __nullptr);
		ipSSSearcher->InitSpatialStringSearcher(ipSpatialString);
		ipSSSearcher->SetIncludeDataOnBoundary(VARIANT_TRUE);

		// Get a substring from the spatial string searcher (do not rotate the rectangle)
		ISpatialStringPtr ipSS = ipSSSearcher->GetDataInRegion(ipRect, VARIANT_FALSE);

		// Look for the other routing number if:
		// 1. The spatial string is not null
		// 2. The spatial string is not empty
		// 3. The spatial string has spatial info
		if (ipSS != __nullptr
			&& ipSS->IsEmpty() != VARIANT_TRUE
			&& ipSS->HasSpatialInfo() == VARIANT_TRUE)
		{
			// Locate the first and second pieces in the string (looking for exact match)
			long lFirstPiece = ipSS->FindFirstInstanceOfString(_bstr_t(strFirstPiece.c_str()), 0);
			long lSecondPiece = ipSS->FindFirstInstanceOfString(_bstr_t(strSecondPiece.c_str()), 0);
			long lStart(-1), lEnd(-1);

			if (lFirstPiece != -1 && lSecondPiece != -1)
			{
				lStart = lFirstPiece;
				lEnd = lSecondPiece + 4;
			}
			// No exact match
			else
			{
				// Look for the other routing number
				IIUnknownVectorPtr ipMatches =
					getOtherRoutingNumberRegexParser()->Find(ipSS->String,
					VARIANT_TRUE, VARIANT_TRUE);
				ASSERT_RESOURCE_ALLOCATION("ELI24949", ipMatches != __nullptr);

				if (ipMatches->Size() >= 1)
				{
					// Get the match object
					IObjectPairPtr ipObject(ipMatches->At(0));
					ASSERT_RESOURCE_ALLOCATION("ELI24950", ipObject != __nullptr);
					ITokenPtr ipToken(ipObject->Object1);
					ASSERT_RESOURCE_ALLOCATION("ELI24951", ipToken != __nullptr);

					// Get the start and end position
					lStart = ipToken->StartPosition;
					lEnd = ipToken->EndPosition;
				}
			}
				
			// If a start and end were computed, attempt to get a sub string
			ISpatialStringPtr ipSubString = __nullptr;
			if (lStart != -1 && lEnd != -1)
			{
				try
				{
					// Attempt to get a substring based on the start and end position
					ipSubString = ipSS->GetSubString(lStart, lEnd);
				}
				catch(...)
				{
					// Just ignore exceptions
				}
			}

			if (ipSubString != __nullptr)
			{
				// Trim white space from the beginning and end of the string
				ipSubString->Trim(" \r\n", " \r\n");
				ipNewAttribute.CreateInstance(CLSID_Attribute);
				ASSERT_RESOURCE_ALLOCATION("ELI24952", ipNewAttribute != __nullptr);

				ipNewAttribute->Value = ipSubString;
				ipNewAttribute->Name = "OtherRouting";
			}
		}

		// Return the new attribute
		return ipNewAttribute;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24953");
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CMicrFinder::getOtherRoutingNumberRegexParser()
{
	try
	{
		string strOtherRoutingRegex = "";

		// Get the name for the regex file
		string strRoutingRegexFile = getModuleDirectory(_Module.m_hInst) + "\\" + gstrREGEX_FILE;

		// Check if the file exists
		if (isValidFile(strRoutingRegexFile))
		{
			// [FlexIDSCore:3643] Load the regular expression from disk if necessary.
			m_cachedRegExLoader.loadObjectFromFile(strRoutingRegexFile);

			// Retrieve the pattern
			strOtherRoutingRegex = (string)m_cachedRegExLoader.m_obj;
		}
		else
		{
			strOtherRoutingRegex = gstrOTHER_ROUTING_REGEX;
		}

		// Get a regular expression parser
		IRegularExprParserPtr ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("");
		ASSERT_RESOURCE_ALLOCATION("ELI24948", ipParser != __nullptr );

		// Load the regex pattern into the parser
		ipParser->Pattern = strOtherRoutingRegex.c_str();

		// Return the parser
		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24954");
}
//-------------------------------------------------------------------------------------------------
void CMicrFinder::resetSettings()
{
	m_bSplitAccountNumber = true;
	m_bSplitRoutingNumber = true;
	m_bSplitCheckNumber = false;
	m_bSplitAmount = false;

	m_mapRotations[0] = true;
	m_mapRotations[90] = true;
	m_mapRotations[180] = true;
	m_mapRotations[270] = true;
}
//-------------------------------------------------------------------------------------------------
EMicrReaderFlags CMicrFinder::getMicrReaderFlags()
{
	static long lMicrFlags = -1;

	try
	{
		if (lMicrFlags == -1)
		{
			RegistryPersistenceMgr regMgr(HKEY_CURRENT_USER, gstrMICR_FINDER_KEY);

			if (regMgr.keyExists("", gstrMICR_FINDER_FLAGS_KEY))
			{
				lMicrFlags = asLong(regMgr.getKeyValue("", gstrMICR_FINDER_FLAGS_KEY));
			}
			else
			{
				regMgr.createKey("", gstrMICR_FINDER_FLAGS_KEY, asString(giMICR_FINDER_FLAGS_DEFAULT));
				lMicrFlags = giMICR_FINDER_FLAGS_DEFAULT;
			}
		}

		return (EMicrReaderFlags) lMicrFlags;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25358");
}
//-------------------------------------------------------------------------------------------------
void CMicrFinder::validateLicense()
{
	VALIDATE_LICENSE( gnMICR_FINDING_ENGINE_FEATURE, "ELI24366", "MICR finder" );
}
//-------------------------------------------------------------------------------------------------