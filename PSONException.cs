using System;

namespace PSON
{
	[Serializable]
	public class PsonException : Exception
	{
		public PsonException(string message) : base(message)
		{
		}

		public PsonException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
