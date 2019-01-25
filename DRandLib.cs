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

        [OpCode(OpCode.ABS)]
        public extern static BigInteger Abs(this BigInteger data);


        // reduce between [0, max). Example: Reduce(7, 4) => 7 % 4 = 3
        public static BigInteger Reduce(BigInteger x, BigInteger max)
        {
            // this could introduce some bias towards smaller numbers
            return x % max;
        }

        // reduce between [begin,end).  Example: Reduce(7, 1, 3) => 7 % (3-1) + 1 = 2
        public static BigInteger ReduceInterval(BigInteger x, BigInteger begin, BigInteger end)
        {
            // this could introduce some bias towards smaller numbers
            return (x % (end - begin)) + begin;
        }

        // converts 256-bit hash as a non-negative integer < max, i.e., in interval [0, max)
        // max can be up to 256-bit big integer
        public static BigInteger RandHash256(this byte[] hash, BigInteger max)
        {
            // value cannot be negative (using Abs to not surpass 32-byte limit and keep positive)
            BigInteger bi = hash.AsBigInteger().Abs();
            Runtime.Notify("source number:");
            Runtime.Notify(bi);
            Runtime.Notify("max range:");
            Runtime.Notify(max);
            Runtime.Notify("reduced number:");
            BigInteger reduced = Reduce(bi, max);
            Runtime.Notify(reduced);
            return reduced;
        }

        // converts 256-bit hash as a non-negative integer in interval [begin, end)
        // begin < end can be up to 256-bit big integers
        public static BigInteger RandHash256Interval(this byte[] hash, BigInteger begin, BigInteger end)
        {
            // value cannot be negative (using Abs to not surpass 32-byte limit and keep positive)
            BigInteger bi = hash.AsBigInteger().Abs();
            BigInteger reduced = ReduceInterval(bi, begin, end);
            return reduced;
        }

        // Fisher-Yates random shuffle for [from, to) interval using 256-bit hash on input object array
        // returns next hash, using SHA-256
        // price: ~(to-from)*
        public static byte[] Shuffle256(this object[] array, int from, int to, byte[] hash)
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

            Runtime.Notify("finished shuffle!");

            return nextHash;
        }

        /*
        // Fisher-Yates random shuffle for [from, to) interval using 256-bit hash on input byte array
        // returns updated byte array
        // price: this may require several SHA-256 in a single round
        public static sbyte[] ShuffleBytes(this sbyte[] array, int from, int to, byte[] hash)
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

            Runtime.Notify("finished shuffle!");

            return nextHash;
        }
        */
    }

    public class RandomShuffle : Framework.SmartContract
    {
        public static byte[] Main(byte[] b)
        {
            sbyte[] sb = b.AsSbyteArray();
            Runtime.Notify(sb);
            Runtime.Notify(sb.Length);

            object[] array = new object[sb.Length];

            int i=0;
            for(i=0; i<sb.Length; i++)
            {
                BigInteger bi1 = sb[i];
                array[i] = bi1;
                //array[i] = new BigInteger(sb[i]); // strange problem: TODO
            }

            //Runtime.Notify(array.Length);

            byte[] nextHash = array.Shuffle256(0, array.Length, b.SHA256());

            for(i=0; i<array.Length; i++)
            {
                sbyte elem = ((BigInteger)array[i]).AsSbyte();
                sb[i] = elem;
            //    sb[i] = ((BigInteger)(array[i])).AsSbyte(); // causes strange error: TODO
            }


            return sb.AsByteArray();
        }
    }
}
