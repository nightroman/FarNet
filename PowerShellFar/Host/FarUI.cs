using FarNet;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;

namespace PowerShellFar;

class FarUI : UniformUI, IHostUISupportsMultipleChoiceSelection
{
	const string TextDefaultChoiceForMultipleChoices = @"(default is ""{0}"")";
	const string TextDefaultChoicePrompt = @"(default is ""{0}"")";
	const string TextDefaultChoicesForMultipleChoices = "(default choices are {0})";
	const string TextPromptSuffix = ": ";
	const string TextPromptForChoiceHelp = "[?] Help ";
	const int MaxReadLinePrompt = 50;

	const ConsoleColor ForegroundColor = ConsoleColor.Gray;
	const ConsoleColor BackgroundColor = ConsoleColor.Black;
	const ConsoleColor PromptColor = ConsoleColor.White;
	const ConsoleColor DefaultPromptColor = ConsoleColor.Yellow;

	AbcOutputWriter? _writer;
	readonly ConsoleOutputWriter _console = new();

	/// <summary>
	/// Gets true for using the console UI for read line, choice, etc.
	/// </summary>
	bool IsConsole()
	{
		if (_writer is ConsoleOutputWriter)
			return true;
		else
			return UI.ReadCommand.IsActive();
	}

	/// <summary>
	/// Gets the current explicit writer or the fallback console writer.
	/// </summary>
	internal override AbcOutputWriter Writer => _writer ?? _console;

	internal AbcOutputWriter PopWriter()
	{
		var head = _writer!;
		_writer = head.Next;
		return head;
	}

	internal void PushWriter(AbcOutputWriter writer)
	{
		writer.Next = _writer;
		_writer = writer;
	}

	#region IHostUISupportsMultipleChoiceSelection
	/// <summary>
	/// Shows a list of choices to select many.
	/// </summary>
	public Collection<int> PromptForChoice(string? caption, string? message, Collection<ChoiceDescription> choices, IEnumerable<int>? defaultChoices)
	{
		if (A.IsAsyncSession)
		{
			_writer?.Flush();
			return Tasks
				.Job(() => PromptForChoice(caption, message, choices, defaultChoices))
				.AwaitResult();
		}

		if (choices == null || choices.Count == 0) throw new ArgumentOutOfRangeException(nameof(choices));

		// defaults
		Dictionary<int, bool> defaults = [];
		if (defaultChoices is { })
		{
			foreach (var defaultChoice in defaultChoices)
			{
				if (defaultChoice < 0 || defaultChoice >= choices.Count)
					throw new ArgumentOutOfRangeException(nameof(defaultChoices));

				defaults.TryAdd(defaultChoice, true);
			}
		}

		// PS trims, e.g. message "\n" is discarded
		caption = caption is null ? string.Empty : caption.TrimEnd();
		message = message is null ? string.Empty : message.TrimEnd();

		if (!IsConsole())
		{
			return UI.ChoiceDialog.SelectMany(caption, message, choices, defaults)
				?? throw new PipelineStoppedException();
		}

		// See: EmulatePromptForMultipleChoice

		WriteLine();
		if (caption.Length > 0)
			WriteLine(PromptColor, BackgroundColor, caption);
		if (message.Length > 0)
			WriteLine(message);

		BuildHotkeysAndPlainLabels(choices, out string[,] hotkeysAndPlainLabels);

		WriteChoicePrompt(hotkeysAndPlainLabels, defaults, true);

		Collection<int> result = [];
		int num = 0;
		while (true)
		{
			var prompt = $"Choice[{num}]{TextPromptSuffix}";
			var ui = new UI.ReadLine(new UI.ReadLine.Args
			{
				Prompt = prompt,
				IsPromptForChoice = true,
			});
			if (!ui.Show())
				throw new PipelineStoppedException();

			var text = ui.Out;

			// new line
			var lineText = Far.Api.UI.GetBufferLineText(Far.Api.UI.BufferCursor.Y).Trim();
			if (lineText.Length > 0)
				WriteLine();

			// echo
			WriteLine($"{prompt}{text}");

			//: done
			if (text.Length == 0)
				break;

			//: help
			if (text.Trim() == "?")
			{
				ShowChoiceHelp(choices, hotkeysAndPlainLabels);
				continue;
			}

			//: choice
			var num2 = DetermineChoicePicked(text.Trim(), choices, hotkeysAndPlainLabels);
			if (num2 >= 0 && !result.Contains(num2))
			{
				++num;
				result.Add(num2);
			}
		}

		// nothing? add defaults
		if (result.Count == 0)
		{
			foreach (int key in defaults.Keys)
				result.Add(key);
		}

		return result;
	}

