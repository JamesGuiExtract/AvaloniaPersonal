// ImageData.cpp : Implementation of CImageData

#include "stdafx.h"
#include "ImageData.h"
#include <MiscLeadutils.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>

//-------------------------------------------------------------------------------------------------
// CImageData
//-------------------------------------------------------------------------------------------------
CImageData::CImageData()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageData::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IImageData
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IImageDAta
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageData::GetImagePageCount(BSTR bstrImageName, LONG *pnNumPages)
{
	try
	{
		ASSERT_ARGUMENT("ELI15754", pnNumPages != NULL );

		*pnNumPages = getNumberOfPagesInImage(asString(bstrImageName));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15755");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
