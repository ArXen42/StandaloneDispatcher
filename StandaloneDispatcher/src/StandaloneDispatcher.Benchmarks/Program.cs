using System;
using BenchmarkDotNet.Running;

namespace StandaloneDispatcher.Benchmarks
{
	internal static class Program
	{
		private static void Main(String[] args)
		{
			BenchmarkRunner.Run<DispatcherBenchmarks>();
		}
	}
}