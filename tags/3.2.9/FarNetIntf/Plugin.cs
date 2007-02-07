using System;

namespace FarManager
{
	/// <summary>
	/// Plugin
	/// </summary>
	/// <remarks>Plugin is controlled by 
	/// <see cref="Far"/> property.
	/// </remarks>
	/// 
	public interface IPlugin
	{
		/// <summary>
		/// Far manager application 
		/// </summary>
		/// <remarks>
		/// When object is set - plugin must register 
		/// MenuItems, prefixes, etc
		/// 
		/// When null is set plugin must unregister its
		/// hooks
		/// </remarks>
		IFar Far
		{
			get;
			set;
		}
	}
	/// <summary>
	/// Base class for all plugins
	/// </summary>
	/// <remarks>It stores reference to the FarManager and
	/// provides <see cref="Connect"/> and <see cref="Disconnect"/> methods
	/// override those methods to handle attachment and 
	/// detachment of plugin
	/// </remarks>
	public class BasePlugin : IPlugin
	{
		/// <summary>
		/// reference to the FarManager
		/// </summary>
		IFar far;
		/// <summary>
		/// <see cref="IPlugin.Far"/>
		/// </summary>
		public IFar Far
		{
			get { return far; }
			set
			{
				if (far != null)
					Disconnect();
				far = value;
				if (far != null)
				{
					Connect();
				}
			}
		}
		/// <summary>
		/// Override this method to handle plugin
		/// startup
		/// </summary>
		public virtual void Connect() { }
		/// <summary>
		/// Override this metod to handle plugin shutdown
		/// </summary>
		public virtual void Disconnect() { }
	}
}
