
using System.IO;
using System.Runtime.InteropServices;
namespace FarNet.Demo
{
	/// <summary>
	/// Filer opening *.resources files in the panel.
	/// </summary>
	[ModuleFiler(Name = "FarNet.Demo Filer", Mask = "*.resources")]
	[Guid("ef555d78-a06f-47be-b138-a0afd36459f7")]
	public class DemoFiler : ModuleFiler
	{
		/// <summary>
		/// Reads and checks the magic number, then opens the resource panel.
		/// </summary>
		public override void Invoke(object sender, ModuleFilerEventArgs e)
		{
			try
			{
				// Check the magic number of .resources files
				using (var reader = new BinaryReader(e.Data))
				{
					int magic = reader.ReadInt32();
					if (magic != -1091581234)
						throw new ModuleException();
				}
			}
			catch
			{
				// This is not a .resources file
				Far.Net.Message(GetString("BadResources"));
				return;
			}

			// Open the resource panel
			(new DemoExplorer(e.Name)).OpenPanel();
		}
	}
}
