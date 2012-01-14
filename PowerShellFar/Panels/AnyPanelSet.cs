
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
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
	public partial class AnyPanel
	{
		#region OpenFile
		/// <summary>
		/// Gets or sets the script to open a file (e.g. on [Enter]).
		/// Arguments: 0: this panel, 1: <see cref="OpenFileEventArgs"/>.
		/// </summary>
		public ScriptBlock AsOpenFile { get; set; }
		/// <summary>
		/// Opens the file using <see cref="AsOpenFile"/> or the default method.
		/// </summary>
		public sealed override void UIOpenFile(FarFile file)
		{
			if (file == null)
				return;

			// lookup closer?
			if (UserWants == UserAction.Enter && Lookup != null)
			{
				Lookup.Invoke(this, new OpenFileEventArgs(file));
				UIEscape(false);
				return;
			}

			// script
			if (AsOpenFile != null)
			{
				A.InvokeScriptReturnAsIs(AsOpenFile, this, new OpenFileEventArgs(file));
				return;
			}

			// base
			if (Explorer.CanOpenFile)
			{
				base.UIOpenFile(file);
				return;
			}

			// PSF
			OpenFile(file);
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
					A.InvokeCode("Invoke-Item -LiteralPath $args[0] -ErrorAction Stop", fi.FullName);
					return;
				}
			}
			catch (RuntimeException ex)
			{
				A.Msg(ex);
			}
		}
		#endregion
		#region EditFile
		/// <summary>
		/// <see cref="UIEditFile"/> worker.
		/// Arguments: 0: this panel, 1: <see cref="FarFile"/>.
		/// </summary>
		public ScriptBlock AsEditFile { get; set; }
		/// <summary>
		/// <see cref="UIEditFile"/> worker.
		/// </summary>
		public void DoEditFile(FarFile file) { base.UIEditFile(file); }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void UIEditFile(FarFile file) //_091202_073429 NB: Data can be wrapped by PSObject.
		{
			if (AsEditFile != null)
				A.InvokeScriptReturnAsIs(AsEditFile, this, file);
			else
				DoEditFile(file);
		}
		#endregion
		#region ViewFile
		/// <summary>
		/// <see cref="UIViewFile"/> worker.
		/// Arguments: 0: this panel, 1: <see cref="FarFile"/>.
		/// </summary>
		public ScriptBlock AsViewFile { get; set; }
		/// <summary>
		/// <see cref="UIViewFile"/> worker.
		/// </summary>
		public void DoViewFile(FarFile file) { base.UIViewFile(file); }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void UIViewFile(FarFile file) //_091202_073429
		{
			if (AsViewFile != null)
				A.InvokeScriptReturnAsIs(AsViewFile, this, file);
			else
				DoViewFile(file);
		}
		#endregion
		#region ViewAll
		/// <summary>
		/// Gets or sets the script to show all files information (e.g. on [F3] on the dots).
		/// Arguments: 0: this panel.
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
				A.InvokeCode("$args[0] | Format-Table -AutoSize -ea 0 | Out-File -FilePath $args[1]", ShownItems, tmp);

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
