// LongRectangle.cpp : Implementation of CLongRectangle
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "LongRectangle.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
CLongRectangle::CLongRectangle()
:m_nLeft(0), m_nTop(0), m_nRight(0), m_nBottom(0)
{
}
//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Ensure that the object is a Vector
		UCLID_COMUTILSLib::ILongRectanglePtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08391", ipSource != NULL);
	
		m_nLeft = ipSource->GetLeft();
		m_nRight = ipSource->GetRight();
		m_nTop = ipSource->GetTop();
		m_nBottom = ipSource->GetBottom();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08392");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create a new variant vector
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI08396", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08397");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo interface
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILongRectangle
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILongRectangle interface
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::get_Left(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = m_nLeft;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::put_Left(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_nLeft = newVal;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::get_Top(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = m_nTop;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::put_Top(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_nTop = newVal;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::get_Right(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = m_nRight;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::put_Right(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_nRight = newVal;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::get_Bottom(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = m_nBottom;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::put_Bottom(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_nBottom = newVal;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::SetBounds(long nLeft, long nTop, long nRight, long nBottom)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_nLeft = nLeft;
	m_nTop = nTop;
	m_nRight = nRight;
	m_nBottom = nBottom;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::Offset(long nX, long nY)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_nLeft += nX;
	m_nTop += nY;
	m_nRight += nX;
	m_nBottom += nY;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::Expand(long nX, long nY)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_nRight += nX;
	m_nBottom += nY;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::Clip(long nLeft, long nTop, long nRight, long nBottom)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (m_nLeft < nLeft)
	{
		m_nLeft = nLeft;
	}

	if (m_nTop < nTop)
	{
		m_nTop = nTop;
	}

	if (m_nRight > nRight)
	{
		m_nRight = nRight;
	}

	if (m_nBottom > nBottom)
	{
		m_nBottom = nBottom;
	}

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::Rotate(long nXLimit, long nYLimit, long nAngleInDegrees)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Store original settings
		long lOrigTop    = m_nTop;
		long lOrigLeft   = m_nLeft;
		long lOrigBottom = m_nBottom;
		long lOrigRight  = m_nRight;

		// Validate nAngleInDegrees
		switch (nAngleInDegrees)
		{
		case 0:
		case 360:
			// No rotation is needed
			break;

		case 90:
		case -270:
			// Rotate the rectangle 90-degrees clockwise
			m_nTop = lOrigLeft;
			m_nLeft = nYLimit - lOrigBottom;
			m_nRight = nYLimit - lOrigTop;
			m_nBottom = lOrigRight;
			break;

		case 180:
		case -180:
			// Turn the rectangle upside down
			m_nTop = nYLimit - lOrigBottom;
			m_nLeft = nXLimit - lOrigRight;
			m_nRight = nXLimit - lOrigLeft;
			m_nBottom = nYLimit - lOrigTop;
			break;

		case 270:
		case -90:
			// Rotate the rectangle 90-degrees counterclockwise
			m_nTop = nXLimit - lOrigRight;
			m_nLeft = lOrigTop;
			m_nRight = lOrigBottom;
			m_nBottom = nXLimit - lOrigLeft;
			break;

		default:
			UCLIDException ue("ELI16737", "Invalid rotation angle for Long Rectangle!");
			ue.addDebugInfo("Angle", nAngleInDegrees);
			throw ue;
		}

		// Check that bounds are >= 0
		if (m_nTop < 0 || m_nLeft < 0 || m_nBottom < 0 || m_nRight < 0)
		{
			UCLIDException ue("ELI16742", "Invalid bounds for Long Rectangle after rotation!");
			ue.addDebugInfo("Angle", nAngleInDegrees);
			ue.addDebugInfo("New Top", m_nTop);
			ue.addDebugInfo("New Left", m_nLeft);
			ue.addDebugInfo("New Bottom", m_nBottom);
			ue.addDebugInfo("New Right", m_nRight);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16736");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongRectangle::GetBounds(long *plLeft, long *plTop, long *plRight, long *plBottom)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check arguments
		ASSERT_ARGUMENT("ELI25726", plLeft != NULL);
		ASSERT_ARGUMENT("ELI25727", plTop != NULL);
		ASSERT_ARGUMENT("ELI25728", plRight != NULL);
		ASSERT_ARGUMENT("ELI25729", plBottom != NULL);

		// Set the return values
		*plLeft = m_nLeft;
		*plTop = m_nTop;
		*plRight = m_nRight;
		*plBottom = m_nBottom;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25730");
}
//-------------------------------------------------------------------------------------------------
