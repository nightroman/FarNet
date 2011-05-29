
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
				var text = property.DefaultValue as string;
				if (text == null)
					continue;

				file.Description = _Explorer.SetPropertyText(property, text);
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
