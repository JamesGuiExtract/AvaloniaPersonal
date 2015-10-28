#pragma once

#include "BaseUtils.h"

#include <string>

using namespace std;

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

	// encrypts the bsInput with the key bsKey and returns an encrypted string that has the first
	// 8 bytes as a long value converted to string hex that is used as the scramble key
	static string setMapLabelWithS(ByteStream &bsInput, ByteStream &bsKey);

	// decrypts the strInput ( that was originally encrypted with setMapLabelWithS) an returns
	// the resulting ByteStream
	static ByteStream getMapLabelWithS(string strInput, ByteStream& bsKey);

private:

	//-------------------------------------------------------------------------------------------------
	// This method re-orders bytes in the array based on nScrambleKey. This does not encrypt, but
	// ensures that when the scrambled data is encrypted the result will not share any common blocks
	// across multiple attempts even when the source data contains largely the same data.
	// When bScramble is false, data that was previously scrambled will be unscrambled (assuming the
	// same nScrambleKey is used as when the data was scrambled).
	static void scrambleData(unsigned char* pszData, unsigned long nLength, unsigned long nScrambleKey,
		bool bScramble);};
