
#include "stdafx.h"
#include "ParagraphTextCorrectors.h"
#include "CRLFParagraphTextCorrector.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <string>
using namespace std;

//--------------------------------------------------------------------------------------------------
CCRLFParagraphTextCorrector::CCRLFParagraphTextCorrector()
{
}
//--------------------------------------------------------------------------------------------------
CCRLFParagraphTextCorrector::~CCRLFParagraphTextCorrector()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20405");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CCRLFParagraphTextCorrector::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IParagraphTextCorrector
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CCRLFParagraphTextCorrector::raw_CorrectText(ISpatialString *pTextToCorrect)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ISpatialStringPtr ipTextToCorrect(pTextToCorrect);
		ASSERT_RESOURCE_ALLOCATION("ELI06516", ipTextToCorrect != NULL);

		// replace \n with CRLF
		ipTextToCorrect->Replace("\n", "\r\n", VARIANT_TRUE, 0, NULL);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03474")
}
//--------------------------------------------------------------------------------------------------
