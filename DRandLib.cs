// DRandLib: a decentralized randomization library
// Implementation for Neo blockchain on C# language
// Author: Igor M. Coelho
// MIT License - 2019

using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework;
using System.Numerics;
using Neo.VM;

namespace Neo.SmartContract
{
    public static class Help
    {
        [OpCode(OpCode.SHA256)]
        public extern static byte[] SHA256(this byte[] data);


        // Adjust sbyte to range [0, 255]
        //PUSHDATA1 = 0x4C 02 8000 (+128) ADD = 0x93
        [Script("4c02800093")]
        public extern static BigInteger Add128(this sbyte source);

        // Concat 0x00 to input byte array. Example: [ffaa] => [ffaa00]
        // PUSHM1 0x4F, INC 0x8B, CAT 0x7E
        [Script("4f8b7e")]
        public extern static byte[] ConcatZero(this byte[] source);

        [OpCode(OpCode.ABS)]
        public extern static BigInteger Abs(this BigInteger data);

        // converts 256-bit hash as a non-negative integer < max, i.e., in interval [0, max)
        // max can be up to 256-bit big integer
        public static BigInteger RandHash256(this byte[] hash, BigInteger max)
        {
            // value cannot be negative (using Abs since appending zero could surpass 32-byte limit)
            BigInteger x = hash.AsBigInteger().Abs();
            // reduce between [0, max). Example: Reduce(7, 4) => 7 % 4 = 3
            BigInteger reduced = x % max; // this could introduce some bias towards smaller numbers
            return reduced;
        }

        // converts 256-bit hash as a non-negative integer in interval [begin, end)
        // begin < end can be up to 256-bit big integers
        public static BigInteger RandHash256Interval(this byte[] hash, BigInteger begin, BigInteger end)
        {
            // value cannot be negative (using Abs since appending zero could surpass 32-byte limit)
            BigInteger x = hash.AsBigInteger().Abs();
            // reduce between [begin,end).  Example: Reduce(7, 1, 3) => 7 % (3-1) + 1 = 2
            BigInteger reduced = (x % (end - begin)) + begin; // this could introduce some bias towards smaller numbers
            return reduced;
        }

        // Fisher-Yates random shuffle for [from, to) interval using 256-bit hash on input object array
        // returns next hash, using SHA-256
        // price: ~(to-from)*
        public static byte[] ShuffleArray256Hash(this object[] array, int from, int to, byte[] hash)
        {
            int i;
            byte[] nextHash = hash;
            //int rand = (int)(nextHash.rand_hash(to - from)+from);
            //Runtime.Notify(from);
            //Runtime.Notify(to);
            //Runtime.Notify(rand);

            for(i = from; i < to; i++)
            {
                //int j = (int)(nextHash.rand_hash(to - i)+i);
                int j = (int)nextHash.RandHash256Interval(i, to);
                Runtime.Notify(i);
                Runtime.Notify(j);
                object obj = array[j];
                array[j] = array[i];
                array[i] = obj;
                nextHash = nextHash.SHA256();
            }

            //Runtime.Notify("finished shuffle!");

            return nextHash;
        }

        // Fisher-Yates random shuffle by swaps on chunk i=[from, to) j=[from, N) interval using
        //   baseHash (not exactly 32-byte) on given complete input array
        // returns updated byte array
        // nbytes define how many bytes are consumed on each random step
        // baseHash length should be >= (to-from)*nbytes
        // price: this only depends on from/to interval and baseHash calculation. If using Sha256, 10 GAS shuffles ~92 elements
        public static void ShuffleArrayChunk(this object[] array, int from, int to, byte[] baseHash, int nbytes=1)
        {
            // guarantee there is enough bytes to consume
            (baseHash.Length >= (to-from)*nbytes).Assert();
            int k = 0;
            for(int i = from; i < to; i++)
            {
                // concat zero to guarantee positive integers
                BigInteger x = baseHash.Range(k, nbytes).ConcatZero().ToBigInteger();
                k+=nbytes;
                // j in [i, len)
                int j = (int) ((x % (array.Length - i)) + i);
                object itemj = array[j];
                array[j] = array[i];
                array[i] = itemj;
            }
        }

        // Fisher-Yates random shuffle by swaps on chunk i=[from, to) j=[from, N) interval using
        //   baseHash (not exactly 32-byte) on given complete input bytearray
        // returns updated byte array
        // nbytes define how many bytes are consumed on each random step
        // baseHash length should be >= (to-from)*nbytes
        // price: this only depends on from/to interval and baseHash calculation. If using Sha256, 10 GAS shuffles ~92 elements
        public static byte[] ShuffleBytesChunk(this byte[] barray, int from, int to, byte[] baseHash, int nbytes=1)
        {
            // guarantee there is enough bytes to consume
            (baseHash.Length >= (to-from)*nbytes).Assert();
            sbyte[] array = barray.AsSbyteArray();
            int k = 0;
            for(int i = from; i < to; i++)
            {
                // concat zero to guarantee positive integers
                BigInteger x = baseHash.Range(k, nbytes).ConcatZero().ToBigInteger();
                k+=nbytes;
                // j in [i, len)
                int j = (int) ((x % (array.Length - i)) + i);
                sbyte itemj = array[j];
                sbyte itemi = array[i];
                array[j] = itemi;
                array[i] = itemj;
            }
            return array.AsByteArray(); // copy based array
        }

