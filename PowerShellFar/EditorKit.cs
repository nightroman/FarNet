
using FarNet;
using FarNet.Forms;
using FarNet.Tools;
using System.Collections.Specialized;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.RegularExpressions;

namespace PowerShellFar;

/// <summary>
/// Editor tools.
/// </summary>
static class EditorKit
{
	static bool _doneTabExpansion;

	static void InitTabExpansion()
	{
		if (!_doneTabExpansion)
		{
			_doneTabExpansion = true;
			InitTabExpansion(null);
		}
	}

	//! It is called once in the main session and once per each local and remote session.
	public static void InitTabExpansion(Runspace? runspace)
	{
		// load TabExpansion
		using var ps = runspace is null ? A.Psf.NewPowerShell() : PowerShell.Create();
		if (runspace is not null)
			ps.Runspace = runspace;

		ps.AddCommand(Path.Combine(A.Psf.AppHome, "TabExpansion2.ps1"), false).Invoke();
	}

	/// <summary>
	/// Expands PowerShell code in an edit line.
	/// </summary>
	/// <param name="editLine">Editor line, command line or dialog edit box line; if null then <see cref="IFar.Line"/> is used.</param>
	/// <param name="runspace">Runspace or null for the main.</param>
	/// <seealso cref="Actor.ExpandCode"/>
	public static void ExpandCode(ILine? editLine, Runspace? runspace)
	{
		using FarHost.IgnoreApplications ignoreApplications = new();

		InitTabExpansion();
		A.Psf.SyncPaths();

		// hot line
		if (editLine is null)
		{
			editLine = Far.Api.Line;
			if (editLine is null)
			{
				A.Message("There is no current editor line.");
				return;
			}
		}

		ReadOnlySpan<char> prefix = default;
		ReadOnlySpan<char> inputScript;
		int lineOffset = 0;
		int cursorColumn;

		IEditor? editor = null;
		InteractiveArea? area;

		// script?
		if (editLine.WindowKind == WindowKind.Editor && My.PathEx.IsPSFile((editor = Far.Api.Editor)!.FileName))
		{
			int lineIndex = editor.Caret.Y;
			int lastIndex = editor.Count - 1;

			// previous text
			var sb = new StringBuilder();
			for (int i = 0; i < lineIndex; ++i)
				sb.AppendLine(editor[i].Text);

			// current line
			lineOffset = sb.Length;
			cursorColumn = lineOffset + editLine.Caret;

			// remaining text
			for (int i = lineIndex; i < lastIndex; ++i)
				sb.AppendLine(editor[i].Text);
			sb.Append(editor[lastIndex]);

			// whole text
			inputScript = sb.ToString();
		}
		// area?
		else if (editor != null && editor.Host is Interactive console && (area = console.CommandArea()) != null)
		{
			int lineIndex = area.Caret.Y;
			int lastIndex = area.LastLineIndex;

			// previous text
			var sb = new StringBuilder();
			for (int i = area.FirstLineIndex; i < lineIndex; ++i)
				sb.AppendLine(editor[i].Text);

			// current line
			lineOffset = sb.Length;
			cursorColumn = lineOffset + area.Caret.X;

			// remaining text
			for (int i = lineIndex; i < lastIndex; ++i)
				sb.AppendLine(editor[i].Text);
			sb.Append(editor[lastIndex]);

			// whole text
			inputScript = sb.ToString();
		}
		// line
		else
		{
			// original line
			inputScript = editLine.Text;
			cursorColumn = editLine.Caret;

			// 2020-08-05-0001 mind auto complete selection, use selection start as cursor column
			var selectionSpan = editLine.SelectionSpan;
			if (cursorColumn == selectionSpan.End)
				cursorColumn = selectionSpan.Start;

			// process prefix, accepts any
			FarNet.Works.Kit.SplitCommandWithPrefix(inputScript, out prefix, out inputScript, (_) => true);

			// correct caret
			cursorColumn -= prefix.Length;
			if (cursorColumn < 0)
				return;
		}

		// skip empty (also avoid errors)
		if (inputScript.Length == 0)
			return;

		// invoke
		try
		{
			// call TabExpansion
			using var ps = runspace is null ? A.Psf.NewPowerShell() : PowerShell.Create();
			if (runspace is not null)
				ps.Runspace = runspace;

			var result = (CommandCompletion)ps
				.AddCommand("TabExpansion2", true)
				.AddArgument(inputScript.ToString())
				.AddArgument(cursorColumn)
				.Invoke()[0].BaseObject;

			// results
			int replacementIndex = result.ReplacementIndex;
			int replacementLength = result.ReplacementLength;
			replacementIndex -= lineOffset;
			if (replacementIndex < 0 || replacementLength < 0)
				return;

			// original or joined list // mind null matches
			IReadOnlyList<CompletionResult> words = result.CompletionMatches;
			if (words is null)
				return;

			// variables from the current editor
			if (editLine.WindowKind == WindowKind.Editor)
			{
				// replaced text
				var lastWord = inputScript.Slice(lineOffset + replacementIndex, replacementLength).ToString();

				//! as TabExpansion.ps1 but ends with \$(\w*)$
				var matchVar = MyRegex.CompleteVariable().Match(lastWord);
				if (matchVar.Success)
				{
					var start = matchVar.Groups[1].Value;
					var scope = matchVar.Groups[2].Value;
					var re = new Regex(@"\$(global:|script:|private:)?(" + scope + matchVar.Groups[3].Value + @"\w+:?)", RegexOptions.IgnoreCase);

					var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					foreach (var line1 in Far.Api.Editor!.Lines)
					{
						foreach (var m in re.Matches(line1.Text).Cast<Match>())
						{
							var all = m.Value;
							if (all[^1] != ':')
							{
								variables.Add(start + all);
								if (scope.Length == 0 && m.Groups[1].Value.Length > 0)
									variables.Add(start + "$" + m.Groups[2].Value);
							}
						}
					}

					// join lists
					if (variables.Count > 0)
					{
						List<CompletionResult> list = new(variables.Count + words.Count);
						foreach (var text in variables.OrderBy(x => x))
						{
							if (!words.Any(x => text.Equals(x.CompletionText, StringComparison.OrdinalIgnoreCase)))
								list.Add(new CompletionResult(text, text, CompletionResultType.Variable, text));
						}

						list.AddRange(words);
						words = list;
					}
				}
			}

			// expand
			ExpandText(editLine, replacementIndex + prefix.Length, replacementLength, words);
		}
		catch (RuntimeException)
		{
		}
	}

