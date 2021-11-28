
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

		/// <summary>
		/// .
		/// </summary>
		public override string ToString()
		{
			return Value;
		}

		/// <summary>
		/// .
		/// </summary>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// .
		/// </summary>
		/// <param name="reader">.</param>
		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			var isEmptyElement = reader.IsEmptyElement;
			reader.ReadStartElement();
			if (!isEmptyElement)
			{
				Value = reader.ReadContentAsString();
				reader.ReadEndElement();
			}
		}

		/// <summary>
		/// .
		/// </summary>
		/// <param name="writer">.</param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteCData(Value);
		}
	}
}
