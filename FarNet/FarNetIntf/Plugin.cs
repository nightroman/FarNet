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
		/// Far manager application.
		/// </summary>
		/// <remarks>
		/// When an object is set - plugin must register menu items, prefixes, etc.
		/// When null is set plugin must unregister its hooks.
		/// </remarks>
		IFar Far { get; set; }
	}

	/// <summary>
	/// Base class for all plugins.
	/// </summary>
	/// <remarks>
	/// It keeps reference to <see cref="IFar"/> and provides <see cref="Connect"/> and <see cref="Disconnect"/> methods.
	/// Override those methods to handle attachment and detachment of your plugin.
	/// </remarks>
	public class BasePlugin : IPlugin
	{
		// FarManager
		IFar _far;

		/// <summary>
		/// Object implementing <see cref="IFar"/>.
		/// </summary>
		public IFar Far
		{
			get { return _far; }
			set
			{
				if (_far != null)
					Disconnect();
				_far = value;
				if (value != null)
					Connect();
			}
		}

		/// <include file='doc.xml' path='docs/pp[@name="Connect"]/*'/>
		public virtual void Connect() { }

		/// <summary>
		/// Override this metod to handle plugin shutdown.
		/// </summary>
		public virtual void Disconnect() { }
	}
}
