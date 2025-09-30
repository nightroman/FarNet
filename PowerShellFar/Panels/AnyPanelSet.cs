using FarNet;
using System.Management.Automation;

namespace PowerShellFar;

public partial class AnyPanel
{
	#region OpenFile
	/// <summary>
	/// Gets or sets the script to open a file (e.g. on [Enter]).
	/// Arguments: 0: this panel, 1: <see cref="OpenFileEventArgs"/>.
	/// </summary>
	public ScriptBlock? AsOpenFile { get; set; }

	/// <summary>
	/// Opens the file using <see cref="AsOpenFile"/> or the default method.
	/// </summary>
	/// <param name="file">The file to be opened.</param>
	public sealed override void UIOpenFile(FarFile file)
	{
		ArgumentNullException.ThrowIfNull(file);

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
			AsOpenFile.InvokeReturnAsIs(this, new OpenFileEventArgs(file));
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
	/// <param name="file">The file to open.</param>
	/// <remarks>
	/// The base method calls Invoke-Item for <see cref="FileSystemInfo"/> files.
	/// </remarks>
	public virtual void OpenFile(FarFile file)
	{
		ArgumentNullException.ThrowIfNull(file);

		if (file.Data is null)
			return;

		//! use try, e.g. Invoke-Item throws exception with any error action (PS bug?)
		try
		{
			// case: file system
			var fi = Cast<FileSystemInfo>.From(file.Data);
			if (fi != null)
			{
				A.InvokeCode("Invoke-Item -LiteralPath $args[0] -ErrorAction Stop", fi.FullName);
				return;
			}
		}
		catch (RuntimeException ex)
		{
			A.MyError(ex);
		}
	}
	#endregion

	#region EditFile
	/// <summary>
	/// <see cref="UIEditFile"/> worker.
	/// Arguments: 0: this panel, 1: <see cref="FarFile"/>.
	/// </summary>
	public ScriptBlock? AsEditFile { get; set; }

	/// <summary>
	/// <see cref="UIEditFile"/> worker.
	/// </summary>
	/// <param name="file">The file to edit.</param>
	public void DoEditFile(FarFile file) => base.UIEditFile(file);

	/// <include file='doc.xml' path='doc/ScriptFork/*'/>
	/// <param name="file">The file to edit.</param>
	public sealed override void UIEditFile(FarFile file) //_091202_073429 NB: Data can be wrapped by PSObject.
	{
		if (AsEditFile != null)
			AsEditFile.InvokeReturnAsIs(this, file);
		else
			DoEditFile(file);
	}
	#endregion

	#region ViewFile
	/// <summary>
	/// <see cref="UIViewFile"/> worker.
	/// Arguments: 0: this panel, 1: <see cref="FarFile"/>.
	/// </summary>
	public ScriptBlock? AsViewFile { get; set; }

	/// <summary>
	/// <see cref="UIViewFile"/> worker.
	/// </summary>
	/// <param name="file">The file to view.</param>
	public void DoViewFile(FarFile file) => base.UIViewFile(file);

	/// <include file='doc.xml' path='doc/ScriptFork/*'/>
	/// <param name="file">The file to view.</param>
	public sealed override void UIViewFile(FarFile file) //_091202_073429
	{
		if (AsViewFile != null)
			AsViewFile.InvokeReturnAsIs(this, file);
		else
			DoViewFile(file);
	}
	#endregion

	#region ViewAll
	/// <summary>
	/// Gets or sets the script to show all files information (e.g. on [F3] on the dots).
	/// Arguments: 0: this panel.
	/// </summary>
	public ScriptBlock? AsViewAll { get; set; }

	/// <summary>
	/// Shows all files information using <see cref="AsViewAll"/> or the default method.
	/// </summary>
	void UIViewAll()
	{
		if (AsViewAll != null)
		{
			AsViewAll.InvokeReturnAsIs(this, null);
			return;
		}

		string tmp = Far.Api.TempName();
		try
		{
			A.InvokeCode("$args[0] | Format-Table -AutoSize -ea 0 | Out-File -FilePath $args[1]", ShownItems, tmp);

			var v = Far.Api.CreateViewer();
			v.FileName = tmp;
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
