
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2011 FarNet Team
*/

using System;
using System.Configuration;

namespace FarNet.Works.Config
{
	class SettingsPanel : Panel
	{
		bool _isDirty;
		SettingsExplorer _Explorer;
		public SettingsPanel(SettingsExplorer explorer)
			: base(explorer)
		{
			_Explorer = explorer;

			var type = _Explorer.Settings.GetType();
			Title = Far.Net.GetModuleManager(type).ModuleName + "\\" + type.Name;

			SortMode = PanelSortMode.Name;
			ViewMode = PanelViewMode.AlternativeFull;

			PanelPlan plan = new PanelPlan();
			plan.Columns = new FarColumn[]
			{
				new SetColumn() { Kind = "N", Name = "Name" },
				new SetColumn() { Kind = "Z", Name = "Value" }
			};
			SetPlan(PanelViewMode.AlternativeFull, plan);
		}
		public override void UISetText(SetTextEventArgs args)
		{
			base.UISetText(args);

			Update(true);
			Redraw();

			_isDirty = true;
		}
		public override void UIClosed()
		{
			base.UIClosed();
			if (_isDirty)
				_Explorer.Settings.Save();
		}
		void SetDefaults()
		{
			foreach (FarFile file in SelectedFiles)
			{
				var property = (SettingsProperty)file.Data;
				if (property.DefaultValue == null)
					continue;

				var value = _Explorer.Settings.PropertyValues[property.Name];

				//! fragile
				value.PropertyValue = null;
				value.Deserialized = false;
				value.SerializedValue = property.DefaultValue.ToString();
				file.Description = Convert.ToString(_Explorer.Settings[property.Name]);

				_isDirty = true;
			}

			Update(false);
			Redraw();
		}
		public override bool UIKeyPressed(int code, KeyStates state)
		{
			switch (code)
			{
				case VKeyCode.F1:

					if (state == KeyStates.None)
					{
						Far.Net.ShowHelp(Far.Net.GetType().Assembly.Location, SettingsUI.HelpSettings, HelpOptions.None);
						return true;
					}

					break;

				case VKeyCode.Delete:

					goto case VKeyCode.F8;

				case VKeyCode.F8:

					SetDefaults();
					return true;
			}

			return base.UIKeyPressed(code, state);
		}
	}
}
