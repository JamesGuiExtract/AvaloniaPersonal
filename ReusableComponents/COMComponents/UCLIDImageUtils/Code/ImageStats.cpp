// ImageStats.cpp : Implementation of CImageStats

#include "stdafx.h"
#include "ImageStats.h"
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// CImageStats
//-------------------------------------------------------------------------------------------------
CImageStats::CImageStats()
:	m_lWidth(0),
	m_lHeight(0),
	m_crFGColor(RGB(0,0,0)),
	m_ipVecFGPixelsInRow(NULL)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageStats::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IImageStats
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IImageStats
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageStats::get_FGPixelsInRow(IVariantVector ** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if ( m_ipVecFGPixelsInRow == __nullptr )
		{
			m_ipVecFGPixelsInRow.CreateInstance(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI13289", m_ipVecFGPixelsInRow != __nullptr );
		}

		IVariantVectorPtr ipShallowCopy = m_ipVecFGPixelsInRow;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13288");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageStats::put_FGPixelsInRow(IVariantVector * newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_ipVecFGPixelsInRow = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13290");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageStats::get_FGColor(COLORREF* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pVal = m_crFGColor;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13291");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageStats::put_FGColor(COLORREF newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_crFGColor = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13292")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageStats::get_Width(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pVal = m_lWidth;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13293");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageStats::put_Width(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_lWidth = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13294");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageStats::get_Height(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());


	try
	{
		*pVal = m_lHeight;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13295");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageStats::put_Height(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_lHeight = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13296");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
