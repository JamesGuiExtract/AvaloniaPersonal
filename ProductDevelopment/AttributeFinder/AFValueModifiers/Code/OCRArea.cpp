// OCRArea.cpp : Implementation of COCRArea
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "OCRArea.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ByteStream.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

// default filter for new OCRArea objects (all the filter options except kCustomFilter)
const EFilterCharacters geDEFAULT_FILTER = EFilterCharacters(kAlphaFilter | kNumeralFilter | 
	kPeriodFilter | kHyphenFilter |	kUnderscoreFilter |	kCommaFilter | kForwardSlashFilter);

//-------------------------------------------------------------------------------------------------
// COCRArea
//-------------------------------------------------------------------------------------------------
COCRArea::COCRArea()
  : m_bDirty(false),
    m_eFilter(geDEFAULT_FILTER),
	m_strCustomFilterCharacters(""),
	m_bDetectHandwriting(false),
	m_bReturnUnrecognized(false), 
	m_bClearIfNoneFound(true)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI18454");
}
//-------------------------------------------------------------------------------------------------
COCRArea::~COCRArea()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18455");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::InterfaceSupportsErrorInfo(REFIID riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		static const IID* arr[] = 
		{
			&IID_IOCRArea,
			&IID_IAttributeModifyingRule,
			&IID_ICategorizedComponent,
			&IID_ICopyableObject,
			&IID_ILicensedComponent,
			&IID_IMustBeConfiguredObject
		};

		for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
			{
				return S_OK;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18527");

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IOCRArea
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::SetOptions(EFilterCharacters eFilter, BSTR bstrCustomFilterCharacters, 
		VARIANT_BOOL vbDetectHandwriting, VARIANT_BOOL vbReturnUnrecognized, 
		VARIANT_BOOL vbClearIfNoneFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license
		validateLicense();

		// store options
		m_eFilter = eFilter;
		m_strCustomFilterCharacters = asString(bstrCustomFilterCharacters);
		m_bDetectHandwriting = asCppBool(vbDetectHandwriting);
		m_bReturnUnrecognized = asCppBool(vbReturnUnrecognized);
		m_bClearIfNoneFound = asCppBool(vbClearIfNoneFound);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18456");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::GetOptions(EFilterCharacters* peFilter, BSTR* pbstrCustomFilterCharacters, 
		VARIANT_BOOL* pvbDetectHandwriting, VARIANT_BOOL* pvbReturnUnrecognized, 
		VARIANT_BOOL* pvbClearIfNoneFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license
		validateLicense();

		// ensure parameters are non-NULL
		ASSERT_ARGUMENT("ELI18457", peFilter != NULL);
		ASSERT_ARGUMENT("ELI18458", pbstrCustomFilterCharacters != NULL);
		ASSERT_ARGUMENT("ELI18459", pvbDetectHandwriting != NULL);
		ASSERT_ARGUMENT("ELI18460", pvbReturnUnrecognized != NULL);
		ASSERT_ARGUMENT("ELI18495", pvbClearIfNoneFound != NULL);

		// set options
		*peFilter = m_eFilter;
		*pbstrCustomFilterCharacters = _bstr_t(m_strCustomFilterCharacters.c_str()).Detach();
		*pvbDetectHandwriting = asVariantBool(m_bDetectHandwriting);
		*pvbReturnUnrecognized = asVariantBool(m_bReturnUnrecognized);
		*pvbClearIfNoneFound = asVariantBool(m_bClearIfNoneFound);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18492");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput, 
													 IProgressStatus* pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check licensing
		validateLicense();
		if(m_bDetectHandwriting)
		{
			validateHandwritingLicense();
		}

		// get the attribute
		IAttributePtr ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI18461", ipAttribute != NULL);

		// get the attribute's spatial string value	
		ISpatialStringPtr ipSpatialString(ipAttribute->Value);
		ASSERT_RESOURCE_ALLOCATION("ELI18493", ipSpatialString != NULL);

		// check if this attribute is non-spatial
		if(ipSpatialString->HasSpatialInfo() == VARIANT_FALSE)
		{
			// no text can be found in the area of a non-spatial string

			// clear the spatial string if necessary
			if(m_bClearIfNoneFound)
			{
				ipSpatialString->Clear();
			}

			// stop here
			return S_OK;
		}

		// get the source document name
		string strSourceDocName = asString(ipSpatialString->SourceDocName);

		// get the spatial page info map
		ILongToObjectMapPtr ipPageInfoMap(ipSpatialString->SpatialPageInfos);
		ASSERT_RESOURCE_ALLOCATION("ELI19866", ipPageInfoMap != NULL);

		// get the raster zones of this attribute
		IIUnknownVectorPtr ipZones( ipSpatialString->GetOriginalImageRasterZones() );
		ASSERT_RESOURCE_ALLOCATION("ELI19571", ipZones != NULL);

		// create a spatial string to hold the result
		ISpatialStringPtr ipResult(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI19572", ipResult != NULL);
		
		// set a flag to indicate ipResult is empty
		bool bResultIsEmpty = true;

		// instantiate a new OCR engine if there is at least one zone to OCR
		long lSize = ipZones->Size();
		IOCREnginePtr ipOCREngine(lSize > 0 ? getOCREngine() : NULL);

		map<int, ILongRectanglePtr> mapPageBounds;

		// iterate through each zone in the attribute
		for(long i=0; i<lSize; i++)
		{
			// get the ith raster zone
			IRasterZonePtr ipZone = ipZones->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI19573", ipZone != NULL);

			// Get the page number of this zone
			long nPageNumber = ipZone->PageNumber;

			// Get the page bounds (for use by GetRectangularBounds)
			if (mapPageBounds.find(nPageNumber) == mapPageBounds.end())
			{
				mapPageBounds[nPageNumber] = ipSpatialString->GetOriginalImagePageBounds(nPageNumber);
			}
			ASSERT_RESOURCE_ALLOCATION("ELI30333", mapPageBounds[nPageNumber] != NULL);

			// get the text inside this raster zone
			ISpatialStringPtr ipZoneText = ipOCREngine->RecognizeTextInImageZone(
				strSourceDocName.c_str(), nPageNumber, nPageNumber,
				ipZone->GetRectangularBounds(mapPageBounds[nPageNumber]), 0, m_eFilter, 
				m_strCustomFilterCharacters.c_str(), asVariantBool(m_bDetectHandwriting), 
				asVariantBool(m_bReturnUnrecognized), VARIANT_TRUE, pProgressStatus);
			ASSERT_RESOURCE_ALLOCATION("ELI19537", ipZoneText != NULL);
	
			// if any text was found, append it
			if(ipZoneText->IsEmpty() == VARIANT_FALSE)
			{
				// check if this is the first found text
				if(bResultIsEmpty)
				{
					bResultIsEmpty = false;
				}
				else
				{
					// the result already contains some found text.
					// insert a line break between the previous and current found text.
					ipResult->AppendString("\r\n");
				}

				// append the found text
				ipResult->Append(ipZoneText);
			}
		}

		// store the result if either:
		// (a) the value should be cleared when the result is empty
		// (b) the result is not empty
		if(m_bClearIfNoneFound || !bResultIsEmpty)
		{
			ipAttribute->Value = ipResult;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18463");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::raw_CopyFrom(IUnknown* pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// get the OCRArea interface
		UCLID_AFVALUEMODIFIERSLib::IOCRAreaPtr ipOCRArea(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI18464", ipOCRArea != NULL);

		// get the options from the OCRArea object
		EFilterCharacters eFilter; 
		_bstr_t bstrCustomFilterCharacters;
		VARIANT_BOOL vbDetectHandwriting, vbReturnUnrecognized, vbClearIfNoneFound;
		ipOCRArea->GetOptions(&eFilter, bstrCustomFilterCharacters.GetAddress(), &vbDetectHandwriting, 
			&vbReturnUnrecognized, &vbClearIfNoneFound);
		
		// store the found options
		m_eFilter = eFilter;
		m_strCustomFilterCharacters = asString(bstrCustomFilterCharacters);
		m_bDetectHandwriting = asCppBool(vbDetectHandwriting);
		m_bReturnUnrecognized = asCppBool(vbReturnUnrecognized);
		m_bClearIfNoneFound = asCppBool(vbClearIfNoneFound);

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18465");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::raw_Clone(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// ensure that the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI18466", pObject != NULL);

		// get the copyable object interface
		ICopyableObjectPtr ipObjCopy(CLSID_OCRArea);
		ASSERT_RESOURCE_ALLOCATION("ELI18467", ipObjCopy != NULL);

		// create a shallow copy
		IUnknownPtr ipUnknown(this);
		ASSERT_RESOURCE_ALLOCATION("ELI18534", ipUnknown != NULL);
		ipObjCopy->CopyFrom(ipUnknown);

		// return the new OCRArea to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18468");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::raw_GetComponentDescription(BSTR* pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18469", pstrComponentDescription);
		
		*pstrComponentDescription = _bstr_t("OCR image area").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18470");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI18471", pbValue != NULL);

		try
		{
			// check license
			validateLicense();

			// if no exception was thrown, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18472");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::raw_IsConfigured(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI18473", pbValue != NULL);

		// the OCRArea is configured if the custom filter option isn't set or
		// if it is set AND the CustomCharacters are not empty.
		*pbValue = (m_eFilter & kCustomFilter) == 0 || !m_strCustomFilterCharacters.empty();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18474");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::GetClassID(CLSID* pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18529", pClassID != NULL);

		*pClassID = CLSID_OCRArea;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18528");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 1:
//   filter options, custom filter characters, detect handwriting, return unrecognized characters,
//   and clear if no text found
STDMETHODIMP COCRArea::Load(IStream* pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// clear the options
		m_eFilter = geDEFAULT_FILTER;
		m_strCustomFilterCharacters.clear();
		m_bDetectHandwriting = false;
		m_bReturnUnrecognized = false;
		m_bClearIfNoneFound = true;
		
		// use a smart pointer for the IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI18475", ipStream != NULL);

		// read the bytestream data from the IStream object
		long nDataLength = 0;
		HANDLE_HRESULT(ipStream->Read(&nDataLength, sizeof(nDataLength), NULL), "ELI18530", 
			"Unable to read object size from stream.", ipStream, __uuidof(IStream));
		ByteStream data(nDataLength);
		HANDLE_HRESULT(ipStream->Read(data.getData(), nDataLength, NULL), "ELI18531", 
			"Unable to read object from stream.", ipStream, __uuidof(IStream));

		// read the data version
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check if file is a newer version than this object can use
		if (nDataVersion > gnCurrentVersion)
		{
			// throw exception
			UCLIDException ue("ELI18476", "Unable to load newer OCRArea.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// read data items
		unsigned long ulFilter;
		dataReader >> ulFilter;
		m_eFilter = (EFilterCharacters)ulFilter;
		dataReader >> m_strCustomFilterCharacters;
		dataReader >> m_bDetectHandwriting;
		dataReader >> m_bReturnUnrecognized;
		dataReader >> m_bClearIfNoneFound;
		
		// clear the dirty flag since a new object was loaded
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18477");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::Save(IStream* pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;
		dataWriter << (unsigned long) m_eFilter;
		dataWriter << m_strCustomFilterCharacters;
		dataWriter << m_bDetectHandwriting;
		dataWriter << m_bReturnUnrecognized;
		dataWriter << m_bClearIfNoneFound;
		dataWriter.flushToByteStream();

		// use a smart pointer for IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI18478", ipStream != NULL);

		// write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		HANDLE_HRESULT(ipStream->Write(&nDataLength, sizeof(nDataLength), NULL), "ELI18532", 
			"Unable to write object size to stream.", ipStream, __uuidof(IStream));
		HANDLE_HRESULT(ipStream->Write(data.getData(), nDataLength, NULL), "ELI18533", 
			"Unable to write object to stream.", ipStream, __uuidof(IStream));

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18479");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRArea::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
IOCREnginePtr COCRArea::getOCREngine()
{
	// instantiate a new OCR engine every time this function is called [P13 #2909]
	IOCREnginePtr ipOCREngine(CLSID_ScansoftOCR);
	ASSERT_RESOURCE_ALLOCATION("ELI18480", ipOCREngine != NULL);

	// license the engine
	IPrivateLicensedComponentPtr ipOCREngineLicense(ipOCREngine);
	ASSERT_RESOURCE_ALLOCATION("ELI18481", ipOCREngineLicense != NULL);
	ipOCREngineLicense->InitPrivateLicense(LICENSE_MGMT_PASSWORD.c_str());

	return ipOCREngine;
}
//-------------------------------------------------------------------------------------------------
void COCRArea::validateHandwritingLicense()
{
	VALIDATE_LICENSE(gnHANDWRITING_RECOGNITION_FEATURE, "ELI18560", "OCRArea");
}
//-------------------------------------------------------------------------------------------------
void COCRArea::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI18482", "OCRArea");
}
//-------------------------------------------------------------------------------------------------
