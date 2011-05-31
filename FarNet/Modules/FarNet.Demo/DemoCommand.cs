
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace FarNet.Demo
{
	/// <summary>
	/// Command invoked from the command line by the "Demo:" prefix.
	/// It prints some data depending on the command text after the prefix.
	/// </summary>
	[ModuleCommand(Name = "FarNet.Demo Command", Prefix = "Demo")]
	[Guid("e3b61c33-a71f-487d-bad3-5542aed112d6")]
	public class DemoCommand : ModuleCommand
	{
		const int kb = 1024;
		/// <summary>
		/// This method implements the command action.
		/// The command text is the Command property value.
		/// </summary>
		public override void Invoke(object sender, ModuleCommandEventArgs e)
		{
			var process = Process.GetCurrentProcess();
			switch (e.Command.Trim().ToUpper())
			{
				case "TT":
					Far.Net.UI.Write(string.Format("Total time : {0}", process.TotalProcessorTime));
					break;
				case "WS":
					Far.Net.UI.Write(string.Format("Working set : {0:n0} kb", process.WorkingSet64 / kb));
					break;
				case "PM":
					Far.Net.UI.Write(string.Format("Private memory : {0:n0} kb", process.PrivateMemorySize64 / kb));
					break;
				case "MM":
					Far.Net.UI.Write(string.Format("Managed memory : {0:n0} kb", GC.GetTotalMemory(true) / kb));
					break;
				default:
					Far.Net.UI.Write(string.Format(@"
Total time     : {0}
Working set    : {1,7:n0} kb
Private memory : {2,7:n0} kb
Managed memory : {3,7:n0} kb
",
		 process.TotalProcessorTime,
		 process.WorkingSet64 / kb,
		 process.PrivateMemorySize64 / kb,
		 GC.GetTotalMemory(true) / kb));
					break;
			}
		}
	}
}
