// RasterZone.cpp : Implementation of CRasterZone
#include "stdafx.h"
#include "UCLIDRasterAndOCRMgmt.h"
#include "RasterZone.h"

#include <UCLIDException.h>
#include <mathUtil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <TPPolygon.h>

#include <Math.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version number for saving this object
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRasterZone,
		&IID_ICopyableObject,
		&IID_IComparableObject,
		&IID_IPersistStream,
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
// CRasterZone
//-------------------------------------------------------------------------------------------------
CRasterZone::CRasterZone() 
: m_bDirty(false)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	m_nStartX =  m_nStartY =  m_nEndX =  m_nEndY =  m_nHeight = m_nPage = 0;
}

//-------------------------------------------------------------------------------------------------
// IRasterZone
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::get_StartX(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI18349", pVal != __nullptr);

		*pVal = m_nStartX;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03292")


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::put_StartX(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_nStartX = newVal;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03293")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::get_StartY(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI18348", pVal != __nullptr);

		*pVal = m_nStartY;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03294")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::put_StartY(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_nStartY = newVal;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03295")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::get_EndX(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI18347", pVal != __nullptr);

		*pVal = m_nEndX;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03296")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::put_EndX(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_nEndX = newVal;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03297")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::get_EndY(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI18346", pVal != __nullptr);

		*pVal = m_nEndY;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03298")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::put_EndY(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_nEndY = newVal;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03299")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::get_Height(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI18345", pVal != __nullptr);

		*pVal = m_nHeight;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03300")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::put_Height(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_nHeight = newVal;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03301")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::get_PageNumber(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI18334", pVal != __nullptr);

		*pVal = m_nPage;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03302")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::put_PageNumber(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_nPage = newVal;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03303")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::CopyDataTo(IRasterZone *pRasterZone)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipRasterZone(pRasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI18333", ipRasterZone != __nullptr);

		ipRasterZone->StartX = m_nStartX;
		ipRasterZone->StartY = m_nStartY;
		ipRasterZone->EndX = m_nEndX;
		ipRasterZone->EndY = m_nEndY;
		ipRasterZone->Height = m_nHeight;
		ipRasterZone->PageNumber = m_nPage;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03304")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		clear();

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03305")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::Equals(IRasterZone *pRasterZone, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI18332", pbValue != __nullptr);

		// wrap raster zone in smart pointer
		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipRasterZone(pRasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI18331", pRasterZone != __nullptr);

		// Return the result of the equals comparison
		*pbValue = asVariantBool(equals(ipRasterZone));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03306")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::RotateBy(double dAngleInDegrees)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// check for zone coordinates validity
		if (m_nStartX == m_nEndX && m_nStartY  == m_nEndY)
		{
			UCLIDException ue("ELI03446", "Invalid Zone Coordinates!");
			throw ue;
		}

		// calculate slope in radians
		double dSlope = (dAngleInDegrees * MathVars::PI) / 180;

		// calculate mid point of the line joining start and end points
		double dMidX = (m_nStartX + m_nEndX) / 2;
		double dMidY = (m_nStartY + m_nEndY) / 2;

		// rotate the zone coordinates using Rotation and Translation matrix
		long lStartXPos = (long) (m_nStartX * cos(dSlope) - m_nStartY * sin(dSlope)
			- dMidX * cos(dSlope) + dMidY * sin(dSlope) + dMidX);
		
		long lStartYPos = (long) (m_nStartY * cos(dSlope) + m_nStartX * sin(dSlope)
			- dMidX * sin(dSlope) - dMidY * cos(dSlope) + dMidY);
		
		long lEndXPos = (long) (m_nEndX * cos(dSlope) - m_nEndY * sin(dSlope)
			- dMidX * cos(dSlope) + dMidY * sin(dSlope) + dMidX);
		
		long lEndYPos = (long) (m_nEndY * cos(dSlope) + m_nEndX * sin(dSlope)
			- dMidX * sin(dSlope) - dMidY * cos(dSlope) + dMidY);
		
		m_nStartX = lStartXPos;
		m_nStartY = lStartYPos;		
		
		m_nEndX	  = lEndXPos;
		m_nEndY   = lEndYPos;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03445")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::GetData(long *pStartX, long *pStartY, long *pEndX, 
								  long *pEndY, long *pHeight, long *pPageNum)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// validate arguments
		ASSERT_ARGUMENT("ELI06561", pStartX != __nullptr);
		ASSERT_ARGUMENT("ELI06562", pStartY != __nullptr);
		ASSERT_ARGUMENT("ELI06563", pEndX != __nullptr);
		ASSERT_ARGUMENT("ELI06564", pEndY != __nullptr);
		ASSERT_ARGUMENT("ELI06565", pHeight != __nullptr);
		ASSERT_ARGUMENT("ELI06566", pPageNum != __nullptr);

		// copy data for caller
		*pStartX = m_nStartX;
		*pStartY = m_nStartY;
		*pEndX = m_nEndX;
		*pEndY = m_nEndY;
		*pHeight = m_nHeight;
		*pPageNum = m_nPage;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06560")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::CreateFromLongRectangle(ILongRectangle *pRectangle, long nPageNum)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// validate arguments
		ILongRectanglePtr ipLongRectangle(pRectangle);
		ASSERT_ARGUMENT("ELI06571", ipLongRectangle != __nullptr);

		long lLeft(-1), lTop(-1), lRight(-1), lBottom(-1);
		ipLongRectangle->GetBounds(&lLeft, &lTop, &lRight, &lBottom);

		// copy data as specified
		// Ensure odd heights are fully covered by the resulting raster zone [LRCAU #5827]
		m_nStartX = lLeft;
		m_nStartY = (lTop + lBottom + 1) / 2;
		m_nEndX = lRight;
		m_nEndY = m_nStartY;
		m_nHeight = lBottom - lTop + 1;

		// Ensure height is odd
		if (m_nHeight % 2 == 0)
		{
			m_nHeight++;
		}

		m_nPage = nPageNum;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06572")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::GetRectangularBounds(ILongRectangle *pPageBounds, 
											   ILongRectangle* *pRectangle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI18335", pRectangle != __nullptr);

		// calculate the 4 corner points of the raster zone;
		// p1 = point above start point
		// p2 = point above end point
		// p3 = point below end point
		// p4 = point below start point
		POINT p1, p2, p3, p4;

		// calculate the angle of the line dy/dx
		double dAngle = atan2((double) (m_nEndY - m_nStartY), (double) (m_nEndX  - m_nStartX));
		
		// calculate the 4 points
		p1.x = (long) (m_nStartX - (((m_nHeight)/2) * sin (dAngle)));
		p1.y = (long) (m_nStartY + (((m_nHeight)/2) * cos (dAngle)));

		p2.x = (long) (m_nEndX - (((m_nHeight)/2) * sin (dAngle)));
		p2.y = (long) (m_nEndY + (((m_nHeight)/2) * cos (dAngle)));

		p3.x = (long) (m_nEndX + (((m_nHeight)/2) * sin (dAngle)));
		p3.y = (long) (m_nEndY - (((m_nHeight)/2) * cos (dAngle)));

		p4.x = (long) (m_nStartX + (((m_nHeight)/2) * sin (dAngle)));
		p4.y = (long) (m_nStartY - (((m_nHeight)/2) * cos (dAngle)));

		// calculate the rectangular bounds
		RECT rect;
		rect.top = min(p1.y, min(p2.y, min(p3.y, p4.y)));
		rect.left = min(p1.x, min(p2.x, min(p3.x, p4.x)));
		rect.bottom = max(p1.y, max(p2.y, max(p3.y, p4.y)));
		rect.right = max(p1.x, max(p2.x, max(p3.x, p4.x)));

		// fit the rectangular bounds within the page if page info map was specified
		if(pPageBounds != __nullptr)
		{
			// usa a smart pointer for the spatial page info
			ILongRectanglePtr ipPageBounds(pPageBounds);
			ASSERT_RESOURCE_ALLOCATION("ELI30334", ipPageBounds != __nullptr);

			long lLeftBound(-1), lTopBound(-1), lRightBound(-1), lBottomBound(-1);
			ipPageBounds->GetBounds(&lLeftBound, &lTopBound, &lRightBound, &lBottomBound);

			// crop the top and left to their minimum values if needed
			if(rect.top < 0)
			{
				rect.top = 0;
			}
			if(rect.left < 0)
			{
				rect.left = 0;
			}

			// crop the bottom and right to their maximum values if needed
			if(rect.bottom > lBottomBound)
			{
				rect.bottom = lBottomBound;
			}
			if(rect.right > lRightBound)
			{
				rect.right = lRightBound;
			}
		}

		// create a long rectangle object to return to the caller
		ILongRectanglePtr ipLongRectangle(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI06577", ipLongRectangle != __nullptr);

		// populate the long rectangle object
		ipLongRectangle->SetBounds(rect.left, rect.top, rect.right, rect.bottom);

		// return the long rectangle object to the caller
		*pRectangle = ipLongRectangle.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06576");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::GetBoundaryPoints(IIUnknownVector** pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI18336", pRetVal != __nullptr);

		IIUnknownVectorPtr ipPoints(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI10033", ipPoints != __nullptr);

		// calculate the 4 corner points of the raster zone;
		// p1 = point above start point
		// p2 = point above end point
		// p3 = point below end point
		// p4 = point below start point
		IDoublePointPtr ipP1(CLSID_DoublePoint);
		ASSERT_RESOURCE_ALLOCATION("ELI10028", ipP1 != __nullptr);
		IDoublePointPtr ipP2(CLSID_DoublePoint);
		ASSERT_RESOURCE_ALLOCATION("ELI10029", ipP2 != __nullptr);
		IDoublePointPtr ipP3(CLSID_DoublePoint);
		ASSERT_RESOURCE_ALLOCATION("ELI10031", ipP3 != __nullptr);
		IDoublePointPtr ipP4(CLSID_DoublePoint);
		ASSERT_RESOURCE_ALLOCATION("ELI10032", ipP4 != __nullptr);

		// calculate the angle of the line dy/dx
		double dAngle = atan2((double) (m_nEndY - m_nStartY), (double) (m_nEndX  - m_nStartX));
		
		// calculate the 4 points
		// using "2.0" instead of "2" to get a double value of m_nHeight/2.0
		ipP1->X = (m_nStartX - ((m_nHeight/2.0) * sin (dAngle)));
		ipP1->Y = (m_nStartY + ((m_nHeight/2.0) * cos (dAngle)));
		ipPoints->PushBack(ipP1);

		ipP2->X = (m_nEndX - ((m_nHeight/2.0) * sin (dAngle)));
		ipP2->Y = (m_nEndY + ((m_nHeight/2.0) * cos (dAngle)));
		ipPoints->PushBack(ipP2);

		ipP3->X = (m_nEndX + ((m_nHeight/2.0) * sin (dAngle)));
		ipP3->Y = (m_nEndY - ((m_nHeight/2.0) * cos (dAngle)));
		ipPoints->PushBack(ipP3);

		ipP4->X =  (m_nStartX + ((m_nHeight/2.0) * sin (dAngle)));
		ipP4->Y =  (m_nStartY - ((m_nHeight/2.0) * cos (dAngle)));
		ipPoints->PushBack(ipP4);

		*pRetVal = ipPoints.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10027")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::get_Area(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI18337", pVal != __nullptr);

		long dX = m_nEndX - m_nStartX;
		long dY = m_nEndY - m_nStartY;

		double dLengthSq = (dX*dX) + (dY*dY);
		double dLength = sqrt(dLengthSq);
		double dArea = dLength * (double) m_nHeight;

		*pVal = (long) dArea;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12025")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::GetAreaOverlappingWith(IRasterZone *pRasterZone, double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI18338", pVal != __nullptr);

		// Create IIUnknownVectorPtr to hold the corners of this zone
		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI15084", ipThis != __nullptr);

		// wrap the other raster zone in a smart pointer object
		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipSecond(pRasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI15085", ipSecond != __nullptr);

		// default overlap to 0
		*pVal = 0.0;

		// make sure the zones are from the same page [p13 #4674, p16 #2401] - JDS
		if (m_nPage == ipSecond->PageNumber)
		{
			// Get the boundary points
			IIUnknownVectorPtr ipPoints = ipThis->GetBoundaryPoints();
			ASSERT_RESOURCE_ALLOCATION("ELI18498", ipPoints != __nullptr);

			// Create a polygon describing this raster zone
			TPPolygon polyFirst;
			long lPointsSize = ipPoints->Size();
			for (long i = 0; i < lPointsSize; i++)
			{
				// Get the point out of the IIUnknownVector
				IDoublePointPtr ipP1 = ipPoints->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI15070", ipP1 != __nullptr);

				// Create the TPPoint and push into the polygon
				TPPoint p1(ipP1->X, ipP1->Y);
				polyFirst.addPoint(p1);
			}

			// clear the points vector
			ipPoints = __nullptr;

			// Create IIUnknownVectorPtr hold all the points inside the zone
			// pointed by pRasterZone
			ipPoints = ipSecond->GetBoundaryPoints();
			ASSERT_RESOURCE_ALLOCATION("ELI18499", ipPoints != __nullptr);

			// Create a polygon describe the zone pointed by pRasterZone
			TPPolygon polySecond;
			lPointsSize = ipPoints->Size();
			for (long i = 0; i < lPointsSize; i++)
			{
				// Get the point out of the IIUnknownVector
				IDoublePointPtr ipP2 = ipPoints->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI15071", ipP2 != __nullptr);

				// Create the TPPoint and push into the polygon
				TPPoint p2(ipP2->X, ipP2->Y);
				polySecond.addPoint(p2);
			}

			// Get the intersection area of the two polygons
			*pVal = polyFirst.getIntersectionArea(polySecond);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15042")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::GetBoundsFromMultipleRasterZones(IIUnknownVector *pZones,
														   ISpatialPageInfo *pPageInfo,
														   ILongRectangle **ppRectangle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Wrap the IUnknownVector in a smart pointer
		IIUnknownVectorPtr ipZones(pZones);
		ASSERT_ARGUMENT("ELI25652", ipZones != __nullptr);
		ASSERT_ARGUMENT("ELI25653", ppRectangle != __nullptr);

		// Validate the license
		validateLicense();

		// Create some longs to keep track of the max values for each side of the rectangle
		long nMaxBottom(0), nMaxRight(0);
		long nMinTop(0), nMinLeft(0);
		long lPageNumber = -1;

		// Loop through each raster zone and update the max/min values for each side of
		// the rectangle
		long lSize = ipZones->Size();
		for(long i=0; i < lSize; i++)
		{
			UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipZones->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI25654", ipZone != __nullptr);

			if (lPageNumber == -1)
			{
				lPageNumber = ipZone->PageNumber;
			}
			else
			{
				if (lPageNumber != ipZone->PageNumber)
				{
					UCLIDException ue("ELI25655",
						"Cannot compute bounds of zones from different pages!");
					throw ue;
				}
			}

			// Get the associated bounding rectangle
			ILongRectanglePtr ipLongRectangle = ipZone->GetRectangularBounds(NULL);
			ASSERT_RESOURCE_ALLOCATION("ELI25656", ipLongRectangle != __nullptr);

			// Get the bounds
			long lLeft, lTop, lRight, lBottom;
			ipLongRectangle->GetBounds(&lLeft, &lTop, &lRight, &lBottom);

			// Fill the variables with some initial values
			if( i == 0 )
			{
				nMinTop = lTop;
				nMaxBottom = lBottom;
				nMinLeft = lLeft;
				nMaxRight = lRight;
			}
			else
			{
				// If not the first time through, compare the rectangle dimensions to create
				// a larger rectangle that bounds all the raster zones.
				nMinTop = min(lTop, nMinTop);
				nMaxBottom = max(lBottom, nMaxBottom);
				nMinLeft = min(lLeft, nMinLeft);
				nMaxRight = max(lRight, nMaxRight);
			}
		}

		// Create a rectangle with the computed bounds
		RECT rect;
		rect.left = nMinLeft;
		rect.top = nMinTop;
		rect.right = nMaxRight;
		rect.bottom = nMaxBottom;

		// Wrap spatial page info in a smart pointer and check for NULL
		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo(pPageInfo);
		if (ipPageInfo != __nullptr)
		{
			// Page info is not null, restrict the bounds of the returned rectangle
			// by page dimensions
			if(rect.top < 0)
			{
				rect.top = 0;
			}
			if(rect.left < 0)
			{
				rect.left = 0;
			}

			// crop the bottom and right to their maximum values if needed
			long lMaxBottom, lMaxWidth;
			ipPageInfo->GetWidthAndHeight(&lMaxWidth, &lMaxBottom);
			if(rect.bottom > lMaxBottom)
			{
				rect.bottom = lMaxBottom;
			}
			if(rect.right > lMaxWidth)
			{
				rect.right = lMaxWidth;
			}
		}

		// Create the return rectangle
		ILongRectanglePtr ipRect(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI25657", ipRect != __nullptr);
		ipRect->SetBounds(rect.left, rect.top, rect.right, rect.bottom);

		// Return the bounding rectangle
		*ppRectangle = ipRect.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25628");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::CreateFromData(long lStartX, long lStartY, long lEndX, long lEndY,
										 long lHeight, long lPageNum)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Set the values
		m_nStartX = lStartX;
		m_nStartY = lStartY;
		m_nEndX = lEndX;
		m_nEndY = lEndY;
		m_nHeight = lHeight;
		m_nPage = lPageNum;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25741");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18350", pbValue != __nullptr);

		try
		{
			validateLicense();

			// If validateLicense doesn't throw any exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18351");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Verify a valid IRasterZone
		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI15162", ipSource != __nullptr);

		// Load each member variable
		m_nStartX = ipSource->StartX;
		m_nStartY = ipSource->StartY;
		m_nEndX = ipSource->EndX;
		m_nEndY = ipSource->EndY;
		m_nHeight = ipSource->Height;
		m_nPage = ipSource->PageNumber;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15160");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18339", pObject != __nullptr);

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_RasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI15163", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15161");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IComparableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::raw_IsEqualTo(IUnknown * pObj, VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI18340", pbValue != __nullptr);

		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipObj( pObj );
		ASSERT_RESOURCE_ALLOCATION("ELI15299", ipObj != __nullptr);

		// Return the result of the equals comparison
		*pbValue = asVariantBool(equals(ipObj));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15323");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI18342", pClassID != __nullptr);
		*pClassID = CLSID_RasterZone;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18341");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Just check this object's dirty flag
		if( m_bDirty )
		{
			return S_OK;
		}
		else
		{
			return S_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15158");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI18343", pStream != __nullptr);

		// Read the bytestream data from the IStream object
		long nDataLength = 0;

		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );

		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Clear this object without setting the dirty flag, set it below
		clear(); 

		// read the data version
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI15156", "Unable to load newer RasterZone." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Read the member variables from the stream
		dataReader >> m_nStartX;
		dataReader >> m_nStartY;
		dataReader >> m_nEndX;
		dataReader >> m_nEndY;
		dataReader >> m_nHeight;
		dataReader >> m_nPage;	

		// Since we just loaded this object, dirty flag is false
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15157");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI18344", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );

		// Write the data version
		dataWriter << gnCurrentVersion;

		// Write the items for this object
		dataWriter << m_nStartX;
		dataWriter << m_nStartY;
		dataWriter << m_nEndX;
		dataWriter << m_nEndY;
		dataWriter << m_nHeight;
		dataWriter << m_nPage;

		// flugh the bytestream
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15155");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRasterZone::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRasterZone::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI03482", "Raster Zone" );
}
//-------------------------------------------------------------------------------------------------
void CRasterZone::clear()
{
	// clear all the local member variables
	m_nStartX =  m_nStartY =  m_nEndX =  m_nEndY =  m_nHeight = m_nPage = 0;
}
//-------------------------------------------------------------------------------------------------
bool CRasterZone::equals(UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone)
{
	try
	{
		ASSERT_ARGUMENT("ELI25742", ipZone != __nullptr);

		// Get the data from the provided raster zone
		long lStartX, lStartY, lEndX, lEndY, lHeight, lPageNumber;
		ipZone->GetData(&lStartX, &lStartY, &lEndX, &lEndY, &lHeight, &lPageNumber);

		// Compare each data item
		return m_nStartX == lStartX
			&& m_nStartY == lStartY
			&& m_nEndX == lEndX
			&& m_nEndY == lEndY
			&& m_nHeight == lHeight
			&& m_nPage == lPageNumber;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25743");
}
//-------------------------------------------------------------------------------------------------
