// ExtractEncryption.h

#pragma once

using namespace Extract::Licensing;
using namespace System;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Security::Cryptography;
using namespace System::Text;

// This namespace only contains one type: ExtractEncryption
// As a general rule you should not create a new namespace for just one
// type, but this namespace has been created specificially for encryption
// and to obscure the some of the encryption functionality via unmanaged code
// so it is safe to ignore this warning.
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
						 Scope="namespace", Target="Extract.Encryption")];
namespace Extract
{
	namespace Encryption
	{
        //------------------------------------------------------------------------------------------
		public ref class ExtractEncryption sealed
		{
		public:
            //--------------------------------------------------------------------------------------
			// Public methods
            //--------------------------------------------------------------------------------------
			// PURPOSE: To encrypt a specified file
			//
			// ARGS:	fileToEncrypt - The name of the file to be encrypted
			//			encryptedFileName - The name of the encrypted file
			//			overwrite - Whether to overwrite the encrypted file or fail if
			//						it already exists
			//			mapLabel - Used to prevent calling this method as a delegate or
			//					   event handler which could potentially circumvent the
			//					   check used to determine if the calling assembly was
			//					   signed by Extract Systems
			// REQUIRE:	fileToEncrypt - Must not be null/empty and file must exist
			//			encryptedFileName - Must not be null/empty and if overwrite is false
			//								then it must not exist
			//			
			static void EncryptFile(String^ fileToEncrypt, String^ encryptedFileName, bool overwrite,
				MapLabel^ mapLabel);
            //--------------------------------------------------------------------------------------
			// PURPOSE:	To encrypt the specified string
			//
			// ARGS:	data - The string to be encrypted
			//			mapLabel - Used to prevent calling this method as a delegate or
			//					   event handler which could potentially circumvent the
			//					   check used to determine if the calling assembly was
			//					   signed by Extract Systems
			// RETURNS:	A string containing the encrypted data as a base64 encrypted string
			static String^ EncryptString(String^ data, MapLabel^ mapLabel);
            //--------------------------------------------------------------------------------------
			// PURPOSE:	To encrypt the specified string and store the encrypted data in a file
			//
			// ARGS:	data - The string to be encrypted
			//			encryptedFileName - The name of the encrypted file
			//			overwrite - Whether to overwrite the encrypted file or fail if
			//						it already exists
			//			mapLabel - Used to prevent calling this method as a delegate or
			//					   event handler which could potentially circumvent the
			//					   check used to determine if the calling assembly was
			//					   signed by Extract Systems
			//
			// REQUIRE:	data - Must not be null/empty
			//			encryptedFileName - Must not be null/empty and if overwrite is false
			//								then it must not exist
			static void EncryptTextFile(String^ data, String^ encryptedFileName, bool overwrite,
				MapLabel^ mapLabel);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To encrypt the bytes contained in the input stream and place them in
			//			the cipher data stream. The encryption key will be generated from
			//			the provided password.
			//
			// ARGS:	plainData - The input stream to read the data from
			//			cipherData - The output stream to write the encrypted bytes to
			//			password - The password used to generate the encryption key
			static void EncryptStream(Stream^ plainData, Stream^ cipherData, String^ password);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To decrypt the specified binary file
			//
			// ARGS:	fileName - The file to be decrypted
			//			mapLabel - Used to prevent calling this method as a delegate or
			//					   event handler which could potentially circumvent the
			//					   check used to determine if the calling assembly was
			//					   signed by Extract Systems
			// RETURNS:	An array of bytes containing the decrypted data
			static array<Byte>^ DecryptBinaryFile(String^ fileName, MapLabel^ mapLabel);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To decrypt the specified encrypted text file
			//
			// ARGS:	fileName - The file to be decrypted
			//			encoding - The encoding to use when converting the decrypted bytes
			//					   to a string (ASCII, UTF8, etc)
			//			mapLabel - Used to prevent calling this method as a delegate or
			//					   event handler which could potentially circumvent the
			//					   check used to determine if the calling assembly was
			//					   signed by Extract Systems
			// RETURNS:	A string containing the decrypted data
			static String^ DecryptTextFile(String^ fileName, Encoding^ encoding, MapLabel^ mapLabel);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To decrypt the specified string
			//
			// ARGS:	data - The string to be decrypted
			//			mapLabel - Used to prevent calling this method as a delegate or
			//					   event handler which could potentially circumvent the
			//					   check used to determine if the calling assembly was
			//					   signed by Extract Systems
			// RETURNS:	A string containing the decrypted data
			static String^ DecryptString(String^ data, MapLabel^ mapLabel);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To decrypt the bytes contained in the input stream and place them in
			//			the plain data stream. The decryption key will be generated from
			//			the provided password.
			//
			// ARGS:	cipherData - The input stream to read the encrypted bytes from
			//			plainData - The output stream to write the decrypted data to
			//			password - The password used to generate the decryption key
			static void DecryptStream(Stream^ cipherData, Stream^ plainData, String^ password);

		private:

			// Tag prepended to a stream that will be password encrypted
			static System::String^ _STREAM_ENCRYPT_TAG = "ExtractPasswordStreamEncryption";

            //--------------------------------------------------------------------------------------
			// Private variables
            //--------------------------------------------------------------------------------------
			// The array used to store the public key for this assembly
			static array<Byte>^ _myArray = CreateInternalArray();

            //--------------------------------------------------------------------------------------
			// Private methods
            //--------------------------------------------------------------------------------------
			// PURPOSE: To encrypt and array of bytes.
			static array<Byte>^ Encrypt(array<Byte>^ plainBytes);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To decrypt an array of bytes.
			static array<Byte>^ Decrypt(array<Byte>^ cipherBytes);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To create an array containing the public key data for this assembly
			static array<Byte>^ CreateInternalArray();
            //--------------------------------------------------------------------------------------
			// PURPOSE: To check the given assemblies public key against the public key for this
			//			assembly.  Returns true if they match or false otherwise.
			//
			// ARGS:	assembly - the Assembly whose public key will be checked.
			static bool CheckData(Assembly^ assembly);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To return a new instance of the RijndaelManaged encryption object.
			static RijndaelManaged^ GetRijndael();
            //--------------------------------------------------------------------------------------
			// PURPOSE: To return a new instance of the RijndaelManaged encryption object,
			//			with its key initialized by the specified password.
			static RijndaelManaged^ GetRijndael(System::String^ password);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To return a new instance of the RSA encryption object.
			static RSACryptoServiceProvider^ GetRSA();
            //--------------------------------------------------------------------------------------
			// PURPOSE:	Added to remove FxCop error - http://msdn.microsoft.com/en-us/ms182169.aspx
			//			Microsoft.Design::CA1053 - Static holder types should not have constructors
			ExtractEncryption(void) {};
		};

		[System::Runtime::CompilerServices::Extension]
		public ref class EncryptionExtensions abstract sealed
		{
		public:
			[System::Runtime::CompilerServices::Extension]
			static String^ ExtractEncrypt(String^ value, MapLabel^ mapLabel)
			{
				return ExtractEncryption::EncryptString(value, mapLabel);
			}

			[System::Runtime::CompilerServices::Extension]
			static String^ ExtractDecrypt(String^ value, MapLabel^ mapLabel)
			{
				return ExtractEncryption::DecryptString(value, mapLabel);
			}

			[System::Runtime::CompilerServices::Extension]
			static void ExtractEncrypt(Stream^ plainData, Stream^ cipherData, String^ password)
			{
				ExtractEncryption::EncryptStream(plainData, cipherData, password);
			}

			[System::Runtime::CompilerServices::Extension]
			static void ExtractDecrypt(Stream^ cipherData, Stream^ plainData, String^ password)
			{
				ExtractEncryption::DecryptStream(cipherData, plainData, password);
			}
		};
	};
};
