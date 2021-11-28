using System;

namespace FarNet.Demo
{
	/// <summary>
	/// The module host class.
	/// It is created and connected on the first call of any module job.
	/// </summary>
	[ModuleHost(Load = false)]
	public class DemoHost : ModuleHost
	{
		/// <summary>
		/// This method is called once before anything else.
		/// </summary>
		public override void Connect()
		{
			// update the local settings
			var settings = new Workings();
			var data = settings.GetData();
			data.LastLoadTime = DateTime.Now;
			data.LoadCount += 1;
			settings.Save();
		}

		/// <summary>
		/// This method is called once on exit.
		/// NOTE: it should not call the core.
		/// </summary>
		public override void Disconnect()
		{
		}
	}
}
