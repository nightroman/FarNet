
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
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
			
			var type = _settings.GetType();
			Location = Far.Api.GetModuleManager(type).ModuleName + "\\" + type.Name;

			foreach (SettingsProperty property in _settings.Properties)
			{
				// skip not browsable
				var browsable = (BrowsableAttribute)property.Attributes[typeof(BrowsableAttribute)];
				if (browsable != null && !browsable.Browsable)
					continue;

				// ensure the property value exists
				var dummy = _settings[property.Name];
				var value = _settings.PropertyValues[property.Name];
				
				var file = new SetFile();
				_files.Add(file);
				file.Data = value;
				file.Name = property.Name;
				file.Description = GetPropertyValueText(value);

				CompleteFileData(file, value);
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
			var value = (SettingsPropertyValue)args.File.Data;

			// no text
			if (value.Property.SerializeAs != SettingsSerializeAs.String && value.Property.SerializeAs != SettingsSerializeAs.Xml)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			// text
			args.UseText = args.File.Description;

			// tweaks
			args.CanSet = !value.Property.IsReadOnly;
			if (value.Property.SerializeAs == SettingsSerializeAs.Xml)
				args.UseFileExtension = ".xml";
			else
				args.UseFileExtension = ".txt";
		}
		public override void SetText(SetTextEventArgs args)
		{
			if (args == null) return;
			var value = (SettingsPropertyValue)args.File.Data;

			var ex = ValidatePropertyText(value.Property, args.Text);
			if (ex != null)
			{
				args.Result = JobResult.Ignore;
				Far.Api.ShowError("Settings", ex);
				return;
			}

			args.File.Description = SetPropertyValueText(value, args.Text);

			CompleteFileData(args.File, value);
		}
		internal static void CompleteFileData(FarFile file, SettingsPropertyValue value)
		{
			// defaults
			var prefix = value.UsingDefaultValue ? "- " : "+ ";
			file.Owner = prefix + file.Name;
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
		static string GetPropertyValueText(SettingsPropertyValue value)
		{
			// get its default or serialized text (the latter may be not set if the default is used)
			if (value.UsingDefaultValue)
				return value.Property.DefaultValue as string ?? string.Empty;
			else
				return value.SerializedValue as string ?? string.Empty;
		}
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		internal static string SetPropertyValueDefault(SettingsPropertyValue value)
		{
			//! fragile

			// set the text to be deserialized
			value.SerializedValue = null;
			value.Deserialized = false;

			// set the default flag (it seems to be the only way to set this flag)
			var dummy = value.PropertyValue;
			value.IsDirty = true;
			return value.Property.DefaultValue as string ?? string.Empty;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		internal static string SetPropertyValueText(SettingsPropertyValue value, string text)
		{
			if (value.Property.SerializeAs != SettingsSerializeAs.String && value.Property.SerializeAs != SettingsSerializeAs.Xml)
				throw new InvalidOperationException();

			//! fragile

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
