
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

namespace PowerShellFar
{
	/// <summary>
	/// .NET objects explorer.
	/// </summary>
	public sealed class ObjectExplorer : FormatExplorer
	{
		///
		public ObjectExplorer()
			: base(new Guid(Guids.ObjectExplorer))
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
		/// <inheritdoc/>
		public override Panel DoCreatePanel()
		{
			return new ObjectPanel(this);
		}
		/// <inheritdoc/>
		public override void DoAcceptFiles(AcceptFilesEventArgs args)
		{
			AddObjects(args.FilesData);
		}
		/// <inheritdoc/>
		public override void DoDeleteFiles(DeleteFilesEventArgs args)
		{
			//: force delete in interactive mode
			if (args.Force && args.UI)
			{
				// collect known items
				var knownFiles = new List<(FarFile File, string Path)>();
				var knownProcesses = new List<(FarFile File, Process Process)>();
				foreach (FarFile file in args.Files)
				{
					string filePath = My.PathEx.TryGetFilePath(file.Data);
					if (filePath != null)
					{
						knownFiles.Add((file, filePath));
						continue;
					}

					var process = Cast<Process>.From(file.Data);
					if (process != null)
					{
						knownProcesses.Add((file, process));
						continue;
					}

					args.Result = JobResult.Incomplete;
					args.FilesToStay.Add(file);
				}

				if (knownFiles.Count == 0 && knownProcesses.Count == 0)
				{
					args.Result = JobResult.Ignore;
					Far.Api.Message("No known objects to process.");
					return;
				}

				void Done(FarFile file)
				{
					Cache.Remove(file);
				}

				void Skip(FarFile file)
				{
					args.Result = JobResult.Incomplete;
					args.FilesToStay.Add(file);
				}

				AboutPanel.DeleteKnownFiles(knownFiles, Done, Skip);
				AboutPanel.StopKnownProcesses(knownProcesses, Done, Skip);
			}
			//: normal delete or non interactive
			else
			{
				//: interactive, confirm
				if (args.UI && 0 != (long)Far.Api.GetSetting(FarSetting.Confirmations, "Delete"))
				{
					int choice = Far.Api.Message(
						"Remove object(s)?",
						Res.Remove,
						MessageOptions.None,
						new string[] { Res.Remove, Res.Cancel });

					if (choice != 0)
					{
						args.Result = JobResult.Ignore;
						return;
					}
				}

				// remove objects from the panel
				foreach (FarFile file in args.Files)
					Cache.Remove(file);
			}
		}
		/// <inheritdoc/>
		public override void DoGetContent(GetContentEventArgs args)
		{
			var data = args.File.Data;

			// use existing file
			string filePath = My.PathEx.TryGetFilePath(data);
			if (filePath != null)
			{
				args.UseFileName = filePath;
				args.CanSet = true;
				return;
			}

			// MatchInfo of Select-String
			var obj = PS2.BaseObject(data);
			if (obj.GetType().FullName == Res.MatchInfoTypeName)
			{
				var dynamo = (dynamic)obj;
				filePath = (string)dynamo.Path;
				args.UseFileName = filePath;

				var lineIndex = (int)dynamo.LineNumber - 1;
				var match = ((Match[])dynamo.Matches)[0];
				args.EditorOpened = (sender, e) =>
				{
					var editor = (IEditor)sender;
					var frame = new TextFrame
					{
						VisibleLine = Math.Max(lineIndex - Far.Api.UI.WindowSize.Y / 3, 0),
						CaretLine = lineIndex
					};
					editor.Frame = frame;
					var line = editor.Line;
					line.Caret = match.Index + match.Length;
					line.SelectText(match.Index, match.Index + match.Length);
					editor.Redraw();
				};
				return;
			}

			// text
			args.UseText = A.InvokeFormatList(args.File.Data, true);
		}
		/// <inheritdoc/>
		public override void DoImportFiles(ImportFilesEventArgs args)
		{
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
		/// <para>
		/// Returned objects are converted to files and cached internally.
		/// Scripts may reuse these data and return the <c>Cache</c> as <c>, $args[0].Cache</c>.
		/// </para>
		/// </remarks>
		/// <example>Panel-Job-.ps1, Panel-Process-.ps1</example>
		public ScriptBlock AsGetData { get; set; }
		internal override object GetData(GetFilesEventArgs args)
		{
			// custom script
			if (AsGetData != null)
			{
				// call
				var result = AsGetData.Invoke(this, args);

				// discover and get the cache or get other objects as they are
				if (result.Count == 1 && result[0].BaseObject == Cache)
					return Cache;
				else
					return result;
			}

			var Files = Cache;
			try
			{
				//???? it works but smells
				if (!args.NewFiles && _AddedValues == null && (Map != null || Files.Count > 0 && Files[0] is SetFile))
					return Files;

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
			get { return _AddedValues ??= new Collection<PSObject>(); }
		}
		/// <inheritdoc/>
		public override Explorer DoOpenFile(OpenFileEventArgs args)
		{
			object data = args.File.Data;

			// open file-like
			{
				string filePath = My.PathEx.TryGetFilePath(data);
				if (filePath != null)
				{
					Process.Start(filePath);
					return null;
				}
			}

			// open directory-like
			{
				string directoryPath = My.PathEx.TryGetDirectoryPath(data);
				if (directoryPath != null)
				{
					Far.Api.Panel2.CurrentDirectory = directoryPath;
					return null;
				}
			}

			PSObject psData = PSObject.AsPSObject(data);
			var type = psData.BaseObject.GetType();

			// replace dictionary entry with its value if it is complex
			if (type == typeof(DictionaryEntry))
			{
				var value = ((DictionaryEntry)psData.BaseObject).Value;
				if (value != null && !Converter.IsLinearType(value.GetType()))
				{
					data = value;
					psData = PSObject.AsPSObject(value);
				}
			}

			// replace key/value pair with its value if it is complex
			var typeName = type.FullName;
			if (typeName.StartsWith("System.Collections.Generic.KeyValuePair`", StringComparison.OrdinalIgnoreCase))
			{
				var value = psData.Properties["Value"].Value;
				if (value != null && !Converter.IsLinearType(value.GetType()))
				{
					data = value;
					psData = PSObject.AsPSObject(value);
				}
			}

			// case: linear type: ignore, it is useless to open
			if (Converter.IsLinearType(type))
			{
				args.Result = JobResult.Ignore;
				return null;
			}

			// case: enumerable (string is excluded by linear type case)
			IEnumerable asIEnumerable = Cast<IEnumerable>.From(data);
			if (asIEnumerable != null)
			{
				var explorer = new ObjectExplorer();
				explorer.AddObjects(asIEnumerable);
				return explorer;
			}

			// case: group
			PSPropertyInfo pi = psData.Properties["Group"];
			if (pi != null && pi.Value is IEnumerable && pi.Value is not string)
			{
				var explorer = new ObjectExplorer();
				explorer.AddObjects(pi.Value);
				return explorer;
			}

			// case: WMI
			if (typeName == "System.Management.ManagementClass")
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
			return new MemberExplorer(data);
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
			string message = enumerable is ICollection collection ?
				$"There are {collection.Count} panel files, the limit is {maximumFileCount}." :
				$"There are more than {maximumFileCount} panel files.";

			return Far.Api.Message(message, "$Psf.Settings.MaximumPanelFileCount", MessageOptions.AbortRetryIgnore);
		}
		/// <inheritdoc/>
		public override void DoCreateFile(CreateFileEventArgs args)
		{
			args.Result = JobResult.Ignore;

			// prompt for a command
			string code = Far.Api.MacroState == MacroState.None ? A.Psf.InputCode() : Far.Api.Input(null);
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
