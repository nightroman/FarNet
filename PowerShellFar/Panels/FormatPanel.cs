
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using FarNet;

namespace PowerShellFar;

/// <summary>
/// Formatted table panel.
/// </summary>
/// <param name="explorer">The panel explorer.</param>
public abstract class FormatPanel(FormatExplorer explorer) : TablePanel(explorer)
{
	/// <summary>
	/// Gets the panel explorer.
	/// </summary>
	public new FormatExplorer Explorer => (FormatExplorer)base.Explorer;

	/// <summary>
	/// Gets a list of ready files or a collection of PS objects.
	/// </summary>
	internal override string HelpMenuTextOpenFileMembers => "Object members";

	internal void BuildPlan(string? sameType)
	{
		var plan = GetPlan(PanelViewMode.AlternativeFull) ?? new PanelPlan();

		// choose columns
		if (sameType == null)
		{
			//! The long "Index" clashes to sort order mark, use the short "##"
			plan.Columns = [
				new SetColumn() { Kind = "S", Name = "##" },
				new SetColumn() { Kind = "N", Name = "Value" },
				new SetColumn() { Kind = "Z", Name = "Type" },
			];
		}
		else
		{
			plan.Columns = [
				new SetColumn() { Kind = "N", Name = sameType },
			];
		}

		SetPlan(PanelViewMode.AlternativeFull, plan);
	}

	/// <inheritdoc/>
	public override void Open()
	{
		if (IsOpened)
			return;

		if (Explorer.Metas != null)
			SetPlan(PanelViewMode.AlternativeFull, Format.SetupPanelMode(Explorer.Metas));

		base.Open();
	}

	/// <inheritdoc/>
	public override IEnumerable<FarFile> UIGetFiles(GetFilesEventArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);

		args.Parameter = this;

		return base.UIGetFiles(args);
	}
}
