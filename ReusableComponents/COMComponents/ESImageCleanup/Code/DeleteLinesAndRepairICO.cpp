// DeleteLinesAndRepairICO.cpp : Implementation of CDeleteLinesAndRepairICO

#include "stdafx.h"
#include "DeleteLinesAndRepairICO.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

// default line angle value
const double gdMAX_LINE_ANGLE = 5.0;

// default line curvature value - ci values are ClearImage enums
const ELineCurvature guiMAX_LINE_CURVATURE = ciCurvLow;

//-------------------------------------------------------------------------------------------------
// CDeleteLinesAndRepairICO
//-------------------------------------------------------------------------------------------------
CDeleteLinesAndRepairICO::CDeleteLinesAndRepairICO() :
m_lLineLength(-1),
m_lLineGap(-1),
m_ulLineDirection(ciLineUnknown),
m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CDeleteLinesAndRepairICO::~CDeleteLinesAndRepairICO()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17654");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDeleteLinesAndRepairICO,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_IImageCleanupOperation,
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
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17655", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("Delete lines and repair").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17656")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ESImageCleanupLib::IDeleteLinesAndRepairICOPtr ipCopyThis(pObject);	
		ASSERT_RESOURCE_ALLOCATION("ELI17657", ipCopyThis != __nullptr);

		// copy each of the data members
		m_lLineLength = ipCopyThis->LineLength;
		m_lLineGap = ipCopyThis->LineGap;
		m_ulLineDirection = ipCopyThis->LineDirection;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17658");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::raw_Clone(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17659", pObject != __nullptr);

		// create a copyable object pointer
		ICopyableObjectPtr ipObjCopy(CLSID_DeleteLinesAndRepairICO);
		ASSERT_RESOURCE_ALLOCATION("ELI17660", ipObjCopy != __nullptr);

		// set the IUnknownPtr to the current object
		IUnknownPtr ipUnk = this;
		ASSERT_RESOURCE_ALLOCATION("ELI17661", ipUnk != __nullptr);

		// copy to the copyable object pointer
		ipObjCopy->CopyFrom(ipUnk);

		// return the new DeleteLinesAndRepairICO object 
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17662");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI17663", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17686");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17664", pClassID != __nullptr);

		*pClassID = CLSID_DeleteLinesAndRepairICO;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17696");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17665", pStream != __nullptr);

		// reset member variables to unconfigured state
		m_lLineLength = -1;
		m_lLineGap = -1;
		m_ulLineDirection = ciLineUnknown;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// read the version number from the stream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check the version number
		if (nDataVersion > gnCurrentVersion)
		{
			UCLIDException ue("ELI17666", "Unable to load newer DeleteLinesAndRepairICO.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// read the member variables from the stream
		dataReader >> m_lLineLength;
		dataReader >> m_lLineGap;
		dataReader >> m_ulLineDirection;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17667");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17668", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// write the version number to the stream
		dataWriter << gnCurrentVersion;

		// write the member variables to the stream
		dataWriter << m_lLineLength;
		dataWriter << m_lLineGap;
		dataWriter << m_ulLineDirection;

		// flush the stream
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (asCppBool(fClearDirty))
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17669");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// item is configured if LineLength and LineGap are >= 0 and the
		// direction is configured
		bool bConfigured = (m_lLineLength >= 0 && m_lLineGap >= 0 && isDirectionConfigured());

		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12167");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// IImageCleanupOperation Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::Perform(void* pciRepair)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License first
		validateLicense();

		// wrap the ClearImage repair pointer in smart pointer
		ICiRepairPtr ipciRepair((ICiRepair*) pciRepair);
		ASSERT_RESOURCE_ALLOCATION("ELI17670", ipciRepair != __nullptr);

		// set the default settings
		ipciRepair->pMaxLineAngle = gdMAX_LINE_ANGLE;
		ipciRepair->pLineCurvature = guiMAX_LINE_CURVATURE;

		// load the user configured settings into the repair object
		ipciRepair->pMinLineLength = m_lLineLength;
		ipciRepair->pMaxLineGap = m_lLineGap;

		// perform the ClearImage DeleteLines method with repair set to ciTrue
		ipciRepair->DeleteLines((ELineDirection)m_ulLineDirection, ciTrue);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17671");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDeleteLinesAndRepairICO
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::get_LineLength(long* plLineLength)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17672", plLineLength != __nullptr);

		*plLineLength = m_lLineLength;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17673");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::put_LineLength(long lLineLength)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (lLineLength < 0)
		{
			UCLIDException ue("ELI17674", "Line length must be a positive value!");
			ue.addDebugInfo("Line length", lLineLength);
			throw ue;
		}

		m_lLineLength = lLineLength;

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17675");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::get_LineGap(long* plLineGap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17676", plLineGap != __nullptr);

		*plLineGap = m_lLineGap;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17677");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::put_LineGap(long lLineGap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (lLineGap < 0)
		{
			UCLIDException ue("ELI17678", "Line gap must be a positive value!");
			ue.addDebugInfo("Line gap", lLineGap);
			throw ue;
		}

		m_lLineGap = lLineGap;

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17679");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::get_LineDirection(unsigned long *pulLineDirection)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17680", pulLineDirection != __nullptr);

		*pulLineDirection = m_ulLineDirection;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17681");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDeleteLinesAndRepairICO::put_LineDirection(unsigned long ulLineDirection)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check to be sure the direction is a recognized direction.
		// if it is, set direction to the new direction.
		switch(ulLineDirection)
		{
		case ciLineUnknown:
		case ciLineVert:
		case ciLineHorz:
		case ciLineVertAndHorz:
			m_ulLineDirection = ulLineDirection;

			// set the dirty flag
			m_bDirty = true;
			break;

		default:
			{
				UCLIDException ue("ELI17682", "Unrecognized line direction!");
				ue.addDebugInfo("Direction", ulLineDirection);
				throw ue;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17683");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CDeleteLinesAndRepairICO::validateLicense()
{
	VALIDATE_LICENSE( gnIMAGE_CLEANUP_ENGINE_FEATURE, "ELI17684", "DeleteLinesAndRepair ICO" );
}
//-------------------------------------------------------------------------------------------------
bool CDeleteLinesAndRepairICO::isDirectionConfigured()
{
	// default to not configured
	bool bDirectionIsConfigured = false;

	// if direction is set then set to configured
	switch(m_ulLineDirection)
	{
	case ciLineVert:
	case ciLineHorz:
	case ciLineVertAndHorz:
		bDirectionIsConfigured = true;
		break;
	}

	return bDirectionIsConfigured;
}
//-------------------------------------------------------------------------------------------------