using System;
using System.Threading;

namespace DispatcherThread.Sample
{
	internal static class Program
	{
		private static void Main(String[] args)
		{
			using var thread = new DispatcherThread();

			for (int i = 0; i < 100; i++)
			{
				Int32 i1 = i;
				thread.InvokeAsync(
					() =>
					{
						Thread.Sleep(100 - i1);
						Console.WriteLine(i1);
					}
				);
			}

			Thread.Sleep(200);
		}

		private static void Test()
		{
			using var slim = new Semaphore(1, 1);
		}
	}
}