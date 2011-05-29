
using System.Runtime.InteropServices;
namespace FarNet.Demo
{
	/// <summary>
	/// Menu item in all menus available for module actions.
	/// It shows "Hello, World!" in English or Russian, according to the UI settings.
	/// </summary>
	/// <remarks>
	/// NOTE: the menu item title is shown in English or Russian, according to the UI settings, too.
	/// The attribute <c>Name</c> is treated as the resource string due to the <c>Resources</c> flag.
	/// </remarks>
	[ModuleTool(Name = "MenuTitle", Options = ModuleToolOptions.AllAreas, Resources = true)]
	[Guid("a10218a8-76b3-47f7-8900-3a162bf16c49")]
	public class DemoTool : ModuleTool
	{
		/// <summary>
		/// This method implements the menu tool action.
		/// </summary>
		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			Far.Net.Message(string.Format(GetString("Format"), GetString("Hello"), GetString("World")));
		}
	}
}
