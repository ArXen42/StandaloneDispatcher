namespace StandaloneDispatcher.Interfaces
{
	/// <summary>
	///     Dispatcher execution state.
	/// </summary>
	public enum DispatcherState
	{
		/// <summary>
		///     Dispatcher execution loop not yet started. Pushing items is permitted in this state.
		/// </summary>
		NotRun,

		/// <summary>
		///     Dispatcher is executing user actions. Pushing items is permitted in this state.
		/// </summary>
		Running,

		/// <summary>
		///     Dispatcher is finishing active user action and shutting down. Pushing items is forbidden in this state.
		/// </summary>
		ShuttingDown,

		/// <summary>
		///     Dispatcher was shut down. Pushing items is forbidden in this state.
		/// </summary>
		Finished
	}
}