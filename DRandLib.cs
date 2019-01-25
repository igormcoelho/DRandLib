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

        [OpCode(OpCode.ABS)]
        public extern static BigInteger Abs(this BigInteger data);

        // converts 256-bit hash as a non-negative integer < max, i.e., in interval [0, max)
        // max can be up to 256-bit big integer
        public static BigInteger RandHash256(this byte[] hash, BigInteger max)
        {
            // value cannot be negative (using Abs to not surpass 32-byte limit and keep positive)
            BigInteger x = hash.AsBigInteger().Abs();
            Runtime.Notify("source number:");
            Runtime.Notify(x);
            Runtime.Notify("max range:");
            Runtime.Notify(max);
            Runtime.Notify("reduced number:");
            // reduce between [0, max). Example: Reduce(7, 4) => 7 % 4 = 3
            BigInteger reduced = x % max; // this could introduce some bias towards smaller numbers
            Runtime.Notify(reduced);
            return reduced;
        }

        // converts 256-bit hash as a non-negative integer in interval [begin, end)
        // begin < end can be up to 256-bit big integers
        public static BigInteger RandHash256Interval(this byte[] hash, BigInteger begin, BigInteger end)
        {
            // value cannot be negative (using Abs to not surpass 32-byte limit and keep positive)
            BigInteger x = hash.AsBigInteger().Abs();
            // reduce between [begin,end).  Example: Reduce(7, 1, 3) => 7 % (3-1) + 1 = 2
            BigInteger reduced = (x % (end - begin)) + begin; // this could introduce some bias towards smaller numbers
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

            //Runtime.Notify("finished shuffle!");

            return nextHash;
        }


        // Fisher-Yates random shuffle for [from, to) interval using 256-bit hash on input byte array
        // returns updated byte array
        // price: this may require several SHA-256 in a single round
        public static sbyte[] ShuffleBytes(this sbyte[] array, byte[] hash)
        {
            Runtime.Notify("initial hash");
            Runtime.Notify(hash);
            int i;
            byte[] nextHash = hash;
            int len = array.Length;
            sbyte[] shash = hash.AsSbyteArray();
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
                    shash = hash.AsSbyteArray();  // type convertion byte[] to sbyte[] (all information is kept original)
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
            return array; // copy based array
        }
    }

    public class RandomShuffle : Framework.SmartContract
    {
        public static byte[] Main(byte[] b)
        {
            sbyte[] sb = b.AsSbyteArray();

            Runtime.Notify(sb);
            Runtime.Notify(sb.Length);

/*
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
*/
            sb = sb.ShuffleBytes(b.SHA256());

            return sb.AsByteArray();

        }
    }
}
