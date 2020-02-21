using System;
using System.Threading;
using System.Threading.Tasks;

namespace DispatcherThread
{
	internal class DispatcherItem
	{
		public readonly Func<Object> Method;

		private readonly SemaphoreSlim _semaphoreSlim;
		private          Object        _result;

		public DispatcherItem(Func<Object> method)
		{
			_semaphoreSlim = new SemaphoreSlim(0);
			Method         = method;
		}

		public void SignalFinished(Object result)
		{
			_result = result;
			_semaphoreSlim.Release();
		}

		public async Task<Object> WaitAsync()
		{
			await _semaphoreSlim.WaitAsync();
			return _result;
		}
	}
}