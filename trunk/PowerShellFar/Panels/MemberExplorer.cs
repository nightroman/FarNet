
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Management.Automation;
using System.Text.RegularExpressions;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Explorer of an object members (properties by default or all members).
	/// </summary>
	public sealed class MemberExplorer : Explorer
	{
		const string TypeIdString = "be8984d6-fc4d-466d-9e5b-dede881f2232";
		/// <summary>
		/// Regular expression pattern of members to be excluded.
		/// </summary>
		public string ExcludeMemberPattern
		{
			get { return _ExcludeMemberRegex == null ? null : _ExcludeMemberRegex.ToString(); }
			set { _ExcludeMemberRegex = new Regex(value, RegexOptions.IgnoreCase); }
		}
		Regex _ExcludeMemberRegex;
		/// <summary>
		/// Regular expression pattern of members to be hidden.
		/// </summary>
		public string HideMemberPattern
		{
			get { return _HideMemberRegex == null ? null : _HideMemberRegex.ToString(); }
			set { _HideMemberRegex = new Regex(value, RegexOptions.IgnoreCase); }
		}
		Regex _HideMemberRegex;
		internal readonly PSObject Value;
		internal int MemberMode { get; set; }
		/// <summary>
		/// New member explorer with an object.
		/// </summary>
		/// <param name="instance">An object which members are shown.</param>
		public MemberExplorer(object instance) :
			base(new Guid(TypeIdString))
		{
			if (instance == null) throw new ArgumentNullException("instance");
			if (instance is IPanel) throw new NotSupportedException("Panel instance is not supported.");
			Value = PSObject.AsPSObject(instance);
			if (Value.BaseObject == null)
				throw new ArgumentNullException("instance");

			Functions =
				ExplorerFunctions.DeleteFiles |
				ExplorerFunctions.CreateFile |
				ExplorerFunctions.GetContent |
				ExplorerFunctions.SetText;
		}
		/// <inheritdoc/>
		public override Panel CreatePanel()
		{
			return new MemberPanel(this);
		}
		/// <inheritdoc/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public override IList<FarFile> GetFiles(GetFilesEventArgs args)
		{
			if (args == null) return null;

			var result = new List<FarFile>();
			try
			{
				if (MemberMode == 0)
				{
					//! _100426_034702
					//! Get actual members to show:
					//! _Value.Properties contains too much, i.e. for DataRow: HasErrors, ItemArray, RowError, RowState;
					//! use Get-Member at first (don't use it itself, because it returns sorted data, we want unsorted).
					//! Idea to cache them is not good:
					//! price: high (have to sync on exclude, add, delete, etc.)
					//! value: low (it is UI and member number is normally small)
					var membersToShow = new List<string>();
					{
						//_131002_111804 in a DictionaryEntry avoid Name (same as Key)
						if (Value.BaseObject.GetType() == typeof(DictionaryEntry))
						{
							membersToShow.Add("Key");
							membersToShow.Add("Value");
						}
						else
						{
							string code = "Get-Member -InputObject $args[0] -MemberType Properties -ErrorAction 0";
							foreach (PSObject o in A.InvokeCode(code, Value))
							{
								string name = o.Properties[Word.Name].Value.ToString();
								if (_ExcludeMemberRegex == null || !_ExcludeMemberRegex.IsMatch(name))
									membersToShow.Add(name);
							}
						}
					}

					// to check hidden columns
					var datarow = Value.BaseObject as DataRow;

					// now we are ready to process properties in their original order
					foreach (PSPropertyInfo pi in Value.Properties)
					{
						if (!membersToShow.Contains(pi.Name))
							continue;

						var value = A.SafePropertyValue(pi);

						SetFile file = new SetFile()
						{
							Name = pi.Name,
							Data = pi
						};

						// base object
						PSObject asPSObject = value as PSObject;
						if (asPSObject != null)
							value = asPSObject.BaseObject;

						// value
						file.Description = Converter.FormatValue(value, Settings.Default.FormatEnumerationLimit);

						// hidden by user
						if (_HideMemberRegex != null && _HideMemberRegex.IsMatch(file.Name))
							file.IsHidden = true;

						// hidden due to column features
						if (!file.IsHidden && datarow != null)
						{
							var column = datarow.Table.Columns[file.Name];
							if (column.AutoIncrement || column.ColumnMapping == MappingType.Hidden || column.Expression.Length > 0)
								file.IsHidden = true;
						}

						result.Add(file);
					}
				}
				else
				{
					string code;
					if (MemberMode == 1)
						code = "Get-Member -InputObject $args[0] -ErrorAction 0 -View All";
					else
						code = "Get-Member -InputObject $args[0] -ErrorAction 0 -View All -Static";
					foreach (PSObject o in A.InvokeCode(code, Value))
					{
						SetFile f = new SetFile();
						f.Name = o.Properties[Word.Name].Value.ToString();

						PSPropertyInfo pi;
						pi = o.Properties["MemberType"];
						f.Description = pi.Value.ToString();

						pi = o.Properties["Definition"];
						f.Description += " " + pi.Value.ToString();

						f.Data = o;
						result.Add(f);
					}
				}
			}
			catch (RuntimeException exception)
			{
				if (args.UI)
					A.Message(exception.Message);
			}

			return result;
		}
		/// <inheritdoc/>
		public override void GetContent(GetContentEventArgs args)
		{
			if (args == null) return;

			// info
			PSPropertyInfo pi = Cast<PSPropertyInfo>.From(args.File.Data);
			if (pi == null)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			// text
			args.UseText = Converter.InfoToText(pi);
			if (args.UseText == null)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			args.CanSet = pi.IsSettable;
		}
		/// <inheritdoc/>
		public override void SetText(SetTextEventArgs args)
		{
			if (args == null) return;

			PSPropertyInfo pi = Cast<PSPropertyInfo>.From(args.File.Data);
			if (pi == null)
				args.Result = JobResult.Ignore;
			else
				A.SetPropertyFromTextUI(Value, pi, args.Text.TrimEnd());
		}
		/// <inheritdoc/>
		public override void DeleteFiles(DeleteFilesEventArgs args)
		{
			if (args == null) return;

			// skip not default modes
			if (MemberMode != 0)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			// ask
			if (args.UI && 0 != (long)Far.Api.GetSetting(FarSetting.Confirmations, "Delete"))
			{
				if (Far.Api.Message("Delete selected members", Res.Delete, MessageOptions.None, new string[] { Res.Delete, Res.Cancel }) != 0)
				{
					args.Result = JobResult.Ignore;
					return;
				}
			}

			try
			{
				int count1 = 0;
				foreach (var it in Value.Properties)
					++count1;

				foreach (FarFile file in args.Files)
				{
					PSPropertyInfo pi = file.Data as PSPropertyInfo;
					if (pi == null)
						continue;

					Value.Properties.Remove(pi.Name);
				}

				int count2 = 0;
				foreach (var it in Value.Properties)
					++count2;

				if (count1 - args.Files.Count != count2)
					args.Result = JobResult.Incomplete;
			}
			finally
			{
				// update always, some members can be deleted, don't leave them
				MemberPanel.WhenMemberChanged(Value); //????? will it be 2 times for THIS panel?
			}
		}
		/// <inheritdoc/>
		public override void CreateFile(CreateFileEventArgs args)
		{
			if (args == null) return;

			args.Result = JobResult.Ignore;

			UI.NewValueDialog ui = new UI.NewValueDialog("New property");
			while (ui.Dialog.Show())
			{
				try
				{
					// call value
					object value;
					if (ui.Type.Text.Length == 0)
						value = ui.Value.Text;
					else
						value = A.InvokeCode("[" + ui.Type.Text + "]$args[0]", ui.Value.Text)[0].BaseObject;

					// call Add-Member
					A.InvokeCode(
						"$args[0] | Add-Member -MemberType NoteProperty -Name $args[1] -Value $args[2] -Force -ErrorAction Stop",
						Value, ui.Name.Text, value);

					// done, post name
					args.Result = JobResult.Done;
					args.PostName = ui.Name.Text;
					return;
				}
				catch (RuntimeException ex)
				{
					A.Message(ex.Message);
					continue;
				}
			}
		}
	}
}
