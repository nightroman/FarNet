
using System;
using System.Runtime.Serialization;
namespace FarNet.Demo
{
	/// <summary>
	/// Custom data class used in settings, serialized as XML.
	/// </summary>
	[Serializable]
	public class UserDataXml
	{
		///
		public int Id { get; set; }
		///
		public string Name { get; set; }
	}
	/// <summary>
	/// Custom data class used in settings, serialized as binary, using custom serialization.
	/// </summary>
	[Serializable]
	public class UserDataBinary : UserDataXml, ISerializable
	{
		const string keyId = "key1", keyName = "key2";
		///
		public UserDataBinary() { }
		/// <summary>
		/// Custom deserialization.
		/// </summary>
		protected UserDataBinary(SerializationInfo info, StreamingContext context)
		{
			Id = info.GetInt32(keyId);
			Name = info.GetString(keyName);
		}
		/// <summary>
		/// Custom serialization.
		/// </summary>
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(keyId, Id);
			info.AddValue(keyName, Name);
		}
	}
}
