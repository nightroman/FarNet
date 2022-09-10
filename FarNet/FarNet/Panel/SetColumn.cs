
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Panel column options.
/// </summary>
/// <remarks>
/// Use this class directly to create column options instance and set its properties.
/// See <see cref="FarColumn"/> for details.
/// </remarks>
public sealed class SetColumn : FarColumn
{
	/// <inheritdoc/>
	public override string? Name { get; set; }

	/// <inheritdoc/>
	public override string? Kind { get; set; }

	/// <inheritdoc/>
	public override int Width { get; set; }
}
