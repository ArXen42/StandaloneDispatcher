using System;
using System.Threading;
using System.Threading.Tasks;

namespace StandaloneDispatcher.Interfaces
{
	/// <summary>
	///     Provides methods resembling commonly used WPF Dispatcher functionality.
	/// </summary>
	public interface IDispatcher : IDisposable
	{
		/// <summary>
		///     Current dispatcher state.
		/// </summary>
		DispatcherState State { get; }

		/// <summary>
		///     Run dispatcher loop on calling thread.
		/// </summary>
		void Run();

		/// <summary>
		///     Invoke dispatcher shutdown and await shutdown completion, causing it to enter <see cref="DispatcherState.ShuttingDown" />.
		/// </summary>
		Task InvokeShutdownAsync();

		Task          InvokeAsync(Action                 action, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken ct = default);
		Task<TResult> InvokeAsync<TResult>(Func<TResult> func,   DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken ct = default) where TResult : class;
	}
}