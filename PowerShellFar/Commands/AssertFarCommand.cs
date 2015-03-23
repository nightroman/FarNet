
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System.Collections;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class AssertFarCommand : BaseCmdlet
	{
		internal const string MyName = "Assert-Far";

		[Parameter(Position = 0)]
		public object Conditions
		{
			get { return _Conditions; }
			set { _Conditions = value; _isConditions = true; }
		}
		object _Conditions;
		bool _isConditions;

		[Parameter(Position = 1)]
		public object Message { get; set; }

		[Parameter(Position = 2)]
		public string Title { get; set; }

		[Parameter]
		public string FileDescription { get; set; }

		[Parameter]
		public string FileName { get; set; }

		[Parameter]
		public string FileOwner { get; set; }

		[Parameter]
		public SwitchParameter Dialog { get; set; }

		[Parameter]
		public SwitchParameter Editor { get; set; }

		[Parameter]
		public SwitchParameter Panels { get; set; }

		[Parameter]
		public SwitchParameter Viewer { get; set; }

		[Parameter]
		public SwitchParameter Plugin { get; set; }

		[Parameter]
		public SwitchParameter Plugin2 { get; set; }

		[Parameter]
		public SwitchParameter Native { get; set; }

		[Parameter]
		public SwitchParameter Native2 { get; set; }

		bool IsError
		{
			get { return Title == null; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void BeginProcessing()
		{
			// to be false on extra checks
			bool fail = true;

			// check dialog
			if (Dialog && (fail = Far.Api.Window.Kind != WindowKind.Dialog))
				Fail(Message ?? "The current window is expected to be dialog.");

			// check editor
			if (Editor && (fail = Far.Api.Window.Kind != WindowKind.Editor))
				Fail(Message ?? "The current window is expected to be editor.");

			// check panels
			if (Panels && (fail = Far.Api.Window.Kind != WindowKind.Panels))
				Fail(Message ?? "The current window is expected to be panels.");

			// check viewer
			if (Viewer && (fail = Far.Api.Window.Kind != WindowKind.Viewer))
				Fail(Message ?? "The current window is expected to be viewer.");

			// check plugin
			if (Plugin && (fail = !Far.Api.Panel.IsPlugin))
				Fail(Message ?? "The active panel is expected to be plugin.");

			// check plugin 2
			if (Plugin2 && (fail = !Far.Api.Panel2.IsPlugin))
				Fail(Message ?? "The passive panel is expected to be plugin.");

			// check native
			if (Native && (fail = Far.Api.Panel.IsPlugin))
				Fail(Message ?? "The active panel is expected to be native.");

			// check native 2
			if (Native2 && (fail = Far.Api.Panel2.IsPlugin))
				Fail(Message ?? "The passive panel is expected to be native.");

			// check file data
			if (FileDescription != null || FileName != null || FileOwner != null)
			{
				var file = Far.Api.Panel.CurrentFile;
				if (file == null)
					Fail(Message ?? "Expected the current panel file.");

				//1 case sensitive!
				if (FileName != null && (fail = FileName != file.Name))
					Fail(Message ?? "Unexpected current file name.");

				//2 case sensitive!
				if (FileDescription != null && (fail = FileDescription != file.Description))
					Fail(Message ?? "Unexpected current file description.");

				//3 case sensitive!
				if (FileOwner != null && (fail = FileOwner != file.Owner))
					Fail(Message ?? "Unexpected current file owner.");
			}

			// at least one check is done and there are no conditions
			if (!fail && !_isConditions)
				return;

			// invoke script to get conditions
			var script = Cast<ScriptBlock>.From(Conditions);
			if (script != null)
			{
				AssertEnumerable(script.Invoke());
				return;
			}

			// check conditions
			var set = LanguagePrimitives.GetEnumerable(Conditions);
			if (set != null)
				AssertEnumerable(set);
			else if (!LanguagePrimitives.IsTrue(Conditions))
				Fail();
		}
		void AssertEnumerable(IEnumerable set)
		{
			int i = -1;
			foreach (var it in set)
			{
				++i;
				if (!LanguagePrimitives.IsTrue(it))
					Fail(null, i);
			}
			if (i < 0)
				Fail("Assertion set is empty.");
		}
		void Fail(object message = null, int conditionIndex = 0)
		{
			// break a macro
			MacroState macroState = Far.Api.MacroState;
			if (macroState == MacroState.Executing || macroState == MacroState.ExecutingCommon)
				Far.Api.UI.Break();

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
				if (conditionIndex > 0)
					body = string.Concat(body, (body.Length > 0 ? "\n" : string.Empty), "Condition #", conditionIndex + 1);

				//! Trim() for PowerShell 2.0
				body = string.Concat(body, (body.Length > 0 ? "\n" : string.Empty), MyInvocation.PositionMessage.Trim());
			}

			// buttons
			string[] buttons;
			if (!IsError)
				buttons = new string[] { BtnBreak, BtnThrow };
			else if (string.IsNullOrEmpty(MyInvocation.ScriptName))
				buttons = new string[] { BtnBreak, BtnThrow, BtnDebug };
			else
				buttons = new string[] { BtnBreak, BtnThrow, BtnDebug, BtnEdit };

			// prompt
			for (; ; )
			{
				int result = Far.Api.Message(
				body,
				Title ?? MyName,
				IsError ? (MessageOptions.Warning | MessageOptions.LeftAligned) : MessageOptions.None,
				buttons);

				if (result < 0)
					continue;

				switch (buttons[result])
				{
					case BtnBreak:
						{
							throw new PipelineStoppedException();
						}
					case BtnThrow:
						{
							throw new PSInvalidOperationException(body);
						}
					case BtnDebug:
						{
							A.InvokeCode("Set-PSBreakpoint -Variable daf01ff6-f004-43bd-b6bf-cf481e9333d3 -Mode Read");
							SessionState.PSVariable.Set("daf01ff6-f004-43bd-b6bf-cf481e9333d3", null);
							GetVariableValue("daf01ff6-f004-43bd-b6bf-cf481e9333d3");
							return;
						}
					case BtnEdit:
						{
							IEditor editor = Far.Api.CreateEditor();
							editor.FileName = MyInvocation.ScriptName;
							editor.GoToLine(MyInvocation.ScriptLineNumber - 1);
							editor.Open();
							goto case BtnBreak;
						}
				}
			}
		}
		const string
			BtnBreak = "&Break",
			BtnThrow = "&Throw",
			BtnDebug = "&Debug",
			BtnEdit = "&Edit";
	}
}
