
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;

namespace PowerShellFar
{
	class FarUI : UniformUI
	{
		readonly Stack<OutputWriter> _writers = new Stack<OutputWriter>();
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
			if (descriptions == null) throw new ArgumentNullException("descriptions");

			var r = new Dictionary<string, PSObject>();

			foreach (var current in descriptions)
			{
				var prompt = current.Name;

				var type = Type.GetType(current.ParameterAssemblyFullName);
				if (type.GetInterface(typeof(IList).FullName) != null)
				{
					var arrayList = new ArrayList();
					for (;;)
					{
						var prompt2 = string.Format(null, "{0}[{1}]", prompt, arrayList.Count);
						string text;
						{
							//TODO HelpMessage - not done
							for (;;)
							{
								var ui = new UI.InputBoxEx()
								{
									Title = caption,
									Prompt = string.IsNullOrEmpty(message) ? prompt2 : message + "\r" + prompt2,
									History = Res.HistoryPrompt
								};
								if (!ui.Show())
								{
									A.AskStopPipeline();
									continue;
								}
								text = ui.Text;
								break;
							}
						}
						if (text.Length == 0)
							break;

						arrayList.Add(text);
					}
					r.Add(prompt, new PSObject(arrayList));
				}
				else
				{
					var safe = type == typeof(SecureString);
					string text;
					{
						//TODO HelpMessage - not done
						for (;;)
						{
							var ui = new UI.InputBoxEx()
							{
								Title = caption,
								Prompt = string.IsNullOrEmpty(message) ? prompt : message + "\r" + prompt,
								History = Res.HistoryPrompt,
								Password = safe
							};
							if (!ui.Show())
							{
								A.AskStopPipeline();
								continue;
							}

							text = ui.Text;
							break;
						}
					}
					r.Add(prompt, ValueToResult(text, safe));
				}
			}
			return r;
		}
		/// <summary>
		/// Shows a dialog with a number of choices.
		/// </summary>
		public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		{
			if (choices == null || choices.Count == 0) throw new ArgumentOutOfRangeException("choices");
			if (defaultChoice < -1 || defaultChoice >= choices.Count) throw new ArgumentOutOfRangeException("defaultChoice");

			//! DON'T Check(): crash on pressed CTRL-C and an error in 'Inquire' mode
			//! 090211 The above is obsolete, perhaps.
			return UI.ChoiceMsg.Show(caption, message, choices);
		}
		/// <summary>
		/// Reads a string.
		/// </summary>
		public override string ReadLine()
		{
			string text;
			{
				for (;;)
				{
					var ui = new UI.InputDialog() { History = Res.HistoryPrompt };
					if (ui.Show())
					{
						text = ui.Text;
						break;
					}
					A.AskStopPipeline();
				}
			}
			return text;
		}
		/// <summary>
		/// True if there was progress Processing.
		/// </summary>
		internal bool IsProgressStarted;
		/// <summary>
		/// Used to avoid too frequent progress updates.
		/// </summary>
		Stopwatch _progressWatch = Stopwatch.StartNew();
		/// <summary>
		/// Shows progress information. Used by Write-Progress cmdlet.
		/// It actually works at most once a second (for better performance on frequent calls).
		/// </summary>
		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
			if (record == null) throw new ArgumentNullException("record");

			// done
			if (record.RecordType == ProgressRecordType.Completed)
			{
				//! PS may call Completed without Processing (`gcm zzz`).
				//! We do not want such a message in the console title.
				if (IsProgressStarted)
					Far.Api.UI.WindowTitle = "Done : " + record.Activity + " : " + record.StatusDescription;

				// win7 NoProgress
				Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);

				return;
			}

			// check time
			if (IsProgressStarted && _progressWatch.ElapsedMilliseconds < 1000)
				return;

			// update
			IsProgressStarted = true;
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
		#endregion
	}
}
