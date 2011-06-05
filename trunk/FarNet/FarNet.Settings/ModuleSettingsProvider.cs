
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
	/// This provider reads and writes .resources using <c>ResourceReader</c> and <c>ResourceWriter</c>.
	/// Settings contexts should have <see cref="RoamingFileName"/> and <see cref="LocalFileName"/>
	/// paths of .resources files where roaming and local settings are stored.
	/// </para>
	/// <para>
	/// Settings having <c>UsingDefaultValue</c> equal to true are not stored in files.
	/// Thus, such settings are restored with their current default values.
	/// </para>
	/// <para>
	/// Settings having <c>SerializedValue</c> equal to null are not stored.
	/// Thus, such settings are restored with their current default values.
	/// If defaults are not null then there is a subtle issue of settings values set to null.
	/// Basically it is better to avoid not null defaults for reference types other than strings.
	/// It is fine to have not null defaults for strings but consider to save empty strings, not nulls.
	/// Compare: stored empty strings are restored exactly, not stored nulls are restored as current defaults.
	/// </para>
	/// </remarks>
	public sealed class ModuleSettingsProvider : SettingsProvider
	{
		///
		public const string RoamingFileName = "RoamingFileName";
		///
		public const string LocalFileName = "LocalFileName";
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
			var result = new SettingsPropertyValueCollection();
			Hashtable hashRoaming = null;
			Hashtable hashLocal = null;

			foreach (SettingsProperty property in collection)
			{
				var value = new SettingsPropertyValue(property);
				result.Add(value);

				Hashtable hash;
				if (IsRoaming(property))
					hash = hashRoaming ?? (hashRoaming = ReadData(FileName(context, RoamingFileName, false)));
				else
					hash = hashLocal ?? (hashLocal = ReadData(FileName(context, LocalFileName, false)));

				var serialized = hash[property.Name];
				if (serialized != null)
					value.SerializedValue = serialized;
			}

			return result;
		}
		///
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
		{
			bool dirtyRoaming = false;
			bool dirtyLocal = false;

			foreach (SettingsPropertyValue value in collection)
			{
				if (value.IsDirty)
				{
					if (IsRoaming(value.Property))
						dirtyRoaming = true;
					else
						dirtyLocal = true;
				}
			}

			ResourceWriter writerRoaming = null;
			ResourceWriter writerLocal = null;
			try
			{
				foreach (SettingsPropertyValue value in collection)
				{
					if (value.UsingDefaultValue)
						continue;

					//? Deserialized nulls cause default values to be used.
					//? Thus, it does not make much sense to store nulls.
					var serialized = value.SerializedValue;
					if (serialized == null)
						continue;

					ResourceWriter writer;
					if (IsRoaming(value.Property))
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

					writer.AddResource(value.Name, serialized);
				}
			}
			finally
			{
				// close files being written or delete dirty but not written files

				if (writerRoaming != null)
				{
					writerRoaming.Close();
				}
				else if (dirtyRoaming)
				{
					File.Delete(FileName(context, RoamingFileName, false));
				}

				if (writerLocal != null)
				{
					writerLocal.Close();
				}
				else if (dirtyLocal)
				{
					File.Delete(FileName(context, LocalFileName, false));
				}
			}

			// all is saved now, clean all dirty flags
			foreach (SettingsPropertyValue value in collection)
				value.IsDirty = false;
		}
	}
}