	public static void ExpandText(ILine editLine, int replacementIndex, int replacementLength, IReadOnlyList<CompletionResult> words)
	{
		// select a word
		string word;
		if (words.Count == 1)
		{
			word = words[0].CompletionText;
		}
		else
		{
			// make menu
			IListMenu menu = Far.Api.CreateListMenu();
			var cursor = Far.Api.UI.WindowCursor;
			menu.X = cursor.X;
			menu.Y = cursor.Y;
			Settings.Default.PopupMenu(menu);

			// case: empty
			if (words.Count == 0)
			{
				menu.Add(Res.Empty).Disabled = true;
				menu.NoInfo = true;
				menu.Show();
				return;
			}

			menu.Incremental = "*";
			menu.IncrementalOptions = PatternOptions.Substring;

			// populate menu
			int end = words.Count - 1;
			for (int i = 0; i <= end; ++i)
			{
				var str1 = words[i].ListItemText;
				var str2 = words[i].CompletionText;
				if (i > 0 && str1 == words[i - 1].ListItemText || i < end && str1 == words[i + 1].ListItemText)
					str1 = str2;

				var item = new SetItem { Text = str1, Data = str2 };
				menu.Items.Add(item);
			}

			// show menu
			if (!menu.Show())
				return;

			word = (string)menu.Items[menu.Selected].Data!;
		}

		// line text and head to keep
		var text = editLine.Text.AsSpan();
		var head = text[..replacementIndex];

		// 2020-08-05-0001 mind auto complete selection, skip selected text
		var selectionSpan = editLine.SelectionSpan;
		var tail = selectionSpan.Start == replacementIndex + replacementLength ?
			text[selectionSpan.End..] :
			text[(replacementIndex + replacementLength)..];

		// set new text = old head + expanded + old tail
		editLine.Text = string.Concat(head, word, tail);

		// set caret
		editLine.Caret = head.Length + word.Length;
	}