	#endregion

	#region PSHostUserInterface
	/// <summary>
	/// Shows a dialog with a number of input fields.
	/// </summary>
	public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
	{
		ArgumentNullException.ThrowIfNull(descriptions);

		if (A.IsAsyncSession)
		{
			_writer?.Flush();
			return Tasks
				.Job(() => Prompt(caption, message, descriptions))
				.AwaitResult();
		}

		var r = new Dictionary<string, PSObject>();

		if (IsConsole())
		{
			if (!string.IsNullOrEmpty(caption))
				WriteLine(PromptColor, BackgroundColor, caption);
			if (!string.IsNullOrEmpty(message))
				WriteLine(message);
		}

		foreach (var current in descriptions)
		{
			var prompt = current.Name;

			var type = Type.GetType(current.ParameterAssemblyFullName)!;
			if (type.GetInterface(typeof(IList).FullName!) != null)
			{
				var arrayList = new ArrayList();
				for (; ; )
				{
					var prompt2 = $"{prompt}[{arrayList.Count}]";
					string text;
					if (IsConsole())
					{
						var ui = new UI.ReadLine(new UI.ReadLine.Args
						{
							Prompt = prompt2 + TextPromptSuffix,
							History = Res.HistoryPrompt,
							HelpMessage = current.HelpMessage,
						});
						if (!ui.Show())
							throw new PipelineStoppedException();

						text = ui.Out;
						WriteLine(ui.In.Prompt + text);
					}
					else
					{
						//TODO HelpMessage - not done
						var ui = new UI.InputBoxEx()
						{
							Title = caption,
							Prompt = string.IsNullOrEmpty(message) ? prompt2 : message + "\r" + prompt2,
							History = Res.HistoryPrompt,
							TypeId = new Guid(Guids.PSPromptDialog)
						};

						if (!ui.Show())
							throw new PipelineStoppedException();

						text = ui.Text!;
					}

					if (text.Length == 0)
						break;

					arrayList.Add(text);
				}
				r.Add(prompt, new PSObject(arrayList));
			}
			else
			{
				// secure?
				var isSecret = type == typeof(SecureString);

				// conventional history prompt
				string history;
				bool useLastHistory;
				if (prompt.StartsWith("[[") && prompt.EndsWith("]]"))
				{
					history = prompt[2..(prompt.Length - 2)];
					useLastHistory = true;
				}
				else
				{
					history = Res.HistoryPrompt;
					useLastHistory = false;
				}

				string text;
				if (IsConsole())
				{
					string prompt2;
					if (isSecret && prompt == " ")
					{
						//: Read-Host -AsSecureString
						prompt2 = string.Empty;
					}
					else if (prompt.Length > MaxReadLinePrompt || prompt.Contains('\n'))
					{
						//: Read-Host -Prompt {too long|many lines}
						WriteLine(prompt);
						prompt2 = TextPromptSuffix;
					}
					else
					{
						prompt2 = prompt + TextPromptSuffix;
					}

					var ui = new UI.ReadLine(new UI.ReadLine.Args
					{
						Prompt = prompt2,
						History = history,
						HelpMessage = current.HelpMessage,
						IsPassword = isSecret,
						UseLastHistory = useLastHistory,
					});
					if (!ui.Show())
						throw new PipelineStoppedException();

					text = ui.Out;
					WriteLine(prompt2 + (isSecret ? "*" : text));
				}
				else
				{
					//TODO HelpMessage - not done
					var ui = new UI.InputBoxEx()
					{
						Title = caption,
						Prompt = string.IsNullOrEmpty(message) ? prompt : message + "\r" + prompt,
						History = history,
						Password = isSecret,
						UseLastHistory = useLastHistory,
						TypeId = new Guid(Guids.PSPromptDialog),
					};

					if (!ui.Show())
						throw new PipelineStoppedException();

					text = ui.Text!;
				}
				r.Add(prompt, ValueToResult(text, isSecret));
			}
		}

		return r;
	}

