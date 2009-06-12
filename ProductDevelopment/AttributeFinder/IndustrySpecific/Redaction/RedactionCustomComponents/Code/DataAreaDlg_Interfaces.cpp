// DataAreaDlg_Interfaces.cpp : implementation of the DataAreaDlg interfaces

#include "stdafx.h"
#include "DataAreaDlg.h"

#include <SRWShortcuts.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <MiscLeadUtils.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const long  gnREDACTED_ZONE_COLOR = RGB(100, 149, 237);

//-------------------------------------------------------------------------------------------------
// IUnknown
//------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) CDataAreaDlg::AddRef()
{
	InterlockedIncrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) CDataAreaDlg::Release()
{
	InterlockedDecrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataAreaDlg::QueryInterface( REFIID iid, void FAR* FAR* ppvObj)
{
	ISRWEventHandler *pTmp = this;

	if (iid == IID_IUnknown)
		*ppvObj = static_cast<IUnknown *>(pTmp);
	else if (iid == IID_ISRWEventHandler)
		*ppvObj = static_cast<ISRWEventHandler *>(this);
	else
		*ppvObj = NULL;

	if (*ppvObj != NULL)
	{
		AddRef();
		return S_OK;
	}
	else
	{
		return E_NOINTERFACE;
	}
}

//-------------------------------------------------------------------------------------------------
// ISRWEventHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataAreaDlg::raw_AboutToRecognizeParagraphText()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataAreaDlg::raw_AboutToRecognizeLineText()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataAreaDlg::raw_NotifyKeyPressed(long nKeyCode, short shiftState)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Process character unless active Confirmation dialog
		if (!m_bActiveConfirmationDlg)
		{
			// Handle or ignore the character
			handleCharacter( nKeyCode );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11313")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataAreaDlg::raw_NotifyCurrentPageChanged()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Process page-change unless active Confirmation dialog
		if (!m_bActiveConfirmationDlg)
		{
			// Get the new page number
			long lNewPage = m_ipSRIR->GetCurrentPageNumber();

			// Move to this page and select first item on the page
			setPage( lNewPage, true );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11314")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataAreaDlg::raw_NotifyEntitySelected(long nZoneID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Step through Data Settings objects
		long lCount = m_vecDataItems.size();
		int i;
		for (i = 0; i < lCount; i++)
		{
			// Retrieve this Settings object
			DataDisplaySettings* pDDS = m_vecDataItems[i].m_pDisplaySettings;
			ASSERT_RESOURCE_ALLOCATION("ELI19508", pDDS != NULL);

			// Get associated Zone IDs
			std::vector<long> vecZoneIDs = pDDS->getHighlightIDs();
			unsigned int uj;
			for (uj = 0; uj < vecZoneIDs.size(); uj++)
			{
				// Check this ID
				if (vecZoneIDs[uj] == nZoneID)
				{
					// Found the desired Settings object
					// NOTE: Data grid is 1-relative, vector is 0-relative
					long nIndex = getGridIndex(i);
					if (nIndex == -1)
					{
						// it should never be the case that you have a zoneID
						// that is not related to an element in the DDA
						THROW_LOGIC_ERROR_EXCEPTION("ELI20259");
					}
				
					// Select this item, but don't auto zoom [FlexIDSCore #3404]
					selectDataRow(nIndex + 1, false);

					// Refresh display
					refreshDataRow(nIndex + 1, pDDS, false);

					// found a matching zone, break from the for loop
					break;
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11366")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataAreaDlg::raw_NotifyFileOpened(BSTR /*bstrFileFullPath*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Do nothing at this time
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12991")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataAreaDlg::raw_NotifyOpenToolbarButtonPressed(VARIANT_BOOL *pbContinueWithOpen)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbContinueWithOpen == NULL)
		{
			return E_POINTER;
		}

		// Allow File - Open to continue unless active Confirmation dialog
		if (m_bActiveConfirmationDlg)
		{
			*pbContinueWithOpen = VARIANT_FALSE;
		}
		else
		{
			*pbContinueWithOpen = VARIANT_TRUE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13108")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataAreaDlg::raw_NotifyZoneEntityMoved(long nZoneID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the currently selected data item. (This corresponds to the one that moved.)
		DataItem dataItem = m_vecDataItems[m_vecVerifyAttributes[m_iDataItemIndex - 1]];

		// Get the raster zones associated with this attribute
		ISpatialStringPtr ipOldValue = dataItem.m_ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI23551", ipOldValue != NULL);
		IIUnknownVectorPtr ipRasterZones = ipOldValue->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI23552", ipRasterZones != NULL);
		
		// Get the vector of highlight ids (indexed by raster zone id)
		vector<long> vecHighlightIds = dataItem.m_pDisplaySettings->getHighlightIDs();

		// Search for the raster zone that corresponds to this zone entity id
		for (vector<long>::size_type i = 0; i < vecHighlightIds.size(); i++)
		{
			if (vecHighlightIds[i] == nZoneID)
			{
				// Update the raster zones with the new spatial information
				IRasterZonePtr ipRasterZone = getZoneFromID(nZoneID);
				ASSERT_RESOURCE_ALLOCATION("ELI23553", ipRasterZone != NULL);
				ipRasterZones->Set(i, ipRasterZone);

				// Create a new spatial string.
				// Note: If the spatial string was spatial before, it is now hybrid.
				ISpatialStringPtr ipNewValue(CLSID_SpatialString);
				ASSERT_RESOURCE_ALLOCATION("ELI23554", ipNewValue);
				ipNewValue->CreateHybridString(ipRasterZones, ipOldValue->String, 
					ipOldValue->SourceDocName, ipOldValue->SpatialPageInfos);

				// Store the new attribute value
				dataItem.m_ipAttribute->Value = ipNewValue;

				// Set the dirty flag
				m_bChangesMadeForHistory = true;

				// We are done
				return S_OK;
			}
		}

		// If we reached this point, no raster zone corresponded to the moved zone entity
		UCLIDException ue("ELI23549", "Invalid zone entity id.");
		ue.addDebugInfo("Zone entity id", nZoneID);
		throw ue;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23523")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataAreaDlg::raw_NotifyZoneEntitiesCreated(IVariantVector *pZoneIDs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Ignore entity creation if active Confirmation dialog
		if (m_bActiveConfirmationDlg)
		{
			return S_OK;
		}

		// Use a smart pointer
		IVariantVectorPtr variantVector(pZoneIDs);
		ASSERT_RESOURCE_ALLOCATION("ELI23762", pZoneIDs != NULL);

		// Create a vector of the zone IDs
		long lSize = variantVector->Size;
		vector<long> vecIDs;
		vecIDs.reserve(lSize);
		for (long i = 0; i < lSize; i++)
		{
			_variant_t var = variantVector->Item[i];
			vecIDs.push_back(var.lVal);
		}

		// Add the zone entities to the data area dialog
		addManualRedactions(vecIDs);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23761")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
