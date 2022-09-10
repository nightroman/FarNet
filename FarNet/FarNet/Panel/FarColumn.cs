
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.ObjectModel;

namespace FarNet;

/// <summary>
/// Panel column options (abstract).
/// </summary>
/// <remarks>
/// Column options are used by <see cref="PanelPlan.Columns"/> and <see cref="PanelPlan.StatusColumns"/>.
/// <para>
/// This class is only a base for <see cref="SetColumn"/> (recommended and ready to use)
/// and other classes derived by modules (basically they are not needed).
/// </para>
/// </remarks>
public class FarColumn
{
	/// <summary>
	/// Column name.
	/// </summary>
	/// <remarks>
	/// Title of a standard panel column. It is ignored for a status column.
	/// </remarks>
	public virtual string? Name
	{
		get => null;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Column kind. See <see cref="PanelPlan.Columns"/>.
	/// </summary>
	public virtual string? Kind
	{
		get => null;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Column width.
	/// </summary>
	/// <remarks>
	/// Positive: absolute width; negative: percentage; 0: variable.
	/// </remarks>
	public virtual int Width
	{
		get => 0;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Default column kind sequence: "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
	/// </summary>
	public static ReadOnlyCollection<string> DefaultColumnKinds { get; } =
		new ReadOnlyCollection<string>(new string[] { "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9" });

	/// <summary>
	/// Only for derived classes.
	/// </summary>
	protected FarColumn()
	{
	}
}
