// LongRectangle.cpp : Implementation of CLongRectangle
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "LongRectangle.h"

#include <UCLIDException.h>
#include <MathUtil.h>

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
		ASSERT_RESOURCE_ALLOCATION("ELI08391", ipSource != __nullptr);
	
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
		ASSERT_RESOURCE_ALLOCATION("ELI08396", ipObjCopy != __nullptr);

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
		// Rotate the rectangle by the specified angle
		rotateRectangle(m_nLeft, m_nTop, m_nRight, m_nBottom, nXLimit, nYLimit, nAngleInDegrees);
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
		ASSERT_ARGUMENT("ELI25726", plLeft != __nullptr);
		ASSERT_ARGUMENT("ELI25727", plTop != __nullptr);
		ASSERT_ARGUMENT("ELI25728", plRight != __nullptr);
		ASSERT_ARGUMENT("ELI25729", plBottom != __nullptr);

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
