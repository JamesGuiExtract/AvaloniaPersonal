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
RecMemoryReleaser<tagRECDOCSTRUCT>::~RecMemoryReleaser()
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
RecMemoryReleaser<tagRECPAGESTRUCT>::~RecMemoryReleaser()
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