	public static string ActiveText
	{
		get
		{
			// case: editor
			if (Far.Api.Window.Kind == WindowKind.Editor)
			{
				var editor = Far.Api.Editor!;
				if (editor.SelectionExists)
					return editor.GetSelectedText();
				return editor.Line.Text;
			}

			// other lines
			var line = Far.Api.Line;
			if (line is null)
				return string.Empty;
			else
				return line.ActiveText;
		}
		set
		{
			// case: editor
			if (Far.Api.Window.Kind == WindowKind.Editor)
			{
				var editor = Far.Api.Editor!;
				switch (editor.SelectionKind)
				{
					case PlaceKind.Column:
						throw new NotSupportedException("Rectangular selection is not supported.");
					case PlaceKind.Stream:
						editor.SetSelectedText(value);
						return;
				}

				editor.Line.Text = value;
				return;
			}

			// other lines
			var line = Far.Api.Line;
			if (line is null)
				throw new InvalidOperationException("There is no current text to set.");
			else
				line.ActiveText = value;
		}
	}

	public static void OnEditorFirstOpening(object? sender, EventArgs e)
	{
		A.Psf.Invoking();

		try
		{
			var profile = Entry.RoamingData + "\\Profile-Editor.ps1";
			if (File.Exists(profile))
			{
				using var ps = A.Psf.NewPowerShell();
				ps.AddCommand(profile, false).Invoke();
			}
		}
		catch (RuntimeException ex)
		{
			throw new RuntimeException("Error in Profile-Editor.ps1, see $Error for details.", ex);
		}
	}

	public static void OnEditorOpened(object? sender, EventArgs e)
	{
		var editor = (IEditor)sender!;
		var fileName = editor.FileName;
		bool isInteractive = fileName.EndsWith(Word.InteractiveSuffix, StringComparison.OrdinalIgnoreCase);
		if (isInteractive)
		{
			editor.Host ??= new Interactive(editor);
		}
		else if (My.PathEx.IsPSFile(fileName))
		{
			editor.KeyDown += OnKeyDownPSFile;
			editor.Changed += OnChangedPSFile;
		}
	}

