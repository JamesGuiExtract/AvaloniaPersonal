
#include "stdafx.h"
#include "ByteStream.h"

#include "UCLIDException.h"
#include "cpputil.h"

#include <fstream>
using namespace std;

//-------------------------------------------------------------------------------------------------
// ByteStream
//-------------------------------------------------------------------------------------------------
ByteStream::ByteStream(const ByteStream& bs)
: m_pszData(NULL)
{
	try
	{
		unsigned long ulCopyLength = bs.getLength();
		if (ulCopyLength > 0)
		{
			// copy the data to this object
			m_bIsOwnerOfMemory = true;
			m_pszData = new unsigned char[ulCopyLength];
			if (m_pszData == NULL)
			{
				throw UCLIDException("ELI00835", "Unable to allocate memory!");
			}
			else
			{
				memcpy((char *) m_pszData, (char *) bs.getData(), ulCopyLength);
			}

			m_ulLength = ulCopyLength;
		}
		else
		{
			// nothing to copy
			m_bIsOwnerOfMemory = true;
			m_pszData = NULL;
			m_ulLength = 0;
		}
	}
	catch (...)
	{
		// delete the memory if it was allocated
		if (m_pszData)
		{
			delete[] m_pszData;
			m_pszData = NULL;
		}

		throw;
	}
}
//-------------------------------------------------------------------------------------------------
ByteStream::ByteStream(const unsigned char* pszData, unsigned long ulLength)
:m_pszData((unsigned char *) pszData), m_ulLength(ulLength)
{
	m_bIsOwnerOfMemory = false;
}
//-------------------------------------------------------------------------------------------------
ByteStream::ByteStream(const string& strData)
:m_pszData(NULL)
{
	try
	{
		// ensure that the length of the string is an even number
		unsigned long ulStrLength = strData.length();
		if (ulStrLength % 2 != 0)
		{
			UCLIDException ue("ELI00836", "Invalid byte stream data!");
			ue.addDebugInfo("Invalid string size:", ulStrLength);
			throw ue;
		}

		// allocate the memory
		m_ulLength = ulStrLength / 2;
		m_pszData = new unsigned char[m_ulLength];
		m_bIsOwnerOfMemory = true;
		if (m_pszData == NULL)
		{
			throw UCLIDException("ELI01682", "Unable to allocate memory!");
		}

		// set each of the bits in the byte stream to the correct value
		for (unsigned int i = 0; i < m_ulLength; i++)
		{
			// get the first char of the 2-char hex string
			unsigned char ucFirst = strData[2*i];
			toupper(ucFirst);
			if (!isHexChar(ucFirst))
			{
				UCLIDException ue("ELI01683", "Invalid hex character!");
				ue.addDebugInfo("position", i);
				ue.addDebugInfo("character", ucFirst);
				throw ue;
			}

			// get the second char of the 2-char hex string
			unsigned char ucSecond = strData[2*i + 1];
			toupper(ucSecond);
			if (!isHexChar(ucSecond))
			{
				UCLIDException ue("ELI01684", "Invalid hex character!");
				ue.addDebugInfo("position", i);
				ue.addDebugInfo("character", ucSecond);
				throw ue;
			}

			// get the hex value for this position
			unsigned char ulValue = getValueOfHexChar(ucFirst) * 16 + getValueOfHexChar(ucSecond);
			m_pszData[i] = ulValue;
		}
	}
	catch (...)
	{
		// delete the memory if it was allocated
		if (m_pszData)
		{
			delete[] m_pszData;
			m_pszData = NULL;
		}

		throw;
	}
}
//-------------------------------------------------------------------------------------------------
ByteStream::ByteStream()
{
	m_bIsOwnerOfMemory = true;
	m_pszData = NULL;
	m_ulLength = 0;
}
//-------------------------------------------------------------------------------------------------
ByteStream::ByteStream(unsigned long ulLength)
:m_ulLength(ulLength)
{
	m_bIsOwnerOfMemory = true;
	m_pszData = NULL;
	setSize(ulLength);
}
//-------------------------------------------------------------------------------------------------
ByteStream::~ByteStream()
{
	try
	{
		if (m_bIsOwnerOfMemory)
		{
			delete[] m_pszData;
			m_pszData = NULL;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16374");
}
//-------------------------------------------------------------------------------------------------
ByteStream& ByteStream::operator=(const ByteStream& bs)
{
	unsigned long ulCopyLength = bs.getLength();
	if (ulCopyLength > 0)
	{
		// copy the data to this object
		m_bIsOwnerOfMemory = true;
		m_pszData = new unsigned char[ulCopyLength];
		if (m_pszData == NULL)
		{
			throw UCLIDException("ELI00837", 
				"ByteStream - Unable to allocate memory in copy constructor!");
		}
		else
		{
			memcpy((char *) m_pszData, (char *) bs.getData(), ulCopyLength);
		}

		m_ulLength = ulCopyLength;
	}
	else
	{
		// nothing to copy
		m_bIsOwnerOfMemory = true;
		m_pszData = NULL;
		m_ulLength = 0;
	}

	return *this;
}
//-------------------------------------------------------------------------------------------------
string ByteStream::asString() const
{
	string strResult;

	try
	{
		INIT_EXCEPTION_AND_TRACING("MLI00653");
		try
		{
			_lastCodePos = "10";

			// write out each of the bytes as two characters
			for (unsigned int i = 0; i < m_ulLength; i++)
			{
				char pszTemp[3] = {0};
				int retVal = sprintf_s(pszTemp, sizeof(pszTemp), "%02x",   m_pszData[i]);

				if (retVal == -1)
				{
					UCLIDException ue("ELI21397", "Failed converting item to hex char!");
					ue.addWin32ErrorInfo();
					throw ue;
				}

				strResult += pszTemp;
				_lastCodePos = "20_" + ::asString(i);
			}
			_lastCodePos = "30";
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21395");
	}
	catch(UCLIDException& uex)
	{
		uex.addDebugInfo("Length", m_ulLength);
		throw uex;
	}

	return strResult;
}
//-------------------------------------------------------------------------------------------------
void ByteStream::setSize(unsigned long _ulLength)
{
	// delete previously allocated memory
	if (m_pszData != NULL && m_bIsOwnerOfMemory)
	{
		delete[] m_pszData;
	}
	// if new size is zero set the memory to null and length to zero
	if ( _ulLength == 0 )
	{
		// if length is zero need to clear the state
		m_pszData = NULL;
		m_bIsOwnerOfMemory = true;
		m_ulLength = 0;
	}
	else 
	{
		m_pszData = new unsigned char[_ulLength];
		
		if (m_pszData == NULL)
		{
			throw UCLIDException("ELI00840", "ByteStream::setSize() error: Out of memory!");
		}
		
		// if this method has been called, no matter what, we are the owners of the memory
		m_bIsOwnerOfMemory = true;
		
		m_ulLength = _ulLength;
	}
}
//-------------------------------------------------------------------------------------------------
void ByteStream::saveTo(const string& strFileName)
{
	// open the file in write mode
	ofstream outfile(strFileName.c_str(), ios::binary | ios::out);
	if (!outfile)
	{
		UCLIDException ue("ELI06602", "Unable to open file in write mode!");
		ue.addDebugInfo("strFileName", strFileName);
		throw ue;
	}

	// write the bytes to the file
	outfile.write((char *) m_pszData, m_ulLength);
	outfile.close();
	waitForFileToBeReadable(strFileName);
}
//-------------------------------------------------------------------------------------------------
void ByteStream::loadFrom(const string& strFileName)
{
	try
	{
		try
		{
			// Pointer to an ifstream
			ifstream* pinFile = NULL;

			// Open the file, retrying if necessary
			waitForFileToBeReadable(strFileName, true, &pinFile, ios::in | ios::binary);

			// Create an auto pointer to manage the pinFile
			auto_ptr<ifstream> apInFile(pinFile);

			// If pointer is null then file was not opened, make one more attempt to open the file
			if (apInFile.get() == NULL)
			{
				// Open the file
				apInFile.reset(new ifstream(strFileName.c_str(), ios::in | ios::binary));
			}

			if (apInFile.get() == NULL || !apInFile->is_open())
			{
				UCLIDException ue("ELI06605", "Unable to open file in read mode.");
				throw ue;
			}

			// get the number of bytes associated with the file
			apInFile->seekg(0, ios::end);
			streampos nFileSize = apInFile->tellg();

			// read the bytes from the file
			setSize(nFileSize);
			apInFile->seekg(0, ios::beg);

			if(nFileSize > 0)
			{
				apInFile->read((char *)getData(), nFileSize);
			}

			// Close the file and reset the auto pointer
			apInFile->close();
			apInFile.reset();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30110");
	}
	catch(UCLIDException& uex)
	{
		UCLIDException ue("ELI30111", "Unable to load bytestream from file.", uex);
		ue.addDebugInfo("File To Load", strFileName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
