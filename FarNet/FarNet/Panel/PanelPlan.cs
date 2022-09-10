
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Panel view plan.
/// </summary>
/// <remarks>
/// Normally it is used for <see cref="Panel.SetPlan"/>.
/// When a panel is opened you can change modes dynamically, but do not forget
/// to reset the list itself, changes in items are not reflected without this.
/// <para>
/// WARNING: column titles, kinds and custom columns is a sort of low level stuff;
/// if you use this incorrectly the Far may crash.
/// </para>
/// </remarks>
/// <seealso cref="FarFile.Columns"/>
/// <seealso cref="SetFile.Columns"/>
public sealed class PanelPlan
{
	/// <summary>
	/// Columns info.
	/// </summary>
	/// <remarks>
	/// <para>
	/// All supported kinds: "N", "Z", "O", "S", "DC", "DM", "DA", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
	/// Supported Far column kind suffixes may be added to the end, e.g. NR, ST, DCB, and etc., see Far API [Column types].
	/// </para>
	/// <para>
	/// Default column kind sequence: "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
	/// It is exposed as <see cref="FarColumn.DefaultColumnKinds"/>.
	/// </para>
	/// <para>
	/// Column kind rules:
	/// <ul>
	/// <li>Specify column kinds only when you really have to do so, especially try to avoid C0..C9, let them to be processed by default.</li>
	/// <li>C0...C9 must be listed incrementally without gaps; but other kinds between them is OK. E.g. C0, C2 is bad; C0, N, C1 is OK.</li>
	/// <li>If a kind is not specified then the next available from the remaining default sequence is taken.</li>
	/// <li>Column kinds should not be specified more than once.</li>
	/// </ul>
	/// </para>
	/// </remarks>
	public FarColumn[]? Columns { get; set; }

	/// <summary>
	/// Status columns info.
	/// </summary>
	/// <remarks>
	/// Use it for status columns in the same way as <see cref="Columns"/> is used.
	/// Column names are ignored.
	/// </remarks>
	public FarColumn[]? StatusColumns { get; set; }

	/// <summary>
	/// Tells to resize panel to fill the entire window (instead of a half).
	/// </summary>
	public bool IsFullScreen { get; set; }

	/// <summary>
	/// Tells to display full status info for a file.
	/// </summary>
	/// <remarks>
	/// Tells to display full status info for a file if <c>Status*</c> are not defined.
	/// Otherwise, the status line displays the file name.
	/// </remarks>
	public bool IsDetailedStatus { get; set; }

	/// <summary>
	/// Tells to align file extensions.
	/// </summary>
	public bool IsAlignedExtensions { get; set; }

	/// <summary>
	/// Tells to use name case conversion.
	/// </summary>
	public bool IsCaseConversion { get; set; }

	/// <summary>
	/// Creates a new mode as a shallow copy of this.
	/// </summary>
	/// <remarks>
	/// Use it to create another mode with the same properties and then change a few of them.
	/// </remarks>
	public PanelPlan Clone()
	{
		return (PanelPlan)MemberwiseClone();
	}
}
