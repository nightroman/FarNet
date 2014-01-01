
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
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

			Title = explorer.Location;
			CurrentLocation = explorer.Location;

			SortMode = PanelSortMode.Name;
			ViewMode = PanelViewMode.AlternativeFull;

			PanelPlan plan = new PanelPlan();
			plan.Columns = new FarColumn[]
			{
				new SetColumn() { Kind = "O", Name = "Setting" },
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
		void SetDefaults()
		{
			foreach (FarFile file in SelectedFiles)
			{
				var value = (SettingsPropertyValue)file.Data;
				file.Description = SettingsExplorer.SetPropertyValueDefault(value);
				SettingsExplorer.CompleteFileData(file, value);
				_isDirty = true;
			}

			Update(false);
			Redraw();
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
		public override bool SaveData()
		{
			if (_isDirty)
			{
				_Explorer.Settings.Save();
				_isDirty = false;
			}
			return true;
		}
		protected override bool CanClose()
		{
			if (!SaveData())
				return false;

			return base.CanClose();
		}
		public override void UIClosed()
		{
			SaveData();
			base.UIClosed();
		}
	}
}
