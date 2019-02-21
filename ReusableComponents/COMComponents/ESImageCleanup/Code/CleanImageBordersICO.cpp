// CleanImageBordersICO.cpp : Implementation of CCleanImageBordersICO

#include "stdafx.h"
#include "CleanImageBordersICO.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <MiscLeadUtils.h>
#include <LeadToolsLicenseRestrictor.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CCleanImageBordersICO
//-------------------------------------------------------------------------------------------------
CCleanImageBordersICO::CCleanImageBordersICO() :
m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CCleanImageBordersICO::~CCleanImageBordersICO()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17741");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ICleanImageBordersICO,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
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
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17768", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("Clean borders").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17742")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// nothing to copy
	try
	{
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17743");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::raw_Clone(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17722", pObject != __nullptr);

		// create a copyable object pointer
		ICopyableObjectPtr ipObjCopy(CLSID_CleanImageBordersICO);
		ASSERT_RESOURCE_ALLOCATION("ELI17744", ipObjCopy != __nullptr);

		// set the IUnknownPtr to the current object
		IUnknownPtr ipUnk = this;
		ASSERT_RESOURCE_ALLOCATION("ELI17745", ipUnk != __nullptr);

		// copy to the copyable object pointer
		ipObjCopy->CopyFrom(ipUnk);

		// return the new CleanImageBordersICO object 
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17746");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17747", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17748");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17723", pClassID != __nullptr);

		*pClassID = CLSID_CleanImageBordersICO;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17724");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17725", pStream != __nullptr);

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// read the version number from the stream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check the version number
		if (nDataVersion > gnCurrentVersion)
		{
			UCLIDException ue("ELI17749", "Unable to load newer CleanImageBordersICO.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17750");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (pStream == NULL)
		{
			return E_POINTER;
		}

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// write the version number to the stream
		dataWriter << gnCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (asCppBool(fClearDirty))
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17751");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17726", pcbSize != __nullptr);

		return E_NOTIMPL;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17727");
}

