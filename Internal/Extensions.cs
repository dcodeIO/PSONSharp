using System.IO;

namespace PSON.Internal
{
	internal static class Extensions
	{
		public static int WriteVarint(this Stream stream, uint value)
		{
			int size = 0;
			while (value >= 0x80)
			{
				stream.WriteByte((byte)((value & 0x7f) | 0x80));
				++size;
				value >>= 7;
			}
			stream.WriteByte((byte)value);
			return size;
		}

		public static int WriteVarint(this Stream stream, ulong value)
		{
			int size = 0;
			while (value >= 0x80)
			{
				stream.WriteByte((byte)((value & 0x7f) | 0x80));
				++size;
				value >>= 7;
			}
			stream.WriteByte((byte)value);
			return size;
		}

		public static uint ReadVarint32(this Stream stream)
		{
			uint value = 0;
			int count = 0;
			byte b;
			do
			{
				b = (byte)stream.ReadByte();
				if (count < 5)
					value |= (uint)((b & 0x7f) << (7 * count));
				++count;
			} while ((b & 0x80) != 0);
			return value;
		}

		public static ulong ReadVarint64(this Stream stream)
		{
			ulong value = 0;
			int count = 0;
			byte b;
			do
			{
				b = (byte)stream.ReadByte();
				if (count < 10)
					value |= (uint)((b & 0x7f) << (7 * count));
				++count;
			} while ((b & 0x80) == 0);
			return value;
		}

		public static uint ZigZagEncode(this int value) => (uint)(((value |= 0) << 1) ^ (value >> 31));

		public static ulong ZigZagEncode(this long value) => (ulong)(((value |= 0) << 1) ^ (value >> 63));

		public static int ZigZagDecode(this uint value) => (int)((value >> 1) ^ -(value & 1));

		public static long ZigZagDecode(this ulong value) => (long)((value >> 1) ^ (ulong)-(long)(value & 1));
	}
}
