
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Assert-Far command.
	/// Checks for the condition(s) and stops the pipeline with a message if any condition is false or not Boolean.
	/// </summary>
	/// <remarks>
	/// If the assertion fails then a message is shown and the <c>PipelineStoppedException</c> exception is thrown after that.
	/// A running macro, if any, is stopped before showing the message.
	/// <para>
	/// If the message <see cref="Title"/> is provided then just a simple message is shown on failures,
	/// all the assertion details are omitted. This configuration is designed for production scripts.
	/// </para>
	/// <example>
	/// <code>
	/// # Hardcoded breakpoint:
	/// Assert-Far
	///
	/// # Single checks:
	/// Assert-Far -Panels
	/// Assert-Far -Plugin
	/// Assert-Far ($Far.Window.Kind -eq 'Panels')
	///
	/// # Combined checks:
	/// Assert-Far -Panels -Plugin
	/// Assert-Far -Panels ($Far.Panel.IsPlugin)
	/// Assert-Far @(
	///     $Far.Window.Kind -eq 'Panels'
	///     $Far.Panel.IsPlugin
	/// )
	///
	/// # User friendly error message. Mind use of -Message and -Title with switches:
	/// Assert-Far -Panels -Message "Run this script from panels." -Title "Search-Regex"
	/// Assert-Far ($Far.Window.Kind -eq 'Panels') "Run this script from panels." "Search-Regex"
	/// </code>
	/// </example>
	/// </remarks>
	[Description("Checks for the condition(s) and stops the pipeline with a message if any condition is false or not Boolean.")]
	public sealed class AssertFarCommand : BaseCmdlet
	{
		internal const string MyName = "Assert-Far";
		/// <summary>
		/// A single Boolean value or an array of Boolean values to be checked.
		/// </summary>
		[Parameter(Position = 0, HelpMessage = "A Boolean value or an array of Boolean values to be checked.")]
		public object Conditions { get; set; }
		/// <summary>
		/// The message to display on failure or a script block to invoke and get the message.
		/// </summary>
		[Parameter(Position = 1, HelpMessage = "The message to display on failure or a script block to invoke and get the message.")]
		public object Message { get; set; }
		/// <summary>
		/// The title of a simple message designed for production scripts.
		/// </summary>
		[Parameter(Position = 2, HelpMessage = "The title of a simple message designed for production scripts.")]
		public string Title { get; set; }
		/// <summary>
		/// Asserts the current file description.
		/// </summary>
		[Parameter(HelpMessage = "Asserts the current file description.")]
		public string FileDescription { get; set; }
		/// <summary>
		/// Asserts the current file name.
		/// </summary>
		[Parameter(HelpMessage = "Asserts the current file name.")]
		public string FileName { get; set; }
		/// <summary>
		/// Asserts the current file owner.
		/// </summary>
		[Parameter(HelpMessage = "Asserts the current file owner.")]
		public string FileOwner { get; set; }
		/// <summary>
		/// Checks the current window is dialog.
		/// </summary>
		[Parameter(HelpMessage = "Checks the current window is dialog.")]
		public SwitchParameter Dialog { get; set; }
		/// <summary>
		/// Checks the current window is editor.
		/// </summary>
		[Parameter(HelpMessage = "Checks the current window is editor.")]
		public SwitchParameter Editor { get; set; }
		/// <summary>
		/// Checks the current window is panels.
		/// </summary>
		[Parameter(HelpMessage = "Checks the current window is panels.")]
		public SwitchParameter Panels { get; set; }
		/// <summary>
		/// Checks the current window is viewer.
		/// </summary>
		[Parameter(HelpMessage = "Checks the current window is viewer.")]
		public SwitchParameter Viewer { get; set; }
		/// <summary>
		/// Checks the active panel is plugin.
		/// </summary>
		[Parameter(HelpMessage = "Checks the active panel is plugin.")]
		public SwitchParameter Plugin { get; set; }
		/// <summary>
		/// Checks the passive panel is plugin.
		/// </summary>
		[Parameter(HelpMessage = "Checks the passive panel is plugin.")]
		public SwitchParameter Plugin2 { get; set; }
		/// <summary>
		/// Checks the active panel is native (not plugin).
		/// </summary>
		[Parameter(HelpMessage = "Checks the active panel is native (not plugin).")]
		public SwitchParameter Native { get; set; }
		/// <summary>
		/// Checks the passive panel is native (not plugin).
		/// </summary>
		[Parameter(HelpMessage = "Checks the passive panel is native (not plugin).")]
		public SwitchParameter Native2 { get; set; }
		bool IsError
		{
			get { return Title == null; }
		}
		int ConditionCount;
		int ConditionIndex;
		///
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void BeginProcessing()
		{
			// to be false on extra checks
			bool fail = true;

			// check dialog
			if (Dialog && (fail = Far.Net.Window.Kind != WindowKind.Dialog))
				Fail(Message ?? "The current window is expected to be dialog.");

			// check editor
			if (Editor && (fail = Far.Net.Window.Kind != WindowKind.Editor))
				Fail(Message ?? "The current window is expected to be editor.");

			// check panels
			if (Panels && (fail = Far.Net.Window.Kind != WindowKind.Panels))
				Fail(Message ?? "The current window is expected to be panels.");

			// check viewer
			if (Viewer && (fail = Far.Net.Window.Kind != WindowKind.Viewer))
				Fail(Message ?? "The current window is expected to be viewer.");

			// check plugin
			if (Plugin && (fail = !Far.Net.Panel.IsPlugin))
				Fail(Message ?? "The active panel is expected to be plugin.");

			// check plugin 2
			if (Plugin2 && (fail = !Far.Net.Panel2.IsPlugin))
				Fail(Message ?? "The passive panel is expected to be plugin.");

			// check native
			if (Native && (fail = Far.Net.Panel.IsPlugin))
				Fail(Message ?? "The active panel is expected to be native.");

			// check native 2
			if (Native2 && (fail = Far.Net.Panel2.IsPlugin))
				Fail(Message ?? "The passive panel is expected to be native.");

			// check file data
			if (FileDescription != null || FileName != null || FileOwner != null)
			{
				var file = Far.Net.Panel.CurrentFile;
				if (file == null)
					Fail(Message ?? "Expected the current panel file.");

				//1
				if (FileName != null && (fail = FileName != file.Name))
					Fail(Message ?? "Unexpected current file name.");

				//2
				if (FileDescription != null && (fail = FileDescription != file.Description))
					Fail(Message ?? "Unexpected current file description.");

				//3
				if (FileOwner != null && (fail = FileOwner != file.Owner))
					Fail(Message ?? "Unexpected current file owner.");
			}

			// at least one check is done and there are no conditions
			if (fail == false && Conditions == null)
				return;

			// check conditions
			object[] array = Conditions as object[];
			if (array == null)
			{
				Assert(Conditions);
			}
			else
			{
				ConditionCount = array.Length;
				for (ConditionIndex = 0; ConditionIndex < ConditionCount; ++ConditionIndex)
					Assert(array[ConditionIndex]);
			}
		}
		void Assert(object condition)
		{
			if (condition == null)
			{
				Title = null;
			}
			else
			{
				PSObject psobject = condition as PSObject;
				if (psobject != null)
					condition = psobject.BaseObject;

				if (condition.GetType() != typeof(bool))
				{
					Message = "Expected Boolean conditions; actual input type is " + condition.GetType().Name;
					Title = null;
				}
				else if ((bool)condition)
				{
					// OK
					return;
				}
			}

			Fail(null);
		}
		void Fail(object message)
		{
			// break a macro
			MacroState macroState = Far.Net.MacroState;
			if (macroState == MacroState.Executing || macroState == MacroState.ExecutingCommon)
				Far.Net.UI.Break();

			// get the message
			if (message == null)
			{
				ScriptBlock messageScript = Cast<ScriptBlock>.From(Message);
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

			// body
			//! use "\n" as the separator, not "\r": PositionMessage starts with "\n".
			string body = Message == null ? "Assertion failed" : Message.ToString();
			if (IsError)
			{
				if (ConditionCount > 0)
					body = string.Concat(body, (body.Length > 0 ? "\n" : string.Empty), "Condition ", ConditionIndex + 1, " out of ", ConditionCount);

				body = string.Concat(body, (body.Length > 0 ? "\n" : string.Empty), MyInvocation.PositionMessage);
			}

			// buttons
			string[] buttons;
			if (!IsError)
				buttons = new string[] { BtnBreak };
			else if (string.IsNullOrEmpty(MyInvocation.ScriptName))
				buttons = new string[] { BtnBreak, BtnDebug };
			else
				buttons = new string[] { BtnBreak, BtnDebug, BtnEdit };

			// prompt
			int result = Far.Net.Message(
				body,
				Title ?? MyName,
				IsError ? (MsgOptions.Warning | MsgOptions.LeftAligned) : MsgOptions.None,
				buttons);

			// editor
			if (result >= 0)
			{
				if (buttons[result] == BtnEdit)
				{
					IEditor editor = Far.Net.CreateEditor();
					editor.FileName = MyInvocation.ScriptName;
					editor.GoToLine(MyInvocation.ScriptLineNumber - 1);
					editor.Open();
				}
				else if (buttons[result] == BtnDebug)
				{
					A.Psf.InvokeCode("Set-PSBreakpoint -Command " + MyName);
					return;
				}
			}

			// break
			throw new PipelineStoppedException();
		}
		const string
			BtnBreak = "&Break",
			BtnDebug = "&Debug",
			BtnEdit = "&Edit";
	}
}
