
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;
using FarNet;

namespace PowerShellFar
{
	public abstract partial class AnyPanel
	{
		#region OpenFile
		/// <summary>
		/// Gets or sets the script to open a file (e.g. on [Enter]).
		/// Variables: <c>$this</c> is this panel, <c>$_</c> is <see cref="FileEventArgs"/>.
		/// </summary>
		public ScriptBlock AsOpenFile { get; set; }
		/// <summary>
		/// Opens the file using <see cref="AsOpenFile"/> or the default method.
		/// </summary>
		internal void UIOpenFile(FarFile file)
		{
			if (file == null)
				return;

			// lookup closer?
			if (UserWants == UserAction.Enter && _LookupCloser != null)
			{
				_LookupCloser(this, new FileEventArgs() { File = file });
				UIEscape(false);
				return;
			}

			// default or external action
			if (AsOpenFile == null)
				OpenFile(file);
			else
				A.InvokeScriptReturnAsIs(AsOpenFile, this, new FileEventArgs() { File = file });
		}
		/// <summary>
		/// Opens a file.
		/// </summary>
		public virtual void OpenFile(FarFile file)
		{
			if (file == null)
				throw new ArgumentNullException("file");

			if (file.Data == null)
				return;

			//! use try, e.g. Invoke-Item throws exception with any error action (PS bug?)
			try
			{
				// case: file system
				FileSystemInfo fi = Cast<FileSystemInfo>.From(file.Data);
				if (fi != null)
				{
					A.Psf.InvokeCode("Invoke-Item -LiteralPath $args[0] -ErrorAction Stop", fi.FullName);
					return;
				}
			}
			catch (RuntimeException ex)
			{
				A.Msg(ex);
			}
		}
		#endregion
		#region DeleteFiles
		/// <summary>
		/// Gets or sets the script to delete files (e.g. on [F8]).
		/// Variables: <c>$this</c> is this panel, <c>$_</c> is <see cref="FilesEventArgs"/>.
		/// </summary>
		public ScriptBlock AsDeleteFiles { get; set; }
		/// <summary>
		/// Deletes files using <see cref="AsDeleteFiles"/> or the default method.
		/// </summary>
		public override sealed void UIDeleteFiles(FilesEventArgs args)
		{
			if (args == null)
				return;

			if (AsDeleteFiles != null)
			{
				A.InvokeScriptReturnAsIs(AsDeleteFiles, this, args);
				if (args.Ignore)
					return;
			}

			DoDeleteFiles(args);
		}
		#endregion
		#region EditFile
		/// <summary>
		/// Gets or sets the script to edit a file (e.g. on [F4]).
		/// Variables: <c>$this</c> is this panel, <c>$_</c> is <see cref="FileEventArgs"/>.
		/// </summary>
		public ScriptBlock AsEditFile { get; set; }
		/// <summary>
		/// Opens the file in the editor using <see cref="AsEditFile"/> or the default method.
		/// </summary>
		public override sealed void UIEditFile(FarFile file) //_091202_073429 NB: Data can be wrapped by PSObject.
		{
			if (file == null)
				return;

			if (AsEditFile != null)
			{
				var args = new FileEventArgs() { File = file };
				A.InvokeScriptReturnAsIs(AsEditFile, this, args);
				if (args.Result != JobResult.Default)
					return;
			}

			DoEditFile(file);
		}
		internal virtual void DoEditFile(FarFile file)
		{
			// no data, no job
			if (file.Data == null)
				return;

			// get file and open in internal/external editor
			string filePath = My.PathEx.TryGetFilePath(file.Data);
			if (filePath != null)
			{
				A.CreateEditor(filePath).Open(OpenMode.None); //???? use explorer?
				return;
			}

			// source info
			PSObject data = PSObject.AsPSObject(file.Data);
			if (data.BaseObject.GetType().Name == "MatchInfo")
			{
				string path;
				if (!A.TryGetPropertyValue(data, "Path", out path))
					return;
				int lineNumber;
				if (!A.TryGetPropertyValue(data, "LineNumber", out lineNumber))
					return;
				Match[] matches;
				if (!A.TryGetPropertyValue(data, "Matches", out matches) || matches.Length == 0)
					return;

				IEditor editor = Far.Net.CreateEditor();
				editor.DisableHistory = true;
				editor.FileName = path;
				TextFrame frame = new TextFrame();
				frame.CaretLine = lineNumber - 1;
				editor.Frame = frame;
				editor.Open();
				frame.VisibleLine = frame.CaretLine - Far.Net.UI.WindowSize.Y / 3;
				editor.Frame = frame;
				ILine line = editor[-1]; // can be null if a file is already opened
				if (line != null)
				{
					int end = matches[0].Index + matches[0].Length;
					line.Caret = end;
					line.SelectText(matches[0].Index, end);
					editor.Redraw();
				}
			}
		}
		#endregion
		#region ViewFile
		/// <summary>
		/// Gets or sets the script to view a file (e.g. on [F3]).
		/// Variables: <c>$this</c> is this panel, <c>$_</c> is <see cref="FileEventArgs"/>.
		/// </summary>
		public ScriptBlock AsViewFile { get; set; }
		/// <summary>
		/// Opens the file in the viewer using <see cref="AsViewFile"/> or the default method.
		/// </summary>
		public override sealed void UIViewFile(FarFile file) //_091202_073429
		{
			if (file == null)
				return;

			if (AsViewFile != null)
			{
				var args = new FileEventArgs() { File = file };
				A.InvokeScriptReturnAsIs(AsViewFile, this, args);
				if (args.Result != JobResult.Default)
					return;
			}

			// get file path and open in internal viewer
			string filePath = My.PathEx.TryGetFilePath(file.Data);
			if (filePath != null)
			{
				IViewer view = A.CreateViewer(filePath);
				view.Open(OpenMode.None);
				return;
			}

			//! use `try` to delete a tmp file, error can be at root of cert (PS bug?)
			string tmp = Far.Net.TempName();
			try
			{
				WriteFile(file, tmp);

				IViewer v = A.CreateViewer(tmp);
				v.DisableHistory = true;
				v.Title = file.Name;
				v.Open(OpenMode.None);
			}
			finally
			{
				File.Delete(tmp); //???? bad: open 1, don't close, then open 2: TempName() gets the same name --> cannot open 2
			}
		}
		#endregion
		#region ViewAll
		/// <summary>
		/// Gets or sets the script to show all files information (e.g. on [F3] on the dots).
		/// Variables: <c>$this</c> is this panel.
		/// </summary>
		public ScriptBlock AsViewAll { get; set; }
		/// <summary>
		/// Shows all files information using <see cref="AsViewAll"/> or the default method.
		/// </summary>
		void UIViewAll()
		{
			if (AsViewAll != null)
			{
				A.InvokeScriptReturnAsIs(AsViewAll, this, null);
				return;
			}

			string tmp = Far.Net.TempName();
			try
			{
				A.Psf.InvokeCode("$args[0] | Format-Table -AutoSize -ea 0 | Out-File -FilePath $args[1]", ShownItems, tmp);

				IViewer v = A.CreateViewer(tmp);
				v.DisableHistory = true;
				v.Title = CurrentDirectory;
				v.Open(OpenMode.None);
			}
			finally
			{
				File.Delete(tmp);
			}
		}
		#endregion
	}
}
