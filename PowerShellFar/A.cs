/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using FarNet;

namespace PowerShellFar
{
	static class A
	{
		/// <summary>PowerShellFar actor.</summary>
		public static Actor Psf { get { return _Psf_; } }
		static Actor _Psf_;

		public static void Connect(Actor psf)
		{
			_Psf_ = psf;
		}

		/// <summary>
		/// Shows an error.
		/// </summary>
		public static void Msg(Exception error)
		{
			Far.Net.Message(error.Message, "PowerShellFar error", MsgOptions.LeftAligned);
		}

		/// <summary>
		/// Shows a message.
		/// </summary>
		public static void Msg(string message)
		{
			Far.Net.Message(message, Res.Me, MsgOptions.LeftAligned);
		}

		/// <summary>
		/// Creates standard Far editor ready for opening (F4)
		/// </summary>
		/// <param name="filePath">Existing file to edit.</param>
		public static IEditor CreateEditor(string filePath)
		{
			IEditor edit = Far.Net.CreateEditor();
			edit.FileName = filePath;
			return edit;
		}

		/// <summary>
		/// Creates standard Far viewer ready for opening (F3)
		/// </summary>
		/// <param name="filePath">Existing file to view.</param>
		public static IViewer CreateViewer(string filePath)
		{
			IViewer view = Far.Net.CreateViewer();
			view.FileName = filePath;
			return view;
		}

		/// <summary>
		/// Sets an object member value.
		/// </summary>
		public static void SetMemberValue(PSPropertyInfo info, object value)
		{
			//! SetValueInvocationException (RuntimeException is its ancestor)
			info.Value = value;
		}

		/// <summary>
		/// Sets an item property value as it is.
		/// </summary>
		public static void SetPropertyValue(string itemPath, string propertyName, object value)
		{
			//! setting PSPropertyInfo.Value is not working, so do use Set-ItemProperty
			Psf.InvokeCode(
				"Set-ItemProperty -LiteralPath $args[0] -Name $args[1] -Value $args[2] -ErrorAction Stop",
				itemPath, propertyName, value);
		}

		/// <summary>
		/// Tries to get a property value.
		/// </summary>
		public static bool TryGetPropertyValue<T>(PSObject target, string name, out T value)
		{
			value = default(T);

			PSPropertyInfo pi = target.Properties[name];
			if (pi == null)
				return false;

			return LanguagePrimitives.TryConvertTo<T>(pi.Value, out value);
		}

		// Sets location (with workaround)
		public static void SetLocation(string path)
		{
			Psf.Engine.SessionState.Path.SetLocation(Kit.EscapeWildcard(path));
		}

		/// <summary>
		/// Writes invocation errors.
		/// </summary>
		public static void WriteErrors(TextWriter writer, IEnumerable errors)
		{
			if (errors == null)
				return;

			foreach (object error in errors)
			{
				writer.WriteLine("ERROR:");
				writer.WriteLine(error.ToString());

				ErrorRecord asErrorRecord = Cast<ErrorRecord>.From(error);
				if (asErrorRecord != null && asErrorRecord.InvocationInfo != null)
					writer.WriteLine(Kit.PositionMessage(asErrorRecord.InvocationInfo.PositionMessage));
			}
		}

		/// <summary>
		/// Writes invocation exception.
		/// </summary>
		public static void WriteException(TextWriter writer, Exception ex)
		{
			if (ex == null)
				return;

			writer.WriteLine("ERROR:");
			writer.WriteLine(ex.GetType().Name + ":");
			writer.WriteLine(ex.Message);

			RuntimeException asRuntimeException = ex as RuntimeException;
			if (asRuntimeException != null && asRuntimeException.ErrorRecord != null && asRuntimeException.ErrorRecord.InvocationInfo != null)
				writer.WriteLine(Kit.PositionMessage(asRuntimeException.ErrorRecord.InvocationInfo.PositionMessage));
		}

