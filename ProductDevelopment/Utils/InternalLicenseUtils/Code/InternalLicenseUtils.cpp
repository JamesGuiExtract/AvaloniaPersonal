// LicenseUtils.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "InternalLicenseUtils.h"

#include <BaseUtils.h>
#include <ByteStreamManipulator.h>
#include <EncryptionEngine.h>
#include <UCLIDException.h>
#include <Random.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Define four UCLID passwords used for encrypting the password
// NOTE: These passwords were not exposed at the header file level because
//		 no user of this class needs to know that these passwords exist
// These passwords are also uses in the FileProcessingDB.cpp
const unsigned long	gulFAMKey1 = 0x78932517;
const unsigned long	gulFAMKey2 = 0x193E2224;
const unsigned long	gulFAMKey3 = 0x20134253;
const unsigned long	gulFAMKey4 = 0x15990323;

//--------------------------------------------------------------------------------------------------
// FILE-SCOPE FUNCTIONS
//--------------------------------------------------------------------------------------------------
// NOTE: This function is purposely not exposed at header file level as a class
//		 method, as no user of this class needs to know that such a function
//		 exists.
void getFAMPassword(ByteStream& rPasswordBytes)
{
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, rPasswordBytes);
	
	bsm << gulFAMKey1;
	bsm << gulFAMKey2;
	bsm << gulFAMKey3;
	bsm << gulFAMKey4;
	bsm.flushToByteStream(8);
}

// This method re-orders bytes in the array based on nScrambleKey. This does not encrypt, but
// ensures that when the scrambled data is encrypted all portions of the encrypted result will
// appear completely different across multiple attempts even when the source data contains largely
// the same data.
// When bScramble is false, data that was previously scrambled will be unscrambled (assuming the
// same nScrambleKey is used as when the data was scrambled).
void scrambleData(unsigned char* pszData, unsigned long nLength, unsigned long nScrambleKey,
				  bool bScramble)
{
	// For every other byte in the array, swap it with a byte that is half the array
	// size ahead +- 8 bytes. This position to swap with should roll over to the
	// beginning of the array once it gets past the end of it.
	// When unscrambling, swap the same bytes, but in the opposite order (start with the last even
	// numbered index in the array and work backwards).
	long nStart = bScramble ? 0 : (nLength - 1) - ((nLength - 1) % 2);
	long nIncrement = bScramble ? 2 : -2;
	for (long i = nStart; (unsigned long)i < nLength; i += nIncrement)
	{
		// Random offset will be a number between 0 and 15 based on i and nScrambleKey.
		unsigned long nRandomOffset = (nScrambleKey >> (i % 32)) & 0xF;
		// Use the random offset to pick the byte to swap with.
		unsigned long nSwapPos = i + (nLength / 2) - 8 + nRandomOffset;
		// Roll over to the beginning of the array once past the end of the array.
		nSwapPos = nSwapPos % nLength;

		swap(pszData[i], pszData[nSwapPos]);
	}
}

//-------------------------------------------------------------------------------------------------
// PURPOSE:		Exported encrypt method for P/Invoke calls from C# to encrypt/decrypt data using
//				the password for secure FAM counters. This process includes an extra step to
//				randomly scramble the bytes of the data before encrypting so that encrypted results
//				are not mostly the same for data that is mostly the same.
// ARGS:
// pszInput-	Pointer to the bytes to be encrypted/decrypted
// bEncrypt-	true to encrypt pszInput; otherwise, false.
// pulLength-	Should be the number of pszInput bytes to encrypt/decrypt when called. The value
//				will be modified to specify the number of encrypted/decrypted bytes returned.
//				Must be a multiple of 8.
// RETURNS:		The encrypted bytes. The caller is responsible for freeing the buffer using
//				Marshal.FreeCoTaskMem().
//-------------------------------------------------------------------------------------------------
unsigned char* externManipulator(unsigned char* pszInput, bool bEncrypt, unsigned long* pulLength)
{
	unsigned char* pszOutput = __nullptr;

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI38791", pszInput != __nullptr);
			ASSERT_ARGUMENT("ELI38792", pulLength != __nullptr);

			unsigned long nScrambleKey;
			ByteStream bytes;

			if (bEncrypt)
			{
				// In order to avoid having encrypted results be mostly the same for data that is
				// mostly the same, scramble the bytes of the input text before encrypting. A random
				// number will be used as the key for the scrambling process and will be included as
				// the first 4 bytes of the value to be encrypted.
				static Random rand;
				nScrambleKey = rand.uniform(0, ULONG_MAX);

				scrambleData(pszInput, *pulLength, nScrambleKey, true);

				bytes = ByteStream(pszInput, *pulLength);
			}
			else
			{
				// When decrypting, the first 4 bytes will be the key used to unscramble the
				// unencrypted results.
				*pulLength -= 4;
				nScrambleKey = *(long *)pszInput;
				bytes = ByteStream(pszInput + 4, *pulLength);
			}

			ByteStream bsPassword;
			getFAMPassword(bsPassword);

			MapLabel encryptionEngine;
			ByteStream bsOutputBytes;
			if (bEncrypt)
			{
				encryptionEngine.setMapLabel(bsOutputBytes, bytes, bsPassword);
			}
			else
			{
				encryptionEngine.getMapLabel(bsOutputBytes, bytes, bsPassword);
			}

			*pulLength = bsOutputBytes.getLength();
			if (bEncrypt)
			{
				// When encrypting, allocate 4 extra bytes to allow for nScrambleKey.
				*pulLength += 4;
			}

			// Allocate a buffer to hold the encrypted data.
			// NOTE: Need to use CoTaskMemAlloc to allocate the memory so that it can be
			// released on the C# side, CANNOT USE NEW
			pszOutput = (unsigned char*) CoTaskMemAlloc(sizeof(char) **pulLength);
			ASSERT_RESOURCE_ALLOCATION("ELI38793", pszOutput != __nullptr);
			
			if (bEncrypt)
			{
				memcpy(pszOutput, &nScrambleKey, 4);
				memcpy(pszOutput + 4, bsOutputBytes.getData(), *pulLength - 4);
			}
			else
			{
				memcpy(pszOutput, bsOutputBytes.getData(), *pulLength);

				// Unscramble the now unencrypted bytes of the array.
				scrambleData(pszOutput, *pulLength, nScrambleKey, false);
			}

			return pszOutput;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38794");
	}
	catch(UCLIDException& ue)
	{
		*pulLength = 0;

		// Ensure all allocated memory is cleaned up
		if (pszOutput != __nullptr)
		{
			CoTaskMemFree(pszOutput);	
		}

		// Log this exception to the standard exception log
		ue.log("", true, false, true);

		// Now throw the exception
		throw ue;
	}
}