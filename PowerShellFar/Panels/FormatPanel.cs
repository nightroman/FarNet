
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
	/// Formatted table panel.
	/// </summary>
	public abstract class FormatPanel : TablePanel
	{
		///
		protected FormatPanel(FormatExplorer explorer) : base(explorer) { }
		/// <summary>
		/// Gets a list of ready files or a collection of PS objects.
		/// </summary>
		internal abstract object GetData();
		internal override string HelpMenuTextOpenFileMembers { get { return "Object members"; } }
		internal void BuildPlan(string sameType)
		{
			PanelPlan plan = GetPlan(PanelViewMode.AlternativeFull);
			if (plan == null)
				plan = new PanelPlan();

			// choose columns
			if (sameType == null)
			{
				//! The long "Index" clashes to sort order mark, use the short "##"
				plan.Columns = new FarColumn[]
				{
				new SetColumn() { Kind = "S", Name = "##"},
				new SetColumn() { Kind = "N", Name = "Value"},
				new SetColumn() { Kind = "Z", Name = "Type"}
				};
			}
			else
			{
				plan.Columns = new FarColumn[]
				{
				new SetColumn() { Kind = "N", Name = sameType }
				};
			}

			SetPlan(PanelViewMode.AlternativeFull, plan);
		}
	}
}
