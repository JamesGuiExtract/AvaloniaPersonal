#pragma once

// Releases memory allocated by RecAPI calls. Create this object after the RecAPI call has 
// allocated space for the object. MemoryType is the data type of the object to release when 
// RecMemoryReleaser goes out of scope.
template<typename MemoryType>
class RecMemoryReleaser
{
public:
	RecMemoryReleaser(MemoryType* pMemoryType);
	~RecMemoryReleaser();

private:
	MemoryType* m_pMemoryType;
};

// Releases memory allocated by a RecInitPlus call. Calls RecQuitPlus when it goes out of scope.
class MainRecMemoryReleaser
{
public:
	MainRecMemoryReleaser();
	~MainRecMemoryReleaser();

private:
};

// NOTE: template classes cannot be compiled separately in Visual C++.
// This ensure the implementation will be compiled with the header.
#include "RecMemoryReleaser.cpp"