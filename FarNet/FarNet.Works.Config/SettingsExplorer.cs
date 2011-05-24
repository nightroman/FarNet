
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

namespace FarNet.Works.Config
{
	public class SettingsExplorer : Explorer
	{
		readonly ApplicationSettingsBase _settings;
		internal ApplicationSettingsBase Settings { get { return _settings; } }
		readonly List<FarFile> _files = new List<FarFile>();
		public SettingsExplorer(ApplicationSettingsBase settings)
			: base(new Guid("B760CC2A-F836-403E-9BD5-17807A387A8E"))
		{
			_settings = settings;

			Functions = ExplorerFunctions.GetContent | ExplorerFunctions.SetText;

			foreach (SettingsProperty property in _settings.Properties)
			{
				// skip not browsable
				var browsable = (BrowsableAttribute)property.Attributes[typeof(BrowsableAttribute)];
				if (browsable != null && !browsable.Browsable)
					continue;

				var file = new SetFile();
				_files.Add(file);
				file.Name = property.Name;
				file.Data = property;
				file.Description = GetPropertyText(property);
			}
		}
		string GetPropertyText(SettingsProperty property)
		{
			var value = _settings[property.Name];
			if (value == null)
				return string.Empty;
			else
				return Convert.ToString(value);
		}
		internal void SetPropertyText(SettingsProperty property, string text)
		{
			_settings[property.Name] = Convert.ChangeType(text, property.PropertyType);
		}
		public override Panel CreatePanel()
		{
			return new SettingsPanel(this);
		}
		public override IList<FarFile> GetFiles(GetFilesEventArgs args)
		{
			return _files;
		}
		public override void GetContent(GetContentEventArgs args)
		{
			var property = (SettingsProperty)args.File.Data;
			args.CanSet = !property.IsReadOnly;
			args.UseText = args.File.Description;
		}
		public override void SetText(SetTextEventArgs args)
		{
			var property = (SettingsProperty)args.File.Data;
			SetPropertyText(property, args.Text);
			args.File.Description = GetPropertyText(property);
		}
	}
}
