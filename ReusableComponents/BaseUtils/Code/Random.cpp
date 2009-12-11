// Implementation of Random.h

#include "stdafx.h"
#include "Random.h"
#include "cpputil.h"
#include "UclidException.h"

//--------------------------------------------------------------------------------------------------
// Static members
//--------------------------------------------------------------------------------------------------
CMutex Random::ms_Mutex;
int Random::m_nNext = -1;
unsigned long Random::m_ulx[Random::N] = {0};

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
Random::Random()
{
	// Check if this object has been initialized yet
	if (m_nNext == -1)
	{
		// Lock the mutex
		CSingleLock lg(&ms_Mutex, TRUE);

		// Check again for initialization
		if (m_nNext == -1)
		{
			// Not initialized to seed the object
			reseed();
		}
	}
}
//--------------------------------------------------------------------------------------------------
double Random::uniform()
{
	// Lock the mutex
	CSingleLock lg(&ms_Mutex, TRUE);

	return internalUniform();
}
//--------------------------------------------------------------------------------------------------
unsigned long Random::uniform(unsigned long hi)
{
	// Lock the mutex
	CSingleLock lg(&ms_Mutex, TRUE);

	return static_cast<unsigned long>(internalUniform() * hi);
}
//--------------------------------------------------------------------------------------------------
unsigned long Random::uniform(unsigned long lo, unsigned long hi)
{
	// Lock the mutex
	CSingleLock lg(&ms_Mutex, TRUE);

	return lo + uniform(hi - lo);
}
//--------------------------------------------------------------------------------------------------
void Random::reseed(unsigned long seed)
{
	// Lock the mutex
	CSingleLock lg(&ms_Mutex, TRUE);

	// Seed the generator and set m_nNext to 0
	seedgen(seed);
	m_nNext = 0;
}
//--------------------------------------------------------------------------------------------------
void Random::reseed()
{
	try
	{
		// Lock the mutex
		CSingleLock lg(&ms_Mutex, TRUE);

		string strSeedString = getComputerName();

		try
		{
			strSeedString += getMACAddress();
		}
		catch(...)
		{
		}

		// Initialize the seed value with the current ProcessID
		unsigned __int64 ullSeedVal = GetCurrentProcessId();

		// Add in the character value for the machine name and MAC address
		for (size_t i = 0; i < strSeedString.length(); i++)
		{
			ullSeedVal += strSeedString[i];
		}

		// Add the free disk space to the seed
		try
		{
			ullSeedVal = (ullSeedVal + getFreeSpaceOnDisk()) % ULLONG_MAX;
		}
		catch(...)
		{
			srand((unsigned int) time(NULL));
			ullSeedVal += ((unsigned int)rand()) % UINT_MAX;
		}

		// Add the current time value to the seed
		ullSeedVal = (ullSeedVal + time(NULL)) % ULLONG_MAX;

		// Seed the generator and reset m_nNext to 0
		seedgen((unsigned long) ullSeedVal % ULONG_MAX);
		m_nNext = 0;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28721");
}
//--------------------------------------------------------------------------------------------------
string Random::getRandomString(long nLength, bool bUpperCase, bool bLowerCase, bool bDigits)
{
	try
	{
		// Build the character set
		string strCharacterSet = (bUpperCase ? gstrUPPER_ALPHA : "")
			+ (bLowerCase ? gstrLOWER_ALPHA : "") + (bDigits ? gstrNUMBERS : "");
		if (strCharacterSet.empty())
		{
			throw UCLIDException("ELI28723", "Must specify at least one set of characters.");
		}
		unsigned long nRange = strCharacterSet.length();

		string strReturnString = "";

		// Lock the mutex
		CSingleLock lg(&ms_Mutex, TRUE);

		// Generate the random string
		for (long i=0; i < nLength; i++)
		{
			unsigned long nRand = static_cast<unsigned long>(internalUniform() * nRange);
			strReturnString += strCharacterSet[nRand];
		}

		return strReturnString;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28724");
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
void Random::seedgen(unsigned long seed)
{
	m_ulx[0] = seed & MAX;

	for (int i = 1; i < N; i++)
	{
		m_ulx[i] = (1812433253UL * (m_ulx[i - 1] ^ (m_ulx[i - 1] >> 30)) + i);
		m_ulx[i] &= MAX;
	}
}
//--------------------------------------------------------------------------------------------------
double Random::internalUniform()
{
	return randgen() * (1.0 / (MAX + 1.0));
}
//--------------------------------------------------------------------------------------------------
// Mersenne Twister algorithm
unsigned long Random::randgen()
{
	unsigned long rnd = 0;

	// Refill the pool when exhausted
	if (m_nNext == N)
	{
		int a = 0;

		for (int i = 0; i < N - 1; i++)
		{
			rnd = (m_ulx[i] & UPPER_MASK) | m_ulx[i + 1] & LOWER_MASK;
			a = (rnd & 0x1UL) ? MATRIX_A : 0x0UL;
			m_ulx[i] = m_ulx[(i + M) % N] ^ (rnd >> 1) ^ a;
		}

		rnd = (m_ulx[N - 1] & UPPER_MASK) | m_ulx[0] & LOWER_MASK;
		a = (rnd & 0x1UL) ? MATRIX_A : 0x0UL;
		m_ulx[N - 1] = m_ulx[M - 1] ^ (rnd >> 1) ^ a;

		m_nNext = 0; // Rewind index
	}

	rnd = m_ulx[m_nNext++]; // Grab the next number

	// Voodoo to improve distribution
	rnd ^= (rnd >> 11);
	rnd ^= (rnd << 7) & 0x9d2c5680UL;
	rnd ^= (rnd << 15) & 0xefc60000UL;
	rnd ^= (rnd >> 18);

	return rnd;
}