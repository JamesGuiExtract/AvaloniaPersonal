// Letter.cpp : Implementation of CLetter

#include "stdafx.h"
#include "UCLIDRasterAndOCRMgmt.h"
#include "Letter.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 7;

//-------------------------------------------------------------------------------------------------
// CLetter
//-------------------------------------------------------------------------------------------------
CLetter::CLetter()
{
	// reset member varibales to default value
	reset();
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILetter,
		&IID_IPersistStream,
		&IID_ICopyableObject,
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// verify valid object
		UCLID_RASTERANDOCRMGMTLib::ILetterPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08311", ipSource != __nullptr);

		// copy the information from this object to the other object
		m_letter.m_ulLeft = ipSource->GetLeft();
		m_letter.m_ulTop = ipSource->GetTop();
		m_letter.m_ulRight = ipSource->GetRight();
		m_letter.m_ulBottom = ipSource->GetBottom();

		m_letter.m_usGuess1 = (unsigned short) ipSource->GetGuess1();
		m_letter.m_usGuess2 = (unsigned short) ipSource->GetGuess2();
		m_letter.m_usGuess3= (unsigned short) ipSource->GetGuess3();

		m_letter.m_bIsEndOfParagraph = (ipSource->GetIsEndOfParagraph()==VARIANT_TRUE) ? true : false;
		m_letter.m_bIsEndOfZone = (ipSource->GetIsEndOfZone()==VARIANT_TRUE) ? true : false;

		m_letter.m_bIsSpatial = (ipSource->GetIsSpatialChar()==VARIANT_TRUE) ? true : false;
		m_letter.m_usPageNumber = (unsigned short) ipSource->GetPageNumber();

		m_letter.m_ucFontSize = (unsigned char) ipSource->FontSize;

		m_letter.m_ucCharConfidence = (unsigned char) ipSource->CharConfidence;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08312");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create a new ILetter object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_Letter);
		ASSERT_RESOURCE_ALLOCATION("ELI08371", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06470");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILetter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_Left(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ulLeft;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05655")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_Left(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_ulLeft = (unsigned short) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05656")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_Top(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ulTop;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05657")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_Top(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_ulTop = (unsigned short) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05658")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_Right(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ulRight;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05659")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_Right(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_ulRight = (unsigned short) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05660")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_Bottom(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ulBottom;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05661")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_Bottom(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_ulBottom = (unsigned short) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05662")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_Guess1(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_usGuess1;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05665")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_Guess1(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_usGuess1 = (unsigned short) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05666")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_Guess2(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_usGuess2;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05667")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_Guess2(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_usGuess2 = (unsigned short) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05668")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_Guess3(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_usGuess3;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05669")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_Guess3(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_usGuess3 = (unsigned short) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05670")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsEndOfParagraph(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_bIsEndOfParagraph ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05677")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsEndOfParagraph(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_bIsEndOfParagraph = newVal == VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05678")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsEndOfZone(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_bIsEndOfZone ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05679")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsEndOfZone(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_bIsEndOfZone = (newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05680")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsSpatialChar(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_bIsSpatial ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05689")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsSpatialChar(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_bIsSpatial = (newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05690")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_PageNumber(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_usPageNumber;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06221")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_PageNumber(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_usPageNumber = (unsigned short) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06222")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::SetAsNonSpatialChar(long nGuess1)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_letter.m_usGuess1 = (unsigned short) nGuess1;
	m_letter.m_bIsSpatial = false;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::ToLowerCase()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_usGuess1 = ::tolower(m_letter.m_usGuess1);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06609")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::ToUpperCase()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_usGuess1 = ::toupper(m_letter.m_usGuess1);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06610")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::Offset(long nX, long nY)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// if this letter is spatial, then apply the offset
		if (m_letter.m_bIsSpatial)
		{
			m_letter.m_ulTop += nY;
			m_letter.m_ulLeft += nX;
			m_letter.m_ulRight += nX;
			m_letter.m_ulBottom += nY;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06661")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_FontSize(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ucFontSize;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19318")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_FontSize(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_ucFontSize = (unsigned char) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19319")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_CharConfidence(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ucCharConfidence;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10623")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_CharConfidence(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		m_letter.m_ucCharConfidence = (unsigned char) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10624")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsItalic(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();
		*pVal = m_letter.m_ucFont & LTR_ITALIC ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10673")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsItalic(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();
		setFontFlag(LTR_ITALIC, newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10668")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsBold(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ucFont & LTR_BOLD ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10672")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsBold(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();
		setFontFlag(LTR_BOLD, newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10669")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsSansSerif(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ucFont & LTR_SANSSERIF ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10671")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsSansSerif(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();
		setFontFlag(LTR_SANSSERIF, newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10670")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsSerif(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ucFont & LTR_SERIF ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10674")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsSerif(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();
		setFontFlag(LTR_SERIF, newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10675")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsProportional(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ucFont & LTR_PROPORTIONAL ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10676")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsProportional(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();
		setFontFlag(LTR_PROPORTIONAL, newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10677")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsUnderline(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ucFont & LTR_UNDERLINE ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10678")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsUnderline(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();
		setFontFlag(LTR_UNDERLINE, newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10679")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsSuperScript(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ucFont & LTR_SUPERSCRIPT ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10680")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsSuperScript(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();
		setFontFlag(LTR_SUPERSCRIPT, newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10681")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::get_IsSubScript(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*pVal = m_letter.m_ucFont & LTR_SUBSCRIPT ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19386")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::put_IsSubScript(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();
		setFontFlag(LTR_SUBSCRIPT, newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19387")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::CreateFromCppLetter(void *pLetter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		ASSERT_ARGUMENT("ELI25737", pLetter != __nullptr);

		// validate license
		validateLicense();

		// Copy the CPPLetter into this letter
		CPPLetter* pCppLetter = (CPPLetter*) pLetter;
		m_letter = *pCppLetter;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25738");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::GetCppLetter(void *pLetter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		ASSERT_ARGUMENT("ELI25739", pLetter != __nullptr);

		// validate license
		validateLicense();

		// Copy this letter into the CPPLetter
		CPPLetter* pCppLetter = (CPPLetter*) pLetter;
		*pCppLetter = m_letter;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25740");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_Letter;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// return hr value based on m_bDirty
		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06654");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// reset all the member variables to default state
		reset();

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI07671", "Unable to load newer Letter." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if(nDataVersion == 1)
		{
		// read the object data
			dataReader >> m_letter.m_usGuess1 >> m_letter.m_usGuess2 >> m_letter.m_usGuess3;
			unsigned short usTop, usLeft, usRight, usBottom;
			dataReader >> usTop >> usLeft >> usRight >> usBottom;
			m_letter.m_ulTop = usTop;
			m_letter.m_ulLeft = usLeft;
			m_letter.m_ulRight = usRight;
			m_letter.m_ulBottom = usBottom;
			unsigned char ucTemp;
			dataReader >> ucTemp;
			m_letter.m_usPageNumber = ucTemp;
			// eat the endOfLine and end Of word flags that used to be in letters
			bool bJunk;
			dataReader >> bJunk >> bJunk >> m_letter.m_bIsEndOfParagraph;
			dataReader >> m_letter.m_bIsEndOfZone >> bJunk;
			dataReader >> m_letter.m_bIsSpatial;
		}
		else if(nDataVersion == 2)
		{
			dataReader >> m_letter.m_usGuess1 >> m_letter.m_usGuess2 >> m_letter.m_usGuess3;
			unsigned short usTop, usLeft, usRight, usBottom;
			dataReader >> usTop >> usLeft >> usRight >> usBottom;
			m_letter.m_ulTop = usTop;
			m_letter.m_ulLeft = usLeft;
			m_letter.m_ulRight = usRight;
			m_letter.m_ulBottom = usBottom;
			unsigned char ucTemp;
			dataReader >> ucTemp;
			m_letter.m_usPageNumber = ucTemp;
			bool bJunk;
			dataReader >> m_letter.m_bIsEndOfParagraph;
			dataReader >> m_letter.m_bIsEndOfZone >> bJunk;
			dataReader >> m_letter.m_bIsSpatial;
		}
		else if(nDataVersion == 3)
		{
			dataReader >> m_letter.m_usGuess1 >> m_letter.m_usGuess2 >> m_letter.m_usGuess3;
			unsigned short usTop, usLeft, usRight, usBottom;
			dataReader >> usTop >> usLeft >> usRight >> usBottom;
			m_letter.m_ulTop = usTop;
			m_letter.m_ulLeft = usLeft;
			m_letter.m_ulRight = usRight;
			m_letter.m_ulBottom = usBottom;
			unsigned char ucTemp;
			dataReader >> ucTemp;
			m_letter.m_usPageNumber = ucTemp;
			dataReader >> m_letter.m_bIsEndOfParagraph;
			dataReader >> m_letter.m_bIsEndOfZone;
			dataReader >> m_letter.m_bIsSpatial;
		}
		else if(nDataVersion == 4)
		{
			dataReader >> m_letter.m_usGuess1 >> m_letter.m_usGuess2 >> m_letter.m_usGuess3;
			unsigned short usTop, usLeft, usRight, usBottom;
			dataReader >> usTop >> usLeft >> usRight >> usBottom;
			m_letter.m_ulTop = usTop;
			m_letter.m_ulLeft = usLeft;
			m_letter.m_ulRight = usRight;
			m_letter.m_ulBottom = usBottom;
			unsigned char ucTemp;
			dataReader >> ucTemp;
			m_letter.m_usPageNumber = ucTemp;
			dataReader >> m_letter.m_bIsEndOfParagraph;
			dataReader >> m_letter.m_bIsEndOfZone;
			dataReader >> m_letter.m_bIsSpatial;
			dataReader >> m_letter.m_ucFontSize;
			dataReader >> m_letter.m_ucCharConfidence;
		}
		else if(nDataVersion == 5)
		{
			dataReader >> m_letter.m_usGuess1 >> m_letter.m_usGuess2 >> m_letter.m_usGuess3;
			unsigned short usTop, usLeft, usRight, usBottom;
			dataReader >> usTop >> usLeft >> usRight >> usBottom;
			m_letter.m_ulTop = usTop;
			m_letter.m_ulLeft = usLeft;
			m_letter.m_ulRight = usRight;
			m_letter.m_ulBottom = usBottom;
			unsigned char ucTemp;
			dataReader >> ucTemp;
			m_letter.m_usPageNumber = ucTemp;
			dataReader >> m_letter.m_bIsEndOfParagraph;
			dataReader >> m_letter.m_bIsEndOfZone;
			dataReader >> m_letter.m_bIsSpatial;
			dataReader >> m_letter.m_ucFontSize;
			dataReader >> m_letter.m_ucCharConfidence;
			dataReader >> m_letter.m_ucFont;
		}
		else if(nDataVersion == 6)
		{
			dataReader >> m_letter.m_usGuess1 >> m_letter.m_usGuess2 >> m_letter.m_usGuess3;
			unsigned short usTop, usLeft, usRight, usBottom;
			dataReader >> usTop >> usLeft >> usRight >> usBottom;
			m_letter.m_ulTop = usTop;
			m_letter.m_ulLeft = usLeft;
			m_letter.m_ulRight = usRight;
			m_letter.m_ulBottom = usBottom;
			dataReader >> m_letter.m_usPageNumber;
			dataReader >> m_letter.m_bIsEndOfParagraph;
			dataReader >> m_letter.m_bIsEndOfZone;
			dataReader >> m_letter.m_bIsSpatial;
			dataReader >> m_letter.m_ucFontSize;
			dataReader >> m_letter.m_ucCharConfidence;
			dataReader >> m_letter.m_ucFont;
		}
		else if(nDataVersion >= 7)
		{
			dataReader >> m_letter.m_usGuess1 >> m_letter.m_usGuess2 >> m_letter.m_usGuess3;
			dataReader >> m_letter.m_ulTop >> m_letter.m_ulLeft >> m_letter.m_ulRight >> m_letter.m_ulBottom;
			dataReader >> m_letter.m_usPageNumber;
			dataReader >> m_letter.m_bIsEndOfParagraph;
			dataReader >> m_letter.m_bIsEndOfZone;
			dataReader >> m_letter.m_bIsSpatial;
			dataReader >> m_letter.m_ucFontSize;
			dataReader >> m_letter.m_ucCharConfidence;
			dataReader >> m_letter.m_ucFont;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06653");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		
		// write the object version #
		dataWriter << gnCurrentVersion;

		// write the object data
		dataWriter << m_letter.m_usGuess1 << m_letter.m_usGuess2 << m_letter.m_usGuess3;
		dataWriter << m_letter.m_ulTop << m_letter.m_ulLeft << m_letter.m_ulRight << m_letter.m_ulBottom;
		dataWriter << m_letter.m_usPageNumber;
		dataWriter << m_letter.m_bIsEndOfParagraph;
		dataWriter << m_letter.m_bIsEndOfZone;
		dataWriter << m_letter.m_bIsSpatial;
		dataWriter << m_letter.m_ucFontSize;
		dataWriter << m_letter.m_ucCharConfidence;
		dataWriter << m_letter.m_ucFont;
		
		// flush the bytestream
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06652");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLetter::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CLetter::reset()
{
	m_letter.m_usGuess1 = m_letter.m_usGuess2 = m_letter.m_usGuess3 = 0;
	m_letter.m_ulTop = m_letter.m_ulLeft = m_letter.m_ulRight = m_letter.m_ulBottom = 0;
	m_letter.m_bIsEndOfParagraph = m_letter.m_bIsEndOfZone = false;
	m_letter.m_bIsSpatial = false;
	m_letter.m_usPageNumber = -1;
	m_letter.m_ucFontSize = 0;
	m_letter.m_ucCharConfidence = 100;
	m_bDirty = false;
}
//-------------------------------------------------------------------------------------------------
void CLetter::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI05784", "Letter" );
}
//-------------------------------------------------------------------------------------------------
void CLetter::setFontFlag(unsigned char flag, bool bSet)
{
	if(bSet)
	{
		m_letter.m_ucFont |= flag;
	}
	else
	{
		m_letter.m_ucFont &= (flag ^ 0xFF);
	}
}
//-------------------------------------------------------------------------------------------------
