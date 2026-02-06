using FarNet.Works;
using System.ComponentModel.DataAnnotations;
using System.Xml;
using System.Xml.Serialization;

namespace FarNet;

/// <summary>
/// Internal base class for <see cref="ModuleSettings{T}"/>.
/// </summary>
public abstract class ModuleSettingsBase
{
	object? _data;

	internal static readonly XmlRootAttribute _rootAttribute = new("Data");
	static readonly XmlWriterSettings _xmlWriterSettings = new() { Indent = true, OmitXmlDeclaration = true };
	static readonly XmlSerializerNamespaces _xmlSerializerNamespaces = new([XmlQualifiedName.Empty]);

	/// <summary>
	/// Gets the stored settings file path.
	/// </summary>
	public string FileName { get; }

	internal ModuleSettingsBase(ModuleSettingsArgs args)
	{
		if (args.FileName is null)
		{
			var myType = GetType();
			var manager = Far.Api.GetModuleManager(myType);
			FileName = manager.GetFolderPath(args.IsLocal ? SpecialFolder.LocalData : SpecialFolder.RoamingData, true) + "\\" + myType.Name + ".xml";
		}
		else
		{
			FileName = args.FileName;
		}
	}

	internal object GetOrReadData()
	{
		if (_data is not null)
			return _data;

		try
		{
			object data;
			if (File.Exists(FileName))
			{
				try
				{
					data = Read();
					if (DoUpdateData(data))
						Save(FileName, data);
				}
				catch (Exception ex)
				{
					throw new ModuleException($"Cannot read settings.\nFile: {FileName}\nError: {ex.Message}", ex);
				}
			}
			else
			{
				data = DoNewData();
			}

			ValidateData(data);
			return _data = data;
		}
		catch (Exception ex)
		{
			// log and post error
			Log.TraceException(ex);
			Far.Api.PostJob(() => Far.Api.ShowError("Using default settings", ex));

			// use default data
			_data = DoNewData();
			ValidateData(_data);

			return _data;
		}
	}

	internal abstract object DoNewData();

	internal abstract bool DoUpdateData(object data);

	internal abstract XmlSerializer GetXmlSerializer();

	internal abstract string GetTypeName();

	object Read()
	{
		using var reader = File.OpenRead(FileName);
		return GetXmlSerializer().Deserialize(reader)!;
	}

	void Save(string fileName, object data)
	{
		//! ensure the directory, e.g. FarNet\FarNet.xml in vanilla FarNet
		Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);

		// serialize
		using var writer = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
		using var xmlWriter = XmlWriter.Create(writer, _xmlWriterSettings);
		GetXmlSerializer().Serialize(xmlWriter, data, _xmlSerializerNamespaces);
	}

	void ValidateData(object data)
	{
		if (data is IValidatableObject validatable)
		{
			try
			{
				Validator.ValidateObject(
					validatable,
					new ValidationContext(data),
					validateAllProperties: true);
			}
			catch (ValidationException ex)
			{
				throw new ModuleException($"""
					Settings validation error.
					File: {FileName}
					Field: {string.Join(", ", ex.ValidationResult.MemberNames)}
					Error: {ex.Message}
					""");
			}
		}
	}

	/// <summary>
	/// Saves the data.
	/// </summary>
	public void Save()
	{
		Save(FileName, GetOrReadData());
	}

	/// <summary>
	/// Resets the data.
	/// </summary>
	public void Reset()
	{
		_data = null;
	}

	/// <summary>
	/// Opens the editor with settings XML.
	/// </summary>
	public void Edit()
	{
		//! ensure data, we need it for diff
		GetOrReadData();

		if (!File.Exists(FileName))
			Save(FileName, _data!);

		//! use CodePage, we get XML from editor and possible BOM garbage on wrong CodePage is an issue
		var editor = Far.Api.CreateEditor();
		editor.FileName = FileName;
		editor.DisableHistory = true;
		editor.CodePage = 65001;

		// set title, mind raw types used by scripts
		editor.Title = $"{GetTypeName()} - {FileName}";

		editor.Saving += (sender, args) =>
		{
			var xml = editor.GetText();
			using var reader = new StringReader(xml);
			var data = GetXmlSerializer().Deserialize(reader)!;
			ValidateData(data);
			_data = data;
		};

		editor.Opened += (sender, args) =>
		{
			var xmlSettings = new XmlReaderSettings { IgnoreComments = true };
			using var reader = XmlReader.Create(new StringReader(editor.GetText()), xmlSettings);
			var docOld = new XmlDocument { PreserveWhitespace = false };
			docOld.Load(reader);
			var xmlOld = docOld.DocumentElement!.OuterXml;

			using var writer = new StringWriter();
			using var xmlWriter = XmlWriter.Create(writer, _xmlWriterSettings);
			GetXmlSerializer().Serialize(xmlWriter, _data, _xmlSerializerNamespaces);
			var docNew = new XmlDocument { PreserveWhitespace = false };
			docNew.LoadXml(writer.ToString());
			var xmlNew = docNew.DocumentElement!.OuterXml;

			//! ensure same line ends
			xmlOld = xmlOld.ReplaceLineEndings("\n");
			xmlNew = xmlNew.ReplaceLineEndings("\n");
			if (xmlOld == xmlNew)
				return;

			var answer = Far.Api.Message(
				DifferentXml,
				GetTypeName(),
				MessageOptions.YesNo);

			if (answer != 0)
				return;

			// get new XML via file, avoid UTF16 on using StringWriter
			var temp = Kit.TempFileName(null);
			try
			{
				Save(temp, _data!);
				var newText = File.ReadAllText(temp);
				editor.SetText(newText);
			}
			finally
			{
				File.Delete(temp);
			}
		};

		editor.Open();
	}

	const string DifferentXml = """
		The XML is different from the current settings:
		(?) added new elements or removed old
		(?) changed formatting or data order
		(?) syntax or validation errors

		Update the text with the current settings?
		(you may undo this change before saving)
		""";
}