	/// <summary>
	/// Shows a list of choices to select one.
	/// </summary>
	public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
	{
		if (A.IsAsyncSession)
		{
			_writer?.Flush();
			return Tasks
				.Job(() => PromptForChoice(caption, message, choices, defaultChoice))
				.AwaitResult();
		}

		if (choices == null || choices.Count == 0) throw new ArgumentOutOfRangeException(nameof(choices));
		if (defaultChoice < -1 || defaultChoice >= choices.Count) throw new ArgumentOutOfRangeException(nameof(defaultChoice));

		// defaults
		Dictionary<int, bool> defaults = [];
		if (defaultChoice >= 0)
			defaults.Add(defaultChoice, true);

		// PS trims, e.g. message "\n" is discarded
		caption = caption is null ? string.Empty : caption.TrimEnd();
		message = message is null ? string.Empty : message.TrimEnd();

		if (!IsConsole())
		{
			int choice = UI.ChoiceDialog.SelectOne(caption, message, choices, defaults);
			if (choice < 0)
				throw new PipelineStoppedException();

			return choice;
		}

		WriteLine();
		if (caption.Length > 0)
			WriteLine(PromptColor, BackgroundColor, caption);
		if (message.Length > 0)
			WriteLine(message);

		BuildHotkeysAndPlainLabels(choices, out string[,] hotkeysAndPlainLabels);

		while (true)
		{
			WriteChoicePrompt(hotkeysAndPlainLabels, defaults, false);

			var ui = new UI.ReadLine(new UI.ReadLine.Args
			{
				Prompt = TextPromptSuffix,
				IsPromptForChoice = true,
			});
			if (!ui.Show())
				throw new PipelineStoppedException();

			var text = ui.Out;

			// new line
			var lineText = Far.Api.UI.GetBufferLineText(Far.Api.UI.BufferCursor.Y).Trim();
			if (lineText.Length > 0)
				WriteLine();

			// echo
			WriteLine(TextPromptSuffix + text);

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

	//! see PS ShowChoiceHelp
	internal static string GetChoiceHelp(Collection<ChoiceDescription> choices, string[,] hotkeysAndPlainLabels)
	{
		StringBuilder sb = new();
		for (int i = 0; i < choices.Count; i++)
		{
			string text;
			if (hotkeysAndPlainLabels[0, i].Length > 0)
				text = hotkeysAndPlainLabels[0, i];
			else
				text = hotkeysAndPlainLabels[1, i];
			sb.AppendLine(string.Format(null, "{0} - {1}", text, choices[i].HelpMessage));
		}
		sb.Append("Escape - stop the pipeline.");
		return sb.ToString();
	}

	//! from PS
	void ShowChoiceHelp(Collection<ChoiceDescription> choices, string[,] hotkeysAndPlainLabels)
	{
		WriteLine(GetChoiceHelp(choices, hotkeysAndPlainLabels));
	}

	//! from PS
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
			StringBuilder stringBuilder = new();
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

	//! from PS, revised
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

	//! from PS
	internal static void BuildHotkeysAndPlainLabels(Collection<ChoiceDescription> choices, out string[,] hotkeysAndPlainLabels)
	{
		hotkeysAndPlainLabels = new string[2, choices.Count];
		for (int i = 0; i < choices.Count; i++)
		{
			hotkeysAndPlainLabels[0, i] = string.Empty;
			var label = choices[i].Label;
			int num = label.IndexOf('&');
			if (num >= 0)
			{
				StringBuilder stringBuilder = new(label.Length);
				stringBuilder.Append(label.AsSpan(0, num));
				if (num + 1 < label.Length)
				{
					stringBuilder.Append(label.AsSpan(num + 1));
					hotkeysAndPlainLabels[0, i] = CultureInfo.CurrentCulture.TextInfo.ToUpper(label.AsSpan(num + 1, 1).Trim().ToString());
				}
				hotkeysAndPlainLabels[1, i] = stringBuilder.ToString().Trim();
			}
			else
			{
				hotkeysAndPlainLabels[1, i] = label;
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
		if (A.IsAsyncSession)
		{
			_writer?.Flush();
			return Tasks
				.Job(ReadLine)
				.AwaitResult();
		}

		if (IsConsole())
		{
			var ui = new UI.ReadLine(new UI.ReadLine.Args
			{
				History = Res.HistoryPrompt
			});
			if (!ui.Show())
				throw new PipelineStoppedException();

			var text = ui.Out;
			WriteLine(text);
			return text;
		}
		else
		{
			var ui = new UI.InputBox2();
			ui.Edit.History = Res.HistoryPrompt;
			var text = ui.Show();
			if (text != null)
				return text;

			throw new PipelineStoppedException();
		}
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
		ArgumentNullException.ThrowIfNull(record);

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
