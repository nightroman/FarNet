
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Works;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FarNet
{
	/// <summary>
	/// Internal base class for <see cref="ModuleSettings{T}"/>.
	/// </summary>
	public abstract class ModuleSettingsBase
	{
		readonly Type _type;
		object _data;

		/// <summary>
		/// Gets the stored settings file path.
		/// </summary>
		public string FileName { get; }

		/// <summary>
		/// Gets true for local settings.
		/// </summary>
		public bool IsLocal { get; }

		/// <summary>
		/// Gets the module manager.
		/// </summary>
		public IModuleManager Manager { get; }

		internal ModuleSettingsBase(Type dataType, ModuleSettingsArgs args)
		{
			_type = dataType;
			IsLocal = args.IsLocal;

			var myType = GetType();
			Manager = Far.Api.GetModuleManager(myType);

			FileName = Manager.GetFolderPath(IsLocal ? SpecialFolder.LocalData : SpecialFolder.RoamingData, true) + "\\" + myType.Name + ".xml";
		}

		internal object GetOrReadData()
		{
			if (_data is not null)
				return _data;

			object data;
			if (File.Exists(FileName))
			{
				try
				{
					data = Read();
				}
				catch (Exception ex)
				{
					throw new ModuleException($"Cannot read settings.\nFile: {FileName}\nError: {ex.Message}", ex);
				}
			}
			else
			{
				data = Activator.CreateInstance(_type);
			}

			ValidateData(data);
			return _data = data;
		}

		object Read()
		{
			using var reader = File.OpenRead(FileName);
			var serializer = new XmlSerializer(_type);
			return serializer.Deserialize(reader);
		}

		static void Save(string fileName, object data)
		{
			using var writer = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
			var serializer = new XmlSerializer(data.GetType());
			serializer.Serialize(writer, data);
		}

		void ValidateData(object data)
		{
			if (data is not IValidate validate)
				return;

			try
			{
				validate.Validate();
			}
			catch (Exception ex)
			{
				throw new ModuleException($"Cannot validate data.\nFile: {FileName}\nError: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Saves the settings.
		/// </summary>
		public void Save()
		{
			Save(FileName, GetOrReadData());
		}

		/// <summary>
		/// Opens the editor with settings XML.
		/// </summary>
		public void Edit()
		{
			//! ensure data, we need it for diff
			GetOrReadData();

			if (!File.Exists(FileName))
				Save(FileName, _data);

			var editor = Far.Api.CreateEditor();
			editor.FileName = FileName;
			editor.Title = $"{GetType().FullName} - {FileName}";

			editor.Saving += (sender, args) =>
			{
				var xml = editor.GetText();
				using var reader = new StringReader(xml);
				var serializer = new XmlSerializer(_type);
				var data = serializer.Deserialize(reader);
				ValidateData(data);
				_data = data;
			};

			editor.Opened += (sender, args) =>
			{
				var xmlSettings = new XmlReaderSettings { IgnoreComments = true };
				using var reader = XmlReader.Create(new StringReader(editor.GetText()), xmlSettings);
				var docOld = new XmlDocument { PreserveWhitespace = false };
				docOld.Load(reader);
				var xmlOld = docOld.DocumentElement.OuterXml;

				var serializer = new XmlSerializer(_type);
				using var writer = new StringWriter();
				serializer.Serialize(writer, _data);
				var docNew = new XmlDocument { PreserveWhitespace = false };
				docNew.LoadXml(writer.ToString());
				var xmlNew = docNew.DocumentElement.OuterXml;

				if (xmlOld == xmlNew)
					return;

				var answer = Far.Api.Message(
					DifferentXml,
					GetType().FullName,
					MessageOptions.YesNo | MessageOptions.LeftAligned);

				if (answer != 0)
					return;

				// get new XML via file, avoid UTF16 on using StringWriter
				var temp = Kit.TempFileName(null);
				Save(temp, _data);
				var newText = File.ReadAllText(temp);
				File.Delete(temp);
				editor.SetText(newText);
			};

			editor.Open();
		}

		const string DifferentXml = @"
The XML is different from the current settings XML.
Replace the text with the current settings XML?

You may undo this change before saving.

What you may get:
- Original formatting and elements order
- Added new and removed old elements
- Comments are removed
";
	}
}
