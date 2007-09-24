/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

using System;

namespace FarManager
{
	/// <summary>
	/// Plugin interface. See <see cref="Far"/> property.
	/// </summary>
	public interface IPlugin
	{
		/// <summary>
		/// FAR Manager application.
		/// </summary>
		/// <remarks>
		/// When an object is set - plugin must register menu items, prefixes, etc.
		/// When null is set plugin must unregister its hooks.
		/// </remarks>
		IFar Far { get; set; }
	}

	/// <summary>
	/// Base class for any FAR.NET plugin.
	/// </summary>
	/// <remarks>
	/// It keeps reference to <see cref="IFar"/> and provides
	/// <see cref="Connect"/> and <see cref="Disconnect"/> methods.
	/// Normally a plugin should implement at least <see cref="Connect"/>.
	/// </remarks>
	public class BasePlugin : IPlugin
	{
		IFar _Far;

		/// <summary>
		/// Object implementing <see cref="IFar"/> interface.
		/// It is set internally and normally should not be changed directly.
		/// </summary>
		public IFar Far
		{
			get { return _Far; }
			set
			{
				if (_Far != null)
					Disconnect();
				_Far = value;
				if (value != null)
					Connect();
			}
		}

		/// <include file='doc.xml' path='docs/pp[@name="Connect"]/*'/>
		public virtual void Connect() { }

		/// <summary>
		/// Override this to handle plugin shutdown.
		/// </summary>
		public virtual void Disconnect() { }
	}
}
