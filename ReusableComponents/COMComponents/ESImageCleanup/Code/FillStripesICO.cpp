// FillStripesICO.cpp : Implementation of CFillStripesICO

#include "stdafx.h"
#include "FillStripesICO.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <SafeTwoDimensionalArray.h>

#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

// array of bit masks to use to check for a 1 bit (white pixel)
// 10000000 = 128
// 01000000 = 64
// 00100000 = 32
// 00010000 = 16
// 00001000 = 8
// 00000100 = 4
// 00000010 = 2
// 00000001 = 1
static unsigned char cCheckMaskArray[8] = {128, 64, 32, 16, 8, 4, 2, 1};

// array of bit masks to use to set a 0 bit (black pixel)
// 01111111 = 127
// 10111111 = 191
// 11011111 = 223
// 11101111 = 239
// 11110111 = 247
// 11111011 = 251
// 11111101 = 253
// 11111110 = 254
static unsigned char cSetMaskArray[8] = {127, 191, 223, 239, 247, 251, 253, 254};

//-------------------------------------------------------------------------------------------------
// CFillStripesICO
//-------------------------------------------------------------------------------------------------
CFillStripesICO::CFillStripesICO() :
m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CFillStripesICO::~CFillStripesICO()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19047");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFillStripesICO::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFillStripesICO,
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
STDMETHODIMP CFillStripesICO::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19048", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("Fill stripes").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19049")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFillStripesICO::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// nothing to copy
	try
	{
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19050");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFillStripesICO::raw_Clone(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19051", pObject != __nullptr);

		// create a copyable object pointer
		ICopyableObjectPtr ipObjCopy(CLSID_FillStripesICO);
		ASSERT_RESOURCE_ALLOCATION("ELI19052", ipObjCopy != __nullptr);

		// set the IUnknownPtr to the current object
		IUnknownPtr ipUnk = this;
		ASSERT_RESOURCE_ALLOCATION("ELI19053", ipUnk != __nullptr);

		// copy to the copyable object pointer
		ipObjCopy->CopyFrom(ipUnk);

		// return the new FillStripesICO object 
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19054");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFillStripesICO::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19055", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19056");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFillStripesICO::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19057", pClassID != __nullptr);

		*pClassID = CLSID_FillStripesICO;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19058");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFillStripesICO::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFillStripesICO::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19059", pStream != __nullptr);

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
			UCLIDException ue("ELI19060", "Unable to load newer FillStripesICO.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19061");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFillStripesICO::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19079", pStream != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19062");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFillStripesICO::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19063", pcbSize != __nullptr);

		return E_NOTIMPL;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19064");
}

