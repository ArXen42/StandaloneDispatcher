using System;
using System.Threading;
using System.Threading.Tasks;

namespace StandaloneDispatcher
{
	internal class DispatcherItem
	{
		public readonly Func<Object> DelegateWithResult;
		public readonly Action       DelegateWithoutResult;

		private readonly TaskCompletionSource<Object> _tcs;

		public DispatcherItem(Func<Object> delegateWithResult)
			: this(delegateWithResult, null)
		{
		}

		public DispatcherItem(Action delegateWithoutResult)
			: this(null, delegateWithoutResult)
		{
		}

		private DispatcherItem(Func<Object> delegateWithResult, Action delegateWithoutResult)
		{
			DelegateWithResult    = delegateWithResult;
			DelegateWithoutResult = delegateWithoutResult;

			_tcs = new TaskCompletionSource<Object>();
		}

		public void SignalFinishedWithException(Exception ex)
		{
			_tcs.SetResult(ex);
		}

		public void SignalFinished(Object result)
		{
			_tcs.SetResult(result);
		}

		public void SignalFinished()
		{
			_tcs.SetResult(null);
		}

		public Task<Object> WaitAsync(CancellationToken ct) => _tcs.Task;
	}
}