using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace StandaloneDispatcher.Benchmarks
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31, baseline: true)]
	[SimpleJob(RuntimeMoniker.Net472)]
	[SimpleJob(RuntimeMoniker.Net48)]
	public class DispatcherBenchmarks
	{
		private readonly Dispatcher _dispatcher;

		public DispatcherBenchmarks()
		{
			_dispatcher = new Dispatcher();
			new Thread(_dispatcher.Run).Start();
		}

		[Benchmark]
		public async Task ExecuteLotsOfSmallActions()
		{
			const Int32 actionsCount = 100000;

			var tasks = new List<Task>();
			for (Int32 i = 0; i < actionsCount; i++)
			{
				tasks.Add(_dispatcher.InvokeAsync(() => {}));
			}

			await Task.WhenAll(tasks);
		}


		[GlobalCleanup]
		public void GlobalCleanup()
		{
			_dispatcher.Dispose();
		}
	}
}