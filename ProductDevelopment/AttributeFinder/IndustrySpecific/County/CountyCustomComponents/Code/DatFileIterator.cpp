#include "stdafx.h"
#include "DatFileIterator.h"

#include <Common.h>
#include <cpputil.h>
#include <Misc.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
DatFileIterator::DatFileIterator(const string& strDatFileBaseFolder, const string& strDocType) 
: FileIterator("")
{
	// Check if the corresponding dat file folder exists
	string strDatFileFolder = strDatFileBaseFolder + "\\" + strDocType;
	m_bIsValidDirectory = isValidFolder(strDatFileFolder);
	if (m_bIsValidDirectory)
	{
		// Auto encrypt the dat files
		string strDatSearchPath = strDatFileFolder + "\\*.dat";
		autoEncryptFiles(strDatSearchPath);

		// Prepare to search for the etfs
		reset(strDatSearchPath + ".etf");
	}
}
//-------------------------------------------------------------------------------------------------
DatFileIterator::~DatFileIterator()
{
}
//-------------------------------------------------------------------------------------------------
bool DatFileIterator::moveNext()
{
	return m_bIsValidDirectory ? FileIterator::moveNext() : false;
}
//-------------------------------------------------------------------------------------------------
string DatFileIterator::getFileName()
{
	// Remove the .dat.etf extension
	string fileName = FileIterator::getFileName();
	fileName.resize(fileName.length() - 8);
	return fileName;
}
//-------------------------------------------------------------------------------------------------
void DatFileIterator::autoEncryptFiles(const string& strSearchPath)
{
	// Search for dat files
	FileIterator fileIter(strSearchPath);
	while (fileIter.moveNext())
	{
		// Auto encrypt the dat files
		string fileName = fileIter.getFileName();
		ULONGLONG nFileSize = fileIter.getFileSize();
		if (nFileSize == 0)
		{
			// Log exception without displaying
			UCLIDException ue("ELI26099", "Rules Dat File Size is zero.");
			ue.addDebugInfo("File Name", fileName);
			ue.log();
		}

		autoEncryptFile(fileName + ".etf", gstrAF_AUTO_ENCRYPT_KEY_PATH);
	}
}
//-------------------------------------------------------------------------------------------------
