using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace AsteroidLibrary
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FloatUnion
    {
        [FieldOffset(0)]
        public float Value;
        [FieldOffset(0)]
        public byte Byte0;
        [FieldOffset(1)]
        public byte Byte1;
        [FieldOffset(2)]
        public byte Byte2;
        [FieldOffset(3)]
        public byte Byte3;

        public byte[] ToByteArray()
        {
            return new[] { Byte0, Byte1, Byte2, Byte3 };
        }

        public static byte[] FloatToBytes(float value)
        {
            return new FloatUnion { Value = value }.ToByteArray();
        }

        public static float BytesToFloat(byte[] bytes)
        {
            if (bytes.Length != 4) throw new ArgumentException("You must provide four bytes.");
            return new FloatUnion { Byte0 = bytes[0], Byte1 = bytes[1], Byte2 = bytes[2], Byte3 = bytes[3] }.Value;
        }

        public static float BytesToFloat(byte[] inputArray, int startIndex, int endIndex)
        {
            byte[] bytes = new byte[4];
            int x = 0;
            for(int i =startIndex; i<=endIndex; i++, x++)
            {
                bytes[x] = inputArray[i];
            }

            if (bytes.Length != 4) throw new ArgumentException("You must provide four bytes.");
            return new FloatUnion { Byte0 = bytes[0], Byte1 = bytes[1], Byte2 = bytes[2], Byte3 = bytes[3] }.Value;
        }
    }
}