		/// <summary>
		/// Sets an item content, shows errors.
		/// </summary>
		public static bool SetContentUI(string itemPath, string text)
		{
			//! PSF only, can do in console:
			//! as a script: if param name -Value is used, can't set e.g. Alias:\%
			//! as a block: same problem
			//! so, use a command
			using (PowerShell p = Psf.CreatePipeline())
			{
				Command command = new Command("Set-Content");
				command.Parameters.Add("LiteralPath", itemPath);
				command.Parameters.Add("Value", text);
				command.Parameters.Add(Prm.Force);
				command.Parameters.Add(Prm.ErrorAction, ActionPreference.Continue);
				p.Commands.AddCommand(command);
				p.Invoke();
				if (ShowError(p))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Outputs to the default formatter.
		/// </summary>
		/// <param name="ps">Pipeline.</param>
		/// <param name="input">Input objects.</param>
		public static void Out(PowerShell ps, IEnumerable input)
		{
			ps.Commands.AddCommand(A.OutCommand);
			ps.Invoke(input);
		}

		/// <summary>
		/// Outputs an exception or its error record.
		/// </summary>
		/// <param name="ps">Pipeline.</param>
		/// <param name="ex">Exception.</param>
		public static void OutReason(PowerShell ps, Exception ex)
		{
			RuntimeException asRuntimeException = ex as RuntimeException;
			object error;
			if (asRuntimeException == null)
				error = ex;
			else
				error = asRuntimeException.ErrorRecord;

			Out(ps, new object[] { "ERROR: " + ex.GetType().Name + ":", error });
		}

		/// <summary>
		/// Shows errors, if any, in a message box and returns true, else just returns false.
		/// </summary>
		public static bool ShowError(PowerShell ps)
		{
			if (ps.Streams.Error.Count == 0)
				return false;

			StringBuilder sb = new StringBuilder();
			foreach (object o in ps.Streams.Error)
				sb.AppendLine(o.ToString());

			Far.Net.Message(sb.ToString(), "PowerShellFar error(s)", MsgOptions.LeftAligned);
			return true;
		}

		/// <summary>
		/// Sets current directory, shows errors but does not throw.
		/// </summary>
		/// <param name="currentDirectory">null or a path.</param>
		public static void SetCurrentDirectoryFinally(string currentDirectory)
		{
			if (currentDirectory != null)
			{
				try
				{
					Directory.SetCurrentDirectory(currentDirectory);
				}
				catch (IOException ex)
				{
					Far.Net.ShowError(Res.Me, ex);
				}
			}
		}

		static Command _OutCommand;
		/// <summary>
		/// Command for formatted output of everything.
		/// </summary>
		/// <remarks>
		/// "Out-Default" is not suitable for external apps, output goes to console.
		/// </remarks>
		public static Command OutCommand
		{
			get
			{
				if (_OutCommand == null)
				{
					_OutCommand = new Command("Microsoft.PowerShell.Utility\\Out-Host");
					_OutCommand.MergeUnclaimedPreviousCommandResults = PipelineResultTypes.Output | PipelineResultTypes.Error;
				}
				return _OutCommand;
			}
		}

		/// <summary>
		/// Finds heuristically a property to be used to display the object.
		/// </summary>
		public static PSPropertyInfo FindDisplayProperty(PSObject value)
		{
			//! Microsoft.PowerShell.Commands.Internal.Format.PSObjectHelper.GetDisplayNameExpression()

			PSPropertyInfo pi;
			pi = value.Properties[Word.Name];
			if (pi != null)
				return pi;
			pi = value.Properties[Word.Id];
			if (pi != null)
				return pi;
			pi = value.Properties[Word.Key];
			if (pi != null)
				return pi;

			ReadOnlyPSMemberInfoCollection<PSPropertyInfo> ppi;
			ppi = value.Properties.Match("*" + Word.Key);
			if (ppi.Count > 0)
				return ppi[0];
			ppi = value.Properties.Match("*" + Word.Name);
			if (ppi.Count > 0)
				return ppi[0];
			ppi = value.Properties.Match("*" + Word.Id);
			if (ppi.Count > 0)
				return ppi[0];

			return null;
		}

		public static Type FindCommonType(Collection<PSObject> values)
		{
			Type result = null;
			object sample = null;
			bool baseMode = false;

			foreach (PSObject value in values)
			{
				// get the sample
				if (sample == null)
				{
					sample = value.BaseObject;
					result = sample.GetType();
				}
				// base mode: compare base types
				else if (baseMode)
				{
					if (GetCommonBaseType(value.BaseObject) != result)
						return null;
				}
				// mono mode: compare directly; works for mono sets
				else if (value.BaseObject.GetType() != result)
				{
					// compare base types
					result = GetCommonBaseType(sample);
					if (GetCommonBaseType(value.BaseObject) != result)
						return null;

					// turn on
					baseMode = true;
				}
			}

			return result;
		}

		/// <summary>
		/// Gets heuristic base type suitable to be common for mixed sets.
		/// </summary>
		/// <remarks>
		/// This method should be consistent with redirection in <see cref="FindTableControl"/>.
		/// </remarks>
		static Type GetCommonBaseType(object value)
		{
			if (value is System.IO.FileSystemInfo) return typeof(System.IO.FileSystemInfo);
			if (value is CommandInfo) return typeof(CommandInfo);
			if (value is Breakpoint) return typeof(Breakpoint);
			if (value is PSVariable) return typeof(PSVariable);
			return value.GetType();
		}

		/// <summary>
		/// Finds an available table control.
		/// </summary>
		/// <param name="typeName">The type name. To be redirected for some types.</param>
		/// <param name="tableName">Optional table name to find.</param>
		/// <returns>Found table control or null.</returns>
		/// <remarks>
		/// Type name redirection should be consistent with <see cref="GetCommonBaseType"/>.
		/// </remarks>
		public static TableControl FindTableControl(string typeName, string tableName)
		{
			// process\redirect special types
			switch (typeName)
			{
				case "System.Management.Automation.PSCustomObject": return null;

				case "System.IO.FileSystemInfo": typeName = "FileSystemTypes"; break;
				case "System.IO.DirectoryInfo": typeName = "FileSystemTypes"; break;
				case "System.IO.FileInfo": typeName = "FileSystemTypes"; break;

				case "System.Management.Automation.Breakpoint": typeName = "BreakpointTypes"; break;
				case "System.Management.Automation.LineBreakpoint": typeName = "BreakpointTypes"; break;
				case "System.Management.Automation.CommandBreakpoint": typeName = "BreakpointTypes"; break;
				case "System.Management.Automation.VariableBreakpoint": typeName = "BreakpointTypes"; break;
			}

			// extended type definitions:
			foreach (PSObject it in Psf.InvokeCode("Get-FormatData -TypeName $args[0]", typeName))
			{
				ExtendedTypeDefinition typeDef = it.BaseObject as ExtendedTypeDefinition;
				foreach (FormatViewDefinition viewDef in typeDef.FormatViewDefinition)
				{
					if (string.IsNullOrEmpty(tableName) || Kit.Compare(tableName, viewDef.Name) == 0)
					{
						TableControl table = viewDef.Control as TableControl;
						if (table != null)
							return table;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Robust Get-ChildItem.
		/// </summary>
		public static Collection<PSObject> GetChildItems(string literalPath)
		{
			//! If InvokeProvider.ChildItem.Get() fails (e.g. hklm:) then we get nothing at all.
			//! Get-ChildItem gets some items even on errors, that is much better than nothing.
			//! NB: exceptions on getting data: FarNet returns false and Far closes the panel.

			try
			{
				return A.Psf.Engine.InvokeProvider.ChildItem.Get(new string[] { literalPath }, false, true, true);
			}
			catch (RuntimeException)
			{ }

			try
			{
				return A.Psf.InvokeCode("Get-ChildItem -LiteralPath $args[0] -Force -ErrorAction 0", literalPath);
			}
			catch (RuntimeException)
			{ }

			return new Collection<PSObject>();
		}

		/// <summary>
		/// Gets the $FormatEnumerationLimit if it is sane or 4.
		/// </summary>
		public static int FormatEnumerationLimit
		{
			get
			{
				object value = Psf.Engine.SessionState.PSVariable.GetValue("FormatEnumerationLimit") ?? 4;
				if (value.GetType() != typeof(int))
					return 4;
				else
					return (int)value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PowerShellFar")]
		public static void InvokePipelineForEach(IEnumerable<PSObject> input)
		{
			List<PSObject> items = new List<PSObject>(input);
			if (items.Count == 0)
				return;

			// keep the _
			PSVariable variable = A.Psf.Engine.SessionState.PSVariable.Get("_");

			// set the _ to a sample for TabExpansion
			A.Psf.Engine.SessionState.PSVariable.Set("_", items[0]);

			try
			{
				// show the dialog, get the code
				UI.InputDialog ui = new UI.InputDialog(Res.Me, Res.HistoryApply, "For each $_ in " + items.Count + " selected:");
				ui.UICode.IsPath = true;
				ui.UICode.UseLastHistory = true;
				if (!ui.UIDialog.Show())
					return;
				string code = ui.UICode.Text.Trim();
				if (code.Length == 0)
					return;

				// invoke the pipeline using the input
				A.Psf.Engine.SessionState.PSVariable.Set("_", items);
				A.Psf.Act("$_ | .{process{ " + code + " }}", null, false);
			}
			finally
			{
				// restore the _
				if (variable == null)
					A.Psf.Engine.SessionState.PSVariable.Remove("_");
				else
					A.Psf.Engine.SessionState.PSVariable.Set(variable);
			}
		}

	}
}
