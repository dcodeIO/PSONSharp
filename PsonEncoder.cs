using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PSON
{
	/// <summary>
	/// A high-level PSON encoder that maintains a dictionary.
	/// </summary>
	public class PsonEncoder : PsonWriter
    {
		#region Public static methods

		public static byte[] Encode(object structure, IList<string> initialDictionary = null, PsonOptions options = PsonOptions.None)
		{
			var output = new MemoryStream();
			using (var encoder = new PsonEncoder(output, initialDictionary, options))
			{
				encoder.Write(structure);
				return output.ToArray();
			}
		}

		#endregion

		#region Non-public properties

		private PsonOptions options;

		private Dictionary<string, uint> dictionary;

		#endregion

		#region Public constructors

		public PsonEncoder(Stream output, IList<string> initialDictionary = null, PsonOptions options = PsonOptions.None) : base(output)
		{
			this.options = options;
			if (initialDictionary == null)
				dictionary = null;
			else
			{
				dictionary = new Dictionary<string, uint>(initialDictionary.Count);
				uint index = 0;
				foreach (var key in initialDictionary)
					dictionary[key] = index++;
			}
		}

		#endregion

		#region Public methods

		public void Write(object obj)
		{
			if (obj == null)
				WriteNull();

			else if (obj is string)
				writeString((string)obj, false);

			else if (obj is int)
				WriteInt((int)obj);

			else if (obj is uint)
				WriteInt((int)(uint)obj);

			else if (obj is long)
				WriteLong((long)obj);

			else if (obj is ulong)
				WriteLong((long)(ulong)obj);

			else if (obj is float)
				WriteFloat((float)obj);

			else if (obj is double)
				WriteDouble((double)obj);

			else if (obj is bool)
				WriteBool((bool)obj);

			else if (typeof(IList).IsAssignableFrom(obj.GetType()))
				WriteArray((IList)obj);

			else if (typeof(IDictionary).IsAssignableFrom(obj.GetType()))
				WriteObject((IDictionary)obj);

			else
				throw new ArgumentException("unsupported type: " + obj.GetType(), "obj");
		}

		public override void WriteString(string str) => writeString(str, false);

        public void WriteStringKey(string str) => writeString(str, true);

		public void WriteArray(IList list)
		{
			if (ReferenceEquals(list, null))
			{
				WriteNull();
				return;
			}
			var count = list.Count;
			WriteStartArray(count);
			for (var i = 0; i < count; ++i)
				Write(list[i]);
		}

		public void WriteArray(IList<object> list)
		{
			if (ReferenceEquals(list, null))
			{
				WriteNull();
				return;
			}
			var count = list.Count;
			WriteStartArray(count);
			for (var i = 0; i < count; ++i)
				Write(list[i]);
		}

		public void WriteObject(IDictionary obj)
		{
			if (ReferenceEquals(obj, null))
			{
				WriteNull();
				return;
			}
			WriteStartObject(obj.Count);
			foreach (DictionaryEntry entry in obj)
			{
				writeString((string)entry.Key, true);
				Write(entry.Value);
			}
		}

		public void WriteObject(IDictionary<string,object> obj)
		{
			if (obj == null)
			{
				WriteNull();
				return;
			}
			WriteStartObject(obj.Count);
			foreach (var entry in obj)
			{
				writeString(entry.Key, true);
				Write(entry.Value);
			}
		}

		public void WriteObject(IList<string> keys, IList<object> values)
		{
			if (ReferenceEquals(keys, null))
				throw new ArgumentNullException("keys");
			if (ReferenceEquals(values, null))
				throw new ArgumentNullException("values");
			var count = keys.Count;
			if (count != values.Count)
				throw new ArgumentException("element count mismatch");
			WriteStartObject(count);
			for (var i = 0; i < count; ++i)
			{
				writeString(keys[i], true);
				Write(values[i]);
			}
		}

		#endregion

		#region Non-public methods

		private void writeString(string str, bool isKey = false)
		{
			if (ReferenceEquals(str, null))
			{
				WriteNull();
				return;
			}
			if (str.Length == 0)
			{
				WriteEmptyString();
				return;
			}
			uint index;
			if (dictionary != null && dictionary.TryGetValue(str, out index))
			{
				WriteStringGet(index);
				return;
			}
			if (isKey)
			{
				if ((options & PsonOptions.ProgressiveKeys) > 0)
				{
					dictionary.Add(str, (uint)dictionary.Count);
					WriteStringAdd(str);
				}
				else
					base.WriteString(str);
			}
			else
			{
				if ((options & PsonOptions.ProgressiveValues) > 0)
				{
					dictionary.Add(str, (uint)dictionary.Count);
					WriteStringAdd(str);
				}
				else
					base.WriteString(str);
			}
		}

		#endregion
	}
}
