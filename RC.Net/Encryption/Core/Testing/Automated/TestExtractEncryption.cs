using Extract.Licensing;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace Extract.Encryption
{
    /// <summary>
    /// Class for running NUnit tests against the ExtractEncryption class.
    /// </summary>
    [TestFixture]
    [Category("Extract Encryption")]
    public class TestExtractEncryption
    {
        /// <summary>
        /// Setup method to initalize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Tests encrypting a string of text.
        /// </summary>
        /// <param name="text">The text to encrypt.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptingText([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            // Encrypt the string
            string encryptedText = ExtractEncryption.EncryptString(text, new MapLabel());

            // Check that the encrypted string does not match the original string
            Assert.That(encryptedText != text);
        }

        /// <summary>
        /// Tests encrypting a string of text with the extension method.
        /// </summary>
        /// <param name="text">The text to encrypt.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptingTextExtension([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            // Encrypt the string
            string encryptedText = text.ExtractEncrypt(new MapLabel());

            // Check that the encrypted string does not match the original string
            Assert.That(encryptedText != text);
        }

        /// <summary>
        /// Tests encrypting a file.
        /// </summary>
        /// <param name="text">The text to write to the file that will be encrypted.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptingFile([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            using (TemporaryFile textFile = new TemporaryFile(true),
                tempEncrypted = new TemporaryFile(true))
            {
                // Get temporary file names
                string tempTextFile = textFile.FileName;
                string tempEncryptedFile = tempEncrypted.FileName;

                // Write the string to the temp text file
                File.WriteAllBytes(tempTextFile, Encoding.ASCII.GetBytes(text));

                // Encrypt the temp text file
                ExtractEncryption.EncryptFile(tempTextFile, tempEncryptedFile, true, new MapLabel());

                // Read the files back
                string encryptedFile = StringMethods.ConvertBytesToHexString(File.ReadAllBytes(
                    tempEncryptedFile));
                string plainFile = StringMethods.ConvertBytesToHexString(File.ReadAllBytes(
                    tempTextFile));

                // Check that the encrypted file bytes do not match the plain file bytes
                Assert.That(encryptedFile != plainFile);
            }
        }

        /// <summary>
        /// Tests encrypting a string of text to a file.
        /// </summary>
        /// <param name="text">The text to write to the file that will be encrypted.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptTextFile([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            using (TemporaryFile tempFile = new TemporaryFile(true))
            {
                // Get temporary file names
                string tempEncryptedFile = tempFile.FileName;

                // Encrypt the text to a file
                ExtractEncryption.EncryptTextFile(text, tempEncryptedFile, true, new MapLabel());

                // Convert the string to an array of bytes
                string plainBytes = StringMethods.ConvertBytesToHexString(
                    StringMethods.ConvertStringToBytes(text));

                // Read the encrypted file back
                string encryptedFile = StringMethods.ConvertBytesToHexString(
                    File.ReadAllBytes(tempEncryptedFile));

                // Check that the encrypted file bytes do not match the plain bytes
                Assert.That(encryptedFile != plainBytes);
            }
        }

        /// <summary>
        /// Tests decrypting a file that was created with EncryptTextFile
        /// </summary>
        /// <param name="text">The text to write to the file that will be encrypted.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestDecryptingEncryptTextFile([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            using (TemporaryFile tempFile = new TemporaryFile(true))
            {
                // Get temporary file names
                string tempEncryptedFile = tempFile.FileName;

                // Encrypt the text to a file
                ExtractEncryption.EncryptTextFile(text, tempEncryptedFile, true, new MapLabel());

                // Decrypt the text file
                string decryptedText = ExtractEncryption.DecryptTextFile(tempEncryptedFile,
                    Encoding.ASCII, new MapLabel());

                // Check that the decrypted file matches the original text
                Assert.That(decryptedText == text);
            }
        }

        /// <summary>
        /// Tests decrypting an encrypted string of text.
        /// </summary>
        /// <param name="text">The text that will be encrypted and then decrypted.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestDecryptingText([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            // Encrypt and then decrypt the text
            string encryptedText = ExtractEncryption.EncryptString(text, new MapLabel());
            string decryptedText = ExtractEncryption.DecryptString(encryptedText, new MapLabel());

            // Check that the encrypted text is not the same as the original and that
            // the decrypted text is
            Assert.That((encryptedText != text && decryptedText == text));
        }

        /// <summary>
        /// Tests decrypting an encrypted string of text with the extension method.
        /// </summary>
        /// <param name="text">The text that will be encrypted and then decrypted.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestDecryptingTextExtension([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            // Encrypt and then decrypt the text
            string encryptedText = ExtractEncryption.EncryptString(text, new MapLabel());
            string decryptedText = encryptedText.ExtractDecrypt(new MapLabel());

            // Check that the encrypted text is not the same as the original and that
            // the decrypted text is
            Assert.That((encryptedText != text && decryptedText == text));
        }

        /// <summary>
        /// Tests decrypting an ascii text file.
        /// </summary>
        /// <param name="text">The text that will be written to the file, encrypted, and then
        /// decrypted.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestDecryptingAsciiTextFile([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            using (TemporaryFile textFile = new TemporaryFile(true),
                tempEncrypted = new TemporaryFile(true))
            {
                // Get temporary file names
                string tempTextFile = textFile.FileName;
                string tempEncryptedFile = tempEncrypted.FileName;

                // Write the string to the temp text file
                File.WriteAllBytes(tempTextFile, Encoding.ASCII.GetBytes(text));

                // Encrypt the temp text file
                ExtractEncryption.EncryptFile(tempTextFile, tempEncryptedFile, true, new MapLabel());

                // Read the files back
                string encryptedFile = StringMethods.ConvertBytesToHexString(File.ReadAllBytes(
                    tempEncryptedFile));
                string plainFile = StringMethods.ConvertBytesToHexString(File.ReadAllBytes(
                    tempTextFile));

                // Decrypt the encrypted text file
                string decryptedText = ExtractEncryption.DecryptTextFile(tempEncryptedFile,
                    Encoding.ASCII, new MapLabel());

                // Check that the encrypted file bytes are not the same as the plain file
                // and that the decrypted text matches the original text
                Assert.That(encryptedFile != plainFile && text == decryptedText);
            }
        }

        /// <summary>
        /// Tests encrypting a string of text twice produces different encrypted strings.
        /// </summary>
        /// <param name="text">The text to encrypt.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptingTextTwiceProducesDifferentCiphers([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            // Encrypt the string
            string encryptedText1 = ExtractEncryption.EncryptString(text, new MapLabel());
            string encryptedText2 = ExtractEncryption.EncryptString(text, new MapLabel());

            // Check that neither of the encrypted strings match the original string or each other
            Assert.That(encryptedText1 != text
                && encryptedText2 != text
                && encryptedText1 != encryptedText2);
        }

        /// <summary>
        /// Tests encrypting the same file twice produces different encrypted files.
        /// </summary>
        /// <param name="text">The text to write to the file that will be encrypted.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptingFileTwiceProducesDifferentCipherFiles([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            using (TemporaryFile textFile = new TemporaryFile(true),
                tempEncrypted1 = new TemporaryFile(true),
                tempEncrypted2 = new TemporaryFile(true))
            {
                // Get temporary file names
                string tempTextFile = textFile.FileName;
                string tempEncryptedFile1 = tempEncrypted1.FileName;
                string tempEncryptedFile2 = tempEncrypted2.FileName;

                // Write the string to the temp text file
                File.WriteAllBytes(tempTextFile, Encoding.ASCII.GetBytes(text));

                // Encrypt the temp text file twice
                ExtractEncryption.EncryptFile(tempTextFile, tempEncryptedFile1, true, new MapLabel());
                ExtractEncryption.EncryptFile(tempTextFile, tempEncryptedFile2, true, new MapLabel());

                // Read the files back
                string encryptedFile1 = StringMethods.ConvertBytesToHexString(File.ReadAllBytes(
                    tempEncryptedFile1));
                string encryptedFile2 = StringMethods.ConvertBytesToHexString(File.ReadAllBytes(
                    tempEncryptedFile2));
                string plainFile = StringMethods.ConvertBytesToHexString(File.ReadAllBytes(
                    tempTextFile));

                // Check that the encrypted file bytes do not match the plain file bytes
                // or each other
                Assert.That(encryptedFile1 != plainFile
                    && encryptedFile2 != plainFile
                    && encryptedFile1 != encryptedFile2);
            }
        }

        /// <summary>
        /// Tests encrypting a string of text twice produces different encrypted strings and that
        /// both will decrypt to the original plain text.
        /// </summary>
        /// <param name="text">The text to encrypt.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptingTextTwiceProducesDifferentCiphersThatDecryptToSameText([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            // Encrypt the string
            string encryptedText1 = ExtractEncryption.EncryptString(text, new MapLabel());
            string encryptedText2 = ExtractEncryption.EncryptString(text, new MapLabel());

            string decryptedText1 = ExtractEncryption.DecryptString(encryptedText1, new MapLabel());
            string decryptedText2 = ExtractEncryption.DecryptString(encryptedText2, new MapLabel());

            // Check that neither of the encrypted strings match the original string or each other
            // and that both decrypted strings match the original
            Assert.That(encryptedText1 != text
                && encryptedText2 != text
                && encryptedText1 != encryptedText2
                && decryptedText1 == text
                && decryptedText2 == text);
        }

        /// <summary>
        /// Tests encrypting the same Ascii text twice produces two different cipher files
        /// that decrypt to the same Ascii text.
        /// </summary>
        /// <param name="text">The text that will be written to the file, encrypted, and then
        /// decrypted.</param>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptingAsciiFileTwiceProducesDifferentCipherFilesThatDecryptToSameAsciiText([Values(
            "This is a test string!",
            "This is\n\tanother\n\t\ttest string",
            "This test string is larger than the\nother test strings, but should still\t\t\n\n\tencrypt fine!",
            "This string is the longest\n\nof\tthem\tyet\n\t\t\but there should still\n\n\n\t\tbe\tno\nproblem\n\twith\t\t\tencrypting\nthis\ntext"
            )] string text)
        {
            using (TemporaryFile textFile = new TemporaryFile(true),
                tempEncrypted1 = new TemporaryFile(true),
                tempEncrypted2 = new TemporaryFile(true))
            {
                // Get temporary file names
                string tempTextFile = textFile.FileName;
                string tempEncryptedFile1 = tempEncrypted1.FileName;
                string tempEncryptedFile2 = tempEncrypted2.FileName;

                // Write the string to the temp text file
                File.WriteAllBytes(tempTextFile, Encoding.ASCII.GetBytes(text));

                // Encrypt the temp text file twice
                ExtractEncryption.EncryptFile(tempTextFile, tempEncryptedFile1, true, new MapLabel());
                ExtractEncryption.EncryptFile(tempTextFile, tempEncryptedFile2, true, new MapLabel());

                // Read the files back
                string encryptedFile1 = StringMethods.ConvertBytesToHexString(File.ReadAllBytes(
                    tempEncryptedFile1));
                string encryptedFile2 = StringMethods.ConvertBytesToHexString(File.ReadAllBytes(
                    tempEncryptedFile2));
                string plainFile = StringMethods.ConvertBytesToHexString(File.ReadAllBytes(
                    tempTextFile));

                // Decrypt the encrypted text files
                string decryptedText1 = ExtractEncryption.DecryptTextFile(tempEncryptedFile1,
                    Encoding.ASCII, new MapLabel());
                string decryptedText2 = ExtractEncryption.DecryptTextFile(tempEncryptedFile2,
                    Encoding.ASCII, new MapLabel());

                // Check that the encrypted file bytes are not the same as the plain file
                // and that the decrypted text matches the original text
                Assert.That(encryptedFile1 != plainFile
                    && encryptedFile2 != plainFile
                    && decryptedText1 == text
                    && decryptedText2 == text);
            }
        }

        /// <summary>
        /// Tests encrypting a stream.
        /// </summary>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptingStreamStringPassword()
        {
            string textToEncrypt = "This is the text to encrypt.";
            string password = "abcABC123;";
            var encoding = new UnicodeEncoding();
            using (MemoryStream plainStream = new MemoryStream(encoding.GetBytes(textToEncrypt)),
                cipherStream = new MemoryStream())
            {
                ExtractEncryption.EncryptStream(plainStream, cipherStream, password);
                cipherStream.Flush();
                var result = encoding.GetString(cipherStream.ToArray());

                Assert.That(!textToEncrypt.Equals(result, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// Tests encrypting a stream.
        /// </summary>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptingStreamBytePassword()
        {
            string textToEncrypt = "This is the text to encrypt.";
            string password = "abcABC123;";
            var encoding = new UnicodeEncoding();

            using (MemoryStream plainStream = new MemoryStream(encoding.GetBytes(textToEncrypt)),
                cipherStream = new MemoryStream())
            {
                var hash = ExtractEncryption.GetHashedBytes(password,
                    ExtractEncryption.HashVersion, new MapLabel());
                ExtractEncryption.EncryptStream(plainStream, cipherStream, hash,
                    new MapLabel());
                cipherStream.Flush();
                var result = encoding.GetString(cipherStream.ToArray());

                Assert.That(!textToEncrypt.Equals(result, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// Tests encrypting a stream via the extension method.
        /// </summary>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestEncryptingStreamExtension()
        {
            string textToEncrypt = "This is the text to encrypt.";
            string password = "abcABC123;";
            var encoding = new UnicodeEncoding();
            using (MemoryStream plainStream = new MemoryStream(encoding.GetBytes(textToEncrypt)),
                cipherStream = new MemoryStream())
            {
                plainStream.ExtractEncrypt(cipherStream, password);
                cipherStream.Flush();
                var result = encoding.GetString(cipherStream.ToArray());

                Assert.That(!textToEncrypt.Equals(result, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// Tests decrypting a stream.
        /// </summary>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestDecryptingStream()
        {
            string textToEncrypt = "This is the text to encrypt.";
            string password = "abcABC123;";
            var encoding = new UnicodeEncoding();
            byte[] cipher = null;
            using (MemoryStream plainStream = new MemoryStream(encoding.GetBytes(textToEncrypt)),
                cipherStream = new MemoryStream())
            {
                ExtractEncryption.EncryptStream(plainStream, cipherStream, password);
                cipherStream.Flush();
                cipher = cipherStream.ToArray();
            }

            using (MemoryStream plainStream = new MemoryStream(),
                cipherStream = new MemoryStream(cipher))
            {
                ExtractEncryption.DecryptStream(cipherStream, plainStream, password);
                plainStream.Flush();

                var result = encoding.GetString(plainStream.ToArray());
                Assert.That(textToEncrypt.Equals(result, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// Tests decrypting a stream.
        /// </summary>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestDecryptingStreamBytePassword()
        {
            string textToEncrypt = "This is the text to encrypt.";
            string password = "abcABC123;";
            var hash = ExtractEncryption.GetHashedBytes(password,
                ExtractEncryption.HashVersion, new MapLabel());

            var encoding = new UnicodeEncoding();
            byte[] cipher = null;
            using (MemoryStream plainStream = new MemoryStream(encoding.GetBytes(textToEncrypt)),
                cipherStream = new MemoryStream())
            {
                ExtractEncryption.EncryptStream(plainStream, cipherStream, hash,
                    new MapLabel());
                cipherStream.Flush();
                cipher = cipherStream.ToArray();
            }

            using (MemoryStream plainStream = new MemoryStream(),
                cipherStream = new MemoryStream(cipher))
            {
                ExtractEncryption.DecryptStream(cipherStream, plainStream, hash,
                    new MapLabel());
                plainStream.Flush();

                var result = encoding.GetString(plainStream.ToArray());
                Assert.That(textToEncrypt.Equals(result, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// Tests decrypting a stream via the extension method.
        /// </summary>
        [Test, Category("Automated")]
        [CLSCompliant(false)]
        public static void Automated_TestDecryptingStreamExtension()
        {
            string textToEncrypt = "This is the text to encrypt.";
            string password = "abcABC123;";
            var encoding = new UnicodeEncoding();
            byte[] cipher = null;
            using (MemoryStream plainStream = new MemoryStream(encoding.GetBytes(textToEncrypt)),
                cipherStream = new MemoryStream())
            {
                plainStream.ExtractEncrypt(cipherStream, password);
                cipherStream.Flush();
                cipher = cipherStream.ToArray();
            }

            using (MemoryStream plainStream = new MemoryStream(),
                cipherStream = new MemoryStream(cipher))
            {
                cipherStream.ExtractDecrypt(plainStream, password);
                plainStream.Flush();

                var result = encoding.GetString(plainStream.ToArray());
                Assert.That(textToEncrypt.Equals(result, StringComparison.Ordinal));
            }
        }
    }
}
