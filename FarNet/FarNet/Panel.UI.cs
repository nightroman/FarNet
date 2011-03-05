
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FarNet
{
	public partial class Panel
	{
		///
		public static ExportFileEventArgs WorksExportExplorerFile(Explorer explorer, Panel panel, ExplorerModes mode, FarFile file, string fileName)
		{
			if (explorer == null) throw new ArgumentNullException("explorer");

			if (!explorer.CanExportFile)
				return null;

			// export file
			Log.Source.TraceInformation("ExportFile");
			var args = new ExportFileEventArgs(panel, mode, file, fileName);
			explorer.ExportFile(args);
			if (args.Result != JobResult.Done)
				return null;

			// no text or an actual file exists?
			if (args.UseText == null || !string.IsNullOrEmpty(args.UseFileName))
				return args;

			// export text
			string text = args.UseText as string;
			if (text == null)
			{
				IEnumerable collection = args.UseText as IEnumerable;
				if (collection == null)
				{
					text = args.UseText.ToString();
				}
				else
				{
					// write collection
					using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.Unicode))
						foreach (var it in collection)
							writer.WriteLine(it);
					return args;
				}
			}

			// write text
			File.WriteAllText(fileName, text, Encoding.Unicode);
			return args;
		}
		/// <summary>
		/// Copy/move action.
		/// </summary>
		/// <remarks>
		/// The source and target panel are module panels.
		/// The target panel explorer accepts the selected files.
		/// </remarks>
		public void UICopyMove(bool move)
		{
			// target
			var that = TargetPanel;
			if (that == null)
				return;

			// can?
			if (!that.Explorer.CanAcceptFiles)
				return;

			// args
			var args = new AcceptFilesEventArgs(that, ExplorerModes.None, this.SelectedFiles, this.Explorer, move);
			if (args.Files.Count == 0)
				return;

			// call
			that.Explorer.AcceptFiles(args);
			if (args.Result == JobResult.Ignore)
				return;

			// the target may have new files, update, keep selection
			that.Post(args);
			that.Update(true);
			that.Redraw();

			// info
			bool isIncomplete = args.Result == JobResult.Incomplete;
			bool isAllToStay = isIncomplete && args.FilesToStay.Count == 0;

			// Copy: do not update the source, files are the same
			if (!move)
			{
				// keep it as it is
				if (isAllToStay || !SelectionExists)
					return;

				// drop selection
				this.UnselectAll();

				// recover
				if (isIncomplete)
					SelectFiles(args.FilesToStay, null);

				// show
				this.Redraw();
				return;
			}

			// Move: no need to delete or all to stay or cannot delete
			if (!args.Delete || isAllToStay || !this.Explorer.CanDeleteFiles)
			{
				// the source may have some files deleted, update, drop selection
				this.Update(false);

				// recover selection
				if (isIncomplete)
				{
					if (isAllToStay)
						SelectFiles(args.Files, null);
					else
						SelectFiles(args.FilesToStay, null);
				}

				// show
				this.Redraw();
				return;
			}

			// Move: delete is requested, delete the source files

			// exclude files to stay
			var filesToDelete = args.Files;
			if (isIncomplete)
			{
				var files = new List<FarFile>(filesToDelete);
				foreach (var file in args.FilesToStay)
					files.Remove(file);
				filesToDelete = files;
			}

			// call
			var argsDelete = new DeleteFilesEventArgs(this, ExplorerModes.Silent, filesToDelete, false);
			this.UIDeleteWorker(argsDelete, false);
			if (isIncomplete)
				SelectFiles(args.FilesToStay, null);

			// show
			Redraw();
		}
		/// <summary>
		/// Creates a new file or directory. It is called on [F7].
		/// </summary>
		public void UICreate()
		{
			// can?
			if (!Explorer.CanCreateFile)
				return;

			// call
			var args = new CreateFileEventArgs(this, ExplorerModes.None);
			Explorer.CreateFile(args);
			if (args.Result != JobResult.Done)
				return;

			// post
			Post(args);

			// show
			Update(true);
			Redraw();
		}
		/// <summary>
		/// Delete action.
		/// </summary>
		public void UIDelete(bool force)
		{
			// can?
			if (!Explorer.CanDeleteFiles)
				return;

			// args
			var args = new DeleteFilesEventArgs(this, ExplorerModes.None, SelectedFiles, force);
			if (args.Files.Count == 0)
				return;

			// call
			UIDeleteWorker(args, true);
		}
		/// <summary>
		/// Deletes files, heals selection.
		/// </summary>
		void UIDeleteWorker(DeleteFilesEventArgs args, bool redraw)
		{
			// call
			Explorer.DeleteFiles(args);
			if (args.Result == JobResult.Ignore)
				return;

			// to recover
			bool recover = args.Result == JobResult.Incomplete && SelectionExists;

			// update, drop selection
			Update(false);

			// recover selection
			if (recover)
				SelectFiles(args.Files, null);

			// done
			if (redraw)
				Redraw();
		}
		/// <summary>
		/// Called before <see cref="UIEscape"/>.
		/// </summary>
		/// <remarks>
		/// If <see cref="PanelEventArgs.Ignore"/> = true then the core does nothing.
		/// Otherwise it calls <see cref="UIEscape"/> to close the panel.
		/// </remarks>
		public event EventHandler<PanelKeyEventArgs> Escaping;
		///
		public void WorksEscaping(PanelKeyEventArgs e)
		{
			if (Escaping != null)
				Escaping(this, e);
		}
		/// <summary>
		/// Called when [Esc] or [ShiftEsc] is pressed and the command line is empty.
		/// </summary>
		/// <remarks>
		/// By default it closes the the panel itself or with all parent panels.
		/// The panel may override this method or use the <see cref="Escaping"/> event.
		/// </remarks>
		public void UIEscape(bool force)
		{
			if (!CanClose())
				return;

			if (force || _Parent == null)
			{
				// _090321_210416 We do not call Redraw(0, 0) to reset cursor to 0 any more.
				// See Mantis 1114: why it was needed. Now FarNet panels restore original state.

				// ask parents
				if (force)
				{
					for (var parent = _Parent; parent != null; parent = parent._Parent)
						if (!parent.CanClose())
							return;
				}

				// close
				_Panel.Close();
			}
			else
			{
				CloseChild();
			}
		}
		/// <summary>
		/// Opens the file in the editor.
		/// </summary>
		/// <remarks>
		/// The default method calls <see cref="FarNet.Explorer.ExportFile"/>  to get a temporary file to edit
		/// and <see cref="FarNet.Explorer.ImportFile"/> to save changes when the editor closes.
		/// The explorer should have at least export implemented.
		/// </remarks>
		public virtual void UIEditFile(FarFile file)
		{
			if (file == null)
				return;

			// target
			var temp = Far.Net.TempName();

			// export
			var xExportArgs = WorksExportExplorerFile(Explorer, this, ExplorerModes.Edit, file, temp);
			if (xExportArgs == null)
				return;

			// case: actual file exists
			var asExportFileEventArgs = xExportArgs as ExportFileEventArgs;
			if (asExportFileEventArgs != null && !string.IsNullOrEmpty(asExportFileEventArgs.UseFileName))
			{
				var editorActual = Far.Net.CreateEditor();
				editorActual.FileName = asExportFileEventArgs.UseFileName;
				editorActual.Title = file.Name;
				if (!asExportFileEventArgs.CanImport)
					editorActual.IsLocked = true;
				editorActual.Open();
				return;
			}

			// rename
			if (!string.IsNullOrEmpty(xExportArgs.UseFileExtension))
			{
				string temp2 = temp + xExportArgs.UseFileExtension;
				File.Move(temp, temp2);
				temp = temp2;
			}

			// editor
			var editorTemp = Far.Net.CreateEditor();
			editorTemp.DeleteSource = DeleteSource.File;
			editorTemp.FileName = temp;
			editorTemp.Title = file.Name;

			// future
			if (xExportArgs.CanImport)
			{
				if (Explorer.CanImportText)
				{
					editorTemp.Saving += delegate
					{
						var xImportTextArgs = new ImportTextEventArgs(this, ExplorerModes.Edit, file, editorTemp.GetText());
						Log.Source.TraceInformation("ImportText");
						Explorer.ImportText(xImportTextArgs);
					};
				}
				else
				{
					editorTemp.Closed += delegate //???? to use Saved (Far 3), update docs.
					{
						if (editorTemp.TimeOfSave == DateTime.MinValue)
							return;

						var xImportFileArgs = new ImportFileEventArgs(this, ExplorerModes.Edit, file, temp);
						Log.Source.TraceInformation("ImportFile");
						Explorer.ImportFile(xImportFileArgs);
					};
				}
			}
			else
			{
				// to lock
				editorTemp.IsLocked = true;
			}

			// go
			editorTemp.Open();
		}
		/// <summary>
		/// Opens the file in the viewer.
		/// </summary>
		/// <returns>True if it's done.</returns>
		/// <remarks>
		/// The default method calls <see cref="FarNet.Explorer.ExportFile"/> to get a temporary file to view.
		/// The explorer should have it implemented.
		/// </remarks>
		public virtual void UIViewFile(FarFile file)
		{
			if (file == null)
				return;

			// target
			var temp = Far.Net.TempName();

			// export
			var xExportArgs = WorksExportExplorerFile(Explorer, this, ExplorerModes.Edit, file, temp);
			if (xExportArgs == null)
				return;

			// case: actual file exists
			var asExportFileEventArgs = xExportArgs as ExportFileEventArgs;
			if (asExportFileEventArgs != null && !string.IsNullOrEmpty(asExportFileEventArgs.UseFileName))
			{
				var viewerActual = Far.Net.CreateViewer();
				viewerActual.FileName = asExportFileEventArgs.UseFileName;
				viewerActual.Title = file.Name;
				viewerActual.Open();
				return;
			}

			// temp viewer
			var viewerTemp = Far.Net.CreateViewer();
			viewerTemp.DeleteSource = DeleteSource.File;
			viewerTemp.FileName = temp;
			viewerTemp.Title = file.Name;
			viewerTemp.Open();
		}
		/// <summary>
		/// Called when a key is about to be processed.
		/// </summary>
		/// <remarks>
		/// Set <see cref="PanelEventArgs.Ignore"/> = true if the module processes the key.
		/// <para>
		/// Normally panels are not interested in this event,
		/// it is needed for advanced processing of some special keys.
		/// </para>
		/// <para>
		/// In Far Manager this event is called from the <c>ProcessKeyW</c> with the <c>PKF_PREPROCESS</c> flag.
		/// </para>
		/// </remarks>
		public event EventHandler<PanelKeyEventArgs> KeyPressing;
		///
		public void WorksKeyPressing(PanelKeyEventArgs e)
		{
			if (KeyPressing != null)
				KeyPressing(this, e);
		}
		/// <summary>
		/// Called when a key is about to be processed after the <see cref="KeyPressing"/> event.
		/// </summary>
		/// <param name="code">The <see cref="VKeyCode"/> value.</param>
		/// <param name="state">Key state flags.</param>
		/// <returns>True if the key has been processed.</returns>
		public virtual bool UIKeyPressing(int code, KeyStates state)
		{
			return false;
		}
		/// <summary>
		/// Called when a key is pressed.
		/// </summary>
		/// <remarks>
		/// Set <see cref="PanelEventArgs.Ignore"/> = true if the module processes the key itself.
		/// <para>
		/// The event is not called on some special keys processed internally.
		/// Normally panels are not interested in these keys,
		/// otherwise they should use the <see cref="KeyPressing"/> event.
		/// </para>
		/// <para>
		/// In Far Manager this event is called from the <c>ProcessKeyW</c> without the <c>PKF_PREPROCESS</c> flag.
		/// </para>
		/// </remarks>
		public event EventHandler<PanelKeyEventArgs> KeyPressed;
		///
		public void WorksKeyPressed(PanelKeyEventArgs e)
		{
			if (KeyPressed != null)
				KeyPressed(this, e);
		}
		/// <summary>
		/// Called when a key is pressed after the <see cref="KeyPressed"/> event.
		/// </summary>
		/// <param name="code">The <see cref="VKeyCode"/> value.</param>
		/// <param name="state">Key state flags.</param>
		/// <returns>True if the key has been processed.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public virtual bool UIKeyPressed(int code, KeyStates state)
		{
			switch (code)
			{
				case VKeyCode.F3:

					switch (state)
					{
						case KeyStates.None:
							if (RealNames)
								break;

							var file = CurrentFile;
							if (file == null || file.IsDirectory)
								break;

							UIViewFile(file);
							return true;
					}
					break;

				case VKeyCode.F4:

					switch (state)
					{
						case KeyStates.None:
							if (RealNames)
								break;

							var file = CurrentFile;
							if (file == null || file.IsDirectory)
								break;

							UIEditFile(file);
							return true;
					}
					break;

				case VKeyCode.F5:

					switch (state)
					{
						case KeyStates.None:
							var target = TargetPanel;
							if (target == null)
								break;

							UICopyMove(false);
							return true;
					}
					break;

				case VKeyCode.F6:

					switch (state)
					{
						case KeyStates.None:
							var target = TargetPanel;
							if (target == null)
								break;

							UICopyMove(true);
							return true;
					}
					break;

				case VKeyCode.F7:

					switch (state)
					{
						case KeyStates.None:
							if (RealNames && RealNamesMakeDirectory)
								break;

							UICreate();
							return true;
					}

					break;

				case VKeyCode.Delete:

					if (Far.Net.CommandLine.Length > 0)
						break;

					goto case VKeyCode.F8;

				case VKeyCode.F8:

					switch (state)
					{
						case KeyStates.None:
							goto case KeyStates.Shift;

						case KeyStates.Shift:
							if (RealNames && RealNamesDeleteFiles)
								break;

							UIDelete(state == KeyStates.Shift);
							return true;
					}
					break;
			}

			return false;
		}
	}
}
