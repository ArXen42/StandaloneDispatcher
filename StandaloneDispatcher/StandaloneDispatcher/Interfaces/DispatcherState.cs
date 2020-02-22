namespace StandaloneDispatcher.Interfaces
{
	/// <summary>
	///     Dispatcher execution state.
	/// </summary>
	public enum DispatcherState
	{
		/// <summary>
		///     Dispatcher execution loop not yet started.
		/// </summary>
		NotRun,

		/// <summary>
		///     Dispatcher is executing user actions.
		/// </summary>
		Running,

		/// <summary>
		///     Dispatcher is finishing active user action and shutting down.
		/// </summary>
		ShuttingDown,

		/// <summary>
		///     Dispatcher was shut down.
		/// </summary>
		Finished
	}
}