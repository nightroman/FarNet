
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
	/// Formatted table explorer.
	/// </summary>
	public abstract class FormatExplorer : TableExplorer
	{
		internal FileMap Map { get; private set; } // internal ???
		internal Meta[] Metas { get; private set; } // internal ???
		///
		protected FormatExplorer(Guid typeId) : base(typeId) { }
		/// <include file='doc.xml' path='doc/Columns/*'/>
		internal sealed override object[] Columns
		{
			get
			{
				return base.Columns;
			}
			set
			{
				base.Columns = value;
				Metas = null;
				if (value == null)
					Map = null;
				else
					MakeMap(null);
			}
		}
		void MakeMap(Meta[] metas) //????
		{
			//1 make map
			Map = Format.MakeMap(ref metas, Columns);

			// keep metas for panels to set plan
			Metas = metas;
		}
		internal abstract object GetData(ExplorerEventArgs args);
		///
		public override IList<FarFile> DoGetFiles(ExplorerEventArgs args)
		{
			if (args == null) return null;

			var panel = args.Panel as FormatPanel;

			// call the worker
			// _090408_232925 If we throw then FarNet returns false and Far closes the panel.
			object data;
			try
			{
				data = GetData(args);
			}
			catch (RuntimeException ex)
			{
				if (args.UI)
					Far.Net.ShowError(Res.Me, ex);

				data = new List<FarFile>();
			}

			// if the data are files just use them, assume all is done
			IList<FarFile> readyFiles = data as IList<FarFile>;
			if (readyFiles != null)
			{
				Cache = readyFiles;
				return Cache;
			}

			// PS objects
			Collection<PSObject> values = (Collection<PSObject>)data;

			// empty?
			if (values.Count == 0)
			{
				// drop files in any case
				Cache.Clear();

				// no panel, no job
				if (panel == null)
					return Cache;

				// respect custom columns
				if (Columns != null)
					return Cache;

				// is it already <empty>?
				PanelPlan plan = panel.GetPlan(PanelViewMode.AlternativeFull);
				if (plan == null)
					plan = new PanelPlan();
				else if (plan.Columns.Length == 1 && plan.Columns[0].Name == "<empty>")
					return Cache;

				// reuse the mode: reset columns, keep other data intact
				plan.Columns = new FarColumn[] { new SetColumn() { Kind = "N", Name = "<empty>" } };
				panel.SetPlan(PanelViewMode.AlternativeFull, plan);
				return Cache;
			}

			// not empty; values has to be removed in any case
			try
			{
				// custom
				if (Columns != null)
				{
					BuildFiles(values);
					return Cache;
				}

				// Check some special cases and try to get the common type.
				// _100309_121508 Linear type case
				Type theType;
				if (Converter.IsLinearType(values[0].BaseObject.GetType()) ||
					values[0].BaseObject is System.Collections.IEnumerable ||
					null == (theType = A.FindCommonType(values)))
				{
					// use index, value, type mode
					if (panel != null)
						panel.BuildPlan(Format.BuildFilesMixed(Cache, values));
					return Cache;
				}

				Meta[] metas = null;

				// try to get format
				if (theType != typeof(PSCustomObject))
					metas = Format.TryFormatByTableControl(values[0], panel == null ? 80 : panel.Window.Width); //???? avoid formatting at all

				// use members
				if (metas == null)
					metas = Format.TryFormatByMembers(values, theType != null && theType == values[0].BaseObject.GetType());

				if (metas == null)
				{
					if (panel != null)
						panel.BuildPlan(Format.BuildFilesMixed(Cache, values));
				}
				else
				{
					MakeMap(metas);
					if (panel != null)
						panel.SetPlan(PanelViewMode.AlternativeFull, Format.SetupPanelMode(Metas));
					
					BuildFiles(values);
				}
			}
			finally
			{
				values.Clear();
			}

			return Cache;
		}
		///
		internal virtual void BuildFiles(Collection<PSObject> values)
		{
			var files = new List<FarFile>(values.Count);
			Cache = files;

			foreach (PSObject value in values)
				files.Add(new MapFile(value, Map));
		}
	}
}
