#include "stdafx.h"
#include "EncryptionEngine.h"
#include "IceKey.h"
#include "ByteStream.h"
#include "ByteStreamManipulator.h"
#include "UCLIDException.h"
#include "Random.h"

#include <string>

using namespace std;

// MapLabel = EncrytionEngine. The class has been renamed disguise its purpose and make hacking our
// encryption a somewhat more difficult task.
void MapLabel::setMapLabel(ByteStream& cipherByteStream, 
						   const ByteStream& plainByteStream, 
						   const ByteStream& passwordByteStream)
{
	// make sure that the length of the password is divisible
	// by 8, as this is a requirement of using the IceKey engine
	if (passwordByteStream.getLength() % 8 != 0)
	{
		throw UCLIDException("ELI00507", "Invalid password length!");
	}

	// make sure that the length of the plain byte stream is
	// divisible by 8, as this is a requirement of the IceKey engine
	if (plainByteStream.getLength() % 8 != 0)
	{
		throw UCLIDException("ELI00508", "Length of plain text invalid!");
	}

	// allocate memory for the cipher text that needs to be returned.
	cipherByteStream.setSize(plainByteStream.getLength());

	// create and instance of the IceKey encryption engine and
	// initialize the password.
	IceKey ik(passwordByteStream.getLength() / 8);
	ik.set(passwordByteStream.getData());

	// get pointers to the memory with the plain text and the cipher text
	const unsigned char *pszPlainText = plainByteStream.getData();
	unsigned char* pszCipherText = cipherByteStream.getData();

	// encrypt the stream, 8 bytes at a time
	unsigned long ulNumIterations = cipherByteStream.getLength() / 8;
	for (unsigned int i=0; i < ulNumIterations; i++)
		ik.encrypt(pszPlainText+i*8, pszCipherText+i*8);
}
//-------------------------------------------------------------------------------------------------
void MapLabel::getMapLabel(ByteStream& plainByteStream, 
						   const ByteStream& cipherByteStream, 
						   const ByteStream& passwordByteStream)
{
	// make sure that the length of the password is divisible
	// by 8, as this is a requirement of using the IceKey engine
	if (passwordByteStream.getLength() % 8 != 0)
	{
		throw UCLIDException("ELI00509", "Invalid password length!");
	}

	// make sure that the length of the plain byte stream is
	// divisible by 8, as this is a requirement of the IceKey engine
	if (cipherByteStream.getLength() % 8 != 0)
	{
		throw UCLIDException("ELI00510", "Length of cipher text invalid!");
	}

	// allocate memory for the plain text that needs to be returned.
	plainByteStream.setSize(cipherByteStream.getLength());

	// create and instance of the IceKey encryption engine and
	// initialize the password.
	IceKey ik(passwordByteStream.getLength() / 8);
	ik.set(passwordByteStream.getData());

	// get pointers to the memory with the plain text and the cipher text
	const unsigned char* pszCipherText = cipherByteStream.getData();
	unsigned char *pszPlainText = plainByteStream.getData();

	// decrypt the stream, 8 bytes at a time
	unsigned long ulNumIterations = plainByteStream.getLength() / 8;
	for (unsigned int i=0; i < ulNumIterations; i++)
		ik.decrypt(pszCipherText+i*8, pszPlainText+i*8);
}
//-------------------------------------------------------------------------------------------------
void MapLabel::scrambleData(unsigned char* pszData, unsigned long nLength, unsigned long nScrambleKey,
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
		unsigned long nSwapPos = (i + (nLength / 2) - 8 + nRandomOffset);
		// Roll over once past the end of the array.
		nSwapPos = nSwapPos % nLength;

		swap(pszData[i], pszData[nSwapPos]);
	}
}
//-------------------------------------------------------------------------------------------------
ByteStream MapLabel::getMapLabelWithS(string strInput, ByteStream& bsKey)
{
	// Get the scramble key which will be the first 8 bytes  (strInput is stringized bytestream
	string strScrambleKey = strInput.substr(0,8);
	
	ByteStream bsScramble(strScrambleKey);
	unsigned long ulScrambleKey = *(unsigned long*) bsScramble.getData();
	
	ByteStream bytes(strInput.substr(8, string::npos));
	
	ByteStream decryptedBytes;

	// Decrypt the stored, encrypted PW
	MapLabel encryptionEngine;
	encryptionEngine.getMapLabel(decryptedBytes, bytes, bsKey);
	ByteStreamManipulator bsm(ByteStreamManipulator::kRead, decryptedBytes);
	
	encryptionEngine.scrambleData(decryptedBytes.getData(), decryptedBytes.getLength(), ulScrambleKey, false);

	return decryptedBytes;
}
//-------------------------------------------------------------------------------------------------
string MapLabel::setMapLabelWithS(ByteStream &bsInput, ByteStream &bsKey)
{
	unsigned long ulScrambleKey;
	static Random rand;
	ulScrambleKey = rand.uniform(0, ULONG_MAX);

	ByteStream bsScramble((unsigned char*)&ulScrambleKey, sizeof(ulScrambleKey));
	string strScramble = bsScramble.asString();

	MapLabel::scrambleData(bsInput.getData(), bsInput.getLength() ,ulScrambleKey, true);

	// Do the encryption
	ByteStream encryptedBS;
	MapLabel encryptionEngine;
	encryptionEngine.setMapLabel(encryptedBS, bsInput, bsKey);

	// Return the encrypted value
	return encryptedBS.asString().insert(0, strScramble);	
}
