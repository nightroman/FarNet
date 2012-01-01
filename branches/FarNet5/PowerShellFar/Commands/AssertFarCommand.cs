
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class AssertFarCommand : BaseCmdlet
	{
		internal const string MyName = "Assert-Far";
		[Parameter(Position = 0)]
		public object Conditions { get; set; }
		[Parameter(Position = 1)]
		public object Message { get; set; }
		[Parameter(Position = 2)]
		public string Title { get; set; }
		[Parameter()]
		public string FileDescription { get; set; }
		[Parameter()]
		public string FileName { get; set; }
		[Parameter()]
		public string FileOwner { get; set; }
		[Parameter()]
		public SwitchParameter Dialog { get; set; }
		[Parameter()]
		public SwitchParameter Editor { get; set; }
		[Parameter()]
		public SwitchParameter Panels { get; set; }
		[Parameter()]
		public SwitchParameter Viewer { get; set; }
		[Parameter()]
		public SwitchParameter Plugin { get; set; }
		[Parameter()]
		public SwitchParameter Plugin2 { get; set; }
		[Parameter()]
		public SwitchParameter Native { get; set; }
		[Parameter()]
		public SwitchParameter Native2 { get; set; }
		bool IsError
		{
			get { return Title == null; }
		}
		int ConditionCount;
		int ConditionIndex;
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
				// used to require Boolean; let's keep it simple
				if (LanguagePrimitives.IsTrue(condition))
					return;
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
				buttons = new string[] { BtnThrow, BtnBreak };
			else if (string.IsNullOrEmpty(MyInvocation.ScriptName))
				buttons = new string[] { BtnThrow, BtnBreak, BtnDebug };
			else
				buttons = new string[] { BtnThrow, BtnBreak, BtnDebug, BtnEdit };

			// prompt
			int result = Far.Net.Message(
				body,
				Title ?? MyName,
				IsError ? (MsgOptions.Warning | MsgOptions.LeftAligned) : MsgOptions.None,
				buttons);

			// editor
			if (result >= 0)
			{
				switch (buttons[result])
				{
					case BtnBreak:
						{
							throw new PipelineStoppedException();
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
							IEditor editor = Far.Net.CreateEditor();
							editor.FileName = MyInvocation.ScriptName;
							editor.GoToLine(MyInvocation.ScriptLineNumber - 1);
							editor.Open();
							break;
						}
				}
			}

			// throw
			throw new PSInvalidOperationException(body);
		}
		const string
			BtnBreak = "&Break",
			BtnThrow = "&Throw",
			BtnDebug = "&Debug",
			BtnEdit = "&Edit";
	}
}
