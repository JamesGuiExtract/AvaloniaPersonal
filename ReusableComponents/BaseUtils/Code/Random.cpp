// Implementation of Random.h

#include "stdafx.h"
#include "Random.h"
#include "cpputil.h"
#include "UclidException.h"

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
Random::Random(bool bUseTime, bool bUseMachineName, bool bUseMACAddress, bool bUseFreeDiskSpace)
: _next(-1)
{
	reseed(bUseTime, bUseMachineName, bUseMACAddress, bUseFreeDiskSpace);
}
//--------------------------------------------------------------------------------------------------
Random::Random(unsigned long seed)
{
	seedgen(seed);
	_next = 0;
}
//--------------------------------------------------------------------------------------------------
double Random::uniform()
{
	if (_next == -1)
	{
		throw UCLIDException("ELI28718", "Random object has not been initialized.");
	}

	return randgen() * (1.0 / (MAX + 1.0));
}
//--------------------------------------------------------------------------------------------------
unsigned long Random::uniform(unsigned long hi)
{
	if (_next == -1)
	{
		throw UCLIDException("ELI28719", "Random object has not been initialized.");
	}

	return static_cast<unsigned long>(uniform() * hi);
}
//--------------------------------------------------------------------------------------------------
unsigned long Random::uniform(unsigned long lo, unsigned long hi)
{
	if (_next == -1)
	{
		throw UCLIDException("ELI28720", "Random object has not been initialized.");
	}

	return lo + uniform(hi - lo);
}
//--------------------------------------------------------------------------------------------------
bool Random::reseed(unsigned long seed)
{
	seedgen(seed);
	_next = 0;
	return true;
}
//--------------------------------------------------------------------------------------------------
bool Random::reseed(bool bUseTime, bool bUseMachineName, bool bUseMACAddress, bool bUseFreeDiskSpace)
{
	try
	{
		// If at least one value is specified, reseed the object
		if (bUseTime || bUseMachineName || bUseMACAddress || bUseFreeDiskSpace)
		{
			unsigned long ulSeedStringValue = 0;

			string strSeedString = (bUseMachineName ? getComputerName() : "");
			if (bUseMACAddress)
			{
				try
				{
					strSeedString += getMACAddress();
				}
				catch(...)
				{
				}
			}

			ulSeedStringValue = 0;
			for (size_t i = 0; i < strSeedString.length(); i++)
			{
				ulSeedStringValue += strSeedString[i];
			}

			// Build the seed value
			unsigned __int64 ullSeedVal = ulSeedStringValue;
			if (bUseFreeDiskSpace)
			{
				try
				{
					ullSeedVal = (ullSeedVal + getFreeSpaceOnDisk()) % ULLONG_MAX;
				}
				catch(...)
				{
					srand((unsigned int) time(NULL));
					ullSeedVal += ((unsigned int)rand()) % UINT_MAX;
				}
			}

			if (bUseTime)
			{
				ullSeedVal = (ullSeedVal + time(NULL)) % ULLONG_MAX;
			}

			seedgen((unsigned long) ullSeedVal % ULONG_MAX);
			_next = 0;
			return true;
		}

		// Object was not reseeded, return false
		return false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28721");
}
//--------------------------------------------------------------------------------------------------
string Random::getRandomString(long nLength, bool bUpperCase, bool bLowerCase, bool bDigits)
{
	try
	{
		if (_next == -1)
		{
			throw UCLIDException("ELI28722", "Random object has not been initialized.");
		}

		// Build the character set
		string strCharacterSet = (bUpperCase ? gstrUPPER_ALPHA : "")
			+ (bLowerCase ? gstrLOWER_ALPHA : "") + (bDigits ? gstrNUMBERS : "");
		if (strCharacterSet.empty())
		{
			throw UCLIDException("ELI28723", "Must specify at least one set of characters.");
		}
		unsigned long nRange = strCharacterSet.length();

		string strReturnString = "";

		// Generate the random string
		for (long i=0; i < nLength; i++)
		{
			unsigned long nRand = uniform(nRange);
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
	_x[0] = seed & MAX;

	for (int i = 1; i < N; i++)
	{
		_x[i] = (1812433253UL * (_x[i - 1] ^ (_x[i - 1] >> 30)) + i);
		_x[i] &= MAX;
	}
}
//--------------------------------------------------------------------------------------------------
// Mersenne Twister algorithm
unsigned long Random::randgen()
{
	unsigned long rnd = 0;

	// Refill the pool when exhausted
	if (_next == N)
	{
		int a = 0;

		for (int i = 0; i < N - 1; i++)
		{
			rnd = (_x[i] & UPPER_MASK) | _x[i + 1] & LOWER_MASK;
			a = (rnd & 0x1UL) ? MATRIX_A : 0x0UL;
			_x[i] = _x[(i + M) % N] ^ (rnd >> 1) ^ a;
		}

		rnd = (_x[N - 1] & UPPER_MASK) | _x[0] & LOWER_MASK;
		a = (rnd & 0x1UL) ? MATRIX_A : 0x0UL;
		_x[N - 1] = _x[M - 1] ^ (rnd >> 1) ^ a;

		_next = 0; // Rewind index
	}

	rnd = _x[_next++]; // Grab the next number

	// Voodoo to improve distribution
	rnd ^= (rnd >> 11);
	rnd ^= (rnd << 7) & 0x9d2c5680UL;
	rnd ^= (rnd << 15) & 0xefc60000UL;
	rnd ^= (rnd >> 18);

	return rnd;
}