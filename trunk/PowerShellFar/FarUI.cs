/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
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
		internal FarUI()
			: base()
		{ }

		/// <summary>
		/// Current writer or null
		/// </summary>
		internal AnyOutputWriter Writer
		{
			get
			{
				if (_writers.Count == 0)
					return null;
				return _writers.Peek();
			}
		}
		internal AnyOutputWriter PopWriter()
		{
			SetMode(WriteMode.None);
			return _writers.Pop();
		}
		internal void PushWriter(AnyOutputWriter writer)
		{
			SetMode(WriteMode.None);
			_writers.Push(writer);
		}
		Stack<AnyOutputWriter> _writers = new Stack<AnyOutputWriter>();

		internal static void Check()
		{
		}

		internal override void Append(string value)
		{
			Check();
			if (_writers.Count == 0)
				Far.Net.Write(value);
			else
				Writer.Append(value);
		}

		internal override void AppendLine()
		{
			Check();
			if (_writers.Count == 0)
				Far.Net.Write("\r\n");
			else
				Writer.AppendLine();
		}

		internal override void AppendLine(string value)
		{
			Check();
			if (_writers.Count == 0)
				Far.Net.Write(value + "\r\n");
			else
				Writer.AppendLine(value);
		}

		#region PSHostUserInterface

		/// <summary>
		/// Shows a dialog with a number of input fields.
		/// </summary>
		public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
		{
			Check();
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
			Check();
			UI.InputDialog ui = new UI.InputDialog(string.Empty, Res.HistoryPrompt);
			return ui.UIDialog.Show() ? ui.UICode.Text : string.Empty;
		}

		/// <summary>
		/// Shows progress information. Used by Write-Progress cmdlet.
		/// It actually works at most once a second (for better performance on frequent calls).
		/// </summary>
		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
			Check();

			// done
			if (record.RecordType == ProgressRecordType.Completed)
			{
				// title
				Console.Title = "Done : " + record.Activity + " : " + record.StatusDescription;

				// win7 NoProgress
				Far.Net.SetProgressState(FarNet.TaskbarProgressBarState.NoProgress);

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
			Console.Title = text;

			// win7 %
			Far.Net.SetProgressValue(record.PercentComplete, 100);
		}
		Stopwatch _progressWatch = Stopwatch.StartNew();

		#endregion
	}
}
