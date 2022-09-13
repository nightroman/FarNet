
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

/// <summary>
/// Common features of menu cmdlets.
/// </summary>
class BaseMenuCmdlet : BaseCmdlet
{
	[Parameter(Position = 0)]
	public string? Title { get; set; }

	[Parameter(Position = 1)]
	public FarItem[]? Items { get; set; }

	[Parameter]
	public SwitchParameter AutoAssignHotkeys { get; set; }

	[Parameter]
	public string? Bottom { get; set; }

	[Parameter]
	public string? HelpTopic { get; set; }

	[Parameter]
	public int Selected { set => _Selected = value; }
	int? _Selected;

	[Parameter]
	public SwitchParameter SelectLast { get; set; }

	[Parameter]
	public SwitchParameter ShowAmpersands { get; set; }

	[Parameter]
	public SwitchParameter WrapCursor { get; set; }

	[Parameter]
	public int X { set => _X = value; }
	int? _X;

	[Parameter]
	public int Y { set => _Y = value; }
	int? _Y;

	//! it is common for menus and lists but set separately, in lists after preferences if _set*
	[Parameter]
	public SwitchParameter NoShadow { set => _NoShadow = value; }
	protected SwitchParameter? _NoShadow;

	internal void Init(IAnyMenu menu)
	{
		menu.Title = Title;
		menu.Bottom = Bottom;
		menu.HelpTopic = HelpTopic;
		if (_Selected.HasValue)
			menu.Selected = _Selected.Value;
		if (_X.HasValue)
			menu.X = _X.Value;
		if (_Y.HasValue)
			menu.Y = _Y.Value;

		menu.AutoAssignHotkeys = AutoAssignHotkeys;
		menu.SelectLast = SelectLast;
		menu.ShowAmpersands = ShowAmpersands;
		menu.WrapCursor = WrapCursor;

		if (Items != null)
		{
			foreach (FarItem item in Items)
				menu.Items.Add(item);
		}
	}
}
