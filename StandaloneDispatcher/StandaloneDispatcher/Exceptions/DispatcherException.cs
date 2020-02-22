using System;

namespace StandaloneDispatcher.Exceptions
{
	public class DispatcherException : InvalidOperationException
	{
		public DispatcherException(String message)
			: base(message)
		{
		}
	}
}