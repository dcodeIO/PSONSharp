using System;
using System.IO;
using PSON.Internal;
using System.Text;

namespace PSON
{
	/// <summary>
	/// A low-level PSON writer.
	/// </summary>
	public class PsonWriter : IDisposable
	{
		protected Stream output;

		public PsonWriter(Stream output)
		{
			if (ReferenceEquals(output, null))
				throw new ArgumentNullException("output");
			this.output = output;
		}

		public void WriteNull()
		{
			output.WriteByte(Token.NULL);
		}

		public void WriteInt(int num)
		{
			checkDisposed();
			var value = num.ZigZagEncode();
			if (value <= Token.MAX)
			{
				output.WriteByte((byte)value);
				return;
			}
			output.WriteByte(Token.INTEGER);
			output.WriteVarint(value);
		}

		public void WriteLong(long num)
		{
			checkDisposed();
			var value = num.ZigZagEncode();
			if (value <= Token.MAX)
			{
				output.WriteByte((byte)value);
				return;
			}
			output.WriteByte(Token.INTEGER);
			output.WriteVarint(value);
		}

		public void WriteFloat(float flt)
		{
			checkDisposed();
			var bytes = BitConverter.GetBytes(flt);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(bytes);
			output.WriteByte(Token.FLOAT);
			output.Write(bytes, 0, 4);
		}

		public void WriteDouble(double dbl)
		{
			checkDisposed();
			var bytes = BitConverter.GetBytes(dbl);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(bytes);
			output.WriteByte(Token.DOUBLE);
			output.Write(bytes, 0, 8);
		}

		public void WriteBool(bool b)
		{
			checkDisposed();
			output.WriteByte(b ? Token.TRUE : Token.FALSE);
		}

		public void WriteEmptyString()
		{
			output.WriteByte(Token.ESTRING);
		}

		public virtual void WriteString(string str)
		{
			checkDisposed();
			if (ReferenceEquals(str, null))
				throw new ArgumentNullException("str");
			if (str.Length == 0)
			{
				output.WriteByte(Token.ESTRING);
				return;
			}
			output.WriteByte(Token.STRING);
			writeStringDelimited(str);
		}

		public void WriteStringAdd(string str)
		{
			checkDisposed();
			if (ReferenceEquals(str, null))
				throw new ArgumentNullException("str");
			if (str.Length == 0)
				throw new ArgumentException("str must not be empty");
			output.WriteByte(Token.STRING_ADD);
			writeStringDelimited(str);
		}

		protected virtual void writeStringDelimited(string str)
		{
			var bytes = Encoding.UTF8.GetBytes(str);
			output.WriteVarint((uint)bytes.Length);
			output.Write(bytes, 0, bytes.Length);
		}

		public void WriteStringGet(uint index)
		{
			checkDisposed();
			output.WriteByte(Token.STRING_GET);
			output.WriteVarint(index);
		}

		public void WriteEmptyArray()
		{
			checkDisposed();
			output.WriteByte(Token.EARRAY);
		}

		public void WriteStartArray(int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException("count");
			if (count == 0)
			{
				WriteEmptyArray();
				return;
			}
			checkDisposed();
			output.WriteByte(Token.ARRAY);
			output.WriteVarint((uint)count);
		}

		public void WriteEmptyObject()
		{
			checkDisposed();
			output.WriteByte(Token.EOBJECT);
		}

		public void WriteStartObject(int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException("count");
			if (count == 0)
			{
				WriteEmptyObject();
				return;
			}
			checkDisposed();
			output.WriteByte(Token.OBJECT);
			output.WriteVarint((uint)count);
		}

		#region IDisposable Support

		private bool disposed = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					output.Dispose();
				}
				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void checkDisposed()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		#endregion
	}
}
