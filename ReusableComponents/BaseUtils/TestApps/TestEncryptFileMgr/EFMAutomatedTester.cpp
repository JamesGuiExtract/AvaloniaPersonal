// EFMAutomatedTester.cpp : Implementation of CEFMAutomatedTester
#include "stdafx.h"
#include "TestEncryptFileMgr.h"
#include "EFMAutomatedTester.h"

#include <UCLIDException.h>
#include <TemporaryFileName.h>
#include <EncryptedFileManager.h>
#include <cpputil.h>

#include <fstream>
#include <vector>
#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// CEFMAutomatedTester
//-------------------------------------------------------------------------------------------------
CEFMAutomatedTester::CEFMAutomatedTester()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEFMAutomatedTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IEFMAutomatedTester,
		&IID_ITestableComponent
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEFMAutomatedTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	bool bExceptionCaught = false;
	
	try
	{
		// Check test result logger
		if (m_ipResultLogger == __nullptr)
		{
			throw UCLIDException( "ELI07590", "Test Result Logger object not set!" );
		}

		/////////////////////////////////////
		// Set up Encrypt File Manager object
		/////////////////////////////////////
		bool bSuccess = true;

		m_ipResultLogger->StartTestCase( _bstr_t("Object Creation"), 
			_bstr_t("Test EFM"), kAutomatedTestCase ); 

		MapLabelManager encryptedFileManager;

		m_ipResultLogger->EndTestCase(bSuccess ? VARIANT_TRUE : VARIANT_FALSE);

		/////////////////
		// Test text file
		/////////////////

		m_ipResultLogger->StartTestCase( 
			_bstr_t("Text strings with & without leading & trailing slash Rs"), 
			_bstr_t("Test EFM"), kAutomatedTestCase ); 

		// Create vector of text strings
		vector<string> vecData;

		vecData.push_back("String 1");
		vecData.push_back("String 2 with trailing slash R\r");
		vecData.push_back("String 3");
		vecData.push_back("\rString 4 with leading slash R");
		vecData.push_back("\rString 5 with leading and trailing slash Rs\r");
		vecData.push_back("String 6");

		// Store strings in temporary file
		TemporaryFileName	tfnInput(true);
		ofstream outfile( tfnInput.getName().c_str() );
		bool bWriteNewline = false;
		vector<string>::const_iterator iter;
		for (iter = vecData.begin(); iter != vecData.end(); iter++)
		{
			if (bWriteNewline)
			{
				outfile << string("\n");
			}

			string strTemp = iter->c_str();
			outfile << strTemp;
			bWriteNewline = true;
		}
		outfile.flush();
		outfile.close();
		waitForFileToBeReadable(tfnInput.getName());

		// Encrypt the file
		TemporaryFileName	tfnOutput(true);
		encryptedFileManager.setMapLabel( tfnInput.getName().c_str(), tfnOutput.getName().c_str() );

		// Decrypt the file
		vector<string> vecResult = encryptedFileManager.getMapLabel( tfnOutput.getName().c_str());

		// Compare vectors
		if (vecData.size() != vecResult.size())
		{
			// Sizes do not match
			bSuccess = false;
		}
		else
		{
			for (unsigned int i = 0; i < vecData.size(); i++)
			{
				// Retrieve each string
				string strData = vecData[i];
				string strResult = vecResult[i];

				// Trim any leading/trailing /r characters from Data string
				string strTrim = trim( strData, "\r", "\r" );

				// Compare the strings
				if (strTrim != strResult)
				{
					bSuccess = false;
					break;
				}
			}
		}

		// Report results
		m_ipResultLogger->EndTestCase( bSuccess ? VARIANT_TRUE : VARIANT_FALSE );

		//////////////////////////
		// Test padded binary file
		//////////////////////////

		bSuccess = true;
		m_ipResultLogger->StartTestCase( _bstr_t("Text 50 random bytes"), 
			_bstr_t("Test EFM"), kAutomatedTestCase ); 

		// Create random bytes
		srand( (unsigned)time( NULL ) );

		unsigned char *pInData = new unsigned char[50];
		unsigned int ui = 0;
		for (ui = 0; ui < 50; ui++)
		{
			pInData[ui] = rand() % 256;
		}

		// Write bytes to temporary file
		TemporaryFileName	tfnBinaryIn(true);
		CFile	fileIn;
		fileIn.Open( tfnBinaryIn.getName().c_str(), 
			CFile::modeCreate | CFile::modeWrite | CFile::typeBinary );
		fileIn.Write( (void *)pInData, 50 );
		fileIn.Flush();
		fileIn.Close();
		waitForFileToBeReadable(tfnBinaryIn.getName());

		// Encrypt the file
		TemporaryFileName	tfnEncryptBin(true);
		encryptedFileManager.setMapLabel( tfnBinaryIn.getName().c_str(), tfnEncryptBin.getName().c_str() );

		// Decrypt the file
		unsigned long ulSize = 0;
		unsigned char *pOutData = encryptedFileManager.getMapLabel( 
			tfnEncryptBin.getName().c_str(), &ulSize );

		// Compare bytes
		for (ui = 0; ui < ulSize; ui++)
		{
			if (pOutData[ui] != pInData[ui])
			{
				bSuccess = false;
			}
		}

		// Report results
		m_ipResultLogger->EndTestCase( bSuccess ? VARIANT_TRUE : VARIANT_FALSE );
		delete[] pInData;
		delete[] pOutData;

		////////////////////////////
		// Test unpadded binary file
		////////////////////////////
		bSuccess = true;
		m_ipResultLogger->StartTestCase( _bstr_t("Text 60 random bytes + 4 size bytes"), 
			_bstr_t("Test EFM"), kAutomatedTestCase ); 

		// Create random bytes
		srand( (unsigned)time( NULL ) );

		unsigned char *pInData2 = new unsigned char[60];
		for (ui = 0; ui < 60; ui++)
		{
			pInData2[ui] = rand() % 256;
		}

		// Write bytes to temporary file
		TemporaryFileName	tfnBinaryIn2(true);
		CFile	fileIn2;
		fileIn2.Open( tfnBinaryIn2.getName().c_str(), 
			CFile::modeCreate | CFile::modeWrite | CFile::typeBinary );
		fileIn2.Write( (void *)pInData2, 60 );
		fileIn2.Flush();
		fileIn2.Close();
		waitForFileToBeReadable(tfnBinaryIn2.getName());

		// Encrypt the file
		TemporaryFileName	tfnEncryptBin2(true);
		encryptedFileManager.setMapLabel( tfnBinaryIn2.getName().c_str(), tfnEncryptBin2.getName().c_str() );

		// Decrypt the file
		ulSize = 0;
		unsigned char *pOutData2 = encryptedFileManager.getMapLabel( 
			tfnEncryptBin2.getName().c_str(), &ulSize );

		// Compare bytes
		for (ui = 0; ui < ulSize; ui++)
		{
			if (pOutData2[ui] != pInData2[ui])
			{
				bSuccess = false;
			}
		}

		// Report results
		m_ipResultLogger->EndTestCase( bSuccess ? VARIANT_TRUE : VARIANT_FALSE );
		delete[] pInData2;
		delete[] pOutData2;

		////////////////////////////
		// Test zero length file
		////////////////////////////
		bSuccess = true;
		m_ipResultLogger->StartTestCase( _bstr_t("Empty file( zero length)"), 
			_bstr_t("Test EFM"), kAutomatedTestCase ); 


		// Write bytes to temporary file
		TemporaryFileName	tfnBinaryIn3(true);
		CFile	fileIn3;
		fileIn3.Open( tfnBinaryIn3.getName().c_str(), 
			CFile::modeCreate | CFile::modeWrite | CFile::typeBinary );
		fileIn3.Close();
		waitForFileToBeReadable(tfnBinaryIn3.getName());

		// Encrypt the file
		TemporaryFileName	tfnEncryptBin3(true);
		encryptedFileManager.setMapLabel( tfnBinaryIn3.getName(), tfnEncryptBin3.getName().c_str() );

		// Decrypt the file
		ulSize = 0;
		unsigned char *pOutData3 = encryptedFileManager.getMapLabel( 
			tfnEncryptBin3.getName().c_str(), &ulSize );
		
		// Compare size since the length should be zero
		if ( ulSize != 0 )
		{
			bSuccess = false;
		}
		// Report results
		m_ipResultLogger->EndTestCase( bSuccess ? VARIANT_TRUE : VARIANT_FALSE );
		delete[] pOutData3;

	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI07591", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE)

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEFMAutomatedTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEFMAutomatedTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEFMAutomatedTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
