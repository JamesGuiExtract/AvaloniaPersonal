#pragma once

#include "BaseUtils.h"

class ByteStream;

// MapLabel = EncrytionEngine. The class has been renamed disguise its purpose and make hacking our
// encryption a somewhat more difficult task.
class EXPORT_BaseUtils MapLabel
{
public:
	// encrypt
	// require: plainByteStream.getLength() % 8 = 0;
	void setMapLabel(ByteStream& cipherByteStream, const ByteStream& plainByteStream, 
		const ByteStream& passwordByteStream);

	// decrypt
	// require: cipherByteStream.getLength() % 8 = 0;
	void getMapLabel(ByteStream& plainByteStream, const ByteStream& cipherByteStream,
		const ByteStream& passwordByteStream);
};
