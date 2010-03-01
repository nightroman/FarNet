/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

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

		bool IsError
		{
			get { return Title == null; }
		}

		int ConditionCount;
		int ConditionIndex;

		///
		protected override void BeginProcessing()
		{
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

			// break a macro
			FarMacroState macroState = Far.Net.MacroState;
			if (macroState == FarMacroState.Executing || macroState == FarMacroState.ExecutingCommon)
				Far.Net.Zoo.Break();

			// get the message
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
