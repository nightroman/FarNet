
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Configuration;

namespace FarNet.Works.Config
{
	class SettingsPanel : Panel
	{
		readonly SettingsExplorer _Explorer;
		public SettingsPanel(SettingsExplorer explorer)
			: base(explorer)
		{
			_Explorer = explorer;

			Title = explorer.Location;
			CurrentLocation = explorer.Location;

			SortMode = PanelSortMode.Name;
			ViewMode = PanelViewMode.AlternativeFull;

			var plan = new PanelPlan
			{
				Columns = new FarColumn[]
				{
					new SetColumn() { Kind = "O", Name = "Setting" },
					new SetColumn() { Kind = "Z", Name = "Value" }
				}
			};
			SetPlan(PanelViewMode.AlternativeFull, plan);
		}
		public override void UISetText(SetTextEventArgs args)
		{
			base.UISetText(args);

			Update(true);
			Redraw();

			SaveData();
		}
		void SetDefaults()
		{
			foreach (FarFile file in SelectedFiles)
			{
				var value = (SettingsPropertyValue)file.Data;
				file.Description = SettingsExplorer.SetPropertyValueDefault(value);
				SettingsExplorer.CompleteFileData(file, value);
			}

			Update(false);
			Redraw();

			SaveData();
		}
		public override bool UIKeyPressed(KeyInfo key)
		{
			if (key == null) throw new ArgumentNullException("key");

			switch (key.VirtualKeyCode)
			{
				case KeyCode.F1:

					if (key.Is())
					{
						Far.Api.ShowHelp(Far.Api.GetType().Assembly.Location, SettingsUI.HelpSettings, HelpOptions.None);
						return true;
					}

					break;

				case KeyCode.Delete:
				case KeyCode.F8:

					SetDefaults();
					return true;
			}

			return base.UIKeyPressed(key);
		}
		// Call this on changing any value (used to be called on closing).
		// This allows applying changes in modules with overridden `Save`.
		// E.g. RightControl updates cached regexes on `Save`.
		public override bool SaveData()
		{
			_Explorer.Settings.Save();
			return true;
		}
	}
}
