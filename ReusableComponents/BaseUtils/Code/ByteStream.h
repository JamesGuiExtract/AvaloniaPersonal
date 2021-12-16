#ifndef BYTE_STREAM_HPP
#define BYTE_STREAM_HPP

#include "BaseUtils.h"

#include <string>
#include <vector>
using namespace std;

class EXPORT_BaseUtils ByteStream
{
public:
	
	// copy constructor
	ByteStream(const ByteStream& bs);

	// require: memory pointed to by pszData is available for the
	// lifetime of this object
	// this constructor does not take over ownership....
	// the main purpose of this constructor is to provide a ByteStream wrapper
	// for a CORBA sequence<octet> structure
	ByteStream(const unsigned char* pszData, unsigned long ulLength);

	// strHexData.length() is even, and each character in strHexData is a hex digit
	ByteStream(const string& strHexData);

	ByteStream();
	
	// require: ulLength > 0
	ByteStream(unsigned long ulLength);

	~ByteStream();

	// assignment operator
	ByteStream& operator=(const ByteStream& bs);

	// promise: to delete any memory previously associated with this object
	// promise: to initialize a new contiguous memory array of the specified
	// length, if such amount of memory is available.
	// promise: to take over ownership of newly allocated memory
	void setSize(unsigned long ulLength);

	// inline for performance reasons
	// require: setSize() must have been called or
	// the object must have been constructed through ByteStream(unsigned long)
	unsigned long getLength() const
	{
		return m_ulLength;
	}

	// inline for performance reasons
	// require: setSize() must have been called or
	// the object must have been constructed through ByteStream(unsigned long)
	inline unsigned char* getData()
	{
		return m_pszData;
	};

	// return the bytestream as a hex character stream
	string asString() const;

	// copy as a hex character string in to the provided vector
	void copyToCharVector(std::vector<char>& hexChars) const;

	// inline for performance reasons
	// require: setSize() must have been called or
	// the object must have been constructed through ByteStream(unsigned long)
	inline const unsigned char* getData() const
	{
		return m_pszData;
	}

	// methods to load/save the contents of this bytestream from/to files
	void saveTo(const std::string& strFileName);
	void loadFrom(const std::string& strFileName);

private:
	unsigned long m_ulLength;
	unsigned char* m_pszData;
	bool m_bIsOwnerOfMemory;
};

#endif // BYTE_STREAM_HPP
