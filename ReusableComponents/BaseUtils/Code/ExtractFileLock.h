#pragma once

#include "BaseUtils.h"

#include <afxmt.h>

#include <string>
#include <memory>

class UCLIDException;

extern EXPORT_BaseUtils const char* gszLOCK_FILE_EXTENSION;

// https://extract.atlassian.net/browse/ISSUE-13573
// This class is used to protect a file against modification from another Extract Systems process,
// thread or task by using a hidden file with the same name the target file but ending ".ExtractLock"
// NOTE:
// - Presently this class will only actually attempt to lock a file if being used internally at
//	 Extract Systems.
// - A ".ExtractLock" file will not be produced locked if it is read-only.
// - A file can be locked even if it does not yet exist to protect against a race condition for access
//   to a file that is about to be written.
// - FileSupplyingMgmtRole as been hardcoded to ignore ".ExtractLock" files to ensure they are not
//   inadvertently queued.
class EXPORT_BaseUtils ExtractFileLock
{
public:
	// strFileName-		The file to be locked
	// bThrowIfLocked-	true to throw an exception if the file is already locked by another
	//					Extract Systems process. If false, GetExternalLockInfo will indicate the
	//					context that has the file locked.
	// strContext-		What should be reported to other processes as having the file locked. If not
	//					specified (empty), the name of the currently running exe will be used.
	ExtractFileLock(std::string strFileName, bool bThrowIfLocked, std::string strContext = "");

	// If this instance own a lock on the file, the lock will be released upon destruction.
	~ExtractFileLock(void);

	// Provides a sanity check to an outside caller that this instance is valid. Prevents attempts
	// to close handles on a deleted instance or at an invalid address. Primary concern is whether
	// IMiscUtils::DeleteExtractFileLock is being called with a valid pointer.
	inline bool IsValid() { return *m_validator.get() == &m_hLockFileHandle; }

	// Gets the file this lock instance is related to.
	inline std::string GetFileName() { return m_strFileName; }
	
	// Gets whether the specified file is read-only (thus was not locked).
	inline bool IsReadOnly() { return m_bIsReadOnly; }

	// Gets whether this instance owns a lock on the file.
	inline bool HaveLock() { return m_bHaveLock; }

	// Gets whether another process owns a lock on the file.
	inline bool IsLockedByAnotherProcess() { return m_bIsLockedByAnotherProcess; }

	// Checks whether this instance is related to the specified file.
	bool IsForFile(const std::string& strFileName);

	// If the file is locked by another process, provides information about that process.
	std::string GetExternalLockInfo();

	// Add information about the lock another process has on the file as debug data to the provided
	// exception.
	void addExternalLockInfo(UCLIDException &rue);

private:

	///////////////
	// Variables
	///////////////

	HANDLE m_hLockFileHandle;
	std::string m_strFileName;
	std::string m_strLockFileName;
	bool m_bIsReadOnly;
	bool m_bHaveLock;
	bool m_bIsLockedByAnotherProcess;
	std::string m_strExternalLockInfo;
	std::unique_ptr<HANDLE*> m_validator;

	static CMutex ms_Mutex;
	static bool ms_bCheckedInternal;
	static bool ms_bIsInternal;

	///////////////
	// Methods
	///////////////

	// Is this software being run internally at ExtractSystems?
	static bool isInternal();
	
	// Indicates whether this instance is properly initialized.
	void setAsValid(bool bValid);

	// Writes lock info to the specified file handle.
	void writeLockInfo(HANDLE hInfoFileHandle, std::string strLockInfo);

	// Retrieve lock info from the specified lock filename.
	// pbLockExists will indicate whether the specified lock file exists.
	std::string getExternalLockInfo(const std::string& strInfoFileName, bool* pbLockExists);
};