using System;

namespace PSON
{
	[Flags]
	public enum PsonOptions
	{
		/// <summary>
		/// Static dictionary only.
		/// </summary>
		None = 0,

		/// <summary>
		/// Add keys to the dictionary progressively.
		/// </summary>
		ProgressiveKeys = 1,

		/// <summary>
		/// Add values to the dictionary progressively.
		/// </summary>
		ProgressiveValues = 2,

		/// <summary>
		/// Add both keys and values to the dictionary progressively.
		/// </summary>
		Progressive = 3
	}
}
