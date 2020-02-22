namespace StandaloneDispatcher.Interfaces
{
	/// <summary>
	///     An enumeration describing the priorities at which operations can be invoked via the <see cref="IDispatcher" />.
	/// </summary>
	public enum DispatcherPriority
	{
		Lowest      = 0,
		Low         = 1,
		BelowNormal = 2,
		Normal      = 3,
		AboveNormal = 4,
		High        = 5,
		Highest     = 6
	}
}