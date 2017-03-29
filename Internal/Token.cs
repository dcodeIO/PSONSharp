namespace PSON.Internal
{
	internal static class Token
	{
		// Small integer values
		public const byte ZERO = 0x00;
		public const byte MAX  = 0xef;

		// Special objectss
		public const byte NULL = 0xf0;
		public const byte TRUE = 0xf1;
		public const byte FALSE = 0xf2;
		public const byte EOBJECT = 0xf3;
		public const byte EARRAY = 0xf4;
		public const byte ESTRING = 0xf5;

		// Objects
		public const byte OBJECT = 0xf6;

		// Arrays
		public const byte ARRAY = 0xf7;

		// Values
		public const byte INTEGER = 0xf8;
		public const byte LONG = 0xf9;
		public const byte FLOAT = 0xfa;
		public const byte DOUBLE = 0xfb;

		// Strings
		public const byte STRING = 0xfc;
		public const byte STRING_ADD = 0xfd;
		public const byte STRING_GET = 0xfe;

		// Binary
		public const byte BINARY = 0xff;
	}
}
