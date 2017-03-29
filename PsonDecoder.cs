using PSON.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PSON
{
	/// <summary>
	/// A high-level PSON decoder that maintains a dictionary.
	/// </summary>
	public class PsonDecoder : IDisposable
	{
		public static object Decode(byte[] buffer, IList<string> initialDictionary = null, PsonOptions options = PsonOptions.None, int allocationLimit = -1)
		{
			var input = new MemoryStream(buffer);
			using (var decoder = new PsonDecoder(input, initialDictionary, options, allocationLimit))
				return decoder.Read();
		}

		private Stream input;

		private List<string> dictionary;

		private PsonOptions options;

		private int allocationLimit;

		private readonly byte[] convertArray = new byte[8];

		public PsonDecoder(Stream input, IList<string> initialDictionary = null, PsonOptions options = PsonOptions.None, int allocationLimit = -1)
		{
			if (ReferenceEquals(input, null))
				throw new ArgumentNullException("input");
			this.input = input;
			this.options = options;
			this.allocationLimit = allocationLimit;
			if (initialDictionary == null)
				dictionary = null;
			else
                dictionary = new List<string>(initialDictionary);
		}

		public object Read()
		{
			checkDisposed();
			return decodeValue();
		}

		private object decodeValue()
		{ 
			var token = (byte)input.ReadByte();
			if (token <= Token.MAX)
				return token;
			switch (token)
			{
				case Token.NULL:
					return null;

				case Token.TRUE:
					return true;

				case Token.FALSE:
					return false;

				case Token.EOBJECT:
					return new Dictionary<string, object>();

				case Token.EARRAY:
					return new List<object>();

				case Token.ESTRING:
					return string.Empty;

				case Token.OBJECT:
					return decodeObject();

				case Token.ARRAY:
					return decodeArray();

				case Token.INTEGER:
					return input.ReadVarint32().ZigZagDecode();

				case Token.LONG:
					return input.ReadVarint64().ZigZagDecode();

				case Token.FLOAT:
					if (input.Read(convertArray, 0, 4) != 4)
						throw new PsonException("stream ended prematurely");
					if (!BitConverter.IsLittleEndian)
						Array.Reverse(convertArray, 0, 4);
					return BitConverter.ToSingle(convertArray, 0);

				case Token.DOUBLE:
					if (input.Read(convertArray, 0, 8) != 8)
						throw new PsonException("stream ended prematurely");
					if (!BitConverter.IsLittleEndian)
						Array.Reverse(convertArray, 0, 8);
					return BitConverter.ToDouble(convertArray, 0);

				case Token.STRING_ADD:
				case Token.STRING:
					return decodeString(token, false);

				case Token.STRING_GET:
					return getString(input.ReadVarint32());

				case Token.BINARY:
					return decodeBinary();

				default:
					throw new PsonException("illegal token: 0x" + token.ToString("x2")); // should never happen
			}
		}

		private IList<object> decodeArray()
		{
			var count = input.ReadVarint32();
			if (allocationLimit > -1 && count > allocationLimit)
				throw new PsonException("allocation limit exceeded:" + count);
			var list = new List<object>(checked((int)count));
			while (count-- > 0)
				list.Add(decodeValue());
			return list;
		}

		private Dictionary<string,object> decodeObject()
		{
			var count = input.ReadVarint32();
			if (allocationLimit > -1 && count > allocationLimit)
				throw new PsonException("allocation limit exceeded: " + count);
			var obj = new Dictionary<string, object>(checked((int)count));
			while (count-- > 0)
			{
				var strToken = (byte)input.ReadByte();
				switch (strToken)
				{
					case Token.STRING_ADD:
					case Token.STRING:
						obj[decodeString(strToken, true)] = decodeValue();
						break;

					case Token.STRING_GET:
						obj[getString(input.ReadVarint32())] = decodeValue();
						break;

					default:
						throw new PsonException("string token expected");
				}
			}
			return obj;
		}

		private string decodeString(byte token, bool isKey)
		{
            var count = checked((int)input.ReadVarint32());
			if (allocationLimit > -1 && count > allocationLimit)
				throw new PsonException("allocation limit exceeded: " + count);
			var buffer = new byte[count];
			if (input.Read(buffer, 0, count) != count)
				throw new PsonException("stream ended prematurely");
			var value = Encoding.UTF8.GetString(buffer);
			if (token == Token.STRING_ADD)
			{
				if (isKey)
				{
					if ((options & PsonOptions.ProgressiveKeys) == 0)
						throw new PsonException("illegal progressive key");
				}
				else
				{
					if ((options & PsonOptions.ProgressiveValues) == 0)
						throw new PsonException("illegal progressive value");
				}
				dictionary.Add(value);
			}
			return value;
		}

		private string getString(uint index)
		{
            if (index >= dictionary.Count)
				throw new PsonException("dictionary index out of bounds: " + index);
            return dictionary[checked((int)index)];
		}

		private byte[] decodeBinary()
		{
			var count = (int)input.ReadVarint32();
			if (allocationLimit > -1 && count > allocationLimit)
				throw new PsonException("allocation limit exceeded: " + count);
			var bytes = new byte[count];
			if (input.Read(bytes, 0, count) != count)
				throw new PsonException("stream ended prematurely");
			return bytes;
		}

		#region IDisposable Support

		private bool disposed = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					input.Dispose();
					input = null;
					dictionary = null;
				}
				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void checkDisposed()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		#endregion
	}
}
