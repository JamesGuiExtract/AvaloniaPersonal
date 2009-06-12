#pragma once

#include "BaseUtils.h"

class ByteStream;

class EXPORT_BaseUtils EncryptionEngine
{
public:
	// require: plainByteStream.getLength() % 8 = 0;
	void encrypt(ByteStream& cipherByteStream, const ByteStream& plainByteStream, 
		const ByteStream& passwordByteStream);

	// require: cipherByteStream.getLength() % 8 = 0;
	void decrypt(ByteStream& plainByteStream, const ByteStream& cipherByteStream,
		const ByteStream& passwordByteStream);
};
