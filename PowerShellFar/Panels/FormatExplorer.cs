
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PowerShellFar;

/// <summary>
/// Formatted table explorer.
/// </summary>
/// <inheritdoc/>
public abstract class FormatExplorer(Guid typeId) : TableExplorer(typeId)
{
	internal FileMap? Map { get; private set; } // internal ???
	internal Meta[]? Metas { get; private set; } // internal ???

	/// <include file='doc.xml' path='doc/Columns/*'/>
	internal sealed override object[]? Columns
	{
		get => base.Columns;
		set
		{
			base.Columns = value;
			Metas = null;
			if (value is null)
				Map = null;
			else
				MakeMap(null);
		}
	}

	void MakeMap(Meta[]? metas) //????
	{
		//1 make map
		Map = Format.MakeMap(ref metas, Columns!);

		// keep metas for panels to set plan
		Metas = metas;
	}

	internal abstract object GetData(GetFilesEventArgs args);

	/// <inheritdoc/>
	public override IList<FarFile> DoGetFiles(GetFilesEventArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);

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
				Far.Api.ShowError(Res.Me, ex);

			data = new List<FarFile>();
		}

		// if the data are files just use them, assume all is done
		if (data is IList<FarFile> readyFiles)
		{
			Cache = readyFiles;
			return Cache;
		}

		// PS objects
		Collection<PSObject> values = (Collection<PSObject>)data;

		// empty?
		var panel = args.Parameter as FormatPanel;
		if (values.Count == 0)
		{
			// drop files in any case
			Cache.Clear();

			// no panel, no job
			if (panel is null)
				return Cache;

			// respect custom columns
			if (Columns != null)
				return Cache;

			// is it already <empty>?
			var plan = panel.GetPlan(PanelViewMode.AlternativeFull);
			if (plan is null)
				plan = new PanelPlan();
			else if (plan.Columns!.Length == 1 && plan.Columns[0].Name == "<empty>")
				return Cache;

			// reuse the mode: reset columns, keep other data intact
			plan.Columns = [new SetColumn() { Kind = "N", Name = "<empty>" }];
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
			Type? commonType;
			var sample = values[0].BaseObject;
			var sampleType = sample.GetType();

			// case: use panel columns Index, Value, Type
			if (Converter.IsLinearType(sampleType) ||
				sample is IEnumerable && !Converter.IsGrouping(sampleType) ||
				null == (commonType = A.FindCommonType(values)))
			{
				panel?.BuildPlan(Format.BuildFilesMixed(Cache, values));
				return Cache;
			}

			Meta[]? metas = null;

			// MatchInfo of Select-String
			if (commonType.FullName == Res.MatchInfoTypeName)
			{
				metas = [
					new("Path"),
					new("Line"),
				];
			}
			else if (Converter.IsGrouping(commonType))
			{
				metas = [
					new("Count") { Kind = "S", Width = 7, Alignment = Alignment.Right },
					new("Key"),
				];
			}
			else
			{
				// try to get format
				if (commonType != typeof(PSCustomObject))
					metas = Format.TryFormatByTableControl(values[0], panel is null ? 80 : panel.Window.Width); //???? avoid formatting at all
			}

			// use members
			metas ??= Format.TryFormatByMembers(values, commonType != null && commonType == values[0].BaseObject.GetType());

			if (metas is null)
			{
				panel?.BuildPlan(Format.BuildFilesMixed(Cache, values));
			}
			else
			{
				MakeMap(metas);
				panel?.SetPlan(PanelViewMode.AlternativeFull, Format.SetupPanelMode(Metas!));

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
			files.Add(new MapFile(value, Map!));
	}
}
