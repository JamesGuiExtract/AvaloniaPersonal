#pragma once
#include <FileIterator.h>

class DatFileIterator : private FileIterator
{
public:
	DatFileIterator(const string& strDatFileBaseFolder, const string& strDocType);
	~DatFileIterator();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns true if another dat file is found; returns false otherwise.
	// NOTE:    This method must be called before any accessor will be valid.
	bool moveNext();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets the name of the current dat file without the .dat.etf extension.
	string getFileName();

private:

	static void autoEncryptFiles(const string& strSearchPath);

	bool m_bIsValidDirectory;
};
