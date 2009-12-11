//--------------------------------------------------------------------------------------------------
/*
12/01/2009 - JDS
			This class taken from/based on http://www.dreamincode.net/code/snippet342.htm
			This class is designed to produce random numbers in a specific range
			with a higher resolution and longer cycle time than the built in rand() function.

  Random number generator class
  =============================
  History:

  Created - Sarah "Voodoo Doll" White (2006/01/24)
  =============================
  Description:

  This class wraps the Mersenne Twister generator
  with a public interface that supports three common
  pseudorandom number requests:

  === Uniform deviate [0,1) ===
  Random rnd(seed);
  double r = rnd.uniform();

  === Uniform deviate [0,hi) ===
  Random rnd(seed);
  unsigned long r = rnd.uniform(hi);

  === Uniform deviate [lo,hi) ===
  Random rnd(seed);
  unsigned long r = rnd.uniform(lo, hi);

  seed, lo, and hi are user supplied values, with
  seed having a default setting of 1 for debugging
  and testing purposes.
*/
//--------------------------------------------------------------------------------------------------
#pragma once
#include "BaseUtils.h"

#include <afxmt.h>
#include <string>

using namespace std;

class EXPORT_BaseUtils Random
{
	// Arbitrary constants that work well
	static const int           N = 624; // Seed array size
	static const int           M = 397;
	static const unsigned long MATRIX_A = 0x9908b0dfUL;
	static const unsigned long UPPER_MASK = 0x80000000UL;
	static const unsigned long LOWER_MASK = 0x7fffffffUL;
	static const unsigned long MAX = 0xffffffffUL;

public:
	// Creates the class and intializes the seed based upon the process ID,
	// the machine name, the MAC address, the free disk space on the drive
	// containing the TEMP folder, and the current time
	Random();

	// Resets the Random object with new seed value, returns true if the object is reseeded
	void reseed(unsigned long seed);

	// Resets the Random object with a computed seed value based upon the process ID,
	// the machine name, the MAC address, the free disk space on the drive
	// containing the TEMP folder, and the current time
	void reseed();

	// Return a uniform deviate in the range [0,1)
	double uniform();

	// Return a uniform deviate in the range [0,hi)
	unsigned long uniform(unsigned long hi);

	// Return a uniform deviate in the range [lo,hi)
	unsigned long uniform(unsigned long lo, unsigned long hi);

	// Returns a random string of nLength containing only the characters specified
	// by the settings.  If bUpperCase == true then [A-Z] may be included in the
	// return string, if bLowerCase == true then [a-z] may be included in the return string,
	// if bDigits == true then [0-9] may be included in the return string.  If bUpperCase,
	// bLowerCase, and bDigits are all false then an exception will be thrown.
	string getRandomString(long nLength, bool bUpperCase, bool bLowerCase, bool bDigits);

private:

	static unsigned long m_ulx[N]; // Random number pool
	static int           m_nNext; // Current pool index

	void seedgen(unsigned long seed);
	unsigned long randgen();

	// The internal function to return the uniform random number
	double internalUniform();

	// Mutex to provide thread safety
	static CMutex ms_Mutex;
};
