using System;
using System.Threading;

namespace StandaloneDispatcher.Sample
{
	internal static class Program
	{
		private static void Main(String[] args)
		{
			var dispatcher = new Dispatcher();
			var thread     = new Thread(dispatcher.Run);
			thread.Start();

			for (Int32 i = 0; i < 10000; i++)
			{
				Int32 copy = i;
				dispatcher.InvokeAsync(
					() =>
					{
						Thread.Sleep(100);
						Console.WriteLine(copy);
					}
				);
			}

			Console.WriteLine("Offloaded all work to dispatcher");
			Thread.Sleep(1000);
			dispatcher.InvokeShutdownAsync().Wait();
			Console.WriteLine("Shut down");
		}
	}
}