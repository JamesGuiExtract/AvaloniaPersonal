#include <UCLIDException.h>
#include <ScansoftErr.h>

//-------------------------------------------------------------------------------------------------
// RecMemoryReleaser
//
// Ensures proper destruction and memory cleanup of Nuance HIMGFILE and HPAGE instances.
// Create this object after the RecAPI call has allocated space for the object. MemoryType is the
// data type of the object to release when  RecMemoryReleaser goes out of scope.
//-------------------------------------------------------------------------------------------------
template<typename MemoryType>
class RecMemoryReleaser
{
public:
	explicit RecMemoryReleaser(MemoryType* pMemoryType);
	~RecMemoryReleaser();

private:
	MemoryType* m_pMemoryType;
};

//-------------------------------------------------------------------------------------------------
// Implementation
//-------------------------------------------------------------------------------------------------
template<typename MemoryType>
RecMemoryReleaser<MemoryType>::RecMemoryReleaser(MemoryType* pMemoryType)
 : m_pMemoryType(pMemoryType)
{

}
//-------------------------------------------------------------------------------------------------
template<>
RecMemoryReleaser<tagIMGFILEHANDLE>::~RecMemoryReleaser()
{
	try
	{
		// The image may have been closed before the call to destructor. Don't both checking error
		// code.
		kRecCloseImgFile(m_pMemoryType);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI39944");
}
//-------------------------------------------------------------------------------------------------
template<>
RecMemoryReleaser<RECPAGESTRUCT>::~RecMemoryReleaser()
{
	try
	{
		RECERR rc = kRecFreeRecognitionData(m_pMemoryType);

		if (rc != REC_OK)
		{
			UCLIDException ue("ELI39945", 
				"Application trace: Unable to release recognition data. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}

		rc = kRecFreeImg(m_pMemoryType);

		if (rc != REC_OK)
		{
			UCLIDException ue("ELI39946", 
				"Application trace: Unable to release page image. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI39947");
}