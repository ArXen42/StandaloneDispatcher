using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using StandaloneDispatcher.Exceptions;
using StandaloneDispatcher.Interfaces;

namespace StandaloneDispatcher
{
	/// <summary>
	///    Default implementation of <see cref="IDispatcher"/>.
	/// </summary>
	public class Dispatcher : IDispatcher
	{
		private static readonly Int32 PrioritiesCount;

		private readonly ConcurrentQueue<DispatcherItem>[] _queues;
		private readonly SemaphoreSlim                     _newWorkSemaphore;
		private readonly TaskCompletionSource<Object>      _shutdownTaskCompletionSource;

		static Dispatcher()
		{
			PrioritiesCount = typeof(DispatcherPriority).GetEnumValues().Length;
		}

		public Dispatcher()
		{
			_newWorkSemaphore             = new SemaphoreSlim(0);
			_queues                       = new ConcurrentQueue<DispatcherItem>[PrioritiesCount];
			_shutdownTaskCompletionSource = new TaskCompletionSource<Object>();

			for (Int32 i = 0; i < PrioritiesCount; i++)
				_queues[i] = new ConcurrentQueue<DispatcherItem>();

			State = DispatcherState.NotRun;
		}

		public DispatcherState State { get; private set; }

		public void Run()
		{
			State = DispatcherState.Running;

			while (true)
			{
				// Wait for new work or dispatcher shutting down
				_newWorkSemaphore.Wait();

				if (State == DispatcherState.ShuttingDown)
					break;

				// Pick highest priority work
				DispatcherItem dispatcherItem = null;
				for (Int32 i = _queues.Length - 1; i >= 0; i--)
				{
					_queues[i].TryDequeue(out dispatcherItem);
					if (dispatcherItem != null)
						break;
				}

				// This, actually, should never happen
				if (dispatcherItem == null)
					throw new DispatcherException("Work is null");

				// Execute user action
				try
				{
					if (dispatcherItem.DelegateWithoutResult != null)
					{
						dispatcherItem.DelegateWithoutResult.Invoke();
						dispatcherItem.SignalFinished();
					}
					else
					{
						var result = dispatcherItem.DelegateWithResult.Invoke();
						dispatcherItem.SignalFinished(result);
					}
				}
				catch (Exception ex)
				{
					dispatcherItem.SignalFinishedWithException(ex);
				}
			}

			_shutdownTaskCompletionSource.SetResult(null);
		}

		public Task InvokeShutdownAsync()
		{
			State = DispatcherState.ShuttingDown;
			_newWorkSemaphore.Release();

			return _shutdownTaskCompletionSource.Task;
		}

		public async Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken ct = default)
		{
			if (State != DispatcherState.Running)
				throw new DispatcherException($"Dispatcher is in {State} state, Invoke operation cannot be executed.");

			var workItem = new DispatcherItem(action);
			_queues[(Int32) priority].Enqueue(workItem);
			_newWorkSemaphore.Release();
			var result = await workItem.WaitAsync(ct);

			if (result is Exception ex)
				throw ex;
		}

		public async Task<TResult> InvokeAsync<TResult>(Func<TResult> func, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken ct = default) where TResult : class
		{
			if (State != DispatcherState.Running)
				throw new DispatcherException($"Dispatcher is in {State} state, Invoke operation cannot be executed.");

			var workItem = new DispatcherItem(func);
			_queues[(Int32) priority].Enqueue(workItem);
			_newWorkSemaphore.Release();

			var result = await workItem.WaitAsync(ct);

			if (result is Exception ex)
				throw ex;

			return (TResult) result;
		}

		public void Dispose()
		{
			InvokeShutdownAsync().GetAwaiter().GetResult();
		}
	}
}