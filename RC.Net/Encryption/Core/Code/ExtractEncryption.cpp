// This is the main DLL file.

#include "stdafx.h"
#include "ExtractEncryption.h"

#include <string>

using namespace Extract;
using namespace Extract::Encryption;
using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Security::Cryptography;
using namespace System::Security::Permissions;
using namespace System::Security::Policy;
using namespace System::Text;

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
#pragma unmanaged
// Combine the strings in the following order to recreate the key blob (2344 characters):
// KEY01 + KEY07 + KEY02 + KEY08 + KEY11 + KEY03 + KEY17 + KEY09 + KEY04 + KEY10 + KEY16 + KEY05
// + KEY14 + KEY12 + KEY06 + KEY13 + KEY15.substr(0, length-2)
// NOTE: The key blob must be converted back into an array of bytes (each two hex characters
// represent one byte).  The array of bytes must then be manipulated in the following way:
// 1. Each byte must be XOR'd with 21
// 2. Each two byte block must be swapped (i.e. byte[0] and byte[1] swap, byte[2] and byte[3] swap)
const string _KEY01 = "17121515B1151515464727541D15151515141514EA9098CFC25844BF11B2A6C1483FD543D369CAB7CFD73E1B604D5C03E6CBA50AE9170E4DF165742887FDC2169AE7EB6A57";
const string _KEY02 = "94F996B14066681697C2131CB6E9069BA679A39DAD106FF38B4544EC5DE38F58D24886B00BE4519E54BF399CB0791DE373EB3068DE3B82405DAE0CAE50233003B4B0C8DBD2";
const string _KEY03 = "F1D380701874E8CCE669293913C4BBD8C25DDF1DD912BB65ED23ED3B24DC0CBFB778ED16DC09910354BAC1EA4A10AB23D66518C162907EE5D6E74A90DA7F09DFC29C39E9C8";
const string _KEY04 = "B0D497539ED0C3E2AD771B974FA165984643713D6C78E2A82F21CE0D2F5F1185236A84EED6311A6D8046E0D59F28338AE23C4366105475314799E9C2BCA7D42DF67EDD8AB5";
const string _KEY05 = "76AE112FBE30B4504A295440D690DC07546606F085D9DB97867AB433821ED058C2D8F16AC902D12AE6F390ECECF24607C80FCD214D274CCF9607CEE86B4FFE38ACE0667B4A";
const string _KEY06 = "D96A61D39D6453B8C4C5D3DF8F3F2622B9D92DB3A6940EC73D8E5D71DD5EBB9A95A42E0CB3587623EB4732CB95FC1995734963B6D066FF6E2044349967852A9F404FB554D3";
const string _KEY07 = "35C969EA08E6610826C9C74AA6330A2BE689F3644808D54E129DBD3C4A7963E8E7BC6AEA911EA7D01BB8B61892A924640CB16E5130B7A5AA4A87915A02871B9D9D7229159F";
const string _KEY08 = "6A9AE9B02777BA049FA1EB281A502EE1CB0F9D93863E3B4AD0732A9D7CCB8573B1ED19C2E31C69A2C5122E8F231DA4DA26971177C303EF4C066081A229DC455FB3EE36B8A4";
const string _KEY09 = "1058263DA8303605090BF9F48AE6DE24191B52B9A63BE7DD9D177EDA4EDCF0A90B9C4468717F37EDDDEC6CA5D1BC3AA26006F0DC31F7AA986599F9EED3909FC386B2CA782B";
const string _KEY10 = "DA1D8F019149DFF29F7C6A4717008DC8A859034DA0AAA3EFF61ABEB6F7D84816D44B95D6E8DFEA8278B985B2C27B5ED6D46560CB4AB87BD60A040C8D003C3345775D5E68AB";
const string _KEY11 = "5B7EEBA3051EA2F56223AC4C8B37542BCF3E53BF445D9A475B08AE1D9027224A4F0A5CA2E332B857CDD951EA2F51A455936E4739A1737E18663A901F13C62AA32C38743C8A";
const string _KEY12 = "B5C1688DCF872D8652129D4336A312CE01DB7919C05DD1A10F635D3ECAD64B7E5667C7A917B29D6B2A4CB8DF18059B69A95CB660E9E59AE716E4F213E560C460E47707B7EA";
const string _KEY13 = "DB9BB87BAA82C4CB2F2C74E631CFA10483DB3C72878B00F328CEC42285F7009175BA841AF2B5D78C33A9256B251FF8273FB53786B66E86D3FEDAE3EDE16400094B9EACFFD7";
const string _KEY14 = "51EE2CEF464B7945E627F8D46C415B3AFA7A6B9DCED6FDF239E4CB4BF9BC87F3009F8324746CD603BAF11268DA91D1FC793333F85BCA6397DADFF8CB58D2C96313C8251CEC";
const string _KEY15 = "E7433A4822EF3F628F33A8E84CF5A43041ABF8D931698CD1CBDA22205E85926EA9A5CFB402849B2FD3BB4C46AF8AEFDB5EE2346F873562B544135B066F0C05E460907AC242";
const string _KEY16 = "BD6A36C07C78A9D82FD350E1EF75DE38AB84D0636D8D9CDAAA47B41665DBC90AEB1B22428A888EB978C1E5C8E3D44B5AB8E6D3660EAEDEB1D51661428B9A27D155A9B01E68";
const string _KEY17 = "133A604188C5AFB3D712ED1E41D6A317B8AFFDCE5110E6BA5415AC96109665D8C7F413C861F6C5686D572A13B2A5CABDB69A6FFA33B8B7FF95AD9B329F74CE8218A6E76DAB";

