using System;

namespace Project2.Particles
{
	public class NaNException : Exception
	{
		public NaNException()
		{
		}

		public NaNException(string message) : base(message)
		{
		}

		public NaNException(string message,
		                            Exception innerException) : base(message, innerException)
		{
		}

	}
}
