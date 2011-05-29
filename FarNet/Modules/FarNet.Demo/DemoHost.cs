
namespace FarNet.Demo
{
	/// <summary>
	/// The module host class.
	/// It is created and connected on the first call of any module action.
	/// </summary>
	[ModuleHost(Load = false)]
	public class DemoHost : ModuleHost
	{
		/// <summary>
		/// This method is called first and once.
		/// This demo shows a message "Connect()"
		/// </summary>
		public override void Connect()
		{
			Far.Net.Message("Connect()");
		}
		/// <summary>
		/// This method is called once on exit.
		/// NOTE: any UI API is not allowed.
		/// </summary>
		public override void Disconnect()
		{
			Far.Net.UI.WindowTitle = "Disconnect()";
		}
	}
}
