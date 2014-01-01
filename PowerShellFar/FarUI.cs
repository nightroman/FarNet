
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Host;
using FarNet;

namespace PowerShellFar
{
	class FarUI : UniformUI
	{
		Stack<OutputWriter> _writers = new Stack<OutputWriter>();
		ConsoleOutputWriter _console;

		/// <summary>
		/// Current writer or the fallback console writer.
		/// </summary>
		internal override OutputWriter Writer
		{
			get
			{
				if (_writers.Count > 0)
					return _writers.Peek();

				if (_console == null)
					_console = new ConsoleOutputWriter();

				return _console;
			}
		}

		internal OutputWriter PopWriter()
		{
			return _writers.Pop();
		}

		internal void PushWriter(OutputWriter writer)
		{
			_writers.Push(writer);
		}

		#region PSHostUserInterface

		/// <summary>
		/// Shows a dialog with a number of input fields.
		/// </summary>
		public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
		{
			return UI.PromptDialog.Prompt(caption, message, descriptions);
		}

		/// <summary>
		/// Shows a dialog with a number of choices.
		/// </summary>
		public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		{
			//! DON'T Check(): crash on pressed CTRL-C and an error in 'Inquire' mode
			//! 090211 The above is obsolete, perhaps.
			return UI.ChoiceMsg.Show(caption, message, choices);
		}

		/// <summary>
		/// Reads a string.
		/// </summary>
		public override string ReadLine()
		{
			UI.InputDialog ui = new UI.InputDialog(string.Empty, Res.HistoryPrompt);
			return ui.UIDialog.Show() ? ui.UIEdit.Text : string.Empty;
		}

		/// <summary>
		/// Shows progress information. Used by Write-Progress cmdlet.
		/// It actually works at most once a second (for better performance on frequent calls).
		/// </summary>
		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
			if (record == null)
				throw new ArgumentNullException("record");

			// done
			if (record.RecordType == ProgressRecordType.Completed)
			{
				// title
				Far.Api.UI.WindowTitle = "Done : " + record.Activity + " : " + record.StatusDescription;

				// win7 NoProgress
				Far.Api.UI.SetProgressState(FarNet.TaskbarProgressBarState.NoProgress);

				return;
			}

			// check time
			if (_progressWatch.ElapsedMilliseconds < 1000)
				return;

			// update
			_progressWatch = Stopwatch.StartNew();
			string text = record.Activity + " : " + record.StatusDescription;
			if (record.PercentComplete > 0)
				text = string.Empty + record.PercentComplete + "% " + text;
			if (record.SecondsRemaining > 0)
				text = string.Empty + record.SecondsRemaining + " sec. " + text;
			Far.Api.UI.WindowTitle = text;

			// win7 %
			Far.Api.UI.SetProgressValue(record.PercentComplete, 100);
		}
		Stopwatch _progressWatch = Stopwatch.StartNew();

		#endregion
	}
}
