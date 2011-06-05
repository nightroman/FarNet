
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
		/// This method is called once first of all.
		/// </summary>
		public override void Connect()
		{
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
