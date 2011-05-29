
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;

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
			if (args == null) return;
			var property = (SettingsProperty)args.File.Data;

			// no text
			if (property.SerializeAs != SettingsSerializeAs.String && property.SerializeAs != SettingsSerializeAs.Xml)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			// text
			args.UseText = args.File.Description;

			// tweaks
			args.CanSet = !property.IsReadOnly;
			if (property.SerializeAs == SettingsSerializeAs.Xml)
				args.UseFileExtension = ".xml";
			else
				args.UseFileExtension = ".txt";
		}
		public override void SetText(SetTextEventArgs args)
		{
			if (args == null) return;
			var property = (SettingsProperty)args.File.Data;

			var ex = ValidatePropertyText(property, args.Text);
			if (ex != null)
			{
				args.Result = JobResult.Ignore;
				Far.Net.ShowError("Settings", ex);
				return;
			}

			args.File.Description = SetPropertyText(property, args.Text);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static Exception ValidatePropertyText(SettingsProperty property, string text)
		{
			try
			{
				if (property.SerializeAs == SettingsSerializeAs.String)
				{
					var converter = TypeDescriptor.GetConverter(property.PropertyType);
					if (converter != null && converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
					{
						converter.ConvertFromInvariantString(text);
						return null;
					}
				}
				else
				{
					using (var reader = new StringReader(text))
					{
						var obj = new XmlSerializer(property.PropertyType).Deserialize(reader);
						if (obj == null || property.PropertyType.IsAssignableFrom(obj.GetType()))
							return null;
					}
				}
			}
			catch (Exception ex)
			{
				return ex;
			}

			return new NotSupportedException("Cannot convert.");
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		string GetPropertyText(SettingsProperty property)
		{
			// ensure the value
			var foo = _settings[property.Name];
			var value = _settings.PropertyValues[property.Name];

			// get its serialized text
			var text = value.SerializedValue as string;
			return text ?? string.Empty;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		internal string SetPropertyText(SettingsProperty property, string text)
		{
			if (property.SerializeAs != SettingsSerializeAs.String && property.SerializeAs != SettingsSerializeAs.Xml)
				throw new InvalidOperationException();

			//! fragile

			// ensure the value exists
			var foo = _settings[property.Name];
			var value = _settings.PropertyValues[property.Name];

			// set the text to be deserialized
			value.SerializedValue = text;
			value.Deserialized = false;

			// get and set: deserialize and set data (this invalidates serialized)
			value.PropertyValue = value.PropertyValue;

			// return the serialized (it is updated)
			return (string)value.SerializedValue ?? string.Empty;
		}
	}
}
