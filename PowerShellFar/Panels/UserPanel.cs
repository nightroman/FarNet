/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Panel exploring any user objects.
	/// </summary>
	public sealed class UserPanel : ObjectPanel
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public UserPanel()
		{
			Panel.Info.Title = string.Empty;
		}

		internal override bool CanClose()
		{
			return true;
		}

		#region Delete
		ScriptBlock _Delete;

		/// <summary>
		/// Handler to delete files. $_ = <see cref="FilesEventArgs"/>
		/// </summary>
		public void SetDelete(ScriptBlock handler)
		{
			_Delete = handler;
		}

		internal override void DeleteFiles(IList<FarFile> files, bool shift)
		{
			//! do not call base which removes items
			if (_Delete != null)
				InvokeScriptReturnAsIs(_Delete, new FilesEventArgs(files, OperationModes.None, shift));
		}

		#endregion

		#region Write
		ScriptBlock _Write;

		/// <summary>
		/// Handler to write an object data to a file. $_ = <see cref="WriteEventArgs"/>
		/// </summary>
		public void SetWrite(ScriptBlock handler)
		{
			_Write = handler;
		}

		internal override void WriteFile(FarFile file, string path)
		{
			if (_Write == null)
				base.WriteFile(file, path);
			else
				InvokeScriptReturnAsIs(_Write, new WriteEventArgs(file, path));
		}

		#endregion

		#region GetData
		ScriptBlock _GetFiles;
		ScriptBlock _GetObjects;

		/// <summary>
		/// Handler called to update the panel files.
		/// </summary>
		/// <remarks>
		/// This handler should work on <see cref="IPluginPanel.Files"/> (recreate completely or update existing).
		/// <para>
		/// One possible scenario is to clear the file list and then call <see cref="ObjectPanel.AddObjects"/> one or more times.
		/// But <see cref="SetGetObjects"/> method is usually more convenient for this.
		/// </para>
		/// <para>
		/// In another scenario new files may be created and added to the list.
		/// In this case you should create files by <see cref="ObjectPanel.NewFile"/> method.
		/// </para>
		/// <para>
		/// Normally this handler should not be used together with custom <see cref="FormatPanel.Columns"/>
		/// because it operates on files directly and performs object to file mapping itself.
		/// </para>
		/// </remarks>
		/// <example>Panel-Process-.ps1</example>
		public void SetGetFiles(ScriptBlock handler)
		{
			_GetFiles = handler;
		}

		/// <summary>
		/// Handler called to get all the objects (not files).
		/// </summary>
		/// <remarks>
		/// This handler simply returns all the objects to be shown in the panel.
		/// It should not operate directly on existing or new panel files, it is done internally.
		/// <para>
		/// Normally this handler is used together with custom <see cref="FormatPanel.Columns"/>
		/// otherwise default data formatting will not always be suitable or even possible.
		/// </para>
		/// </remarks>
		/// <example>Panel-Job-.ps1</example>
		public void SetGetObjects(ScriptBlock handler)
		{
			_GetObjects = handler;
		}

		internal override bool OnGettingData()
		{
			// case: custom data update
			if (_GetFiles != null)
			{
				InvokeScript(_GetFiles, null);
				return Map != null;
			}

			// case: custom new objects
			if (_GetObjects != null)
			{
				Collection<PSObject> result = InvokeScript(_GetObjects, null);
				Panel.Files.Clear();
				AddObjects(result);
				return Map != null;
			}

			// case: base
			return base.OnGettingData();
		}

		#endregion
	}

	/// <summary>
	/// Panel file event arguments.
	/// </summary>
	public class FileEventArgs : EventArgs
	{
		///
		public FileEventArgs(FarFile file)
		{
			_File = file;
		}

		///
		public FileEventArgs(FarFile file, bool alternative)
		{
			_File = file;
			_Alternative = alternative;
		}

		FarFile _File;
		/// <summary>
		/// Panel file instance being processed.
		/// </summary>
		public FarFile File
		{
			get { return _File; }
		}

		bool _Alternative;
		/// <summary>
		/// Alternative action flag.
		/// </summary>
		public bool Alternative
		{
			get { return _Alternative; }
			set { _Alternative = value; }
		}
	}

	/// <summary>
	/// Arguments of Write event.
	/// </summary>
	public class WriteEventArgs : FileEventArgs
	{
		///
		public WriteEventArgs(FarFile file, string path)
			: base(file)
		{
			_path = path;
		}

		string _path;
		/// <summary>
		/// File system path where data are written to.
		/// </summary>
		public string Path
		{
			get { return _path; }
		}
	}

}
