// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the INTERNALLICENSEUTILS_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// INTERNALLICENSEUTILS_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef INTERNALLICENSEUTILS_EXPORTS
#define INTERNALLICENSEUTILS_API __declspec(dllexport)
#else
#define INTERNALLICENSEUTILS_API __declspec(dllimport)
#endif

// Exposes encryption/decryption for .Net license utilities to be used only internally at Extract
// Systems.
INTERNALLICENSEUTILS_API unsigned char* externManipulator(
	unsigned char* pszInput, bool bEncrypt, unsigned long* pulLength);

// Only to be used internally 
INTERNALLICENSEUTILS_API unsigned char* externManipulatorInternal(unsigned char* pszInput, bool bEncrypt, unsigned long* pulLength,
	unsigned long key1, unsigned long key2, unsigned long key3, unsigned long key4);

INTERNALLICENSEUTILS_API unsigned char* externManipulatorInternal(unsigned char* pszInput, unsigned char* key, unsigned long keyLength, 
	bool bEncrypt, unsigned long* pulLength);