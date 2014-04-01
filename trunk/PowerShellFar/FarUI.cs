
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;
using FarNet;

namespace PowerShellFar
{
	class FarUI : UniformUI
	{
		const string TextDefaultChoiceForMultipleChoices = @"(default is ""{0}"")";
		const string TextDefaultChoicePrompt = @"(default is ""{0}"")";
		const string TextDefaultChoicesForMultipleChoices = "(default choices are {0})";
		const string TextPrompt = ": ";
		const string TextPromptForChoiceHelp = "[?] Help ";

		const ConsoleColor ForegroundColor = ConsoleColor.Gray;
		const ConsoleColor BackgroundColor = ConsoleColor.Black;
		const ConsoleColor PromptColor = ConsoleColor.White;
		const ConsoleColor DefaultPromptColor = ConsoleColor.Yellow;

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
			var r = new Dictionary<string, PSObject>();

			if (Far.Api.UI.IsCommandMode)
			{
				if (!string.IsNullOrEmpty(caption))
					WriteLine(PromptColor, BackgroundColor, caption);
				if (!string.IsNullOrEmpty(message))
					WriteLine(message);
			}

			foreach (var current in descriptions)
			{
				var prompt = current.Name;

				var type = Type.GetType(current.ParameterAssemblyFullName);
				if (type.GetInterface(typeof(IList).FullName) != null)
				{
					var arrayList = new ArrayList();
					for (; ; )
					{
						var prompt2 = string.Format(null, "{0}[{1}]", prompt, arrayList.Count);
						string text;
						if (Far.Api.UI.IsCommandMode)
						{
							WriteLine(prompt2);

							//TODO HelpMessage - is fine by [F1]?
							var ui = new UI.ReadLine() { Prompt = TextPrompt, HelpMessage = current.HelpMessage, History = Res.HistoryPrompt };
							if (!ui.Show() || (text = ui.Text).Length == 0)
								break;

							WriteLine(TextPrompt + text);
						}
						else
						{
							//TODO HelpMessage - not done
							var ui = new UI.InputBoxEx()
							{
								Title = caption,
								Prompt = string.IsNullOrEmpty(message) ? prompt2 : message + "\r" + prompt2,
								History = Res.HistoryPrompt
							};
							if (!ui.Show() || (text = ui.Text).Length == 0)
								break;
						}
						arrayList.Add(text);
					}
					r.Add(prompt, new PSObject(arrayList));
				}
				else
				{
					var safe = type == typeof(SecureString);

					string text;
					if (Far.Api.UI.IsCommandMode)
					{
						WriteLine(prompt);

						//TODO HelpMessage - [F1] - really?
						var ui = new UI.ReadLine() { Prompt = TextPrompt, HelpMessage = current.HelpMessage, History = Res.HistoryPrompt, Password = safe };
						if (!ui.Show())
							break;

						text = ui.Text;
						WriteLine(TextPrompt + (safe ? "*" : text));
					}
					else
					{
						//TODO HelpMessage - not done
						var ui = new UI.InputBoxEx()
						{
							Title = caption,
							Prompt = string.IsNullOrEmpty(message) ? prompt : message + "\r" + prompt,
							History = Res.HistoryPrompt,
							Password = safe
						};
						if (!ui.Show())
							break;

						text = ui.Text;
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
			if (choices == null || choices.Count == 0)
				throw new ArgumentException("choices");
			if (defaultChoice < -1 || defaultChoice >= choices.Count)
				throw new ArgumentException("defaultChoice");

			//! DON'T Check(): crash on pressed CTRL-C and an error in 'Inquire' mode
			//! 090211 The above is obsolete, perhaps.
			if (!Far.Api.UI.IsCommandMode)
				return UI.ChoiceMsg.Show(caption, message, choices);

			WriteLine();
			if (!string.IsNullOrEmpty(caption))
				WriteLine(PromptColor, BackgroundColor, caption);
			if (!string.IsNullOrEmpty(message))
				WriteLine(message);

			string[,] hotkeysAndPlainLabels = null;
			BuildHotkeysAndPlainLabels(choices, out hotkeysAndPlainLabels);

			Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
			if (defaultChoice >= 0)
				dictionary.Add(defaultChoice, true);

			for (; ; )
			{
				WriteChoicePrompt(hotkeysAndPlainLabels, dictionary, false);

				var ui = new UI.ReadLine() { Prompt = TextPrompt };
				if (!ui.Show())
					throw new PipelineStoppedException(); //TODO

				var text = ui.Text;
				
				// echo
				WriteLine(TextPrompt + ui.Text);

				if (text.Length == 0)
				{
					if (defaultChoice >= 0)
						return defaultChoice;
				}
				else
				{
					if (text.Trim() == "?")
					{
						ShowChoiceHelp(choices, hotkeysAndPlainLabels);
					}
					else
					{
						var num = DetermineChoicePicked(text.Trim(), choices, hotkeysAndPlainLabels);
						if (num >= 0)
							return num;
					}
				}
			}
		}
		static int DetermineChoicePicked(string response, Collection<ChoiceDescription> choices, string[,] hotkeysAndPlainLabels)
		{
			int num = -1;
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			for (int i = 0; i < choices.Count; i++)
			{
				if (string.Compare(response, hotkeysAndPlainLabels[1, i], true, currentCulture) == 0)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				for (int j = 0; j < choices.Count; j++)
				{
					if (hotkeysAndPlainLabels[0, j].Length > 0 && string.Compare(response, hotkeysAndPlainLabels[0, j], true, currentCulture) == 0)
					{
						num = j;
						break;
					}
				}
			}
			return num;
		}
		//! c&p
		void ShowChoiceHelp(Collection<ChoiceDescription> choices, string[,] hotkeysAndPlainLabels)
		{
			for (int i = 0; i < choices.Count; i++)
			{
				string text;
				if (hotkeysAndPlainLabels[0, i].Length > 0)
					text = hotkeysAndPlainLabels[0, i];
				else
					text = hotkeysAndPlainLabels[1, i];
				WriteLine(string.Format(null, "{0} - {1}", text, choices[i].HelpMessage));
			}
		}
		//! c&p
		void WriteChoicePrompt(string[,] hotkeysAndPlainLabels, Dictionary<int, bool> defaultChoiceKeys, bool shouldEmulateForMultipleChoiceSelection)
		{
			int lineLenMax = RawUI.BufferSize.Width - 1;
			string format = "[{0}] {1}  ";
			
			for (int i = 0; i < hotkeysAndPlainLabels.GetLength(1); i++)
			{
				ConsoleColor fg = PromptColor;
				if (defaultChoiceKeys.ContainsKey(i))
					fg = DefaultPromptColor;

				string text = string.Format(null, format, hotkeysAndPlainLabels[0, i], hotkeysAndPlainLabels[1, i]);
				WriteChoiceHelper(text, fg, BackgroundColor, lineLenMax);
				if (shouldEmulateForMultipleChoiceSelection)
					WriteLine();
			}

			WriteChoiceHelper(TextPromptForChoiceHelp, ForegroundColor, BackgroundColor, lineLenMax);
			if (shouldEmulateForMultipleChoiceSelection)
				WriteLine();

			string text2 = "";
			if (defaultChoiceKeys.Count > 0)
			{
				string text3 = "";
				StringBuilder stringBuilder = new StringBuilder();
				foreach (int current in defaultChoiceKeys.Keys)
				{
					string text4 = hotkeysAndPlainLabels[0, current];
					if (string.IsNullOrEmpty(text4))
						text4 = hotkeysAndPlainLabels[1, current];

					stringBuilder.Append(string.Format(null, "{0}{1}", text3, text4));
					text3 = ",";
				}
				string o = stringBuilder.ToString();
				if (defaultChoiceKeys.Count == 1)
				{
					text2 = shouldEmulateForMultipleChoiceSelection ?
						string.Format(null, TextDefaultChoiceForMultipleChoices, o) :
						string.Format(null, TextDefaultChoicePrompt, o);
				}
				else
				{
					text2 = string.Format(null, TextDefaultChoicesForMultipleChoices, o);
				}
			}
			
			WriteChoiceHelper(text2, ForegroundColor, BackgroundColor, lineLenMax);
			WriteLine(); //! or it is under the dialog
		}
		//! revised
		// MS issues: wrapped line text misses end spaces due to TrimEnd(); 1st very long line is written with new line before it.
		// Do: if (text can be written without wrapping) {write as it is} else (write it from a new line).
		// We use the cursor just because we have it. It is simple and has no issues.
		void WriteChoiceHelper(string text, ConsoleColor fg, ConsoleColor bg, int lineLenMax)
		{
			int x = Far.Api.UI.BufferCursor.X;
			if (x > 0 && x + text.Length >= lineLenMax)
				WriteLine();
			Write(fg, bg, text);
		}
		//! c&p
		static void BuildHotkeysAndPlainLabels(Collection<ChoiceDescription> choices, out string[,] hotkeysAndPlainLabels)
		{
			hotkeysAndPlainLabels = new string[2, choices.Count];
			for (int i = 0; i < choices.Count; i++)
			{
				hotkeysAndPlainLabels[0, i] = string.Empty;
				int num = choices[i].Label.IndexOf('&');
				if (num >= 0)
				{
					StringBuilder stringBuilder = new StringBuilder(choices[i].Label.Substring(0, num), choices[i].Label.Length);
					if (num + 1 < choices[i].Label.Length)
					{
						stringBuilder.Append(choices[i].Label.Substring(num + 1));
						hotkeysAndPlainLabels[0, i] = choices[i].Label.Substring(num + 1, 1).Trim().ToUpper(CultureInfo.CurrentCulture);
					}
					hotkeysAndPlainLabels[1, i] = stringBuilder.ToString().Trim();
				}
				else
				{
					hotkeysAndPlainLabels[1, i] = choices[i].Label;
				}
				if (hotkeysAndPlainLabels[0, i] == "?")
					throw new InvalidOperationException("Invalid hotkey '?'.");
			}
		}
		/// <summary>
		/// Reads a string.
		/// </summary>
		public override string ReadLine()
		{
			string text;
			if (Far.Api.UI.IsCommandMode)
			{
				var ui = new UI.ReadLine() { History = Res.HistoryPrompt };
				text = ui.Show() ? ui.Text : string.Empty;
				WriteLine(text);
			}
			else
			{
				var ui = new UI.InputDialog() { History = Res.HistoryPrompt };
				text = ui.Show() ? ui.Text : string.Empty;
			}
			return text;
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
