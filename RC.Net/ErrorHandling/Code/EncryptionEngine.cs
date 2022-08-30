using System;
using System.Reflection;

namespace Extract.ErrorHandling.Encryption
{
    static internal class EncryptionEngine
    {
        static private void verifyCaller(AssemblyName callingAssemblyName)
        {
            string callerPublicKey = callingAssemblyName.GetPublicKey().ToHexString(true);

            if (!callerPublicKey.Equals(Constants.ExtractPublicKey))
            {
                throw new ExtractException("ELI53548", "Invalid Caller");
            }
        }

        static public void Encrypt (Byte[] plain, Byte[] key, Byte[] cipher)
        {
            verifyCaller(Assembly.GetCallingAssembly().GetName());
            if (key.Length % 8 != 0)
                throw new ArgumentException("ELI51690: Must be a multiple of 8");
            if (plain.Length % 8 != 0)
                throw new ArgumentException("ELI51691: Must be a multiple of 8");
            if (cipher.Length != plain.Length)
                throw new ArgumentException($"ELI51692: Output buffer is {cipher.Length} instead of {plain.Length}");

            EncryptionEngine.Encrypt(
                new ArraySegment<Byte>(plain, 0, plain.Length),
                key,
                new ArraySegment<Byte>(cipher, 0, cipher.Length));
        }

        //[System.Diagnostics.Conditional("CONTRACTS_FULL")]
        static public void Encrypt (ArraySegment<Byte> plain, Byte[] key, ArraySegment<Byte> cipher)
        {
            verifyCaller(Assembly.GetCallingAssembly().GetName());
            if (key.Length % 8 != 0)
                throw new ArgumentException("ELI51684: Must be a multiple of 8");
            if (plain.Count % 8 != 0)
                throw new ArgumentException("ELI51685: Must be a multiple of 8");
            if (cipher.Count != plain.Count)
                throw new ArgumentException($"ELI51687: Output buffer is {cipher.Count} instead of {plain.Count}");

            IceKey iceKey = new IceKey(key.Length / 8);
            iceKey.Set(key);

            UInt32 numberOfIterations = (UInt32)plain.Count / 8;

            for (Int32 i = 0; i < numberOfIterations; i++)
            {
                var p = new ArraySegment<Byte>(plain.Array, plain.Offset + i * 8, 8);
                var c = new ArraySegment<Byte>(cipher.Array, cipher.Offset + i * 8, 8);
                iceKey.Encrypt(p, c);
            }
        }

        static public void Decrypt(Byte[] cipher, Byte[] key, Byte[] plain)
        {
            verifyCaller(Assembly.GetCallingAssembly().GetName());
            if (key.Length % 8 != 0)
                throw new ArgumentException("ELI51693: Must be a multiple of 8");
            if (plain.Length % 8 != 0)
                throw new ArgumentException("ELI51694: Must be a multiple of 8");
            if (cipher.Length != plain.Length)
                throw new ArgumentException($"ELI51695: Output buffer is {cipher.Length} instead of {plain.Length}");

            Decrypt(
                new ArraySegment<Byte>(cipher, 0, cipher.Length),
                key,
                new ArraySegment<Byte>(plain, 0, plain.Length));
        }

        //[System.Diagnostics.Conditional("CONTRACTS_FULL")]
        static public void Decrypt(ArraySegment<Byte> cipher, Byte[] key, ArraySegment<Byte> plain)
        {
            verifyCaller(Assembly.GetCallingAssembly().GetName());
            if (key.Length % 8 != 0)
                throw new ArgumentException("ELI51686: Must be a multiple of 8");
            if (cipher.Count % 8 != 0)
                throw new ArgumentException("ELI51685: Must be a multiple of 8");
            if (plain.Count != cipher.Count)
                throw new ArgumentException($"ELI51687: Output buffer is {plain.Count} instead of {plain.Count}");

            IceKey iceKey = new IceKey(key.Length / 8);
            iceKey.Set(key);

            UInt32 numberOfIterations = (UInt32)cipher.Count / 8;

            for (Int32 i = 0; i < numberOfIterations; i++)
            {
                var p = new ArraySegment<Byte>(plain.Array, plain.Offset + i * 8, 8);
                var c = new ArraySegment<Byte>(cipher.Array, cipher.Offset + i * 8, 8);

                iceKey.Decrypt(c, p);
            }
        }

        static public string EncryptS(Byte[] input, Byte[] key)
        {
            verifyCaller(Assembly.GetCallingAssembly().GetName());
            Byte[] scrambleKey = new Byte[4];
            new Random().NextBytes(scrambleKey);

            Byte[] output = new byte[input.Length + 4];

            scrambleKey.CopyTo(output, 0);

            ScrambleData(input, BitConverter.ToUInt32(scrambleKey, 0), true);

            Encrypt(
                new ArraySegment<Byte>(input, 0, input.Length),
                key,
                new ArraySegment<Byte>(output, 4, output.Length - 4));

            return output.ToHexString();
        }

        static public Byte[] DecryptS(string input, Byte[] key)
        {
            verifyCaller(Assembly.GetCallingAssembly().GetName());
            var inputBytes = input.FromHexString();

            UInt32 scrambleKey = BitConverter.ToUInt32(inputBytes, 0);

            Byte[] output = new byte[inputBytes.Length - 4];

            Decrypt(
                new ArraySegment<Byte>(inputBytes, 4, inputBytes.Length - 4),
                key,
                new ArraySegment<Byte>(output, 0, output.Length));

            ScrambleData(output, scrambleKey, false);

            return output;
        }

        static void ScrambleData(Byte[] data, UInt32 scrambleKey, bool scamble)
        {
            UInt32 length = (UInt32)data.Length;

            // For every other byte in the array, swap it with a byte that is half the array
            // size ahead +- 8 bytes. This position to swap with should roll over to the
            // beginning of the array once it gets past the end of it.
            // When unscrambling, swap the same bytes, but in the opposite order (start with the last even
            // numbered index in the array and work backwards).
            UInt32 nStart =  scamble ? 0: (length - 1) - ((length - 1) % 2);
            Int32 nIncrement = scamble ? 2 : -2;
            for (Int32 i = (Int32)nStart; (UInt32) i < length; i += nIncrement)
	{
                // Random offset will be a number between 0 and 15 based on i and nScrambleKey.
                UInt32 nRandomOffset = (scrambleKey >> (i % 32)) & 0xF;
                // Use the random offset to pick the byte to swap with.
                UInt32 nSwapPos = ((UInt32)i + (length / 2) - 8 + nRandomOffset);
                // Roll over once past the end of the array.
                nSwapPos = nSwapPos % length;
                Byte temp = data[i];
                data[i] = data[nSwapPos];
                data[nSwapPos] = temp;
            }
        }
    }
}
