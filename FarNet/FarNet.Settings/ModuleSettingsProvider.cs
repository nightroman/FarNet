
/*
FarNet.Settings library for FarNet
Copyright (c) 2011 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace FarNet.Settings
{
	/// <summary>
	/// Settings provider using .resource files.
	/// </summary>
	/// <remarks>
	/// The type is used as an argument of <c>SettingsProviderAttribute</c>
	/// of classes derived from <c>ApplicationSettingsBase</c> or its child classes.
	/// <para>
	/// Implementation is straightforward: it reads and writes .resources using <c>ResourceReader</c> and <c>ResourceWriter</c>.
	/// It writes all settings and reads present only. Missing old settings data are removed from files when settings are saved.
	/// </para>
	/// </remarks>
	[ComVisible(false)]
	public sealed class ModuleSettingsProvider : SettingsProvider
	{
		internal const string LocalFileName = "LocalFileName";
		internal const string RoamingFileName = "RoamingFileName";
		///
		public override void Initialize(string name, NameValueCollection config)
		{
			base.Initialize(name ?? ApplicationName, config);
		}
		///
		public override string ApplicationName
		{
			get { return Assembly.GetExecutingAssembly().GetName().Name; }
			set { }
		}
		///
		public override string Name
		{
			get { return GetType().Name; }
		}
		static bool IsRoaming(SettingsProperty property)
		{
			return property.Attributes.ContainsKey(typeof(SettingsManageabilityAttribute));
		}
		static string GetFileName(SettingsContext context, string name)
		{
			var file = (string)context[name];
			if (string.IsNullOrEmpty(file))
				throw new InvalidOperationException("Settings context does not contain " + name);
			return file;
		}
		static Hashtable ReadData(string fileName)
		{
			var data = new Hashtable();
			if (!File.Exists(fileName))
				return data;

			using (var reader = new ResourceReader(fileName))
			{
				var it = reader.GetEnumerator();
				while (it.MoveNext())
				{
					try
					{
						data.Add(it.Key, it.Value);
					}
					catch (Exception ex)
					{
						Far.Net.ShowError("Reading settings", ex);
					}
				}
			}

			return data;
		}
		///
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
		{
			ResourceWriter writerLocal = null;
			ResourceWriter writerRoaming = null;
			try
			{
				foreach (SettingsPropertyValue settingsPropertyValue in collection)
				{
					ResourceWriter writer;
					if (IsRoaming(settingsPropertyValue.Property))
						writer = writerRoaming ?? (writerRoaming = new ResourceWriter(GetFileName(context, RoamingFileName)));
					else
						writer = writerLocal ?? (writerLocal = new ResourceWriter(GetFileName(context, LocalFileName)));

					switch (settingsPropertyValue.Property.SerializeAs)
					{
						case SettingsSerializeAs.String:
							writer.AddResource(settingsPropertyValue.Name, Convert.ToString(settingsPropertyValue.PropertyValue, CultureInfo.InvariantCulture));
							break;
						case SettingsSerializeAs.Binary:
							writer.AddResource(settingsPropertyValue.Name, settingsPropertyValue.PropertyValue);
							break;
						case SettingsSerializeAs.Xml:
							var serializer = new XmlSerializer(settingsPropertyValue.Property.PropertyType);
							using (var sw = new StringWriter())
							{
								serializer.Serialize(sw, settingsPropertyValue.PropertyValue);
								writer.AddResource(settingsPropertyValue.Name, sw.ToString());
							}
							break;
					}

					settingsPropertyValue.IsDirty = false;
				}
			}
			finally
			{
				if (writerLocal != null)
					writerLocal.Close();

				if (writerRoaming != null)
					writerRoaming.Close();
			}
		}
		///
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
		{
			Hashtable dataLocal = null;
			Hashtable dataRoaming = null;

			var settingsPropertyValueCollection = new SettingsPropertyValueCollection();
			foreach (SettingsProperty settingsProperty in collection)
			{
				var settingsPropertyValue = new SettingsPropertyValue(settingsProperty);

				Hashtable data;
				if (IsRoaming(settingsPropertyValue.Property))
					data = dataRoaming ?? (dataRoaming = ReadData(GetFileName(context, RoamingFileName)));
				else
					data = dataLocal ?? (dataLocal = ReadData(GetFileName(context, LocalFileName)));

				var value = data[settingsProperty.Name];
				if (value != null)
				{
					if (settingsProperty.SerializeAs == SettingsSerializeAs.Binary)
						settingsPropertyValue.PropertyValue = value;
					else
						settingsPropertyValue.SerializedValue = value;
				}

				settingsPropertyValueCollection.Add(settingsPropertyValue);
			}

			return settingsPropertyValueCollection;
		}
	}
}
