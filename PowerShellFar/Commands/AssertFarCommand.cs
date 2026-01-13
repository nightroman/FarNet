using FarNet;
using PowerShellFar.UI;
using System.Collections;
using System.Management.Automation;
using System.Text;

namespace PowerShellFar.Commands;

[Cmdlet("Assert", "Far", DefaultParameterSetName = PSConditions)]
sealed class AssertFarCommand : BaseCmdlet
{
	internal const string MyName = "Assert-Far";
	const string
		PSEq = "Eq",
		PSConditions = "Conditions",
		PSParameters = "Parameters",
		AddDebuggerAction = "Add-Debugger-Action";

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
	public SwitchParameter NoError { get; set; }

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
	public SwitchParameter Menu { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Plugin { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Native { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public SwitchParameter Passive { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public Guid DialogTypeId { set => _DialogTypeId = value; }
	Guid? _DialogTypeId;

	[Parameter(ParameterSetName = PSParameters)]
	public Guid ExplorerTypeId { set => _ExplorerTypeId = value; }
	Guid? _ExplorerTypeId;

	[Parameter(ParameterSetName = PSParameters)]
	public string? EditorFileName { get; set; }

	[Parameter(ParameterSetName = PSParameters)]
	public string? EditorTitle { get; set; }

	IPanel? _panel_;
	IPanel Panel
	{
		get
		{
			if (_panel_ is null)
			{
				_panel_ = Passive ? Far.Api.Panel2 : Far.Api.Panel;
				if (_panel_ is null)
					throw new InvalidOperationException("Far has no panels.");
			}
			return _panel_;
		}
	}

	protected override void BeginProcessing()
	{
		// case: Eq
		if (ParameterSetName == PSEq)
		{
			var a = Value.BaseObject(out _);
			var b = Eq.BaseObject(out _);
			if (!Equals(a, b))
				AssertDialog(Message ?? EqualsFailMessage(a, b));
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

		// async? fail
		if (A.IsAsyncSession)
			throw new InvalidOperationException("Invalid Assert-Far parameters in async session.");

		// check errors
		if (NoError)
		{
			var errors = SessionState.PSVariable.GetValue("Global:Error") as IList;
			if (errors?.Count > 0)
				AssertDialog($"Expected no errors, found {errors.Count}.");
		}

		// check dialog
		if (Dialog || _DialogTypeId.HasValue)
		{
			if (Far.Api.Window.Kind != WindowKind.Dialog)
				AssertDialog(Message ?? "The current window is not dialog.");

			if (_DialogTypeId.HasValue && Far.Api.Dialog!.TypeId != _DialogTypeId)
				AssertDialog(Message ?? $"Unexpected dialog type ID {Far.Api.Dialog.TypeId}");
		}

		// check editor
		if (Editor || EditorFileName is { } || EditorTitle is { })
		{
			if (Far.Api.Window.Kind != WindowKind.Editor)
				AssertDialog(Message ?? "The current window is not editor.");

			if (EditorFileName is { } &&
				!new WildcardPattern(EditorFileName, WildcardOptions.IgnoreCase).IsMatch(Far.Api.Editor!.FileName))
				AssertDialog(Message ?? $"The editor file name is not like '{EditorFileName}'.");

			if (EditorTitle is { } &&
				!new WildcardPattern(EditorTitle, WildcardOptions.IgnoreCase).IsMatch(Far.Api.Editor!.Title))
				AssertDialog(Message ?? $"The editor file name is not like '{EditorTitle}'.");
		}

		// check panels
		if (Panels && Far.Api.Window.Kind != WindowKind.Panels)
			AssertDialog(Message ?? "The current window is not panels.");

		// check viewer
		if (Viewer && Far.Api.Window.Kind != WindowKind.Viewer)
			AssertDialog(Message ?? "The current window is not viewer.");

		// check menu
		if (Menu && Far.Api.Window.Kind != WindowKind.Menu)
			AssertDialog(Message ?? "The current window is not menu.");

		// check native
		if (Native && Panel.IsPlugin)
			AssertDialog(Message ?? "The panel is not native.");

		// check plugin
		if (Plugin || _ExplorerTypeId.HasValue)
		{
			if (!Panel.IsPlugin)
				AssertDialog(Message ?? "The panel is not plugin.");

			if (_ExplorerTypeId.HasValue)
			{
				if (Panel is not Panel panel)
					AssertDialog(Message ?? "The panel is not module panel.");
				else if (panel.Explorer.TypeId != _ExplorerTypeId)
					AssertDialog(Message ?? $"Unexpected panel explorer type ID {panel.Explorer.TypeId}");
			}
		}

		// check file data
		if (FileDescription != null || FileName != null || FileOwner != null)
		{
			var file = Panel.CurrentFile;
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
		if (A.IsAsyncSession)
		{
			Far.Api.PostJobAsync(() => AssertDialog(message, conditionIndex, doNotIgnore)).Await();
			return;
		}

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
		string body = Message is null ? "Assertion failed." : Message.ToString()!;

		//: user friendly message and stop
		if (Title is { })
		{
			Far.Api.Message(body, Title);
			throw new PipelineStoppedException();
		}

		// complete the message body
		{
			var sb = new StringBuilder(body);

			if (conditionIndex > 0)
			{
				sb.AppendLine();
				sb.Append("Condition #");
				sb.Append(conditionIndex + 1);
			}

			if (MyInvocation.ScriptName is { })
			{
				sb.AppendLine();
				sb.Append(MyInvocation.ScriptName);
				sb.Append('(');
				sb.Append(MyInvocation.ScriptLineNumber);
				sb.Append(',');
				sb.Append(MyInvocation.OffsetInLine);
				sb.AppendLine("):");
			}

			// amended statement
			{
				var statement = MyInvocation.Statement;
				int ix = statement.IndexOfAny(FarNet.Works.Kit.NewLineChars);
				if (ix < 0)
					ix = statement.Length;

				var line1 = statement.AsSpan(0, ix).TrimEnd();
				var line2 = MyInvocation.Line.AsSpan().TrimEnd();
				if (!line2.SequenceEqual(line1))
					statement = $"{line2}{statement.AsSpan(ix)}";

				sb.Append(statement);
			}

			body = sb.ToString();
		}

		// buttons
		string[] buttons;
		if (string.IsNullOrEmpty(MyInvocation.ScriptName))
		{
			if (doNotIgnore)
				buttons = [BtnStop, BtnThrow, BtnDebug];
			else
				buttons = [BtnStop, BtnThrow, BtnIgnore, BtnDebug];
		}
		else
		{
			if (doNotIgnore)
				buttons = [BtnStop, BtnThrow, BtnDebug, BtnEdit];
			else
				buttons = [BtnStop, BtnThrow, BtnIgnore, BtnDebug, BtnEdit];
		}

	repeat_dialog:

		int result = Far.Api.Message(new MessageArgs
		{
			TypeId = new Guid(Guids.AssertDialog),
			Text = body,
			Buttons = buttons,
			Caption = Title ?? MyName,
			Options = MessageOptions.Warning,
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
					while (!DebuggerKit.HasDebugger(A.Runspace))
					{
						var r = AttachDebuggerDialog.Show(A.Runspace);
						if (r == AttachDebuggerDialog.Continue)
							continue;

						if (r == AttachDebuggerDialog.AddDebugger)
						{
							try
							{
								DebuggerKit.ValidateAvailable();

								//! use own environment
								A.InvokeCode("Add-Debugger.ps1 -Env Assert-Far");

								//! force the debugger action to "Quit"
								Environment.SetEnvironmentVariable(AddDebuggerAction, "Quit");

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
					var debugger = A.Runspace.Debugger;
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
							// clean
							debugger.RemoveBreakpoint(bp);
							SessionState.PSVariable.Remove(MyName);
							Environment.SetEnvironmentVariable(AddDebuggerAction, null);
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
		BtnEdit = "&Edit";
}
