// RecMemoryReleaser.cpp : Defines the RecMemoryReleaser template class.
//
// NOTE: this source file is excluded from the build because it is explicitly included by its header file

#include "stdafx.h"
#include "RecMemoryReleaser.h"
#include "ScansoftErr.h"

#include <UCLIDException.h>
#include <RecAPIPlus.h>

//-------------------------------------------------------------------------------------------------
// RecMemoryReleaser
//-------------------------------------------------------------------------------------------------
template<typename MemoryType>
RecMemoryReleaser<MemoryType>::RecMemoryReleaser(MemoryType* pMemoryType) 
	: m_pMemoryType(pMemoryType)
{
}
//-------------------------------------------------------------------------------------------------
template<>
RecMemoryReleaser<RECDOCSTRUCT>::~RecMemoryReleaser()
{
	try
	{
		// [LegacyRCAndUtils:6184]
		// Exceptions are being thrown in some cases trying to close the document. I am adding
		// retries here in case there is a chance that a subsequent call to RecCloseDoc would work.
		static const int nRetries = 0;

		for (int i = 0; i < nRetries; i++)
		{
			try
			{
				try
				{
					// close the document
					RECERR rc = RecCloseDoc(0, m_pMemoryType);

					// log any errors
					if (rc != REC_OK)
					{
						UCLIDException ue("ELI18584",
							"Application trace: Unable to close document. Possible memory leak.");
						loadScansoftRecErrInfo(ue, rc);
						ue.log();
					}

					return;
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34320");
			}
			catch (UCLIDException &ue)
			{
				// After all retry attempts, throw the exception so it gets logged.
				if (i == nRetries - 1)
				{
					throw ue;
				}

				Sleep(200);
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18591");
}
//-------------------------------------------------------------------------------------------------
template<>
RecMemoryReleaser<tagIMGFILEHANDLE>::~RecMemoryReleaser()
{
	try
	{
		RECERR rc = kRecCloseImgFile(m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI18593",
				"Application trace: Unable to close image file. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18585");
}
//-------------------------------------------------------------------------------------------------
template<>
RecMemoryReleaser<RECPAGESTRUCT>::~RecMemoryReleaser()
{
	try
	{
		RECERR rc = kRecFreeRecognitionData(m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI18581", 
				"Application trace: Unable to release recognition data. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}

		rc = kRecFreeImg(m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI18582", 
				"Application trace: Unable to release page image. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18583");
}

//-------------------------------------------------------------------------------------------------
// MainRecMemoryReleaser
//-------------------------------------------------------------------------------------------------
MainRecMemoryReleaser::MainRecMemoryReleaser()
{
	// do nothing of consequence
}
//-------------------------------------------------------------------------------------------------
MainRecMemoryReleaser::~MainRecMemoryReleaser()
{
	try
	{
		RECERR rc = RecQuitPlus();
		if(rc != REC_OK)
		{
			UCLIDException ue("ELI18604", 
				"Application trace: Unable to free OCR engine resources. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18603");
}
//-------------------------------------------------------------------------------------------------