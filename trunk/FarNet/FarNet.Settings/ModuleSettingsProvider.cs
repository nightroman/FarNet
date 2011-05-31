
/*
FarNet.Settings library for FarNet
Copyright (c) 2011 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Resources;
using System.Security.Permissions;

namespace FarNet.Settings
{
	/// <summary>
	/// Settings provider using .resources files.
	/// </summary>
	/// <remarks>
	/// The type of this class is used as the argument of <c>SettingsProviderAttribute</c> of settings classes.
	/// <para>
	/// Implementation.
	/// It reads and writes .resources using <c>ResourceReader</c> and <c>ResourceWriter</c>.
	/// It reads existing now settings and saves all requested settings including not dirty.
	/// Thus, old missing entries are removed from files when settings are saved.
	/// </para>
	/// <para>
	/// Settings contexts should have <see cref="RoamingFileName"/> and <see cref="LocalFileName"/>
	/// paths of .resources files where roaming and local settings are stored.
	/// </para>
	/// </remarks>
	public sealed class ModuleSettingsProvider : SettingsProvider
	{
		///
		public const string LocalFileName = "LocalFileName";
		///
		public const string RoamingFileName = "RoamingFileName";
		///
		public override void Initialize(string name, NameValueCollection config)
		{
			base.Initialize(name ?? "ModuleSettingsProvider", config);
		}
		///
		public override string ApplicationName
		{
			get { return string.Empty; }
			set { }
		}
		///
		public override string Name
		{
			get { return "ModuleSettingsProvider"; }
		}
		static bool IsRoaming(SettingsProperty property)
		{
			return property.Attributes.ContainsKey(typeof(SettingsManageabilityAttribute));
		}
		static string FileName(SettingsContext context, string name, bool create)
		{
			var file = (string)context[name];
			if (file == null)
				throw new InvalidOperationException("Settings context does not contain " + name);

			if (create)
			{
				var dir = Path.GetDirectoryName(file);
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}

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
					data.Add(it.Key, it.Value);
			}

			return data;
		}
		///
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
		{
			Hashtable dataLocal = null;
			Hashtable dataRoaming = null;

			var settingsPropertyValueCollection = new SettingsPropertyValueCollection();
			foreach (SettingsProperty settingsProperty in collection)
			{
				Hashtable data;
				if (IsRoaming(settingsProperty))
					data = dataRoaming ?? (dataRoaming = ReadData(FileName(context, RoamingFileName, false)));
				else
					data = dataLocal ?? (dataLocal = ReadData(FileName(context, LocalFileName, false)));

				var settingsPropertyValue = new SettingsPropertyValue(settingsProperty);

				var value = data[settingsProperty.Name];
				if (value != null)
					settingsPropertyValue.SerializedValue = value;

				settingsPropertyValueCollection.Add(settingsPropertyValue);
			}

			return settingsPropertyValueCollection;
		}
		///
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
		{
			bool dirtyLocal = false;
			bool dirtyRoaming = false;

			foreach (SettingsPropertyValue settingsPropertyValue in collection)
			{
				if (settingsPropertyValue.IsDirty)
				{
					if (IsRoaming(settingsPropertyValue.Property))
						dirtyRoaming = true;
					else
						dirtyLocal = true;
				}
			}

			ResourceWriter writerLocal = null;
			ResourceWriter writerRoaming = null;
			try
			{
				foreach (SettingsPropertyValue settingsPropertyValue in collection)
				{
					//?????? good idea to not write default values but
					// * find out how to restore 'default' state on [Del] in panel
					// * count actual data to write, if it is 0 the delete the store
					//if (settingsPropertyValue.UsingDefaultValue)
					//    continue;
					
					ResourceWriter writer;
					if (IsRoaming(settingsPropertyValue.Property))
					{
						if (!dirtyRoaming)
							continue;

						writer = writerRoaming ?? (writerRoaming = new ResourceWriter(FileName(context, RoamingFileName, true)));
					}
					else
					{
						if (!dirtyLocal)
							continue;

						writer = writerLocal ?? (writerLocal = new ResourceWriter(FileName(context, LocalFileName, true)));
					}

					writer.AddResource(settingsPropertyValue.Name, settingsPropertyValue.SerializedValue);
				}
			}
			finally
			{
				if (writerLocal != null)
					writerLocal.Close();

				if (writerRoaming != null)
					writerRoaming.Close();
			}

			foreach (SettingsPropertyValue settingsPropertyValue in collection)
				settingsPropertyValue.IsDirty = false;
		}
	}
}
