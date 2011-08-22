#include "stdafx.h"
#include "EncryptionEngine.h"
#include "IceKey.h"
#include "ByteStream.h"
#include "UCLIDException.h"

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

	// decrpypt the stream, 8 bytes at a time
	unsigned long ulNumIterations = plainByteStream.getLength() / 8;
	for (unsigned int i=0; i < ulNumIterations; i++)
		ik.decrypt(pszCipherText+i*8, pszPlainText+i*8);
}
