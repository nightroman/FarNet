
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Data;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar;

/// <summary>
/// Base panel for property list, member list and etc.
/// </summary>
public abstract class ListPanel : AnyPanel
{
	static string? _lastCurrentName;

	internal ListPanel(Explorer explorer) : base(explorer)
	{
		PostName(_lastCurrentName);

		// 090411 Use custom Descriptions mode
		var plan = new PanelPlan
		{
			Columns = [
				new SetColumn { Kind = "N", Name = "Name" },
				new SetColumn { Kind = "Z", Name = "Value" },
			]
		};
		SetPlan(PanelViewMode.AlternativeFull, plan);

		InvokingCommand += OnInvokingCommand;
	}

	/// <summary>
	/// The target object.
	/// </summary>
	internal abstract PSObject Target { get; }

	/// <summary>
	/// Puts a value into the command line or opens a lookup panel or member panel.
	/// </summary>
	/// <param name="file">The file to process.</param>
	public override void OpenFile(FarFile file)
	{
		ArgumentNullException.ThrowIfNull(file);

		// e.g. visible mode: sender is MemberDefinition
		if (file.Data is not PSPropertyInfo pi)
			return;

		// lookup opener?
		if (_LookupOpeners is not null)
		{
			if (_LookupOpeners.TryGetValue(file.Name, out ScriptHandler<OpenFileEventArgs>? handler))
			{
				handler.Invoke(this, new OpenFileEventArgs(file));
				return;
			}
		}

		// case: can show value in the command line
		if (Converter.InfoToLine(pi) is { } text)
		{
			// set command line
			ILine cl = Far.Api.CommandLine;
			cl.Text = "=" + text;
			cl.SelectText(1, text.Length + 1);
			return;
		}

		// case: enumerable
		if (Cast<IEnumerable>.From(pi.Value) is { } ie)
		{
			ObjectPanel panel = new();
			panel.AddObjects(ie);
			panel.OpenChild(this);
			return;
		}

		// case: data table
		if (Cast<DataTable>.From(pi.Value) is { } dt)
		{
			DataPanel.OpenChild(dt, this);
			return;
		}

		// open members
		OpenFileMembers(file);
	}

	internal override MemberPanel? OpenFileMembers(FarFile file)
	{
		if (file.Data is not PSPropertyInfo pi)
			return null;

		if (pi.Value is null)
			return null;

		var r = new MemberPanel(new MemberExplorer(pi.Value));
		r.OpenChild(this);

		return r;
	}

	/// <summary>
	/// Sets new value.
	/// </summary>
	/// <param name="info">Property info.</param>
	/// <param name="value">New value.</param>
	internal abstract void SetUserValue(PSPropertyInfo info, string? value);

	/// <summary>
	/// Calls base or assigns a value to the current property.
	/// </summary>
	void OnInvokingCommand(object? sender, CommandLineEventArgs e)
	{
		// base
		string code = e.Command.TrimStart();
		if (!code.StartsWith('='))
			return;

		// we do
		e.Ignore = true;

		// skip empty
		var file = CurrentFile;
		if (file is null)
			return;

		if (file.Data is not PSPropertyInfo pi)
			return;

		try
		{
			SetUserValue(pi, code[1..]);
			UpdateRedraw(true);
		}
		catch (RuntimeException ex)
		{
			A.Message(ex.Message);
		}
	}

	/// <inheritdoc/>
	protected override bool CanClose()
	{
		if (Child != null)
			return true;

		var file = CurrentFile;
		if (file is null)
			_lastCurrentName = null;
		else
			_lastCurrentName = file.Name;

		return true;
	}

	internal override void ShowHelpForPanel()
	{
		Entry.Instance.ShowHelpTopic(HelpTopic.ListPanel);
	}

	internal override void UIApply()
	{
		A.InvokePipelineForEach([Target]);
	}

	internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
	{
		items.ApplyCommand ??= new SetItem()
		{
			Text = Res.UIApply,
			Click = delegate { UIApply(); }
		};

		base.HelpMenuInitItems(items, e);
	}

	/// <summary>
	/// It deletes property values = assigns nulls.
	/// </summary>
	internal void UISetNulls()
	{
		foreach (FarFile file in GetSelectedFiles())
		{
			if (file.Data is not PSPropertyInfo pi)
				continue;

			try
			{
				//_110326_150007 Setting null fails for DataRow with value types, use DBNull
				if (Target.BaseObject is DataRow)
					pi.Value = DBNull.Value;
				else
					SetUserValue(pi, null);

				UpdateRedraw(true);
			}
			catch (RuntimeException ex)
			{
				A.Message(ex.Message);
			}
		}
	}

	/// <inheritdoc/>
	public override bool UIKeyPressed(KeyInfo key)
	{
		ArgumentNullException.ThrowIfNull(key);

		switch (key.VirtualKeyCode)
		{
			case KeyCode.Delete:
			case KeyCode.F8:

				if (key.IsShift())
				{
					UISetNulls();
					return true;
				}

				break;
		}

		// base
		return base.UIKeyPressed(key);
	}
}
