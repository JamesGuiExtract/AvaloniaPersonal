using NUnit.Framework;
using System;
using System.Text;

namespace Extract.ErrorHandling.Encryption.Test
{
    [TestFixture()]
    public class EncryptionEngineTests
    {
        [Test()]
        [Category("Automated")]
        public void EncryptDecryptTest()
        {
            string test = "This is a Test";
            var length = (test.Length % 8 > 0) ? test.Length + 8 - test.Length % 8 : test.Length;
            var testBytes = new Byte[length];
            Encoding.ASCII.GetBytes(test).CopyTo(testBytes, 0);

            var outBytes = new Byte[length];

            var key = new byte[16];

            new Random().NextBytes(key);
            EncryptionEngine.Encrypt(testBytes, key, outBytes);

            var decrypted = new Byte[length];
            EncryptionEngine.Decrypt(outBytes, key, decrypted);

            Assert.AreEqual(test, Encoding.ASCII.GetString(decrypted).Substring(0,test.Length));
        }

        [Test()]
        [Category("Automated")]
        public void EncryptSDecryptSTest()
        {
            string test = "This is another test but using hex encoded.";

            var hexString = Encoding.ASCII.GetBytes(test);
            var byteArray = new Byte[hexString.Length + 8 - hexString.Length % 8];
            hexString.CopyTo(byteArray, 0);

            var key = new byte[16];

            new Random().NextBytes(key);
            var outputString = EncryptionEngine.EncryptS(byteArray, key);

            var outputBytes = EncryptionEngine.DecryptS(outputString, key);

            Assert.AreEqual(test, Encoding.ASCII.GetString(outputBytes).Substring(0, test.Length));
            
        }
    }
}