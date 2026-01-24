namespace FarNet;

/// <summary>
/// Standard Far menu.
/// It is created by <see cref="IFar.CreateMenu"/>.
/// </summary>
public interface IMenu : IAnyMenu, IDisposable
{
	/// <summary>
	/// Tells to assign hotkeys automatically from bottom.
	/// </summary>
	bool ReverseAutoAssign { get; set; }

	/// <summary>
	/// Tells to set the console title to the menu title.
	/// </summary>
	bool ChangeConsoleTitle { get; set; }

	/// <summary>
	/// Tells to show the menu with no box. Options <see cref="NoMargin"/> and <see cref="SingleBox"/> are not used.
	/// </summary>
	bool NoBox { get; set; }

	/// <summary>
	/// Tells to show the menu with no margin.
	/// </summary>
	bool NoMargin { get; set; }

	/// <summary>
	/// Tells to show the menu with single box.
	/// </summary>
	bool SingleBox { get; set; }

	/// <summary>
	/// Creates low level internal data of the menu from the current items.
	/// Normally you have to call <see cref="Unlock"/> after use.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Used for better performance for multiple <see cref="IAnyMenu.Show"/> with same items.
	/// </para>
	/// <para>
	/// After locking do not add or remove items before <see cref="Unlock"/>.
	/// You can change item properties except <see cref="FarItem.Text"/>.
	/// </para>
	/// </remarks>
	void Lock();

	/// <summary>
	/// Releases internal data created by <see cref="Lock"/>.
	/// If needed, the menu may be changed and shown again.
	/// </summary>
	void Unlock();
}