//-------------------------------------------------------------------------------------------------
// IImageCleanupOperation Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCleanImageBordersICO::Perform(void* pciRepair)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	INIT_EXCEPTION_AND_TRACING("MLI00039");

	try
	{
		// validate License first
		validateLicense();
		_lastCodePos = "10";

		// wrap the ClearImage repair pointer in smart pointer
		ICiRepairPtr ipciRepair((ICiRepair*) pciRepair);
		ASSERT_RESOURCE_ALLOCATION("ELI17752", ipciRepair != __nullptr);

		ICiImagePtr ipciImage = ipciRepair->Image;
		ASSERT_RESOURCE_ALLOCATION("ELI20321", ipciRepair->Image != __nullptr);
		_lastCodePos = "20";

		// the following code seems a bit convoluted but it is due to a problem with
		// Inlite's BorderExtract method.  a side effect of the BorderExtract method
		// is that it will shift the image 4 pixels left and up.  the following
		// code is a work around for this based on Inlite's support teams suggestion:
		// --------------------------------------------------------------------------
		//	The following code should serve your purpose (in VB6):
		//
		//	Sub BorderExtractSpec (ByRef oImageIn as CiImage)
		//	  Repair.Image = oImageIn.Duplicate
		//	  Repair.BorderExtract ciBexBorder, ciBeaCleaner
		//  
		//	  Tools.Image = oImageIn
		//	  Tools.Image.Clear
		//	  Tools.PasteImage Repair.Image, 4, 4
		//	End sub
		// 
		//	Best regards,
		//
		//	Gene Manheim
		//	Inlite Research, Inc.
		//	Email: support@inliteresearch.com
		// --------------------------------------------------------------------------
		// [p13 4824] - BorderCleanup shifts image
		// as per another support email, we also need to check the image width and 
		// height to determine if the border has been cleaned.  If a border extraction
		// has occurred the image will be smaller otherwise it will be unchanged
		// --------------------------------------------------------------------------
		// All of this is supposed to be fixed in the next version from inlite
		// Current version: 5.4.15
		// 01/31/2008 - received version 5.5.5
		//		Ver. 5.5.5 fixed the offset issue, but only cleaned the vertical border
		//		not the horizontal border
		// --------------------------------------------------------------------------

		// as per [p13 #4824] store the image size for later comparison
		long nWidth = ipciImage->Width;
		long nHeight = ipciImage->Height;
		_lastCodePos = "30";

		// clean the borders (if a border is removed it may change the size of the image 
		// if it does then the image will be shifted 4 pixels left and up)
		ipciRepair->BorderExtract(ciBexBorder, ciBeaCleaner);
		_lastCodePos = "40";

		// now check the height and width and only paste the image with an offset if
		// either changed [p13 #4824]
		if (ipciImage->Height != nHeight || ipciImage->Width != nWidth)
		{
			// declare the bitmap handles here so the memory can be released if
			// there is an exception
			BITMAPHANDLE hOldImage;
			BITMAPHANDLE hNewImage;
			bool bOldAllocated = false;
			bool bNewAllocated = false;
			_lastCodePos = "40_10";

			try
			{
				try
				{
					// as per [p13 #4833] modified to use LeadTools for the image pasting
					//------------------------------------------------
					// load the image into LeadTools
					//------------------------------------------------

					// create a new lead tools bitmap
					{
						LeadToolsLicenseRestrictor leadToolsLicenseGuard;

						throwExceptionIfNotSuccess(L_CreateBitmap(&hOldImage, sizeof(hOldImage),
							TYPE_CONV, ipciImage->Width, ipciImage->Height,
							ipciImage->BitsPerPixel, ORDER_RGB, NULL, TOP_LEFT, NULL, 0),
							"ELI20338", "Failed to create the temporary bitmap!");

						bOldAllocated = true;
						_lastCodePos = "40_20";

						// lock the bitmap so its memory may be accessed
						L_AccessBitmap(&hOldImage);
						_lastCodePos = "40_30";
					}

					// save the ClearImage image to memory
					_variant_t vImageFile = ipciImage->SaveToMemory();
					_lastCodePos = "40_40";

					// get a pointer to the memory
					L_UCHAR* pImage = NULL;
					SafeArrayAccessData(vImageFile.parray, (void**) &pImage);
					_lastCodePos = "40_50";

					// get the row size
					unsigned long ulRowSize = ipciImage->LineBytes;
					_lastCodePos = "40_60";



					{
						LeadToolsLicenseRestrictor leadToolsLicenseGuard;

						_lastCodePos = "40_70";
						
						// copy the image from ClearImage to LeadTools
						for (long i = 0; i < hOldImage.Height; i++)
						{
							L_INT32 nRet = (L_INT32)L_PutBitmapRow(&hOldImage,
								(pImage + (i * ulRowSize)), i, ulRowSize);
							if (nRet < 0)
							{
								throwExceptionIfNotSuccess(
									nRet, "ELI20339", "Failed copying row of image!");
							}
							_lastCodePos = "40_70 #" + asString(i);
						}
					}
					_lastCodePos = "40_80";
					
					// done accessing the data from ClearImage unlock and close the image
					SafeArrayUnaccessData(vImageFile.parray);
					ipciImage->Close();
					_lastCodePos = "40_90";

					{
						LeadToolsLicenseRestrictor leadToolsLicenseGuard;

						// unlock the LeadTools memory
						L_ReleaseBitmap(&hOldImage);
						_lastCodePos = "40_100";

						// create a new LeadTools bitmap to paste the image to
						throwExceptionIfNotSuccess(L_CreateBitmap(&hNewImage, sizeof(hNewImage),
							TYPE_CONV, nWidth, nHeight, hOldImage.BitsPerPixel,
							ORDER_RGB, NULL, TOP_LEFT, NULL, 0),
							"ELI20340", "Failed to create a new bitmap!");
						bNewAllocated = true;
						_lastCodePos = "40_110";

						// clear the image
						throwExceptionIfNotSuccess(L_ClearBitmap(&hNewImage), "ELI20341",
							"Unable to clear the bitmap!");
						_lastCodePos = "40_120";

						// now paste back with an offset
						throwExceptionIfNotSuccess(L_CombineBitmap(&hNewImage, 4, 4,
							hOldImage.Width, hOldImage.Height, &hOldImage, 0, 0,
							CB_DST_NOP | CB_RES_MASTER | CB_SRC_MASTER | CB_DST_MASTER | CB_OP_OR, 0),
							"ELI20342", "Unable to paste the bitmap!");
						_lastCodePos = "40_130";

						// free the old bitmap
						L_FreeBitmap(&hOldImage);
						bOldAllocated = false;
						_lastCodePos = "40_140";
					}

					//------------------------------------------------
					// copy the image back to ClearImage
					//------------------------------------------------

					// get the size of the memory chunk for the bitmap
					unsigned long ulMemorySize = (unsigned long) hNewImage.Size;
					_lastCodePos = "40_150";

					// get the pointer to the bitmap in memory
					L_UCHAR* pBuf = hNewImage.Addr.Windows.pData;
					_lastCodePos = "40_160";

					// prepare the variant and safe array for loading from memory
					SAFEARRAYBOUND saBounds = {ulMemorySize, 0};
					vImageFile.vt = (VT_ARRAY | VT_UI1);
					vImageFile.parray = SafeArrayCreate(VT_UI1, 1, &saBounds);
					_lastCodePos = "40_170";

					// copy the bitmap into the safe array
					SafeArrayAccessData(vImageFile.parray, (void**) &pImage);
					memcpy(pImage, pBuf, ulMemorySize);
					SafeArrayUnaccessData(vImageFile.parray);
					_lastCodePos = "40_180";

					// create a ClearImage server (we need this to create an image object)
					ICiServerPtr ipciServer(CLSID_CiServer);
					ASSERT_RESOURCE_ALLOCATION("ELI17795", ipciServer != __nullptr);
					_lastCodePos = "40_190";

					// create the ClearImage image
					ipciImage = ipciServer->CreateImage();
					ASSERT_RESOURCE_ALLOCATION("ELI20343", ipciImage != __nullptr);
					ipciImage->CreateBpp(hNewImage.Width, hNewImage.Height, hNewImage.BitsPerPixel);
					_lastCodePos = "40_200";

					// load the LeadTools bmp into the ClearImage Image
					ipciImage->LoadFromMemory(vImageFile);
					_lastCodePos = "40_210";

					// set the repair pointer to the new image
					ipciRepair->Image = ipciImage;
					_lastCodePos = "40_220";

					{
						LeadToolsLicenseRestrictor leadToolsLicenseGuard;
						// free the new bitmap
						L_FreeBitmap(&hNewImage);
						bNewAllocated = false;
						_lastCodePos = "40_230";
					}
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20344");
			}
			catch(UCLIDException& ue)
			{
				if (bOldAllocated || bNewAllocated)
				{
					LeadToolsLicenseRestrictor leadToolsLicenseGuard;
					if (bOldAllocated)
					{
						L_FreeBitmap(&hOldImage);
					}

					if (bNewAllocated)
					{
						L_FreeBitmap(&hNewImage);
					}
				}

				throw ue;
			}
		}
		_lastCodePos = "50";
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17753");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CCleanImageBordersICO::validateLicense()
{
	VALIDATE_LICENSE( gnIMAGE_CLEANUP_ENGINE_FEATURE, "ELI17754", "Clean Image Borders ICO" );
}
//-------------------------------------------------------------------------------------------------
