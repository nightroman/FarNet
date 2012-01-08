
/*
PowerShellFar module for Far Manager
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
	/// .NET objects explorer.
	/// </summary>
	public sealed class ObjectExplorer : FormatExplorer
	{
		const string TypeIdString = "07e4dde7-e113-4622-b2e9-81cf3cda927a";
		///
		public ObjectExplorer()
			: base(new Guid(TypeIdString))
		{
			FileComparer = new FileDataComparer();
			Functions =
				ExplorerFunctions.AcceptFiles |
				ExplorerFunctions.ImportFiles |
				ExplorerFunctions.DeleteFiles |
				ExplorerFunctions.CreateFile |
				ExplorerFunctions.GetContent |
				ExplorerFunctions.OpenFile;
		}
		///
		public override Panel DoCreatePanel()
		{
			return new ObjectPanel(this);
		}
		///
		public override void DoAcceptFiles(AcceptFilesEventArgs args)
		{
			if (args == null) return;

			AddObjects(args.FilesData);
		}
		///
		public override void DoDeleteFiles(DeleteFilesEventArgs args)
		{
			if (args == null) return;

			if (args.UI && (Far.Net.Confirmations & FarConfirmations.Delete) != 0)
			{
				if (Far.Net.Message("Remove object(s) from the panel?", Res.Remove, MessageOptions.None, new string[] { Res.Remove, Res.Cancel }) != 0)
				{
					args.Result = JobResult.Ignore;
					return;
				}
			}

			foreach (FarFile file in args.Files)
				Cache.Remove(file);
		}
		///
		public override void DoGetContent(GetContentEventArgs args)
		{
			if (args == null) return;

			// use existing file
			string filePath = My.PathEx.TryGetFilePath(args.File.Data);
			if (filePath != null)
			{
				args.UseFileName = filePath;
				args.CanSet = true;
				return;
			}

			// text
			args.UseText = A.InvokeFormatList(args.File.Data, true);
		}
		///
		public override void DoImportFiles(ImportFilesEventArgs args)
		{
			if (args == null) return;

			//! Assume this is the passive panel, so call the active
			AddObjects(A.InvokeCode("Get-FarItem -Selected")); //????? crap. but...
		}
		/// <summary>
		/// Gets or sets the script getting raw file data objects.
		/// Arguments: 0: this explorer, 1: <see cref="ExplorerEventArgs"/>.
		/// </summary>
		/// <remarks>
		/// The script returns raw data to be represented as files with the data attached.
		/// It should not operate directly on existing or new files, it is done internally.
		/// <para>
		/// Normally it is used together with custom columns
		/// otherwise default formatting is not always suitable.
		/// </para>
		/// </remarks>
		/// <example>Panel-Job-.ps1, Panel-Process-.ps1</example>
		public ScriptBlock AsGetData { get; set; }
		internal override object GetData(ExplorerEventArgs args)
		{
			if (AsGetData != null)
				return A.InvokeScript(AsGetData, this, args);

			var panel = args.Parameter as ObjectPanel;
			var Files = Cache;
			try
			{
				if (panel != null)
				{
					//???? it works but looks like a hack
					if (panel.UserWants != UserAction.CtrlR && _AddedValues == null && (Map != null || Files.Count > 0 && Files[0] is SetFile))
						return Files;
				}

				if (Map == null || Columns == null)
				{
					if (Files.Count == 0)
						return _AddedValues ?? new Collection<PSObject>();

					var result = new Collection<PSObject>();
					foreach (FarFile file in Files)
						result.Add(PSObject.AsPSObject(file.Data));
					if (_AddedValues != null)
						foreach (PSObject value in _AddedValues)
							result.Add(value);

					return result;
				}

				// _100330_191639
				if (_AddedValues == null)
					return Files;

				var map = Map;
				var files = new List<FarFile>(_AddedValues.Count);
				foreach (PSObject value in _AddedValues)
					files.Add(new MapFile(value, map));

				return files;
			}
			finally
			{
				_AddedValues = null;
			}
		}
		Collection<PSObject> _AddedValues;
		internal Collection<PSObject> AddedValues
		{
			get { return _AddedValues ?? (_AddedValues = new Collection<PSObject>()); }
		}
		///
		public override Explorer DoOpenFile(OpenFileEventArgs args)
		{
			if (args == null) return null;

			PSObject psData = PSObject.AsPSObject(args.File.Data);

			// case: linear type: ignore, it is useless to open
			if (Converter.IsLinearType(psData.BaseObject.GetType()))
			{
				args.Result = JobResult.Ignore;
				return null;
			}

			// case: enumerable (string is excluded by linear type case)
			IEnumerable asIEnumerable = Cast<IEnumerable>.From(args.File.Data);
			if (asIEnumerable != null)
			{
				var explorer = new ObjectExplorer();
				explorer.AddObjects(asIEnumerable);
				return explorer;
			}

			// case: group
			PSPropertyInfo pi = psData.Properties["Group"];
			if (pi != null && pi.Value is IEnumerable && !(pi.Value is string))
			{
				var explorer = new ObjectExplorer();
				explorer.AddObjects(pi.Value);
				return explorer; 
			}

			// case: WMI
			if (psData.BaseObject.GetType().FullName == "System.Management.ManagementClass")
			{
				pi = psData.Properties[Word.Name];
				if (pi != null && pi.Value != null)
				{
					var values = A.InvokeCode("Get-WmiObject -Class $args[0] -ErrorAction SilentlyContinue", pi.Value.ToString());
					var explorer = new ObjectExplorer();
					explorer.AddObjects(values);
					return explorer;
				}
			}

			// open members
			return new MemberExplorer(args.File.Data);
		}
		internal void AddObjects(object values)
		{
			if (values == null)
				return;

			var added = AddedValues;

			IEnumerable enumerable = Cast<IEnumerable>.From(values);
			if (enumerable == null || enumerable is string)
			{
				added.Add(PSObject.AsPSObject(values));
			}
			else
			{
				int maximumFileCount = Settings.Default.MaximumPanelFileCount;
				int fileCount = 0;
				foreach (object value in enumerable)
				{
					if (value == null)
						continue;

					// ask to cancel
					if (fileCount >= maximumFileCount && maximumFileCount > 0)
					{
						int res = ShowTooManyFiles(maximumFileCount, enumerable);

						// abort, show what we have got
						if (res == 0)
							break;

						if (res == 1)
							// retry with a larger number
							maximumFileCount *= 2;
						else
							// ignore the limit
							maximumFileCount = 0;
					}

					// add
					added.Add(PSObject.AsPSObject(value));
					++fileCount;
				}
			}
		}
		static int ShowTooManyFiles(int maximumFileCount, IEnumerable enumerable)
		{
			ICollection collection = enumerable as ICollection;
			string message = collection == null ?
				string.Format(null, "There are more than {0} panel files.", maximumFileCount) :
				string.Format(null, "There are {0} panel files, the limit is {1}.", collection.Count, maximumFileCount);

			return Far.Net.Message(message, "$Psf.Settings.MaximumPanelFileCount", MessageOptions.AbortRetryIgnore);
		}
		///
		public override void DoCreateFile(CreateFileEventArgs args)
		{
			if (args == null) return;

			args.Result = JobResult.Ignore;

			// prompt for a command
			string code = Far.Net.MacroState == MacroState.None ? A.Psf.InputCode() : Far.Net.Input(null);
			if (string.IsNullOrEmpty(code))
				return;

			// invoke the command
			Collection<PSObject> values = A.InvokeCode(code);
			if (values.Count == 0)
				return;

			// add the objects
			AddObjects(values);

			// done, post the first object
			args.PostData = values[0];
			args.Result = JobResult.Done;
		}
	}
}
