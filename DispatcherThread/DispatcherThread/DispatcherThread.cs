using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DispatcherThread
{
	public class DispatcherThread : IDisposable
	{
		private readonly BlockingCollection<DispatcherItem> _pendingActions;

		public DispatcherThread(ApartmentState apartmentState = ApartmentState.MTA)
		{
			_pendingActions = new BlockingCollection<DispatcherItem>();

			var thread = new Thread(ThreadWorker);
			if (apartmentState != ApartmentState.MTA)
				thread.SetApartmentState(apartmentState);
			thread.Start();
		}

		public async Task<TResult> InvokeAsync<TResult>(Func<TResult> method) where TResult : class
		{
			var item = new DispatcherItem(method);
			_pendingActions.Add(item);
			return (TResult) await item.WaitAsync();
		}

		public async Task InvokeAsync(Action method)
		{
			await InvokeAsync<Object>(
				() =>
				{
					method.Invoke();
					return null;
				}
			);
		}

		public void Dispose()
		{
			_pendingActions.Dispose();
		}

		private void ThreadWorker()
		{
			foreach (var action in _pendingActions.GetConsumingEnumerable())
			{
				var result = action.Method.Invoke();
				action.SignalFinished(result);
			}

			Console.WriteLine("Finished");
		}
	}
}