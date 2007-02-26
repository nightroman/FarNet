using System;

namespace FarManager
{
	/// <summary>
	/// Plugin
	/// </summary>
	/// <remarks>
	/// Plugin is controlled by <see cref="Far"/> property.
	/// </remarks>
	public interface IPlugin
	{
		/// <summary>
		/// Far manager application 
		/// </summary>
		/// <remarks>
		/// When object is set - plugin must register MenuItems, prefixes, etc.
		/// When null is set plugin must unregister its hooks
		/// </remarks>
		IFar Far { get; set; }
	}

	/// <summary>
	/// Base class for all plugins.
	/// </summary>
	/// <remarks>
	/// It stores reference to the FarManager and provides
	/// <see cref="Connect"/> and <see cref="Disconnect"/> methods.
	/// Override those methods to handle attachment and detachment of plugin.
	/// </remarks>
	public class BasePlugin : IPlugin
	{
		// FarManager
		IFar _far;

		/// <summary>
		/// See <see cref="IPlugin.Far"/>.
		/// </summary>
		public IFar Far
		{
			get { return _far; }
			set
			{
				if (_far != null)
					Disconnect();
				_far = value;
				if (_far != null)
					Connect();
			}
		}

		/// <summary>
		/// Override this method to handle plugin startup
		/// </summary>
		public virtual void Connect() { }

		/// <summary>
		/// Override this metod to handle plugin shutdown
		/// </summary>
		public virtual void Disconnect() { }
	}
}
