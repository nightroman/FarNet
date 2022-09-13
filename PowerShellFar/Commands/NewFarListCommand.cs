
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

//! Why _set*: we set preferences from settings first, then change them if _set*.
[OutputType(typeof(IListMenu))]
class NewFarListCommand : BaseMenuCmdlet
{
	[Parameter]
	public SwitchParameter AutoSelect { set => _AutoSelect = value; }
	SwitchParameter? _AutoSelect;

	[Parameter]
	public string? Incremental { get; set; }

	[Parameter]
	public PatternOptions IncrementalOptions { set => _IncrementalOptions = value; }
	protected PatternOptions? _IncrementalOptions;

	[Parameter]
	public int ScreenMargin { set => _ScreenMargin = value; }
	int? _ScreenMargin;

	[Parameter]
	public SwitchParameter UsualMargins { set => _UsualMargins = value; }
	SwitchParameter? _UsualMargins;

	[Parameter]
	public SwitchParameter Popup { get; set; }

	internal IListMenu Create()
	{
		IListMenu menu = Far.Api.CreateListMenu();
		Init(menu);

		if (Popup)
			Settings.Default.PopupMenu(menu);
		else
			Settings.Default.ListMenu(menu);

		if (_AutoSelect.HasValue)
			menu.AutoSelect = _AutoSelect.Value;

		if (Incremental != null)
			menu.Incremental = Incremental;

		if (_IncrementalOptions.HasValue)
			menu.IncrementalOptions = _IncrementalOptions.Value;

		if (_NoShadow.HasValue)
			menu.NoShadow = _NoShadow.HasValue;

		if (_ScreenMargin.HasValue)
			menu.ScreenMargin = _ScreenMargin.Value;

		if (_UsualMargins.HasValue)
			menu.UsualMargins = _UsualMargins.Value;

		return menu;
	}

	protected override void BeginProcessing()
	{
		WriteObject(Create());
	}
}
