using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project1
{
	internal class NaNException : Exception
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
