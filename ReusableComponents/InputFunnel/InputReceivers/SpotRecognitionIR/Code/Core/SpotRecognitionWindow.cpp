
#include "stdafx.h"
#include "SpotRecognitionIR.h"
#include "SpotRecognitionWindow.h"
#include "SpotRecognitionDlg.h"

#include <cpputil.h>
#include <LicenseMgmt.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <Comdef.h>
#include <CommentedTextFileReader.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>
#include <StringTokenizer.h>

//-------------------------------------------------------------------------------------------------
// CSpotRecognitionWindow
//-------------------------------------------------------------------------------------------------
CSpotRecognitionWindow::CSpotRecognitionWindow()
: m_lParentWndHandle(0)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// do not create the spot rec dialog here.
		m_apDlg.reset(__nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI03172")
}
//-------------------------------------------------------------------------------------------------
CSpotRecognitionWindow::~CSpotRecognitionWindow()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// force deletion of the dialog object within the scope of this destructor
		// so that we know the destruction is happening with the correct AFX module state
		m_apDlg.reset(__nullptr);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16501");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISpotRecognitionWindow,
		&IID_IInputEntityManager,
		&IID_IInputReceiver,
		&IID_ICategorizedComponent,
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
// ISpotRecognitionWindow
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::OpenImageFile(BSTR strImageFileFullPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{

		// try to ensure that this component is licensed.
		validateLicense();

		string stdstrFileName = asString( strImageFileFullPath );

		getSpotRecognitionDlg()->openFile2(stdstrFileName);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02315")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::OpenGDDFile(BSTR strGDDFileFullPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		string stdstrFileName = asString( strGDDFileFullPath );

		getSpotRecognitionDlg()->openFile2(stdstrFileName);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02316")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::SaveAs(BSTR strFileFullPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		string stdstrFileName = asString( strFileFullPath );

		getSpotRecognitionDlg()->saveAs(stdstrFileName);
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02317")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::Save()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->save();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19275")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->clear();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02318")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::IsModified(VARIANT_BOOL *pbIsModified)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23839", pbIsModified != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();

		*pbIsModified = getSpotRecognitionDlg()->isModified() ? VARIANT_TRUE : VARIANT_FALSE;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02319")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetCurrentPageNumber(long *plPageNum)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23840", plPageNum != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();

		*plPageNum = getSpotRecognitionDlg()->getCurrentPageNumber();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02320")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::SetCurrentPageNumber(long lPageNumber)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->setCurrentPageNumber(lPageNumber);
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02321")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetTotalPages(long *plTotalPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23841", plTotalPages != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();

		*plTotalPages = getSpotRecognitionDlg()->getTotalPages();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02322")
}
//-------------------------------------------------------------------------------------------------
void CSpotRecognitionWindow::getIDs(BSTR bstrID, vector<long>& rvecIDs)
{
	// The id is a comma-delimited list of zone entity IDs. Get a vector of ids.
	StringTokenizer tokenizer(',');
	vector<string> tokens;
	tokenizer.parse(asString(bstrID), tokens);

	// Ensure the input entity id contains at least one zone entity id
	ASSERT_ARGUMENT("ELI23796", tokens.size() > 0);

	// Convert each id to a long
	rvecIDs.clear();
	rvecIDs.reserve(tokens.size());
	for (unsigned int i = 0; i < tokens.size(); i++)
	{
		rvecIDs.push_back(asLong(tokens[i]));
	}
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_CanBeDeleted(BSTR strID, VARIANT_BOOL *bCanBeDeleted)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// Get the zone entity ids from the input entity id
		vector<long> ids;
		getIDs(strID, ids);

		// If the entity is already marked as used, then it can't be deleted.
		// Otherwise, it can
		*bCanBeDeleted = VARIANT_TRUE;
		for (unsigned int i = 0; i < ids.size(); i++)
		{
			if (getSpotRecognitionDlg()->isMarkedAsUsed(ids[i]))
			{
				*bCanBeDeleted = VARIANT_FALSE;
				break;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02857")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_Delete(BSTR strID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// Get the zone entity ids from the input entity id
		vector<long> ids;
		getIDs(strID, ids);

		// Delete the zone entities
		for (unsigned int i = 0; i < ids.size(); i++)
		{
			getSpotRecognitionDlg()->m_UCLIDGenericDisplayCtrl.deleteEntity(ids[i]);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02323")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_SetText(BSTR strID, BSTR strText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// Get the zone entity ids from the input entity id
		vector<long> ids;
		getIDs(strID, ids);

		// Get the text for each zone entity
		StringTokenizer tokenizer("\r\n");
		vector<string> tokens;
		tokens.reserve(ids.size());
		tokenizer.parse(asString(strText), tokens);
		ASSERT_ARGUMENT("ELI23801", tokens.size() >= ids.size());

		// Add any extra lines to the first zone entity
		unsigned int numLinesInFirstZone = tokens.size() - ids.size() + 1;
		string stdstrText = tokens[0];
		for (unsigned int i = 1; i < numLinesInFirstZone; i++)
		{
			stdstrText += "\r\n" + tokens[i];
		}
		getSpotRecognitionDlg()->setZoneEntityText(ids[0], stdstrText);

		// Set the text for the remaining zone entities
		for (unsigned int i = 1; i < ids.size(); i++)
		{
			getSpotRecognitionDlg()->setZoneEntityText(ids[i], tokens[i + numLinesInFirstZone - 1]);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02324")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_GetText(BSTR strID, BSTR *pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23829", pstrText != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();

		// Get the zone entity ids from the input entity id
		vector<long> ids;
		getIDs(strID, ids);

		string stdstrText = getSpotRecognitionDlg()->getZoneEntityText(ids[0]);
		for (unsigned int i = 1; i < ids.size(); i++)
		{
			stdstrText += "\r\n" + getSpotRecognitionDlg()->getZoneEntityText(ids[i]);
		}

		*pstrText = _bstr_t(stdstrText.c_str());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02331")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_CanBeMarkedAsUsed(BSTR strID, VARIANT_BOOL *pbCanBeMarkedAsUsed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23830", pbCanBeMarkedAsUsed != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();
		
		// as long as the ID is valid, the entity can be marked as used
		*pbCanBeMarkedAsUsed = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02330")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_MarkAsUsed(BSTR strID, VARIANT_BOOL bValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{		
		// try to ensure that this component is licensed.
		validateLicense();

		// Get the zone entity ids from the input entity id
		vector<long> ids;
		getIDs(strID, ids);

		bool bUsed = asCppBool(bValue);
		for (unsigned int i = 0; i < ids.size(); i++)
		{
			getSpotRecognitionDlg()->markAsUsed(ids[i], bUsed);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02325")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_IsMarkedAsUsed(BSTR strID, VARIANT_BOOL *pbIsMarkedAsUsed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23831", pbIsMarkedAsUsed != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();

		// Get the zone entity ids from the input entity id
		vector<long> ids;
		getIDs(strID, ids);

		// The input entity used if any of its zone entities are used.
		*pbIsMarkedAsUsed = VARIANT_FALSE;
		for (unsigned int i = 0; i < ids.size(); i++)
		{
			if (getSpotRecognitionDlg()->isMarkedAsUsed(ids[i]))
			{
				*pbIsMarkedAsUsed = VARIANT_TRUE;
				break;
			}
		}
			
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02326")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_IsFromPersistentSource(BSTR strID, VARIANT_BOOL * pbIsFromPersistentSource)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23832", pbIsFromPersistentSource != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();
			
		// as long as the ID is valid, the entity is indeed from a persistent source
		*pbIsFromPersistentSource = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02327")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_GetPersistentSourceName(BSTR strID, BSTR * pstrSourceName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23833", pstrSourceName != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();
		
		// the persistent source is the same as the image that is currently loaded
		// in the spot recognition window
		string strImageFileName = getSpotRecognitionDlg()->getImageFileName();
		*pstrSourceName = _bstr_t(strImageFileName.c_str()).copy();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02328")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_GetOCRImage(BSTR strID, BSTR* pbstrImageFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		validateLicense();

		// get current recognized zone image file, not the whole image file
		*pbstrImageFileName = _bstr_t(getSpotRecognitionDlg()->getCurrentZoneImageFileName().c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03000")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_HasBeenOCRed(BSTR strID, VARIANT_BOOL * pbHasBeenOCRed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23834", pbHasBeenOCRed != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();
			
		// as long as the ID is valid, the entity has been ocr'ed
		*pbHasBeenOCRed = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02329")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_HasIndirectSource(BSTR strID, VARIANT_BOOL *pbHasIndirectSource)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// no indirect source.
		*pbHasIndirectSource = VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03131")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_GetIndirectSource(BSTR strID, BSTR *pstrIndirectSourceName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		throw UCLIDException("ELI03153", "GetIndirectSource() is not implemented.");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03154")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_GetOCRZones(BSTR strID, IIUnknownVector **pRasterZones)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		ASSERT_ARGUMENT("ELI23846", pRasterZones != __nullptr);

		IIUnknownVectorPtr ipRasterZones(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI23797", ipRasterZones != __nullptr);

		// Get the zone entity ids from the input entity id
		vector<long> ids;
		getIDs(strID, ids);

		for (unsigned int i = 0; i < ids.size(); i++)
		{
			IRasterZonePtr ipRasterZone = getSpotRecognitionDlg()->getOCRZone(ids[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI23798", ipRasterZone != __nullptr);
			ipRasterZones->PushBack(ipRasterZone);
		}

		*pRasterZones = ipRasterZones.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03321")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_get_WindowShown(VARIANT_BOOL * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23835", pVal != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();
		
		// return the status of whether the dialog is currently visible
		// or not
		*pVal = getSpotRecognitionDlg()->IsWindowVisible() == TRUE ? 
				VARIANT_TRUE : VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02340")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_get_InputIsEnabled(VARIANT_BOOL * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		ASSERT_ARGUMENT("ELI23836", pVal != __nullptr);
			
		*pVal = getSpotRecognitionDlg()->inputIsEnabled() ? VARIANT_TRUE : VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02339")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_get_HasWindow(VARIANT_BOOL * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		ASSERT_ARGUMENT("ELI23837", pVal != __nullptr);
		
		// this method will always return true, as the spot recognition window
		// in the context of an input receiver always has a window
		*pVal = VARIANT_TRUE;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02338")
}
#include "windows.h"
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_get_WindowHandle(LONG * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		ASSERT_ARGUMENT("ELI23838", pVal != __nullptr);
		
		HWND hWnd = getSpotRecognitionDlg()->m_hWnd;
		*pVal = (long) hWnd;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02337")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_get_ParentWndHandle(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_lParentWndHandle;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03027")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_put_ParentWndHandle(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_lParentWndHandle = newVal;

		// if the dialog object has already been created, then update its parent
		if (m_apDlg.get())
		{
			CWnd *pParentWnd = m_lParentWndHandle == NULL ? NULL : CWnd::FromHandle((HWND) m_lParentWndHandle);
			m_apDlg->SetParent(pParentWnd);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03028")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_EnableInput(BSTR strInputType, 
													 BSTR strPrompt)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// enable input at the UI level
		getSpotRecognitionDlg()->enableInput(strInputType, strPrompt);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02333")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_DisableInput()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// disable input at the UI level
		getSpotRecognitionDlg()->disableInput();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02334")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_SetEventHandler(IIREventHandler * pEventHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->setEventHandler(pEventHandler);
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02335")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_get_UsesOCR(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Confirm that component is licensed
		validateLicense();

		*pVal = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03464")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_SetOCRFilter(IOCRFilter *pFilter)
{
	try
	{
		// Confirm that component is licensed
		validateLicense();

		// ensure that the filter object pointer is not NULL
		if (pFilter == NULL)
		{
			UCLIDException ue("ELI03589", "Invalid OCR filter.");
			ue.addDebugInfo("pFilter", "NULL");
			throw ue;
		}
		
		// pass on the filter to the dlg object
		getSpotRecognitionDlg()->setOCRFilter(pFilter);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03465")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_SetOCREngine(IOCREngine *pEngine)
{
	try
	{
		// Confirm that component is licensed
		validateLicense();
		
		// ensure that the Engine object pointer is not NULL
		if (pEngine == NULL)
		{
			UCLIDException ue("ELI03590", "Invalid OCR engine.");
			ue.addDebugInfo("pEngine", "NULL");
			throw ue;
		}

		// pass on the filter to the dlg object
		getSpotRecognitionDlg()->setOCREngine(pEngine);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03588")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_ShowWindow(VARIANT_BOOL bShow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->ShowWindow(bShow == VARIANT_TRUE ? SW_SHOW : SW_HIDE);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02336")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI19630", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Image viewer").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02341")
}
//-------------------------------------------------------------------------------------------------
SpotRecognitionDlg* CSpotRecognitionWindow::getSpotRecognitionDlg()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	if (m_apDlg.get() == NULL)
	{
		// try to ensure that this component is licensed.
		validateLicense();
		
		// create the dialog object, and bring it up in a modeless fashion
		// Provide OCR license status
		m_apDlg = unique_ptr<SpotRecognitionDlg>( new SpotRecognitionDlg( this, isOCRLicensed()) );
		if (!m_lParentWndHandle)
		{
			CWnd *pParentWnd = m_lParentWndHandle == NULL ? NULL : CWnd::FromHandle((HWND) m_lParentWndHandle);
			m_apDlg->createModeless(pParentWnd);
		}
		else
		{
			// create the modeless dialog with parent wnd set to m_lParentWndHandle
			m_apDlg->createModeless(CWnd::FromHandle((HWND)m_lParentWndHandle));
		}
	}
	
	return m_apDlg.get();
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetImageFileName(BSTR *pstrImageFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23842", pstrImageFileName != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();

		string strImageFileName = getSpotRecognitionDlg()->getImageFileName();
		*pstrImageFileName = _bstr_t(strImageFileName.c_str()).copy();
			
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02422")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetGDDFileName(BSTR *pstrGDDFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ASSERT_ARGUMENT("ELI23843", pstrGDDFileName != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();

		string strGDDFileName = getSpotRecognitionDlg()->getGDDFileName();
		*pstrGDDFileName = _bstr_t(strGDDFileName.c_str()).copy();
			
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02421")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::SetLineTextCorrector(ILineTextCorrector *pLineTextCorrector)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->setLineTextCorrector(pLineTextCorrector);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02769")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::SetLineTextEvaluator(ILineTextEvaluator *pLineTextEvaluator)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->setLineTextEvaluator(pLineTextEvaluator);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02770")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::SetParagraphTextCorrector(IParagraphTextCorrector *pParagraphTextCorrector)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->setParagraphTextCorrector(pParagraphTextCorrector);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02771")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::SetSubImageHandler(ISubImageHandler *pSubImageHandler, 
														BSTR strToolbarBtnTooltip,
														BSTR strTrainingFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->setSubImageHandler(pSubImageHandler, strToolbarBtnTooltip, 
			strTrainingFile);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03249")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::ClearParagraphTextHandlers()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->clearParagraphTextHandlers();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04752")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::SetParagraphTextHandlers(IIUnknownVector *pHandlers)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->setParagraphTextHandlers(pHandlers);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02796")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::SetSRWEventHandler(ISRWEventHandler *pHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->setSRWEventHandler(pHandler);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03070")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::get_AlwaysAllowHighlighting(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		*pVal = getSpotRecognitionDlg()->isAlwaysAllowHighlighting() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03236")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::put_AlwaysAllowHighlighting(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->setAlwaysAllowHighlighting(newVal == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19278")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::ShowOpenDialogBox()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->showOpenDialog();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03238")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::OpenImagePortion(BSTR strOriginalImageFileName, 
													  IRasterZone *pImagePortionInfo, 
													  double dRotationAngle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->openImagePortion( asString(strOriginalImageFileName), 
			pImagePortionInfo, dRotationAngle );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03242")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetLineTextCorrector(ILineTextCorrector **ppLineTextCorrector)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->getLineTextCorrector(
			(UCLID_SPOTRECOGNITIONIRLib::ILineTextCorrector**)ppLineTextCorrector);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03258")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetLineTextEvaluator(ILineTextEvaluator **ppLineTextEvaluator)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->getLineTextEvaluator(
			(UCLID_SPOTRECOGNITIONIRLib::ILineTextEvaluator**)ppLineTextEvaluator);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03259")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetParagraphTextCorrector(IParagraphTextCorrector **ppParagraphTextCorrector)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->getParagraphTextCorrector(
			(UCLID_SPOTRECOGNITIONIRLib::IParagraphTextCorrector**)ppParagraphTextCorrector);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03260")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetParagraphTextHandlers(IIUnknownVector **ppHandlers)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();
		
		getSpotRecognitionDlg()->getParagraphTextHandlers(ppHandlers);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03262")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetSRWEventHandler(ISRWEventHandler **ppHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->getSRWEventHandler(
			(UCLID_SPOTRECOGNITIONIRLib::ISRWEventHandler**)ppHandler);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03263")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetSubImageHandler(ISubImageHandler **ppSubImageHandler,
														BSTR *pstrToolbarBtnTooltip,
														BSTR *pstrTrainingFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->getSubImageHandler(
			(UCLID_SPOTRECOGNITIONIRLib::ISubImageHandler**)ppSubImageHandler, 
			pstrToolbarBtnTooltip,	pstrTrainingFile);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03276")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::IsImagePortionOpened(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		*pbValue = getSpotRecognitionDlg()->isImagePortionOpened() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03289")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetImagePortion(IRasterZone **pImagePortion)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->getImagePortion(pImagePortion);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03290")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::OCRCurrentPage(ISpatialString** ppSpatialString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// OCR the current page
		ISpatialStringPtr ipRecognizedStr = getSpotRecognitionDlg()->getCurrentPageText();
		
		// return the results to the caller
		*ppSpatialString = ipRecognizedStr.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06135")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::CreateZoneEntity(IRasterZone *pZone, 
													  long nColor, long *pID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// create the zone entity as specified and return
		// the zone entity ID
		*pID = getSpotRecognitionDlg()->createZoneEntity(pZone, nColor);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06556")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::DeleteZoneEntity(long nID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// delete the zone entity represented by the given ID
		getSpotRecognitionDlg()->deleteZoneEntity(nID);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06557")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::ZoomAroundZoneEntity(long nID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// zoom around the zone entity represented by the given ID
		getSpotRecognitionDlg()->zoomAroundZoneEntity(nID);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06558")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::CreateTemporaryHighlight(
	ISpatialString *pText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// temporarily highlight the specified spatial string
		getSpotRecognitionDlg()->createTemporaryHighlight(pText);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06579")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::DeleteTemporaryHighlight()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// delete any temporary highlights
		getSpotRecognitionDlg()->deleteTemporaryHighlight();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06581")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::NotifyKeyPressed(long nKeyCode)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		getSpotRecognitionDlg()->handleShortCutKeys(nKeyCode);
		// delete any temporary highlights
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11167")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetGenericDisplayOCX(IDispatch **pOCX)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		// return a reference to the underlying OCX control
		CUCLIDGenericDisplay& rOCX = 
			getSpotRecognitionDlg()->m_UCLIDGenericDisplayCtrl;
		IDispatchPtr ipDispatch = rOCX.GetControlUnknown();
		IDispatchPtr ipShallowCopy = ipDispatch;
		*pOCX = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10368")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::ShowToolbarCtrl(ESRIRToolbarCtrl eCtrl, VARIANT_BOOL bShow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();
		getSpotRecognitionDlg()->showToolbarCtrl(eCtrl, bShow == VARIANT_TRUE);

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11284")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::ShowTitleBar(VARIANT_BOOL bShow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();
		getSpotRecognitionDlg()->showTitleBar(bShow == VARIANT_TRUE);

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19395")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::ZoomPointWidth(long nX, long nY, long nWidth)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();
		getSpotRecognitionDlg()->zoomPointWidth(nX, nY, nWidth);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11391")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::EnableAutoOCR(VARIANT_BOOL bEnable)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();
		getSpotRecognitionDlg()->enableAutoOCR(bEnable == VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11525")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::get_WindowPos(ILongRectangle **ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		if (ppVal == NULL)
		{
			UCLIDException ue("ELI12055", "Unable to return window position.");
			throw ue;
		}

		RECT rect;
		getSpotRecognitionDlg()->GetWindowRect(&rect);
		ILongRectanglePtr ipRect(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI12058", ipRect != __nullptr);

		ipRect->Left = rect.left;
		ipRect->Right = rect.right;
		ipRect->Top = rect.top;
		ipRect->Bottom = rect.bottom;

		*ppVal = ipRect.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12054")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::put_WindowPos(ILongRectangle *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		ILongRectanglePtr ipRect(pNewVal);
		if (ipRect == __nullptr)
		{
			UCLIDException ue("ELI12056", "Window postion rectangle must not be NULL.");
			throw ue;
		}
		long nLeft = ipRect->Left;
		long nTop = ipRect->Top;

		getSpotRecognitionDlg()->MoveWindow(nLeft, nTop, ipRect->Right - nLeft, ipRect->Bottom - nTop);
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12057")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::LoadOptionsFromFile(BSTR bstrFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// try to ensure that this component is licensed.
		validateLicense();
		string strFileName = asString( bstrFileName );
		getSpotRecognitionDlg()->loadOptionsFromFile(strFileName);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12064")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::SetCurrentTool(ESRIRToolbarCtrl eCtrl)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// try to ensure that this component is licensed.
		validateLicense();

		ETool currTool = kNone;
		switch (eCtrl)
		{
		case kBtnOpenImage:
			currTool = kOpenImage;
			break;

		case kBtnSave:
			currTool = kSave;
			break;

		case kBtnZoomWindow:
			currTool = kZoomWindow;
			break;

		case kBtnZoomIn:
			currTool = kZoomIn;
			break;

		case kBtnZoomOut:
			currTool = kZoomOut;
			break;

		case kBtnFitPage:
			currTool = kFitPage;
			break;

		case kBtnFitWidth:
			currTool = kFitWidth;
			break;

		case kBtnPan:
			currTool = kPan;
			break;

		case kBtnSelectText:
			currTool = kSelectText;
			break;

		case kBtnSetHighlightHeight:
			currTool = kSetHighlightHeight;
			break;

		case kBtnEditZoneText:
			currTool = kEditZoneText;
			break;

		case kBtnDeleteEntities:
			currTool = kDeleteEntities;
			break;

		case kBtnSelectHighlight:
			currTool = kSelectHighlight;
			break;

		case kBtnPrint:
			currTool = kPrint;
			break;
		}
		getSpotRecognitionDlg()->setCurrentTool(currTool);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13103")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::IsOCRLicensed(VARIANT_BOOL *pbLicensed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI23844", pbLicensed != __nullptr);

		// Return response from local method
		*pbLicensed = isOCRLicensed() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15683")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::GetCurrentTool(ESRIRToolbarCtrl *peCtrl)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Try to ensure that this component is licensed.
		validateLicense();

		ETool currTool = getSpotRecognitionDlg()->getCurrentTool();
		switch (currTool)
		{

		case kOpenImage:
			*peCtrl = kBtnOpenImage;
			break;

		case kSave:
			*peCtrl = kBtnSave;
			break;

		case kZoomWindow:
			*peCtrl = kBtnZoomWindow;
			break;

		case kZoomIn:
			*peCtrl = kBtnZoomIn;
			break;

		case kZoomOut:
			*peCtrl = kBtnZoomOut;
			break;

		case kPan:
			*peCtrl = kBtnPan;
			break;

		case kSelectText:
		case kInactiveSelectText:
		case kSelectRectText:
		case kInactiveSelectRectText:
			*peCtrl = kBtnSelectText;
			break;

		case kSetHighlightHeight:
			*peCtrl = kBtnSetHighlightHeight;
			break;

		case kEditZoneText:
			*peCtrl = kBtnEditZoneText;
			break;

		case kDeleteEntities:
			*peCtrl = kBtnDeleteEntities;
			break;

		case kOpenSubImgInWindow:
			*peCtrl = kBtnOpenSubImgInWindow;
			break;

		case kPrint:
			*peCtrl = kBtnPrint;
			break;

		case kFitPage:
			*peCtrl = kBtnFitPage;
			break;

		case kFitWidth:
			*peCtrl = kBtnFitWidth;
			break;

		case kSelectHighlight:
			*peCtrl = kBtnSelectHighlight;
			break;

		default:
			UCLIDException ue("ELI23474", "Unexpected tool.");
			ue.addDebugInfo("Tool", currTool);
			throw ue;
		}
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23473")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::get_HighlightsAdjustableEnabled(VARIANT_BOOL* pvbEnable)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pvbEnable = asVariantBool(getSpotRecognitionDlg()->isHighlightsAdjustableEnabled());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23502")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::put_HighlightsAdjustableEnabled(VARIANT_BOOL vbEnable)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		getSpotRecognitionDlg()->enableHighlightsAdjustable(asCppBool(vbEnable));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23511")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::get_FittingMode(long* peFittingMode)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI23788", peFittingMode != __nullptr);

		*peFittingMode = getSpotRecognitionDlg()->getFittingMode();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23782")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::put_FittingMode(long eFittingMode)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		getSpotRecognitionDlg()->setFittingMode(eFittingMode);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23783")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecognitionWindow::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		ASSERT_ARGUMENT("ELI23845", pbValue != __nullptr);

		// try to ensure that this component is licensed.
		validateLicense();

		// if the above method call does not throw an exception, then this
		// component is licensed.
		*pbValue = VARIANT_TRUE;
	}
	catch (...)
	{
		// if we caught some exception, then this component is not licensed.
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private method
//-------------------------------------------------------------------------------------------------
void CSpotRecognitionWindow::validateLicense()
{
	static const unsigned long ulTHIS_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( ulTHIS_COMPONENT_ID, "ELI02775", 
		"Extract Image Viewer" );
}
//-------------------------------------------------------------------------------------------------
void CSpotRecognitionWindow::validateOCRLicense()
{
	static const unsigned long OCR_LICENSE_ID = gnOCR_ON_CLIENT_FEATURE;

	VALIDATE_LICENSE( OCR_LICENSE_ID, "ELI15681", "Extract Image Viewer OCR" );
}
//-------------------------------------------------------------------------------------------------
bool CSpotRecognitionWindow::isOCRLicensed()
{
	bool bLicensed = true;

	try
	{
		// Check OCR license
		validateOCRLicense();
	}
	catch (...)
	{
		// OCR functionality is not licensed
		bLicensed = false;
	}

	return bLicensed;
}
//-------------------------------------------------------------------------------------------------
