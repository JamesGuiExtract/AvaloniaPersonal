#pragma once
#include "stdafx.h"

#include <PDFInputOutputMgr.h>

namespace Extract
{
	namespace Imaging
	{
		namespace Utilities
		{
			// Inlcude the using statements at namespace level (not global)
			using namespace Extract::Licensing;
			using namespace System;

			public ref class ExtractPdfManager sealed
			{
			private:
				// The pdf manager file
				PDFInputOutputMgr* _pdfFile;

				// Finalizer - Cleans up unmanaged resources
				!ExtractPdfManager();

				// The file name returned by the getFileName() call of the PDFInputOutputMgr
				String^ _fileName;

				// The file name returned by the getFileNameInformationString of PDFInputOutputMgr
				String^ _fileNameInformation;

				// Whether the file is being used as input or output
				bool _inputFile;

			public:
				// Constructs a new ExtractPdfManager for the specified file name.
				// If fileUsedAsInput == true then the file will be treated as an input file
				// (in which case it will be converted if necessary on the front end).
				// If fileUsedAsInput == false then the file will be converted when the
				// class is disposed.
				ExtractPdfManager(String^ fileName, bool fileUsedAsInput);

				// Implements IDisposable and cleans up all managed and unmanaged resources
				~ExtractPdfManager(); // Dispose method

				// Returns true if the file being managed is used as an input file
				// and false if the file being manager is used as an output file
				bool IsInputFile();

				// Returns the name of the temporary file if the file was converted
				// or the original file name if the file was not converted
				property String^ FileName
				{
					String^ get();
				}

				// Returns the name of the original file, or if the file has been converted
				// returns a string of the format "Original: <orig> As: <converted>"
				property String^ FileNameInformationString
				{
					String^ get();
				}

			}; // End class ExtractPdfManager
		}; // End namespace Utilities
	}; // End namespace Imaging
}; // End namespace Extract