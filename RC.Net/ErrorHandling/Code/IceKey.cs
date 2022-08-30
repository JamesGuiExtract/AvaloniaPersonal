using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.ErrorHandling.Encryption
{
    internal struct IceSubkey
    {
        public UInt32[] val;
    };

    internal class IceKey
    {
        Int32[] IceKeyRot = { 0, 1, 2, 3, 2, 1, 3, 0, 1, 3, 2, 0, 3, 1, 0, 2 };

        static bool ice_sboxInitialized;

        static UInt32[,] ice_sbox = new UInt32[4, 1024];

        /* Modulo values for the S-boxes */
        static readonly Int32[,] ice_smod = new Int32[4, 4]
        { { 333, 313, 505, 369 }, { 379, 375, 319, 391 }, { 361, 445, 451, 397 }, { 397, 425, 395, 505 } };

        /* XOR values for the S-boxes */
        static readonly Int32[,] ice_sxor = new Int32[4, 4]
        {
        {
            0x83,
            0x85,
            0x9b,
            0xcd
        },
        {
            0xcc,
            0xa7,
            0xad,
            0x41
        },
        {
            0x4b,
            0x2e,
            0xd4,
            0x33
        },
        {
            0xea,
            0xcb,
            0x2e,
            0x04
        }
        };

        /* Permutation values for the P-box */
        static readonly UInt32[] ice_pbox = new UInt32[32]
        {
            0x00000001,
            0x00000080,
            0x00000400,
            0x00002000,
            0x00080000,
            0x00200000,
            0x01000000,
            0x40000000,
            0x00000008,
            0x00000020,
            0x00000100,
            0x00004000,
            0x00010000,
            0x00800000,
            0x04000000,
            0x20000000,
            0x00000004,
            0x00000010,
            0x00000200,
            0x00008000,
            0x00020000,
            0x00400000,
            0x08000000,
            0x10000000,
            0x00000002,
            0x00000040,
            0x00000800,
            0x00001000,
            0x00040000,
            0x00100000,
            0x02000000,
            0x80000000
        };

        public IceKey(Int32 n)
        {
            if (!ice_sboxInitialized)
            {
                ice_sboxes_init();
                ice_sboxInitialized = true;
            }

            if (n < 1)
            {
                size = 1;
                Rounds = 8;
            }
            else
            {
                size = n;
                Rounds = n * 16;
            }

            KeySched = new IceSubkey[Rounds];
            for (int i = 0; i < KeySched.Length; i++)
                KeySched[i].val = new UInt32[3];
        }

        ~IceKey()
        {
            for (int i = 0; i < Rounds; i++)
                for (int j = 0; j < 3; j++)
                    KeySched[i].val[j] = 0;
            Rounds = 0;
            KeySched = null;
        }

        public void Set(Byte[] key)
        {
            if (Rounds == 8)
            {
                UInt16[] kb = new UInt16[4];
                for (int i = 0; i < 4; i++)
                    kb[3 - i] = (UInt16)((key[i * 2] << 8) | key[i * 2 + 1]);

                ScheduleBuild(ref kb, 0, 0);
                return;
            }

            for (int i = 0; i < size; i++)
            {
                UInt16[] kb = new UInt16[4];
                for (int j = 0; j < 4; j++)
                    kb[3 - j] = (UInt16)((key[i * 8 + j * 2] << 8) | key[i * 8 + j * 2 + 1]);

                ScheduleBuild(ref kb, i * 8, 0);
                ScheduleBuild(ref kb, Rounds - 8 - i * 8, 8);
            }
        }

        /// <summary>
        /// Encrypt a block of 8 bytes of data
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public void Encrypt (ArraySegment<Byte> plainText, ArraySegment<Byte> cipherText)
        {
            Contract.Requires(plainText.Count == 8, "ELI51688: Must have exactly 8 bytes.");
            
            int i;
            UInt32 l, r;

            l = (((UInt32)plainText.Array[plainText.Offset]) << 24)

                | (((UInt32)plainText.Array[plainText.Offset + 1]) << 16)

                | (((UInt32)plainText.Array[plainText.Offset + 2]) << 8) | plainText.Array[plainText.Offset + 3];
            r = (((UInt32)plainText.Array[plainText.Offset + 4]) << 24) |
                (((UInt32)plainText.Array[plainText.Offset + 5]) << 16) |
                (((UInt32)plainText.Array[plainText.Offset + 6]) << 8) |
                plainText.Array[plainText.Offset + 7];

            for (i = 0; i < Rounds; i += 2)
            {
                l ^= ice_f(r, ref KeySched[i]);
                r ^= ice_f(l, ref KeySched[i + 1]);
            }


            for (i = 0; i < 4; i++)
            {
                cipherText.Array[cipherText.Offset + 3 - i] = (Byte)(r & 0xff);
                cipherText.Array[cipherText.Offset + 7 - i] = (Byte)(l & 0xff);

                r >>= 8;
                l >>= 8;
            }

        }

        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="CipherText"></param>
        /// <returns></returns>
        public void Decrypt(ArraySegment<Byte> cipherText, ArraySegment<Byte> planText)
        {
            Int32 i;
            UInt32 L, r;

            L = (((UInt32)cipherText.Array[cipherText.Offset]) << 24)

                | (((UInt32)cipherText.Array[cipherText.Offset + 1]) << 16)

                | (((UInt32)cipherText.Array[cipherText.Offset + 2]) << 8) | cipherText.Array[cipherText.Offset + 3];
            r = (((UInt32)cipherText.Array[cipherText.Offset + 4]) << 24)

                | (((UInt32)cipherText.Array[cipherText.Offset + 5]) << 16)

                | (((UInt32)cipherText.Array[cipherText.Offset + 6]) << 8) | cipherText.Array[cipherText.Offset + 7];

            for (i = Rounds - 1; i > 0; i -= 2)
            {
                L ^= ice_f(r, ref KeySched[i]);
                r ^= ice_f(L, ref KeySched[i - 1]);
            }

            for (i = 0; i < 4; i++)
            {
                planText.Array[planText.Offset + 3 - i] = (Byte)(r & 0xff);
                planText.Array[planText.Offset + 7 - i] = (Byte)(L & 0xff);

                r >>= 8;
                L >>= 8;
            }
        }


        public Int32 KeySize { get { return size * 8; } }

        public Int32 BlockSize { get; } = 8;

        void ScheduleBuild(ref UInt16[] kb, Int32 n, Int32 keyRotStartIndex)
        {
            int i;

            for (i = 0; i < 8; i++)
            {
                int j;
                int kr = IceKeyRot[keyRotStartIndex + i];
                int keySchedIndex = n + i;

                for (j = 0; j < 3; j++)
                    KeySched[keySchedIndex].val[j] = 0;

                for (j = 0; j < 15; j++)
                {
                    int k;
                    int curr_sk = j % 3;

                    for (k = 0; k < 4; k++)
                    {
                        int curr_kb = (kr + k) & 3;
                        UInt16 bit = (ushort)(kb[curr_kb] & 1);

                        KeySched[keySchedIndex].val[curr_sk] = (KeySched[keySchedIndex].val[curr_sk] << 1) | bit;
                        kb[curr_kb] = (ushort)((kb[curr_kb] >> 1) | ((bit ^ 1) << 15));
                    }
                }
            }
        }

        Int32 size { get; set; }

        Int32 Rounds { get; set; }

        IceSubkey[] KeySched { get; set; }


        /// <summary>
        /// * 8-bit Galois Field multiplication of a by b, modulo m. * Just like arithmetic multiplication, except that
        /// additions and * subtractions are replaced by XOR.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        static UInt32 gf_mult(UInt32 a, UInt32 b, UInt32 m)
        {
            UInt32 res = 0;

            while (b != 0)
            {
                if ((b & 1) != 0)
                    res ^= a;

                a <<= 1;
                b >>= 1;

                if (a >= 256)
                    a ^= m;
            }

            return (res);
        }

        /// <summary>
        /// * Galois Field exponentiation. * Raise the base to the power of 7, modulo m.
        /// </summary>
        static UInt32 gf_exp7(UInt32 b, UInt32 m)
        {
            UInt32 x;

            if (b == 0)
                return (0);

            x = gf_mult(b, b, m);
            x = gf_mult(b, x, m);
            x = gf_mult(x, x, m);
            return (gf_mult(b, x, m));
        }

        /// <summary>
        /// Carry out the ICE 32-bit P-box permutation.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        static UInt32 ice_perm32(UInt32 x)
        {
            UInt32 res = 0;
            UInt32 pbox = 0;

            while (x != 0)
            {
                if ((x & 1) != 0)
                    res |= ice_pbox[pbox];
                pbox++;
                x >>= 1;
            }

            return (res);
        }

        /// <summary>
        /// * Initialise the ICE S-boxes. * This only has to be done once.
        /// </summary>
        static void ice_sboxes_init()
        {
            for (int i = 0; i < 1024; i++)
            {
                int col = (i >> 1) & 0xff;
                int row = (i & 0x1) | ((i & 0x200) >> 8);
                UInt32 x;

                x = gf_exp7((UInt32)(col ^ ice_sxor[0, row]), (UInt32)ice_smod[0, row]) << 24;
                ice_sbox[0, i] = ice_perm32(x);

                x = gf_exp7((UInt32)(col ^ ice_sxor[1, row]), (UInt32)ice_smod[1, row]) << 16;
                ice_sbox[1, i] = ice_perm32(x);

                x = gf_exp7((UInt32)(col ^ ice_sxor[2, row]), (UInt32)ice_smod[2, row]) << 8;
                ice_sbox[2, i] = ice_perm32(x);

                x = gf_exp7((UInt32)(col ^ ice_sxor[3, row]), (UInt32)ice_smod[3, row]);
                ice_sbox[3, i] = ice_perm32(x);
            }
        }

        /// <summary>
        ///  The single round ICE f function.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="sk"></param>
        /// <returns></returns>
        static UInt32 ice_f(UInt32 p, ref IceSubkey sk)
        {
            UInt32 tl, tr;       /* Expanded 40-bit values */
            UInt32 al, ar;       /* Salted expanded 40-bit values */

            /* Left half expansion */
            tl = ((p >> 16) & 0x3ff) | (((p >> 14) | (p << 18)) & 0xffc00);

            /* Right half expansion */
            tr = (p & 0x3ff) | ((p << 2) & 0xffc00);

            /* Perform the salt permutation */
            // al = (tr & sk->val[2]) | (tl & ~sk->val[2]);
            // ar = (tl & sk->val[2]) | (tr & ~sk->val[2]);
            al = sk.val[2] & (tl ^ tr);
            ar = al ^ tr;
            al ^= tl;

            al ^= sk.val[0];       /* XOR with the subkey */
            ar ^= sk.val[1];

            /* S-box lookup and permutation */
            return (ice_sbox[0,al >> 10] | ice_sbox[1,al & 0x3ff] | ice_sbox[2,ar >> 10] | ice_sbox[3,ar & 0x3ff]);
        }
    }
}