        // Fisher-Yates random shuffle by swaps using baseHash (less or more than 32-byte) on
        //    given subsequence [from, to) of input bytearray
        // returns updated byte array
        // nbytes define how many bytes are consumed on each random step
        // baseHash length should be >= (to-from)*nbytes
        // price: this only depends on from/to interval.
        public static byte[] ShuffleSubseqBytes(this byte[] barray, int from, int to, byte[] baseHash, int nbytes=1)
        {
            // guarantee there is enough bytes to consume
            (baseHash.Length >= (to-from)*nbytes).Assert();
            sbyte[] array = barray.AsSbyteArray();
            int k = 0;
            for(int i = from; i < to; i++)
            {
                // concat zero to guarantee positive integers
                BigInteger x = baseHash.Range(k, nbytes).ConcatZero().ToBigInteger();
                k+=nbytes;
                // j in [i, len)
                int j = (int) ((x % (to - i)) + i);
                sbyte itemj = array[j];
                sbyte itemi = array[i];
                array[j] = itemi;
                array[i] = itemj;
            }
            return array.AsByteArray(); // copy based array
        }

        // Fisher-Yates random shuffle using 256-bit hash on input byte array
        // returns updated byte array
        // price: this may require several SHA-256 in a single round. 10 GAS around ~96 elements
        public static byte[] ShuffleBytesSHA256(this byte[] barray, byte[] nextHash)
        {
            Runtime.Notify("initial hash");
            Runtime.Notify(nextHash);
            sbyte[] array = barray.AsSbyteArray();
            int i;
            //byte[] nextHash = hash;
            int len = array.Length;
            sbyte[] shash = nextHash.AsSbyteArray();
            //int rand = (int)(nextHash.rand_hash(to - from)+from);
            //Runtime.Notify(from);
            //Runtime.Notify(to);
            //Runtime.Notify(rand);
            int k = 0;
            for(i = 0; i < len; i++)
            {
                if(k == 32) // 32-bytes
                {
                    Runtime.Notify("generating new hash!");
                    nextHash = nextHash.SHA256(); // update hash
                    Runtime.Notify(nextHash);
                    shash = nextHash.AsSbyteArray();  // type convertion byte[] to sbyte[] (all information is kept original)
                    k = 0;
                }
                //int j = (int)(nextHash.rand_hash(to - i)+i);
                int j = (int) ((shash[k].Add128() % (len - i)) + i); //(int)nextHash.RandHash256Interval(i, to);
                //Runtime.Notify(i);
                //Runtime.Notify(j);
                sbyte itemj = array[j];
                sbyte itemi = array[i];
                array[j] = itemi;
                array[i] = itemj;
                k++;
            }
            return array.AsByteArray(); // copy based array
        }
    }

    public class RandomShuffle : Framework.SmartContract
    {
        public static byte[] Main(byte[] b)
        {
            Runtime.Notify(b);

            byte[] hash = b.SHA256();//.SHA256().SHA256().SHA256();
            Runtime.Notify(hash);

            //BigInteger bigx = hash.RandHash256(100);
            //Runtime.Notify(bigx);
            hash = hash.Concat(hash.SHA256());
            hash = hash.Concat(hash.SHA256());
            Runtime.Notify(hash); // 96 bytes
            //byte[] b1 = b.ShuffleBytesChunk(0, b.Length, hash);
            //byte[] b1 = hash;
            //byte[] b2 = b.ShuffleBytesChunk(0, 5, hash);
            //byte[] b3 = b2.ShuffleBytesChunk(5, b2.Length, hash.Range(5, hash.Length-3));

            //byte[] b4 = b.ShuffleSubseqBytes(2, b.Length-2, hash);
            //Runtime.Notify(b1);
            //Runtime.Notify(b2);
            //Runtime.Notify(b3);
            //Runtime.Notify(b4);


            sbyte[] sb = b.AsSbyteArray();
            Runtime.Notify(sb);
            object[] array = new object[sb.Length];

            int i=0;
            for(i=0; i<sb.Length; i++)
            {
                BigInteger bi1 = sb[i];
                array[i] = bi1;
                //array[i] = new BigInteger(sb[i]); // strange problem: TODO
            }

            array.ShuffleArrayChunk(0, array.Length, hash);

            for(i=0; i<array.Length; i++)
            {
                sbyte elem = ((BigInteger)array[i]).AsSbyte();
                sb[i] = elem;
            //    sb[i] = ((BigInteger)(array[i])).AsSbyte(); // causes strange error: TODO
            }

            Runtime.Notify(sb);

            return sb.AsByteArray();//b;//b1;
        }
    }
}
