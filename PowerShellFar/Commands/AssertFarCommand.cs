
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;
using System.Management.Automation;
using System.Reflection;
using System.Text;

namespace PowerShellFar.Commands;

[Cmdlet("Assert", "Far", DefaultParameterSetName = PSConditions)]
sealed class AssertFarCommand : BaseCmdlet
{
	internal const string MyName = "Assert-Far";
	const string
		PSEq = "Eq",
		PSConditions = "Conditions",
		PSParameters = "Parameters";

	[Parameter(ParameterSetName = PSEq, Position = 0)]
	[Parameter(ParameterSetName = PSConditions, Position = 0)]
	public object? Value { get; set; }

	[Parameter(ParameterSetName = PSEq)]
	public object? Eq { get; set; }

	[Parameter]
	public object? Message { get; set; }

	[Parameter]
	public string? Title { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public string? FileDescription { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public string? FileName { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public string? FileOwner { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Dialog { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Editor { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Panels { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Viewer { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Plugin { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Plugin2 { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Native { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Native2 { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public Guid DialogTypeId { set => _DialogTypeId = value; }
	Guid? _DialogTypeId;

	[Parameter(ParameterSetName = PSParameters)]
	public Guid ExplorerTypeId { set => _ExplorerTypeId = value; }
	Guid? _ExplorerTypeId;

	[Parameter(ParameterSetName = PSParameters)]
	public Guid ExplorerTypeId2 { set => _ExplorerTypeId2 = value; }
	Guid? _ExplorerTypeId2;

	[Parameter(ParameterSetName = PSParameters)]
	public string EditorFileName { set { _isEditorFileName = true; _EditorFileName = value; } }
	string? _EditorFileName;
	bool _isEditorFileName;

	[Parameter(ParameterSetName = PSParameters)]
	public string EditorTitle { set { _isEditorTitle = true; _EditorTitle = value; } }
	string? _EditorTitle;
	bool _isEditorTitle;

	bool IsError => Title is null;

	protected override void BeginProcessing()
	{
		// case: Eq
		if (ParameterSetName == PSEq)
		{
			if (!PSEquals(Value, Eq))
				AssertDialog(Message ?? EqualsFailMessage(Value, Eq));
			return;
		}

		// case: Conditions
		if (ParameterSetName == PSConditions)
		{
			var many = LanguagePrimitives.GetEnumerable(Value);
			if (many is null)
			{
				if (LanguagePrimitives.IsTrue(Value))
				{
					var it = Value is PSObject ps ? ps.BaseObject : Value;
					if (it is ScriptBlock)
						throw new PSArgumentException("Script block is not allowed as a single condition.");
				}
				else
				{
					AssertDialog();
				}
			}
			else
			{
				AssertEnumerable(many);
			}
			return;
		}

		// check dialog
		if (Dialog || _DialogTypeId.HasValue)
		{
			if (Far.Api.Window.Kind != WindowKind.Dialog)
				AssertDialog(Message ?? "The current window is not dialog.");

			if (_DialogTypeId.HasValue && Far.Api.Dialog.TypeId != _DialogTypeId)
				AssertDialog(Message ?? $"Unexpected dialog type ID {Far.Api.Dialog.TypeId}");
		}

		// check editor
		if (Editor || _isEditorFileName || _isEditorTitle)
		{
			if (Far.Api.Window.Kind != WindowKind.Editor)
				AssertDialog(Message ?? "The current window is not editor.");

			if (_isEditorFileName &&
				!new WildcardPattern(_EditorFileName, WildcardOptions.IgnoreCase).IsMatch(Far.Api.Editor!.FileName))
				AssertDialog(Message ?? $"The editor file name is not like '{_EditorFileName}'.");

			if (_isEditorTitle &&
				!new WildcardPattern(_EditorTitle, WildcardOptions.IgnoreCase).IsMatch(Far.Api.Editor!.Title))
				AssertDialog(Message ?? $"The editor file name is not like '{_EditorTitle}'.");
		}

		// check panels
		if (Panels && Far.Api.Window.Kind != WindowKind.Panels)
			AssertDialog(Message ?? "The current window is not panels.");

		// check viewer
		if (Viewer && Far.Api.Window.Kind != WindowKind.Viewer)
			AssertDialog(Message ?? "The current window is not viewer.");

		// check plugin
		if (Plugin || _ExplorerTypeId.HasValue)
		{
			if (!Far.Api.Panel.IsPlugin)
				AssertDialog(Message ?? "The active panel is not plugin.");

			if (_ExplorerTypeId.HasValue)
			{
				if (Far.Api.Panel is not Panel panel)
					AssertDialog(Message ?? "Active panel is not module panel.");
				else if (panel.Explorer.TypeId != _ExplorerTypeId)
					AssertDialog(Message ?? $"Unexpected active panel explorer type ID {panel.Explorer.TypeId}");
			}
		}

		// check plugin 2
		if (Plugin2 || _ExplorerTypeId2.HasValue)
		{
			if (!Far.Api.Panel2.IsPlugin)
				AssertDialog(Message ?? "The passive panel is not plugin.");

			if (_ExplorerTypeId2.HasValue)
			{
				if (Far.Api.Panel2 is not Panel panel)
					AssertDialog(Message ?? "Passive panel is not module panel.");
				else if (panel.Explorer.TypeId != _ExplorerTypeId2)
					AssertDialog(Message ?? "Unexpected passive panel explorer type ID {panel.Explorer.TypeId}");
			}
		}

		// check native
		if (Native && Far.Api.Panel.IsPlugin)
			AssertDialog(Message ?? "The active panel is not native.");

		// check native 2
		if (Native2 && Far.Api.Panel2.IsPlugin)
			AssertDialog(Message ?? "The passive panel is not native.");

		// check file data
		if (FileDescription != null || FileName != null || FileOwner != null)
		{
			var file = Far.Api.Panel.CurrentFile;
			if (file is null)
			{
				AssertDialog(Message ?? "There is not a current file.");

				// user tells to ignore so skip other file checks
				return;
			}

			//1 case sensitive!
			if (FileName != null && FileName != file.Name)
				AssertDialog(Message ?? $"The current file name is not '{FileName}'.");

			//2 case sensitive!
			if (FileDescription != null && FileDescription != file.Description)
				AssertDialog(Message ?? $"The current file description is not '{FileDescription}'.");

			//3 case sensitive!
			if (FileOwner != null && FileOwner != file.Owner)
				AssertDialog(Message ?? $"The current file owner is not '{FileOwner}'.");
		}
	}

	static bool PSEquals(object? a, object? b)
	{
		a = PS2.BaseObject(a, out _);
		b = PS2.BaseObject(b, out _);
		return Equals(a, b);
	}

	static string EqualsFailMessage(object? a, object? b)
	{
		var sb = new StringBuilder();
		sb.AppendLine("Objects are not equal:");

		sb.Append("A:");
		if (a != null)
			sb.Append($" {a} [{a.GetType()}]");
		sb.AppendLine();

		sb.Append("B:");
		if (b != null)
			sb.Append($" {b} [{b.GetType()}]");
		sb.AppendLine();

		return sb.ToString();
	}

	void AssertEnumerable(IEnumerable set)
	{
		int i = -1;
		foreach (var it in set)
		{
			++i;
			if (!LanguagePrimitives.IsTrue(it))
				AssertDialog(null, i);
		}
		if (i < 0)
			AssertDialog("Assertion set is empty.");
	}

	void AssertDialog(object? message = null, int conditionIndex = 0, bool doNotIgnore = false)
	{
		// break a macro
		MacroState macroState = Far.Api.MacroState;
		if (macroState == MacroState.Executing || macroState == MacroState.ExecutingCommon)
			Far.Api.UI.Break();

		// get the message
		if (message is null)
		{
			var messageScript = Cast<ScriptBlock>.From(Message);
			if (messageScript != null)
			{
				try
				{
					Message = messageScript.InvokeReturnAsIs();
				}
				catch (RuntimeException ex)
				{
					Message = "Error in the message script: " + ex.Message;
					Title = null;
				}
			}
		}
		else
		{
			Message = message;
		}

		// message body
		var body = Message is null ? "Assertion failed." : Message.ToString();
		if (IsError)
		{
			var sb = new StringBuilder(body);
			if (conditionIndex > 0)
			{
				sb.AppendLine();
				sb.Append("Condition #");
				sb.Append(conditionIndex + 1);
			}
			if (MyInvocation.ScriptName != null)
			{
				sb.AppendLine();
				sb.Append(MyInvocation.ScriptName);
				sb.Append('(');
				sb.Append(MyInvocation.ScriptLineNumber);
				sb.Append(',');
				sb.Append(MyInvocation.OffsetInLine);
				sb.AppendLine("):");
				sb.Append(MyInvocation.Line.Trim());
			}
			body = sb.ToString();
		}

		// buttons
		string[] buttons;
		if (!IsError)
		{
			buttons = new[] { BtnStop, BtnThrow };
		}
		else if (string.IsNullOrEmpty(MyInvocation.ScriptName))
		{
			if (doNotIgnore)
				buttons = new[] { BtnStop, BtnThrow, BtnDebug };
			else
				buttons = new[] { BtnStop, BtnThrow, BtnIgnore, BtnDebug };
		}
		else
		{
			if (doNotIgnore)
				buttons = new[] { BtnStop, BtnThrow, BtnDebug, BtnEdit };
			else
				buttons = new[] { BtnStop, BtnThrow, BtnIgnore, BtnDebug, BtnEdit };
		}

	repeat_dialog:

		int result = Far.Api.Message(new MessageArgs
		{
			TypeId = new Guid(Guids.AssertDialog),
			Text = body,
			Buttons = buttons,
			Caption = Title ?? MyName,
			Options = IsError ? (MessageOptions.Warning | MessageOptions.LeftAligned) : MessageOptions.None,
		});

		switch (result < 0 ? BtnStop : buttons[result])
		{
			case BtnStop:
				{
					throw new PipelineStoppedException();
				}
			case BtnThrow:
				{
					throw new PSInvalidOperationException(body);
				}
			case BtnIgnore:
				{
					return;
				}
			case BtnDebug:
				{
					// ask to attach a debugger
					bool isAddDebugger = false;
					var debugger = A.Psf.Runspace.Debugger;
					while (typeof(Debugger).GetField("DebuggerStop", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(debugger) is not Delegate)
					{
						var buttonsAttachDebugger = new[] { BtnOK, BtnAddDebugger, BtnCancel };
						var res = Far.Api.Message("Attach a debugger and continue.", "Debug", 0, buttonsAttachDebugger);
						if (res == 0)
							continue;

						if (res == 1)
						{
							try
							{
								AddDebuggerKit.ValidateAvailable();
								A.InvokeCode(@"Add-Debugger.ps1 $env:TEMP\Add-Debugger.log -Context 10");
								isAddDebugger = true;
								break;
							}
							catch (Exception ex)
							{
								Far.Api.ShowError("Add-Debugger", ex);
								continue;
							}
						}

						goto repeat_dialog;
					}

					// trigger debugger (do not Wait-Debugger, it shows with no source)
					try
					{
						// ensure variable and its breakpoint
						SessionState.PSVariable.Set(MyName, null);
						var bp = debugger.SetVariableBreakpoint(MyName, VariableAccessMode.Write, null, null);

						// set variable to stop debugger
						try
						{
							// this starts the debugger and blocks
							SessionState.PSVariable.Set(MyName, null);
						}
						catch (TerminateException) // Quit in debugger
						{
						}
						finally
						{
							// remove variable and breakpoint
							debugger.RemoveBreakpoint(bp);
							SessionState.PSVariable.Remove(MyName);
						}
					}
					finally
					{
						// If Add-Debugger has been added, restore.
						if (isAddDebugger)
							A.InvokeCode(@"Restore-Debugger");
					}

					// let user to decide how to continue
					goto repeat_dialog;
				}
			case BtnEdit:
				{
					IEditor editor = Far.Api.CreateEditor();
					editor.FileName = MyInvocation.ScriptName!;
					editor.GoTo(MyInvocation.OffsetInLine - 1, MyInvocation.ScriptLineNumber - 1);

					//! post opening or editor may be half rendered
					Far.Api.PostJob(editor.Open);

					throw new PipelineStoppedException();
				}
		}
	}
	const string
		BtnStop = "&Stop",
		BtnThrow = "&Throw",
		BtnIgnore = "&Ignore",
		BtnDebug = "&Debug",
		BtnEdit = "&Edit",
		//
		BtnOK = "OK",
		BtnAddDebugger = "Add-&Debugger",
		BtnCancel = "Cancel";
}