	static void OnChangedPSFile(object? sender, EditorChangedEventArgs e)
	{
		if (e.Kind == EditorChangeKind.LineChanged)
			return;

		var editor = (IEditor)sender!;
		var line = e.Line + 1;
		var fullPath = Path.GetFullPath(editor.FileName); //!

		IEnumerable<LineBreakpoint>? bps = null;
		int delta = 0;
		if (e.Kind == EditorChangeKind.LineAdded)
		{
			delta = 1;
			bps = A.Psf.Breakpoints.Where(x => x.Line >= line && x.Script.Equals(fullPath, StringComparison.OrdinalIgnoreCase)).ToArray();
		}
		else
		{
			var bp = A.Psf.Breakpoints.FirstOrDefault(x => x.Line == line && x.Script.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
			if (bp != null)
				A.RemoveBreakpoint(bp);

			delta = -1;
			bps = A.Psf.Breakpoints.Where(x => x.Line > line && x.Script.Equals(fullPath, StringComparison.OrdinalIgnoreCase)).ToArray();
		}

		foreach (var bp in bps)
		{
			A.RemoveBreakpoint(bp);
			A.SetBreakpoint(bp.Script, bp.Line + delta, bp.Action);
		}
	}

	/// <summary>
	/// Called on key in *.ps1.
	/// </summary>
	static void OnKeyDownPSFile(object? sender, KeyEventArgs e)
	{
		switch (e.Key.VirtualKeyCode)
		{
			case KeyCode.F1:
				if (e.Key.IsShift())
				{
					// [ShiftF1]
					e.Ignore = true;
					Help.ShowHelpForContext();
				}
				return;
			case KeyCode.F5:
				if (e.Key.Is())
				{
					// [F5]
					e.Ignore = true;
					var editor = (IEditor)sender!;
					InvokeScriptFromEditor(editor);
				}
				return;
			case KeyCode.Tab:
				if (e.Key.Is())
				{
					// [Tab]
					var editor = (IEditor)sender!;
					if (!editor.SelectionExists && NeedsTabExpansion(editor))
					{
						// TabExpansion
						e.Ignore = true;
						A.Psf.ExpandCode(editor.Line);
						editor.Redraw();
					}
				}
				return;
		}
	}

	public static void InvokeSelectedCode()
	{
		string code;
		var from = Far.Api.Window.Kind;

		OutputWriter? writer = null;
		if (from == WindowKind.Editor)
		{
			var editor = Far.Api.Editor!;
			code = editor.GetSelectedText();
			if (string.IsNullOrEmpty(code))
				code = editor[editor.Caret.Y].Text;
		}
		else if (from == WindowKind.Dialog)
		{
			var dialog = Far.Api.Dialog!;
			if (dialog.Focused is not IEdit edit)
				return;
			code = edit.Line.SelectedText;
			if (string.IsNullOrEmpty(code))
				code = edit.Text;
		}
		else
		{
			var line = Far.Api.CommandLine;
			code = line.SelectedText;
			if (string.IsNullOrEmpty(code))
				code = line.Text;
			writer = new ConsoleOutputWriter();
		}

		// command
		FarNet.Works.Kit.SplitCommandWithPrefix(code, out _, out var command, Entry.IsMyPrefix);

		// history
		if (from == WindowKind.Panels)
			HistoryCommands.AddSessionLine(command.ToString());

		A.Psf.SyncPaths();
		A.Psf.Run(new RunArgs(command.ToString()) { Writer = writer });
	}

	internal static void PlayNativeEnter()
	{
		var text = Far.Api.CommandLine.Text;
		var caret = Far.Api.CommandLine.Caret;
		var span = Far.Api.CommandLine.SelectionSpan;

		Far.Api.CommandLine.Text = string.Empty;
		Far.Api.PostMacro("Keys 'Enter'");
		Far.Api.PostJob(() =>
		{
			Far.Api.CommandLine.Text = text;
			Far.Api.CommandLine.Caret = caret;
			if (span.Length >= 0)
				Far.Api.CommandLine.SelectText(span.Start, span.End);
		});
	}

	// PSF sets the current directory and location to the script directory.
	// This is often useful and consistent with invoking from panels.
	// NOTE: ISE [F5] does not.
	public static void InvokeScriptFromEditor(IEditor? editor)
	{
		// editor
		editor ??= Far.Api.Editor ?? throw new ModuleException("No current editor.");

		// commit
		editor.Save();

		var fileName = editor.FileName;

		// case: Invoke-Build
		if (fileName.EndsWith(".build.ps1", StringComparison.OrdinalIgnoreCase) ||
			fileName.EndsWith(".test.ps1", StringComparison.OrdinalIgnoreCase))
		{
			InvokeTaskFromEditor(editor);
			return;
		}

		// sync the directory and location to the script directory
		// maybe it is questionable but it is very handy too often
		string dir0, dir1;

		// save/set the directory, allow failures (e.g. a long path)
		// note: GetDirectoryName fails on a long path, too
		try
		{
			dir1 = Path.GetDirectoryName(fileName)!;
			dir0 = Environment.CurrentDirectory;
			Environment.CurrentDirectory = dir1;
		}
		catch (PathTooLongException)
		{
			// PowerShell is not able to invoke this script anyway, almost for sure
			Far.Api.Message("The script path is too long.\rInvoking is not supported.");
			return;
		}

		try
		{
			// push/set the location; let's ignore issues
			A.Psf.Engine.SessionState.Path.PushCurrentLocation(null);
			A.Psf.Engine.SessionState.Path.SetLocation(Kit.EscapeWildcard(dir1));

			// invoke the script by the runner or directly
			if (fileName.EndsWith(".fas.ps1", StringComparison.OrdinalIgnoreCase))
			{
				A.InvokeCode("Start-FarTask $args[0]", editor.FileName);
			}
			else
			{
				A.Psf.Run(new RunArgs($"& '{fileName.Replace("'", "''")}'"));
			}
		}
		finally
		{
			// restore the directory first
			Environment.CurrentDirectory = dir0;

			// then pop the location, it may fail perhaps
			A.Psf.Engine.SessionState.Path.PopLocation(null);
		}
	}

	//! Use PowerShell for getting tasks, script block fails with weird NRE on exit.
	public static void InvokeTaskFromEditor(IEditor editor)
	{
		var fileName = editor.FileName;

		void GoToError(RuntimeException ex, bool redraw)
		{
			//! InvocationInfo null on CtrlC in prompts
			if (ex.ErrorRecord.InvocationInfo is { } ii &&
				string.Equals(fileName, ii.ScriptName, StringComparison.OrdinalIgnoreCase))
			{
				editor.GoTo(ii.OffsetInLine - 1, ii.ScriptLineNumber - 1);
				if (redraw)
					editor.Redraw();
			}
		}

		try
		{
			// get tasks
			var ps = A.Psf.NewPowerShell();
			ps.AddScript("Invoke-Build ?? $args[0]").AddArgument(fileName);
			var tasks = (OrderedDictionary)ps.Invoke()[0].BaseObject;

			// find the caret task
			var taskName = ".";
			var lineIndex = editor.Caret.Y;
			foreach (PSObject pso in tasks.Values)
			{
				var ii = (InvocationInfo)pso.Properties["InvocationInfo"].Value;
				if (!string.Equals(fileName, ii.ScriptName, StringComparison.OrdinalIgnoreCase))
					continue;

				if ((ii.ScriptLineNumber - 1) > lineIndex)
					break;

				taskName = (string)pso.Properties["Name"].Value;
			}

			Far.Api.UI.ShowUserScreen();
			try
			{
				// invoke task
				var args = new RunArgs("Invoke-Build $args[0] $args[1]")
				{
					Arguments = [taskName, fileName],
					Writer = new ConsoleOutputWriter()
				};
				A.Psf.Run(args);

				// on error in the editor script go to its position
				//! do not redraw now or the editor is shown
				if (args.Reason is RuntimeException ex)
					GoToError(ex, false);

				Far.Api.UI.SetProgressState(TaskbarProgressBarState.Paused);
				Far.Api.UI.WindowTitle = "Press Esc to continue...";
				while (true)
				{
					var key = Far.Api.UI.ReadKey(ReadKeyOptions.IncludeKeyDown | ReadKeyOptions.IncludeKeyUp);
					if (key.VirtualKeyCode == KeyCode.Escape)
						break;
				}
				Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
			}
			finally
			{
				Far.Api.UI.SaveUserScreen();
			}
		}
		catch (RuntimeException ex)
		{
			// it is a build script issue more likely, go to its position and redraw, show the simple message
			GoToError(ex, true);
			Far.Api.Message(ex.Message, "Invoke-Build task", MessageOptions.Warning | MessageOptions.LeftAligned);
		}
	}

	// true if there is a solid char anywhere before the caret
	internal static bool NeedsTabExpansion(IEditor editor)
	{
		ILine line = editor.Line;
		string text = line.Text;

		int pos = line.Caret;
		if (pos > text.Length)
			return false;

		while (--pos >= 0)
			if (text[pos] > ' ')
				return true;

		return false;
	}
}
