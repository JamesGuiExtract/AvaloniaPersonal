// SpatialPageInfo.cpp : Implementation of CSpatialPageInfo
#include "stdafx.h"
#include "UCLIDRasterAndOCRMgmt.h"
#include "SpatialPageInfo.h"
#include <UCLIDException.h>
#include <ByteStreamManipulator.h>
//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CSpatialPageInfo
//-------------------------------------------------------------------------------------------------
CSpatialPageInfo::CSpatialPageInfo()
{
}
//-------------------------------------------------------------------------------------------------
CSpatialPageInfo::~CSpatialPageInfo()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16538");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISpatialPageInfo,
		&IID_IPersistStream,
		&IID_ICopyableObject
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ISpatialPageInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::get_Deskew(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25744", pVal != NULL);

		*pVal = m_fDeskew;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25745");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::put_Deskew(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_fDeskew = newVal;
		m_bDirty = true;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25746");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::get_Orientation(EOrientation *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25747", pVal != NULL);

		*pVal = (EOrientation) m_eOrientation;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25748");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::put_Orientation(EOrientation newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_eOrientation = (UCLID_RASTERANDOCRMGMTLib::EOrientation) newVal;
		m_bDirty = true;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25749");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::get_Width(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25750", pVal != NULL);

		*pVal = m_nWidth;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25751");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::put_Width(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nWidth = newVal;
		m_bDirty = true;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25752");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::get_Height(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25753", pVal != NULL);

		*pVal = m_nHeight;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25754");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::put_Height(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nHeight = newVal;
		m_bDirty = true;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25755");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::SetPageInfo(long lWidth, long lHeight,
										   EOrientation eOrientation, double dDeskew)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Set the values
		m_nWidth = lWidth;
		m_nHeight = lHeight;
		m_eOrientation = (UCLID_RASTERANDOCRMGMTLib::EOrientation)eOrientation;
		m_fDeskew = dDeskew;

		// Set the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25756");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::GetWidthAndHeight(long* plWidth, long* plHeight)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25757", plWidth != NULL);
		ASSERT_ARGUMENT("ELI25758", plHeight != NULL);

		// Set the return values
		*plWidth = m_nWidth;
		*plHeight = m_nHeight;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25759");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::GetPageInfo(long* plWidth, long* plHeight,
										   EOrientation* peOrientation, double* pdDeskew)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check arguments
		ASSERT_ARGUMENT("ELI25760", plWidth != NULL);
		ASSERT_ARGUMENT("ELI25761", plHeight != NULL);
		ASSERT_ARGUMENT("ELI25762", peOrientation != NULL);
		ASSERT_ARGUMENT("ELI25763", pdDeskew != NULL);

		// Set the return values
		*plWidth = m_nWidth;
		*plHeight = m_nHeight;
		*peOrientation = (EOrientation) m_eOrientation;
		*pdDeskew = m_fDeskew;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25764");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::Equal(ISpatialPageInfo *pPageInfo, VARIANT_BOOL *pEqual)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check arguments
		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo(pPageInfo);
		ASSERT_ARGUMENT("ELI25765", ipPageInfo);
		ASSERT_ARGUMENT("ELI25766", pEqual != NULL);

		long lWidth, lHeight;
		UCLID_RASTERANDOCRMGMTLib::EOrientation eOrient;
		double dDeskew;
		ipPageInfo->GetPageInfo(&lWidth, &lHeight, &eOrient, &dDeskew);

		// Set the return value (equal if all pieces are equal)
		*pEqual = asVariantBool(m_nWidth == lWidth
								&& m_nHeight == lHeight
								&& m_eOrientation == eOrient
								&& m_fDeskew == dDeskew);
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25767");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// verify valid object
		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI09148", ipSource != NULL);

		// Get the values from the source object
		ipSource->GetPageInfo(&m_nWidth, &m_nHeight, &m_eOrientation, &m_fDeskew);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09149");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25768", pObject != NULL);

		// Create a new ISpatialString object
		ICopyableObjectPtr ipObjCopy(CLSID_SpatialPageInfo);
		ASSERT_RESOURCE_ALLOCATION("ELI09150", ipObjCopy != NULL);

		IUnknownPtr ipUnk(this);
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09151");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_SpatialPageInfo;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// If the dirty flag is set then this object is dirty
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09141");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Read the individual data items from the bytestream
		// read the data version
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		long nTmp;
		dataReader >> nTmp;
		m_eOrientation = (UCLID_RASTERANDOCRMGMTLib::EOrientation)nTmp;
		dataReader >> m_fDeskew;

		dataReader >> m_nWidth;
		dataReader >> m_nHeight;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI09142", "Unable to load newer SpatialPageInfo." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// clear the dirty flag as we just loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09144");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		
		// write the data version
		dataWriter << gnCurrentVersion;

		dataWriter << (long)m_eOrientation;
		dataWriter << m_fDeskew;

		dataWriter << m_nWidth;
		dataWriter << m_nHeight;

		// flush the bytestream
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// clear the flag as specified
		if (asCppBool(fClearDirty))
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09143");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialPageInfo::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------