//-------------------------------------------------------------------------------------------------
// IImageCleanupOperation Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFillStripesICO::Perform(void* pciRepair)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	INIT_EXCEPTION_AND_TRACING("MLI00022");

	try
	{
		// validate License first
		validateLicense();
		_lastCodePos = "10";

		// wrap the ClearImage repair pointer in smart pointer
		ICiRepairPtr ipciRepair((ICiRepair*) pciRepair);
		ASSERT_RESOURCE_ALLOCATION("ELI19065", ipciRepair != __nullptr);
		_lastCodePos = "20";

		// ensure that there is an image in the repair pointer
		ICiImagePtr ipImage = ipciRepair->Image;
		ASSERT_RESOURCE_ALLOCATION("ELI19066", ipImage != __nullptr);
		_lastCodePos = "30";

		// only works for bitonal images
		if (ipImage->BitsPerPixel == 1)
		{
			long lBytesPerLine = ipImage->LineBytes;
			long lWidth = ipImage->Width;
			long lHeight = ipImage->Height;
			_lastCodePos = "40";

			// save the image into memory then close the image
			_variant_t vtImage = ipImage->SaveToMemory();
			_lastCodePos = "50";
			ipImage->Close();
			_lastCodePos = "60";

			// get a pointer to the image in memory and lock the data so we can manipulate it
			unsigned char* pImage = NULL;
			SafeArrayAccessData(vtImage.parray, (void**) &pImage);
			_lastCodePos = "70";

			// create a vector to hold the number of black pixels in each column
			vector<long> vecColumns(lBytesPerLine*8);
			_lastCodePos = "80";

			// get the number of black pixels in each column
			countBlackPixlesInColumns(vecColumns, lBytesPerLine, lHeight, pImage);
			_lastCodePos = "90";

			// fill in any striped area (any column that contains no black pixels)
			fillInStripedArea(vecColumns, lBytesPerLine, lHeight, lWidth, pImage);
			_lastCodePos = "100";

			// unlock the safe array
			SafeArrayUnaccessData(vtImage.parray);
			_lastCodePos = "110";

			// create a new image
			ipImage->CreateBpp(lWidth, lHeight, 1);
			_lastCodePos = "120";

			// load the modified image from memory
			ipImage->LoadFromMemory(vtImage);
			_lastCodePos = "130";
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19067");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CFillStripesICO::validateLicense()
{
	VALIDATE_LICENSE( gnIMAGE_CLEANUP_ENGINE_FEATURE, "ELI19068", "Fill Stripes ICO" );
}
//-------------------------------------------------------------------------------------------------
void CFillStripesICO::countBlackPixlesInColumns(vector<long>& rvecColumns, long lBytesPerLine, 
										long lHeight, unsigned char* pImage)
{
	ASSERT_ARGUMENT("ELI19069", pImage != __nullptr);

	// loop through each byte in a line
	for (long lByteInRow=0; lByteInRow < lBytesPerLine; lByteInRow++)
	{
		// for each byte in the line loop through all of the rows
		for (long lRow=0; lRow < lHeight; lRow++)
		{
			// get the current byte based on the row and byte position
			unsigned char cByte = getByteFromImage(lRow, lByteInRow, lBytesPerLine, pImage);
			
			// check each pixel in the byte
			for (long lBit=0; lBit < 8; lBit++)
			{
				if ((cByte & cCheckMaskArray[lBit]) == 0)
				{
					// there are lByteInRow * 8 columns in the image
					// the current column can be computed by adding the current bit
					// position to the current byte * 8
					// (i.e. if lBit = 1 and lByteInRow = 2 then the column is 17 since
					// the 17th column of the image is the first (leftmost) bit in the
					// third byte)
					rvecColumns[lBit + (lByteInRow*8)]++;
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CFillStripesICO::fillInStripedArea(vector<long> &rvecColumns, long lBytesPerLine, 
										long lHeight, long lWidth, unsigned char *pImage)
{
	ASSERT_ARGUMENT("ELI19070", pImage != __nullptr);

	// ignore the first and last columns on the image
	for (long lColumn=1; lColumn < lWidth-1; lColumn++)
	{
		// if the count is 0 then there were no black pixels in the column
		// if there are no black pixels this very well may be a striped
		// column, need to look down the column to the left and right.
		// if there are black pixels on both sides then change this pixel
		// to black as well
		if (rvecColumns[lColumn] == 0)
		{
			// make sure there are black pixels in the columns to the left and right
			if (rvecColumns[lColumn-1] != 0 && rvecColumns[lColumn+1] != 0)
			{
				for (long lRow=0; lRow < lHeight; lRow++)
				{
					// if both left and right pixels are black make this pixel black as well
					if (isLeftPixelBlack(lRow, lColumn, lBytesPerLine, pImage) &&
						isRightPixelBlack(lRow, lColumn, lBytesPerLine, pImage))
					{
						// compute the current byte and bit containing this column
						long lColumnBytePosition = lColumn/8;
						long lCurrentBit = lColumn%8;

						// get a pointer to the byte containing the pixel
						unsigned char* pcByte = getPointerToByteFromImage(lRow, 
							lColumnBytePosition, lBytesPerLine, pImage);

						// set the pixel to black
						*pcByte = (getByteFromImage(lRow, lColumnBytePosition, 
							lBytesPerLine, pImage) & cSetMaskArray[lCurrentBit]);
					} // else there is no black pixel on the left and/or the right
				} // end for each row
			} // else there are no black pixels in either the left column or right column
		} // else there is at least one black pixel in the column
	} // end for each column
}
//-------------------------------------------------------------------------------------------------
bool CFillStripesICO::isLeftPixelBlack(long lRow, long lColumn, long lBytesPerLine, 
									   unsigned char *pImage)
{
	ASSERT_ARGUMENT("ELI19071", pImage != __nullptr);
	ASSERT_ARGUMENT("ELI19082", lColumn != 0); // cannot be leftmost column

	bool bReturn = false;

	// get the current byte and bit position
	long lCurrentByte = lColumn / 8;
	long lBitPosition = lColumn % 8;

	unsigned char cTemp = 0;

	// get the byte and mask the bit for comparison
	// check for first bit (leftmost bit)
	if (lBitPosition == 0)
	{
		// if position is first bit in the byte need to look at the last bit of 
		// previous byte and check its rightmost bit
		cTemp = getByteFromImage(lRow, lCurrentByte-1, lBytesPerLine, pImage);
		cTemp &= cCheckMaskArray[7];
	}
	else
	{
		// get the current byte and look at the bit to the left of the current bit
		cTemp = getByteFromImage(lRow, lCurrentByte, lBytesPerLine, pImage);
		cTemp &= cCheckMaskArray[lBitPosition-1];
	}
	
	// if cTemp == 0 then there is a black pixel in the left bit position
	bReturn = (cTemp == 0);

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
bool CFillStripesICO::isRightPixelBlack(long lRow, long lColumn, long lBytesPerLine, 
										unsigned char *pImage)
{
	ASSERT_ARGUMENT("ELI19072", pImage != __nullptr);
	ASSERT_ARGUMENT("ELI19083", lColumn != ((lBytesPerLine * 8) - 1)); // cannot be rightmost column

	bool bReturn = false;

	// get the current byte and bit position
	long lCurrentByte = lColumn / 8;
	long lBitPosition = lColumn % 8;

	unsigned char cTemp = 0;

	// get the current byte, mask the appropriate bit and compare
	// check for the last bit (right most bit)
	if (lBitPosition == 7)
	{
		// if position is last bit then need to look at the first bit 
		// (leftmost bit) of the next byte
		cTemp = getByteFromImage(lRow, lCurrentByte+1, lBytesPerLine, pImage);
		cTemp &= cCheckMaskArray[0];
	}
	else
	{
		// get the current byte and look at the bit to the right of the current bit
		cTemp = getByteFromImage(lRow, lCurrentByte, lBytesPerLine, pImage);
		cTemp &= cCheckMaskArray[lBitPosition+1];
	}

	// if cTemp == 0 then there is a black pixel in the right bit position
	bReturn = (cTemp == 0);

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
unsigned char CFillStripesICO::getByteFromImage(long lRow, long lByteInColumn, long lBytesPerLine, 
												unsigned char *pImage)
{
	ASSERT_ARGUMENT("ELI19074", pImage != __nullptr);

	return *(pImage + (lByteInColumn + (lRow * lBytesPerLine)));
}
//-------------------------------------------------------------------------------------------------
unsigned char* CFillStripesICO::getPointerToByteFromImage(long lRow, long lByteInColumn, 
													  long lBytesPerLine, unsigned char *pImage)
{
	ASSERT_ARGUMENT("ELI19075", pImage != __nullptr);

	return (pImage + (lByteInColumn + (lRow * lBytesPerLine)));
}
//-------------------------------------------------------------------------------------------------