// CheckFinder.cpp : Implementation of CCheckFinder
#include "stdafx.h"
#include "CheckFinder.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <InliteNamedMutexConstants.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <TemporaryFileName.h>
#include <MiscLeadUtils.h>

#include <vector>
#include <utility>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const unsigned long gulCURRENT_VERSION = 1;

//-------------------------------------------------------------------------------------------------
// CCheckFinder
//-------------------------------------------------------------------------------------------------
CCheckFinder::CCheckFinder() 
:	m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CCheckFinder::~CCheckFinder()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24376");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_ICheckFinder,
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24392");
}

//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::raw_ParseText(IAFDocument * pAFDoc, IProgressStatus *pProgressStatus,
										   IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI24499", ipAFDoc != __nullptr);

		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI24377", ipAttributes != __nullptr);

		// Get the spatial string
		ISpatialStringPtr ipSS = ipAFDoc->Text;

		// Check that there is a spatial string and it has spatial info
		if (ipSS != __nullptr && ipSS->HasSpatialInfo() == VARIANT_TRUE)
		{
			// Get the pages from the spatial string [FlexIDSCore #3523]
			IIUnknownVectorPtr ipPages = ipSS->GetPages();
			ASSERT_RESOURCE_ALLOCATION("ELI25602", ipPages != __nullptr);

			// Build a vector of pages to process
			long lSize = ipPages->Size();
			vector<long> vecPages;
			for (long i=0; i < lSize; i++)
			{
				// Get the page
				ISpatialStringPtr ipPage = ipPages->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI25603", ipPage != __nullptr);

				// Get the page number for the page
				long lPageNum = ipPage->GetFirstPageNumber();

				// Add the page number to the vector
				vecPages.push_back(lPageNum);
			}

			findChecks(asString(ipSS->SourceDocName), vecPages, ipAttributes);
		}
				
		*pAttributes = ipAttributes.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24378");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24379", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Check finder").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24380")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::raw_CopyFrom(IUnknown * pObject)
{
	try
	{
		// validate license first
		validateLicense();

		// Nothing to copy
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24381");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI24382", pObject != __nullptr);

		ICopyableObjectPtr ipObjCopy(CLSID_CheckFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI24383", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new micr finder to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24384");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_CheckFinder;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::Load(IStream * pStream)
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

		if (nDataVersion > gulCURRENT_VERSION)
		{
			UCLIDException uex("ELI24385", "Unable to load newer Check finder!");
			uex.addDebugInfo("Current Version", gulCURRENT_VERSION);
			uex.addDebugInfo("Version To Load", nDataVersion);
			throw uex;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24386");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::Save(IStream * pStream, BOOL fClearDirty)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24387");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	if (pcbSize == NULL)
		return E_POINTER;
		
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCheckFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI24388", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24389");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CCheckFinder::findChecks(const string& strImageName, const vector<long>& vecPages,
							  IIUnknownVectorPtr ipAttributes)
{
	INIT_EXCEPTION_AND_TRACING("MLI02361");
	try
	{
		// Ensure the image exists
		ASSERT_ARGUMENT("ELI24502", isValidFile(strImageName));
		ASSERT_ARGUMENT("ELI24503", ipAttributes != __nullptr);

		// Handle PDF's (cannot load PDF directly into Inlite engine without PDF license)
		string strWorkingFile = strImageName;
		unique_ptr<TemporaryFileName> pTempFile(__nullptr);
		if (isPDF(strImageName))
		{
			pTempFile.reset(new TemporaryFileName(__nullptr, ".tif"));
			strWorkingFile = pTempFile->getName();
			convertPDFToTIF(strImageName, strWorkingFile);
		}

		_bstr_t bstrImageName = _bstr_t(strWorkingFile.c_str());
		_lastCodePos = "10";

		// Create a map to store the check image bounds and page dimensions for found checks
		map<long, pair<RECT, CheckExtractionData>> mapPageChecks;

		for(vector<long>::const_iterator it = vecPages.begin(); it != vecPages.end(); it++)
		{
			string strCount = asString(*it);
			_lastCodePos = "20_A_" + strCount;

			// Lock over MICR finder objects
			CSingleLock lock(&sg_mutexINLITE_MUTEX, TRUE);

			// Create the MICR reader (need to create a new one for each page)
			_CcMicrReaderPtr ipReader(CLSID_CcMicrReader);
			ASSERT_RESOURCE_ALLOCATION("ELI24504", ipReader != __nullptr);

			// Open the image
			ICiImagePtr ipImage = ipReader->Image;
			ASSERT_RESOURCE_ALLOCATION("ELI24505", ipImage != __nullptr);
			ipImage->Open(bstrImageName, *it);
			_lastCodePos = "20_B_" + strCount;

			// Get the image dimensions (need to do this before ExtractCheck call)
			CheckExtractionData checkData;
			checkData.width = ipImage->Width;
			checkData.height = ipImage->Height;
			_lastCodePos = "20_C_" + strCount;

			// Attempt to extract the check
			ICiImagePtr ipCheckImage = ipReader->ExtractCheck();
			_lastCodePos = "20_D_" + strCount;

			// Close the original image
			ipImage->Close();
			_lastCodePos = "20_E_" + strCount;

			// Check for extracted check
			if (ipCheckImage != __nullptr)
			{
				// Get the first MICR object
				_CcMicrPtr ipMicr = ipReader->GetMicrLine(1);
				ASSERT_RESOURCE_ALLOCATION("ELI24506", ipMicr != __nullptr);

				// Get the document object
				_CcMicrInfoPtr ipDocument = ipMicr->Document;
				ASSERT_RESOURCE_ALLOCATION("ELI24507", ipDocument != __nullptr);

				checkData.skew = ipDocument->Skew;
				_lastCodePos = "20_E_15" + strCount;

				// Build a RECT with the coordinates of the extracted document
				RECT rect;
				rect.left = ipDocument->Left;
				rect.top = ipDocument->Top;
				rect.right = ipDocument->Right;
				rect.bottom = ipDocument->Bottom;
				_lastCodePos = "20_E_20_" + strCount;

				// Store the RECT and page dimensions
				mapPageChecks[*it] = pair<RECT, CheckExtractionData>(rect, checkData);
				_lastCodePos = "20_E_30_" + strCount;

				// Close the sub image
				if (ipCheckImage->IsValid == ciTrue)
				{
					ipCheckImage->Close();
				}
				_lastCodePos = "20_E_40_" + strCount;
			}
			_lastCodePos = "30";
		}

		for(map<long, pair<RECT, CheckExtractionData>>::iterator it = mapPageChecks.begin();
			it != mapPageChecks.end(); it++)
		{
			string strCount = asString(it->first);
			_lastCodePos = "30_A_" + strCount;

			// Get the RECT and ImageData from the iterator
			RECT rect = it->second.first;
			CheckExtractionData checkData = it->second.second;
			_lastCodePos = "30_B_" + strCount;

			// Build the new attribute
			IAttributePtr ipNewAttribute = buildAttribute(strImageName, it->first,
				checkData, rect, "Check");
			ASSERT_RESOURCE_ALLOCATION("ELI24508", ipNewAttribute != __nullptr);

			// Add the new attribute to the vector of attributes
			ipAttributes->PushBack(ipNewAttribute);
			_lastCodePos = "30_C_" + strCount;
		}
		_lastCodePos = "40";
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24509");
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CCheckFinder::buildAttribute(const string& strImageName, long lPage, 
										   const CheckExtractionData& checkData, const RECT& rect,
										   const string& strAttributeName)
{
	INIT_EXCEPTION_AND_TRACING("MLI02362");
	try
	{
		// Create a spatial page info
		ISpatialPageInfoPtr ipInfo(CLSID_SpatialPageInfo);
		ASSERT_RESOURCE_ALLOCATION("ELI24510", ipInfo != __nullptr);
		ipInfo->Width = checkData.width;
		ipInfo->Height = checkData.height;
		ipInfo->Deskew = 0.0;
		ipInfo->Orientation = kRotNone;
		_lastCodePos = "10";

		// Create a spatial page info map
		ILongToObjectMapPtr ipMap(CLSID_LongToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI24511", ipMap != __nullptr);
		ipMap->Set(lPage, ipInfo);
		_lastCodePos = "20";

		// Create a new rectangle
		ILongRectanglePtr ipRect(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI24512", ipRect != __nullptr);
		ipRect->SetBounds(rect.left, rect.top, rect.right, rect.bottom);
		_lastCodePos = "30";

		// Create a new raster zone
		IRasterZonePtr ipRasterZone(CLSID_RasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI24513", ipRasterZone != __nullptr);
		ipRasterZone->CreateFromLongRectangle(ipRect, lPage);
		_lastCodePos = "40";

		// If there was a skew, apply it
		if (checkData.skew != 0.0)
		{
			ipRasterZone->RotateBy(checkData.skew);
		}
		_lastCodePos = "45";

		// Create a new IUnknownVector
		IIUnknownVectorPtr ipZones(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI24514", ipZones != __nullptr);
		ipZones->PushBack(ipRasterZone);
		_lastCodePos = "50";

		// Create a new hybrid spatial string
		ISpatialStringPtr ipSS(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI24515", ipSS != __nullptr);
		ipSS->CreateHybridString(ipZones, "Check image", strImageName.c_str(), ipMap);
		_lastCodePos = "60";

		// Create a new attribute
		IAttributePtr ipAttribute(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI24516", ipAttribute != __nullptr);
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24517");
}
//-------------------------------------------------------------------------------------------------
void CCheckFinder::validateLicense()
{
	VALIDATE_LICENSE( gnMICR_FINDING_ENGINE_FEATURE, "ELI24390", "Check finder" );
}
//-------------------------------------------------------------------------------------------------
