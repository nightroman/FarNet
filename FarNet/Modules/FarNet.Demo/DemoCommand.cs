
using System.Runtime.InteropServices;
namespace FarNet.Demo
{
	/// <summary>
	/// Command invoked from the command line by the "Demo:" prefix.
	/// It shows a dialog with the command text (the text after the prefix).
	/// </summary>
	[ModuleCommand(Name = "FarNet.Demo Command", Prefix = "Demo")]
	[Guid("e3b61c33-a71f-487d-bad3-5542aed112d6")]
	public class DemoCommand : ModuleCommand
	{
		/// <summary>
		/// This method implements the command action.
		/// </summary>
		public override void Invoke(object sender, ModuleCommandEventArgs e)
		{
			Far.Net.Message(string.Format(null, GetString("CommandFormat"), this, e.Command));
		}
	}
}
