
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
		/// <summary>
		/// Called when files of another module panel have been changed.
		/// </summary>
		/// <remarks>
		/// This panel may want to be updated if it contains data related to that panel.
		/// </remarks>
		public virtual void OnThatFileChanged(Panel that, EventArgs args)
		{ }
		/// <summary>
		/// Called when files of this panel have been changed.
		/// </summary>
		/// <remarks>
		/// The base method calls <see cref="OnThatFileChanged"/>.
		/// </remarks>
		public virtual void OnThisFileChanged(EventArgs args)
		{
			var that = TargetPanel as Panel;
			if (that != null)
				that.OnThatFileChanged(this, args);
		}
		/// <summary>
		/// Called by <see cref="UIExplorerEntered"/>.
		/// </summary>
		public event EventHandler<ExplorerEnteredEventArgs> ExplorerEntered;
		/// <summary>
		/// It is called when a new explorer has been attached after one of the explore methods.
		/// </summary>
		/// <remarks>
		/// The base method triggers the <see cref="ExplorerEntered"/> event.
		/// </remarks>
		public virtual void UIExplorerEntered(ExplorerEnteredEventArgs args)
		{
			if (ExplorerEntered != null)
				ExplorerEntered(this, args);
		}
		///
		public static GetContentEventArgs WorksExportExplorerFile(Explorer explorer, Panel panel, ExplorerModes mode, FarFile file, string fileName)
		{
			if (explorer == null) throw new ArgumentNullException("explorer");
			if (panel == null) throw new ArgumentNullException("panel");

			if (!explorer.CanGetContent)
				return null;

			// export file
			Log.Source.TraceInformation("ExportFile");
			var args = new GetContentEventArgs(mode, file, fileName);
			panel.UIGetContent(args);
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
		public virtual void UICopyMove(bool move)
		{
			// target
			var that = TargetPanel;

			// commit
			if (that == null)
			{
				// can?
				if (!Explorer.CanExportFiles)
					return;

				// target?
				var native = Far.Net.Panel2;
				if (native.IsPlugin || native.Kind != PanelKind.File)
					return;

				// args
				var argsExport = new ExportFilesEventArgs(ExplorerModes.None, SelectedFiles, move, native.CurrentDirectory);
				if (argsExport.Files.Count == 0)
					return;

				// call
				UIExportFiles(argsExport);
				if (argsExport.Result == JobResult.Ignore)
					return;

				// complete
				UICopyMoveComplete(argsExport);
				return;
			}

			// can?
			if (!that.Explorer.CanAcceptFiles)
				return;

			// args
			var argsAccept = new AcceptFilesEventArgs(ExplorerModes.None, this.SelectedFiles, move, this.Explorer);
			if (argsAccept.Files.Count == 0)
				return;

			// call
			that.UIAcceptFiles(argsAccept);
			if (argsAccept.Result == JobResult.Ignore)
				return;

			// the target may have new files, update, keep selection
			that.Post(argsAccept);
			that.Update(true);
			that.Redraw();

			// complete
			UICopyMoveComplete(argsAccept);
		}
		void UICopyMoveComplete(CopyFilesEventArgs args)
		{
			// info
			bool isIncomplete = args.Result == JobResult.Incomplete;
			bool isAllToStay = isIncomplete && args.FilesToStay.Count == 0;

			// Copy: do not update the source, files are the same
			if (!args.Move)
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
			if (!args.ToDeleteFiles || isAllToStay || !this.Explorer.CanDeleteFiles)
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
				var filesToDelete2 = new List<FarFile>(filesToDelete);
				foreach (var file in args.FilesToStay)
					filesToDelete2.Remove(file);
				filesToDelete = filesToDelete2;
			}

			// call
			var argsDelete = new DeleteFilesEventArgs(ExplorerModes.Silent, filesToDelete, false);
			this.UIDeleteWithRecover(argsDelete, false);
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
			var args = new CreateFileEventArgs(ExplorerModes.None);
			UICreateFile(args);
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
			var args = new DeleteFilesEventArgs(ExplorerModes.None, SelectedFiles, force);
			if (args.Files.Count == 0)
				return;

			// call
			UIDeleteWithRecover(args, true);
		}
		/// <summary>
		/// Deletes files, heals selection.
		/// </summary>
		void UIDeleteWithRecover(DeleteFilesEventArgs args, bool redraw)
		{
			// call
			UIDeleteFiles(args);
			if (args.Result == JobResult.Ignore)
				return;

			// to recover
			bool recover = args.Result == JobResult.Incomplete && SelectionExists;

			// update, drop selection
			Update(false);

			// recover selection
			if (recover)
			{
				if (args.FilesToStay.Count > 0)
					SelectFiles(args.FilesToStay, null);
				else
					SelectFiles(args.Files, null);
			}

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
		/// The default method calls <see cref="FarNet.Explorer.GetContent"/>  to get a temporary file to edit
		/// and <see cref="FarNet.Explorer.SetFile"/> to save changes when the editor closes.
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
			var asExportFileEventArgs = xExportArgs as GetContentEventArgs;
			if (asExportFileEventArgs != null && !string.IsNullOrEmpty(asExportFileEventArgs.UseFileName))
			{
				var editorActual = Far.Net.CreateEditor();
				editorActual.FileName = asExportFileEventArgs.UseFileName;
				editorActual.Title = file.Name;
				if (!asExportFileEventArgs.CanSet)
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
			if (xExportArgs.CanSet)
			{
				if (Explorer.CanSetText)
				{
					editorTemp.Saving += delegate
					{
						var xImportTextArgs = new SetTextEventArgs(ExplorerModes.Edit, file, editorTemp.GetText());
						Log.Source.TraceInformation("ImportText");
						UISetText(xImportTextArgs);
					};
				}
				else
				{
					editorTemp.Closed += delegate //???? to use Saved (Far 3), update docs.
					{
						if (editorTemp.TimeOfSave == DateTime.MinValue)
							return;

						var xImportFileArgs = new SetFileEventArgs(ExplorerModes.Edit, file, temp);
						Log.Source.TraceInformation("ImportFile");
						UISetFile(xImportFileArgs);
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
		/// <remarks>
		/// The default method calls <see cref="FarNet.Explorer.GetContent"/> to get a temporary file to view.
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
			var asExportFileEventArgs = xExportArgs as GetContentEventArgs;
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
		/// Opens the file.
		/// </summary>
		/// <remarks>
		/// It is called for the current file when [Enter] is pressed.
		/// The base method just calls <see cref="FarNet.Explorer.OpenFile"/> if the explorer supports it.
		/// </remarks>
		public virtual void UIOpenFile(FarFile file)
		{
			if (file == null)
				return;

			if (!Explorer.CanOpenFile)
				return;

			var args = new OpenFileEventArgs(file);
			var explorer = UIOpenFile(args);
			if (explorer != null)
				explorer.OpenPanelChild(this);
		}
		/// <summary>
		/// Rename action.
		/// </summary>
		/// <remarks>
		/// It is called for the current item when [ShiftF6] is pressed.
		/// If the explorer supports renaming then the method prompts to input a new name and then calls <see cref="UIRenameFile"/>.
		/// </remarks>
		public void UIRename()
		{
			if (!Explorer.CanRenameFile)
				return;

			var file = CurrentFile;
			if (file == null)
				return;

			// new name
			IInputBox input = Far.Net.CreateInputBox();
			input.Title = "Rename";
			input.Prompt = "New name";
			input.History = "Copy";
			input.Text = file.Name;
			if (!input.Show() || input.Text == file.Name)
				return;

			// call
			var args = new RenameFileEventArgs(ExplorerModes.None, file, input.Text);
			UIRenameFile(args);
			if (args.Result != JobResult.Done)
				return;

			if (args.PostData == null && args.PostFile == null)
				args.PostName = input.Text;

			Update(true);
			Redraw();
		}
		/// <summary>
		/// Tells to update and redraw the panel automatically when idle.
		/// </summary>
		/// <remarks>
		/// If it is set the panel is updated automatically every few seconds when idle.
		/// This is suitable only for panels with very frequently changed data,
		/// otherwise it results in expensive updates for nothing.
		/// </remarks>
		/// <seealso cref="Idled"/>
		public bool IdleUpdate { get; set; }
		/// <summary>
		/// Called periodically when a user is idle.
		/// </summary>
		/// <seealso cref="IdleUpdate"/>
		/// <seealso cref="IdledHandler"/>
		public event EventHandler Idled;
		/// <summary>
		/// Called periodically when idle.
		/// </summary>
		/// <remarks>
		/// It is used for panel updating and redrawing if data have changed.
		/// The base method triggers the <see cref="Idled"/> event.
		/// </remarks>
		public virtual void UIIdle()
		{
			if (Idled != null)
				Idled(this, null);
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
				case VKeyCode.Enter:

					switch (state)
					{
						case KeyStates.None:
							if (RealNames)
								break;

							var file = CurrentFile;
							if (file == null || file.IsDirectory)
								break;

							UIOpenFile(file);
							return true;
					}
					break;

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
							UICopyMove(false);
							return true;
					}
					break;

				case VKeyCode.F6:

					switch (state)
					{
						case KeyStates.None:
							UICopyMove(true);
							return true;

						case KeyStates.Shift: //???? if (RealNames) ?
							//! return true even if the file is dots
							UIRename();
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
