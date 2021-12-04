
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FarNet
{
	/// <summary>
	/// Helper for serializing browsable settings as CDATA.
	/// </summary>
	public struct XmlCData : IXmlSerializable
	{
		/// <summary>
		/// Creates from the specified string.
		/// </summary>
		/// <param name="value">String value.</param>
		public XmlCData(string value)
		{
			Value = value;
		}

		/// <summary>
		/// Gets the string value.
		/// </summary>
		public string Value { get; set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return Value;
		}

		/// <inheritdoc/>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <inheritdoc/>
		public void ReadXml(XmlReader reader)
		{
			Value = reader.ReadElementContentAsString();
		}

		/// <inheritdoc/>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteCData(Value);
		}

		/// <summary>
		/// Converts from string.
		/// </summary>
		/// <param name="value">.</param>
		public static implicit operator XmlCData(string value) => new(value);

		/// <summary>
		/// Converts to string.
		/// </summary>
		/// <param name="value">.</param>
		public static implicit operator string(XmlCData value) => value.Value;
	}
}
