
/*
FarNet.Settings library for FarNet
Copyright (c) 2011-2013 Roman Kuzmin
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
	/// <para>
	/// Recommended data types for trivial settings are <c>String</c> and <c>DateTime</c> and primitive types:
	/// <c>Boolean</c>, <c>Byte/SByte</c>, <c>Int16/UInt16</c>, <c>Int32/UInt32</c>, <c>Int64/UInt64</c>, <c>Char</c>, <c>Double</c>, and <c>Single</c>.
	/// </para>
	/// <para>
	/// Other trivial standard value types (for example <c>Guid</c>, <c>Decimal</c>, etc.) can be used as well, with or without defaults,
	/// but the settings engine never treats them as using default values, <c>UsingDefaultValue</c> is always false.
	/// </para>
	/// <para>
	/// Implementation.
	/// This provider reads and writes .resources files using <c>ResourceReader</c> and <c>ResourceWriter</c>.
	/// Settings classes should have added to their contexts two key/value pairs where keys are
	/// <see cref="RoamingFileName"/> and <see cref="LocalFileName"/>
	/// and values are full roaming and local paths of settings files.
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
			string fileRoaming = null;
			string fileLocal = null;
			string tempRoaming = null;
			string tempLocal = null;
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

						if (writerRoaming == null)
						{
							fileRoaming = FileName(context, RoamingFileName, true);
							tempRoaming = fileRoaming + ".tmp";
							writerRoaming = new ResourceWriter(tempRoaming);
						}

						writer = writerRoaming;
					}
					else
					{
						if (!dirtyLocal)
							continue;

						if (writerLocal == null)
						{
							fileLocal = FileName(context, LocalFileName, true);
							tempLocal = fileLocal + ".tmp";
							writerLocal = new ResourceWriter(tempLocal);
						}

						writer = writerLocal;
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
					ReplaceFile(tempRoaming, fileRoaming);
				}
				else if (dirtyRoaming)
				{
					File.Delete(FileName(context, RoamingFileName, false));
				}

				if (writerLocal != null)
				{
					writerLocal.Close();
					ReplaceFile(tempLocal, fileLocal);
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
		static void ReplaceFile(string source, string destination)
		{
			if (File.Exists(destination))
				File.Replace(source, destination, null);
			else
				File.Move(source, destination);
		}
	}
}