// Length of the above RSA key
const size_t _LENGTH = 2344;

const int _KEY_SIZE = 256;
const int _IV_LENGTH = 32;
const int _BUFFER_SIZE = 1024;

// Current versions
const int _VERSION = 1;

// Version 1 - Makes use of the version RSA key
const int _PASSWORD_ENCRYPT_VERSION = 1;

//--------------------------------------------------------------------------------------------------
// Local methods
//--------------------------------------------------------------------------------------------------
// This function gets the internal password for encryption/decryption.  There is no need to expose
// this in the header file.  Also need to ensure that this is bracketed by a pragma unmanaged
// section so that the algorithm and string are not exposed in IL code.
unsigned char* getInternalKey()
{
	// Declare the string and set the capacity for the string
	string returnString;
	returnString.reserve(_LENGTH);

	// Build the string
	returnString = _KEY01 + _KEY07 + _KEY02 + _KEY08 + _KEY11 + _KEY03 + _KEY17 + _KEY09
		+ _KEY04 + _KEY10 + _KEY16 + _KEY05+ _KEY14 + _KEY12 + _KEY06 + _KEY13 +
		_KEY15.substr(0, _KEY15.length() - 2);

	// Get the length of the key
	size_t length = returnString.length();
	unsigned char* key = (unsigned char*)malloc(length);
	if (key == NULL)
	{
		return NULL;
	}

	// Copy the string into the key
	memset(key, 0, length);
	memcpy(key, returnString.c_str(), length);

	// 0 the string
	memset((void*)returnString.c_str(), 0, length);

	// Return the key
	return key;
}
//--------------------------------------------------------------------------------------------------
// This is a copy of the method from BaseUtils, it is copied here so that there will be
// no BaseUtils dependency for this assembly.  The only change in the method is that
// the check for whether a character is a hex char and throwing an exception if not
// has been removed since this is a special case where we know that each character is
// a hex character.
unsigned char getValueOfHexChar(unsigned char ucChar)
{
	if (ucChar >= '0' && ucChar <= '9')
	{
		return ucChar - '0';
	}
	else
	{
		return ucChar - 'A' + 10;
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: An unmanaged function copy the Extract RSA key into a byte array that
// will be passed into the rsa->ImportCspBlob function
void modifyData(unsigned char* dest)
{
	unsigned char* source = NULL;
	
	try
	{
		// Get the source key
		source = getInternalKey();

		// Copy the source key to the destination array
		for(size_t i=0; i < _LENGTH; i += 2)
		{
			// Compute the byte value for the array (XOR by 21)
			dest[i/2] = ((getValueOfHexChar(source[i]) * 16) + getValueOfHexChar(source[i+1])) ^ 21;

			// 0 the memory location for the source array
			source[i] = source[i+1] = 0;
		}

		// Iterate throught the destination array and swap every two elements
		size_t length = _LENGTH/2;
		for(size_t i=0; i < length; i += 2)
		{
			dest[i] ^= dest[i+1] ^= dest[i] ^= dest[i+1];
		}

		// Free the source
		free(source);
	}
	catch(...)
	{
		if (source != __nullptr)
		{
			free(source);
		}

		throw;
	}
}
//--------------------------------------------------------------------------------------------------
void wrapData(unsigned char* data, __int64 length, int version)
{
	if (version == 1)
	{
		// XOR each piece of the data and then swap its position
		for(__int64 i=4; i < length; i += 2)
		{
			data[i] ^= 100;
			data[i+1] ^= 212;
			data[i] ^= data[i+1] ^= data[i] ^= data[i+1];
		}
	}
}
//--------------------------------------------------------------------------------------------------
void unwrapData(unsigned char* data, __int64 length, int version)
{
	if (version == 1)
	{
		// Swap each piece of data and then XOR it
		for (__int64 i=4; i < length; i += 2)
		{
			data[i] ^= data[i+1] ^= data[i] ^= data[i+1];
			data[i] ^= 100;
			data[i+1] ^= 212;
		}
	}
}
//--------------------------------------------------------------------------------------------------
// Unmanaged function for returning the length of the array to be allocated for holding the key
size_t getDestinationLength() { return _LENGTH/2; }

#pragma managed
//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
void ExtractEncryption::EncryptFile(String^ fileToEncrypt, String^ encryptedFileName,
									bool overwrite, MapLabel^ mapLabel)
{
	try
	{
		// Ensure calling assembly is signed by Extract
		ExtractException::Assert("ELI22627", "Failed internal data check!",
			CheckData(Assembly::GetCallingAssembly()));

		// Check that the file names are not null or empty
		ExtractException::Assert("ELI22628", "File names must be specified!",
			!String::IsNullOrEmpty(fileToEncrypt) && !String::IsNullOrEmpty(encryptedFileName));

		// Ensure that the file to encrypt exists
		ExtractException::Assert("ELI22629", "File does not exist!", File::Exists(fileToEncrypt),
			"File to encrypt", fileToEncrypt);

		// Ensure that if overwrite is not specified that the
		// file to output does not exist
		ExtractException::Assert("ELI22630", "File already exists!",
			(overwrite || File::Exists(encryptedFileName)),
			"Encrypted file name", encryptedFileName);

		// Get all the bytes from the file and encrypt them
		array<Byte>^ cipherBytes = Encrypt(File::ReadAllBytes(fileToEncrypt));

		// Write the encrypted file
		File::WriteAllBytes(encryptedFileName, cipherBytes);
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI22631", ex);
	}
}
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
String^ ExtractEncryption::EncryptString(String^ data, MapLabel^ mapLabel)
{
	try
	{
		// Ensure calling assembly is signed by Extract
		ExtractException::Assert("ELI22632", "Failed internal data check!",
			CheckData(Assembly::GetCallingAssembly()));

		// Create the return string
		String^ returnData = gcnew String("");

		// Check if the specified string is null or empty
		if (!String::IsNullOrEmpty(data))
		{
			// Get the plain text as an array of bytes
			array<Byte>^ plainText = StringMethods::ConvertStringToBytes(data);

			// Encrypt the bytes and convert the encrypted bytes to a string
			returnData = StringMethods::ConvertBytesToHexString(Encrypt(plainText));
		}

		// Return the encrypted string
		return returnData;
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI22633", ex);
	}
}
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
void ExtractEncryption::EncryptTextFile(System::String ^data, System::String ^encryptedFileName,
										bool overwrite, Extract::Licensing::MapLabel ^mapLabel)
{
	try
	{
		// Ensure calling assembly is signed by Extract
		ExtractException::Assert("ELI23020", "Failed internal data check!",
			CheckData(Assembly::GetCallingAssembly()));

		// Check that the file name is not null or empty
		ExtractException::Assert("ELI23021", "File name must be specified!",
			!String::IsNullOrEmpty(encryptedFileName));

		// Ensure that if overwrite is not specified that the
		// file to output does not exist
		ExtractException::Assert("ELI23022", "File already exists!",
			(overwrite || File::Exists(encryptedFileName)),
			"Encrypted file name", encryptedFileName);

		// Encrypt the string
		array<Byte>^ encryptedBytes = Encrypt(StringMethods::ConvertStringToBytes(data));

		// Write the encrypted data to the file
		File::WriteAllBytes(encryptedFileName, encryptedBytes);
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI23023", ex);
	}
}
//--------------------------------------------------------------------------------------------------
void ExtractEncryption::EncryptStream(Stream^ plainData, Stream^ cipherData, String^ password)
{
	try
	{
		Encrypt(plainData, cipherData, ComputeHash(password, HashVersion));
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI32315", ex);
	}
}
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
void ExtractEncryption::EncryptStream(Stream^ plainData, Stream^ cipherData, array<Byte>^ password,
	MapLabel^ mapLabel)
{
	try
	{
		// Ensure calling assembly is signed by Extract
		ExtractException::Assert("ELI32363", "Failed internal data check.",
			CheckData(Assembly::GetCallingAssembly()));

		Encrypt(plainData, cipherData, password);
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI32316", ex);
	}
}
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
array<Byte>^ ExtractEncryption::DecryptBinaryFile(String^ fileName, MapLabel^ mapLabel)
{
	try
	{
		// Ensure calling assembly is signed by Extract
		ExtractException::Assert("ELI22634", "Failed internal data check!",
			CheckData(Assembly::GetCallingAssembly()));

		// Ensure that the file name is not null or empty
		ExtractException::Assert("ELI22635", "File name must be specified!",
			!String::IsNullOrEmpty(fileName));

		// Ensure that the file to decrypt exists
		ExtractException::Assert("ELI22636", "File does not exist!", File::Exists(fileName),
			"File to decrypt", fileName);

		// Decrypt the data from the file
		array<Byte>^ returnData = Decrypt(File::ReadAllBytes(fileName));

		// Return the decrypted data
		return returnData;
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI22637", ex);
	}
}
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
String^ ExtractEncryption::DecryptTextFile(String^ fileName, Encoding^ encoding, MapLabel^ mapLabel)
{
	try
	{
		// Ensure calling assembly is signed by Extract
		ExtractException::Assert("ELI22660", "Failed internal data check!",
			CheckData(Assembly::GetCallingAssembly()));

		// Ensure that the file name is not null or empty
		ExtractException::Assert("ELI22661", "File name must be specified!",
			!String::IsNullOrEmpty(fileName));

		// Ensure that the file to decrypt exists
		ExtractException::Assert("ELI22662", "File does not exist!", File::Exists(fileName),
			"File to decrypt", fileName);

		// Get the decrypted bytes from the file
		array<Byte>^ plainBytes = Decrypt(File::ReadAllBytes(fileName));

		// Return the data as a String^ with the specified Encoding
		return encoding->GetString(plainBytes);
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI22663", ex);
	}
}
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
String^ ExtractEncryption::DecryptString(String^ data, MapLabel^ mapLabel) 
{
	try
	{
		// Ensure calling assembly is signed by Extract
		ExtractException::Assert("ELI22638", "Failed internal data check!",
			CheckData(Assembly::GetCallingAssembly()));

		// Create the return string
		String^ returnData = gcnew String("");

		// Check if the specified string is null or empty
		if (!String::IsNullOrEmpty(data))
		{
			// Get the cipher text as an array of bytes
			array<Byte>^ cipherText = StringMethods::ConvertHexStringToBytes(data);

			// Decrypt the bytes and convert the decrypted bytes to a string
			returnData = StringMethods::ConvertBytesToString(Decrypt(cipherText));
		}

		// Return the decrypted data
		return returnData;
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI22639", ex);
	}
}
//--------------------------------------------------------------------------------------------------
void ExtractEncryption::DecryptStream(Stream^ cipherData, Stream^ plainData, String^ password)
{
	try
	{
		Decrypt(cipherData, plainData, password);
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI32313", ex);
	}
}
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
void ExtractEncryption::DecryptStream(Stream^ cipherData, Stream^ plainData, array<Byte>^ password,
	MapLabel^ mapLabel)
{
	try
	{
		// Ensure calling assembly is signed by Extract
		ExtractException::Assert("ELI32326", "Failed internal data check.",
			CheckData(Assembly::GetCallingAssembly()));

		// Get the version number from the stream
		auto version = GetEncryptedStreamVersion(cipherData);

		Decrypt(cipherData, plainData, version, password);
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI32314", ex);
	}
}
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
array<Byte>^ ExtractEncryption::GetHashedBytes(String^ value, int version, MapLabel^ mapLabel)
{
	try
	{
		// Ensure calling assembly is signed by Extract
		ExtractException::Assert("ELI32369", "Failed internal data check.",
			CheckData(Assembly::GetCallingAssembly()));

		return ComputeHash(value, version);
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI32370", ex);
	}
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
array<Byte>^ ExtractEncryption::Encrypt(array<Byte>^ plainBytes)
{
	RijndaelManaged^ rjndl = nullptr;
	RSACryptoServiceProvider^ rsa = nullptr;
	MemoryStream^ memoryStream = nullptr;
	CryptoStream^ cryptoStream = nullptr;
	try
	{
		// Get a Rijndael instance for performing the encryption
		rjndl = GetRijndael();

		// Get an RSA instance for performing encryption
		rsa = GetRSA();

		// Encrypt the Rijndael key
		array<Byte>^ keyEncrypted = rsa->Encrypt(rjndl->Key, false);

		// Create the memory stream to write to
		memoryStream = gcnew MemoryStream();

		// Write the version, the encrypted key and the initialization vector to the stream
		memoryStream->Write(BitConverter::GetBytes(_VERSION), 0, 4);
		memoryStream->Write(keyEncrypted, 0, keyEncrypted->Length);
		memoryStream->Write(rjndl->IV, 0, rjndl->IV->Length);

		// Create a CryptoStream to write the encrypted data
		cryptoStream = gcnew CryptoStream(memoryStream, rjndl->CreateEncryptor(),
			CryptoStreamMode::Write);

		// Write all of the bytes into the crypto stream
		for each(Byte byte in plainBytes)
		{
			cryptoStream->WriteByte(byte);
		}

		// Flush the final bytes in the stream
		cryptoStream->FlushFinalBlock();

		// Return the encrypted bytes
		array<Byte>^ data = memoryStream->ToArray();

		// Scope for pin_ptr
		{
			// Wrap the cipher bytes
			pin_ptr<Byte> p = &data[0];
			wrapData((unsigned char*) p, data->LongLength, _VERSION);
		}

		return data;
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI22640", ex);
	}
	finally
	{
		// Close the streams and clear the encryption objects
		if (cryptoStream != nullptr)
		{
			cryptoStream->Close();
		}
		if (memoryStream != nullptr)
		{
			memoryStream->Close();
		}
		if (rsa != nullptr)
		{
			rsa->Clear();
		}
		if (rjndl != nullptr)
		{
			rjndl->Clear();
		}
	}
}
//--------------------------------------------------------------------------------------------------
void ExtractEncryption::Encrypt(Stream^ plain, Stream^ cipher, array<Byte>^ passwordHash)
{
	RijndaelManaged^ rjndl = nullptr;
	CryptoStream^ cryptoStream = nullptr;
	try
	{
		// Write the tag and version number to the stream
		auto encoding = gcnew UnicodeEncoding();
		auto name = encoding->GetBytes(_STREAM_ENCRYPT_TAG);
		auto length = name->Length;
		cipher->Write(BitConverter::GetBytes(length), 0, 4);
		cipher->Write(name, 0, length);
		cipher->Write(BitConverter::GetBytes(_PASSWORD_ENCRYPT_VERSION), 0, 4);

		rjndl = GetRijndael(passwordHash, HashVersion);
		cryptoStream = gcnew CryptoStream(cipher, rjndl->CreateEncryptor(),
			CryptoStreamMode::Write);
		auto buffer = gcnew array<Byte>(_BUFFER_SIZE);
		auto bytes = plain->Read(buffer, 0, _BUFFER_SIZE);
		while (bytes > 0)
		{
			cryptoStream->Write(buffer, 0, bytes);
			bytes = plain->Read(buffer, 0, _BUFFER_SIZE);
		}
		cryptoStream->FlushFinalBlock();
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI32306", ex);
	}
	finally
	{
		// Close the streams and clear the encryption objects
		if (cryptoStream != nullptr)
		{
			cryptoStream->Close();
		}
		if (rjndl != nullptr)
		{
			rjndl->Clear();
		}
	}
}
//--------------------------------------------------------------------------------------------------
array<Byte>^ ExtractEncryption::Decrypt(array<Byte>^ cipherBytes)
{
	RijndaelManaged^ rjndl = nullptr;
	RSACryptoServiceProvider^ rsa = nullptr;
	MemoryStream^ memoryStream = nullptr;
	CryptoStream^ cryptoStream = nullptr;
	try
	{
		// Ensure calling assembly is signed by Extract
		ExtractException::Assert("ELI22652", "Failed internal data check!",
			CheckData(Assembly::GetCallingAssembly()));

		// Create an array to hold version number
		array<Byte>^ version = gcnew array<Byte>(4);

		// Get the version number
		Array::Copy(cipherBytes, 0, version, 0, 4);
		int versionNumber = BitConverter::ToInt32(version, 0);

		// Check the version
		if (versionNumber > _VERSION)
		{
			// Throw an exception
			ExtractException^ ee = gcnew ExtractException("ELI22676",
				"Unrecognized version number!");
			ee->AddDebugData("Current version", _VERSION, false);
			ee->AddDebugData("Version to load", versionNumber, false);
			throw ee;
		}

		// Scope for pin_ptr
		{
			// Unwrap the cipher bytes
			pin_ptr<Byte> p = &cipherBytes[0];
			unwrapData((unsigned char*) p, cipherBytes->LongLength, versionNumber);
		}

		// Get the key
		array<Byte>^ key = gcnew array<Byte>(_KEY_SIZE);
		Array::Copy(cipherBytes, 4, key, 0, _KEY_SIZE);

		// Get the initialization vector
		array<Byte>^ iv = gcnew array<Byte>(_IV_LENGTH);
		Array::Copy(cipherBytes, 4 + _KEY_SIZE, iv, 0, _IV_LENGTH);
		
		// Get a Rijndael instance for performing the decryption
		rjndl = GetRijndael();

		// Get an RSA instance for performing decryption
		rsa = GetRSA();

		// Create a new memory stream to hold the decrypted data
		memoryStream = gcnew MemoryStream();

		// Create the CryptoStream for decrypting the remaining bytes
		cryptoStream = gcnew CryptoStream(memoryStream,
			rjndl->CreateDecryptor(rsa->Decrypt(key, false), iv), CryptoStreamMode::Write);

		// Create an array to hold the remainder of the bytes and copy the bytes to it
		array<Byte>^ temp = gcnew array<Byte>(cipherBytes->Length - (4 + _KEY_SIZE + _IV_LENGTH));
		Array::Copy(cipherBytes, (4 + _KEY_SIZE + _IV_LENGTH), temp, 0, temp->Length);

		// For all of the remaining bytes in the cipher text, decrypt them
		for each(Byte byte in temp)
		{
			cryptoStream->WriteByte(byte);
		}

		// Flush the final block
		cryptoStream->FlushFinalBlock();

		// Return the decrypted bytes
		return memoryStream->ToArray();
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI22647", ex);
	}
	finally
	{
		// Close the streams and clear the encryption objects
		if (cryptoStream != nullptr)
		{
			cryptoStream->Close();
		}
		if (memoryStream != nullptr)
		{
			memoryStream->Close();
		}
		if (rsa != nullptr)
		{
			rsa->Clear();
		}
		if (rjndl != nullptr)
		{
			rjndl->Clear();
		}
	}
}
//--------------------------------------------------------------------------------------------------
void ExtractEncryption::Decrypt(Stream^ cipher, Stream^ plain, String^ password)
{
	try
	{
		auto version = GetEncryptedStreamVersion(cipher);
		switch(version)
		{
		case 1:
			Decrypt(cipher, plain, version, ComputeHash(password, 1));
			break;

		default:
			auto ee = gcnew ExtractException("ELI32320", "Unrecognized version.");
			ee->AddDebugData("Version Specified", version, false);
			throw ee;
		}
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI32319", ex);
	}
}
//--------------------------------------------------------------------------------------------------
void ExtractEncryption::Decrypt(Stream^ cipher, Stream^ plain, int version,
	array<Byte>^ passwordHash)
{
	switch(version)
	{
	case 1:
		{
			RijndaelManaged^ rjndl = nullptr;
			CryptoStream^ cryptoStream = nullptr;
			try
			{
				rjndl = GetRijndael(passwordHash, 1);
				cryptoStream = gcnew CryptoStream(plain, rjndl->CreateDecryptor(),
					CryptoStreamMode::Write);
				auto buffer = gcnew array<Byte>(_BUFFER_SIZE);
				auto bytes = cipher->Read(buffer, 0, _BUFFER_SIZE);
				while (bytes > 0)
				{
					cryptoStream->Write(buffer, 0, bytes);
					bytes = cipher->Read(buffer, 0, _BUFFER_SIZE);
				}
				cryptoStream->FlushFinalBlock();
			}
			catch(Exception^ ex)
			{
				throw ExtractException::AsExtractException("ELI32307", ex);
			}
			finally
			{
				// Close the streams and clear the encryption objects
				if (cryptoStream != nullptr)
				{
					cryptoStream->Close();
				}
				if (rjndl != nullptr)
				{
					rjndl->Clear();
				}
			}
		}
		break;

	default:
		auto ee = gcnew ExtractException("ELI32321", "Unrecognized version.");
		ee->AddDebugData("Version Number", version, false);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
int ExtractEncryption::GetEncryptedStreamVersion(Stream^ cipherStream)
{
	try
	{
		auto encoding = gcnew UnicodeEncoding();
		auto data = gcnew array<Byte>(4);

		// Read the tag from the stream
		cipherStream->Read(data, 0, 4);
		auto length = BitConverter::ToInt32(data, 0);
		auto tag = gcnew array<Byte>(length);
		cipherStream->Read(tag, 0, length);

		// Validate the tag
		if (!_STREAM_ENCRYPT_TAG->Equals(encoding->GetString(tag), StringComparison::Ordinal))
		{
			// Throw an exception
			auto ee = gcnew ExtractException("ELI32309", "Unrecognized tag in stream.");
			ee->AddDebugData("Tag Loaded", encoding->GetString(tag), true);
			ee->AddDebugData("Tag Expected", _STREAM_ENCRYPT_TAG, true);
			throw ee;
		}

		// Check the version
		Array::Clear(data, 0, 4);
		cipherStream->Read(data, 0, 4);
		auto version = BitConverter::ToInt32(data, 0);
		if (version > _PASSWORD_ENCRYPT_VERSION)
		{
			// Throw an exception
			auto ee = gcnew ExtractException("ELI32310", "Unrecognized version number.");
			ee->AddDebugData("Current version", _PASSWORD_ENCRYPT_VERSION, false);
			ee->AddDebugData("Version to load", version, false);
			throw ee;
		}

		return version;
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI32318", ex);
	}
}
//--------------------------------------------------------------------------------------------------
array<Byte>^ ExtractEncryption::CreateInternalArray()
{
	try
	{
		return StringMethods::ConvertHexStringToBytes(Constants::ExtractPublicKey);
	}
	catch(Exception^ ex)
	{
		throw gcnew ExtractException("ELI22641", "Failed while initializing Encryption!", ex);
	}
}
//--------------------------------------------------------------------------------------------------
bool ExtractEncryption::CheckData(Assembly^ assembly)
{
	ExtractException::Assert("ELI22642", "Assembly is null!", assembly != nullptr);

	// Get the name from the assembly
	AssemblyName^ assemblyName = assembly->GetName();

	if (assemblyName != nullptr)
	{
		// Get the public key for this assembly
		array<Byte>^ publicKey = assemblyName->GetPublicKey();

		if (publicKey != nullptr && publicKey->Length != 0)
		{
			// Build a StrongNameMembershipCondition from the assembly
			// and our internal public key
			StrongNameMembershipCondition^ mc =
				gcnew StrongNameMembershipCondition(gcnew StrongNamePublicKeyBlob(_myArray),
				assemblyName->Name, assemblyName->Version);

			// Check the StrongNameMembershipCondition against this assembly
			// (if the assembly has a different public key than our internal
			// public key then this check will return false)
			return mc->Check(assembly->Evidence);
		}
	}

	// assemblyName is null or publicKey is either null or empty
	return false;
}
//--------------------------------------------------------------------------------------------------
RijndaelManaged^ ExtractEncryption::GetRijndael()
{
	// Create the Rijndael encryption object and set the properties
	RijndaelManaged^ rjndl = gcnew RijndaelManaged();
	rjndl->KeySize = _KEY_SIZE;
	rjndl->BlockSize = _KEY_SIZE;
	rjndl->Mode = CipherMode::CBC;
	rjndl->Padding = PaddingMode::ISO10126;

	// Return the new object
	return rjndl;
}
//--------------------------------------------------------------------------------------------------
RijndaelManaged^ ExtractEncryption::GetRijndael(array<Byte>^ passwordHash, int hashVersion)
{
	if (passwordHash->Length != 64)
	{
		auto ee = gcnew ExtractException("ELI32312", "Invalid array length.");
		ee->AddDebugData("Array Length", passwordHash->Length, false);
		ee->AddDebugData("Required Length", 64, false);
	}

	array<Byte>^ otherHash = nullptr;

	// Create an array to hold the modified key
	array<Byte>^ keyBytes = gcnew array<Byte>(getDestinationLength());

	// Scope for pin_ptr
	{
		// Pin the array so we can pass it to the unmanaged code
		pin_ptr<Byte> p = &keyBytes[0];

		// Copy the data into the array
		modifyData((unsigned char*)p);

		// Get the hash of the key data
		otherHash = ComputeHash(keyBytes, hashVersion);

		// Clear the array
		Array::Clear(keyBytes,0, keyBytes->Length);
	}

	auto combined = gcnew List<Byte>();
	auto iv = gcnew List<Byte>();
	for(int i=0; i < otherHash->Length; i ++)
	{
		auto value = (Byte)(passwordHash[i] ^ otherHash[i]);
		if (i % 2 == 0)
		{
			combined->Add(value);
		}
		else
		{
			iv->Add((Byte)(value ^ 42));
		}
	}

	// Create the Rijndael encryption object and set the properties
	RijndaelManaged^ rjndl = gcnew RijndaelManaged();
	rjndl->KeySize = _KEY_SIZE;
	rjndl->BlockSize = _KEY_SIZE;
	rjndl->Mode = CipherMode::CBC;
	rjndl->Padding = PaddingMode::ISO10126;
	rjndl->Key = combined->ToArray();
	rjndl->IV = iv->ToArray();

	// Return the new object
	return rjndl;
}
//--------------------------------------------------------------------------------------------------
RSACryptoServiceProvider^ ExtractEncryption::GetRSA()
{
	// Ensure calling assembly is signed by Extract
	ExtractException::Assert("ELI23280", "Failed internal data check!",
		CheckData(Assembly::GetCallingAssembly()));

	// Get an RSA instance for performing encryption
	RSACryptoServiceProvider^ rsa = gcnew RSACryptoServiceProvider();

	// Create an array to hold the modified key
	array<Byte>^ keyBytes = gcnew array<Byte>(getDestinationLength());

	// Scope for pin_ptr
	{
		// Pin the array so we can pass it to the unmanaged code
		pin_ptr<Byte> p = &keyBytes[0];

		// Copy the data into the array
		modifyData((unsigned char*)p);

		// Import the array
		rsa->ImportCspBlob(keyBytes);

		// Clear the array
		Array::Clear(keyBytes,0, keyBytes->Length);
	}
	// pin_ptr released

	// Return the new object
	return rsa;
}
//--------------------------------------------------------------------------------------------------
array<Byte>^ ExtractEncryption::ComputeHash(String^ value, int version)
{
	auto encoding = gcnew UnicodeEncoding();
	return ComputeHash(encoding->GetBytes(value), version);
}
//--------------------------------------------------------------------------------------------------
array<Byte>^ ExtractEncryption::ComputeHash(array<Byte>^ value, int version)
{
	switch(version)
	{
	case 1:
		{
			auto sha = gcnew SHA512Managed();
			return sha->ComputeHash(value);
		}

	default:
		auto ee =  gcnew ExtractException("ELI32317", "Invalid version specified.");
		ee->AddDebugData("Version Specified", version, false);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------